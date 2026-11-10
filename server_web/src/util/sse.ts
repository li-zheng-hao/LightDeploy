export type SSEState = 'connecting' | 'connected' | 'reconnecting' | 'disconnected';
export interface LogEvent {
    id: number;
    message: string;
}

export class SSEClient {
    private eventSource: EventSource | null = null;
    private reconnectTimer: ReturnType<typeof setTimeout> | null = null;
    private heartbeatTimer: ReturnType<typeof setInterval> | null = null;
    private reconnectTimeout = 1000;
    private shouldReconnect = false;
    private state: SSEState = 'disconnected';
    private lastActivity = 0;
    private lastID = 0;
    private gapReported = false;
    private waiters = new Set<{ resolve: () => void; reject: (error: Error) => void }>();
    private readonly heartbeatTimeout = 45000;

    constructor(
        private url: string,
        private onMessage: (message: string, id?: number) => void,
        private onError: (error: Event) => void,
        private onStateChange: (state: SSEState) => void = () => {},
        private onComplete: () => void = () => {},
    ) {}

    public get lastEventID() {
        return this.lastID;
    }

    private setState(state: SSEState) {
        this.state = state;
        this.onStateChange(state);
    }

    private clearReconnectTimer() {
        if (this.reconnectTimer !== null) clearTimeout(this.reconnectTimer);
        this.reconnectTimer = null;
    }

    private connect() {
        if (!this.shouldReconnect || this.eventSource) return;
        this.clearReconnectTimer();
        this.setState(this.state === 'disconnected' ? 'connecting' : 'reconnecting');
        const separator = this.url.includes('?') ? '&' : '?';
        const source = new EventSource(`${this.url}${separator}after=${this.lastID}`);
        this.eventSource = source;
        this.lastActivity = Date.now();

        source.onmessage = (event: MessageEvent) => {
            if (source !== this.eventSource) return;
            this.lastActivity = Date.now();
            this.accept(event.data, Number(event.lastEventId) || 0);
        };
        source.addEventListener('ping', () => {
            if (source === this.eventSource) this.lastActivity = Date.now();
        });
        source.addEventListener('ready', () => {
            if (source !== this.eventSource) return;
            this.lastActivity = Date.now();
            this.reconnectTimeout = 1000;
            this.setState('connected');
            for (const waiter of this.waiters) waiter.resolve();
            this.waiters.clear();
        });
        source.addEventListener('log-gap', () => {
            if (source === this.eventSource) this.reportGap();
        });
        source.addEventListener('complete', () => {
            if (source !== this.eventSource) return;
            this.stop();
            this.onComplete();
        });
        source.addEventListener('expired', (event) => {
            if (source !== this.eventSource) return;
            this.onMessage((event as MessageEvent).data || '部署日志已过期，请查看服务端日志');
            this.stop();
            this.onComplete();
        });
        source.onerror = (event: Event) => {
            if (source !== this.eventSource) return;
            source.close();
            this.eventSource = null;
            this.setState('reconnecting');
            this.onError(event);
            if (this.shouldReconnect) {
                this.reconnectTimer = setTimeout(() => this.connect(), this.reconnectTimeout);
                this.reconnectTimeout = Math.min(this.reconnectTimeout * 2, 30000);
            }
        };
    }

    private accept(message: string, id: number) {
        if (id > 0) {
            if (id <= this.lastID) return;
            this.lastID = id;
        }
        this.onMessage(message, id || undefined);
    }

    public replay(events: LogEvent[]) {
        for (const event of events) this.accept(event.message, event.id);
    }

    public reportGap() {
        if (this.gapReported) return;
        this.gapReported = true;
        this.onMessage('部分日志已超出保留范围，请查看服务端日志');
    }

    private recover = () => {
        if (!this.shouldReconnect) return;
        if (this.eventSource && Date.now() - this.lastActivity >= this.heartbeatTimeout) {
            this.eventSource.close();
            this.eventSource = null;
            this.setState('reconnecting');
        }
        if (!this.eventSource) {
            this.clearReconnectTimer();
            this.connect();
        }
    };

    private onVisibilityChange = () => {
        if (document.visibilityState === 'visible') this.recover();
    };

    public start() {
        if (this.shouldReconnect) return;
        this.shouldReconnect = true;
        window.addEventListener('online', this.recover);
        document.addEventListener('visibilitychange', this.onVisibilityChange);
        this.heartbeatTimer = setInterval(() => {
            if (this.eventSource && Date.now() - this.lastActivity >= this.heartbeatTimeout) this.recover();
        }, 5000);
        this.connect();
    }

    public ensureConnected(timeout = 10000): Promise<void> {
        this.start();
        this.recover();
        if (this.state === 'connected') return Promise.resolve();
        return new Promise((resolve, reject) => {
            const timer = setTimeout(() => {
                this.waiters.delete(waiter);
                reject(new Error('日志连接未就绪，请检查网络后重试，部署尚未提交'));
            }, timeout);
            const waiter = {
                resolve: () => { clearTimeout(timer); resolve(); },
                reject: (error: Error) => { clearTimeout(timer); reject(error); },
            };
            this.waiters.add(waiter);
        });
    }

    public stop() {
        this.shouldReconnect = false;
        this.clearReconnectTimer();
        if (this.heartbeatTimer !== null) clearInterval(this.heartbeatTimer);
        this.heartbeatTimer = null;
        window.removeEventListener('online', this.recover);
        document.removeEventListener('visibilitychange', this.onVisibilityChange);
        this.eventSource?.close();
        this.eventSource = null;
        this.setState('disconnected');
        for (const waiter of this.waiters) waiter.reject(new Error('日志连接已关闭，部署尚未提交'));
        this.waiters.clear();
    }
}

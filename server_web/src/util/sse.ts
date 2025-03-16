
export class SSEClient {
    private eventSource: EventSource | null = null;
    private onMessage: (message: string) => void;
    private onError: (error: Event) => void;
    private url: string;
    private isConnecting: boolean = false;
    private reconnectTimeout: number = 1000; // 初始重连间隔1秒
    private maxReconnectTimeout: number = 30000; // 最大重连间隔30秒
    private shouldReconnect: boolean = false;
    
    constructor(url: string, onMessage: (message: string) => void, onError: (error: Event) => void) {
        this.url = url;
        this.onMessage = onMessage;
        this.onError = onError;
    }

    private connect() {
        if (this.isConnecting || !this.shouldReconnect) return;
        
        this.isConnecting = true;
        this.eventSource = new EventSource(this.url);

        this.eventSource.onmessage = (event: MessageEvent) => {
            this.reconnectTimeout = 1000; // 连接成功后重置重连间隔
            this.onMessage(event.data);
        };

        this.eventSource.onerror = (event: Event) => {
            this.onError(event);
            this.eventSource?.close();
            this.eventSource = null;
            this.isConnecting = false;

            if (this.shouldReconnect) {
                // 使用递增的重连间隔
                setTimeout(() => this.connect(), this.reconnectTimeout);
                // 增加重连间隔，但不超过最大值
                this.reconnectTimeout = Math.min(this.reconnectTimeout * 2, this.maxReconnectTimeout);
            }
        };

        this.eventSource.onopen = () => {
            this.isConnecting = false;
            console.log('SSE连接已建立');
        };
    }

    public start() {
        this.shouldReconnect = true;
        this.connect();
    }

    public stop() {
        this.shouldReconnect = false;
        if (this.eventSource) {
            this.eventSource.close();
            this.eventSource = null;
        }
        this.isConnecting = false;
    }
}



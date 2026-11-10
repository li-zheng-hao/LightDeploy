import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';
import vm from 'node:vm';
import ts from 'typescript';

// Run the production TypeScript with a controllable network and browser clock.
const compiled = ts.transpileModule(readFileSync(new URL('./sse.ts', import.meta.url), 'utf8'), {
  compilerOptions: { module: ts.ModuleKind.CommonJS, target: ts.ScriptTarget.ES2022 },
}).outputText;

function setup() {
  let now = 0;
  let timerID = 0;
  const timers = new Map();
  const streams = [];
  const messages = [];
  const states = [];
  let completions = 0;
  const window = new EventTarget();
  const document = new EventTarget();
  document.visibilityState = 'visible';
  class ClockDate extends Date { static now() { return now; } }
  class NetworkStream extends EventTarget {
    static CONNECTING = 0;
    static OPEN = 1;
    static CLOSED = 2;
    readyState = 0;
    constructor(url) {
      super();
      this.url = url;
      streams.push(this);
    }
    close() { this.readyState = 2; }
    open() { this.readyState = 1; this.onopen?.(new Event('open')); }
    fail() { this.onerror?.(new Event('error')); }
    send(type, data = '', id = '') {
      const event = new MessageEvent(type, { data, lastEventId: id });
      if (type === 'message') this.onmessage?.(event);
      else this.dispatchEvent(event);
    }
  }
  function schedule(fn, delay, interval = false) {
    const id = ++timerID;
    timers.set(id, { fn, at: now + delay, interval: interval ? delay : 0 });
    return id;
  }
  const context = {
    exports: {}, EventSource: NetworkStream, Event, Date: ClockDate, URL, URLSearchParams,
    window, document, console: { log() {}, error() {} },
    setTimeout: (fn, delay) => schedule(fn, delay),
    clearTimeout: id => timers.delete(id),
    setInterval: (fn, delay) => schedule(fn, delay, true),
    clearInterval: id => timers.delete(id),
  };
  vm.runInNewContext(compiled, context);
  const client = new context.exports.SSEClient('/api/sse/connect?taskId=example',
    (message, id) => messages.push({ message, id }), () => {}, state => states.push(state), () => { completions++; });
  function advance(ms) {
    const end = now + ms;
    while (true) {
      const next = [...timers].filter(([, timer]) => timer.at <= end)
        .sort((a, b) => a[1].at - b[1].at)[0];
      if (!next) break;
      const [id, timer] = next;
      now = timer.at;
      if (timer.interval) timer.at += timer.interval;
      else timers.delete(id);
      timer.fn();
    }
    now = end;
  }
  return { client, streams, messages, states, advance, timers, window, document, get completions() { return completions; } };
}

test('deployment waits for the server readiness event, not just HTTP headers', async () => {
  const { client, streams, states } = setup();
  client.start();
  let submitted = false;
  const deployment = client.ensureConnected().then(() => { submitted = true; });
  streams[0].open();
  await Promise.resolve();
  assert.equal(submitted, false);
  streams[0].send('ready');
  await deployment;
  assert.equal(submitted, true);
  assert.equal(states.at(-1), 'connected');
  client.stop();
});

test('reconnect requests missed logs and suppresses duplicate replay', () => {
  const { client, streams, messages, advance, states } = setup();
  client.start();
  streams[0].send('ready');
  streams[0].send('message', '开始部署', '1');
  streams[0].fail();
  assert.equal(states.at(-1), 'reconnecting');
  advance(1000);
  assert.equal(new URL(streams[1].url, 'http://localhost').searchParams.get('after'), '1');
  streams[1].send('ready');
  streams[1].send('message', '开始部署', '1');
  streams[1].send('message', '需要更新 1 个文件: app.dll', '2');
  streams[1].send('message', '部署完成', '3');
  assert.deepEqual(messages, [
    { message: '开始部署', id: 1 },
    { message: '需要更新 1 个文件: app.dll', id: 2 },
    { message: '部署完成', id: 3 },
  ]);
  client.stop();
});

test('deployment request expedites a pending reconnect without opening duplicate streams', async () => {
  const { client, streams, advance } = setup();
  client.start();
  streams[0].send('ready');
  streams[0].fail();
  const ready = client.ensureConnected();
  assert.equal(streams.length, 2);
  streams[1].send('ready');
  await ready;
  advance(2000);
  assert.equal(streams.length, 2);
  client.stop();
});

test('unavailable log connection rejects readiness instead of authorizing deployment', async () => {
  const { client, advance, streams } = setup();
  let submitted = false;
  const ready = client.ensureConnected(1000).then(() => { submitted = true; });
  const rejected = assert.rejects(ready, /日志连接/);
  advance(1000);
  await rejected;
  assert.equal(submitted, false);
  streams[0].send('ready');
  await Promise.resolve();
  assert.equal(submitted, false);
  client.stop();
});

test('stopping cancels reconnects and pending deployment readiness', async () => {
  const { client, streams, advance, states, timers } = setup();
  client.start();
  const rejected = assert.rejects(client.ensureConnected(), /日志连接/);
  streams[0].fail();
  client.stop();
  await rejected;
  advance(60000);
  assert.equal(streams.length, 1);
  assert.equal(states.at(-1), 'disconnected');
  assert.equal(timers.size, 0);
});

test('missing heartbeats reconnect a silently dead idle connection', () => {
  const { client, streams, advance, states } = setup();
  client.start();
  streams[0].send('ready');
  advance(60000);
  assert.ok(streams.length >= 2);
  assert.notEqual(states.at(-1), 'connected');
  client.stop();
});

test('heartbeats keep an idle connection healthy without adding deployment logs', () => {
  const { client, streams, advance, messages, states } = setup();
  client.start();
  streams[0].send('ready');
  for (let i = 0; i < 8; i++) {
    advance(15000);
    streams[0].send('ping');
  }
  assert.equal(streams.length, 1);
  assert.equal(states.at(-1), 'connected');
  assert.deepEqual(messages, []);
  client.stop();
});

test('network recovery immediately reconnects a failed stream', () => {
  const { client, streams, window } = setup();
  client.start();
  streams[0].send('ready');
  streams[0].fail();
  window.dispatchEvent(new Event('online'));
  assert.equal(streams.length, 2);
  client.stop();
});

test('finished deployment releases its stream and never reconnects', () => {
  const context = setup();
  context.client.start();
  context.streams[0].send('ready');
  context.streams[0].send('message', '部署成功', '1');
  context.streams[0].send('complete');
  context.advance(120000);
  assert.equal(context.completions, 1);
  assert.equal(context.streams[0].readyState, 2);
  assert.equal(context.streams.length, 1);
  assert.equal(context.states.at(-1), 'disconnected');
  assert.equal(context.timers.size, 0);
});

test('expired deployment reports the missing history and stops retrying', () => {
  const context = setup();
  context.client.start();
  context.streams[0].send('expired', '部署日志已过期');
  context.advance(120000);
  assert.equal(context.completions, 1);
  assert.equal(context.streams.length, 1);
  assert.equal(context.states.at(-1), 'disconnected');
  assert.match(context.messages[0].message, /日志已过期/);
  assert.equal(context.timers.size, 0);
});

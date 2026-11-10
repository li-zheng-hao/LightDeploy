import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';
import vm from 'node:vm';
import { parse, compileScript } from 'vue/compiler-sfc';
import * as vue from 'vue';
import ts from 'typescript';

const { descriptor } = parse(readFileSync(new URL('./Deploy.vue', import.meta.url), 'utf8'));
const script = compileScript(descriptor, { id: 'deploy-regression' }).content;
const compiled = ts.transpileModule(script, { compilerOptions: {
  module: ts.ModuleKind.CommonJS, target: ts.ScriptTarget.ES2022,
} }).outputText;

function setup({ available = false } = {}) {
  const requests = [];
  const streams = [];
  let resolveConnection;
  let completeTask;
  class LogConnection {
    lastEventID = 0;
    constructor(url, onMessage, onError, onState, onComplete) {
      this.url = url;
      this.onMessage = onMessage;
      this.onState = onState;
      this.onComplete = onComplete;
      streams.push(this);
    }
    start() { this.onState('connecting'); }
    stop() { this.onState('disconnected'); }
    ensureConnected() {
      if (!available) return Promise.reject(new Error('日志连接未就绪'));
      return new Promise(resolve => { resolveConnection = resolve; });
    }
    replay(events) {
      for (const event of events) this.onMessage(event.message);
    }
  }
  const deploy = {
    createDeployTask: async () => ({ data: { taskId: 'isolated' } }),
    deployService: async (...args) => {
      requests.push({ method: 'deploy', args });
      completeTask = streams.at(-1).onComplete;
    },
    getTaskLogs: async () => ({ data: {
      events: [{ id: 1, message: '需要更新 1 个文件: app.dll' }], truncated: false, finished: true,
    } }),
    getDeployHistory: async () => ({ data: [] }),
    startService: async (...args) => requests.push({ method: 'start', args }),
    stopService: async (...args) => requests.push({ method: 'stop', args }),
  };
  const service = {
    installService: async (...args) => requests.push({ method: 'install', args }),
    deleteService: async (...args) => requests.push({ method: 'delete', args }),
    getServiceList: async () => ({ data: [] }),
    getServiceStatus: async () => ({ data: [] }),
  };
  const context = { exports: {}, console: { error() {} }, localStorage: { getItem() { return null; } },
    require(name) {
      if (name === 'vue') return { ...vue, onMounted() {}, onUnmounted() {}, watch() {} };
      if (name === 'naive-ui') return { useMessage: () => ({ error() {}, success() {} }) };
      if (name.endsWith('/api/deploy')) return deploy;
      if (name.endsWith('/api/service')) return service;
      if (name.endsWith('/api/target')) return { getTargetList: async () => ({ data: [] }) };
      if (name.endsWith('/util/sse')) return { SSEClient: LogConnection };
      throw new Error(`Unexpected dependency: ${name}`);
    },
  };
  vm.runInNewContext(compiled, context);
  const page = context.exports.default.setup({}, { expose() {} });
  page.selectedServiceId.value = 1;
  page.selectedServiceName.value = 'test';
  page.checkedRowKeys.value = [2];
  return { page, requests, streams, ready: () => resolveConnection(), complete: () => completeTask() };
}

test('start stop install and delete remain available when SSE is unavailable', async () => {
  const { page, requests } = setup();
  for (const handler of ['handleStart', 'handleStop', 'handleInstall', 'handleDeleteWindowsService']) {
    page.checkedRowKeys.value = [2];
    await page[handler]();
  }
  assert.deepEqual(requests.map(request => request.method), ['start', 'stop', 'install', 'delete']);
});

test('publish does not submit while log connection is unavailable', async () => {
  const { page, requests } = setup();
  await page.confirmDeploy();
  assert.equal(requests.length, 0);
  assert.ok(page.deployLogs.value.some(message => message.includes('部署尚未提交')));
});

test('completed snapshot is replayed before restoring the global log connection', async () => {
  const context = setup({ available: true });
  const deployment = context.page.confirmDeploy();
  await new Promise(setImmediate);
  assert.equal(context.requests.length, 0);
  context.ready();
  await deployment;
  assert.equal(context.requests.length, 1);
  assert.ok(context.page.deployLogs.value.includes('需要更新 1 个文件: app.dll'));
  assert.equal(context.streams.at(-1).url, '/api/sse/connect');
});

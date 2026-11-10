<template>
  <div class="p-4">
    <n-grid :cols="24" :x-gap="12" :y-gap="12">
      <!-- 左侧面板 -->
      <n-grid-item :span="8">
        <n-card title="服务选择">
          <n-space vertical>
            <!-- 服务选择三级联动 -->
            <n-form-item label="环境">
              <n-select
                v-model:value="selectedEnvironment"
                :options="environmentOptions"
                placeholder="请选择环境"
                :disabled="isDeploying"
                @update:value="handleEnvironmentChange"
              />
            </n-form-item>
            <n-form-item label="服务分组">
              <n-select
                v-model:value="selectedGroup"
                :options="groupOptions"
                placeholder="请选择服务分组"
                :disabled="!selectedEnvironment || isDeploying"
                @update:value="handleGroupChange"
              />
            </n-form-item>
            <n-form-item label="服务名称">
              <n-select
                v-model:value="selectedServiceName"
                :options="serviceNameOptions"
                placeholder="请选择服务名称"
                :disabled="!selectedGroup || isDeploying"
                @update:value="handleServiceNameChange"
              />
            </n-form-item>
            <n-space>
              <n-button
                type="primary"
                @click="handleDeploy"
                :disabled="!selectedServiceId || !hasSelectedTargets || isDeploying"
                :loading="isDeploying"
              >
                开始部署
              </n-button>
              <n-button
                @click="handleInstall"
                :disabled="!selectedServiceId || !hasSelectedTargets || isDeploying"
              >
                安装服务
              </n-button>
              <n-button
                @click="handleStart"
                :disabled="!selectedServiceId || !hasSelectedTargets || isDeploying"
              >
                启动服务
              </n-button>
              <n-button
                @click="handleStop"
                :disabled="!selectedServiceId || !hasSelectedTargets || isDeploying"
              >
                停止服务
              </n-button>
              <n-button
                @click="handleDeleteWindowsService"
                :disabled="!selectedServiceId || !hasSelectedTargets || isDeploying"
                type="error"
              >
                删除Windows/Linux服务
              </n-button>
              <n-button
                @click="
                  () => {
                    deployLogs.splice(0, deployLogs.length);
                  }
                "
              >
                清空日志
              </n-button>
            </n-space>
          </n-space>
        </n-card>

        <!-- 发布历史 -->
        <n-card title="发布历史" class="mt-4">
          <n-list>
            <n-list-item v-for="history in deployHistory" :key="history.id">
              <div class="flex flex-col gap-2">
                <div class="flex">
                  <span class="w-20 font-medium">发布时间：</span>
                  <span>{{ history.deployTime }}</span>
                </div>
                <div class="flex" v-if="history.comment">
                  <span class="w-20 font-medium">发布说明：</span>
                  <span class="text-sm text-gray-500">{{
                    history.comment
                  }}</span>
                </div>
              </div>
            </n-list-item>
          </n-list>
        </n-card>
      </n-grid-item>

      <!-- 右侧面板 -->
      <n-grid-item :span="16">
        <n-card title="发布目标">
          <n-data-table
            :columns="targetColumns"
            :data="targetData"
            :pagination="{ pageSize: 10 }"
            :bordered="false"
            :row-key="(row: DeployTarget) => row.id"
            :checked-row-keys="checkedRowKeys"
            @update:checked-row-keys="handleCheckedRowKeysChange"
          />
        </n-card>

        <!-- 部署日志 -->
        <n-card title="部署日志" class="mt-4">
          <template #header-extra>
            <n-tag :type="logConnectionState === 'connected' ? 'success' : 'warning'" size="small">
              {{ logConnectionText }}
            </n-tag>
          </template>
          <n-alert v-if="logConnectionState !== 'connected'" type="warning" class="mb-3" :show-icon="false">
            {{ isDeploying ? '日志连接正在恢复，部署提交后将继续执行，恢复连接后会自动补齐日志。' : '日志连接未就绪，确认部署后会先尝试恢复连接。' }}
          </n-alert>
          <div
            class="h-[300px] overflow-auto bg-black p-4 rounded"
            ref="logContainer"
          >
            <n-text type="info" v-for="(log, index) in deployLogs" :key="index">
              {{ log }}<br />
            </n-text>
          </div>
        </n-card>
      </n-grid-item>
    </n-grid>
    <!-- 添加发布说明对话框 -->
    <n-modal
      v-model:show="showDeployModal"
      title="发布说明"
      preset="dialog"
      positive-text="确认"
      negative-text="取消"
      @positive-click="confirmDeploy"
      @negative-click="
        () => {
          showDeployModal = false;
          deployComment = '';
          useFastMode = false;
          sendWecomNotify = false;
        }
      "
    >
      <n-checkbox v-model:checked="useFastMode">
        使用快速发布模式（按需校验内容，仅更新变化文件）
      </n-checkbox>
      <n-checkbox
        v-model:checked="sendWecomNotify"
        :disabled="!selectedServiceHasWecomKey"
        style="margin-top: 8px"
      >
        发送企业微信通知{{
          selectedServiceHasWecomKey
            ? ""
            : "（请先在服务配置中填写机器人Key）"
        }}
      </n-checkbox>
      <n-input
        v-model:value="deployComment"
        type="textarea"
        placeholder="请输入发布说明"
        :rows="4"
      />
    </n-modal>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted, onUnmounted, nextTick, watch } from "vue";
import { useMessage } from "naive-ui"; // 添加 NInput, NModal
import type { DataTableColumns } from "naive-ui";
import {
  getDeployHistory,
  createDeployTask,
  getTaskLogs,
  deployService,
  startService,
  stopService,
  type DeployHistory,
} from "../api/deploy";
import {
  installService,
  getServiceList,
  getServiceStatus,
  type DeployService,
  deleteService,
} from "../api/service";
import { getTargetList, type DeployTarget } from "@/api/target";
import { SSEClient, type SSEState } from "@/util/sse";

const message = useMessage();

// 服务选择相关数据
const allServices = ref<DeployService[]>([]);
const selectedGroup = ref<string | null>(null);
const selectedServiceName = ref<string | null>(null);
const selectedEnvironment = ref<string | null>(null);
const selectedServiceId = ref<number | null>(null);
const checkedRowKeys = ref<number[]>([]);

// 从 localStorage 读取保存的 serviceId
const loadSelectedServiceId = () => {
  const savedId = localStorage.getItem("selectedServiceId");
  if (savedId) {
    selectedServiceId.value = parseInt(savedId);
    return parseInt(savedId);
  }
  return null;
};

// 保存 serviceId 到 localStorage
const saveSelectedServiceId = (id: number | null) => {
  if (id) {
    localStorage.setItem("selectedServiceId", id.toString());
  } else {
    localStorage.removeItem("selectedServiceId");
  }
};

// 修改 handleEnvironmentChange 函数
const handleEnvironmentChange = (value: string | null) => {
  selectedEnvironment.value = value;
  selectedGroup.value = null;
  selectedServiceName.value = null;
  selectedServiceId.value = null;
  resetTargetData();
};

// 修改 onMounted
onMounted(async () => {
  try {
    const response = await getServiceList();
    allServices.value = response.data;

    // 恢复保存的服务ID
    const savedServiceId = loadSelectedServiceId();
    if (savedServiceId) {
      const service = allServices.value.find((s) => s.id === savedServiceId);
      if (service) {
        selectedGroup.value = service.groupName;
        selectedServiceName.value = service.serviceName;
        selectedEnvironment.value = service.environment || "默认环境";
        selectedServiceId.value = savedServiceId;
        await handleServiceChange(savedServiceId);
      }
    }
  } catch (error) {
    message.error("加载服务列表失败");
  }
});
const hasSelectedTargets = computed(() => checkedRowKeys.value.length > 0);

// 当前选中服务是否配置了企业微信机器人Key
const selectedServiceHasWecomKey = computed(() => {
  if (!selectedServiceId.value) return false;
  const service = allServices.value.find((s) => s.id === selectedServiceId.value);
  return !!service?.wecomWebhookKey;
});

// 选项数据
const environmentOptions = computed(() => {
  const environments = new Set(
    allServices.value.map((service) => service.environment || "默认环境")
  );
  return Array.from(environments).map((env) => ({
    label: env,
    value: env,
  }));
});

const groupOptions = computed(() => {
  if (!selectedEnvironment.value) return [];
  const groups = new Set(
    allServices.value
      .filter((service) => service.environment === selectedEnvironment.value)
      .map((service) => service.groupName)
  );
  return Array.from(groups).map((group) => ({
    label: group,
    value: group,
  }));
});

const serviceNameOptions = computed(() => {
  if (!selectedGroup.value) return [];
  const services = allServices.value.filter(
    (service) =>
      service.groupName === selectedGroup.value &&
      service.environment === selectedEnvironment.value
  );
  const serviceNames = new Set(services.map((service) => service.serviceName));
  return Array.from(serviceNames).map((name) => ({
    label: name,
    value: name,
  }));
});

// 处理选择变更
const handleGroupChange = (value: string | null) => {
  selectedGroup.value = value;
  selectedServiceName.value = null;
  selectedServiceId.value = null;
  resetTargetData();
};

const handleServiceNameChange = (value: string | null) => {
  selectedServiceName.value = value;
  selectedServiceId.value = null;
  if (value) {
    const service = allServices.value.find(
      (s) =>
        s.serviceName === value &&
        s.groupName === selectedGroup.value &&
        s.environment === selectedEnvironment.value
    );
    if (service) {
      selectedServiceId.value = service.id;
      saveSelectedServiceId(service.id);
      handleServiceChange(service.id);
    }
  }
  resetTargetData();
};

// 重置目标数据
const resetTargetData = () => {
  targetData.value = [];
  deployHistory.value = [];
  deployLogs.value = [];
  checkedRowKeys.value = [];
};

// 发布目标表格列定义
const targetColumns: DataTableColumns<DeployTarget> = [
  { type: "selection" },
  { title: "编号", key: "id" },
  { title: "状态", key: "status" },
  { title: "IP", key: "host" },
  { title: "端口", key: "port" },
];

// 发布目标数据
const targetData = ref<DeployTarget[]>([]);

// 发布历史
const deployHistory = ref<DeployHistory[]>([]);

// 部署日志
const deployLogs = ref<string[]>([]);

// 处理选中行变更
const handleCheckedRowKeysChange = (keys: number[]) => {
  checkedRowKeys.value = keys;
};

// 处理服务变更
const handleServiceChange = async (value: number | null) => {
  targetData.value = [];
  deployHistory.value = [];
  checkedRowKeys.value = [];
  try {
    // 加载发布目标
    if (value === null) return;
    const targetsResponse = await getTargetList(value);
    targetData.value = targetsResponse.data;
    const statusResponse = await getServiceStatus(value);
    if (statusResponse?.data) {
      targetData.value.forEach((target) => {
        const status = statusResponse.data!.find(
          (s) => s.targetId === target.id
        );
        if (status) {
          target.status = status.status;
          target.message = status.message;
        }
        if (status?.message) {
          window.$notify.error({
            title: "错误",
            content: status?.message || "未知错误",
            duration: 3000,
          });
        }
      });
    }
    // 加载发布历史
    const historyResponse = await getDeployHistory(value);
    deployHistory.value = historyResponse.data;
  } catch (error) {
    message.error("加载服务数据失败" + error);
  }
};

// 添加对话框相关的状态
const showDeployModal = ref(false);
const deployComment = ref("");
const useFastMode = ref(false);
const sendWecomNotify = ref(false);
const isDeploying = ref(false);
const logConnectionState = ref<SSEState>('connecting');
const logConnectionText = computed(() => ({
  connecting: '日志连接中',
  connected: '日志已连接',
  reconnecting: '日志重连中',
  disconnected: '日志已断开',
}[logConnectionState.value]));

// 修改部署操作处理函数
const handleDeploy = async () => {
  if (isDeploying.value || !selectedServiceId.value || checkedRowKeys.value.length === 0) return;
  showDeployModal.value = true;
};

// 添加实际的部署处理函数
const confirmDeploy = async () => {
  if (isDeploying.value || !selectedServiceId.value || checkedRowKeys.value.length === 0) return;
  isDeploying.value = true;
  const serviceId = selectedServiceId.value;
  const targetIds = [...checkedRowKeys.value];
  const comment = deployComment.value;
  const fastMode = useFastMode.value;
  const notify = sendWecomNotify.value;
  let taskId: string | null = null;
  let taskClient: SSEClient | null = null;
  let submitted = false;
  let taskFinished = false;
  try {
    deployLogs.value = [];
    showDeployModal.value = false;
    deployLogs.value.push("正在建立部署日志连接");
    const response = await createDeployTask();
    if (isUnmounted) return;
    taskId = response.data.taskId;
    taskClient = connectLogs(`/api/sse/connect?taskId=${encodeURIComponent(taskId)}`, () => {
      taskFinished = true;
      if (!isDeploying.value && !isUnmounted && sseClient === taskClient) connectLogs();
    });
    await taskClient.ensureConnected();
    if (isUnmounted) return;
    deployLogs.value.push("开始部署服务");
    submitted = true;
    await deployService(
      serviceId,
      targetIds,
      comment,
      fastMode,
      notify,
      taskId
    );
    deployComment.value = "";
    useFastMode.value = false;
    sendWecomNotify.value = false;
    // 重新加载数据
    if (!isUnmounted && selectedServiceId.value === serviceId) await handleServiceChange(serviceId);
  } catch (error) {
    console.error(error);
    deployLogs.value.push((submitted ? "部署请求失败: " : "部署尚未提交: ") + error);
  } finally {
    // HTTP 结果与 SSE 独立返回，即使日志流仍在重连，也主动补取遗漏的文件列表。
    if (!isUnmounted && submitted && taskId && taskClient) {
      try {
        const response = await getTaskLogs(taskId, taskClient.lastEventID);
        if (response.data.truncated) taskClient.reportGap();
        taskClient.replay(response.data.events);
        taskFinished ||= response.data.finished;
      } catch (error) {
        deployLogs.value.push('补取部署日志失败，恢复连接后将自动重试: ' + error);
      }
    }
    isDeploying.value = false;
    if (!isUnmounted && sseClient === taskClient && (!submitted || taskFinished)) connectLogs();
  }
};

// 处理安装服务
const handleInstall = async () => {
  if (!selectedServiceId.value || checkedRowKeys.value.length === 0) return;

  try {
    deployLogs.value = [];
    deployLogs.value.push("开始安装服务");
    connectLogs();

    await installService(selectedServiceId.value, checkedRowKeys.value);

    await handleServiceChange(selectedServiceId.value);
    deployLogs.value.push("安装成功");
  } catch (error) {
    deployLogs.value.push("安装失败 error: " + error);
  }
};

// 处理启动服务
const handleStart = async () => {
  if (!selectedServiceId.value || checkedRowKeys.value.length === 0) return;

  try {
    deployLogs.value = [];
    deployLogs.value.push("开始启动服务");
    connectLogs();
    await startService(selectedServiceId.value, checkedRowKeys.value);
    await handleServiceChange(selectedServiceId.value);
    deployLogs.value.push("启动服务成功");
  } catch (error) {
    deployLogs.value.push("启动失败 error: " + error);
  }
};

// 处理停止服务
const handleStop = async () => {
  if (!selectedServiceId.value || checkedRowKeys.value.length === 0) return;

  try {
    deployLogs.value = [];
    deployLogs.value.push("开始停止服务");
    connectLogs();
    await stopService(selectedServiceId.value, checkedRowKeys.value);
    await handleServiceChange(selectedServiceId.value);
    deployLogs.value.push("停止服务成功");
  } catch (error) {
    deployLogs.value.push("停止失败 error: " + error);
  }
};

// 删除服务（Windows/Linux 通用）
const handleDeleteWindowsService = async () => {
  if (!selectedServiceName.value || checkedRowKeys.value.length === 0) return;
  try {
    deployLogs.value = [];
    deployLogs.value.push("开始删除Windows/Linux服务");
    connectLogs();
    await deleteService(selectedServiceName.value, checkedRowKeys.value);
    await handleServiceChange(selectedServiceId.value);
    deployLogs.value.push("删除Windows/Linux服务成功");
    message.success("删除Windows/Linux服务成功");
  } catch (error) {
    deployLogs.value.push("删除Windows/Linux服务失败 error: " + error);
    message.error("删除Windows/Linux服务失败");
  }
};

let sseClient: SSEClient | null = null;
let isUnmounted = false;
const connectLogs = (url = '/api/sse/connect', onComplete = () => {}) => {
  sseClient?.stop();
  sseClient = new SSEClient(
    url,
    (message: string) => {
      if (!isUnmounted) deployLogs.value.push(message);
    },
    (error: Event) => {
      console.error(error);
    },
    (state: SSEState) => { logConnectionState.value = state; },
    onComplete
  );
  if (!isUnmounted) sseClient.start();
  return sseClient;
};
onMounted(() => {
  connectLogs();
});

onUnmounted(() => {
  isUnmounted = true;
  sseClient?.stop();
});

const logContainer = ref<HTMLElement | null>(null);

// 监听 deployLogs 变化
watch(() => deployLogs.value.length, () => {
  nextTick(() => {
    if (logContainer.value) {
      logContainer.value.scrollTop = logContainer.value.scrollHeight;
    }
  });
});
</script>

<style scoped></style>

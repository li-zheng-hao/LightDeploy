<template>
  <div class="p-4">
    <n-grid :cols="24" :x-gap="12" :y-gap="12">
      <!-- 左侧面板 -->
      <n-grid-item :span="8">
        <n-card title="服务选择">
          <n-space vertical>
            <!-- 服务选择三级联动 -->
            <n-form-item label="服务分组">
              <n-select
                v-model:value="selectedGroup"
                :options="groupOptions"
                placeholder="请选择服务分组"
                @update:value="handleGroupChange"
              />
            </n-form-item>
            <n-form-item label="服务名称">
              <n-select
                v-model:value="selectedServiceName"
                :options="serviceNameOptions"
                placeholder="请选择服务名称"
                :disabled="!selectedGroup"
                @update:value="handleServiceNameChange"
              />
            </n-form-item>
            <n-form-item label="环境">
              <n-select
                v-model:value="selectedEnvironment"
                :options="environmentOptions"
                placeholder="请选择环境"
                :disabled="!selectedServiceName"
                @update:value="handleEnvironmentChange"
              />
            </n-form-item>
            <n-space>
              <n-button
                type="primary"
                @click="handleDeploy"
                :disabled="!selectedServiceId || !hasSelectedTargets"
              >
                开始部署
              </n-button>
              <n-button
                @click="handleInstall"
                :disabled="!selectedServiceId || !hasSelectedTargets"
              >
                安装服务
              </n-button>
              <n-button
                @click="handleStart"
                :disabled="!selectedServiceId || !hasSelectedTargets"
              >
                启动服务
              </n-button>
              <n-button
                @click="handleStop"
                :disabled="!selectedServiceId || !hasSelectedTargets"
              >
                停止服务
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
            @update:checked-row-keys="handleCheckedRowKeysChange"
          />
        </n-card>

        <!-- 部署日志 -->
        <n-card title="部署日志" class="mt-4">
          <div class="h-[300px] overflow-auto bg-black p-4 rounded">
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
        }
      "
    >
      <n-checkbox v-model:checked="useFastMode">
        使用快速发布模式(仅对比文件大小和修改时间)
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
import { ref, computed, onMounted, onUnmounted } from "vue";
import { useMessage } from "naive-ui"; // 添加 NInput, NModal
import type { DataTableColumns } from "naive-ui";
import {
  getDeployHistory,
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
} from "../api/service";
import { getTargetList, type DeployTarget } from "@/api/target";
import { SSEClient } from "@/util/sse";

const message = useMessage();

// 服务选择相关数据
const allServices = ref<DeployService[]>([]);
const selectedGroup = ref<string | null>(null);
const selectedServiceName = ref<string | null>(null);
const selectedEnvironment = ref<number | null>(null);
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
const handleEnvironmentChange = async (value: number | null) => {
  selectedEnvironment.value = value ?? null;
  selectedServiceId.value = value ?? null;
  saveSelectedServiceId(value); // 保存选中的服务ID
  if (value) {
    await handleServiceChange(selectedServiceId.value);
  } else {
    resetTargetData();
  }
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
        selectedEnvironment.value = savedServiceId;
        selectedServiceId.value = savedServiceId;
        await handleServiceChange(savedServiceId);
      }
    }
  } catch (error) {
    message.error("加载服务列表失败");
  }
});
const hasSelectedTargets = computed(() => checkedRowKeys.value.length > 0);

// 选项数据
const groupOptions = computed(() => {
  const groups = new Set(allServices.value.map((service) => service.groupName));
  return Array.from(groups).map((group) => ({
    label: group,
    value: group,
  }));
});

const serviceNameOptions = computed(() => {
  if (!selectedGroup.value) return [];
  const services = allServices.value.filter(
    (service) => service.groupName === selectedGroup.value
  );
  const serviceNames = new Set(services.map((service) => service.serviceName));
  return Array.from(serviceNames).map((name) => ({
    label: name,
    value: name,
  }));
});

const environmentOptions = computed(() => {
  if (!selectedGroup.value || !selectedServiceName.value) return [];
  const services = allServices.value.filter(
    (service) =>
      service.groupName === selectedGroup.value &&
      service.serviceName === selectedServiceName.value
  );
  return services.map((service) => ({
    label: service.environment || "默认环境",
    value: service.id,
  }));
});

// 处理选择变更
const handleGroupChange = (value: string | null) => {
  selectedGroup.value = value;
  selectedServiceName.value = null;
  selectedEnvironment.value = null;
  selectedServiceId.value = null;
  resetTargetData();
};

const handleServiceNameChange = (value: string | null) => {
  selectedServiceName.value = value;
  selectedEnvironment.value = null;
  selectedServiceId.value = null;
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

// 初始化数据
onMounted(async () => {
  try {
    const response = await getServiceList();
    allServices.value = response.data;
  } catch (error) {
    message.error("加载服务列表失败");
  }
});

// 处理选中行变更
const handleCheckedRowKeysChange = (keys: number[]) => {
  checkedRowKeys.value = keys;
};

// 处理服务变更
const handleServiceChange = async (value: number | null) => {
  if (!value) {
    targetData.value = [];
    deployHistory.value = [];
    deployLogs.value = [];
    return;
  }

  try {
    // 加载发布目标
    const targetsResponse = await getTargetList(value);
    targetData.value = targetsResponse.data;
    const statusResponse = await getServiceStatus(value);
    if (statusResponse && statusResponse.data) {
      targetData.value.forEach((target) => {
        const status = statusResponse.data!.find(
          (s) => s.targetId === target.id
        );
        if (status) {
          target.status = status.status;
          target.message = status.message;
        }
        if (status?.message) {
          window.$message.error(status.message || "未知错误");
        }
      });
    }
    // 加载发布历史
    const historyResponse = await getDeployHistory(value);
    deployHistory.value = historyResponse.data;
  } catch (error) {
    message.error("加载服务数据失败");
  }
};

// 添加对话框相关的状态
const showDeployModal = ref(false);
const deployComment = ref("");
const useFastMode = ref(false);

// 修改部署操作处理函数
const handleDeploy = async () => {
  if (!selectedServiceId.value || checkedRowKeys.value.length === 0) return;
  showDeployModal.value = true;
};

// 添加实际的部署处理函数
const confirmDeploy = async () => {
  try {
    showDeployModal.value = false;
    await deployService(
      selectedServiceId.value!,
      checkedRowKeys.value,
      deployComment.value,
      useFastMode.value
    );
    deployLogs.value = [];
    deployLogs.value.push("开始部署服务");
    deployComment.value = "";
    useFastMode.value = false;
    // 重新加载数据
    await handleServiceChange(selectedServiceId.value);
    deployLogs.value.push("部署成功");
  } catch (error) {
    console.error(error);
    deployLogs.value.push("部署失败 error: " + error);
  }
};

// 处理安装服务
const handleInstall = async () => {
  if (!selectedServiceId.value || checkedRowKeys.value.length === 0) return;

  try {
    deployLogs.value = [];
    deployLogs.value.push("开始安装服务");

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
    await stopService(selectedServiceId.value, checkedRowKeys.value);
    await handleServiceChange(selectedServiceId.value);
    deployLogs.value.push("停止服务成功");
  } catch (error) {
    deployLogs.value.push("停止失败 error: " + error);
  }
};
var sseClient: SSEClient | null = null;
onMounted(() => {
  sseClient = new SSEClient(
    "/api/sse/connect",
    (message: string) => {
      console.log("sse message", message);
      deployLogs.value.push(message);
    },
    (error: Event) => {
      console.error(error);
    }
  );
  sseClient.start();
});

onUnmounted(() => {
  sseClient?.stop();
});
</script>

<style scoped></style>

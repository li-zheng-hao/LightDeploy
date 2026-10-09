<template>
  <div class="agent-update">
    <n-card title="Agent管理">
      <n-space vertical :size="24">
        <n-data-table
          :columns="columns"
          :data="agents"
          :row-key="(row: Agent) => row.id"
          :pagination="pagination"
          :loading="loading"
          @update:checked-row-keys="handleSelectionChange"
        />
        <n-space vertical>
          <n-alert type="info">
            请选择要更新的Agent，并上传新版本的程序文件
          </n-alert>
          <n-upload
            :max="1"
            :disabled="!selectedAgentIds.length"
            @change="handleUpload"
          >
            <n-button :disabled="!selectedAgentIds.length">
              {{
                selectedAgentIds.length
                  ? "选择更新文件"
                  : "请先选择要更新的Agent"
              }}
            </n-button>
          </n-upload>
        </n-space>
      </n-space>
    </n-card>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted } from "vue";
import { useMessage, useDialog } from "naive-ui";
import type { DataTableColumns } from "naive-ui";
import { getAgentVersion, getAllAgent, updateAgent } from "@/api/agent";

interface Agent {
  id: number;
  name: string;
  version: string;
  status: string;
  lastHeartbeat: string;
}

const message = useMessage();
const dialog = useDialog();
const loading = ref(false);
const agents = ref<Agent[]>([]);
const selectedAgentIds = ref<number[]>([]);

// 记录已成功加载版本的 Agent id，避免重复请求
const versionLoaded = new Set<number>();

// 分页配置：仅加载当前页 Agent 的版本号
const pagination = ref({
  page: 1,
  pageSize: 10,
  showSizePicker: true,
  pageSizes: [10, 20, 50],
  onUpdatePage: (page: number) => {
    pagination.value.page = page;
    loadVersionsForCurrentPage();
  },
  onUpdatePageSize: (pageSize: number) => {
    pagination.value.pageSize = pageSize;
    pagination.value.page = 1;
    loadVersionsForCurrentPage();
  },
});

const columns: DataTableColumns<Agent> = [
  {
    type: "selection",
  },
  {
    title: "地址",
    key: "host",
  },
  {
    title: "版本号",
    key: "version",
  },
];

// 仅加载当前分页内的 Agent 版本号
const loadVersionsForCurrentPage = async () => {
  const start = (pagination.value.page - 1) * pagination.value.pageSize;
  const end = start + pagination.value.pageSize;
  const pageAgents = agents.value.slice(start, end);

  await Promise.all(
    pageAgents.map(async (agent, offset) => {
      const idx = start + offset;
      if (!agents.value[idx] || versionLoaded.has(agent.id)) return;

      // 标记为加载中（重新进入该页时可见）
      agents.value[idx].version = "加载中...";
      versionLoaded.add(agent.id);
      try {
        const versionResponse = await getAgentVersion(agent.id);
        agents.value[idx].version = versionResponse.data.version;
      } catch (error) {
        agents.value[idx].version = "获取失败";
        // 失败不缓存，允许下次重试
        versionLoaded.delete(agent.id);
      }
    })
  );
};

const fetchAgents = async () => {
  loading.value = true;
  try {
    const response = await getAllAgent();
    const agentsData = response.data;
    // 先渲染基础信息
    agents.value = agentsData.map((agent: any) => ({
      ...agent,
      version: "加载中...",
    }));
    versionLoaded.clear();
    pagination.value.page = 1;
    loading.value = false; // 基础信息渲染后提前关闭 loading

    // 仅异步获取当前页的版本号
    await loadVersionsForCurrentPage();
  } catch (error) {
    message.error("获取Agent列表失败");
    loading.value = false;
  }
};

const handleUpload = async ({ file }: { file: any }) => {
  if (!file) return;

  try {
    // 使用 Promise 方式等待用户确认
    await new Promise((resolve, reject) => {
      dialog.warning({
        title: "确认更新",
        content: `确定要更新选中的 ${selectedAgentIds.value.length} 个Agent吗？`,
        positiveText: "确定",
        negativeText: "取消",
        onPositiveClick: () => {
          resolve(true);
        },
        onNegativeClick: () => {
          reject(new Error("用户取消"));
        },
      });
    });

    // 用户点击确定后，才会执行以下更新操作
    for (const agentId of selectedAgentIds.value) {
      await updateAgent({
        file: file.file,
        agentServiceName: "LightDeployAgentV2",
        targetId: agentId,
      });
    }
    message.success("更新文件上传成功");
    fetchAgents();
  } catch (error: any) {
    if (error.message === "用户取消") {
      return; // 用户取消操作，静默处理
    }
    message.error("更新文件上传失败");
  }
};

const handleSelectionChange = (rowKeys: number[]) => {
  selectedAgentIds.value = rowKeys;
};

onMounted(() => {
  fetchAgents();
});
</script>

<style scoped>
.agent-update {
  padding: 24px;
}
</style>
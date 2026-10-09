<template>
  <div class="p-4 space-y-4">
    <n-spin :show="loading">
      <!-- KPI 概览 -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <n-card v-for="item in kpiItems" :key="item.label" size="small">
          <n-statistic :label="item.label" :value="item.value" />
        </n-card>
      </div>

      <!-- 服务状态总览 -->
      <n-card class="mt-4" title="服务状态总览">
        <template #header-extra>
          <n-space align="center">
            <n-switch v-model:value="autoRefresh" size="small" />
            <span class="text-xs text-gray-500">自动刷新(30s)</span>
            <n-button size="small" :loading="loading" @click="fetchOverview">
              刷新
            </n-button>
          </n-space>
        </template>
        <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div
            v-for="item in statusItems"
            :key="item.label"
            class="rounded-lg p-4 text-center"
            :style="{ backgroundColor: item.bg }"
          >
            <div class="text-sm" :style="{ color: item.color }">
              {{ item.label }}
            </div>
            <div class="text-3xl font-bold mt-1" :style="{ color: item.color }">
              {{ item.value }}
            </div>
          </div>
        </div>
      </n-card>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-4 mt-4">
        <!-- 服务状态明细 -->
        <n-card
          class="lg:col-span-2"
          title="服务状态明细"
          size="small"
        >
          <n-data-table
            :columns="serviceColumns"
            :data="overview?.services || []"
            :bordered="false"
            :pagination="{ pageSize: 10 }"
            size="small"
          />
        </n-card>

        <div class="space-y-4">
          <!-- 环境分布 -->
          <n-card title="环境分布" size="small">
            <div
              v-if="(overview?.environmentCount?.length || 0) === 0"
              class="text-gray-400 text-sm"
            >
              暂无数据
            </div>
            <div
              v-for="env in overview?.environmentCount || []"
              :key="env.environment"
              class="mb-3"
            >
              <div class="flex justify-between text-sm mb-1">
                <span>{{ env.environment }}</span>
                <span class="text-gray-500">{{ env.count }}</span>
              </div>
              <n-progress
                type="line"
                :percentage="envPercent(env.count)"
                :show-indicator="false"
                color="#2080f0"
              />
            </div>
          </n-card>

          <!-- 最近部署 -->
          <n-card title="最近部署" size="small">
            <n-empty
              v-if="recentDeploys.length === 0"
              description="暂无部署记录"
            />
            <n-list v-else hoverable clickable>
              <n-list-item v-for="h in recentDeploys" :key="h.id">
                <div class="flex flex-col">
                  <div class="flex items-center justify-between">
                    <span class="font-medium">{{ h.serviceName }}</span>
                    <n-tag v-if="h.environment" size="small" :bordered="false">
                      {{ h.environment }}
                    </n-tag>
                  </div>
                  <span class="text-xs text-gray-400 mt-1">
                    {{ formatTime(h.deployTime) }}
                  </span>
                  <span v-if="h.comment" class="text-xs text-gray-500 mt-1">
                    {{ h.comment }}
                  </span>
                </div>
              </n-list-item>
            </n-list>
          </n-card>
        </div>
      </div>
    </n-spin>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted, onUnmounted, watch, h } from "vue";
import type { DataTableColumns } from "naive-ui";
import dayjs from "dayjs";
import { getOverview, type Overview, type ServiceStatus } from "@/api/metric";
import { getDeployHistoryPageList, type HistoryResponse } from "@/api/deploy";

const loading = ref(false);
const overview = ref<Overview | null>(null);
const recentDeploys = ref<HistoryResponse[]>([]);
const autoRefresh = ref(true);
let timer: number | undefined;

// KPI 概览
const kpiItems = computed(() => [
  { label: "服务总数", value: overview.value?.serviceCount ?? 0 },
  { label: "目标主机", value: overview.value?.hostCount ?? 0 },
  { label: "代理数量", value: overview.value?.agentCount ?? 0 },
  { label: "累计部署", value: overview.value?.deployCount ?? 0 },
]);

// 状态汇总
const statusItems = computed(() => {
  const s = overview.value?.statusSummary;
  return [
    { label: "运行中", value: s?.running ?? 0, color: "#18a058", bg: "#e8f5ee" },
    { label: "已停止", value: s?.stopped ?? 0, color: "#f0a020", bg: "#fdf4e6" },
    { label: "未安装", value: s?.notInstalled ?? 0, color: "#2080f0", bg: "#e8f2fd" },
    { label: "异常", value: s?.error ?? 0, color: "#d03050", bg: "#fdecef" },
  ];
});

// 服务状态明细列
const serviceColumns: DataTableColumns<ServiceStatus> = [
  { title: "服务名称", key: "serviceName" },
  { title: "环境", key: "environment", render: (row) => row.environment || "-" },
  { title: "分组", key: "groupName", render: (row) => row.groupName || "-" },
  {
    title: "运行中",
    key: "running",
    render: (row) => h("span", { style: { color: "#18a058" } }, String(row.running)),
  },
  {
    title: "已停止",
    key: "stopped",
    render: (row) => h("span", { style: { color: "#f0a020" } }, String(row.stopped)),
  },
  {
    title: "未安装",
    key: "notInstalled",
    render: (row) => h("span", { style: { color: "#2080f0" } }, String(row.notInstalled)),
  },
  {
    title: "异常",
    key: "error",
    render: (row) => h("span", { style: { color: "#d03050" } }, String(row.error)),
  },
];

const fetchOverview = async () => {
  loading.value = true;
  try {
    const { data } = await getOverview();
    overview.value = data;
  } catch (error) {
    // 错误提示由 apiClient 拦截器统一处理
  } finally {
    loading.value = false;
  }
};

const fetchRecentDeploys = async () => {
  try {
    const { data } = await getDeployHistoryPageList(1, 8, null);
    recentDeploys.value = data.data || [];
  } catch (error) {
    recentDeploys.value = [];
  }
};

const refreshAll = async () => {
  await Promise.all([fetchOverview(), fetchRecentDeploys()]);
};

// 环境占比（相对服务总数）
const envPercent = (count: number) => {
  const total = overview.value?.serviceCount || 0;
  if (total <= 0) return 0;
  return Math.round((count / total) * 100);
};

const formatTime = (t: string) => dayjs(t).format("YYYY-MM-DD HH:mm:ss");

const startTimer = () => {
  stopTimer();
  if (!autoRefresh.value) return;
  timer = window.setInterval(() => {
    fetchOverview();
  }, 30000);
};

const stopTimer = () => {
  if (timer) {
    clearInterval(timer);
    timer = undefined;
  }
};

watch(autoRefresh, () => {
  if (autoRefresh.value) startTimer();
  else stopTimer();
});

onMounted(() => {
  refreshAll();
  startTimer();
});

onUnmounted(() => {
  stopTimer();
});
</script>

<style scoped>
.home {
  padding: 16px;
}
</style>
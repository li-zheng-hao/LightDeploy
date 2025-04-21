<template>
  <div>
    <n-select
      v-model:value="serviceId"
      :options="serviceList"
      placeholder="请选择服务"
      style="width: 500px; margin-bottom: 16px"
      @update:value="onServiceChange"
      clearable
      filterable
    />
    <n-data-table
      :columns="columns"
      :data="historyList"
      :pagination="false"
      style="margin-top: 16px"
    />
    <n-pagination
      style="margin-top: 16px"
      :page="page"
      :page-size="pageSize"
      :item-count="total"
      @update:page="onPageChange"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { NDataTable, NSelect, NPagination } from "naive-ui";
import { getDeployHistoryPageList, type HistoryResponse } from "@/api/deploy";
import dayjs from "dayjs";
import { getServiceList } from "@/api/service";

const historyList = ref<HistoryResponse[]>([]);
const total = ref(0);
const page = ref(1);
const pageSize = ref(10);
const serviceId = ref<number | undefined | null>(null);
const serviceList = ref<{ label: string; value: number }[]>([]);

const columns = [
  { title: "ID", key: "id", width: 80 },
  { title: "服务名", key: "serviceName" },
  { title: "环境", key: "environment" },
  {
    title: "部署时间",
    key: "deployTime",
    render(row: HistoryResponse) {
      return dayjs(row.deployTime).format("YYYY-MM-DD HH:mm:ss");
    },
  },
  { title: "备注", key: "comment" },
];

const fetchData = async () => {
  const { data } = await getDeployHistoryPageList(
    page.value,
    pageSize.value,
    serviceId.value
  );
  historyList.value = data.data || [];
  console.log(historyList.value);
  total.value = data.total || 0;
};

const fetchServiceList = async () => {
  const { data } = await getServiceList();
  // 确保 value 为 number 类型
  serviceList.value = data.map((item) => ({
    label: `${item.groupName} - ${item.serviceName} (${item.environment})`,
    value: Number(item.id),
  }));
};

const onPageChange = (val: number) => {
  page.value = val;
  fetchData();
};

const onServiceChange = () => {
  page.value = 1;
  fetchData();
};

onMounted(() => {
  fetchServiceList();
  fetchData();
});
</script>

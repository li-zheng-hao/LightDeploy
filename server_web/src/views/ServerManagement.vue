<template>
  <div>
    <n-card>
      <n-form inline :model="searchForm" label-placement="left">
        <n-form-item label="主机">
          <n-select 
            v-model:value="searchForm.host" 
            placeholder="选择主机" 
            :options="hostOptions" 
            clearable 
            filterable
            style="width: 200px"
           />
        </n-form-item>
        <n-form-item>
          <n-button type="primary" @click="fetchServices">查询</n-button>
        </n-form-item>
      </n-form>

      <n-data-table
        :columns="columns"
        :data="services"
        :pagination="false"
        :bordered="false"
      />
    </n-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, h, computed } from 'vue';
import { NButton } from 'naive-ui';
import type { DataTableColumns } from 'naive-ui';
import { getAllHosts, getAllServices, GetAllHostsResponse, GetAllServiceResponse } from '../api/target';

const searchForm = ref({
  host: null
});

const hosts = ref<GetAllHostsResponse[]>([]);
const services = ref<GetAllServiceResponse[]>([]);

const hostOptions = computed(() => 
  hosts.value.map(item => ({ label: item.host, value: item.host }))
);

const createColumns = (): DataTableColumns<GetAllServiceResponse> => {
  return [
    {
      title: '主机',
      key: 'host'
    },
    {
      title: '服务名称',
      key: 'serviceName'
    },
    {
      title: '环境',
      key: 'environment'
    },
    {
      title: '分组',
      key: 'groupName'
    },
    {
      title: '端口',
      key: 'port'
    }
  ];
};

const columns = createColumns();

const fetchHosts = async () => {
  try {
    const response = await getAllHosts();
    hosts.value = response.data;
  } catch (error) {
    console.error('获取主机列表失败:', error);
  }
};

const fetchServices = async () => {
  try {
    const host = searchForm.value.host === null ? undefined : searchForm.value.host;
    const response = await getAllServices(host);
    services.value = response.data;
  } catch (error) {
    console.error('获取服务列表失败:', error);
  }
};

onMounted(() => {
  fetchHosts();
  fetchServices();
});
</script>

<style scoped>
</style>
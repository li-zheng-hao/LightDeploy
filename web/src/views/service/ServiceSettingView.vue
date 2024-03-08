<template>
  <TabPage>
    <div class="search-box">
      <n-select
        placeholder="环境"
        autosize
        style="width: 10%"
        v-model:value="searchParams.environment"
        :options="environmentOptions"
      />
      <n-input
        placeholder="服务类别"
        autosize
        style="min-width: 10%"
        v-model:value="searchParams.serviceGroup"
      />
      <n-input
        placeholder="服务名"
        autosize
        style="min-width: 10%"
        v-model:value="searchParams.serviceName"
      />
      <n-button type="primary" @click="search">查询</n-button>
      <n-button type="primary" @click="addClick">新增</n-button>
      <n-button type="primary" @click="resetSearch">重置</n-button>
    </div>
    <n-data-table
      :columns="columns"
      :data="data"
      remote
      class="h-70vh mt-4"
      flex-height
      :loading="isLoading"
    />
    <AddServiceDialog v-if="showAddModal" @confirm="() => search()"
                      @cancel="showAddModal=!showAddModal"></AddServiceDialog>
    <UpdateServiceDialog
      v-if="showUpdateModal"
      v-model:visible="showUpdateModal"
      @confirm="() => search()"
      @cancel="showUpdateModal=!showUpdateModal"
      v-model:data="editRow"
    ></UpdateServiceDialog>
    <UpdateTargetDialog v-if="showUpdateTargetModal"
                        @cancel="showUpdateTargetModal=!showUpdateTargetModal" :data="editRow"></UpdateTargetDialog>

  </TabPage>
</template>

<script setup lang="tsx">
import { apiClient } from '@/api/client/apiClient'
import TabPage from '@/components/TabPage.vue'
import AddServiceDialog from './serviceSetting/AddServiceDialog.vue'
import UpdateServiceDialog from './serviceSetting/UpdateServiceDialog.vue'
import { onMounted, ref } from 'vue'
import { getEnvironments } from '@/api/service/dicService'
import type DeployServiceDto from '@/dtos/deployServiceDto'
import UpdateTargetDialog from '@/views/service/serviceSetting/UpdateTargetDialog.vue'

const environmentOptions = ref([])
onMounted(() => {
  search()

  getEnvironments().then(res => {
    environmentOptions.value = res
  })
})

const searchParams = ref({
  serviceName: '',
  serviceGroup: '',
  environment: ''
})

async function search() {
  showAddModal.value = false
  showUpdateModal.value = false
  isLoading.value = true
  data.value = await apiClient.request<DeployServiceDto[]>({
    url: '/service/query',
    method: 'get',
    params: searchParams.value
  })
  isLoading.value = false
}

function resetSearch() {
  searchParams.value = {
    serviceName: '',
    serviceGroup: '',
    environment: ''
  }
}

const showAddModal = ref(false)
const showUpdateModal = ref(false)
const showUpdateTargetModal = ref(false)
function addClick() {
  showAddModal.value = true
}


//#region 表格
const columns = [
  {
    title: '编号',
    key: 'id'
  },
  {
    title: '环境',
    key: 'environmentName'
  },

  {
    title: '服务类别',
    key: 'groupName'
  },
  {
    title: '服务名',
    key: 'name'
  },
  {
    title: '端口',
    key: 'port'
  },

  {
    title: '描述',
    key: 'description'
  },
  {
    title: '发布路径',
    key: 'projectPath'
  },
  {
    title: '是否包含运行时',
    key: 'isSelfContained',
    render: (row: DeployServiceDto) => {
      return row.isSelfContained ? '包含' : '不包含'
    }
  },
  {
    title: '是否开启健康检查',
    key: 'enableHealthCheck',
    render: (row: DeployServiceDto) => {
      return row.enableHealthCheck ? '开启' : '关闭'
    }
  },
  {
    title: '操作',
    key: 'action',
    render: (row: DeployServiceDto) => {
      return (
        <div class="flex gap-2">
          <n-button onClick={() => onEdit(row)}>编辑</n-button>
          <n-button onClick={() => onTargetEdit(row)}>编辑目标</n-button>
          <n-button type="warning" onClick={() => onDelete(row)}>
            删除
          </n-button>
        </div>
      )
    },
    width: '250px'
  }
]

const editRow = ref<DeployServiceDto>({})

function onEdit(row: DeployServiceDto) {
  console.log('编辑', row)
  editRow.value = row
  showUpdateModal.value = true
}
function onTargetEdit(row: DeployServiceDto) {
  editRow.value = row
  showUpdateTargetModal.value = true
}
async function onDelete(row: DeployServiceDto) {
  await apiClient.request({
    url: '/service/delete',
    method: 'post',
    params: { id: row.id }
  })
  window.$message.success('删除成功')
  await search()
}

const data = ref<DeployServiceDto[]>([])

const isLoading = ref(false)

//#endregion


</script>

<style scoped>
.search-box {
  display: flex;
  gap: 10px;
}
</style>

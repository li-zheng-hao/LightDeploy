<script lang="ts" setup>
import { onMounted, ref, onUnmounted, nextTick } from 'vue'
import TabPage from '@/components/TabPage.vue'
import type { DeployTargetDto } from '@/dtos/deployTargetDto'
import { apiClient } from '@/api/client/apiClient'

const deployTargets = ref<DeployTargetDto[]>([])
const deployTargetsColumns = [
  {
    type: 'selection'
  },
  {
    title: '编号',
    key: 'id'
  },
  {
    title: 'IP',
    key: 'host'
  },

  {
    title: '端口',
    key: 'port'
  },
  {
    title: '健康检查地址',
    key: 'healthCheckUrl'
  },

  {
    title: '权限认证',
    key: 'authKey'
  }
]
const checkedRowKeysRef = ref<Array<string | number>>([])
const zipFilePath = ref('')
onMounted(()=>{
  apiClient.request<DeployTargetDto[]>({
    url: '/target/query',
    method: 'get',

  }).then(res => {
    const items=[]
    res.forEach(it=>{
      if(items.findIndex(i=>i.host===it.host)===-1){
        items.push(it)
      }
    })
    deployTargets.value = items
    checkedRowKeysRef.value = items.map(it => it.id)
  })
})

function updateClick() {
  console.log(checkedRowKeysRef.value)
  apiClient.request({
    url: '/deploy/update-agent',
    method: 'post',
    params: {
      targetIds: checkedRowKeysRef.value,
      zipFilePath: zipFilePath.value
    }
  }).then(() => {
    window.$message.success('操作成功')
  })
}
const deployLogs = ref([])
//#region sse

let sseClient:any=null
const logScrollBar=ref(null)
onMounted(()=>{
  sseClient=new EventSource('/api/sse')
  sseClient.onmessage=(e:any)=>{
    deployLogs.value.push(e.data)
    nextTick(()=>{
      logScrollBar.value?.scrollBy(0,200)
    })
  }
})
onUnmounted(() => {
  sseClient?.close()
})
</script>

<template>
  <tab-page>
    <div class="flex gap-2 flex-items-center justify-center">
      <n-input placeholder="安装包完整路径" style="max-width: 500px" v-model:value="zipFilePath"></n-input>
      <n-button @click="updateClick">开始更新</n-button>
      <div style="color: lightgray">
        注意：更新操作会将安装包上传到服务器并解压，然后重启服务,在控制台输出操作完成后会花费一定的时间(不超过2分钟)重启Agent服务，
        期间不要进行任何发布更新操作，如果长时间未完成需要自行检查原因
      </div>
    </div>

    <n-data-table :data="deployTargets" :columns="deployTargetsColumns"
                  :row-key="it=>it.id"
                  style=" height: 400px;  margin-top: 20px;"
                  flex-height
                  size="small"
                  v-model:checked-row-keys="checkedRowKeysRef">
    </n-data-table>

    <n-card title="控制台" style="margin-top: 20px;">
      <n-scrollbar style="max-height: 300px;" trigger="none" class="log-box-scrollbar"
                   ref="logScrollBar">
        <div v-for="item in deployLogs">
          {{ item }}
        </div>
      </n-scrollbar>
    </n-card>
  </tab-page>
</template>

<style scoped>

</style>

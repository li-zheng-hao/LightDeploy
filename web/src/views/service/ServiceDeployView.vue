<template>
  <tab-page>
    <div class="deploy-root-box">
      <div class="deploy-left-box">
        <n-card title="部署选择">
          <div class="flex flex-col gap-2">

            <div>环境：</div>
            <n-select v-model:value="deployData.environment" :options="environmentsOptions"
                      @update-value="changeEnvironment"></n-select>
            <div>服务分组：</div>
            <n-select v-model:value="deployData.groupName" :options="groupNameOptions"
                      @update-value="changeGroupName"></n-select>
            <div>服务名：</div>
            <n-select v-model:value="deployData.deployServiceId" :options="serviceOptions" :consistent-menu-width="false"
                      @update-value="changeService"></n-select>
            <n-button @click="showDeployModal=true" type="primary" style="margin-top: 10px">部署服务</n-button>
            <n-button @click="showInstallModal=true">安装服务</n-button>
            <n-button @click="startService">启动服务</n-button>
            <n-button @click="stopService">停止服务</n-button>
            <n-button @click="clearLog">清空日志</n-button>
          </div>
        </n-card>
      </div>
      <div class="deploy-right-box">
        <n-card title="目标选择">
          <n-data-table :data="deployTargets" :columns="deployTargetsColumns"
                        :row-key="it=>it.id"
                        style=" height: 150px; "
                        flex-height
                        size="small"
                        v-model:checked-row-keys="checkedRowKeysRef">
          </n-data-table>
        </n-card>
        <n-card title="发布历史">
          <n-data-table :data="deployHistory" :columns="deployHistoryColumns" size="small"  flex-height
                        style=" height: 150px; "
          >
          </n-data-table>
        </n-card>
        <n-card title="控制台">
          <!-- <n-scrollbar style="max-height: 300px" trigger="none" class="log-box-scrollbar"
           ref="logScrollBar">
            <div v-for="item in deployLogs">
              {{ item }}
            </div>
          </n-scrollbar> -->
                <n-config-provider :hljs="hljs">
         <NLog :lines="deployLogs"  style="height: 300px;" language="console"></NLog>
        </n-config-provider>
        </n-card>
      </div>
    </div>
    <n-modal
      v-if="showDeployModal"
      :show="true"
      preset="card"
      title="发布说明"
      style="width: 500px;height: 200px"
      size="huge"
      :bordered="false"
      @close="showDeployModal=false"
    >
      <div class="flex gap-4 flex-col">
        <n-input placeholder="发布说明" v-model:value="deployComment"></n-input>
        <n-button @click="deployService">开始部署</n-button>
      </div>

    </n-modal>

    <n-modal
      v-if="showInstallModal"
      :show="true"
      preset="card"
      title="安装服务"
      style="width: 500px;height: 500px"
      size="huge"
      :bordered="false"
      @close="showInstallModal=false"
    >
      <div class="flex gap-4 flex-col">
        <n-input placeholder="服务安装目录" v-model:value="installParams.targetDir"></n-input>
        <n-input placeholder="可执行程序(相对安装目录)" v-model:value="installParams.exePath"></n-input>
        <n-input placeholder="执行参数" v-model:value="installParams.exeParams"></n-input>
        <n-input placeholder="服务描述" v-model:value="installParams.description"></n-input>
        <n-button @click="installService">开始安装</n-button>
      </div>

    </n-modal>
  </tab-page>
</template>

<script setup lang="ts">
import TabPage from '@/components/TabPage.vue'
import {  onMounted, onUnmounted, ref } from 'vue'
import type DeployServiceDto from '@/dtos/deployServiceDto'
import { apiClient } from '@/api/client/apiClient'
import type { DeployTargetDto } from '@/dtos/deployTargetDto'
import { getEnvironments } from '@/api/service/dicService'
import { distinct } from '@/utils'
import {useDeployStore} from '@/stores/deployStore'
import hljs from 'highlight.js/lib/core'
import powershell from 'highlight.js/lib/languages/powershell';
import { NLog } from 'naive-ui'
hljs.registerLanguage('console', powershell);

const deployData=useDeployStore()
const services = ref<DeployServiceDto[]>([])
const deployTargets = ref<DeployTargetDto[]>([])
const showDeployModal=ref(false)
const deployLogs = ref([] as string[])
const deployComment=ref('')
const checkedRowKeysRef = ref<Array<string | number>>([])
const deployTargetsColumns = [
  {
    type: 'selection'
  },
  {
    title: '状态',
    key: 'status'
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

const deployHistory = ref([])
const deployHistoryColumns = [
  {
    title: '编号',
    key: 'id'
  },
  {
    title: '发布说明',
    key: 'description'
  }
  ,
  {
    title: '发布时间',
    key: 'publishTime'
  }
]




const environmentsOptions = ref([])
const groupNameOptions = ref([])
const serviceOptions = ref([])
onMounted(async () => {
  const resp=await apiClient.request<DeployServiceDto[]>({
    url: '/service/query',
    method: 'get'
  })
  // resp根据name字段排序
  resp.sort((a, b) => a.name.localeCompare(b.name))
  services.value = resp
  environmentsOptions.value = await getEnvironments()
  serviceOptions.value = services.value.map(it => ({ label: `${it.name}  ${it.description}`, value: it.id }))
  let groupNames = services.value.map(it => it.groupName)
  groupNameOptions.value = distinct(groupNames).map(it => ({ label: it, value: it }))
  changeEnvironment(deployData.environment,false)
  changeGroupName(deployData.groupName,false)
  changeService(deployData.deployServiceId)
})

//#region sse

let sseClient :EventSource|null=null
const logScrollBar=ref(null)

onMounted(()=>{
  
  sseClient=new EventSource('/api/sse')
  sseClient.onmessage=(e)=>{
  let arr=e.data.split('|') as string[]
  console.log(arr);
  
  arr=arr.reverse()
  arr=arr.map(it=>{
    return it+'\n'
  })
    deployLogs.value.unshift(...arr)
  }
  
})
onUnmounted(() => {
  sseClient?.close()
})

//#endregion

function changeEnvironment(val,reset=true) {
  const availableServices = services.value.filter(it => it.environmentName == val)
  if(reset){
    deployData.groupName = null
    deployData.deployServiceId = null
  }
  serviceOptions.value = availableServices.map(it => ({ label: `${it.name}  ${it.description}`, value: it.id }))
  let groupNames = availableServices.map(it => it.groupName)
  groupNameOptions.value = distinct(groupNames).map(it => ({ label: it, value: it }))
}

function changeGroupName(val,reset=true) {

  const availableServices = services.value.filter(it => it.environmentName == deployData.environment
    && it.groupName == val)

  serviceOptions.value = availableServices.map(it => ({ label: `${it.name}  ${it.description}`, value: it.id }))
  if(reset){
    deployData.deployServiceId = null
  }

}

/**
 * 选择了服务
 * @param val
 */
async function changeService(val) {
  deployTargets.value = (await apiClient.request({
    url: '/target/query',
    method: 'get',
    params: {
      serviceId: val
    }
  }))
  deployTargets.value.forEach(it => {checkedRowKeysRef.value.push(it.id)
    apiClient.request({
      url: '/target/status',
      method: 'get',
      params:{
        targetId:it.id
      }
    }).then(res=>{
      it.status=res
    })
  })

  updateDeployHistory(val);
  updateStatus();
}

function updateStatus() {
  deployTargets.value.forEach(async it=>{
    var res=await apiClient.request({
      url:'/target/status',
      method:'get',
      params:{
        targetId:it.id
      }
    })
    it.status=res
  })
}
async function updateDeployHistory(serviceId:number|null) {
  deployHistory.value = (await apiClient.request({
    url: '/history/query',
    method: 'get',
    params: {
      serviceId: serviceId
    }
  }))

  checkedRowKeysRef.value = deployTargets.value.map(it => it.id)
}

function deployService() {
  showDeployModal.value=false
  deployLogs.value.length=0
  console.log(checkedRowKeysRef.value)
  apiClient.request({
    url: '/deploy/deploy',
    method: 'post',
    params:{
      targetIds:checkedRowKeysRef.value,
      deployComment:deployComment.value
    }
  })
  updateDeployHistory(deployData.deployServiceId);
}

function installService() {
  showInstallModal.value=false
  apiClient.request({
    url: '/deploy/install',
    method: 'post',
    params:{
      targetIds:checkedRowKeysRef.value
    },
    data:installParams.value
  })
}

function startService() {
  apiClient.request({
    url: '/deploy/start-service',
    method: 'post',
    params:{
      targetIds:checkedRowKeysRef.value
    }
  })
}

function stopService() {
  apiClient.request({
    url: '/deploy/stop-service',
    method: 'post',
    params:{
      targetIds:checkedRowKeysRef.value
    }
  })
}
function clearLog() {
  deployLogs.value = []
}


const showInstallModal=ref(false)
const installParams=ref({
  targetDir:'',
  exeParams:'',
  exePath:'',
  description:'',

})
</script>

<style scoped lang="less">
.deploy-root-box {
  display: flex;
  width: 100%;
  min-height: 600px;
  gap: 10px;

  .deploy-left-box {
    width: 300px;
  }

  .deploy-right-box {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 10px;
  }
}
</style>

<template>
  <div class="agent-update">
    <n-card title="Agent管理">
      <n-space vertical :size="24">
        <n-data-table
          :columns="columns"
          :data="agents"
          :row-key="(row: Agent) => row.id"
          :pagination="{ pageSize: 10 }"
          :loading="loading"
          @update:checked-row-keys="handleSelectionChange"
        />
        <n-space vertical>
          <n-alert type="info">
            请选择要更新的Agent，并上传新版本的exe文件
          </n-alert>
          <n-upload
            accept=".exe"
            :max="1"
            :disabled="!selectedAgentIds.length"
            @change="handleUpload"
          >
            <n-button :disabled="!selectedAgentIds.length">
              {{ selectedAgentIds.length ? '选择更新文件' : '请先选择要更新的Agent' }}
            </n-button>
          </n-upload>
        </n-space>
      </n-space>
    </n-card>
  </div>
</template>

<script lang="ts" setup>
import { ref,  onMounted } from 'vue'
import { useMessage, useDialog } from 'naive-ui'
import type { DataTableColumns } from 'naive-ui'
import { getAgentVersion, getAllAgent, updateAgent } from '@/api/agent'

interface Agent {
  id: string
  name: string
  version: string
  status: string
  lastHeartbeat: string
}

const message = useMessage()
const dialog = useDialog()
const loading = ref(false)
const agents = ref<Agent[]>([])
const selectedAgentIds = ref<number[]>([])

const columns: DataTableColumns<Agent> = [
  {
    type: 'selection',
  },
  {
    title: '地址',
    key: 'host',
  },
  {
    title: '版本号',
    key: 'version',
  }
]

const fetchAgents = async () => {
  loading.value = true
  try {
    const response = await getAllAgent()
    const agentsData = response.data
    
    // 并行获取所有Agent的版本信息
    const agentsWithVersion = await Promise.all(
      agentsData.map(async (agent:any) => {
        try {
          const versionResponse = await getAgentVersion(agent.id)
          return {
            ...agent,
            version: versionResponse.data.version
          }
        } catch (error) {
          console.error(`获取Agent ${agent.id} 版本信息失败:`, error)
          return {
            ...agent,
            version: '获取失败'
          }
        }
      })
    )
    
    agents.value = agentsWithVersion
  } catch (error) {
    message.error('获取Agent列表失败')
  } finally {
    loading.value = false
  }
}

const handleUpload = async ({ file }: { file: any }) => {
  if (!file) return

  try {
    // 使用 Promise 方式等待用户确认
    await new Promise((resolve, reject) => {
      dialog.warning({
        title: '确认更新',
        content: `确定要更新选中的 ${selectedAgentIds.value.length} 个Agent吗？`,
        positiveText: '确定',
        negativeText: '取消',
        onPositiveClick: () => {
          resolve(true)
        },
        onNegativeClick: () => {
          reject(new Error('用户取消'))
        }
      })
    })

    // 用户点击确定后，才会执行以下更新操作
    for (const agentId of selectedAgentIds.value) {
      await updateAgent({
        file: file.file,
        agentServiceName: 'LightDeployAgentV2',
        targetId: agentId
      })
    }
    message.success('更新文件上传成功')
    fetchAgents()
  } catch (error:any) {
    if (error.message === '用户取消') {
      return // 用户取消操作，静默处理
    }
    message.error('更新文件上传失败')
  }
}

const handleSelectionChange = (rowKeys: number[]) => {
  selectedAgentIds.value = rowKeys
}

onMounted(() => {
  fetchAgents()
})
</script>

<style scoped>
.agent-update {
  padding: 24px;
}
</style>
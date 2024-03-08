<template>
  <n-modal
    :show="true"
    class="w-500px h-500px"
    preset="card"
    title="编辑服务"
    size="huge"
    :bordered="false"
    @close="cancel"
  >
    <div class="flex flex-col gap-2">
      <n-select
        placeholder="环境"
        autosize
        v-model:value="newItem.environmentName"
        style="min-width: 10%"
        :options="environmentOptions"
      />
      <n-input
        placeholder="服务类别"
        autosize
        style="min-width: 10%"
        v-model:value="newItem.groupName"
      />
      <n-input placeholder="服务名" autosize style="min-width: 10%" v-model:value="newItem.name" />
      <n-input placeholder="描述" autosize style="min-width: 10%" v-model:value="newItem.description" />
      <n-select
        placeholder="发布方式"
        style="min-width: 10%"
        v-model:value="newItem.deployMode"
        :options="deployType"
      >
      </n-select>

      <n-input
        placeholder="发布路径"
        autosize
        style="min-width: 10%"
        v-model:value="newItem.projectPath"
      />
      <n-switch v-model:value="newItem.isSelfContained">
        <template #checked> 包含运行时</template>
        <template #unchecked> 不包含运行时</template>
      </n-switch>
      <n-switch v-model:value="newItem.enableHealthCheck">
        <template #checked> 开启健康检查</template>
        <template #unchecked> 关闭健康检查</template>
      </n-switch>
      <n-input placeholder="服务端口" v-model:value="newItem.port"></n-input>
      <n-button @click="confirm">确定</n-button>
    </div>
  </n-modal>
</template>

<script setup lang="ts">
import { apiClient } from '@/api/client/apiClient'
import { ref, onActivated, watch, onMounted } from 'vue'
import { deepMerge } from '@/utils'
import type DeployServiceDto from '@/dtos/deployServiceDto'
import { getEnvironments } from '@/api/service/dicService'
const props = defineProps({
  data: {
    type: Object as () => DeployServiceDto
  }
})


const environmentOptions=ref([])
const newItem = ref({})

onMounted(async () => {
   environmentOptions.value= await getEnvironments();
  deepMerge(newItem.value, props.data)
})



const deployType = [
  {
    label: '项目',
    value: 0
  },
  {
    label: '文件夹',
    value: 1
  }
]

const emits = defineEmits(['cancel','confirm'])

async function confirm() {
  await apiClient.request<any>({
    url: '/service/update',
    method: 'post',
    data: newItem.value
  })
  emits('confirm')
  window.$message.success('新增成功')
}

function cancel() {
  emits('cancel')
}

</script>

<style scoped></style>

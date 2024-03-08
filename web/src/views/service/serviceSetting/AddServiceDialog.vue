<template>
  <n-modal
    :show="true"
    class="w-500px h-500px"
    preset="card"
    :title="'新增服务'"
    size="huge"
    :bordered="false"
    @close="cancel"
  >
    <div class="flex flex-col gap-2">
      <n-select
        placeholder="环境"
        autosize
        v-model:value="data.environmentName"
        style="min-width: 10%"
        :options="environmentOptions"
      />
      <n-input
        placeholder="服务类别"
        autosize
        style="min-width: 10%"
        v-model:value="data.groupName"
      />
      <n-input placeholder="服务名" autosize style="min-width: 10%" v-model:value="data.name" />
      <n-input placeholder="描述" autosize style="min-width: 10%" v-model:value="data.description" />
      <n-select
        placeholder="发布方式"
        style="min-width: 10%"
        v-model:value="data.deployMode"
        :options="deployType"
      >
      </n-select>

      <n-input
        placeholder="发布路径"
        autosize
        style="min-width: 10%"
        v-model:value="data.projectPath"
      />
      <n-switch v-model:value="data.isSelfContained">
        <template #checked> 包含运行时</template>
        <template #unchecked> 不包含运行时</template>
      </n-switch>
      <n-switch v-model:value="data.enableHealthCheck">
        <template #checked> 开启健康检查</template>
        <template #unchecked> 关闭健康检查</template>
      </n-switch>
      <n-input placeholder="服务端口" v-model:value="data.port"></n-input>
      <n-button @click="confirm">确定</n-button>
    </div>
  </n-modal>
</template>

<script setup lang="ts">
import { apiClient } from '@/api/client/apiClient'
import { ref, onMounted } from 'vue'
import { getEnvironments } from '@/api/service/dicService'

const environmentOptions=ref([])

onMounted(async () => {
    environmentOptions.value= await getEnvironments();
})

const data = ref({})

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

const emits = defineEmits(['confirm','cancel'])

async function confirm() {
  await apiClient.request<any>({
    url: '/service/insert',
    method: 'post',
    data: data.value
  })
  emits('confirm')
  window.$message.success('新增成功')
}

function cancel() {
  emits('cancel')
}

</script>

<style scoped></style>

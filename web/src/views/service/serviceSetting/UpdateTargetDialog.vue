<template>
  <n-modal
      :show="true"
      class="h-90vh w-90vw"
      preset="card"
      title="发布目标配置"
      size="huge"
      :bordered="false"
      @close="cancel"
  >
    <div class="flex flex-col gap-2">
      <n-input placeholder="服务器IP" v-model:value="target.host"></n-input>
      <n-input placeholder="服务器端口" v-model:value.number="target.port"></n-input>
      <n-input placeholder="健康检查url" v-model:value="target.healthCheckUrl"></n-input>
      <n-input placeholder="权限认证Key" v-model:value="target.authKey"></n-input>
      <n-button type="primary" @click="addClick">新增</n-button>
    </div>
    <n-data-table :data="targets" :columns="columns"/>
  </n-modal>
</template>

<script setup lang="ts">

import {onMounted, ref, h} from 'vue'
import {apiClient} from '@/api/client/apiClient'
import type {DeployTargetDto} from '@/dtos/deployTargetDto'
import {NButton, NInput} from 'naive-ui'

const props = defineProps({
  data: Object
})

const emits = defineEmits(['confirm', 'cancel'])

const target = ref<DeployTargetDto>({
  serviceId: props.data.id
})
let targets = ref([])

function search() {
  apiClient.request({
    url: '/target/query',
    method: 'get',
    params: {
      serviceId: props.data.id ?? 0
    }
  }).then(res => {
    targets.value = res
  })
}

onMounted(() => {
  search()
})

function cancel() {
  emits('cancel')
}

function addClick() {
  apiClient.request({
    url: '/target/insert',
    method: 'post',
    data: target.value
  }).then(() => {
    search()
    window.$message.success("操作成功")
  })

}

const columns = [
  {
    title: '服务器IP',
    key: 'host',
    render(row: any, index: any) {
      return h(NInput, {
        value: row.host,
        onUpdateValue(v) {
          targets.value[index].host = v
        }
      })
    }
  },
  {
    title: '服务器端口',
    key: 'port',
    render(row: any, index: any) {
      return h(NInput, {
        value: row.port,
        onUpdateValue(v) {
          targets.value[index].port = v
        }
      })
    }
  },
  {
    title: '健康检查url',
    key: 'healthCheckUrl',
    render(row: any, index: any) {
      return h(NInput, {
        value: row.healthCheckUrl,
        onUpdateValue(v) {
          targets.value[index].healthCheckUrl = v
        }
      })
    }
  },
  {
    title: '权限认证Key',
    key: 'authKey',
    render(row: any, index: any) {
      return h(NInput, {
        value: row.authKey,
        onUpdateValue(v) {
          targets.value[index].authKey = v
        }
      })
    }
  }, {
    title: '操作',
    key: 'action',
    render(row: any) {
      return h(
          'div',
          {
            class: 'flex gap-2'
          },
          [
            h(
                NButton,
                {
                  type: 'primary',
                  onClick: () => updateRow(row)
                },
                () => '更新'
            ),
            h(
                NButton,
                {
                  type: 'error',
                  onClick: () => deleteRow(row)
                },
                () => '删除'
            )
          ]
      )
    }
  }]

function deleteRow(row: any) {
  apiClient.request({
    url: '/target/delete',
    method: 'post',
    params: {id: row.id}
  }).then(() => {
    search()
    window.$message.success('删除成功')
  })

}

function updateRow(row: any) {
  debugger
  apiClient.request({
    url: '/target/update',
    method: 'post',
    data: row
  }).then(() => {
    window.$message.success('更新成功')
  })
}
</script>

<style lang="less">


</style>

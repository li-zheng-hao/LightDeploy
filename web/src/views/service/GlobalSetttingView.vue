<template>
  <tab-page>
    <div class="flex gap-2 items-center ">
      <div>企业微信秘钥：</div>
      <n-input v-model:value="data.qiyeWeChatKey" placeholder="企业微信秘钥" style="max-width: 500px"></n-input>
      <n-button @click="onSave" style="width: 200px">保存</n-button>
    </div>
  </tab-page>
</template>

<script setup lang="ts">

import TabPage from "@/components/TabPage.vue";

import {onMounted, ref} from "vue";
import {apiClient} from "@/api/client/apiClient";

const data=ref({
  qiyeWeChatKey:'',
  id:undefined
})
onMounted(async ()=>{
  data.value=await apiClient.request({
    url:'/setting/query',
    method:'get',
  })
  if(data.value==null||data.value.qiyeWeChatKey==null){
    data.value={
      qiyeWeChatKey:'',
      id:undefined
    }
  }
})
function onSave() {
  apiClient.request({
    url:'/setting/update',
    method:'post',
    data:data.value
  })
  window.$message.success("操作成功")
}
</script>

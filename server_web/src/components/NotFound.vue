<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()

const goHome = () => {
  router.push('/')
}
</script>

<template>
  <div class="flex flex-col items-center justify-center min-h-screen bg-gray-100">
    <div class="text-9xl font-bold text-gray-300">404</div>
    <div class="mt-4 text-2xl text-gray-600">页面未找到</div>
    <div class="mt-2 text-gray-500">抱歉，您访问的页面不存在</div>
    <n-button class="mt-8" type="primary" @click="goHome">返回首页</n-button>
  </div>
</template>

<style scoped>
</style>
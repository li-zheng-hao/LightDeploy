<script setup lang="ts">
import { NLayout, NLayoutSider, NLayoutHeader, NLayoutContent, NLayoutFooter, NMenu, NIcon } from 'naive-ui'
import { ref, watchEffect, nextTick } from 'vue'
import { useRoute } from 'vue-router'
import type { MenuInst } from 'naive-ui'
import {
  RocketOutline as LogoIcon
} from '@vicons/ionicons5'
import { menuOptions } from '../config/menuConfig'
import { useMessage } from 'naive-ui'

window.$message = useMessage()
const route = useRoute()
const collapsed = ref(false)
const activeKey = ref<string>('')
const menuInstRef = ref<MenuInst | null>(null)

// 监听路由变化，更新菜单选中状态并展开
watchEffect(() => {
  activeKey.value = route.path
  // 使用 nextTick 确保菜单实例已经挂载
  nextTick(() => {
    menuInstRef.value?.showOption(route.path)
  })
})


</script>

<template>
      <n-layout has-sider class="h-screen">
        <n-layout-sider
          bordered
          show-trigger
          collapse-mode="width"
          :collapsed="collapsed"
          :collapsed-width="64"
          :width="240"
          @collapse="collapsed = true"
          @expand="collapsed = false"
        >
          <div class="flex items-center justify-center h-16 gap-2" :class="{ 'justify-center': collapsed }">
            <n-icon size="32" class="text-primary">
              <logo-icon />
            </n-icon>
            <h1 v-if="!collapsed" class="text-lg font-semibold">LightDeploy</h1>
          </div>
          <n-menu
            ref="menuInstRef"
            :collapsed="collapsed"
            :collapsed-width="64"
            :collapsed-icon-size="22"
            :options="menuOptions"
            :value="activeKey"
          />
        </n-layout-sider>
        <n-layout>
          <n-layout-header bordered class="px-6 py-4">
            LightDeploy 轻量级部署平台
          </n-layout-header>
          <n-layout-content class="p-6 min-h-[calc(100vh-120px)]">
            <router-view></router-view>
          </n-layout-content>
          <n-layout-footer class="h-[60px] flex items-center justify-center border-t border-gray-200 text-gray-600">
            LightDeploy © 2025
          </n-layout-footer>
        </n-layout>
      </n-layout>
</template>

<style scoped>
</style>

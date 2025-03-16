import { h } from 'vue'
import { RouterLink } from 'vue-router'
import {
  HomeOutline as HomeIcon,
  ServerOutline as ServerIcon,
  SettingsOutline as SettingsIcon,
  DocumentTextOutline as DocsIcon,
  InformationCircleOutline as AboutIcon
} from '@vicons/ionicons5'


export const menuOptions = [
  {
    label: () => h(RouterLink, { to: '/' }, { default: () => '仪表盘' }),
    key: '/',
    icon: () => h(HomeIcon)
  },
  {
    label: '部署管理',
    key: '/deploy-group',
    icon: () => h(ServerIcon),
    children: [
      {
        label: () => h(RouterLink, { to: '/service-config' }, { default: () => '服务配置' }),
        key: '/service-config'
      },
      {
        label: () => h(RouterLink, { to: '/deploy' }, { default: () => '部署列表' }),
        key: '/deploy'
      },
      {
        label: () => h(RouterLink, { to: '/deploy/history' }, { default: () => '部署历史' }),
        key: '/deploy-history'
      },
       {
        label: () => h(RouterLink, { to: '/agent-update' }, { default: () => '代理管理' }),
        key: '/agent-update'
      },
    ]
  },
  {
    label: '系统设置',
    key: '/settings-group',
    icon: () => h(SettingsIcon),
    children: [
      {
        label: () => h(RouterLink, { to: '/settings/general' }, { default: () => '常规设置' }),
        key: '/settings/general'
      },
      {
        label: () => h(RouterLink, { to: '/settings/security' }, { default: () => '安全配置' }),
        key: '/settings/security'
      }
    ]
  },
  {
    label: () => h(RouterLink, { to: '/docs' }, { default: () => '文档中心' }),
    key: '/docs',
    icon: () => h(DocsIcon)
  },
  {
    label: () => h(RouterLink, { to: '/about' }, { default: () => '关于' }),
    key: '/about',
    icon: () => h(AboutIcon)
  }
]
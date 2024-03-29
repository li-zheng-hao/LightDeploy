// 全局的菜单配置 (根据当前登录用户权限生成左侧菜单栏)

import type { MenuItem } from "@/stores/menuStore";

// 也可以从后端获取
export function getAllMenus() {
  return [
    {
      label: '首页',
      key: '/home',
      icon: 'DashboardOutlined',
      routePath: '/home'
    },
    {
      label: '系统设置',
      key: '/sys',
      icon: 'SettingOutlined',
      routePath: null,
      children: [
        {
          label: '角色权限管理',
          key: '/sys/role',
          routePath: '/sys/role'
        },
        {
          label: '菜单权限管理',
          key: '/sys/menu',
          routePath: '/sys/menu'
        }
      ]
    },
    {
      label: '异常页面',
      key: '/error',
      icon: 'WarningOutlined',
      routePath: null,
      children: [
        {
          label: '404',
          key: '/error/404',
          routePath: '/error/404'
        },
        {
          label: '403',
          key: '/error/403',
          routePath: '/error/403'
        },
        {
          label: '500',
          key: '/error/500',
          routePath: '/error/500'
        }
      ]
    },
    {
      label: '服务管理',
      key: '/project',
      icon: 'UserOutlined',
      routePath: null,
      children: [
        {
          label: '服务配置',
          key: '/project/service',
          routePath: '/project/service'
        },
        {
          label: '全局设置',
          key: '/project/setting',
          routePath: '/project/setting'
        },
        {
          label: '服务发布',
          key: '/project/deploy',
          routePath: '/project/deploy'
        },{
          label: '代理管理',
          key: '/project/agent',
          routePath: '/project/agent'
        }
      ]
    },
    // {
    //   label: '组件示例',
    //   key: '/example',
    //   icon: 'UserOutlined',
    //   routePath: null,
    //   children: [
    //     {
    //       label: '按钮',
    //       key: '/example/button',
    //       routePath: null,
    //       icon: 'HandClick',
    //       children: [
    //         {
    //           label: '按钮1',
    //           key: '/example/button/button1',
    //           routePath: '/example/button/button1'
    //         },
    //         {
    //           label: '按钮2',
    //           key: '/example/button/button2',
    //           routePath: '/example/button/button2'
    //         }
    //       ]
    //     },
    //     {
    //       label: '表格示例',
    //       key: '/example/table',
    //       routePath: '/example/table',
    //       icon: 'TableIcon'
    //     },
    //     {
    //       label: 'XTable表格示例',
    //       key: '/example/xtable',
    //       routePath: '/example/xtable',
    //       icon: 'TableIcon'
    //     },
    //     {
    //       label: '图表示例',
    //       key: '/example/echarts',
    //       routePath:'/example/echarts',
    //       icon: 'TableIcon'
    //     }
    //   ]
    // },
    {
      label: '第三方内嵌页面',
      key: '/thirdpart',
      icon: 'Share',
      routePath: '/thirdpart'
    }
    ,
    {
      label: '外部页面',
      key: '/baidu',
      icon: 'Share',
      routePath: 'https://www.baidu.com'
    }
  ] as MenuItem[]
}

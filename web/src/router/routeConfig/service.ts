export function getResultRoutes() {
  return [
    {
      path: '/project/service',
      name: 'ServiceSettingView',
      component: () => import('@/views/service/ServiceSettingView.vue')
    },
    {
      path: '/project/setting',
      name: 'GlobalSetttingView',
      component: () => import('@/views/service/GlobalSetttingView.vue')
    },
    {
      path: '/project/deploy',
      name: 'ServiceDeployView',
      component: () => import('@/views/service/ServiceDeployView.vue')
    },
    {
      path: '/project/agent',
      name: 'UpdateAgentView',
      component: () => import('@/views/service/UpdateAgentView.vue')
    }
  ]
}

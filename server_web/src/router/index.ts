import { createRouter, createWebHashHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'

const routes: Array<RouteRecordRaw> = [
  {
    path: '/',
    name: 'Home',
    component: () => import('../views/Home.vue')
  },
  {
    path: '/about',
    name: 'About',
    component: () => import('../views/About.vue')
  },
  {
    path: '/service-config',
    name: 'ServiceConfig',
    component: () => import('../views/ServiceConfig.vue')
  },
  {
    path: '/deploy',
    name: 'Deploy',
    component: () => import('../views/Deploy.vue')
  },
  {  path: '/agent-update',
    name: 'AgentUpdate',
    component: () => import('../views/AgentUpdate.vue')},
  {
    path: '/:pathMatch(.*)*',
    name: 'NotFound',
    component: () => import('../components/NotFound.vue')
  }
]

const router = createRouter({
  history: createWebHashHistory(),
  routes
})

export default router
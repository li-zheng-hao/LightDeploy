import{T as Y}from"./TabPage-7jt19OsU.js";import{d as Z,r as n,o as T,a7 as v,ay as B,az as j,l as J,O as g,P as I,X as o,W as d,R as t,S as c,U as z,_ as Q,F as ee,Y as E,a6 as le,aA as te,aw as ae,Z as oe,a1 as ne,a2 as se}from"./index-1XSuJy-d.js";import{g as ue}from"./dicService-3RU2sEKI.js";import{_ as re}from"./Scrollbar-3RDySHra.js";import{N as ie,a as pe}from"./DataTable-O2-QU6GO.js";import{_ as de}from"./Input-lcRWAkuh.js";import{_ as ve}from"./_plugin-vue_export-helper-x3n3nnut.js";import"./Icon-nXACwdMM.js";import"./use-merged-state-Cofp-Jf5.js";import"./Dropdown-YJhgMFX3.js";import"./Follower-itOr3lBI.js";import"./FocusDetector-r6OEcYlO.js";const U=f=>(ne("data-v-4001e030"),f=f(),se(),f),ce={class:"deploy-root-box"},me={class:"deploy-left-box"},_e={class:"flex flex-col gap-2"},ye=U(()=>d("div",null,"环境：",-1)),ge=U(()=>d("div",null,"服务分组：",-1)),fe=U(()=>d("div",null,"服务名：",-1)),he={class:"deploy-right-box"},xe={class:"flex gap-4 flex-col"},ke={class:"flex gap-4 flex-col"},Se=Z({__name:"ServiceDeployView",setup(f){const m=n([]),h=n([]),x=n(!1),k=n([]),N=n(""),i=n([]),O=[{type:"selection"},{title:"编号",key:"id"},{title:"IP",key:"host"},{title:"端口",key:"port"},{title:"健康检查地址",key:"healthCheckUrl"},{title:"权限认证",key:"authKey"}],q=n([]),R=[{title:"编号",key:"id"},{title:"发布说明",key:"description"},{title:"发布时间",key:"publishTime"}],r=n({environment:null,groupName:null,deployServiceId:null}),P=n([]),C=n([]),S=n([]);T(async()=>{m.value=await v.request({url:"/service/query",method:"get"}),P.value=await ue(),S.value=m.value.map(e=>({label:`${e.name}  ${e.description}`,value:e.id}));let u=m.value.map(e=>e.groupName);C.value=B(u).map(e=>({label:e,value:e}))});let _=null;const $=n(null);T(()=>{_=new EventSource("/api/sse"),_.onmessage=u=>{k.value.push(u.data),j(()=>{var e;(e=$.value)==null||e.scrollBy(0,200)})}}),J(()=>{_==null||_.close()});function F(u){const e=m.value.filter(a=>a.environmentName==u);r.value.groupName=null,r.value.deployServiceId=null,S.value=e.map(a=>({label:`${a.name}  ${a.description}`,value:a.id}));let s=e.map(a=>a.groupName);C.value=B(s).map(a=>({label:a,value:a}))}function K(u){const e=m.value.filter(s=>s.environmentName==r.value.environment&&s.groupName==u);S.value=e.map(s=>({label:`${s.name}  ${s.description}`,value:s.id})),r.value.deployServiceId=null}async function L(u){h.value=await v.request({url:"/target/query",method:"get",params:{serviceId:u}}),h.value.forEach(e=>i.value.push(e.id)),q.value=await v.request({url:"/history/query",method:"get",params:{serviceId:u}}),i.value=h.value.map(e=>e.id)}function M(){x.value=!1,k.value.length=0,console.log(i.value),v.request({url:"/deploy/deploy",method:"post",params:{targetIds:i.value,deployComment:N.value}})}function H(){w.value=!1,v.request({url:"/deploy/install",method:"post",params:{targetIds:i.value},data:p.value})}function A(){v.request({url:"/deploy/start-service",method:"post",params:{targetIds:i.value}})}function G(){v.request({url:"/deploy/stop-service",method:"post",params:{targetIds:i.value}})}function W(){k.value=[]}const w=n(!1),p=n({targetDir:"",exePrams:"",exePath:"",description:""});return(u,e)=>{const s=ie,a=le,b=te,V=pe,X=re,y=de,D=ae;return g(),I(Y,null,{default:o(()=>[d("div",ce,[d("div",me,[t(b,{title:"部署选择"},{default:o(()=>[d("div",_e,[ye,t(s,{value:r.value.environment,"onUpdate:value":e[0]||(e[0]=l=>r.value.environment=l),options:P.value,onUpdateValue:F},null,8,["value","options"]),ge,t(s,{value:r.value.groupName,"onUpdate:value":e[1]||(e[1]=l=>r.value.groupName=l),options:C.value,onUpdateValue:K},null,8,["value","options"]),fe,t(s,{value:r.value.deployServiceId,"onUpdate:value":e[2]||(e[2]=l=>r.value.deployServiceId=l),options:S.value,onUpdateValue:L},null,8,["value","options"]),t(a,{onClick:e[3]||(e[3]=l=>x.value=!0),type:"primary",style:{"margin-top":"10px"}},{default:o(()=>[c("部署服务")]),_:1}),t(a,{onClick:e[4]||(e[4]=l=>w.value=!0)},{default:o(()=>[c("安装服务")]),_:1}),t(a,{onClick:A},{default:o(()=>[c("启动服务")]),_:1}),t(a,{onClick:G},{default:o(()=>[c("停止服务")]),_:1}),t(a,{onClick:W},{default:o(()=>[c("清空日志")]),_:1})])]),_:1})]),d("div",he,[t(b,{title:"目标选择"},{default:o(()=>[t(V,{data:h.value,columns:O,"row-key":l=>l.id,style:{height:"150px"},"flex-height":"",size:"small","checked-row-keys":i.value,"onUpdate:checkedRowKeys":e[5]||(e[5]=l=>i.value=l)},null,8,["data","row-key","checked-row-keys"])]),_:1}),t(b,{title:"发布历史"},{default:o(()=>[t(V,{data:q.value,columns:R,size:"small","flex-height":"",style:{height:"150px"}},null,8,["data"])]),_:1}),t(b,{title:"控制台"},{default:o(()=>[t(X,{style:{"max-height":"300px"},trigger:"none",class:"log-box-scrollbar",ref_key:"logScrollBar",ref:$},{default:o(()=>[(g(!0),z(ee,null,Q(k.value,l=>(g(),z("div",null,oe(l),1))),256))]),_:1},512)]),_:1})])]),x.value?(g(),I(D,{key:0,show:!0,preset:"card",title:"发布说明",style:{width:"500px",height:"200px"},size:"huge",bordered:!1,onClose:e[7]||(e[7]=l=>x.value=!1)},{default:o(()=>[d("div",xe,[t(y,{placeholder:"发布说明",value:N.value,"onUpdate:value":e[6]||(e[6]=l=>N.value=l)},null,8,["value"]),t(a,{onClick:M},{default:o(()=>[c("开始部署")]),_:1})])]),_:1})):E("",!0),w.value?(g(),I(D,{key:1,show:!0,preset:"card",title:"安装服务",style:{width:"500px",height:"500px"},size:"huge",bordered:!1,onClose:e[12]||(e[12]=l=>w.value=!1)},{default:o(()=>[d("div",ke,[t(y,{placeholder:"服务安装目录",value:p.value.targetDir,"onUpdate:value":e[8]||(e[8]=l=>p.value.targetDir=l)},null,8,["value"]),t(y,{placeholder:"可执行程序(相对安装目录)",value:p.value.exePath,"onUpdate:value":e[9]||(e[9]=l=>p.value.exePath=l)},null,8,["value"]),t(y,{placeholder:"执行参数",value:p.value.exePrams,"onUpdate:value":e[10]||(e[10]=l=>p.value.exePrams=l)},null,8,["value"]),t(y,{placeholder:"服务描述",value:p.value.description,"onUpdate:value":e[11]||(e[11]=l=>p.value.description=l)},null,8,["value"]),t(a,{onClick:H},{default:o(()=>[c("开始安装")]),_:1})])]),_:1})):E("",!0)]),_:1})}}}),Be=ve(Se,[["__scopeId","data-v-4001e030"]]);export{Be as default};

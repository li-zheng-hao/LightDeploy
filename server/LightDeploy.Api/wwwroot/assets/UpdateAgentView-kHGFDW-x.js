import{T as B}from"./TabPage-l_kLhg_1.js";import{d as T,r as n,o as g,a7 as h,az as C,l as U,O as u,P,X as i,W as f,R as s,S,U as y,_ as z,F,a6 as N,aA as R,Z as V}from"./index-mJUAFJoQ.js";import{_ as q}from"./Input--1lxrMj0.js";import{a as A}from"./DataTable-h40o47lS.js";import{_ as E}from"./Scrollbar-z_L0Kp3P.js";import"./_plugin-vue_export-helper-x3n3nnut.js";import"./use-merged-state-24cQO82p.js";import"./Icon-TeiBA-kU.js";import"./Dropdown-zTqd8F1V.js";import"./Follower-gq7Zi2AD.js";import"./FocusDetector-wmttWzEh.js";const I={class:"flex gap-2 flex-items-center justify-center"},K=f("div",{style:{color:"lightgray"}}," 注意：更新操作会将安装包上传到服务器并解压，然后重启服务,在控制台输出操作完成后会花费一定的时间(不超过2分钟)重启Agent服务， 期间不要进行任何发布更新操作，如果长时间未完成需要自行检查原因 ",-1),Q=T({__name:"UpdateAgentView",setup(D){const _=n([]),k=[{type:"selection"},{title:"编号",key:"id"},{title:"IP",key:"host"},{title:"端口",key:"port"},{title:"健康检查地址",key:"healthCheckUrl"},{title:"权限认证",key:"authKey"}],a=n([]),c=n("");g(()=>{h.request({url:"/target/query",method:"get"}).then(r=>{const e=[];r.forEach(o=>{e.findIndex(p=>p.host===o.host)===-1&&e.push(o)}),_.value=e,a.value=e.map(o=>o.id)})});function v(){console.log(a.value),h.request({url:"/deploy/update-agent",method:"post",params:{targetIds:a.value,zipFilePath:c.value}}).then(()=>{window.$message.success("操作成功")})}const d=n([]);let l=null;const m=n(null);return g(()=>{l=new EventSource("/api/sse"),l.onmessage=r=>{d.value.push(r.data),C(()=>{var e;(e=m.value)==null||e.scrollBy(0,200)})}}),U(()=>{l==null||l.close()}),(r,e)=>{const o=q,p=N,x=A,w=E,b=R;return u(),P(B,null,{default:i(()=>[f("div",I,[s(o,{placeholder:"安装包完整路径",style:{"max-width":"500px"},value:c.value,"onUpdate:value":e[0]||(e[0]=t=>c.value=t)},null,8,["value"]),s(p,{onClick:v},{default:i(()=>[S("开始更新")]),_:1}),K]),s(x,{data:_.value,columns:k,"row-key":t=>t.id,style:{height:"400px","margin-top":"20px"},"flex-height":"",size:"small","checked-row-keys":a.value,"onUpdate:checkedRowKeys":e[1]||(e[1]=t=>a.value=t)},null,8,["data","row-key","checked-row-keys"]),s(b,{title:"控制台",style:{"margin-top":"20px"}},{default:i(()=>[s(w,{style:{"max-height":"300px"},trigger:"none",class:"log-box-scrollbar",ref_key:"logScrollBar",ref:m},{default:i(()=>[(u(!0),y(F,null,z(d.value,t=>(u(),y("div",null,V(t),1))),256))]),_:1},512)]),_:1})]),_:1})}}});export{Q as default};
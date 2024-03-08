import{v as Ue,an as $e,ao as ze,c as ce,e as c,ap as ve,a as ne,b as $,H as he,d as q,u as Re,f as pe,aq as Be,r as k,t as Ve,i as L,ae as F,ar as le,as as z,j as Ne,at as oe,h as b,ag as T,au as Fe,av as Te,A as ie,o as G,O as M,P,X as w,W as X,R as o,S as _,a7 as N,a6 as R,aw as se,ax as Me,Q as K,Y as re}from"./index-1XSuJy-d.js";import{T as Pe}from"./TabPage-7jt19OsU.js";import{g as ue}from"./dicService-3RU2sEKI.js";import{N as de,a as me}from"./DataTable-O2-QU6GO.js";import{_ as U}from"./Input-lcRWAkuh.js";import{u as je}from"./use-merged-state-Cofp-Jf5.js";import{_ as Ke}from"./_plugin-vue_export-helper-x3n3nnut.js";import"./Icon-nXACwdMM.js";import"./Dropdown-YJhgMFX3.js";import"./Follower-itOr3lBI.js";import"./FocusDetector-r6OEcYlO.js";const De=e=>{const{primaryColor:y,opacityDisabled:v,borderRadius:l,textColor3:a}=e;return Object.assign(Object.assign({},$e),{iconColor:a,textColor:"white",loadingColor:y,opacityDisabled:v,railColor:"rgba(0, 0, 0, .14)",railColorActive:y,buttonBoxShadow:"0 1px 4px 0 rgba(0, 0, 0, 0.3), inset 0 0 1px 0 rgba(0, 0, 0, 0.05)",buttonColor:"#FFF",railBorderRadiusSmall:l,railBorderRadiusMedium:l,railBorderRadiusLarge:l,buttonBorderRadiusSmall:l,buttonBorderRadiusMedium:l,buttonBorderRadiusLarge:l,boxShadowFocus:`0 0 0 2px ${ze(y,{alpha:.2})}`})},Oe={name:"Switch",common:Ue,self:De},He=Oe,qe=ce("switch",`
 height: var(--n-height);
 min-width: var(--n-width);
 vertical-align: middle;
 user-select: none;
 -webkit-user-select: none;
 display: inline-flex;
 outline: none;
 justify-content: center;
 align-items: center;
`,[c("children-placeholder",`
 height: var(--n-rail-height);
 display: flex;
 flex-direction: column;
 overflow: hidden;
 pointer-events: none;
 visibility: hidden;
 `),c("rail-placeholder",`
 display: flex;
 flex-wrap: none;
 `),c("button-placeholder",`
 width: calc(1.75 * var(--n-rail-height));
 height: var(--n-rail-height);
 `),ce("base-loading",`
 position: absolute;
 top: 50%;
 left: 50%;
 transform: translateX(-50%) translateY(-50%);
 font-size: calc(var(--n-button-width) - 4px);
 color: var(--n-loading-color);
 transition: color .3s var(--n-bezier);
 `,[ve({left:"50%",top:"50%",originalTransform:"translateX(-50%) translateY(-50%)"})]),c("checked, unchecked",`
 transition: color .3s var(--n-bezier);
 color: var(--n-text-color);
 box-sizing: border-box;
 position: absolute;
 white-space: nowrap;
 top: 0;
 bottom: 0;
 display: flex;
 align-items: center;
 line-height: 1;
 `),c("checked",`
 right: 0;
 padding-right: calc(1.25 * var(--n-rail-height) - var(--n-offset));
 `),c("unchecked",`
 left: 0;
 justify-content: flex-end;
 padding-left: calc(1.25 * var(--n-rail-height) - var(--n-offset));
 `),ne("&:focus",[c("rail",`
 box-shadow: var(--n-box-shadow-focus);
 `)]),$("round",[c("rail","border-radius: calc(var(--n-rail-height) / 2);",[c("button","border-radius: calc(var(--n-button-height) / 2);")])]),he("disabled",[he("icon",[$("rubber-band",[$("pressed",[c("rail",[c("button","max-width: var(--n-button-width-pressed);")])]),c("rail",[ne("&:active",[c("button","max-width: var(--n-button-width-pressed);")])]),$("active",[$("pressed",[c("rail",[c("button","left: calc(100% - var(--n-offset) - var(--n-button-width-pressed));")])]),c("rail",[ne("&:active",[c("button","left: calc(100% - var(--n-offset) - var(--n-button-width-pressed));")])])])])])]),$("active",[c("rail",[c("button","left: calc(100% - var(--n-button-width) - var(--n-offset))")])]),c("rail",`
 overflow: hidden;
 height: var(--n-rail-height);
 min-width: var(--n-rail-width);
 border-radius: var(--n-rail-border-radius);
 cursor: pointer;
 position: relative;
 transition:
 opacity .3s var(--n-bezier),
 background .3s var(--n-bezier),
 box-shadow .3s var(--n-bezier);
 background-color: var(--n-rail-color);
 `,[c("button-icon",`
 color: var(--n-icon-color);
 transition: color .3s var(--n-bezier);
 font-size: calc(var(--n-button-height) - 4px);
 position: absolute;
 left: 0;
 right: 0;
 top: 0;
 bottom: 0;
 display: flex;
 justify-content: center;
 align-items: center;
 line-height: 1;
 `,[ve()]),c("button",`
 align-items: center; 
 top: var(--n-offset);
 left: var(--n-offset);
 height: var(--n-button-height);
 width: var(--n-button-width-pressed);
 max-width: var(--n-button-width);
 border-radius: var(--n-button-border-radius);
 background-color: var(--n-button-color);
 box-shadow: var(--n-button-box-shadow);
 box-sizing: border-box;
 cursor: inherit;
 content: "";
 position: absolute;
 transition:
 background-color .3s var(--n-bezier),
 left .3s var(--n-bezier),
 opacity .3s var(--n-bezier),
 max-width .3s var(--n-bezier),
 box-shadow .3s var(--n-bezier);
 `)]),$("active",[c("rail","background-color: var(--n-rail-color-active);")]),$("loading",[c("rail",`
 cursor: wait;
 `)]),$("disabled",[c("rail",`
 cursor: not-allowed;
 opacity: .5;
 `)])]),Ie=Object.assign(Object.assign({},pe.props),{size:{type:String,default:"medium"},value:{type:[String,Number,Boolean],default:void 0},loading:Boolean,defaultValue:{type:[String,Number,Boolean],default:!1},disabled:{type:Boolean,default:void 0},round:{type:Boolean,default:!0},"onUpdate:value":[Function,Array],onUpdateValue:[Function,Array],checkedValue:{type:[String,Number,Boolean],default:!0},uncheckedValue:{type:[String,Number,Boolean],default:!1},railStyle:Function,rubberBand:{type:Boolean,default:!0},onChange:[Function,Array]});let H;const fe=q({name:"Switch",props:Ie,setup(e){H===void 0&&(typeof CSS<"u"?typeof CSS.supports<"u"?H=CSS.supports("width","max(1px)"):H=!1:H=!0);const{mergedClsPrefixRef:y,inlineThemeDisabled:v}=Re(e),l=pe("Switch","-switch",qe,He,e,y),a=Be(e),{mergedSizeRef:m,mergedDisabledRef:h}=a,x=k(e.defaultValue),S=Ve(e,"value"),s=je(S,x),i=L(()=>s.value===e.checkedValue),g=k(!1),t=k(!1),r=L(()=>{const{railStyle:p}=e;if(p)return p({focused:t.value,checked:i.value})});function d(p){const{"onUpdate:value":W,onChange:A,onUpdateValue:E}=e,{nTriggerFormInput:J,nTriggerFormChange:Z}=a;W&&ie(W,p),E&&ie(E,p),A&&ie(A,p),x.value=p,J(),Z()}function u(){const{nTriggerFormFocus:p}=a;p()}function n(){const{nTriggerFormBlur:p}=a;p()}function f(){e.loading||h.value||(s.value!==e.checkedValue?d(e.checkedValue):d(e.uncheckedValue))}function Y(){t.value=!0,u()}function I(){t.value=!1,n(),g.value=!1}function D(p){e.loading||h.value||p.key===" "&&(s.value!==e.checkedValue?d(e.checkedValue):d(e.uncheckedValue),g.value=!1)}function Q(p){e.loading||h.value||p.key===" "&&(p.preventDefault(),g.value=!0)}const C=L(()=>{const{value:p}=m,{self:{opacityDisabled:W,railColor:A,railColorActive:E,buttonBoxShadow:J,buttonColor:Z,boxShadowFocus:be,loadingColor:ge,textColor:we,iconColor:ye,[F("buttonHeight",p)]:B,[F("buttonWidth",p)]:ke,[F("buttonWidthPressed",p)]:_e,[F("railHeight",p)]:V,[F("railWidth",p)]:O,[F("railBorderRadius",p)]:xe,[F("buttonBorderRadius",p)]:Ce},common:{cubicBezierEaseInOut:Se}}=l.value;let ee,te,ae;return H?(ee=`calc((${V} - ${B}) / 2)`,te=`max(${V}, ${B})`,ae=`max(${O}, calc(${O} + ${B} - ${V}))`):(ee=le((z(V)-z(B))/2),te=le(Math.max(z(V),z(B))),ae=z(V)>z(B)?O:le(z(O)+z(B)-z(V))),{"--n-bezier":Se,"--n-button-border-radius":Ce,"--n-button-box-shadow":J,"--n-button-color":Z,"--n-button-width":ke,"--n-button-width-pressed":_e,"--n-button-height":B,"--n-height":te,"--n-offset":ee,"--n-opacity-disabled":W,"--n-rail-border-radius":xe,"--n-rail-color":A,"--n-rail-color-active":E,"--n-rail-height":V,"--n-rail-width":O,"--n-width":ae,"--n-box-shadow-focus":be,"--n-loading-color":ge,"--n-text-color":we,"--n-icon-color":ye}}),j=v?Ne("switch",L(()=>m.value[0]),C,e):void 0;return{handleClick:f,handleBlur:I,handleFocus:Y,handleKeyup:D,handleKeydown:Q,mergedRailStyle:r,pressed:g,mergedClsPrefix:y,mergedValue:s,checked:i,mergedDisabled:h,cssVars:v?void 0:C,themeClass:j==null?void 0:j.themeClass,onRender:j==null?void 0:j.onRender}},render(){const{mergedClsPrefix:e,mergedDisabled:y,checked:v,mergedRailStyle:l,onRender:a,$slots:m}=this;a==null||a();const{checked:h,unchecked:x,icon:S,"checked-icon":s,"unchecked-icon":i}=m,g=!(oe(S)&&oe(s)&&oe(i));return b("div",{role:"switch","aria-checked":v,class:[`${e}-switch`,this.themeClass,g&&`${e}-switch--icon`,v&&`${e}-switch--active`,y&&`${e}-switch--disabled`,this.round&&`${e}-switch--round`,this.loading&&`${e}-switch--loading`,this.pressed&&`${e}-switch--pressed`,this.rubberBand&&`${e}-switch--rubber-band`],tabindex:this.mergedDisabled?void 0:0,style:this.cssVars,onClick:this.handleClick,onFocus:this.handleFocus,onBlur:this.handleBlur,onKeyup:this.handleKeyup,onKeydown:this.handleKeydown},b("div",{class:`${e}-switch__rail`,"aria-hidden":"true",style:l},T(h,t=>T(x,r=>t||r?b("div",{"aria-hidden":!0,class:`${e}-switch__children-placeholder`},b("div",{class:`${e}-switch__rail-placeholder`},b("div",{class:`${e}-switch__button-placeholder`}),t),b("div",{class:`${e}-switch__rail-placeholder`},b("div",{class:`${e}-switch__button-placeholder`}),r)):null)),b("div",{class:`${e}-switch__button`},T(S,t=>T(s,r=>T(i,d=>b(Fe,null,{default:()=>this.loading?b(Te,{key:"loading",clsPrefix:e,strokeWidth:20}):this.checked&&(r||t)?b("div",{class:`${e}-switch__button-icon`,key:r?"checked-icon":"icon"},r||t):!this.checked&&(d||t)?b("div",{class:`${e}-switch__button-icon`,key:d?"unchecked-icon":"icon"},d||t):null})))),T(h,t=>t&&b("div",{key:"checked",class:`${e}-switch__checked`},t)),T(x,t=>t&&b("div",{key:"unchecked",class:`${e}-switch__unchecked`},t)))))}}),We={class:"flex flex-col gap-2"},Ae=q({__name:"AddServiceDialog",emits:["confirm","cancel"],setup(e,{emit:y}){const v=k([]);G(async()=>{v.value=await ue()});const l=k({}),a=[{label:"项目",value:0},{label:"文件夹",value:1}],m=y;async function h(){await N.request({url:"/service/insert",method:"post",data:l.value}),m("confirm"),window.$message.success("新增成功")}function x(){m("cancel")}return(S,s)=>{const i=de,g=U,t=fe,r=R,d=se;return M(),P(d,{show:!0,class:"w-500px h-500px",preset:"card",title:"新增服务",size:"huge",bordered:!1,onClose:x},{default:w(()=>[X("div",We,[o(i,{placeholder:"环境",autosize:"",value:l.value.environmentName,"onUpdate:value":s[0]||(s[0]=u=>l.value.environmentName=u),style:{"min-width":"10%"},options:v.value},null,8,["value","options"]),o(g,{placeholder:"服务类别",autosize:"",style:{"min-width":"10%"},value:l.value.groupName,"onUpdate:value":s[1]||(s[1]=u=>l.value.groupName=u)},null,8,["value"]),o(g,{placeholder:"服务名",autosize:"",style:{"min-width":"10%"},value:l.value.name,"onUpdate:value":s[2]||(s[2]=u=>l.value.name=u)},null,8,["value"]),o(g,{placeholder:"描述",autosize:"",style:{"min-width":"10%"},value:l.value.description,"onUpdate:value":s[3]||(s[3]=u=>l.value.description=u)},null,8,["value"]),o(i,{placeholder:"发布方式",style:{"min-width":"10%"},value:l.value.deployMode,"onUpdate:value":s[4]||(s[4]=u=>l.value.deployMode=u),options:a},null,8,["value"]),o(g,{placeholder:"发布路径",autosize:"",style:{"min-width":"10%"},value:l.value.projectPath,"onUpdate:value":s[5]||(s[5]=u=>l.value.projectPath=u)},null,8,["value"]),o(t,{value:l.value.isSelfContained,"onUpdate:value":s[6]||(s[6]=u=>l.value.isSelfContained=u)},{checked:w(()=>[_(" 包含运行时")]),unchecked:w(()=>[_(" 不包含运行时")]),_:1},8,["value"]),o(t,{value:l.value.enableHealthCheck,"onUpdate:value":s[7]||(s[7]=u=>l.value.enableHealthCheck=u)},{checked:w(()=>[_(" 开启健康检查")]),unchecked:w(()=>[_(" 关闭健康检查")]),_:1},8,["value"]),o(g,{placeholder:"服务端口",value:l.value.port,"onUpdate:value":s[8]||(s[8]=u=>l.value.port=u)},null,8,["value"]),o(r,{onClick:h},{default:w(()=>[_("确定")]),_:1})])]),_:1})}}}),Ee={class:"flex flex-col gap-2"},Le=q({__name:"UpdateServiceDialog",props:{data:{type:Object}},emits:["cancel","confirm"],setup(e,{emit:y}){const v=e,l=k([]),a=k({});G(async()=>{l.value=await ue(),Me(a.value,v.data)});const m=[{label:"项目",value:0},{label:"文件夹",value:1}],h=y;async function x(){await N.request({url:"/service/update",method:"post",data:a.value}),h("confirm"),window.$message.success("新增成功")}function S(){h("cancel")}return(s,i)=>{const g=de,t=U,r=fe,d=R,u=se;return M(),P(u,{show:!0,class:"w-500px h-500px",preset:"card",title:"编辑服务",size:"huge",bordered:!1,onClose:S},{default:w(()=>[X("div",Ee,[o(g,{placeholder:"环境",autosize:"",value:a.value.environmentName,"onUpdate:value":i[0]||(i[0]=n=>a.value.environmentName=n),style:{"min-width":"10%"},options:l.value},null,8,["value","options"]),o(t,{placeholder:"服务类别",autosize:"",style:{"min-width":"10%"},value:a.value.groupName,"onUpdate:value":i[1]||(i[1]=n=>a.value.groupName=n)},null,8,["value"]),o(t,{placeholder:"服务名",autosize:"",style:{"min-width":"10%"},value:a.value.name,"onUpdate:value":i[2]||(i[2]=n=>a.value.name=n)},null,8,["value"]),o(t,{placeholder:"描述",autosize:"",style:{"min-width":"10%"},value:a.value.description,"onUpdate:value":i[3]||(i[3]=n=>a.value.description=n)},null,8,["value"]),o(g,{placeholder:"发布方式",style:{"min-width":"10%"},value:a.value.deployMode,"onUpdate:value":i[4]||(i[4]=n=>a.value.deployMode=n),options:m},null,8,["value"]),o(t,{placeholder:"发布路径",autosize:"",style:{"min-width":"10%"},value:a.value.projectPath,"onUpdate:value":i[5]||(i[5]=n=>a.value.projectPath=n)},null,8,["value"]),o(r,{value:a.value.isSelfContained,"onUpdate:value":i[6]||(i[6]=n=>a.value.isSelfContained=n)},{checked:w(()=>[_(" 包含运行时")]),unchecked:w(()=>[_(" 不包含运行时")]),_:1},8,["value"]),o(r,{value:a.value.enableHealthCheck,"onUpdate:value":i[7]||(i[7]=n=>a.value.enableHealthCheck=n)},{checked:w(()=>[_(" 开启健康检查")]),unchecked:w(()=>[_(" 关闭健康检查")]),_:1},8,["value"]),o(t,{placeholder:"服务端口",value:a.value.port,"onUpdate:value":i[8]||(i[8]=n=>a.value.port=n)},null,8,["value"]),o(d,{onClick:x},{default:w(()=>[_("确定")]),_:1})])]),_:1})}}}),Ge={class:"flex flex-col gap-2"},Xe=q({__name:"UpdateTargetDialog",props:{data:Object},emits:["confirm","cancel"],setup(e,{emit:y}){const v=e,l=y,a=k({serviceId:v.data.id});let m=k([]);function h(){N.request({url:"/target/query",method:"get",params:{serviceId:v.data.id??0}}).then(t=>{m.value=t})}G(()=>{h()});function x(){l("cancel")}function S(){N.request({url:"/target/insert",method:"post",data:a.value}).then(()=>{h(),window.$message.success("操作成功")})}const s=[{title:"服务器IP",key:"host",render(t,r){return b(U,{value:t.host,onUpdateValue(d){m.value[r].host=d}})}},{title:"服务器端口",key:"port",render(t,r){return b(U,{value:t.port,onUpdateValue(d){m.value[r].port=d}})}},{title:"健康检查url",key:"healthCheckUrl",render(t,r){return b(U,{value:t.healthCheckUrl,onUpdateValue(d){m.value[r].healthCheckUrl=d}})}},{title:"权限认证Key",key:"authKey",render(t,r){return b(U,{value:t.authKey,onUpdateValue(d){m.value[r].authKey=d}})}},{title:"操作",key:"action",render(t){return b("div",{class:"flex gap-2"},[b(R,{type:"primary",onClick:()=>g(t)},()=>"更新"),b(R,{type:"error",onClick:()=>i(t)},()=>"删除")])}}];function i(t){N.request({url:"/target/delete",method:"post",params:{id:t.id}}).then(()=>{h(),window.$message.success("删除成功")})}function g(t){debugger;N.request({url:"/target/update",method:"post",data:t}).then(()=>{window.$message.success("更新成功")})}return(t,r)=>{const d=me,u=se;return M(),P(u,{show:!0,class:"h-90vh w-90vw",preset:"card",title:"发布目标配置",size:"huge",bordered:!1,onClose:x},{default:w(()=>[X("div",Ge,[o(K(U),{placeholder:"服务器IP",value:a.value.host,"onUpdate:value":r[0]||(r[0]=n=>a.value.host=n)},null,8,["value"]),o(K(U),{placeholder:"服务器端口",value:a.value.port,"onUpdate:value":r[1]||(r[1]=n=>a.value.port=n),valueModifiers:{number:!0}},null,8,["value"]),o(K(U),{placeholder:"健康检查url",value:a.value.healthCheckUrl,"onUpdate:value":r[2]||(r[2]=n=>a.value.healthCheckUrl=n)},null,8,["value"]),o(K(U),{placeholder:"权限认证Key",value:a.value.authKey,"onUpdate:value":r[3]||(r[3]=n=>a.value.authKey=n)},null,8,["value"]),o(K(R),{type:"primary",onClick:S},{default:w(()=>[_("新增")]),_:1})]),o(d,{data:K(m),columns:s},null,8,["data"])]),_:1})}}}),Ye={class:"search-box"},Qe=q({__name:"ServiceSettingView",setup(e){const y=k([]);G(()=>{l(),ue().then(n=>{y.value=n})});const v=k({serviceName:"",serviceGroup:"",environment:""});async function l(){m.value=!1,h.value=!1,u.value=!0,d.value=await N.request({url:"/service/query",method:"get",params:v.value}),u.value=!1}function a(){v.value={serviceName:"",serviceGroup:"",environment:""}}const m=k(!1),h=k(!1),x=k(!1);function S(){m.value=!0}const s=[{title:"编号",key:"id"},{title:"环境",key:"environmentName"},{title:"服务类别",key:"groupName"},{title:"服务名",key:"name"},{title:"端口",key:"port"},{title:"描述",key:"description"},{title:"发布路径",key:"projectPath"},{title:"是否包含运行时",key:"isSelfContained",render:n=>n.isSelfContained?"包含":"不包含"},{title:"是否开启健康检查",key:"enableHealthCheck",render:n=>n.enableHealthCheck?"开启":"关闭"},{title:"操作",key:"action",render:n=>o("div",{class:"flex gap-2"},[o(R,{onClick:()=>g(n)},{default:()=>[_("编辑")]}),o(R,{onClick:()=>t(n)},{default:()=>[_("编辑目标")]}),o(R,{type:"warning",onClick:()=>r(n)},{default:()=>[_("删除")]})]),width:"250px"}],i=k({});function g(n){console.log("编辑",n),i.value=n,h.value=!0}function t(n){i.value=n,x.value=!0}async function r(n){await N.request({url:"/service/delete",method:"post",params:{id:n.id}}),window.$message.success("删除成功"),await l()}const d=k([]),u=k(!1);return(n,f)=>{const Y=de,I=U,D=R,Q=me;return M(),P(Pe,null,{default:w(()=>[X("div",Ye,[o(Y,{placeholder:"环境",autosize:"",style:{width:"10%"},value:v.value.environment,"onUpdate:value":f[0]||(f[0]=C=>v.value.environment=C),options:y.value},null,8,["value","options"]),o(I,{placeholder:"服务类别",autosize:"",style:{"min-width":"10%"},value:v.value.serviceGroup,"onUpdate:value":f[1]||(f[1]=C=>v.value.serviceGroup=C)},null,8,["value"]),o(I,{placeholder:"服务名",autosize:"",style:{"min-width":"10%"},value:v.value.serviceName,"onUpdate:value":f[2]||(f[2]=C=>v.value.serviceName=C)},null,8,["value"]),o(D,{type:"primary",onClick:l},{default:w(()=>[_("查询")]),_:1}),o(D,{type:"primary",onClick:S},{default:w(()=>[_("新增")]),_:1}),o(D,{type:"primary",onClick:a},{default:w(()=>[_("重置")]),_:1})]),o(Q,{columns:s,data:d.value,remote:"",class:"h-70vh mt-4","flex-height":"",loading:u.value},null,8,["data","loading"]),m.value?(M(),P(Ae,{key:0,onConfirm:f[3]||(f[3]=()=>l()),onCancel:f[4]||(f[4]=C=>m.value=!m.value)})):re("",!0),h.value?(M(),P(Le,{key:1,visible:h.value,"onUpdate:visible":f[5]||(f[5]=C=>h.value=C),onConfirm:f[6]||(f[6]=()=>l()),onCancel:f[7]||(f[7]=C=>h.value=!h.value),data:i.value,"onUpdate:data":f[8]||(f[8]=C=>i.value=C)},null,8,["visible","data"])):re("",!0),x.value?(M(),P(Xe,{key:2,onCancel:f[9]||(f[9]=C=>x.value=!x.value),data:i.value},null,8,["data"])):re("",!0)]),_:1})}}}),ut=Ke(Qe,[["__scopeId","data-v-e39905cf"]]);export{ut as default};

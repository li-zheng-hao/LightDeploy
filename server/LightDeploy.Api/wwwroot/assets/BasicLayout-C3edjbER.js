import{_ as ao,a as se,b as Oe}from"./SvgIcon.vue_vue_type_script_setup_true_lang-WPPVge_l.js";import{u as xe}from"./menuStore-QzPyEAxe.js";import{d as H,h as m,c as u,a as b,b as w,e as v,u as te,f as j,g as co,p as q,t as oe,i as x,j as re,k as X,r as $,o as $e,l as so,m as uo,n as V,q as vo,s as mo,v as ho,w as fo,x as Te,y as Ee,N as Le,z as Me,A as F,B as W,F as Fe,C as me,D as po,E as de,G as Ce,H as J,I as go,J as bo,K as ke,L as xo,V as Co,M as yo,O as G,P as Q,Q as O,R as A,S as ee,T as zo,U as he,W as ae,X as L,Y as fe,Z as Ne,_ as _o,$ as Io,a0 as Ae,a1 as wo,a2 as So,a3 as Ro,a4 as Po,a5 as To}from"./index-mJUAFJoQ.js";import{f as ue,N as ko}from"./Icon-TeiBA-kU.js";import{C as No,N as je,_ as Ke,c as ve,u as Ao,V as Bo}from"./Dropdown-zTqd8F1V.js";import{u as pe}from"./use-merged-state-24cQO82p.js";import{_ as Ho}from"./_plugin-vue_export-helper-x3n3nnut.js";import{_ as Oo}from"./Scrollbar-z_L0Kp3P.js";import"./Follower-gq7Zi2AD.js";const $o=H({name:"ChevronDownFilled",render(){return m("svg",{viewBox:"0 0 16 16",fill:"none",xmlns:"http://www.w3.org/2000/svg"},m("path",{d:"M3.20041 5.73966C3.48226 5.43613 3.95681 5.41856 4.26034 5.70041L8 9.22652L11.7397 5.70041C12.0432 5.41856 12.5177 5.43613 12.7996 5.73966C13.0815 6.0432 13.0639 6.51775 12.7603 6.7996L8.51034 10.7996C8.22258 11.0668 7.77743 11.0668 7.48967 10.7996L3.23966 6.7996C2.93613 6.51775 2.91856 6.0432 3.20041 5.73966Z",fill:"currentColor"}))}}),Eo=u("breadcrumb",`
 white-space: nowrap;
 cursor: default;
 line-height: var(--n-item-line-height);
`,[b("ul",`
 list-style: none;
 padding: 0;
 margin: 0;
 `),b("a",`
 color: inherit;
 text-decoration: inherit;
 `),u("breadcrumb-item",`
 font-size: var(--n-font-size);
 transition: color .3s var(--n-bezier);
 display: inline-flex;
 align-items: center;
 `,[u("icon",`
 font-size: 18px;
 vertical-align: -.2em;
 transition: color .3s var(--n-bezier);
 color: var(--n-item-text-color);
 `),b("&:not(:last-child)",[w("clickable",[v("link",`
 cursor: pointer;
 `,[b("&:hover",`
 background-color: var(--n-item-color-hover);
 `),b("&:active",`
 background-color: var(--n-item-color-pressed); 
 `)])])]),v("link",`
 padding: 4px;
 border-radius: var(--n-item-border-radius);
 transition:
 background-color .3s var(--n-bezier),
 color .3s var(--n-bezier);
 color: var(--n-item-text-color);
 position: relative;
 `,[b("&:hover",`
 color: var(--n-item-text-color-hover);
 `,[u("icon",`
 color: var(--n-item-text-color-hover);
 `)]),b("&:active",`
 color: var(--n-item-text-color-pressed);
 `,[u("icon",`
 color: var(--n-item-text-color-pressed);
 `)])]),v("separator",`
 margin: 0 8px;
 color: var(--n-separator-color);
 transition: color .3s var(--n-bezier);
 user-select: none;
 -webkit-user-select: none;
 `),b("&:last-child",[v("link",`
 font-weight: var(--n-font-weight-active);
 cursor: unset;
 color: var(--n-item-text-color-active);
 `,[u("icon",`
 color: var(--n-item-text-color-active);
 `)]),v("separator",`
 display: none;
 `)])])]),Ve=X("n-breadcrumb"),Lo=Object.assign(Object.assign({},j.props),{separator:{type:String,default:"/"}}),Mo=H({name:"Breadcrumb",props:Lo,setup(e){const{mergedClsPrefixRef:o,inlineThemeDisabled:t}=te(e),i=j("Breadcrumb","-breadcrumb",Eo,co,e,o);q(Ve,{separatorRef:oe(e,"separator"),mergedClsPrefixRef:o});const a=x(()=>{const{common:{cubicBezierEaseInOut:d},self:{separatorColor:s,itemTextColor:c,itemTextColorHover:p,itemTextColorPressed:R,itemTextColorActive:_,fontSize:h,fontWeightActive:B,itemBorderRadius:T,itemColorHover:I,itemColorPressed:S,itemLineHeight:k}}=i.value;return{"--n-font-size":h,"--n-bezier":d,"--n-item-text-color":c,"--n-item-text-color-hover":p,"--n-item-text-color-pressed":R,"--n-item-text-color-active":_,"--n-separator-color":s,"--n-item-color-hover":I,"--n-item-color-pressed":S,"--n-item-border-radius":T,"--n-font-weight-active":B,"--n-item-line-height":k}}),n=t?re("breadcrumb",void 0,a,e):void 0;return{mergedClsPrefix:o,cssVars:t?void 0:a,themeClass:n==null?void 0:n.themeClass,onRender:n==null?void 0:n.onRender}},render(){var e;return(e=this.onRender)===null||e===void 0||e.call(this),m("nav",{class:[`${this.mergedClsPrefix}-breadcrumb`,this.themeClass],style:this.cssVars,"aria-label":"Breadcrumb"},m("ul",null,this.$slots))}}),Fo=(e=uo?window:null)=>{const o=()=>{const{hash:a,host:n,hostname:d,href:s,origin:c,pathname:p,port:R,protocol:_,search:h}=(e==null?void 0:e.location)||{};return{hash:a,host:n,hostname:d,href:s,origin:c,pathname:p,port:R,protocol:_,search:h}},t=()=>{i.value=o()},i=$(o());return $e(()=>{e&&(e.addEventListener("popstate",t),e.addEventListener("hashchange",t))}),so(()=>{e&&(e.removeEventListener("popstate",t),e.removeEventListener("hashchange",t))}),i},jo={separator:String,href:String,clickable:{type:Boolean,default:!0},onClick:Function},Ko=H({name:"BreadcrumbItem",props:jo,setup(e,{slots:o}){const t=V(Ve,null);if(!t)return()=>null;const{separatorRef:i,mergedClsPrefixRef:a}=t,n=Fo(),d=x(()=>e.href?"a":"span"),s=x(()=>n.value.href===e.href?"location":null);return()=>{const{value:c}=a;return m("li",{class:[`${c}-breadcrumb-item`,e.clickable&&`${c}-breadcrumb-item--clickable`]},m(d.value,{class:`${c}-breadcrumb-item__link`,"aria-current":s.value,href:e.href,onClick:e.onClick},o),m("span",{class:`${c}-breadcrumb-item__separator`,"aria-hidden":"true"},vo(o.separator,()=>{var p;return[(p=e.separator)!==null&&p!==void 0?p:i.value]})))}}}),Vo=e=>{const{baseColor:o,textColor2:t,bodyColor:i,cardColor:a,dividerColor:n,actionColor:d,scrollbarColor:s,scrollbarColorHover:c,invertedColor:p}=e;return{textColor:t,textColorInverted:"#FFF",color:i,colorEmbedded:d,headerColor:a,headerColorInverted:p,footerColor:d,footerColorInverted:p,headerBorderColor:n,headerBorderColorInverted:p,footerBorderColor:n,footerBorderColorInverted:p,siderBorderColor:n,siderBorderColorInverted:p,siderColor:a,siderColorInverted:p,siderToggleButtonBorder:`1px solid ${n}`,siderToggleButtonColor:o,siderToggleButtonIconColor:t,siderToggleButtonIconColorInverted:t,siderToggleBarColor:Te(i,s),siderToggleBarColorHover:Te(i,c),__invertScrollbar:"true"}},Uo=mo({name:"Layout",common:ho,peers:{Scrollbar:fo},self:Vo}),ye=Uo,Ue=X("n-layout-sider"),ze={type:String,default:"static"},Do=u("layout",`
 color: var(--n-text-color);
 background-color: var(--n-color);
 box-sizing: border-box;
 position: relative;
 z-index: auto;
 flex: auto;
 overflow: hidden;
 transition:
 box-shadow .3s var(--n-bezier),
 background-color .3s var(--n-bezier),
 color .3s var(--n-bezier);
`,[u("layout-scroll-container",`
 overflow-x: hidden;
 box-sizing: border-box;
 height: 100%;
 `),w("absolute-positioned",`
 position: absolute;
 left: 0;
 right: 0;
 top: 0;
 bottom: 0;
 `)]),Go={embedded:Boolean,position:ze,nativeScrollbar:{type:Boolean,default:!0},scrollbarProps:Object,onScroll:Function,contentClass:String,contentStyle:{type:[String,Object],default:""},hasSider:Boolean,siderPlacement:{type:String,default:"left"}},De=X("n-layout");function Ge(e){return H({name:e?"LayoutContent":"Layout",props:Object.assign(Object.assign({},j.props),Go),setup(o){const t=$(null),i=$(null),{mergedClsPrefixRef:a,inlineThemeDisabled:n}=te(o),d=j("Layout","-layout",Do,ye,o,a);function s(I,S){if(o.nativeScrollbar){const{value:k}=t;k&&(S===void 0?k.scrollTo(I):k.scrollTo(I,S))}else{const{value:k}=i;k&&k.scrollTo(I,S)}}q(De,o);let c=0,p=0;const R=I=>{var S;const k=I.target;c=k.scrollLeft,p=k.scrollTop,(S=o.onScroll)===null||S===void 0||S.call(o,I)};Ee(()=>{if(o.nativeScrollbar){const I=t.value;I&&(I.scrollTop=p,I.scrollLeft=c)}});const _={display:"flex",flexWrap:"nowrap",width:"100%",flexDirection:"row"},h={scrollTo:s},B=x(()=>{const{common:{cubicBezierEaseInOut:I},self:S}=d.value;return{"--n-bezier":I,"--n-color":o.embedded?S.colorEmbedded:S.color,"--n-text-color":S.textColor}}),T=n?re("layout",x(()=>o.embedded?"e":""),B,o):void 0;return Object.assign({mergedClsPrefix:a,scrollableElRef:t,scrollbarInstRef:i,hasSiderStyle:_,mergedTheme:d,handleNativeElScroll:R,cssVars:n?void 0:B,themeClass:T==null?void 0:T.themeClass,onRender:T==null?void 0:T.onRender},h)},render(){var o;const{mergedClsPrefix:t,hasSider:i}=this;(o=this.onRender)===null||o===void 0||o.call(this);const a=i?this.hasSiderStyle:void 0,n=[this.themeClass,e&&`${t}-layout-content`,`${t}-layout`,`${t}-layout--${this.position}-positioned`];return m("div",{class:n,style:this.cssVars},this.nativeScrollbar?m("div",{ref:"scrollableElRef",class:[`${t}-layout-scroll-container`,this.contentClass],style:[this.contentStyle,a],onScroll:this.handleNativeElScroll},this.$slots):m(Le,Object.assign({},this.scrollbarProps,{onScroll:this.onScroll,ref:"scrollbarInstRef",theme:this.mergedTheme.peers.Scrollbar,themeOverrides:this.mergedTheme.peerOverrides.Scrollbar,contentClass:this.contentClass,contentStyle:[this.contentStyle,a]}),this.$slots))}})}const qo=Ge(!1),Yo=Ge(!0),Wo=u("layout-header",`
 transition:
 color .3s var(--n-bezier),
 background-color .3s var(--n-bezier),
 box-shadow .3s var(--n-bezier),
 border-color .3s var(--n-bezier);
 box-sizing: border-box;
 width: 100%;
 background-color: var(--n-color);
 color: var(--n-text-color);
`,[w("absolute-positioned",`
 position: absolute;
 left: 0;
 right: 0;
 top: 0;
 `),w("bordered",`
 border-bottom: solid 1px var(--n-border-color);
 `)]),Xo={position:ze,inverted:Boolean,bordered:{type:Boolean,default:!1}},Zo=H({name:"LayoutHeader",props:Object.assign(Object.assign({},j.props),Xo),setup(e){const{mergedClsPrefixRef:o,inlineThemeDisabled:t}=te(e),i=j("Layout","-layout-header",Wo,ye,e,o),a=x(()=>{const{common:{cubicBezierEaseInOut:d},self:s}=i.value,c={"--n-bezier":d};return e.inverted?(c["--n-color"]=s.headerColorInverted,c["--n-text-color"]=s.textColorInverted,c["--n-border-color"]=s.headerBorderColorInverted):(c["--n-color"]=s.headerColor,c["--n-text-color"]=s.textColor,c["--n-border-color"]=s.headerBorderColor),c}),n=t?re("layout-header",x(()=>e.inverted?"a":"b"),a,e):void 0;return{mergedClsPrefix:o,cssVars:t?void 0:a,themeClass:n==null?void 0:n.themeClass,onRender:n==null?void 0:n.onRender}},render(){var e;const{mergedClsPrefix:o}=this;return(e=this.onRender)===null||e===void 0||e.call(this),m("div",{class:[`${o}-layout-header`,this.themeClass,this.position&&`${o}-layout-header--${this.position}-positioned`,this.bordered&&`${o}-layout-header--bordered`],style:this.cssVars},this.$slots)}}),Jo=u("layout-sider",`
 flex-shrink: 0;
 box-sizing: border-box;
 position: relative;
 z-index: 1;
 color: var(--n-text-color);
 transition:
 color .3s var(--n-bezier),
 border-color .3s var(--n-bezier),
 min-width .3s var(--n-bezier),
 max-width .3s var(--n-bezier),
 transform .3s var(--n-bezier),
 background-color .3s var(--n-bezier);
 background-color: var(--n-color);
 display: flex;
 justify-content: flex-end;
`,[w("bordered",[v("border",`
 content: "";
 position: absolute;
 top: 0;
 bottom: 0;
 width: 1px;
 background-color: var(--n-border-color);
 transition: background-color .3s var(--n-bezier);
 `)]),v("left-placement",[w("bordered",[v("border",`
 right: 0;
 `)])]),w("right-placement",`
 justify-content: flex-start;
 `,[w("bordered",[v("border",`
 left: 0;
 `)]),w("collapsed",[u("layout-toggle-button",[u("base-icon",`
 transform: rotate(180deg);
 `)]),u("layout-toggle-bar",[b("&:hover",[v("top",{transform:"rotate(-12deg) scale(1.15) translateY(-2px)"}),v("bottom",{transform:"rotate(12deg) scale(1.15) translateY(2px)"})])])]),u("layout-toggle-button",`
 left: 0;
 transform: translateX(-50%) translateY(-50%);
 `,[u("base-icon",`
 transform: rotate(0);
 `)]),u("layout-toggle-bar",`
 left: -28px;
 transform: rotate(180deg);
 `,[b("&:hover",[v("top",{transform:"rotate(12deg) scale(1.15) translateY(-2px)"}),v("bottom",{transform:"rotate(-12deg) scale(1.15) translateY(2px)"})])])]),w("collapsed",[u("layout-toggle-bar",[b("&:hover",[v("top",{transform:"rotate(-12deg) scale(1.15) translateY(-2px)"}),v("bottom",{transform:"rotate(12deg) scale(1.15) translateY(2px)"})])]),u("layout-toggle-button",[u("base-icon",`
 transform: rotate(0);
 `)])]),u("layout-toggle-button",`
 transition:
 color .3s var(--n-bezier),
 right .3s var(--n-bezier),
 left .3s var(--n-bezier),
 border-color .3s var(--n-bezier),
 background-color .3s var(--n-bezier);
 cursor: pointer;
 width: 24px;
 height: 24px;
 position: absolute;
 top: 50%;
 right: 0;
 border-radius: 50%;
 display: flex;
 align-items: center;
 justify-content: center;
 font-size: 18px;
 color: var(--n-toggle-button-icon-color);
 border: var(--n-toggle-button-border);
 background-color: var(--n-toggle-button-color);
 box-shadow: 0 2px 4px 0px rgba(0, 0, 0, .06);
 transform: translateX(50%) translateY(-50%);
 z-index: 1;
 `,[u("base-icon",`
 transition: transform .3s var(--n-bezier);
 transform: rotate(180deg);
 `)]),u("layout-toggle-bar",`
 cursor: pointer;
 height: 72px;
 width: 32px;
 position: absolute;
 top: calc(50% - 36px);
 right: -28px;
 `,[v("top, bottom",`
 position: absolute;
 width: 4px;
 border-radius: 2px;
 height: 38px;
 left: 14px;
 transition: 
 background-color .3s var(--n-bezier),
 transform .3s var(--n-bezier);
 `),v("bottom",`
 position: absolute;
 top: 34px;
 `),b("&:hover",[v("top",{transform:"rotate(12deg) scale(1.15) translateY(-2px)"}),v("bottom",{transform:"rotate(-12deg) scale(1.15) translateY(2px)"})]),v("top, bottom",{backgroundColor:"var(--n-toggle-bar-color)"}),b("&:hover",[v("top, bottom",{backgroundColor:"var(--n-toggle-bar-color-hover)"})])]),v("border",`
 position: absolute;
 top: 0;
 right: 0;
 bottom: 0;
 width: 1px;
 transition: background-color .3s var(--n-bezier);
 `),u("layout-sider-scroll-container",`
 flex-grow: 1;
 flex-shrink: 0;
 box-sizing: border-box;
 height: 100%;
 opacity: 0;
 transition: opacity .3s var(--n-bezier);
 max-width: 100%;
 `),w("show-content",[u("layout-sider-scroll-container",{opacity:1})]),w("absolute-positioned",`
 position: absolute;
 left: 0;
 top: 0;
 bottom: 0;
 `)]),Qo=H({name:"LayoutToggleButton",props:{clsPrefix:{type:String,required:!0},onClick:Function},render(){const{clsPrefix:e}=this;return m("div",{class:`${e}-layout-toggle-button`,onClick:this.onClick},m(Me,{clsPrefix:e},{default:()=>m(No,null)}))}}),et=H({props:{clsPrefix:{type:String,required:!0},onClick:Function},render(){const{clsPrefix:e}=this;return m("div",{onClick:this.onClick,class:`${e}-layout-toggle-bar`},m("div",{class:`${e}-layout-toggle-bar__top`}),m("div",{class:`${e}-layout-toggle-bar__bottom`}))}}),ot={position:ze,bordered:Boolean,collapsedWidth:{type:Number,default:48},width:{type:[Number,String],default:272},contentClass:String,contentStyle:{type:[String,Object],default:""},collapseMode:{type:String,default:"transform"},collapsed:{type:Boolean,default:void 0},defaultCollapsed:Boolean,showCollapsedContent:{type:Boolean,default:!0},showTrigger:{type:[Boolean,String],default:!1},nativeScrollbar:{type:Boolean,default:!0},inverted:Boolean,scrollbarProps:Object,triggerClass:String,triggerStyle:[String,Object],collapsedTriggerClass:String,collapsedTriggerStyle:[String,Object],"onUpdate:collapsed":[Function,Array],onUpdateCollapsed:[Function,Array],onAfterEnter:Function,onAfterLeave:Function,onExpand:[Function,Array],onCollapse:[Function,Array],onScroll:Function},tt=H({name:"LayoutSider",props:Object.assign(Object.assign({},j.props),ot),setup(e){const o=V(De),t=$(null),i=$(null),a=x(()=>ue(c.value?e.collapsedWidth:e.width)),n=x(()=>e.collapseMode!=="transform"?{}:{minWidth:ue(e.width)}),d=x(()=>o?o.siderPlacement:"left"),s=$(e.defaultCollapsed),c=pe(oe(e,"collapsed"),s);function p(N,y){if(e.nativeScrollbar){const{value:z}=t;z&&(y===void 0?z.scrollTo(N):z.scrollTo(N,y))}else{const{value:z}=i;z&&z.scrollTo(N,y)}}function R(){const{"onUpdate:collapsed":N,onUpdateCollapsed:y,onExpand:z,onCollapse:D}=e,{value:K}=c;y&&F(y,!K),N&&F(N,!K),s.value=!K,K?z&&F(z):D&&F(D)}let _=0,h=0;const B=N=>{var y;const z=N.target;_=z.scrollLeft,h=z.scrollTop,(y=e.onScroll)===null||y===void 0||y.call(e,N)};Ee(()=>{if(e.nativeScrollbar){const N=t.value;N&&(N.scrollTop=h,N.scrollLeft=_)}}),q(Ue,{collapsedRef:c,collapseModeRef:oe(e,"collapseMode")});const{mergedClsPrefixRef:T,inlineThemeDisabled:I}=te(e),S=j("Layout","-layout-sider",Jo,ye,e,T);function k(N){var y,z;N.propertyName==="max-width"&&(c.value?(y=e.onAfterLeave)===null||y===void 0||y.call(e):(z=e.onAfterEnter)===null||z===void 0||z.call(e))}const Z={scrollTo:p},U=x(()=>{const{common:{cubicBezierEaseInOut:N},self:y}=S.value,{siderToggleButtonColor:z,siderToggleButtonBorder:D,siderToggleBarColor:K,siderToggleBarColorHover:ce}=y,E={"--n-bezier":N,"--n-toggle-button-color":z,"--n-toggle-button-border":D,"--n-toggle-bar-color":K,"--n-toggle-bar-color-hover":ce};return e.inverted?(E["--n-color"]=y.siderColorInverted,E["--n-text-color"]=y.textColorInverted,E["--n-border-color"]=y.siderBorderColorInverted,E["--n-toggle-button-icon-color"]=y.siderToggleButtonIconColorInverted,E.__invertScrollbar=y.__invertScrollbar):(E["--n-color"]=y.siderColor,E["--n-text-color"]=y.textColor,E["--n-border-color"]=y.siderBorderColor,E["--n-toggle-button-icon-color"]=y.siderToggleButtonIconColor),E}),M=I?re("layout-sider",x(()=>e.inverted?"a":"b"),U,e):void 0;return Object.assign({scrollableElRef:t,scrollbarInstRef:i,mergedClsPrefix:T,mergedTheme:S,styleMaxWidth:a,mergedCollapsed:c,scrollContainerStyle:n,siderPlacement:d,handleNativeElScroll:B,handleTransitionend:k,handleTriggerClick:R,inlineThemeDisabled:I,cssVars:U,themeClass:M==null?void 0:M.themeClass,onRender:M==null?void 0:M.onRender},Z)},render(){var e;const{mergedClsPrefix:o,mergedCollapsed:t,showTrigger:i}=this;return(e=this.onRender)===null||e===void 0||e.call(this),m("aside",{class:[`${o}-layout-sider`,this.themeClass,`${o}-layout-sider--${this.position}-positioned`,`${o}-layout-sider--${this.siderPlacement}-placement`,this.bordered&&`${o}-layout-sider--bordered`,t&&`${o}-layout-sider--collapsed`,(!t||this.showCollapsedContent)&&`${o}-layout-sider--show-content`],onTransitionend:this.handleTransitionend,style:[this.inlineThemeDisabled?void 0:this.cssVars,{maxWidth:this.styleMaxWidth,width:ue(this.width)}]},this.nativeScrollbar?m("div",{class:[`${o}-layout-sider-scroll-container`,this.contentClass],onScroll:this.handleNativeElScroll,style:[this.scrollContainerStyle,{overflow:"auto"},this.contentStyle],ref:"scrollableElRef"},this.$slots):m(Le,Object.assign({},this.scrollbarProps,{onScroll:this.onScroll,ref:"scrollbarInstRef",style:this.scrollContainerStyle,contentStyle:this.contentStyle,contentClass:this.contentClass,theme:this.mergedTheme.peers.Scrollbar,themeOverrides:this.mergedTheme.peerOverrides.Scrollbar,builtinThemeOverrides:this.inverted&&this.cssVars.__invertScrollbar==="true"?{colorHover:"rgba(255, 255, 255, .4)",color:"rgba(255, 255, 255, .3)"}:void 0}),this.$slots),i?i==="bar"?m(et,{clsPrefix:o,class:t?this.collapsedTriggerClass:this.triggerClass,style:t?this.collapsedTriggerStyle:this.triggerStyle,onClick:this.handleTriggerClick}):m(Qo,{clsPrefix:o,class:t?this.collapsedTriggerClass:this.triggerClass,style:t?this.collapsedTriggerStyle:this.triggerStyle,onClick:this.handleTriggerClick}):null,this.bordered?m("div",{class:`${o}-layout-sider__border`}):null)}}),ne=X("n-menu"),_e=X("n-submenu"),Ie=X("n-menu-item-group"),ie=8;function we(e){const o=V(ne),{props:t,mergedCollapsedRef:i}=o,a=V(_e,null),n=V(Ie,null),d=x(()=>t.mode==="horizontal"),s=x(()=>d.value?t.dropdownPlacement:"tmNodes"in e?"right-start":"right"),c=x(()=>{var h;return Math.max((h=t.collapsedIconSize)!==null&&h!==void 0?h:t.iconSize,t.iconSize)}),p=x(()=>{var h;return!d.value&&e.root&&i.value&&(h=t.collapsedIconSize)!==null&&h!==void 0?h:t.iconSize}),R=x(()=>{if(d.value)return;const{collapsedWidth:h,indent:B,rootIndent:T}=t,{root:I,isGroup:S}=e,k=T===void 0?B:T;return I?i.value?h/2-c.value/2:k:n&&typeof n.paddingLeftRef.value=="number"?B/2+n.paddingLeftRef.value:a&&typeof a.paddingLeftRef.value=="number"?(S?B/2:B)+a.paddingLeftRef.value:0}),_=x(()=>{const{collapsedWidth:h,indent:B,rootIndent:T}=t,{value:I}=c,{root:S}=e;return d.value||!S||!i.value?ie:(T===void 0?B:T)+I+ie-(h+I)/2});return{dropdownPlacement:s,activeIconSize:p,maxIconSize:c,paddingLeft:R,iconMarginRight:_,NMenu:o,NSubmenu:a}}const Se={internalKey:{type:[String,Number],required:!0},root:Boolean,isGroup:Boolean,level:{type:Number,required:!0},title:[String,Function],extra:[String,Function]},qe=Object.assign(Object.assign({},Se),{tmNode:{type:Object,required:!0},tmNodes:{type:Array,required:!0}}),rt=H({name:"MenuOptionGroup",props:qe,setup(e){q(_e,null);const o=we(e);q(Ie,{paddingLeftRef:o.paddingLeft});const{mergedClsPrefixRef:t,props:i}=V(ne);return function(){const{value:a}=t,n=o.paddingLeft.value,{nodeProps:d}=i,s=d==null?void 0:d(e.tmNode.rawNode);return m("div",{class:`${a}-menu-item-group`,role:"group"},m("div",Object.assign({},s,{class:[`${a}-menu-item-group-title`,s==null?void 0:s.class],style:[(s==null?void 0:s.style)||"",n!==void 0?`padding-left: ${n}px;`:""]}),W(e.title),e.extra?m(Fe,null," ",W(e.extra)):null),m("div",null,e.tmNodes.map(c=>Re(c,i))))}}}),Ye=H({name:"MenuOptionContent",props:{collapsed:Boolean,disabled:Boolean,title:[String,Function],icon:Function,extra:[String,Function],showArrow:Boolean,childActive:Boolean,hover:Boolean,paddingLeft:Number,selected:Boolean,maxIconSize:{type:Number,required:!0},activeIconSize:{type:Number,required:!0},iconMarginRight:{type:Number,required:!0},clsPrefix:{type:String,required:!0},onClick:Function,tmNode:{type:Object,required:!0},isEllipsisPlaceholder:Boolean},setup(e){const{props:o}=V(ne);return{menuProps:o,style:x(()=>{const{paddingLeft:t}=e;return{paddingLeft:t&&`${t}px`}}),iconStyle:x(()=>{const{maxIconSize:t,activeIconSize:i,iconMarginRight:a}=e;return{width:`${t}px`,height:`${t}px`,fontSize:`${i}px`,marginRight:`${a}px`}})}},render(){const{clsPrefix:e,tmNode:o,menuProps:{renderIcon:t,renderLabel:i,renderExtra:a,expandIcon:n}}=this,d=t?t(o.rawNode):W(this.icon);return m("div",{onClick:s=>{var c;(c=this.onClick)===null||c===void 0||c.call(this,s)},role:"none",class:[`${e}-menu-item-content`,{[`${e}-menu-item-content--selected`]:this.selected,[`${e}-menu-item-content--collapsed`]:this.collapsed,[`${e}-menu-item-content--child-active`]:this.childActive,[`${e}-menu-item-content--disabled`]:this.disabled,[`${e}-menu-item-content--hover`]:this.hover}],style:this.style},d&&m("div",{class:`${e}-menu-item-content__icon`,style:this.iconStyle,role:"none"},[d]),m("div",{class:`${e}-menu-item-content-header`,role:"none"},this.isEllipsisPlaceholder?this.title:i?i(o.rawNode):W(this.title),this.extra||a?m("span",{class:`${e}-menu-item-content-header__extra`}," ",a?a(o.rawNode):W(this.extra)):null),this.showArrow?m(Me,{ariaHidden:!0,class:`${e}-menu-item-content__arrow`,clsPrefix:e},{default:()=>n?n(o.rawNode):m($o,null)}):null)}}),We=Object.assign(Object.assign({},Se),{rawNodes:{type:Array,default:()=>[]},tmNodes:{type:Array,default:()=>[]},tmNode:{type:Object,required:!0},disabled:Boolean,icon:Function,onClick:Function,domId:String,virtualChildActive:{type:Boolean,default:void 0},isEllipsisPlaceholder:Boolean}),ge=H({name:"Submenu",props:We,setup(e){const o=we(e),{NMenu:t,NSubmenu:i}=o,{props:a,mergedCollapsedRef:n,mergedThemeRef:d}=t,s=x(()=>{const{disabled:h}=e;return i!=null&&i.mergedDisabledRef.value||a.disabled?!0:h}),c=$(!1);q(_e,{paddingLeftRef:o.paddingLeft,mergedDisabledRef:s}),q(Ie,null);function p(){const{onClick:h}=e;h&&h()}function R(){s.value||(n.value||t.toggleExpand(e.internalKey),p())}function _(h){c.value=h}return{menuProps:a,mergedTheme:d,doSelect:t.doSelect,inverted:t.invertedRef,isHorizontal:t.isHorizontalRef,mergedClsPrefix:t.mergedClsPrefixRef,maxIconSize:o.maxIconSize,activeIconSize:o.activeIconSize,iconMarginRight:o.iconMarginRight,dropdownPlacement:o.dropdownPlacement,dropdownShow:c,paddingLeft:o.paddingLeft,mergedDisabled:s,mergedValue:t.mergedValueRef,childActive:me(()=>{var h;return(h=e.virtualChildActive)!==null&&h!==void 0?h:t.activePathRef.value.includes(e.internalKey)}),collapsed:x(()=>a.mode==="horizontal"?!1:n.value?!0:!t.mergedExpandedKeysRef.value.includes(e.internalKey)),dropdownEnabled:x(()=>!s.value&&(a.mode==="horizontal"||n.value)),handlePopoverShowChange:_,handleClick:R}},render(){var e;const{mergedClsPrefix:o,menuProps:{renderIcon:t,renderLabel:i}}=this,a=()=>{const{isHorizontal:d,paddingLeft:s,collapsed:c,mergedDisabled:p,maxIconSize:R,activeIconSize:_,title:h,childActive:B,icon:T,handleClick:I,menuProps:{nodeProps:S},dropdownShow:k,iconMarginRight:Z,tmNode:U,mergedClsPrefix:M,isEllipsisPlaceholder:N,extra:y}=this,z=S==null?void 0:S(U.rawNode);return m("div",Object.assign({},z,{class:[`${M}-menu-item`,z==null?void 0:z.class],role:"menuitem"}),m(Ye,{tmNode:U,paddingLeft:s,collapsed:c,disabled:p,iconMarginRight:Z,maxIconSize:R,activeIconSize:_,title:h,extra:y,showArrow:!d,childActive:B,clsPrefix:M,icon:T,hover:k,onClick:I,isEllipsisPlaceholder:N}))},n=()=>m(po,null,{default:()=>{const{tmNodes:d,collapsed:s}=this;return s?null:m("div",{class:`${o}-submenu-children`,role:"menu"},d.map(c=>Re(c,this.menuProps)))}});return this.root?m(je,Object.assign({size:"large",trigger:"hover"},(e=this.menuProps)===null||e===void 0?void 0:e.dropdownProps,{themeOverrides:this.mergedTheme.peerOverrides.Dropdown,theme:this.mergedTheme.peers.Dropdown,builtinThemeOverrides:{fontSizeLarge:"14px",optionIconSizeLarge:"18px"},value:this.mergedValue,disabled:!this.dropdownEnabled,placement:this.dropdownPlacement,keyField:this.menuProps.keyField,labelField:this.menuProps.labelField,childrenField:this.menuProps.childrenField,onUpdateShow:this.handlePopoverShowChange,options:this.rawNodes,onSelect:this.doSelect,inverted:this.inverted,renderIcon:t,renderLabel:i}),{default:()=>m("div",{class:`${o}-submenu`,role:"menuitem","aria-expanded":!this.collapsed,id:this.domId},a(),this.isHorizontal?null:n())}):m("div",{class:`${o}-submenu`,role:"menuitem","aria-expanded":!this.collapsed,id:this.domId},a(),n())}}),Xe=Object.assign(Object.assign({},Se),{tmNode:{type:Object,required:!0},disabled:Boolean,icon:Function,onClick:Function}),nt=H({name:"MenuOption",props:Xe,setup(e){const o=we(e),{NSubmenu:t,NMenu:i}=o,{props:a,mergedClsPrefixRef:n,mergedCollapsedRef:d}=i,s=t?t.mergedDisabledRef:{value:!1},c=x(()=>s.value||e.disabled);function p(_){const{onClick:h}=e;h&&h(_)}function R(_){c.value||(i.doSelect(e.internalKey,e.tmNode.rawNode),p(_))}return{mergedClsPrefix:n,dropdownPlacement:o.dropdownPlacement,paddingLeft:o.paddingLeft,iconMarginRight:o.iconMarginRight,maxIconSize:o.maxIconSize,activeIconSize:o.activeIconSize,mergedTheme:i.mergedThemeRef,menuProps:a,dropdownEnabled:me(()=>e.root&&d.value&&a.mode!=="horizontal"&&!c.value),selected:me(()=>i.mergedValueRef.value===e.internalKey),mergedDisabled:c,handleClick:R}},render(){const{mergedClsPrefix:e,mergedTheme:o,tmNode:t,menuProps:{renderLabel:i,nodeProps:a}}=this,n=a==null?void 0:a(t.rawNode);return m("div",Object.assign({},n,{role:"menuitem",class:[`${e}-menu-item`,n==null?void 0:n.class]}),m(Ke,{theme:o.peers.Tooltip,themeOverrides:o.peerOverrides.Tooltip,trigger:"hover",placement:this.dropdownPlacement,disabled:!this.dropdownEnabled||this.title===void 0,internalExtraClass:["menu-tooltip"]},{default:()=>i?i(t.rawNode):W(this.title),trigger:()=>m(Ye,{tmNode:t,clsPrefix:e,paddingLeft:this.paddingLeft,iconMarginRight:this.iconMarginRight,maxIconSize:this.maxIconSize,activeIconSize:this.activeIconSize,selected:this.selected,title:this.title,extra:this.extra,disabled:this.mergedDisabled,icon:this.icon,onClick:this.handleClick})}))}}),lt=H({name:"MenuDivider",setup(){const e=V(ne),{mergedClsPrefixRef:o,isHorizontalRef:t}=e;return()=>t.value?null:m("div",{class:`${o.value}-menu-divider`})}}),it=Ce(qe),at=Ce(Xe),ct=Ce(We);function be(e){return e.type==="divider"||e.type==="render"}function st(e){return e.type==="divider"}function Re(e,o){const{rawNode:t}=e,{show:i}=t;if(i===!1)return null;if(be(t))return st(t)?m(lt,Object.assign({key:e.key},t.props)):null;const{labelField:a}=o,{key:n,level:d,isGroup:s}=e,c=Object.assign(Object.assign({},t),{title:t.title||t[a],extra:t.titleExtra||t.extra,key:n,internalKey:n,level:d,root:d===0,isGroup:s});return e.children?e.isGroup?m(rt,de(c,it,{tmNode:e,tmNodes:e.children,key:n})):m(ge,de(c,ct,{key:n,rawNodes:t[o.childrenField],tmNodes:e.children,tmNode:e})):m(nt,de(c,at,{key:n,tmNode:e}))}const Be=[b("&::before","background-color: var(--n-item-color-hover);"),v("arrow",`
 color: var(--n-arrow-color-hover);
 `),v("icon",`
 color: var(--n-item-icon-color-hover);
 `),u("menu-item-content-header",`
 color: var(--n-item-text-color-hover);
 `,[b("a",`
 color: var(--n-item-text-color-hover);
 `),v("extra",`
 color: var(--n-item-text-color-hover);
 `)])],He=[v("icon",`
 color: var(--n-item-icon-color-hover-horizontal);
 `),u("menu-item-content-header",`
 color: var(--n-item-text-color-hover-horizontal);
 `,[b("a",`
 color: var(--n-item-text-color-hover-horizontal);
 `),v("extra",`
 color: var(--n-item-text-color-hover-horizontal);
 `)])],dt=b([u("menu",`
 background-color: var(--n-color);
 color: var(--n-item-text-color);
 overflow: hidden;
 transition: background-color .3s var(--n-bezier);
 box-sizing: border-box;
 font-size: var(--n-font-size);
 padding-bottom: 6px;
 `,[w("horizontal",`
 max-width: 100%;
 width: 100%;
 display: flex;
 overflow: hidden;
 padding-bottom: 0;
 `,[u("submenu","margin: 0;"),u("menu-item","margin: 0;"),u("menu-item-content",`
 padding: 0 20px;
 border-bottom: 2px solid #0000;
 `,[b("&::before","display: none;"),w("selected","border-bottom: 2px solid var(--n-border-color-horizontal)")]),u("menu-item-content",[w("selected",[v("icon","color: var(--n-item-icon-color-active-horizontal);"),u("menu-item-content-header",`
 color: var(--n-item-text-color-active-horizontal);
 `,[b("a","color: var(--n-item-text-color-active-horizontal);"),v("extra","color: var(--n-item-text-color-active-horizontal);")])]),w("child-active",`
 border-bottom: 2px solid var(--n-border-color-horizontal);
 `,[u("menu-item-content-header",`
 color: var(--n-item-text-color-child-active-horizontal);
 `,[b("a",`
 color: var(--n-item-text-color-child-active-horizontal);
 `),v("extra",`
 color: var(--n-item-text-color-child-active-horizontal);
 `)]),v("icon",`
 color: var(--n-item-icon-color-child-active-horizontal);
 `)]),J("disabled",[J("selected, child-active",[b("&:focus-within",He)]),w("selected",[Y(null,[v("icon","color: var(--n-item-icon-color-active-hover-horizontal);"),u("menu-item-content-header",`
 color: var(--n-item-text-color-active-hover-horizontal);
 `,[b("a","color: var(--n-item-text-color-active-hover-horizontal);"),v("extra","color: var(--n-item-text-color-active-hover-horizontal);")])])]),w("child-active",[Y(null,[v("icon","color: var(--n-item-icon-color-child-active-hover-horizontal);"),u("menu-item-content-header",`
 color: var(--n-item-text-color-child-active-hover-horizontal);
 `,[b("a","color: var(--n-item-text-color-child-active-hover-horizontal);"),v("extra","color: var(--n-item-text-color-child-active-hover-horizontal);")])])]),Y("border-bottom: 2px solid var(--n-border-color-horizontal);",He)]),u("menu-item-content-header",[b("a","color: var(--n-item-text-color-horizontal);")])])]),J("responsive",[u("menu-item-content-header",`
 overflow: hidden;
 text-overflow: ellipsis;
 `)]),w("collapsed",[u("menu-item-content",[w("selected",[b("&::before",`
 background-color: var(--n-item-color-active-collapsed) !important;
 `)]),u("menu-item-content-header","opacity: 0;"),v("arrow","opacity: 0;"),v("icon","color: var(--n-item-icon-color-collapsed);")])]),u("menu-item",`
 height: var(--n-item-height);
 margin-top: 6px;
 position: relative;
 `),u("menu-item-content",`
 box-sizing: border-box;
 line-height: 1.75;
 height: 100%;
 display: grid;
 grid-template-areas: "icon content arrow";
 grid-template-columns: auto 1fr auto;
 align-items: center;
 cursor: pointer;
 position: relative;
 padding-right: 18px;
 transition:
 background-color .3s var(--n-bezier),
 padding-left .3s var(--n-bezier),
 border-color .3s var(--n-bezier);
 `,[b("> *","z-index: 1;"),b("&::before",`
 z-index: auto;
 content: "";
 background-color: #0000;
 position: absolute;
 left: 8px;
 right: 8px;
 top: 0;
 bottom: 0;
 pointer-events: none;
 border-radius: var(--n-border-radius);
 transition: background-color .3s var(--n-bezier);
 `),w("disabled",`
 opacity: .45;
 cursor: not-allowed;
 `),w("collapsed",[v("arrow","transform: rotate(0);")]),w("selected",[b("&::before","background-color: var(--n-item-color-active);"),v("arrow","color: var(--n-arrow-color-active);"),v("icon","color: var(--n-item-icon-color-active);"),u("menu-item-content-header",`
 color: var(--n-item-text-color-active);
 `,[b("a","color: var(--n-item-text-color-active);"),v("extra","color: var(--n-item-text-color-active);")])]),w("child-active",[u("menu-item-content-header",`
 color: var(--n-item-text-color-child-active);
 `,[b("a",`
 color: var(--n-item-text-color-child-active);
 `),v("extra",`
 color: var(--n-item-text-color-child-active);
 `)]),v("arrow",`
 color: var(--n-arrow-color-child-active);
 `),v("icon",`
 color: var(--n-item-icon-color-child-active);
 `)]),J("disabled",[J("selected, child-active",[b("&:focus-within",Be)]),w("selected",[Y(null,[v("arrow","color: var(--n-arrow-color-active-hover);"),v("icon","color: var(--n-item-icon-color-active-hover);"),u("menu-item-content-header",`
 color: var(--n-item-text-color-active-hover);
 `,[b("a","color: var(--n-item-text-color-active-hover);"),v("extra","color: var(--n-item-text-color-active-hover);")])])]),w("child-active",[Y(null,[v("arrow","color: var(--n-arrow-color-child-active-hover);"),v("icon","color: var(--n-item-icon-color-child-active-hover);"),u("menu-item-content-header",`
 color: var(--n-item-text-color-child-active-hover);
 `,[b("a","color: var(--n-item-text-color-child-active-hover);"),v("extra","color: var(--n-item-text-color-child-active-hover);")])])]),w("selected",[Y(null,[b("&::before","background-color: var(--n-item-color-active-hover);")])]),Y(null,Be)]),v("icon",`
 grid-area: icon;
 color: var(--n-item-icon-color);
 transition:
 color .3s var(--n-bezier),
 font-size .3s var(--n-bezier),
 margin-right .3s var(--n-bezier);
 box-sizing: content-box;
 display: inline-flex;
 align-items: center;
 justify-content: center;
 `),v("arrow",`
 grid-area: arrow;
 font-size: 16px;
 color: var(--n-arrow-color);
 transform: rotate(180deg);
 opacity: 1;
 transition:
 color .3s var(--n-bezier),
 transform 0.2s var(--n-bezier),
 opacity 0.2s var(--n-bezier);
 `),u("menu-item-content-header",`
 grid-area: content;
 transition:
 color .3s var(--n-bezier),
 opacity .3s var(--n-bezier);
 opacity: 1;
 white-space: nowrap;
 color: var(--n-item-text-color);
 `,[b("a",`
 outline: none;
 text-decoration: none;
 transition: color .3s var(--n-bezier);
 color: var(--n-item-text-color);
 `,[b("&::before",`
 content: "";
 position: absolute;
 left: 0;
 right: 0;
 top: 0;
 bottom: 0;
 `)]),v("extra",`
 font-size: .93em;
 color: var(--n-group-text-color);
 transition: color .3s var(--n-bezier);
 `)])]),u("submenu",`
 cursor: pointer;
 position: relative;
 margin-top: 6px;
 `,[u("menu-item-content",`
 height: var(--n-item-height);
 `),u("submenu-children",`
 overflow: hidden;
 padding: 0;
 `,[go({duration:".2s"})])]),u("menu-item-group",[u("menu-item-group-title",`
 margin-top: 6px;
 color: var(--n-group-text-color);
 cursor: default;
 font-size: .93em;
 height: 36px;
 display: flex;
 align-items: center;
 transition:
 padding-left .3s var(--n-bezier),
 color .3s var(--n-bezier);
 `)])]),u("menu-tooltip",[b("a",`
 color: inherit;
 text-decoration: none;
 `)]),u("menu-divider",`
 transition: background-color .3s var(--n-bezier);
 background-color: var(--n-divider-color);
 height: 1px;
 margin: 6px 18px;
 `)]);function Y(e,o){return[w("hover",e,o),b("&:hover",e,o)]}const ut=Object.assign(Object.assign({},j.props),{options:{type:Array,default:()=>[]},collapsed:{type:Boolean,default:void 0},collapsedWidth:{type:Number,default:48},iconSize:{type:Number,default:20},collapsedIconSize:{type:Number,default:24},rootIndent:Number,indent:{type:Number,default:32},labelField:{type:String,default:"label"},keyField:{type:String,default:"key"},childrenField:{type:String,default:"children"},disabledField:{type:String,default:"disabled"},defaultExpandAll:Boolean,defaultExpandedKeys:Array,expandedKeys:Array,value:[String,Number],defaultValue:{type:[String,Number],default:null},mode:{type:String,default:"vertical"},watchProps:{type:Array,default:void 0},disabled:Boolean,show:{type:Boolean,default:!0},inverted:Boolean,"onUpdate:expandedKeys":[Function,Array],onUpdateExpandedKeys:[Function,Array],onUpdateValue:[Function,Array],"onUpdate:value":[Function,Array],expandIcon:Function,renderIcon:Function,renderLabel:Function,renderExtra:Function,dropdownProps:Object,accordion:Boolean,nodeProps:Function,dropdownPlacement:{type:String,default:"bottom"},responsive:Boolean,items:Array,onOpenNamesChange:[Function,Array],onSelect:[Function,Array],onExpandedNamesChange:[Function,Array],expandedNames:Array,defaultExpandedNames:Array}),vt=H({name:"Menu",props:ut,setup(e){const{mergedClsPrefixRef:o,inlineThemeDisabled:t}=te(e),i=j("Menu","-menu",dt,bo,e,o),a=V(Ue,null),n=x(()=>{var f;const{collapsed:C}=e;if(C!==void 0)return C;if(a){const{collapseModeRef:r,collapsedRef:g}=a;if(r.value==="width")return(f=g.value)!==null&&f!==void 0?f:!1}return!1}),d=x(()=>{const{keyField:f,childrenField:C,disabledField:r}=e;return ve(e.items||e.options,{getIgnored(g){return be(g)},getChildren(g){return g[C]},getDisabled(g){return g[r]},getKey(g){var P;return(P=g[f])!==null&&P!==void 0?P:g.name}})}),s=x(()=>new Set(d.value.treeNodes.map(f=>f.key))),{watchProps:c}=e,p=$(null);c!=null&&c.includes("defaultValue")?ke(()=>{p.value=e.defaultValue}):p.value=e.defaultValue;const R=oe(e,"value"),_=pe(R,p),h=$([]),B=()=>{h.value=e.defaultExpandAll?d.value.getNonLeafKeys():e.defaultExpandedNames||e.defaultExpandedKeys||d.value.getPath(_.value,{includeSelf:!1}).keyPath};c!=null&&c.includes("defaultExpandedKeys")?ke(B):B();const T=Ao(e,["expandedNames","expandedKeys"]),I=pe(T,h),S=x(()=>d.value.treeNodes),k=x(()=>d.value.getPath(_.value).keyPath);q(ne,{props:e,mergedCollapsedRef:n,mergedThemeRef:i,mergedValueRef:_,mergedExpandedKeysRef:I,activePathRef:k,mergedClsPrefixRef:o,isHorizontalRef:x(()=>e.mode==="horizontal"),invertedRef:oe(e,"inverted"),doSelect:Z,toggleExpand:M});function Z(f,C){const{"onUpdate:value":r,onUpdateValue:g,onSelect:P}=e;g&&F(g,f,C),r&&F(r,f,C),P&&F(P,f,C),p.value=f}function U(f){const{"onUpdate:expandedKeys":C,onUpdateExpandedKeys:r,onExpandedNamesChange:g,onOpenNamesChange:P}=e;C&&F(C,f),r&&F(r,f),g&&F(g,f),P&&F(P,f),h.value=f}function M(f){const C=Array.from(I.value),r=C.findIndex(g=>g===f);if(~r)C.splice(r,1);else{if(e.accordion&&s.value.has(f)){const g=C.findIndex(P=>s.value.has(P));g>-1&&C.splice(g,1)}C.push(f)}U(C)}const N=f=>{const C=d.value.getPath(f??_.value,{includeSelf:!1}).keyPath;if(!C.length)return;const r=Array.from(I.value),g=new Set([...r,...C]);e.accordion&&s.value.forEach(P=>{g.has(P)&&!C.includes(P)&&g.delete(P)}),U(Array.from(g))},y=x(()=>{const{inverted:f}=e,{common:{cubicBezierEaseInOut:C},self:r}=i.value,{borderRadius:g,borderColorHorizontal:P,fontSize:no,itemHeight:lo,dividerColor:io}=r,l={"--n-divider-color":io,"--n-bezier":C,"--n-font-size":no,"--n-border-color-horizontal":P,"--n-border-radius":g,"--n-item-height":lo};return f?(l["--n-group-text-color"]=r.groupTextColorInverted,l["--n-color"]=r.colorInverted,l["--n-item-text-color"]=r.itemTextColorInverted,l["--n-item-text-color-hover"]=r.itemTextColorHoverInverted,l["--n-item-text-color-active"]=r.itemTextColorActiveInverted,l["--n-item-text-color-child-active"]=r.itemTextColorChildActiveInverted,l["--n-item-text-color-child-active-hover"]=r.itemTextColorChildActiveInverted,l["--n-item-text-color-active-hover"]=r.itemTextColorActiveHoverInverted,l["--n-item-icon-color"]=r.itemIconColorInverted,l["--n-item-icon-color-hover"]=r.itemIconColorHoverInverted,l["--n-item-icon-color-active"]=r.itemIconColorActiveInverted,l["--n-item-icon-color-active-hover"]=r.itemIconColorActiveHoverInverted,l["--n-item-icon-color-child-active"]=r.itemIconColorChildActiveInverted,l["--n-item-icon-color-child-active-hover"]=r.itemIconColorChildActiveHoverInverted,l["--n-item-icon-color-collapsed"]=r.itemIconColorCollapsedInverted,l["--n-item-text-color-horizontal"]=r.itemTextColorHorizontalInverted,l["--n-item-text-color-hover-horizontal"]=r.itemTextColorHoverHorizontalInverted,l["--n-item-text-color-active-horizontal"]=r.itemTextColorActiveHorizontalInverted,l["--n-item-text-color-child-active-horizontal"]=r.itemTextColorChildActiveHorizontalInverted,l["--n-item-text-color-child-active-hover-horizontal"]=r.itemTextColorChildActiveHoverHorizontalInverted,l["--n-item-text-color-active-hover-horizontal"]=r.itemTextColorActiveHoverHorizontalInverted,l["--n-item-icon-color-horizontal"]=r.itemIconColorHorizontalInverted,l["--n-item-icon-color-hover-horizontal"]=r.itemIconColorHoverHorizontalInverted,l["--n-item-icon-color-active-horizontal"]=r.itemIconColorActiveHorizontalInverted,l["--n-item-icon-color-active-hover-horizontal"]=r.itemIconColorActiveHoverHorizontalInverted,l["--n-item-icon-color-child-active-horizontal"]=r.itemIconColorChildActiveHorizontalInverted,l["--n-item-icon-color-child-active-hover-horizontal"]=r.itemIconColorChildActiveHoverHorizontalInverted,l["--n-arrow-color"]=r.arrowColorInverted,l["--n-arrow-color-hover"]=r.arrowColorHoverInverted,l["--n-arrow-color-active"]=r.arrowColorActiveInverted,l["--n-arrow-color-active-hover"]=r.arrowColorActiveHoverInverted,l["--n-arrow-color-child-active"]=r.arrowColorChildActiveInverted,l["--n-arrow-color-child-active-hover"]=r.arrowColorChildActiveHoverInverted,l["--n-item-color-hover"]=r.itemColorHoverInverted,l["--n-item-color-active"]=r.itemColorActiveInverted,l["--n-item-color-active-hover"]=r.itemColorActiveHoverInverted,l["--n-item-color-active-collapsed"]=r.itemColorActiveCollapsedInverted):(l["--n-group-text-color"]=r.groupTextColor,l["--n-color"]=r.color,l["--n-item-text-color"]=r.itemTextColor,l["--n-item-text-color-hover"]=r.itemTextColorHover,l["--n-item-text-color-active"]=r.itemTextColorActive,l["--n-item-text-color-child-active"]=r.itemTextColorChildActive,l["--n-item-text-color-child-active-hover"]=r.itemTextColorChildActiveHover,l["--n-item-text-color-active-hover"]=r.itemTextColorActiveHover,l["--n-item-icon-color"]=r.itemIconColor,l["--n-item-icon-color-hover"]=r.itemIconColorHover,l["--n-item-icon-color-active"]=r.itemIconColorActive,l["--n-item-icon-color-active-hover"]=r.itemIconColorActiveHover,l["--n-item-icon-color-child-active"]=r.itemIconColorChildActive,l["--n-item-icon-color-child-active-hover"]=r.itemIconColorChildActiveHover,l["--n-item-icon-color-collapsed"]=r.itemIconColorCollapsed,l["--n-item-text-color-horizontal"]=r.itemTextColorHorizontal,l["--n-item-text-color-hover-horizontal"]=r.itemTextColorHoverHorizontal,l["--n-item-text-color-active-horizontal"]=r.itemTextColorActiveHorizontal,l["--n-item-text-color-child-active-horizontal"]=r.itemTextColorChildActiveHorizontal,l["--n-item-text-color-child-active-hover-horizontal"]=r.itemTextColorChildActiveHoverHorizontal,l["--n-item-text-color-active-hover-horizontal"]=r.itemTextColorActiveHoverHorizontal,l["--n-item-icon-color-horizontal"]=r.itemIconColorHorizontal,l["--n-item-icon-color-hover-horizontal"]=r.itemIconColorHoverHorizontal,l["--n-item-icon-color-active-horizontal"]=r.itemIconColorActiveHorizontal,l["--n-item-icon-color-active-hover-horizontal"]=r.itemIconColorActiveHoverHorizontal,l["--n-item-icon-color-child-active-horizontal"]=r.itemIconColorChildActiveHorizontal,l["--n-item-icon-color-child-active-hover-horizontal"]=r.itemIconColorChildActiveHoverHorizontal,l["--n-arrow-color"]=r.arrowColor,l["--n-arrow-color-hover"]=r.arrowColorHover,l["--n-arrow-color-active"]=r.arrowColorActive,l["--n-arrow-color-active-hover"]=r.arrowColorActiveHover,l["--n-arrow-color-child-active"]=r.arrowColorChildActive,l["--n-arrow-color-child-active-hover"]=r.arrowColorChildActiveHover,l["--n-item-color-hover"]=r.itemColorHover,l["--n-item-color-active"]=r.itemColorActive,l["--n-item-color-active-hover"]=r.itemColorActiveHover,l["--n-item-color-active-collapsed"]=r.itemColorActiveCollapsed),l}),z=t?re("menu",x(()=>e.inverted?"a":"b"),y,e):void 0,D=xo(),K=$(null),ce=$(null);let E=!0;const Pe=()=>{var f;E?E=!1:(f=K.value)===null||f===void 0||f.sync({showAllItemsBeforeCalculate:!0})};function Ze(){return document.getElementById(D)}const le=$(-1);function Je(f){le.value=e.options.length-f}function Qe(f){f||(le.value=-1)}const eo=x(()=>{const f=le.value;return{children:f===-1?[]:e.options.slice(f)}}),oo=x(()=>{const{childrenField:f,disabledField:C,keyField:r}=e;return ve([eo.value],{getIgnored(g){return be(g)},getChildren(g){return g[f]},getDisabled(g){return g[C]},getKey(g){var P;return(P=g[r])!==null&&P!==void 0?P:g.name}})}),to=x(()=>ve([{}]).treeNodes[0]);function ro(){var f;if(le.value===-1)return m(ge,{root:!0,level:0,key:"__ellpisisGroupPlaceholder__",internalKey:"__ellpisisGroupPlaceholder__",title:"···",tmNode:to.value,domId:D,isEllipsisPlaceholder:!0});const C=oo.value.treeNodes[0],r=k.value,g=!!(!((f=C.children)===null||f===void 0)&&f.some(P=>r.includes(P.key)));return m(ge,{level:0,root:!0,key:"__ellpisisGroup__",internalKey:"__ellpisisGroup__",title:"···",virtualChildActive:g,tmNode:C,domId:D,rawNodes:C.rawNode.children||[],tmNodes:C.children||[],isEllipsisPlaceholder:!0})}return{mergedClsPrefix:o,controlledExpandedKeys:T,uncontrolledExpanededKeys:h,mergedExpandedKeys:I,uncontrolledValue:p,mergedValue:_,activePath:k,tmNodes:S,mergedTheme:i,mergedCollapsed:n,cssVars:t?void 0:y,themeClass:z==null?void 0:z.themeClass,overflowRef:K,counterRef:ce,updateCounter:()=>{},onResize:Pe,onUpdateOverflow:Qe,onUpdateCount:Je,renderCounter:ro,getCounter:Ze,onRender:z==null?void 0:z.onRender,showOption:N,deriveResponsiveState:Pe}},render(){const{mergedClsPrefix:e,mode:o,themeClass:t,onRender:i}=this;i==null||i();const a=()=>this.tmNodes.map(c=>Re(c,this.$props)),d=o==="horizontal"&&this.responsive,s=()=>m("div",{role:o==="horizontal"?"menubar":"menu",class:[`${e}-menu`,t,`${e}-menu--${o}`,d&&`${e}-menu--responsive`,this.mergedCollapsed&&`${e}-menu--collapsed`],style:this.cssVars},d?m(Bo,{ref:"overflowRef",onUpdateOverflow:this.onUpdateOverflow,getCounter:this.getCounter,onUpdateCount:this.onUpdateCount,updateCounter:this.updateCounter,style:{width:"100%",display:"flex",overflow:"hidden"}},{default:a,counter:this.renderCounter}):a());return d?m(Co,{onResize:this.onResize},{default:s}):s()}});function mt(e,o=300){let t=-1;return function(...i){t>-1&&clearTimeout(t),t=window.setTimeout(()=>{e.bind(this)(...i),t=-1},o)}}function ht(e,o){return(e==null?0:e.length)?e.findIndex((i,a)=>o(i,a,e)):-1}function ft(e,o=300){let t=!1;return function(...i){t||(t=!0,setTimeout(()=>t=!1,o),e.bind(this)(...i))}}const pt={isEmpty:yo,findIndex:ht,debounce:mt,throttle:ft},gt=H({__name:"LeftMenu",setup(e){const o=xe(),t=$({collapsed:!1,menuOptions:[]});function i(n){var d,s;return((d=n.children)==null?void 0:d.length)??0>0?()=>A("div",null,[ee(" "),n.label]):(s=n.routePath)!=null&&s.startsWith("http")?()=>A("a",{href:n.routePath,target:"_blank"},[n.label]):()=>A(zo,{to:n.routePath??""},{default:()=>[n.label]})}function a(n,d){n&&n.forEach((s,c)=>{const p={label:i(s),key:s.key,icon:()=>s.icon?A(ko,null,{default:()=>[A(ao,{name:s.icon},null)]}):null,children:void 0};pt.isEmpty(s.children)||(p.children=[],a(s.children,p.children)),d.push(p)})}return $e(()=>{const n=o.refreshAllMenuInfo(),d=[];a(n,d),t.value.menuOptions=d}),(n,d)=>{const s=vt;return G(),Q(s,{inverted:!0,value:O(o).currentBreadcrumbInfo.routePath,"onUpdate:value":d[0]||(d[0]=c=>O(o).currentBreadcrumbInfo.routePath=c),collapsed:O(o).menuCollapsed,"collapsed-width":64,options:t.value.menuOptions,accordion:!0,indent:20,"watch-props":["defaultExpandedKeys"]},null,8,["value","collapsed","options"])}}}),bt=e=>(wo("data-v-ba82c3e7"),e=e(),So(),e),xt={class:"layout-header"},Ct={class:"layout-header-left flex gap-4 pl-4 pr-4 flex-items-center"},yt={class:"layout-header-right flex gap-4 pl-4 pr-4 flex-items-center flex-shrink-0"},zt=bt(()=>ae("div",null,"李正浩",-1)),_t=H({__name:"TopHeader",setup(e){const o=xe(),t=$([{label:"个人中心",key:"个人中心"},{label:"个人设置",key:"个人设置"},{label:"退出登录",key:"退出登录"}]),i=(a,n)=>{const d=Io();switch(a){case"个人设置":Ae.push("/usersetting");break;case"退出登录":d.logOut(),Ae.push("/login");break}};return(a,n)=>{const d=Ke,s=Ko,c=Mo,p=Oe;return G(),he("div",xt,[ae("div",Ct,[A(d,{trigger:"hover"},{trigger:L(()=>[O(o).menuCollapsed?fe("",!0):(G(),Q(se,{key:0,name:"MenuFoldOutlined",class:"cursor-pointer",onClick:n[0]||(n[0]=R=>O(o).menuCollapsed=!O(o).menuCollapsed)})),O(o).menuCollapsed?(G(),Q(se,{key:1,name:"MenuUnfoldOutlined",class:"cursor-pointer",onClick:n[1]||(n[1]=R=>O(o).menuCollapsed=!O(o).menuCollapsed)})):fe("",!0)]),default:L(()=>[ee(" "+Ne(O(o).menuCollapsed?"展开":"折叠")+"菜单 ",1)]),_:1}),A(c,null,{default:L(()=>{var R;return[(G(!0),he(Fe,null,_o((R=O(o).currentBreadcrumbInfo)==null?void 0:R.paths,(_,h)=>(G(),Q(s,{key:h},{default:L(()=>[ee(Ne(_),1)]),_:2},1024))),128))]}),_:1})]),ae("div",yt,[A(p,{style:{color:"white",backgroundColor:"#2d8cf0"}},{default:L(()=>[ee(" LZH ")]),_:1}),zt,A(O(je),{placement:"bottom-start",trigger:"click",options:t.value,onSelect:i},{default:L(()=>[A(se,{name:"SettingOutlined",class:"cursor-pointer"})]),_:1},8,["options"])])])}}}),It=Ho(_t,[["__scopeId","data-v-ba82c3e7"]]),wt={class:"flex-items-center justify-center flex h-20 gap-2"},St={key:0},$t=H({__name:"BasicLayout",setup(e){let o=xe();const t=Po();return o.refreshAllMenuInfo(),o.refreshCurrentBreadcrumbInfo(),Ro(()=>t.fullPath,()=>{o.refreshCurrentBreadcrumbInfo()}),(i,a)=>{const n=Oe,d=tt,s=Zo,c=To("router-view"),p=Oo,R=Yo,_=qo;return G(),Q(_,{"has-sider":"",class:"w-full h-full bg-#f5f7f9"},{default:L(()=>[A(d,{inverted:!0,bordered:"","collapse-mode":"width","collapsed-width":64,width:200,collapsed:O(o).menuCollapsed,"onUpdate:collapsed":a[0]||(a[0]=h=>O(o).menuCollapsed=h),"native-scrollbar":!1},{default:L(()=>[ae("div",wt,[A(n,{style:{color:"white",backgroundColor:"#2d8cf0"}},{default:L(()=>[ee(" LZH ")]),_:1}),O(o).menuCollapsed?fe("",!0):(G(),he("div",St,"服务发布工具"))]),A(gt)]),_:1},8,["collapsed"]),A(_,{class:"h-full box-border flex w-full bg-#f5f7f9","content-class":"w-full flex flex-col"},{default:L(()=>[A(s,{class:"bg-#f5f7f9"},{default:L(()=>[A(It)]),_:1}),A(R,{"content-style":"box-border",class:"h-full box-border bg-#f5f7f9 p-2"},{default:L(()=>[A(p,null,{default:L(()=>[A(c)]),_:1})]),_:1})]),_:1})]),_:1})}}});export{$t as default};

import{bu as K,bZ as Z,by as X,b_ as be,v as Ce,b$ as fe,ao as t,c as G,b as C,e as x,H as $,a as P,d as Y,r as ve,u as q,f as S,p as pe,t as me,b0 as ke,i as I,ae as u,b2 as ye,j as J,ak as V,ag as A,h as y,c0 as xe,k as Pe,A as Ie,c1 as ze,ah as $e,bf as Se}from"./index-mJUAFJoQ.js";const Re=/^(\d|\.)+$/,U=/(\d|\.)+/;function Be(e,{c:r=1,offset:o=0,attachPx:a=!0}={}){if(typeof e=="number"){const n=(e+o)*r;return n===0?"0":`${n}px`}else if(typeof e=="string")if(Re.test(e)){const n=(Number(e)+o)*r;return a?n===0?"0":`${n}px`:`${n}`}else{const n=U.exec(e);return n?e.replace(U,String((Number(n[0])+o)*r)):e}return e}var He=/\.|\[(?:[^[\]]*|(["'])(?:(?!\1)[^\\]|\\.)*?\1)\]/,_e=/^\w*$/;function Ee(e,r){if(K(e))return!1;var o=typeof e;return o=="number"||o=="symbol"||o=="boolean"||e==null||Z(e)?!0:_e.test(e)||!He.test(e)||r!=null&&e in Object(r)}var Te="Expected a function";function j(e,r){if(typeof e!="function"||r!=null&&typeof r!="function")throw new TypeError(Te);var o=function(){var a=arguments,n=r?r.apply(this,a):a[0],c=o.cache;if(c.has(n))return c.get(n);var l=e.apply(this,a);return o.cache=c.set(n,l)||c,l};return o.cache=new(j.Cache||X),o}j.Cache=X;var Me=500;function we(e){var r=j(e,function(a){return o.size===Me&&o.clear(),a}),o=r.cache;return r}var Oe=/[^.[\]]+|\[(?:(-?\d+(?:\.\d+)?)|(["'])((?:(?!\2)[^\\]|\\.)*?)\2)\]|(?=(?:\.|\[\])(?:\.|\[\]|$))/g,Ne=/\\(\\)?/g,We=we(function(e){var r=[];return e.charCodeAt(0)===46&&r.push(""),e.replace(Oe,function(o,a,n,c){r.push(n?c.replace(Ne,"$1"):a||o)}),r});function Fe(e,r){return K(e)?e:Ee(e,r)?[e]:We(be(e))}var je=1/0;function De(e){if(typeof e=="string"||Z(e))return e;var r=e+"";return r=="0"&&1/e==-je?"-0":r}function Le(e,r){r=Fe(r,e);for(var o=0,a=r.length;e!=null&&o<a;)e=e[De(r[o++])];return o&&o==a?e:void 0}function Qe(e,r,o){var a=e==null?void 0:Le(e,r);return a===void 0?o:a}const Ve=e=>{const{textColor2:r,primaryColorHover:o,primaryColorPressed:a,primaryColor:n,infoColor:c,successColor:l,warningColor:i,errorColor:h,baseColor:v,borderColor:p,opacityDisabled:g,tagColor:m,closeIconColor:s,closeIconColorHover:d,closeIconColorPressed:k,borderRadiusSmall:b,fontSizeMini:f,fontSizeTiny:R,fontSizeSmall:B,fontSizeMedium:H,heightMini:_,heightTiny:E,heightSmall:T,heightMedium:M,closeColorHover:w,closeColorPressed:O,buttonColor2Hover:N,buttonColor2Pressed:W,fontWeightStrong:F}=e;return Object.assign(Object.assign({},fe),{closeBorderRadius:b,heightTiny:_,heightSmall:E,heightMedium:T,heightLarge:M,borderRadius:b,opacityDisabled:g,fontSizeTiny:f,fontSizeSmall:R,fontSizeMedium:B,fontSizeLarge:H,fontWeightStrong:F,textColorCheckable:r,textColorHoverCheckable:r,textColorPressedCheckable:r,textColorChecked:v,colorCheckable:"#0000",colorHoverCheckable:N,colorPressedCheckable:W,colorChecked:n,colorCheckedHover:o,colorCheckedPressed:a,border:`1px solid ${p}`,textColor:r,color:m,colorBordered:"rgb(250, 250, 252)",closeIconColor:s,closeIconColorHover:d,closeIconColorPressed:k,closeColorHover:w,closeColorPressed:O,borderPrimary:`1px solid ${t(n,{alpha:.3})}`,textColorPrimary:n,colorPrimary:t(n,{alpha:.12}),colorBorderedPrimary:t(n,{alpha:.1}),closeIconColorPrimary:n,closeIconColorHoverPrimary:n,closeIconColorPressedPrimary:n,closeColorHoverPrimary:t(n,{alpha:.12}),closeColorPressedPrimary:t(n,{alpha:.18}),borderInfo:`1px solid ${t(c,{alpha:.3})}`,textColorInfo:c,colorInfo:t(c,{alpha:.12}),colorBorderedInfo:t(c,{alpha:.1}),closeIconColorInfo:c,closeIconColorHoverInfo:c,closeIconColorPressedInfo:c,closeColorHoverInfo:t(c,{alpha:.12}),closeColorPressedInfo:t(c,{alpha:.18}),borderSuccess:`1px solid ${t(l,{alpha:.3})}`,textColorSuccess:l,colorSuccess:t(l,{alpha:.12}),colorBorderedSuccess:t(l,{alpha:.1}),closeIconColorSuccess:l,closeIconColorHoverSuccess:l,closeIconColorPressedSuccess:l,closeColorHoverSuccess:t(l,{alpha:.12}),closeColorPressedSuccess:t(l,{alpha:.18}),borderWarning:`1px solid ${t(i,{alpha:.35})}`,textColorWarning:i,colorWarning:t(i,{alpha:.15}),colorBorderedWarning:t(i,{alpha:.12}),closeIconColorWarning:i,closeIconColorHoverWarning:i,closeIconColorPressedWarning:i,closeColorHoverWarning:t(i,{alpha:.12}),closeColorPressedWarning:t(i,{alpha:.18}),borderError:`1px solid ${t(h,{alpha:.23})}`,textColorError:h,colorError:t(h,{alpha:.1}),colorBorderedError:t(h,{alpha:.08}),closeIconColorError:h,closeIconColorHoverError:h,closeIconColorPressedError:h,closeColorHoverError:t(h,{alpha:.12}),closeColorPressedError:t(h,{alpha:.18})})},Ae={name:"Tag",common:Ce,self:Ve},Ue=Ae,Ke={color:Object,type:{type:String,default:"default"},round:Boolean,size:{type:String,default:"medium"},closable:Boolean,disabled:{type:Boolean,default:void 0}},Ze=G("tag",`
 --n-close-margin: var(--n-close-margin-top) var(--n-close-margin-right) var(--n-close-margin-bottom) var(--n-close-margin-left);
 white-space: nowrap;
 position: relative;
 box-sizing: border-box;
 cursor: default;
 display: inline-flex;
 align-items: center;
 flex-wrap: nowrap;
 padding: var(--n-padding);
 border-radius: var(--n-border-radius);
 color: var(--n-text-color);
 background-color: var(--n-color);
 transition: 
 border-color .3s var(--n-bezier),
 background-color .3s var(--n-bezier),
 color .3s var(--n-bezier),
 box-shadow .3s var(--n-bezier),
 opacity .3s var(--n-bezier);
 line-height: 1;
 height: var(--n-height);
 font-size: var(--n-font-size);
`,[C("strong",`
 font-weight: var(--n-font-weight-strong);
 `),x("border",`
 pointer-events: none;
 position: absolute;
 left: 0;
 right: 0;
 top: 0;
 bottom: 0;
 border-radius: inherit;
 border: var(--n-border);
 transition: border-color .3s var(--n-bezier);
 `),x("icon",`
 display: flex;
 margin: 0 4px 0 0;
 color: var(--n-text-color);
 transition: color .3s var(--n-bezier);
 font-size: var(--n-avatar-size-override);
 `),x("avatar",`
 display: flex;
 margin: 0 6px 0 0;
 `),x("close",`
 margin: var(--n-close-margin);
 transition:
 background-color .3s var(--n-bezier),
 color .3s var(--n-bezier);
 `),C("round",`
 padding: 0 calc(var(--n-height) / 3);
 border-radius: calc(var(--n-height) / 2);
 `,[x("icon",`
 margin: 0 4px 0 calc((var(--n-height) - 8px) / -2);
 `),x("avatar",`
 margin: 0 6px 0 calc((var(--n-height) - 8px) / -2);
 `),C("closable",`
 padding: 0 calc(var(--n-height) / 4) 0 calc(var(--n-height) / 3);
 `)]),C("icon, avatar",[C("round",`
 padding: 0 calc(var(--n-height) / 3) 0 calc(var(--n-height) / 2);
 `)]),C("disabled",`
 cursor: not-allowed !important;
 opacity: var(--n-opacity-disabled);
 `),C("checkable",`
 cursor: pointer;
 box-shadow: none;
 color: var(--n-text-color-checkable);
 background-color: var(--n-color-checkable);
 `,[$("disabled",[P("&:hover","background-color: var(--n-color-hover-checkable);",[$("checked","color: var(--n-text-color-hover-checkable);")]),P("&:active","background-color: var(--n-color-pressed-checkable);",[$("checked","color: var(--n-text-color-pressed-checkable);")])]),C("checked",`
 color: var(--n-text-color-checked);
 background-color: var(--n-color-checked);
 `,[$("disabled",[P("&:hover","background-color: var(--n-color-checked-hover);"),P("&:active","background-color: var(--n-color-checked-pressed);")])])])]),Xe=Object.assign(Object.assign(Object.assign({},S.props),Ke),{bordered:{type:Boolean,default:void 0},checked:Boolean,checkable:Boolean,strong:Boolean,triggerClickOnClose:Boolean,onClose:[Array,Function],onMouseenter:Function,onMouseleave:Function,"onUpdate:checked":Function,onUpdateChecked:Function,internalCloseFocusable:{type:Boolean,default:!0},internalCloseIsButtonTag:{type:Boolean,default:!0},onCheckedChange:Function}),Ge=Pe("n-tag"),eo=Y({name:"Tag",props:Xe,setup(e){const r=ve(null),{mergedBorderedRef:o,mergedClsPrefixRef:a,inlineThemeDisabled:n,mergedRtlRef:c}=q(e),l=S("Tag","-tag",Ze,Ue,e,a);pe(Ge,{roundRef:me(e,"round")});function i(s){if(!e.disabled&&e.checkable){const{checked:d,onCheckedChange:k,onUpdateChecked:b,"onUpdate:checked":f}=e;b&&b(!d),f&&f(!d),k&&k(!d)}}function h(s){if(e.triggerClickOnClose||s.stopPropagation(),!e.disabled){const{onClose:d}=e;d&&Ie(d,s)}}const v={setTextContent(s){const{value:d}=r;d&&(d.textContent=s)}},p=ke("Tag",c,a),g=I(()=>{const{type:s,size:d,color:{color:k,textColor:b}={}}=e,{common:{cubicBezierEaseInOut:f},self:{padding:R,closeMargin:B,borderRadius:H,opacityDisabled:_,textColorCheckable:E,textColorHoverCheckable:T,textColorPressedCheckable:M,textColorChecked:w,colorCheckable:O,colorHoverCheckable:N,colorPressedCheckable:W,colorChecked:F,colorCheckedHover:Q,colorCheckedPressed:ee,closeBorderRadius:oe,fontWeightStrong:re,[u("colorBordered",s)]:ne,[u("closeSize",d)]:ce,[u("closeIconSize",d)]:ae,[u("fontSize",d)]:te,[u("height",d)]:D,[u("color",s)]:le,[u("textColor",s)]:se,[u("border",s)]:ie,[u("closeIconColor",s)]:L,[u("closeIconColorHover",s)]:de,[u("closeIconColorPressed",s)]:he,[u("closeColorHover",s)]:ue,[u("closeColorPressed",s)]:ge}}=l.value,z=ye(B);return{"--n-font-weight-strong":re,"--n-avatar-size-override":`calc(${D} - 8px)`,"--n-bezier":f,"--n-border-radius":H,"--n-border":ie,"--n-close-icon-size":ae,"--n-close-color-pressed":ge,"--n-close-color-hover":ue,"--n-close-border-radius":oe,"--n-close-icon-color":L,"--n-close-icon-color-hover":de,"--n-close-icon-color-pressed":he,"--n-close-icon-color-disabled":L,"--n-close-margin-top":z.top,"--n-close-margin-right":z.right,"--n-close-margin-bottom":z.bottom,"--n-close-margin-left":z.left,"--n-close-size":ce,"--n-color":k||(o.value?ne:le),"--n-color-checkable":O,"--n-color-checked":F,"--n-color-checked-hover":Q,"--n-color-checked-pressed":ee,"--n-color-hover-checkable":N,"--n-color-pressed-checkable":W,"--n-font-size":te,"--n-height":D,"--n-opacity-disabled":_,"--n-padding":R,"--n-text-color":b||se,"--n-text-color-checkable":E,"--n-text-color-checked":w,"--n-text-color-hover-checkable":T,"--n-text-color-pressed-checkable":M}}),m=n?J("tag",I(()=>{let s="";const{type:d,size:k,color:{color:b,textColor:f}={}}=e;return s+=d[0],s+=k[0],b&&(s+=`a${V(b)}`),f&&(s+=`b${V(f)}`),o.value&&(s+="c"),s}),g,e):void 0;return Object.assign(Object.assign({},v),{rtlEnabled:p,mergedClsPrefix:a,contentRef:r,mergedBordered:o,handleClick:i,handleCloseClick:h,cssVars:n?void 0:g,themeClass:m==null?void 0:m.themeClass,onRender:m==null?void 0:m.onRender})},render(){var e,r;const{mergedClsPrefix:o,rtlEnabled:a,closable:n,color:{borderColor:c}={},round:l,onRender:i,$slots:h}=this;i==null||i();const v=A(h.avatar,g=>g&&y("div",{class:`${o}-tag__avatar`},g)),p=A(h.icon,g=>g&&y("div",{class:`${o}-tag__icon`},g));return y("div",{class:[`${o}-tag`,this.themeClass,{[`${o}-tag--rtl`]:a,[`${o}-tag--strong`]:this.strong,[`${o}-tag--disabled`]:this.disabled,[`${o}-tag--checkable`]:this.checkable,[`${o}-tag--checked`]:this.checkable&&this.checked,[`${o}-tag--round`]:l,[`${o}-tag--avatar`]:v,[`${o}-tag--icon`]:p,[`${o}-tag--closable`]:n}],style:this.cssVars,onClick:this.handleClick,onMouseenter:this.onMouseenter,onMouseleave:this.onMouseleave},p||v,y("span",{class:`${o}-tag__content`,ref:"contentRef"},(r=(e=this.$slots).default)===null||r===void 0?void 0:r.call(e)),!this.checkable&&n?y(xe,{clsPrefix:o,class:`${o}-tag__close`,disabled:this.disabled,onClick:this.handleCloseClick,focusable:this.internalCloseFocusable,round:l,isButtonTag:this.internalCloseIsButtonTag,absolute:!0}):null,!this.checkable&&this.mergedBordered?y("div",{class:`${o}-tag__border`,style:{borderColor:c}}):null)}}),Ye=G("icon",`
 height: 1em;
 width: 1em;
 line-height: 1em;
 text-align: center;
 display: inline-block;
 position: relative;
 fill: currentColor;
 transform: translateZ(0);
`,[C("color-transition",{transition:"color .3s var(--n-bezier)"}),C("depth",{color:"var(--n-color)"},[P("svg",{opacity:"var(--n-opacity)",transition:"opacity .3s var(--n-bezier)"})]),P("svg",{height:"1em",width:"1em"})]),qe=Object.assign(Object.assign({},S.props),{depth:[String,Number],size:[Number,String],color:String,component:Object}),oo=Y({_n_icon__:!0,name:"Icon",inheritAttrs:!1,props:qe,setup(e){const{mergedClsPrefixRef:r,inlineThemeDisabled:o}=q(e),a=S("Icon","-icon",Ye,ze,e,r),n=I(()=>{const{depth:l}=e,{common:{cubicBezierEaseInOut:i},self:h}=a.value;if(l!==void 0){const{color:v,[`opacity${l}Depth`]:p}=h;return{"--n-bezier":i,"--n-color":v,"--n-opacity":p}}return{"--n-bezier":i,"--n-color":"","--n-opacity":""}}),c=o?J("icon",I(()=>`${e.depth||"d"}`),n,e):void 0;return{mergedClsPrefix:r,mergedStyle:I(()=>{const{size:l,color:i}=e;return{fontSize:Be(l),color:i}}),cssVars:o?void 0:n,themeClass:c==null?void 0:c.themeClass,onRender:c==null?void 0:c.onRender}},render(){var e;const{$parent:r,depth:o,mergedClsPrefix:a,component:n,onRender:c,themeClass:l}=this;return!((e=r==null?void 0:r.$options)===null||e===void 0)&&e._n_icon__&&$e("icon","don't wrap `n-icon` inside `n-icon`"),c==null||c(),y("i",Se(this.$attrs,{role:"img",class:[`${a}-icon`,l,{[`${a}-icon--depth`]:o,[`${a}-icon--color-transition`]:o!==void 0}],style:[this.cssVars,this.mergedStyle]}),n?y(n):this.$slots)}});export{oo as N,eo as a,De as b,Fe as c,Le as d,Be as f,Qe as g,Ee as i,Ge as t};

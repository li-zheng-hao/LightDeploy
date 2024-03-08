import{m as G,k as q,c as C,ai as U,a as L,aj as D,e as H,d as B,u as X,r as y,n as $,i as O,f as I,ae as Y,j as J,ak as Q,o as Z,K as ee,aa as te,a3 as re,q as oe,ag as ne,h as S,al as se,V as ae,O as T,U as W,W as A,am as ie}from"./index-IJZQufTE.js";import{t as le}from"./Icon-9eBcRM8V.js";const de=G&&"loading"in document.createElement("img"),ce=(e={})=>{var s;const{root:a=null}=e;return{hash:`${e.rootMargin||"0px 0px 0px 0px"}-${Array.isArray(e.threshold)?e.threshold.join(","):(s=e.threshold)!==null&&s!==void 0?s:"0"}`,options:Object.assign(Object.assign({},e),{root:(typeof a=="string"?document.querySelector(a):a)||document.documentElement})}},_=new WeakMap,P=new WeakMap,F=new WeakMap,ue=(e,s,a)=>{if(!e)return()=>{};const l=ce(s),{root:d}=l.options;let n;const c=_.get(d);c?n=c:(n=new Map,_.set(d,n));let h,o;n.has(l.hash)?(o=n.get(l.hash),o[1].has(e)||(h=o[0],o[1].add(e),h.observe(e))):(h=new IntersectionObserver(p=>{p.forEach(f=>{if(f.isIntersecting){const m=P.get(f.target),z=F.get(f.target);m&&m(),z&&(z.value=!0)}})},l.options),h.observe(e),o=[h,new Set([e])],n.set(l.hash,o));let u=!1;const g=()=>{u||(P.delete(e),F.delete(e),u=!0,o[1].has(e)&&(o[0].unobserve(e),o[1].delete(e)),o[1].size<=0&&n.delete(l.hash),n.size||_.delete(d))};return P.set(e,g),F.set(e,a),g},fe=q("n-avatar-group"),ve=C("avatar",`
 width: var(--n-merged-size);
 height: var(--n-merged-size);
 color: #FFF;
 font-size: var(--n-font-size);
 display: inline-flex;
 position: relative;
 overflow: hidden;
 text-align: center;
 border: var(--n-border);
 border-radius: var(--n-border-radius);
 --n-merged-color: var(--n-color);
 background-color: var(--n-merged-color);
 transition:
 border-color .3s var(--n-bezier),
 background-color .3s var(--n-bezier),
 color .3s var(--n-bezier);
`,[U(L("&","--n-merged-color: var(--n-color-modal);")),D(L("&","--n-merged-color: var(--n-color-popover);")),L("img",`
 width: 100%;
 height: 100%;
 `),H("text",`
 white-space: nowrap;
 display: inline-block;
 position: absolute;
 left: 50%;
 top: 50%;
 `),C("icon",`
 vertical-align: bottom;
 font-size: calc(var(--n-merged-size) - 6px);
 `),H("text","line-height: 1.25")]),he=Object.assign(Object.assign({},I.props),{size:[String,Number],src:String,circle:{type:Boolean,default:void 0},objectFit:String,round:{type:Boolean,default:void 0},bordered:{type:Boolean,default:void 0},onError:Function,fallbackSrc:String,intersectionObserverOptions:Object,lazy:Boolean,onLoad:Function,renderPlaceholder:Function,renderFallback:Function,imgProps:Object,color:String}),ze=B({name:"Avatar",props:he,setup(e){const{mergedClsPrefixRef:s,inlineThemeDisabled:a}=X(e),l=y(!1);let d=null;const n=y(null),c=y(null),h=()=>{const{value:t}=n;if(t&&(d===null||d!==t.innerHTML)){d=t.innerHTML;const{value:r}=c;if(r){const{offsetWidth:i,offsetHeight:b}=r,{offsetWidth:v,offsetHeight:j}=t,R=.9,E=Math.min(i/v*R,b/j*R,1);t.style.transform=`translateX(-50%) translateY(-50%) scale(${E})`}}},o=$(fe,null),u=O(()=>{const{size:t}=e;if(t)return t;const{size:r}=o||{};return r||"medium"}),g=I("Avatar","-avatar",ve,se,e,s),p=$(le,null),f=O(()=>{if(o)return!0;const{round:t,circle:r}=e;return t!==void 0||r!==void 0?t||r:p?p.roundRef.value:!1}),m=O(()=>o?!0:e.bordered||!1),z=O(()=>{const t=u.value,r=f.value,i=m.value,{color:b}=e,{self:{borderRadius:v,fontSize:j,color:R,border:E,colorModal:N,colorPopover:V},common:{cubicBezierEaseInOut:K}}=g.value;let k;return typeof t=="number"?k=`${t}px`:k=g.value.self[Y("height",t)],{"--n-font-size":j,"--n-border":i?E:"none","--n-border-radius":r?"50%":v,"--n-color":b||R,"--n-color-modal":b||N,"--n-color-popover":b||V,"--n-bezier":K,"--n-merged-size":`var(--n-avatar-size-override, ${k})`}}),x=a?J("avatar",O(()=>{const t=u.value,r=f.value,i=m.value,{color:b}=e;let v="";return t&&(typeof t=="number"?v+=`a${t}`:v+=t[0]),r&&(v+="b"),i&&(v+="c"),b&&(v+=Q(b)),v}),z,e):void 0,w=y(!e.lazy);Z(()=>{if(e.lazy&&e.intersectionObserverOptions){let t;const r=ee(()=>{t==null||t(),t=void 0,e.lazy&&(t=ue(c.value,e.intersectionObserverOptions,w))});te(()=>{r(),t==null||t()})}}),re(()=>{var t;return e.src||((t=e.imgProps)===null||t===void 0?void 0:t.src)},()=>{l.value=!1});const M=y(!e.lazy);return{textRef:n,selfRef:c,mergedRoundRef:f,mergedClsPrefix:s,fitTextTransform:h,cssVars:a?void 0:z,themeClass:x==null?void 0:x.themeClass,onRender:x==null?void 0:x.onRender,hasLoadError:l,shouldStartLoading:w,loaded:M,mergedOnError:t=>{if(!w.value)return;l.value=!0;const{onError:r,imgProps:{onError:i}={}}=e;r==null||r(t),i==null||i(t)},mergedOnLoad:t=>{const{onLoad:r,imgProps:{onLoad:i}={}}=e;r==null||r(t),i==null||i(t),M.value=!0}}},render(){var e,s;const{$slots:a,src:l,mergedClsPrefix:d,lazy:n,onRender:c,loaded:h,hasLoadError:o,imgProps:u={}}=this;c==null||c();let g;const p=!h&&!o&&(this.renderPlaceholder?this.renderPlaceholder():(s=(e=this.$slots).placeholder)===null||s===void 0?void 0:s.call(e));return this.hasLoadError?g=this.renderFallback?this.renderFallback():oe(a.fallback,()=>[S("img",{src:this.fallbackSrc,style:{objectFit:this.objectFit}})]):g=ne(a.default,f=>{if(f)return S(ae,{onResize:this.fitTextTransform},{default:()=>S("span",{ref:"textRef",class:`${d}-avatar__text`},f)});if(l||u.src){const m=this.src||u.src;return S("img",Object.assign(Object.assign({},u),{loading:de&&!this.intersectionObserverOptions&&n?"lazy":"eager",src:n&&this.intersectionObserverOptions?this.shouldStartLoading?m:void 0:m,"data-image-src":m,onLoad:this.mergedOnLoad,onError:this.mergedOnError,style:[u.style||"",{objectFit:this.objectFit},p?{height:"0",width:"0",visibility:"hidden",position:"absolute"}:""]}))}}),S("span",{ref:"selfRef",class:[`${d}-avatar`,this.themeClass],style:this.cssVars},g,n&&p)}}),ge={xmlns:"http://www.w3.org/2000/svg",viewBox:"0 0 1024 1024"},me=["xlink:href"],ye=B({__name:"SvgIconRaw",props:{prefix:{type:String,default:"#icon-"},name:String||null||void 0},setup(e){return(s,a)=>(T(),W("svg",ge,[A("use",{"xlink:href":e.prefix+e.name},null,8,me)]))}}),be=["xlink:href"],Oe=B({__name:"SvgIcon",props:{prefix:{type:String,default:"#icon-"},name:String||null||void 0,color:{type:String,default:null},size:{type:Number,default:18},scale:{type:Number,default:1}},setup(e){return(s,a)=>(T(),W("svg",{class:"svg-node flex flex-items-center",style:ie({width:e.size+"px",height:e.size+"px",scale:e.scale,color:e.color})},[A("use",{"xlink:href":e.prefix+e.name},null,8,be)],4))}});export{ye as _,Oe as a,ze as b};

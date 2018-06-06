(function(a){if(String.prototype.trim===a){String.prototype.trim=function(){return this.replace(/^\s+/,"").replace(/\s+$/,"")}}if(Array.prototype.reduce===a){Array.prototype.reduce=function(c){if(this===void 0||this===null){throw new TypeError()}var f=Object(this),b=f.length>>>0,e=0,d;if(typeof c!="function"){throw new TypeError()}if(b==0&&arguments.length==1){throw new TypeError()}if(arguments.length>=2){d=arguments[1]}else{do{if(e in f){d=f[e++];break}if(++e>=b){throw new TypeError()}}while(true)}while(e<b){if(e in f){d=c.call(a,d,f[e],e,f)}e++}return d}}})();var Zepto=(function(){var i,o,z,a,F=[],k=F.slice,f=window.document,E={},G={},m=f.defaultView.getComputedStyle,N={"column-count":1,columns:1,"font-weight":1,"line-height":1,opacity:1,"z-index":1,zoom:1},s=/^\s*<(\w+|!)[^>]*>/,y=[1,3,8,9,11],t=["after","prepend","before","append"],p=f.createElement("table"),H=f.createElement("tr"),g={tr:f.createElement("tbody"),tbody:p,thead:p,tfoot:p,td:H,th:H,"*":f.createElement("div")},q=/complete|loaded|interactive/,B=/^\.([\w-]+)$/,r=/^#([\w-]+)$/,D=/^[\w-]+$/,e=({}).toString,c={},L,I,A=f.createElement("div");c.matches=function(T,P){if(!T||T.nodeType!==1){return false}var R=T.webkitMatchesSelector||T.mozMatchesSelector||T.oMatchesSelector||T.matchesSelector;if(R){return R.call(T,P)}var S,U=T.parentNode,Q=!U;if(Q){(U=A).appendChild(T)}S=~c.qsa(U,P).indexOf(T);Q&&A.removeChild(T);return S};function l(P){return e.call(P)=="[object Function]"}function C(P){return P instanceof Object}function O(R){var P,Q;if(e.call(R)!=="[object Object]"){return false}Q=(l(R.constructor)&&R.constructor.prototype);if(!Q||!hasOwnProperty.call(Q,"isPrototypeOf")){return false}for(P in R){}return P===i||hasOwnProperty.call(R,P)}function v(P){return P instanceof Array}function w(P){return typeof P.length=="number"}function M(P){return P.filter(function(Q){return Q!==i&&Q!==null})}function x(P){return P.length>0?[].concat.apply([],P):P}L=function(P){return P.replace(/-+(.)?/g,function(Q,R){return R?R.toUpperCase():""})};function j(P){return P.replace(/::/g,"/").replace(/([A-Z]+)([A-Z][a-z])/g,"$1_$2").replace(/([a-z\d])([A-Z])/g,"$1_$2").replace(/_/g,"-").toLowerCase()}I=function(P){return P.filter(function(R,Q){return P.indexOf(R)==Q})};function J(P){return P in G?G[P]:(G[P]=new RegExp("(^|\\s)"+P+"(\\s|$)"))}function d(P,Q){return(typeof Q=="number"&&!N[j(P)])?Q+"px":Q}function K(R){var P,Q;if(!E[R]){P=f.createElement(R);f.body.appendChild(P);Q=m(P,"").getPropertyValue("display");P.parentNode.removeChild(P);Q=="none"&&(Q="block");E[R]=Q}return E[R]}c.fragment=function(R,Q){if(Q===i){Q=s.test(R)&&RegExp.$1}if(!(Q in g)){Q="*"}var P=g[Q];P.innerHTML=""+R;return z.each(k.call(P.childNodes),function(){P.removeChild(this)})};c.Z=function(Q,P){Q=Q||[];Q.__proto__=arguments.callee.prototype;Q.selector=P||"";return Q};c.isZ=function(P){return P instanceof c.Z};c.init=function(P,Q){if(!P){return c.Z()}else{if(l(P)){return z(f).ready(P)}else{if(c.isZ(P)){return P}else{var R;if(v(P)){R=M(P)}else{if(O(P)){R=[z.extend({},P)],P=null}else{if(y.indexOf(P.nodeType)>=0||P===window){R=[P],P=null}else{if(s.test(P)){R=c.fragment(P.trim(),RegExp.$1),P=null}else{if(Q!==i){return z(Q).find(P)}else{R=c.qsa(f,P)}}}}}return c.Z(R,P)}}}};z=function(P,Q){return c.init(P,Q)};z.extend=function(P){k.call(arguments,1).forEach(function(Q){for(o in Q){if(Q[o]!==i){P[o]=Q[o]}}});return P};c.qsa=function(Q,P){var R;return(Q===f&&r.test(P))?((R=Q.getElementById(RegExp.$1))?[R]:F):(Q.nodeType!==1&&Q.nodeType!==9)?F:k.call(B.test(P)?Q.getElementsByClassName(RegExp.$1):D.test(P)?Q.getElementsByTagName(P):Q.querySelectorAll(P))};function u(Q,P){return P===i?z(Q):z(Q).filter(P)}function n(R,Q,P,S){return l(Q)?Q.call(R,P,S):Q}z.isFunction=l;z.isObject=C;z.isArray=v;z.isPlainObject=O;z.inArray=function(Q,R,P){return F.indexOf.call(R,Q,P)};z.trim=function(P){return P.trim()};z.uuid=0;z.map=function(T,U){var S,P=[],R,Q;if(w(T)){for(R=0;R<T.length;R++){S=U(T[R],R);if(S!=null){P.push(S)}}}else{for(Q in T){S=U(T[Q],Q);if(S!=null){P.push(S)}}}return x(P)};z.each=function(R,S){var Q,P;if(w(R)){for(Q=0;Q<R.length;Q++){if(S.call(R[Q],Q,R[Q])===false){return R}}}else{for(P in R){if(S.call(R[P],P,R[P])===false){return R}}}return R};z.fn={forEach:F.forEach,reduce:F.reduce,push:F.push,indexOf:F.indexOf,concat:F.concat,map:function(P){return z.map(this,function(R,Q){return P.call(R,Q,R)})},slice:function(){return z(k.apply(this,arguments))},ready:function(P){if(q.test(f.readyState)){P(z)}else{f.addEventListener("DOMContentLoaded",function(){P(z)},false)}return this},get:function(P){return P===i?k.call(this):this[P]},toArray:function(){return this.get()},size:function(){return this.length},remove:function(){return this.each(function(){if(this.parentNode!=null){this.parentNode.removeChild(this)}})},each:function(P){this.forEach(function(R,Q){P.call(R,Q,R)});return this},filter:function(P){return z([].filter.call(this,function(Q){return c.matches(Q,P)}))},add:function(P,Q){return z(I(this.concat(z(P,Q))))},is:function(P){return this.length>0&&c.matches(this[0],P)},not:function(P){var Q=[];if(l(P)&&P.call!==i){this.each(function(S){if(!P.call(this,S)){Q.push(this)}})}else{var R=typeof P=="string"?this.filter(P):(w(P)&&l(P.item))?k.call(P):z(P);this.forEach(function(S){if(R.indexOf(S)<0){Q.push(S)}})}return z(Q)},eq:function(P){return P===-1?this.slice(P):this.slice(P,+P+1)},first:function(){var P=this[0];return P&&!C(P)?P:z(P)},last:function(){var P=this[this.length-1];return P&&!C(P)?P:z(P)},find:function(Q){var P;if(this.length==1){P=c.qsa(this[0],Q)}else{P=this.map(function(){return c.qsa(this,Q)})}return z(P)},closest:function(P,Q){var R=this[0];while(R&&!c.matches(R,P)){R=R!==Q&&R!==f&&R.parentNode}return z(R)},parents:function(P){var R=[],Q=this;while(Q.length>0){Q=z.map(Q,function(S){if((S=S.parentNode)&&S!==f&&R.indexOf(S)<0){R.push(S);return S}})}return u(R,P)},parent:function(P){return u(I(this.pluck("parentNode")),P)},children:function(P){return u(this.map(function(){return k.call(this.children)}),P)},siblings:function(P){return u(this.map(function(Q,R){return k.call(R.parentNode.children).filter(function(S){return S!==R})}),P)},empty:function(){return this.each(function(){this.innerHTML=""})},pluck:function(P){return this.map(function(){return this[P]})},show:function(){return this.each(function(){this.style.display=="none"&&(this.style.display=null);if(m(this,"").getPropertyValue("display")=="none"){this.style.display=K(this.nodeName)}})},replaceWith:function(P){return this.before(P).remove()},wrap:function(P){return this.each(function(){z(this).wrapAll(z(P)[0].cloneNode(false))})},wrapAll:function(P){if(this[0]){z(this[0]).before(P=z(P));P.append(this)}return this},unwrap:function(){this.parent().each(function(){z(this).replaceWith(z(this).children())});return this},clone:function(){return z(this.map(function(){return this.cloneNode(true)}))},hide:function(){return this.css("display","none")},toggle:function(P){return(P===i?this.css("display")=="none":P)?this.show():this.hide()},prev:function(){return z(this.pluck("previousElementSibling"))},next:function(){return z(this.pluck("nextElementSibling"))},html:function(P){return P===i?(this.length>0?this[0].innerHTML:null):this.each(function(Q){var R=this.innerHTML;z(this).empty().append(n(this,P,Q,R))})},text:function(P){return P===i?(this.length>0?this[0].textContent:null):this.each(function(){this.textContent=P})},attr:function(Q,R){var P;return(typeof Q=="string"&&R===i)?(this.length==0||this[0].nodeType!==1?i:(Q=="value"&&this[0].nodeName=="INPUT")?this.val():(!(P=this[0].getAttribute(Q))&&Q in this[0])?this[0][Q]:P):this.each(function(S){if(this.nodeType!==1){return}if(C(Q)){for(o in Q){this.setAttribute(o,Q[o])}}else{this.setAttribute(Q,n(this,R,S,this.getAttribute(Q)))}})},removeAttr:function(P){return this.each(function(){if(this.nodeType===1){this.removeAttribute(P)}})},prop:function(P,Q){return(Q===i)?(this[0]?this[0][P]:i):this.each(function(R){this[P]=n(this,Q,R,this[P])})},data:function(P,R){var Q=this.attr("data-"+j(P),R);return Q!==null?Q:i},val:function(P){return(P===i)?(this.length>0?this[0].value:i):this.each(function(Q){this.value=n(this,P,Q,this.value)})},offset:function(){if(this.length==0){return null}var P=this[0].getBoundingClientRect();return{left:P.left+window.pageXOffset,top:P.top+window.pageYOffset,width:P.width,height:P.height}},css:function(R,Q){if(Q===i&&typeof R=="string"){return(this.length==0?i:this[0].style[L(R)]||m(this[0],"").getPropertyValue(R))}var P="";for(o in R){if(typeof R[o]=="string"&&R[o]==""){this.each(function(){this.style.removeProperty(j(o))})}else{P+=j(o)+":"+d(o,R[o])+";"}}if(typeof R=="string"){if(Q==""){this.each(function(){this.style.removeProperty(j(R))})}else{P=j(R)+":"+d(R,Q)}}return this.each(function(){this.style.cssText+=";"+P})},index:function(P){return P?this.indexOf(z(P)[0]):this.parent().children().indexOf(this[0])},hasClass:function(P){if(this.length<1){return false}else{return J(P).test(this[0].className)}},addClass:function(P){return this.each(function(Q){a=[];var S=this.className,R=n(this,P,Q,S);R.split(/\s+/g).forEach(function(T){if(!z(this).hasClass(T)){a.push(T)}},this);a.length&&(this.className+=(S?" ":"")+a.join(" "))})},removeClass:function(P){return this.each(function(Q){if(P===i){return this.className=""}a=this.className;n(this,P,Q,a).split(/\s+/g).forEach(function(R){a=a.replace(J(R)," ")});this.className=a.trim()})},toggleClass:function(Q,P){return this.each(function(R){var S=n(this,Q,R,this.className);(P===i?!z(this).hasClass(S):P)?z(this).addClass(S):z(this).removeClass(S)})}};["width","height"].forEach(function(P){z.fn[P]=function(Q){var S,R=P.replace(/./,function(T){return T[0].toUpperCase()});if(Q===i){return this[0]==window?window["inner"+R]:this[0]==f?f.documentElement["offset"+R]:(S=this.offset())&&S[P]}else{return this.each(function(T){var U=z(this);U.css(P,n(this,Q,T,U[P]()))})}}});function h(P,S,R){var Q=(P%2)?S:S.parentNode;Q?Q.insertBefore(R,!P?S.nextSibling:P==1?Q.firstChild:P==2?S:null):z(R).remove()}function b(R,P){P(R);for(var Q in R.childNodes){b(R.childNodes[Q],P)}}t.forEach(function(Q,P){z.fn[Q]=function(){var R=z.map(arguments,function(V){return C(V)?V:c.fragment(V)});if(R.length<1){return this}var S=this.length,T=S>1,U=P<2;return this.each(function(V,Y){for(var W=0;W<R.length;W++){var X=R[U?R.length-W-1:W];b(X,function(Z){if(Z.nodeName!=null&&Z.nodeName.toUpperCase()==="SCRIPT"&&(!Z.type||Z.type==="text/javascript")){window["eval"].call(window,Z.innerHTML)}});if(T&&V<S-1){X=X.cloneNode(true)}h(P,Y,X)}})};z.fn[(P%2)?Q+"To":"insert"+(P?"Before":"After")]=function(R){z(R)[Q](this);return this}});c.Z.prototype=z.fn;c.camelize=L;c.uniq=I;z.zepto=c;return z})();window.Zepto=Zepto;"$" in window||(window.$=Zepto);(function(h){var o=h.zepto.qsa,b={},n=1,q={};q.click=q.mousedown=q.mouseup=q.mousemove="MouseEvents";function l(r){return r._zid||(r._zid=n++)}function c(s,u,t,r){u=e(u);if(u.ns){var v=k(u.ns)}return(b[l(s)]||[]).filter(function(w){return w&&(!u.e||w.e==u.e)&&(!u.ns||v.test(w.ns))&&(!t||l(w.fn)===l(t))&&(!r||w.sel==r)})}function e(r){var s=(""+r).split(".");return{e:s[0],ns:s.slice(1).sort().join(" ")}}function k(r){return new RegExp("(?:^| )"+r.replace(" "," .* ?")+"(?: |$)")}function m(r,t,s){if(h.isObject(r)){h.each(r,s)}else{r.split(/\s/).forEach(function(u){s(u,t)})}}function p(v,u,w,s,r,t){t=!!t;var y=l(v),x=(b[y]||(b[y]=[]));m(u,w,function(C,B){var A=r&&r(B,C),E=A||B;var D=function(G){var F=E.apply(v,[G].concat(G.data));if(F===false){G.preventDefault()}return F};var z=h.extend(e(C),{fn:B,proxy:D,sel:s,del:A,i:x.length});x.push(z);v.addEventListener(z.e,D,t)})}function g(t,s,u,r){var v=l(t);m(s||"",u,function(x,w){c(t,x,w,r).forEach(function(y){delete b[v][y.i];t.removeEventListener(y.e,y.proxy,false)})})}h.event={add:p,remove:g};h.proxy=function(t,s){if(h.isFunction(t)){var r=function(){return t.apply(s,arguments)};r._zid=l(t);return r}else{if(typeof s=="string"){return h.proxy(t[s],t)}else{throw new TypeError("expected function")}}};h.fn.bind=function(r,s){return this.each(function(){p(this,r,s)})};h.fn.unbind=function(r,s){return this.each(function(){g(this,r,s)})};h.fn.one=function(r,s){return this.each(function(u,t){p(this,r,s,null,function(w,v){return function(){var x=w.apply(t,arguments);g(t,v,w);return x}})})};var d=function(){return true},a=function(){return false},j={preventDefault:"isDefaultPrevented",stopImmediatePropagation:"isImmediatePropagationStopped",stopPropagation:"isPropagationStopped"};function i(s){var r=h.extend({originalEvent:s},s);h.each(j,function(u,t){r[u]=function(){this[t]=d;return s[u].apply(s,arguments)};r[t]=a});return r}function f(s){if(!("defaultPrevented" in s)){s.defaultPrevented=false;var r=s.preventDefault;s.preventDefault=function(){this.defaultPrevented=true;r.call(this)}}}h.fn.delegate=function(r,t,u){var s=false;if(t=="blur"||t=="focus"){if(h.iswebkit){t=t=="blur"?"focusout":t=="focus"?"focusin":t}else{s=true}}return this.each(function(w,v){p(v,t,u,r,function(x){return function(A){var y,z=h(A.target).closest(r,v).get(0);if(z){y=h.extend(i(A),{currentTarget:z,liveFired:v});return x.apply(z,[y].concat([].slice.call(arguments,1)))}}},s)})};h.fn.undelegate=function(r,s,t){return this.each(function(){g(this,s,t,r)})};h.fn.live=function(r,s){h(document.body).delegate(this.selector,r,s);return this};h.fn.die=function(r,s){h(document.body).undelegate(this.selector,r,s);return this};h.fn.on=function(s,r,t){return r==undefined||h.isFunction(r)?this.bind(s,r):this.delegate(r,s,t)};h.fn.off=function(s,r,t){return r==undefined||h.isFunction(r)?this.unbind(s,r):this.undelegate(r,s,t)};h.fn.trigger=function(r,s){if(typeof r=="string"){r=h.Event(r)}f(r);r.data=s;return this.each(function(){if("dispatchEvent" in this){this.dispatchEvent(r)}})};h.fn.triggerHandler=function(s,t){var u,r;this.each(function(w,v){u=i(typeof s=="string"?h.Event(s):s);u.data=t;u.target=v;h.each(c(v,s.type||s),function(x,y){r=y.proxy(u);if(u.isImmediatePropagationStopped()){return false}})});return r};("focusin focusout load resize scroll unload click dblclick mousedown mouseup mousemove mouseover mouseout change select keydown keypress keyup error").split(" ").forEach(function(r){h.fn[r]=function(s){return this.bind(r,s)}});["focus","blur"].forEach(function(r){h.fn[r]=function(t){if(t){this.bind(r,t)}else{if(this.length){try{this.get(0)[r]()}catch(s){}}}return this}});h.Event=function(u,t){var v=document.createEvent(q[u]||"Events"),r=true;if(t){for(var s in t){(s=="bubbles")?(r=!!t[s]):(v[s]=t[s])}}v.initEvent(u,r,true,null,null,null,null,null,null,null,null,null,null,null,null);return v}})(Zepto);(function(b){function a(c){var f=this.os={},g=this.browser={},l=c.match(/WebKit\/([\d.]+)/),e=c.match(/(Android)\s+([\d.]+)/),m=c.match(/(iPad).*OS\s([\d_]+)/),k=!m&&c.match(/(iPhone\sOS)\s([\d_]+)/),n=c.match(/(webOS|hpwOS)[\s\/]([\d.]+)/),j=n&&c.match(/TouchPad/),i=c.match(/Kindle\/([\d.]+)/),h=c.match(/Silk\/([\d._]+)/),d=c.match(/(BlackBerry).*Version\/([\d.]+)/);if(g.webkit=!!l){g.version=l[1]}if(e){f.android=true,f.version=e[2]}if(k){f.ios=f.iphone=true,f.version=k[2].replace(/_/g,".")}if(m){f.ios=f.ipad=true,f.version=m[2].replace(/_/g,".")}if(n){f.webos=true,f.version=n[2]}if(j){f.touchpad=true}if(d){f.blackberry=true,f.version=d[2]}if(i){f.kindle=true,f.version=i[1]}if(h){g.silk=true,g.version=h[1]}if(!h&&f.android&&c.match(/Kindle Fire/)){g.silk=true}}a.call(b,navigator.userAgent);b.__detect=a})(Zepto);(function(e,c){var g="",k,b,i,m={Webkit:"webkit",Moz:"",O:"o",ms:"MS"},j=window.document,d=j.createElement("div"),l=/^((translate|rotate|scale)(X|Y|Z|3d)?|matrix(3d)?|perspective|skew(X|Y)?)$/i,h={};function a(n){return n.toLowerCase()}function f(n){return k?k+n:a(n)}e.each(m,function(o,n){if(d.style[o+"TransitionProperty"]!==c){g="-"+a(o)+"-";k=n;return false}});h[g+"transition-property"]=h[g+"transition-duration"]=h[g+"transition-timing-function"]=h[g+"animation-name"]=h[g+"animation-duration"]="";e.fx={off:(k===c&&d.style.transitionProperty===c),cssPrefix:g,transitionEnd:f("TransitionEnd"),animationEnd:f("AnimationEnd")};e.fn.animate=function(n,o,p,q){if(e.isObject(o)){p=o.easing,q=o.complete,o=o.duration}if(o){o=o/1000}return this.anim(n,o,p,q)};e.fn.anim=function(s,p,o,u){var r,w={},t,q=this,n,v=e.fx.transitionEnd;if(p===c){p=0.4}if(e.fx.off){p=0}if(typeof s=="string"){w[g+"animation-name"]=s;w[g+"animation-duration"]=p+"s";v=e.fx.animationEnd}else{for(t in s){if(l.test(t)){r||(r=[]);r.push(t+"("+s[t]+")")}else{w[t]=s[t]}}if(r){w[g+"transform"]=r.join(" ")}if(!e.fx.off&&typeof s==="object"){w[g+"transition-property"]=Object.keys(s).join(", ");w[g+"transition-duration"]=p+"s";w[g+"transition-timing-function"]=(o||"linear")}}n=function(x){if(typeof x!=="undefined"){if(x.target!==x.currentTarget){return}e(x.target).unbind(v,arguments.callee)}e(this).css(h);u&&u.call(this)};if(p>0){this.bind(v,n)}setTimeout(function(){q.css(w);if(p<=0){setTimeout(function(){q.each(function(){n.call(this)})},0)}},0);return this};d=null})(Zepto);(function($){var jsonpID=0,isObject=$.isObject,document=window.document,key,name,rscript=/<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi,scriptTypeRE=/^(?:text|application)\/javascript/i,xmlTypeRE=/^(?:text|application)\/xml/i,jsonType="application/json",htmlType="text/html",blankRE=/^\s*$/;function triggerAndReturn(context,eventName,data){var event=$.Event(eventName);$(context).trigger(event,data);return !event.defaultPrevented}function triggerGlobal(settings,context,eventName,data){if(settings.global){return triggerAndReturn(context||document,eventName,data)}}$.active=0;function ajaxStart(settings){if(settings.global&&$.active++===0){triggerGlobal(settings,null,"ajaxStart")}}function ajaxStop(settings){if(settings.global&&!(--$.active)){triggerGlobal(settings,null,"ajaxStop")}}function ajaxBeforeSend(xhr,settings){var context=settings.context;if(settings.beforeSend.call(context,xhr,settings)===false||triggerGlobal(settings,context,"ajaxBeforeSend",[xhr,settings])===false){return false}triggerGlobal(settings,context,"ajaxSend",[xhr,settings])}function ajaxSuccess(data,xhr,settings){var context=settings.context,status="success";settings.success.call(context,data,status,xhr);triggerGlobal(settings,context,"ajaxSuccess",[xhr,settings,data]);ajaxComplete(status,xhr,settings)}function ajaxError(error,type,xhr,settings){var context=settings.context;settings.error.call(context,xhr,type,error);triggerGlobal(settings,context,"ajaxError",[xhr,settings,error]);ajaxComplete(type,xhr,settings)}function ajaxComplete(status,xhr,settings){var context=settings.context;settings.complete.call(context,xhr,status);triggerGlobal(settings,context,"ajaxComplete",[xhr,settings]);ajaxStop(settings)}function empty(){}$.ajaxJSONP=function(options){var callbackName="jsonp"+(++jsonpID),script=document.createElement("script"),abort=function(){$(script).remove();if(callbackName in window){window[callbackName]=empty}ajaxComplete("abort",xhr,options)},xhr={abort:abort},abortTimeout;if(options.error){script.onerror=function(){xhr.abort();options.error()}}window[callbackName]=function(data){clearTimeout(abortTimeout);$(script).remove();delete window[callbackName];ajaxSuccess(data,xhr,options)};serializeData(options);script.src=options.url.replace(/=\?/,"="+callbackName);$("head").append(script);if(options.timeout>0){abortTimeout=setTimeout(function(){xhr.abort();ajaxComplete("timeout",xhr,options)},options.timeout)}return xhr};$.ajaxSettings={type:"GET",beforeSend:empty,success:empty,error:empty,complete:empty,context:null,global:true,xhr:function(){return new window.XMLHttpRequest()},accepts:{script:"text/javascript, application/javascript",json:jsonType,xml:"application/xml, text/xml",html:htmlType,text:"text/plain"},crossDomain:false,timeout:0};function mimeToDataType(mime){return mime&&(mime==htmlType?"html":mime==jsonType?"json":scriptTypeRE.test(mime)?"script":xmlTypeRE.test(mime)&&"xml")||"text"}function appendQuery(url,query){return(url+"&"+query).replace(/[&?]{1,2}/,"?")}function serializeData(options){if(isObject(options.data)){options.data=$.param(options.data)}if(options.data&&(!options.type||options.type.toUpperCase()=="GET")){options.url=appendQuery(options.url,options.data)}}$.ajax=function(options){var settings=$.extend({},options||{});for(key in $.ajaxSettings){if(settings[key]===undefined){settings[key]=$.ajaxSettings[key]}}ajaxStart(settings);if(!settings.crossDomain){settings.crossDomain=/^([\w-]+:)?\/\/([^\/]+)/.test(settings.url)&&RegExp.$2!=window.location.host}var dataType=settings.dataType,hasPlaceholder=/=\?/.test(settings.url);if(dataType=="jsonp"||hasPlaceholder){if(!hasPlaceholder){settings.url=appendQuery(settings.url,"callback=?")}return $.ajaxJSONP(settings)}if(!settings.url){settings.url=window.location.toString()}serializeData(settings);var mime=settings.accepts[dataType],baseHeaders={},protocol=/^([\w-]+:)\/\//.test(settings.url)?RegExp.$1:window.location.protocol,xhr=$.ajaxSettings.xhr(),abortTimeout;if(!settings.crossDomain){baseHeaders["X-Requested-With"]="XMLHttpRequest"}if(mime){baseHeaders.Accept=mime;if(mime.indexOf(",")>-1){mime=mime.split(",",2)[0]}xhr.overrideMimeType&&xhr.overrideMimeType(mime)}if(settings.contentType||(settings.data&&settings.type.toUpperCase()!="GET")){baseHeaders["Content-Type"]=(settings.contentType||"application/x-www-form-urlencoded")}settings.headers=$.extend(baseHeaders,settings.headers||{});xhr.onreadystatechange=function(){if(xhr.readyState==4){clearTimeout(abortTimeout);var result,error=false;if((xhr.status>=200&&xhr.status<300)||xhr.status==304||(xhr.status==0&&protocol=="file:")){dataType=dataType||mimeToDataType(xhr.getResponseHeader("content-type"));result=xhr.responseText;try{if(dataType=="script"){(1,eval)(result)}else{if(dataType=="xml"){result=xhr.responseXML}else{if(dataType=="json"){result=blankRE.test(result)?null:JSON.parse(result)}}}}catch(e){error=e}if(error){ajaxError(error,"parsererror",xhr,settings)}else{ajaxSuccess(result,xhr,settings)}}else{ajaxError(null,"error",xhr,settings)}}};var async="async" in settings?settings.async:true;xhr.open(settings.type,settings.url,async);for(name in settings.headers){xhr.setRequestHeader(name,settings.headers[name])}if(ajaxBeforeSend(xhr,settings)===false){xhr.abort();return false}if(settings.timeout>0){abortTimeout=setTimeout(function(){xhr.onreadystatechange=empty;xhr.abort();ajaxError(null,"timeout",xhr,settings)},settings.timeout)}xhr.send(settings.data?settings.data:null);return xhr};$.get=function(url,success){return $.ajax({url:url,success:success})};$.post=function(url,data,success,dataType){if($.isFunction(data)){dataType=dataType||success,success=data,data=null}return $.ajax({type:"POST",url:url,data:data,success:success,dataType:dataType})};$.getJSON=function(url,success){return $.ajax({url:url,success:success,dataType:"json"})};$.fn.load=function(url,success){if(!this.length){return this}var self=this,parts=url.split(/\s/),selector;if(parts.length>1){url=parts[0],selector=parts[1]}$.get(url,function(response){self.html(selector?$(document.createElement("div")).html(response.replace(rscript,"")).find(selector).html():response);success&&success.call(self)});return this};var escape=encodeURIComponent;function serialize(params,obj,traditional,scope){var array=$.isArray(obj);$.each(obj,function(key,value){if(scope){key=traditional?scope:scope+"["+(array?"":key)+"]"}if(!scope&&array){params.add(value.name,value.value)}else{if(traditional?$.isArray(value):isObject(value)){serialize(params,value,traditional,key)}else{params.add(key,value)}}})}$.param=function(obj,traditional){var params=[];params.add=function(k,v){this.push(escape(k)+"="+escape(v))};serialize(params,obj,traditional);return params.join("&").replace("%20","+")}})(Zepto);(function(a){a.fn.serializeArray=function(){var b=[],c;a(Array.prototype.slice.call(this.get(0).elements)).each(function(){c=a(this);var e=c.attr("type"),d=c.attr("name");if(this.nodeName.toLowerCase()!="fieldset"&&!this.disabled&&e!="submit"&&e!="reset"&&e!="button"&&((e!="radio"&&e!="checkbox")||this.checked)&&d){b.push({name:d,value:c.val()})}});return b};a.fn.serialize=function(){var b=[];this.serializeArray().forEach(function(c){b.push(encodeURIComponent(c.name)+"="+encodeURIComponent(c.value))});return b.join("&")};a.fn.submit=function(c){if(c){this.bind("submit",c)}else{if(this.length){var b=a.Event("submit");this.eq(0).trigger(b);if(!b.defaultPrevented){this.get(0).submit()}}}return this}})(Zepto);(function(g){var f={},b;function c(j){return"tagName" in j?j:j.parentNode}function h(k,j,m,l){var o=Math.abs(k-j),n=Math.abs(m-l);return o>=n?(k-j>0?"Left":"Right"):(m-l>0?"Up":"Down")}var e=750,a;function i(){a=null;if(f.last){f.el.trigger("longTap");f={}}}function d(){if(a){clearTimeout(a)}a=null}g(document).ready(function(){var j,k;g(document.body).bind("touchstart",function(l){j=Date.now();k=j-(f.last||j);f.el=g(c(l.touches[0].target));b&&clearTimeout(b);f.x1=l.touches[0].pageX;f.y1=l.touches[0].pageY;if(k>0&&k<=250){f.isDoubleTap=true}f.last=j;a=setTimeout(i,e)}).bind("touchmove",function(l){d();f.x2=l.touches[0].pageX;f.y2=l.touches[0].pageY}).bind("touchend",function(l){d();if(f.isDoubleTap){f.el.trigger("doubleTap");f={}}else{if((f.x2&&Math.abs(f.x1-f.x2)>30)||(f.y2&&Math.abs(f.y1-f.y2)>30)){f.el.trigger("swipe")&&f.el.trigger("swipe"+(h(f.x1,f.x2,f.y1,f.y2)));f={}}else{if("last" in f){f.el.trigger("tap");b=setTimeout(function(){b=null;f.el.trigger("singleTap");f={}},250)}}}}).bind("touchcancel",function(){if(b){clearTimeout(b)}if(a){clearTimeout(a)}a=b=null;f={}})});["swipe","swipeLeft","swipeRight","swipeUp","swipeDown","doubleTap","tap","singleTap","longTap"].forEach(function(j){g.fn[j]=function(k){return this.bind(j,k)}})})(Zepto);

/*! iScroll v5.1.3 ~ (c) 2008-2014 Matteo Spinelli ~ http://cubiq.org/license */
(function (window, document, Math) {
var rAF = window.requestAnimationFrame	||
	window.webkitRequestAnimationFrame	||
	window.mozRequestAnimationFrame		||
	window.oRequestAnimationFrame		||
	window.msRequestAnimationFrame		||
	function (callback) { window.setTimeout(callback, 1000 / 60); };

var utils = (function () {
	var me = {};

	var _elementStyle = document.createElement('div').style;
	var _vendor = (function () {
		var vendors = ['t', 'webkitT', 'MozT', 'msT', 'OT'],
			transform,
			i = 0,
			l = vendors.length;

		for ( ; i < l; i++ ) {
			transform = vendors[i] + 'ransform';
			if ( transform in _elementStyle ) return vendors[i].substr(0, vendors[i].length-1);
		}

		return false;
	})();

	function _prefixStyle (style) {
		if ( _vendor === false ) return false;
		if ( _vendor === '' ) return style;
		return _vendor + style.charAt(0).toUpperCase() + style.substr(1);
	}

	me.getTime = Date.now || function getTime () { return new Date().getTime(); };

	me.extend = function (target, obj) {
		for ( var i in obj ) {
			target[i] = obj[i];
		}
	};

	me.addEvent = function (el, type, fn, capture) {
		el.addEventListener(type, fn, !!capture);
	};

	me.removeEvent = function (el, type, fn, capture) {
		el.removeEventListener(type, fn, !!capture);
	};

	me.prefixPointerEvent = function (pointerEvent) {
		return window.MSPointerEvent ? 
			'MSPointer' + pointerEvent.charAt(9).toUpperCase() + pointerEvent.substr(10):
			pointerEvent;
	};

	me.momentum = function (current, start, time, lowerMargin, wrapperSize, deceleration) {
		var distance = current - start,
			speed = Math.abs(distance) / time,
			destination,
			duration;

		deceleration = deceleration === undefined ? 0.0006 : deceleration;

		destination = current + ( speed * speed ) / ( 2 * deceleration ) * ( distance < 0 ? -1 : 1 );
		duration = speed / deceleration;

		if ( destination < lowerMargin ) {
			destination = wrapperSize ? lowerMargin - ( wrapperSize / 2.5 * ( speed / 8 ) ) : lowerMargin;
			distance = Math.abs(destination - current);
			duration = distance / speed;
		} else if ( destination > 0 ) {
			destination = wrapperSize ? wrapperSize / 2.5 * ( speed / 8 ) : 0;
			distance = Math.abs(current) + destination;
			duration = distance / speed;
		}

		return {
			destination: Math.round(destination),
			duration: duration
		};
	};

	var _transform = _prefixStyle('transform');

	me.extend(me, {
		hasTransform: _transform !== false,
		hasPerspective: _prefixStyle('perspective') in _elementStyle,
		hasTouch: 'ontouchstart' in window,
		hasPointer: window.PointerEvent || window.MSPointerEvent, // IE10 is prefixed
		hasTransition: _prefixStyle('transition') in _elementStyle
	});

	// This should find all Android browsers lower than build 535.19 (both stock browser and webview)
	me.isBadAndroid = /Android /.test(window.navigator.appVersion) && !(/Chrome\/\d/.test(window.navigator.appVersion));

	me.extend(me.style = {}, {
		transform: _transform,
		transitionTimingFunction: _prefixStyle('transitionTimingFunction'),
		transitionDuration: _prefixStyle('transitionDuration'),
		transitionDelay: _prefixStyle('transitionDelay'),
		transformOrigin: _prefixStyle('transformOrigin')
	});

	me.hasClass = function (e, c) {
		return ((" "+e.className+" ").indexOf(" "+c+" ") == -1)?false:true;
	};

	me.addClass = function (e, c) {
		if ( me.hasClass(e, c) ) {
			return;
		}

		var newclass = e.className.split(' ');
		newclass.push(c);
		e.className = newclass.join(' ');
	};

	me.removeClass = function (e, c) {
		if ( !me.hasClass(e, c) ) {
			return;
		}
		
		e.className = (" " + e.className + " ").replace(" " + c + " ", " ");
	};

	me.offset = function (el) {
		var left = -el.offsetLeft,
			top = -el.offsetTop;

		// jshint -W084
		while (el = el.offsetParent) {
			left -= el.offsetLeft;
			top -= el.offsetTop;
		}
		// jshint +W084

		return {
			left: left,
			top: top
		};
	};

	me.preventDefaultException = function (el, exceptions) {
		for ( var i in exceptions ) {
			if ( exceptions[i].test(el[i]) ) {
				return true;
			}
		}

		return false;
	};

	me.extend(me.eventType = {}, {
		touchstart: 1,
		touchmove: 1,
		touchend: 1,

		mousedown: 2,
		mousemove: 2,
		mouseup: 2,

		pointerdown: 3,
		pointermove: 3,
		pointerup: 3,

		MSPointerDown: 3,
		MSPointerMove: 3,
		MSPointerUp: 3
	});

	me.extend(me.ease = {}, {
		quadratic: {
			style: 'cubic-bezier(0.25, 0.46, 0.45, 0.94)',
			fn: function (k) {
				return k * ( 2 - k );
			}
		},
		circular: {
			style: 'cubic-bezier(0.1, 0.57, 0.1, 1)',	// Not properly "circular" but this looks better, it should be (0.075, 0.82, 0.165, 1)
			fn: function (k) {
				return Math.sqrt( 1 - ( --k * k ) );
			}
		},
		back: {
			style: 'cubic-bezier(0.175, 0.885, 0.32, 1.275)',
			fn: function (k) {
				var b = 4;
				return ( k = k - 1 ) * k * ( ( b + 1 ) * k + b ) + 1;
			}
		},
		bounce: {
			style: '',
			fn: function (k) {
				if ( ( k /= 1 ) < ( 1 / 2.75 ) ) {
					return 7.5625 * k * k;
				} else if ( k < ( 2 / 2.75 ) ) {
					return 7.5625 * ( k -= ( 1.5 / 2.75 ) ) * k + 0.75;
				} else if ( k < ( 2.5 / 2.75 ) ) {
					return 7.5625 * ( k -= ( 2.25 / 2.75 ) ) * k + 0.9375;
				} else {
					return 7.5625 * ( k -= ( 2.625 / 2.75 ) ) * k + 0.984375;
				}
			}
		},
		elastic: {
			style: '',
			fn: function (k) {
				var f = 0.22,
					e = 0.4;

				if ( k === 0 ) { return 0; }
				if ( k == 1 ) { return 1; }

				return ( e * Math.pow( 2, - 10 * k ) * Math.sin( ( k - f / 4 ) * ( 2 * Math.PI ) / f ) + 1 );
			}
		}
	});

	me.tap = function (e, eventName) {
		var ev = document.createEvent('Event');
		ev.initEvent(eventName, true, true);
		ev.pageX = e.pageX;
		ev.pageY = e.pageY;
		e.target.dispatchEvent(ev);
	};

	me.click = function (e) {
		var target = e.target,
			ev;

		if ( !(/(SELECT|INPUT|TEXTAREA)/i).test(target.tagName) ) {
			ev = document.createEvent('MouseEvents');
			ev.initMouseEvent('click', true, true, e.view, 1,
				target.screenX, target.screenY, target.clientX, target.clientY,
				e.ctrlKey, e.altKey, e.shiftKey, e.metaKey,
				0, null);

			ev._constructed = true;
			target.dispatchEvent(ev);
		}
	};

	return me;
})();

function IScroll (el, options) {
	this.wrapper = typeof el == 'string' ? document.querySelector(el) : el;
	this.scroller = this.wrapper.children[0];
	this.scrollerStyle = this.scroller.style;
	// cache style for better performance

	/*自定义的一些属性*/
	var scroller = this.scroller;
	scroller.iscroll = this;
	utils.addClass(scroller,"iScroll-instance");
	/*自定义的一些属性*/
	
	this.options = {
		resizeScrollbars: true,
		mouseWheelSpeed: 20,
		// INSERT POINT: OPTIONS 

		startX: 0,
		startY: 0,
		scrollY: true,
		directionLockThreshold: 5,
		momentum: true,

		bounce: true,
		bounceTime: 600,
		bounceEasing: '',

		preventDefault: true,
		preventDefaultException: { tagName: /^(INPUT|TEXTAREA|BUTTON|SELECT)$/ },

		HWCompositing: true,
		useTransition: true,
		useTransform: true
	};

	for ( var i in options ) {
		this.options[i] = options[i];
	}

	options = this.options;
	
	// Normalize options
	this.translateZ = options.HWCompositing && utils.hasPerspective ? ' ' : '';

	options.useTransition = utils.hasTransition && options.useTransition;
	options.useTransform = utils.hasTransform && options.useTransform;

	options.eventPassthrough = options.eventPassthrough === true ? 'vertical' : options.eventPassthrough;
	options.preventDefault = !options.eventPassthrough && options.preventDefault;

	// If you want eventPassthrough I have to lock one of the axes
	options.scrollY = options.eventPassthrough == 'vertical' ? false : options.scrollY;
	options.scrollX = options.eventPassthrough == 'horizontal' ? false : options.scrollX;

	// With eventPassthrough we also need lockDirection mechanism
	options.freeScroll = options.freeScroll && !options.eventPassthrough;
	options.directionLockThreshold = options.eventPassthrough ? 0 : options.directionLockThreshold;

	options.bounceEasing = typeof options.bounceEasing == 'string' ? utils.ease[options.bounceEasing] || utils.ease.circular : options.bounceEasing;

	options.resizePolling = options.resizePolling === undefined ? 60 : options.resizePolling;

	if ( options.tap === true ) {
		options.tap = 'tap';
	}

	if ( options.shrinkScrollbars == 'scale' ) {
		options.useTransition = false;
	}

	options.invertWheelDirection = options.invertWheelDirection ? -1 : 1;

	if ( options.probeType == 3 ) {
		options.useTransition = false;	
	}

	// INSERT POINT: NORMALIZATION

	// Some defaults	
	this.x = 0;
	this.y = 0;
	this.directionX = 0;
	this.directionY = 0;
	this._events = {};

// INSERT POINT: DEFAULTS

	this._init();
	this.refresh();

	this.scrollTo(options.startX, options.startY);
	this.enable();
}

IScroll.prototype = {
	
	version: '5.1.3',

	_init: function () {
		var options = this.options;
		this._initEvents();

		if ( options.mouseWheel ) {
			this._initWheel();
		}
		
		if ( options.keyBindings ) {
			this._initKeys();
		}
		// INSERT POINT: _init
	},

	destroy: function () {
		this._initEvents(true);

		this._execEvent('destroy');
	},

	_transitionEnd: function (e) {
		if ( e.target != this.scroller || !this.isInTransition ) {
			return;
		}

		this._transitionTime();
		if ( !this.resetPosition(this.options.bounceTime) ) {
			this.isInTransition = false;
			this._execEvent('scrollEnd');
		}
	},

	_start: function (e) {
		var options = this.options,
			eventType = utils.eventType;
		// React to left mouse button only
		if ( eventType[e.type] != 1 ) {
			if ( e.button !== 0 ) {
				return;
			}
		}

		if ( !this.enabled || (this.initiated && eventType[e.type] !== this.initiated) ) {
			return;
		}

		if ( options.preventDefault && !utils.isBadAndroid && !utils.preventDefaultException(e.target, options.preventDefaultException) ) {
			e.preventDefault();
		}

		var point = e.touches ? e.touches[0] : e,
			pos;

		this.initiated	= eventType[e.type];
		this.moved		= false;
		this.distX		= 0;
		this.distY		= 0;
		this.directionX = 0;
		this.directionY = 0;
		this.directionLocked = 0;

		this._transitionTime();

		this.startTime = utils.getTime();

		if ( options.useTransition && this.isInTransition ) {
			this.isInTransition = false;
			pos = this.getComputedPosition();
			this._translate(Math.round(pos.x), Math.round(pos.y));
			this._execEvent('scrollEnd');
		} else if ( !options.useTransition && this.isAnimating ) {
			this.isAnimating = false;
			this._execEvent('scrollEnd');
		}

		this.startX    = this.x;
		this.startY    = this.y;
		this.absStartX = this.x;
		this.absStartY = this.y;
		this.pointX    = point.pageX;
		this.pointY    = point.pageY;

		this.target = e.target;
		
		this._execEvent('beforeScrollStart');
		
	},

	_move: function (e) {
		if ( !this.enabled || utils.eventType[e.type] !== this.initiated ) {
			return;
		}
		var options = this.options;
		if ( options.preventDefault ) {	// increases performance on Android? TODO: check!
			e.preventDefault();
		}

		var point		= e.touches ? e.touches[0] : e,
			deltaX		= point.pageX - this.pointX,
			deltaY		= point.pageY - this.pointY,
			timestamp	= utils.getTime(),
			newX, newY,
			absDistX, absDistY;

		this.pointX		= point.pageX;
		this.pointY		= point.pageY;

		this.distX		+= deltaX;
		this.distY		+= deltaY;
		absDistX		= Math.abs(this.distX);
		absDistY		= Math.abs(this.distY);

		// We need to move at least 10 pixels for the scrolling to initiate
		if ( timestamp - this.endTime > 300 && (absDistX < 10 && absDistY < 10) ) {
			return;
		}

		// If you are scrolling in one direction lock the other
		if ( !this.directionLocked && !options.freeScroll ) {
			if ( absDistX > absDistY + options.directionLockThreshold ) {
				this.directionLocked = 'h';		// lock horizontally
			} else if ( absDistY >= absDistX + options.directionLockThreshold ) {
				this.directionLocked = 'v';		// lock vertically
			} else {
				this.directionLocked = 'v';		// no lock
			}
		}

		if ( this.directionLocked == 'h' ) {
			if ( options.eventPassthrough == 'vertical' ) {
				e.preventDefault();
			} else if ( options.eventPassthrough == 'horizontal' ) {
				this.initiated = false;
				return;
			}
			deltaY = 0;
		} else if ( this.directionLocked == 'v' ) {
			if ( options.eventPassthrough == 'horizontal' ) {
				e.preventDefault();
			} else if ( options.eventPassthrough == 'vertical' ) {
				this.initiated = false;
				return;
			}

			deltaX = 0;
		}

		deltaX = this.hasHorizontalScroll ? deltaX : 0;
		deltaY = this.hasVerticalScroll ? deltaY : 0;

		newX = this.x + deltaX;
		newY = this.y + deltaY;

		// Slow down if outside of the boundaries
		if ( newX > 0 || newX < this.maxScrollX ) {
			newX = options.bounce ? this.x + deltaX / 3 : newX > 0 ? 0 : this.maxScrollX;
		}
		if ( newY > 0 || newY < this.maxScrollY ) {
			newY = options.bounce ? this.y + deltaY / 3 : newY > 0 ? 0 : this.maxScrollY;
		}

		this.directionX = deltaX > 0 ? -1 : deltaX < 0 ? 1 : 0;
		this.directionY = deltaY > 0 ? -1 : deltaY < 0 ? 1 : 0;

		if ( !this.moved ) {
			this._execEvent('scrollStart');
		}
		
		this.moved = true;
		
		/*自定义的一些判断*/
		var target = this.target || e.target,
			scroller = this.scroller,
			iscroll = null,
			isY = 0,
			maxY = 0;
			
		while(target && !utils.hasClass(target,"iScroll-instance")){
			//肯定会包含该className的值，所以这里不用担心会出现死循环
			target = target.parentNode;
		}
		if(utils.hasClass(target,"iScroll-instance")){
			iscroll = target.iscroll;
			isY = iscroll.y;
			maxY = iscroll.maxScrollY - isY;
			
			if(target !== scroller && ( (isY < 0 && maxY < 0) || (this.directionLocked == 'h'))){
				//如果是上下滚动的，则滚动到结尾，就执行外层的滚动
				//而在左右，则不执行。
				return "";
			}
		}
		
		/*自定义的一些判断*/
		
		this._translate(newX, newY);

/* REPLACE START: _move */

		if ( timestamp - this.startTime > 300 ) {
			this.startTime = timestamp;
			this.startX = this.x;
			this.startY = this.y;
			
			if ( options.probeType == 1 ) {
				this._execEvent('scroll');
			}
		}

		if ( options.probeType > 1 ) {
			this._execEvent('scroll');
		}
		/* REPLACE END: _move */

	},

	silieUpDown:function(){
		var y = this.y;
		
		if ( !this.hasVerticalScroll || y > 0 ) {
			this._execEvent('slideDown');
		} else if ( y < this.maxScrollY ) {
			this._execEvent('slideUp');
		}
	},
	
	_end: function (e) {
		
		var options = this.options;
		this.target = null;
		
		if ( !this.enabled || utils.eventType[e.type] !== this.initiated ) {
			return;
		}

		if ( options.preventDefault && !utils.preventDefaultException(e.target, options.preventDefaultException) ) {
			e.preventDefault();
		}

		var point = e.changedTouches ? e.changedTouches[0] : e,
			momentumX,
			momentumY,
			duration = utils.getTime() - this.startTime,
			newX = Math.round(this.x),
			newY = Math.round(this.y),
			distanceX = Math.abs(newX - this.startX),
			distanceY = Math.abs(newY - this.startY),
			time = 0,
			easing = '';

		this.isInTransition = 0;
		this.initiated = 0;
		this.endTime = utils.getTime();

		// reset if we are outside of the boundaries
		if ( this.resetPosition(options.bounceTime)) {
			this.silieUpDown();
			return;
		}

		this.scrollTo(newX, newY);	// ensures that the last position is rounded

		// we scrolled less than 10 pixels
		if ( !this.moved ) {
			if ( options.tap ) {
				utils.tap(e, options.tap);
			}

			if ( options.click ) {
				utils.click(e);
			}

			this._execEvent('scrollCancel');
			return;
		}

		if ( this._events.flick && duration < 200 && distanceX < 100 && distanceY < 100 ) {
			this._execEvent('flick');
			return;
		}

		// start momentum animation if needed
		if ( options.momentum && duration < 300 ) {
			momentumX = this.hasHorizontalScroll ? utils.momentum(this.x, this.startX, duration, this.maxScrollX, options.bounce ? this.wrapperWidth : 0, options.deceleration) : { destination: newX, duration: 0 };
			momentumY = this.hasVerticalScroll ? utils.momentum(this.y, this.startY, duration, this.maxScrollY, options.bounce ? this.wrapperHeight : 0, options.deceleration) : { destination: newY, duration: 0 };
			newX = momentumX.destination;
			newY = momentumY.destination;
			time = Math.max(momentumX.duration, momentumY.duration);
			this.isInTransition = 1;
		}

		// INSERT POINT: _end
		
		if ( newX != this.x || newY != this.y ) {
			// change easing function when scroller goes out of the boundaries
			if ( newX > 0 || newX < this.maxScrollX || newY > 0 || newY < this.maxScrollY ) {
				easing = utils.ease.quadratic;
			}

			this.scrollTo(newX, newY, time, easing);
			return;
		}

		this._execEvent('scrollEnd');
	},

	_resize: function () {
		var that = this;

		clearTimeout(this.resizeTimeout);

		this.resizeTimeout = setTimeout(function () {
			that.refresh();
		}, this.options.resizePolling);
	},

	resetPosition: function (time) {
		var x = this.x,
			y = this.y;

		time = time || 0;

		if ( !this.hasHorizontalScroll || this.x > 0 ) {
			x = 0;
		} else if ( this.x < this.maxScrollX ) {
			x = this.maxScrollX;
		}

		if ( !this.hasVerticalScroll || this.y > 0 ) {
			y = 0;
		} else if ( this.y < this.maxScrollY ) {
			y = this.maxScrollY;
		}

		if ( x == this.x && y == this.y ) {
			return false;
		}

		this.scrollTo(x, y, time, this.options.bounceEasing);

		return true;
	},

	disable: function () {
		this.enabled = false;
	},

	enable: function () {
		this.enabled = true;
	},

	refresh: function () {
		var wrapper = this.wrapper,
			rf = wrapper.offsetHeight;		// Force reflow

		this.wrapperWidth	= wrapper.clientWidth;
		this.wrapperHeight	= wrapper.clientHeight;

/* REPLACE START: refresh */

		this.scrollerWidth	= this.scroller.offsetWidth;
		this.scrollerHeight	= this.scroller.offsetHeight;

		this.maxScrollX		= this.wrapperWidth - this.scrollerWidth;
		this.maxScrollY		= this.wrapperHeight - this.scrollerHeight;

/* REPLACE END: refresh */

		this.hasHorizontalScroll	= this.options.scrollX && this.maxScrollX < 0;
		this.hasVerticalScroll		= this.options.scrollY && this.maxScrollY < 0;

		if ( !this.hasHorizontalScroll ) {
			this.maxScrollX = 0;
			this.scrollerWidth = this.wrapperWidth;
		}

		if ( !this.hasVerticalScroll ) {
			this.maxScrollY = 0;
			this.scrollerHeight = this.wrapperHeight;
		}

		this.endTime = 0;
		this.directionX = 0;
		this.directionY = 0;

		this.wrapperOffset = utils.offset(wrapper);

		this._execEvent('refresh');

		this.resetPosition();

	},

	on: function (type, fn) {
		if ( !this._events[type] ) {
			this._events[type] = [];
		}

		this._events[type].push(fn);
	},

	off: function (type, fn) {
		var typeList = this._events[type];
		if ( !typeList ) {
			return;
		}

		var index = typeList.indexOf(fn);

		if ( index > -1 ) {
			typeList.splice(index, 1);
		}
	},

	_execEvent: function (type) {
		var typeList = this._events[type];
		if ( !typeList ) {
			return;
		}

		var i = 0,
			l = typeList.length;

		if ( !l ) {
			return;
		}

		for ( ; i < l; i++ ) {
			typeList[i].apply(this, [].slice.call(arguments, 1));
		}
		
	},

	scrollBy: function (x, y, time, easing) {
		x = this.x + x;
		y = this.y + y;
		time = time || 0;

		this.scrollTo(x, y, time, easing);
	},

	scrollTo: function (x, y, time, easing) {
		var useTransition = this.options.useTransition;
		easing = easing || utils.ease.circular;
		this.isInTransition = useTransition && time > 0;

		if ( !time || (useTransition && easing.style) ) {
			this._transitionTime(time);
			this._translate(x, y);
		} else {
			this._animate(x, y, time, easing.fn);
		}
	},

	scrollToElement: function (el, time, offsetX, offsetY, easing) {
		el = el.nodeType ? el : this.scroller.querySelector(el);

		if ( !el ) {
			return;
		}

		var pos = utils.offset(el);

		pos.left -= this.wrapperOffset.left;
		pos.top  -= this.wrapperOffset.top;

		// if offsetX/Y are true we center the element to the screen
		if ( offsetX === true ) {
			offsetX = Math.round(el.offsetWidth / 2 - this.wrapper.offsetWidth / 2);
		}
		if ( offsetY === true ) {
			offsetY = Math.round(el.offsetHeight / 2 - this.wrapper.offsetHeight / 2);
		}

		pos.left -= offsetX || 0;
		pos.top  -= offsetY || 0;

		pos.left = pos.left > 0 ? 0 : pos.left < this.maxScrollX ? this.maxScrollX : pos.left;
		pos.top  = pos.top  > 0 ? 0 : pos.top  < this.maxScrollY ? this.maxScrollY : pos.top;

		time = time === undefined || time === null || time === 'auto' ? Math.max(Math.abs(this.x-pos.left), Math.abs(this.y-pos.top)) : time;

		this.scrollTo(pos.left, pos.top, time, easing);
	},

	_transitionTime: function (time) {
		time = time || 0;

		this.scrollerStyle[utils.style.transitionDuration] = time + 'ms';

		if ( !time && utils.isBadAndroid ) {
			this.scrollerStyle[utils.style.transitionDuration] = '0.001s';
		}
		
	},
	
	_translate: function (x, y) {
		if ( this.options.useTransform ) {

/* REPLACE START: _translate */

			this.scrollerStyle[utils.style.transform] = 'translate(' + x + 'px,' + y + 'px)' + this.translateZ;

/* REPLACE END: _translate */

		} else {
			x = Math.round(x);
			y = Math.round(y);
			this.scrollerStyle.left = x + 'px';
			this.scrollerStyle.top = y + 'px';
		}

		this.x = x;
		this.y = y;

	},

	_initEvents: function (remove) {
		var eventType = remove ? utils.removeEvent : utils.addEvent,
			target = this.options.bindToWrapper ? this.wrapper : window;

		eventType(window, 'orientationchange', this);
		eventType(window, 'resize', this);

		if ( this.options.click ) {
			eventType(this.wrapper, 'click', this, true);
		}

		if ( !this.options.disableMouse ) {
			eventType(this.wrapper, 'mousedown', this);
			eventType(target, 'mousemove', this);
			eventType(target, 'mousecancel', this);
			eventType(target, 'mouseup', this);
		}

		if ( utils.hasPointer && !this.options.disablePointer ) {
			eventType(this.wrapper, utils.prefixPointerEvent('pointerdown'), this);
			eventType(target, utils.prefixPointerEvent('pointermove'), this);
			eventType(target, utils.prefixPointerEvent('pointercancel'), this);
			eventType(target, utils.prefixPointerEvent('pointerup'), this);
		}

		if ( utils.hasTouch && !this.options.disableTouch ) {
			eventType(this.wrapper, 'touchstart', this);
			eventType(target, 'touchmove', this);
			eventType(target, 'touchcancel', this);
			eventType(target, 'touchend', this);
		}

		eventType(this.scroller, 'transitionend', this);
		eventType(this.scroller, 'webkitTransitionEnd', this);
		eventType(this.scroller, 'oTransitionEnd', this);
		eventType(this.scroller, 'MSTransitionEnd', this);
	},

	getComputedPosition: function () {
		var matrix = window.getComputedStyle(this.scroller, null),
			x, y;

		if ( this.options.useTransform ) {
			matrix = matrix[utils.style.transform].split(')')[0].split(', ');
			x = +(matrix[12] || matrix[4]);
			y = +(matrix[13] || matrix[5]);
		} else {
			x = +matrix.left.replace(/[^-\d.]/g, '');
			y = +matrix.top.replace(/[^-\d.]/g, '');
		}

		return { x: x, y: y };
	},
	
	_initWheel: function () {
		var addEvent = utils.addEvent,
			removeEvent = utils.removeEvent;
			
		addEvent(this.wrapper, 'wheel', this);
		addEvent(this.wrapper, 'mousewheel', this);
		addEvent(this.wrapper, 'DOMMouseScroll', this);

		this.on('destroy', function () {
			removeEvent(this.wrapper, 'wheel', this);
			removeEvent(this.wrapper, 'mousewheel', this);
			removeEvent(this.wrapper, 'DOMMouseScroll', this);
		});
	},

	_wheel: function (e) {
		if ( !this.enabled ) {
			return;
		}

		e.preventDefault();
		e.stopPropagation();

		var wheelDeltaX, wheelDeltaY,
			newX, newY,
			that = this;

		if ( this.wheelTimeout === undefined ) {
			that._execEvent('scrollStart');
		}

		// Execute the scrollEnd event after 400ms the wheel stopped scrolling
		clearTimeout(this.wheelTimeout);
		this.wheelTimeout = setTimeout(function () {
			that._execEvent('scrollEnd');
			that.wheelTimeout = undefined;
		}, 400);

		if ( 'deltaX' in e ) {
			if (e.deltaMode === 1) {
				wheelDeltaX = -e.deltaX * this.options.mouseWheelSpeed;
				wheelDeltaY = -e.deltaY * this.options.mouseWheelSpeed;
			} else {
				wheelDeltaX = -e.deltaX;
				wheelDeltaY = -e.deltaY;
			}
		} else if ( 'wheelDeltaX' in e ) {
			wheelDeltaX = e.wheelDeltaX / 120 * this.options.mouseWheelSpeed;
			wheelDeltaY = e.wheelDeltaY / 120 * this.options.mouseWheelSpeed;
		} else if ( 'wheelDelta' in e ) {
			wheelDeltaX = wheelDeltaY = e.wheelDelta / 120 * this.options.mouseWheelSpeed;
		} else if ( 'detail' in e ) {
			wheelDeltaX = wheelDeltaY = -e.detail / 3 * this.options.mouseWheelSpeed;
		} else {
			return;
		}

		wheelDeltaX *= this.options.invertWheelDirection;
		wheelDeltaY *= this.options.invertWheelDirection;

		if ( !this.hasVerticalScroll ) {
			wheelDeltaX = wheelDeltaY;
			wheelDeltaY = 0;
		}

		newX = this.x + Math.round(this.hasHorizontalScroll ? wheelDeltaX : 0);
		newY = this.y + Math.round(this.hasVerticalScroll ? wheelDeltaY : 0);

		if ( newX > 0 ) {
			newX = 0;
		} else if ( newX < this.maxScrollX ) {
			newX = this.maxScrollX;
		}

		if ( newY > 0 ) {
			newY = 0;
		} else if ( newY < this.maxScrollY ) {
			newY = this.maxScrollY;
		}

		this.scrollTo(newX, newY, 0);

		if ( this.options.probeType > 1 ) {
			this._execEvent('scroll');
		}

// INSERT POINT: _wheel
	},
	
	_initKeys: function (e) {
		// default key bindings
		var keys = {
			pageUp: 33,
			pageDown: 34,
			end: 35,
			home: 36,
			left: 37,
			up: 38,
			right: 39,
			down: 40
		};
		var i,
			options = this.options,
			keyBindings = options.keyBindings;

		// if you give me characters I give you keycode
		if ( typeof keyBindings == 'object' ) {
			for ( i in keyBindings ) {
				if ( typeof keyBindings[i] == 'string' ) {
					keyBindings[i] = keyBindings[i].toUpperCase().charCodeAt(0);
				}
			}
		} else {
			options.keyBindings = keyBindings = {};
		}

		for ( i in keys ) {
			keyBindings[i] = keyBindings[i] || keys[i];
		}

		utils.addEvent(window, 'keydown', this);

		this.on('destroy', function () {
			utils.removeEvent(window, 'keydown', this);
		});
	},

	_key: function (e) {
		if ( !this.enabled ) {
			return;
		}

		var options = this.options,
			newX = this.x,
			newY = this.y,
			now = utils.getTime(),
			prevTime = this.keyTime || 0,
			acceleration = 0.250,
			pos,
			keyBindings = options.keyBindings;

		if ( options.useTransition && this.isInTransition ) {
			pos = this.getComputedPosition();

			this._translate(Math.round(pos.x), Math.round(pos.y));
			this.isInTransition = false;
		}

		this.keyAcceleration = now - prevTime < 200 ? Math.min(this.keyAcceleration + acceleration, 50) : 0;

		switch ( e.keyCode ) {
			case keyBindings.pageUp:
				if ( this.hasHorizontalScroll && !this.hasVerticalScroll ) {
					newX += this.wrapperWidth;
				} else {
					newY += this.wrapperHeight;
				}
				break;
			case keyBindings.pageDown:
				if ( this.hasHorizontalScroll && !this.hasVerticalScroll ) {
					newX -= this.wrapperWidth;
				} else {
					newY -= this.wrapperHeight;
				}
				break;
			case keyBindings.end:
				newX = this.maxScrollX;
				newY = this.maxScrollY;
				break;
			case keyBindings.home:
				newX = 0;
				newY = 0;
				break;
			case keyBindings.left:
				newX += 5 + this.keyAcceleration>>0;
				break;
			case keyBindings.up:
				newY += 5 + this.keyAcceleration>>0;
				break;
			case keyBindings.right:
				newX -= 5 + this.keyAcceleration>>0;
				break;
			case keyBindings.down:
				newY -= 5 + this.keyAcceleration>>0;
				break;
			default:
				return;
		}

		if ( newX > 0 ) {
			newX = 0;
			this.keyAcceleration = 0;
		} else if ( newX < this.maxScrollX ) {
			newX = this.maxScrollX;
			this.keyAcceleration = 0;
		}

		if ( newY > 0 ) {
			newY = 0;
			this.keyAcceleration = 0;
		} else if ( newY < this.maxScrollY ){
			newY = this.maxScrollY;
			this.keyAcceleration = 0;
		}

		this.scrollTo(newX, newY, 0);

		this.keyTime = now;
	},

	_animate: function (destX, destY, duration, easingFn) {
		var that = this,
			startX = this.x,
			startY = this.y,
			startTime = utils.getTime(),
			destTime = startTime + duration;

		function step () {
			var now = utils.getTime(),
				newX, newY,
				easing;

			if ( now >= destTime ) {
				that.isAnimating = false;
				that._translate(destX, destY);

				if ( !that.resetPosition(that.options.bounceTime) ) {
					that._execEvent('scrollEnd');
				}

				return;
			}

			now = ( now - startTime ) / duration;
			easing = easingFn(now);
			newX = ( destX - startX ) * easing + startX;
			newY = ( destY - startY ) * easing + startY;
			that._translate(newX, newY);

			if ( that.isAnimating ) {
				rAF(step);
			}

			if ( that.options.probeType == 3 ) {
				that._execEvent('scroll');
			}
		}

		this.isAnimating = true;
		step();
	},
	
	handleEvent: function (e) {
		switch ( e.type ) {
			case 'touchstart':
			case 'pointerdown':
			case 'MSPointerDown':
			case 'mousedown':
				this._start(e);
				break;
			case 'touchmove':
			case 'pointermove':
			case 'MSPointerMove':
			case 'mousemove':
				this._move(e);
				break;
			case 'touchend':
			case 'pointerup':
			case 'MSPointerUp':
			case 'mouseup':
			case 'touchcancel':
			case 'pointercancel':
			case 'MSPointerCancel':
			case 'mousecancel':
				this._end(e);
				break;
			case 'orientationchange':
			case 'resize':
				this._resize();
				break;
			case 'transitionend':
			case 'webkitTransitionEnd':
			case 'oTransitionEnd':
			case 'MSTransitionEnd':
				this._transitionEnd(e);
				break;
			case 'wheel':
			case 'DOMMouseScroll':
			case 'mousewheel':
				this._wheel(e);
				break;
			case 'keydown':
				this._key(e);
				break;
			case 'click':
				if ( !e._constructed ) {
					e.preventDefault();
					e.stopPropagation();
				}
				break;
		}
	}
	
};
function createDefaultScrollbar (direction, interactive, type) {
	var scrollbar = document.createElement('div');

	if ( type === true ) {
		scrollbar.style.cssText = 'position:absolute;z-index:9999';
	}

	if ( direction == 'h' ) {
		if ( type === true ) {
			scrollbar.style.cssText += ';height:7px;left:2px;right:2px;bottom:0';
		}
		scrollbar.className = 'iScrollHorizontalScrollbar';
	} else {
		if ( type === true ) {
			scrollbar.style.cssText += ';width:7px;bottom:2px;top:2px;right:1px';
		}
		scrollbar.className = 'iScrollVerticalScrollbar';
	}

	scrollbar.style.cssText += ';overflow:hidden';

	if ( !interactive ) {
		scrollbar.style.pointerEvents = 'none';
	}

	return scrollbar;
}

if ( typeof module != 'undefined' && module.exports ) {
	module.exports = IScroll;
} else {
	window.IScroll = IScroll;
}

})(window, document, Math);

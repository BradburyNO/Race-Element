import{ɵ as m,a as c,i as _,M as u,b as g,c as w,d as p,e as f,f as h,A as b,g as x,C,D as v,h as l,j as s,k as o,l as T,m as i,n as N,t as S,u as k,o as y}from"./index-CgZtM99z.js";function M(t,a){if(t&1&&(l(0,"article",0)(1,"h1",1)(2,"a",2),s(3,"News"),o(),s(4),o(),l(5,"div",3)(6,"p",4),s(7),p(8,"date"),o(),T(9,"analog-markdown",5),o()()),t&2){const n=a;i(4),N(" > ",n.attributes.title," "),i(3),S(n.attributes.date!==void 0?k(8,3,n.attributes.date,"longDate"):""),i(2),y("content",n.content)}}let E=(()=>{var t;class a{constructor(e){this.meta=e,this.post=_()}ngOnInit(){this.post.forEach(e=>{this.meta.removeTag("og:url"),this.meta.removeTag("og:title"),this.meta.removeTag("og:description"),this.meta.removeTag("twitter:title"),this.meta.addTag({name:"og:url",content:`https://race.elementfuture.com/news/${e.attributes.slug}`}),this.meta.addTag({name:"og:title",content:`Race Element - News | ${e.attributes.title}`}),this.meta.addTag({name:"og:description",content:`${e.attributes.description}`}),this.meta.addTag({name:"twitter:title",content:`Race Element - News | ${e.attributes.title}`})})}}return t=a,t.ɵfac=function(e){return new(e||t)(m(u))},t.ɵcmp=c({type:t,selectors:[["app-news-post"]],standalone:!0,features:[g],decls:2,vars:3,consts:[[1,"rounded-lg","container","mx-auto","max-w-4xl","px-3"],[1,"text-xl","md:text-3xl","font-['Conthrax']","select-none","dark:text-gray-300","dark:bg-black","rounded-tl-xl","border-l-2","pl-2","pr-2","pt-1","pb-1","border-red-800"],["href","/news"],[1,"container","dark:bg-[#050505]","pl-3","pr-[1em]","pt-2","rounded-br-xl"],[1,"select-none","text-sm","mb-3"],[1,"whitespace-pre-line",3,"content"]],template:function(e,d){if(e&1&&(w(0,M,10,6,"article",0),p(1,"async")),e&2){let r;f((r=h(1,1,d.post))?0:-1,r)}},dependencies:[b,x,C,v],encapsulation:2}),a})();export{E as default};

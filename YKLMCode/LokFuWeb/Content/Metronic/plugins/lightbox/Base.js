/*
	Date：2011/10/23 12:58
	Author：Await
	AuthorUrl: http://leotheme.cn/
*/
if (typeof Util == "undefined") var Util = {};
var U = Util, path = "";
//配置信息
Util.Config = {
    loadingICO: path + "/css/loading.gif"			//默认载入时图片路径
};
//获取标签的Name
Util.getName = function (a) {
    return document.getElementsByName(a);
};
//用ID获取对象
Util.getID = function (id) {
    return document.getElementById(id);
};
// 用标签获取对象
Util.getTag = function (tag) {
    return document.getElementsByTagName(tag);
};
//创建文本节点
Util.ct = function (txt) {
    return document.createTextNode(txt);
};
//创建xhtml元素节点
Util.ce = function (name) {
    return document.createElement(name);
};
// 阻止事件冒泡
Util.stopBubble = function (e) {
    e.stopPropagation ? e.stopPropagation() : e.cancelBubble = true;
}
// 阻止浏览器默认行为
Util.stopDefault = function (e) {
    e.preventDefault ? e.preventDefault() : e.returnValue = false;
}
//获取当前样式
Util.getStyle = function (o, key) {
    return o.currentStyle ? o.currentStyle[key] : document.defaultView.getComputedStyle(o, null)[key];
}
/*
	绑定事件
	obj	 对像
	type  事件名称
	fn    回调函数
	2011年10月27日 17:02
*/
Util.bind = function (obj, type, fn) {
    if (obj.attachEvent) {
        obj['e' + type + fn] = fn;
        obj[type + fn] = function () { obj['e' + type + fn](window.event); }
        obj.attachEvent('on' + type, obj[type + fn]);
    } else {
        obj.addEventListener(type, fn, false);
    };
}
/*
	移除事件
	obj	 对像
	type  事件名称
	fn    回调函数
	2011年10月27日 17:05
*/
Util.unbind = function (obj, type, fn) {
    if (obj.detachEvent) {
        try {
            obj.detachEvent('on' + type, obj[type + fn]);
            obj[type + fn] = null;
        } catch (_) { };
    } else {
        obj.removeEventListener(type, fn, false);
    };
}
/*
	判断某个ID是否已存在
	name	 对像
	2011年5月20日 17:02
*/
Util.Eid = function (name) {
    var s = Util.getID(name);
    if (s) { return true; } else { return false; }
};
/*
	判断浏览器及版本
	2011年5月20日 17:01
*/
Util.Browser = function () {
    var a = navigator.userAgent.toLowerCase();
    var b = {};
    b.isStrict = document.compatMode == "CSS1Compat";
    b.isFirefox = a.indexOf("firefox") > -1;
    b.isOpera = a.indexOf("opera") > -1;
    b.isSafari = (/webkit|khtml/).test(a);
    b.isSafari3 = b.isSafari && a.indexOf("webkit/5") != -1;
    b.isIE = !b.isOpera && a.indexOf("msie") > -1;
    b.isIE6 = !b.isOpera && a.indexOf("msie 6") > -1;
    b.isIE7 = !b.isOpera && a.indexOf("msie 7") > -1;
    b.isIE8 = !b.isOpera && a.indexOf("msie 8") > -1;
    b.isGecko = !b.isSafari && a.indexOf("gecko") > -1;
    b.isMozilla = document.all != undefined && document.getElementById != undefined && !window.opera != undefined;
    return b
}();
/*
	判断浏览器是否启用COOKIE
	2011年5月20日 17:01
*/
Util.CookieEnable = function () {
    var a = false;
    if (navigator.cookiesEnabled) return true;
    document.cookie = "b=yes;";
    var c = document.cookie;
    if (c.indexOf("b=yes") > -1) a = true;
    document.cookie = "";
    return a;
    //isCookie: navigator.cookieEnabled ? !0: !1
};
/*
	获取页面大小相关信息
	get	获取方法 var a = Util.pageSize.get();
	2011年5月25日 17:01
*/
Util.pageSize = {
    get: function () {
        var a = Util.Browser.isStrict ? document.documentElement : document.body;
        var b = ["clientWidth", "clientHeight", "scrollWidth", "scrollHeight"];
        var c = {};
        for (var d in b) c[b[d]] = a[b[d]];
        c.scrollLeft = document.body.scrollLeft || document.documentElement.scrollLeft;
        c.scrollTop = document.body.scrollTop || document.documentElement.scrollTop;
        return c
    }
};
/*
	获取DOM位置信息
	obj		对像
	parent	父级节点
	2011年5月20日17:01
*/
Util.getPosition = function (obj, parent) {
    if (typeof obj == "string") obj = Util.getID(obj);
    else if (parent) obj = Util.getID(obj.id) || obj.get(0);
    var c = 0;
    var d = 0;
    var f = obj.offsetWidth;
    var g = obj.offsetHeight;
    do {
        d += obj.offsetTop || 0;
        c += obj.offsetLeft || 0;
        obj = obj.offsetParent
    }
    while (obj) return {
        x: c,
        y: d,
        width: f,
        height: g
    }
};
/*
	计算安全范围
	obj	对像
	2011年5月20日 17:01
*/
Util.safeRange = function (obj) {
    var b = $("#" + obj);
    var c, d, e, f, g, h, j, k;
    var s = Util.pageSize.get();
    st = s.scrollTop;
    j = b.outerWidth();
    k = b.outerHeight();
    p = Util.pageSize.get();
    c = 0;
    e = p.clientWidth - j;
    g = e / 2;
    d = 0;
    f = p.clientHeight - k;
    // 小对话框在视觉黄金比例垂直居中，大对话框绝对居中
    var hc = p.clientHeight * 0.382 - k / 2;
    h = (k < p.clientHeight / 2) ? hc : f / 2;
    if (g < 0) g = 0;
    if (h < 0) h = 0;
    return { width: j, height: k, minX: c, minY: d, maxX: e, maxY: f, centerX: g, centerY: h };
};
/*
	设定对像位置
	obj		对像
	offsets	位置
	referID	对像
	2012年3月5日 9:27:43
*/
Util.setXY = function (obj, offsets, referID, auto) {
    var p = Util.pageSize.get(), s = Util.safeRange(obj), D = $("#" + obj);
    var _this = offsets || { left: s.centerX, top: s.centerY }
    if (referID != undefined && referID != "") {
        var rp = Util.getPosition(referID, "html");
        var left = !_this.right ? parseInt(_this.left) : p.clientWidth - s.width - parseInt(_this.right);
        var top = !_this.bottom ? parseInt(_this.top) : p.clientHeight - s.height - parseInt(_this.bottom);
        //$("#depDateId").val(rp.x+"--"+rp.y+"--"+referID.val())
        left1 = rp.x + parseInt(_this.left); //inside
        left2 = rp.x - parseInt(_this.left) - s.width; //outside
        right1 = rp.x + rp.width - parseInt(_this.right) - s.width; //inside
        right2 = rp.x + rp.width + parseInt(_this.right); //outside
        top1 = rp.y + parseInt(top); //inside
        top2 = rp.y - parseInt(top) - s.height; //outside
        bottom1 = rp.y + rp.height - parseInt(_this.bottom) - s.height; //inside
        bottom2 = rp.y + rp.height + parseInt(_this.bottom); //outside
        left = !_this.right ? (_this.linside ? left1 : left2) : (_this.rinside ? right1 : right2);
        top = !_this.bottom ? (_this.tinside ? top1 : top2) : (_this.binside ? bottom1 : bottom2);
        if (auto) {
            left = p.clientWidth - left < s.width ? left + rp.width - s.width : left;
            top = p.clientHeight - top < s.height ? top - rp.height - s.height : top;
        }
        D.css({ left: left, top: top, zIndex: "891208" });
    } else {
        var left = !_this.right ? "left" : "right", top = !_this.bottom ? "top" : "bottom";
        D.css(left, _this.left || _this.right).css(top, _this.top || _this.bottom).css("zIndex", "891208");
    }
};
/*
	iframe自适应高度 
	obj	对像
	2011年10月28日 17:01
*/
Util.setIframHeight = function (obj) {
    var fun = function (obj) {
        var o = document.getElementById(obj);
        try {
            var a = o.contentWindow.document.body.scrollHeight;
            var b = o.contentWindow.document.documentElement.scrollHeight;
            var h = Math.max(a, b);
            o.height = h;
        } catch (ex) { }
    }
    window.setInterval(fun, 200);
}
String.prototype.IsNotEmpty = function () {
    if (this.replace(/\s+/, '').length > 0) {
        if (arguments.length == 1) {
            return this.indexOf(arguments[0]) == -1;
        }
        return true;
    }
    return false;
}
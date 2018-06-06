/*
	拖拽
	2012年2月14日 15:14:13
*/
;(function($){
Util.drag = function (o) {
    var defaults = {
        obj: "", //拖动手柄[必须参数]
        handle: "", //拖动对像 [必须参数]
        lock: true, //是否限制范围 
        lockX: false, //是否锁定X轴 
        lockY: false, //是否锁定Y轴 
        fixed: false, //对像是否为固定定位
        parent: "", //是否开启基于父层拖动
        sfns: function () { },//拖动开始时执行函数
        mfns: function () { },//拖动进行时执行函数
        ofns: function () { }//拖动结束时执行函数
    };
    var o = $.extend(defaults, o);
    var drag = false;
    var safe = Util.safeRange(o.obj);
    var $box = $("#" + o.obj);
    var moveX = 0, moveY = 0, _x, _y;
    if (o.fixed) {
		if (o.parent != "") { o.parent = ""; }
    }
    if (o.parent != "") {
        $("#" + o.parent).css("position", "relative");
    };
    if (o.handle != "") {
        $Handle = $(o.handle, $box);
    } else {
        $Handle = $box;
    };
	$Handle.css("cursor", "move");
    $Handle.mousedown(function (ev) {
        star(ev);
        if (this.setCapture) {
            this.setCapture();
        }
    });
	//复位
    var star = function (ev) {
        drag = true;
        if (o.sfns != "" && $.isFunction(o.sfns)) { o.sfns(this); };
        ev = ev || window.event;
        ev.preventDefault();
		var s = Util.pageSize.get(),
        p = Util.getPosition(o.obj, "html");
		ny = o.fixed ? Util.Browser.isIE6 ? s.scrollTop : 0 : 0;
        moveX = ev.clientX - p.x;
        moveY = ev.clientY - p.y + ny;
        $(document).bind("mousemove", function (ev) { move(ev) });
        $(document).bind("mouseup", function () { stop() });
    };
    var move = function (ev) {
		var parent;
        ev = ev || window.event;
        window.getSelection ? window.getSelection().removeAllRanges() : document.selection.empty(); //阻止浏览器默认选取
        _x = ev.clientX - moveX;
        _y = ev.clientY - moveY;
        if (o.parent != "") {
			parent = Util.getPosition(o.parent, "html");
			op = Util.getPosition(o.obj, "html");
			_x = ev.clientX - moveX - parent.x ;
			_y = ev.clientY - moveY - parent.y ;
        };
		maxX = o.parent != "" ? parent.width - op.width : safe.maxX;
        maxY = o.parent != "" ? parent.height - op.height : safe.maxY;
		if (o.lockX) { _y = p.y; };
        if (o.lockY) { _x = p.x; };
        if (o.lock) {
            if (_x <= 0) _x = safe.minX;
            if (_y <= 0) _y = safe.minY;
            if (_x >= maxX) { _x = maxX; }
            if (_y >= maxY) { _y = maxY; }
        };
        $box.css({
            left: _x + "px",
            top: _y + "px",
            right: "auto",
            bottom: "auto",
            margin: "auto"
        });
        if (o.mfns != "" && $.isFunction(o.mfns)) { o.mfns(this); };
        //$("#idShow").html("moveX =" + moveX + "; moveY = " + moveY +"maxX =" + maxX + "; maxY = " + maxY + ";clientX = " + ev.clientX + ";clientY = " + ev.clientY + "; ST = " + st + "; X = " + _x + ";Y = " + _y);
    };
    var stop = function () {
        drag = false;
        $(document).unbind("mousemove");
        if (o.ofns != "" && $.isFunction(o.ofns)) { o.ofns(this); };
        if (this.releaseCapture) {
            document.releaseCapture();
        }
    };
}
})(jQuery)
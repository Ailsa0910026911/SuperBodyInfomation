var $main = null;
var $list = null;
var $dex = null;
$(function () {
    function Onclick(obj) {
        obj.on("click", "li", function () {
            var $t = $(this);
            var num = $(this).find("a").data("num");
            obj.removeClass("active").find("a").removeClass("active");
            $t.siblings().find(".two-tank").addClass("hide");
            $t.addClass("active").find("a").addClass("active");
            $(".two-tank").find("a").removeClass("active active-two");
            $(".lison").find("i").addClass("text-blue").css("display", "block");
            var title = $(this).find("a").attr("title") ? $(this).find("a").attr("title") : $(this).find("a").text();
            if (!$t.is(".lison")) {
                repeat(title, num);
                $(".ifarm-index").hide();
                farme($(this).find("a").data("site"), num);
                if ($(this).find("i").length < 1) {
                    $(this).append(i);
                }
            } else {
                $(this).find("i").toggleClass("text-blue");
            }
        })
    }
    Onclick($(".list-ul"));
    $(".top-list, .more").on("click", "a", function () {
        var num = $(this).data("num");
        var $main = $(".main-content");
        var title = $(this).attr("title") ? $(this).attr("title") : $(this).text();
        repeat(title, num);
        farme($(this).data("site"), num);
        var index = showTime($(".ul-list"), "a", num, "num");
        var numper = showTime($main, "iframe", num, "num");
        $(".index-list").removeClass("active").find('a').removeClass('active');
        $(".ul-list").find("li").removeClass('active').find('a').removeClass("active");
        $(".ul-list").find("li").eq(index).addClass("active").find("a").addClass('active');
        select($(".ul-list").find("li").eq(index));
        $(".ifarm-index").hide();
        $main.find("iframe").hide().eq(numper).show();
    })
    $(".lison").click(function () {
        $(this).find("i").css("display", "block");
        $(this).find("div").toggleClass("hide");
        $(this).find(".two-tank").find("a").removeClass("active");
    })
    $(".lison").on("click", "li", function (ev) {
        var num = $(this).find("a").data("num");
        $(".lison").find("li").removeClass("active").find("a").removeClass("active-two");
        var src = $(this).find("a").data("site");
        farme(src, num);
        $(this).addClass("active").find("a").addClass("active-two");
        var title = $(this).find("a").attr("title") ? $(this).find("a").attr("title") : $(this).find("a").text();
        repeat(title, num);
        ev.stopPropagation();
    });
    var $side = $(".list-ul");
    $list = $(".ul-list");
    function indexlist() {
        $list.on("click", "li", function () {
            $(".ifarm-index").hide();
            $list.find("li").removeClass('active').find("a").removeClass("active");
            $(this).addClass("active").find("a").addClass("active").next("span").show();
            $(".index-list").removeClass("active");
            select($(this));
        })
        $(".index-list").on("click", function () {
            $(this).addClass("active");
            $list.find("li").removeClass("active").find("a").removeClass("active");
        })
    }
    indexlist();
    $dex = $(".index-list");
    var $ifarmindex = $(".ifarm-index");
    $main = $(".main-content");
    $dex.on("click", function () {
        $(this).addClass("active");
        $list.find("li").removeClass("active").find("a").removeClass("active");
        $ifarmindex.show();
        $main.find("iframe").hide();
    })
    $list.on("click", "span", function (event) {
        event.stopPropagation();
        IndexColseIFarme($(this).parent());
    })
    $(".list-ul, .lison").on("click", "li", function () {
        var num = $(this).find("a").data("num");
        var index = showTime($(".ul-list"), "a", num, "num");
        $main.find("iframe").eq(index).show().siblings().hide();
        $(".index-list").removeClass("active");
        var eqindex = $(".ul-list").find("li").eq(index);
        eqindex.addClass('active').find("a").addClass("active").parent().siblings().removeClass('active').find("a").removeClass("active");
        select(eqindex);
    })
    $(".ul-list").on("click", "li", function () {
        var $main = $(".main-content");
        var num = $(this).find("a").data("num");
        var index = showTime($main, "iframe", num, "num");
        $main.find("iframe").hide().eq(index).show();
    })
    /*导航*/
    $(".nav-list").on("click", "a", function () {
        $(".nav-list").find("a").removeClass("active");
        $(this).addClass("active");
        var $mainleft = $(".main-left");
        var num = $(this).parent().data("index");
        var index = showTime($mainleft, ".list", num, "index");
        $mainleft.find(".list").eq(index).removeClass("hide").siblings(".side").addClass("hide");
    })
    $(".list").find("ul").children("li").on("click", function () {
        if (!$(this).is("li.lison")) {
            $(this).find("i").show();
            $(this).siblings().find("i").hide();
        }
    })
    $(".Delete").click(function () {
        Url = $(this).attr("href");
        var Tr = $(this).closest("tr");
        DeleteItemDialog = art.dialog({
            title: '温馨提示',
            content: '您确定删除选定的数据吗？',
            icon: 'warning',
            lock: true,
            button: [{
                name: '删除',
                callback: function () {
                    $.get(Url, function (data) {
                        Tr.remove();
                    });
                },
                focus: true,
                disabled: false
            }, {
                name: '取消',
                callback: function () {
                    this.close();
                }
            }
            ]
        });
        return false;
    });
});
function select(obj) {
    var $span = $("<span> X </span>");
    var $dex = $(".index-list");
    $dex.removeClass("active");
    obj.addClass("active").find("a").addClass("active");
    obj.siblings().removeClass("active").find("a").removeClass("active");
    if (obj.find("span").length < 1) {
        obj.append($span).siblings().find("span").remove();
    }
}
/*iframe*/
function farme(src, num) {
    var iframeId = "iframeId" + num;
    var $farme = $("<iframe id='" + iframeId + "'  src = '" + src + "' data-num = '" + num + "' onLoad='iFrameHeight(\"" + iframeId + "\")' name=" + iframeId + " height='506' ></iframe>");
    $farme.attr({
        width: "100%"
    });
    var len = $main.find("iframe").length;
    var arr = [];
    var index;
    $main.find("iframe").each(function () {
        arr.push(parseInt($(this).data("num")));
    });
    var ina = jQuery.inArray(num, arr);
    if (ina < 0) {
        $main.append($farme);
    }
}
function showTime(parent, data, node, num) {
    var index;
    parent.find(data).each(function (i) {
        if (node == $(this).data(num)) {
            index = i;
        }
    })
    return index;
}
function repeat(val, num) {
    var $list = $(".ul-list");
    var arr = [];
    var len = $list.find("li").length;
    $list.find("li").each(function () {
        arr.push($(this).find("a").data("num"));
    })
    var inarr = jQuery.inArray(num, arr);
    if (inarr < 0) {
        var li = $("<li title='" + val + "'><a href='javascript:;'data-num = '" + num + "' >" + val + "</a><span> X </span></li>");
        $list.append(li);
        if (len > 7) {
            var $li = $list.find("li").first();
            $li.remove();
            var num = $li.first().find("a").data("num");
            var index = showTime($li, "a", num, "num");
            $(".main-content").find("iframe").eq(index).remove();
        }
    }
    return inarr ? inarr : 0;
}
/*
打开一个新的ifarme
title:ifarme的名称
src:内容的url
num:编号
*/
var seedNum = 5000;
function OpentIfarme(title, src, num) {
    if (!arguments[2]) num = seedNum;
    repeat(title, num);
    farme(src, num);
    var index = showTime($(".ul-list"), "a", num, "num");
    var numper = showTime($main, "iframe", num, "num");
    $(".index-list").removeClass("active").find('a').removeClass('active');
    $(".ul-list").find("li").removeClass('active').find('a').removeClass("active");
    $(".ul-list").find("li").eq(index).addClass("active").find("a").addClass('active');
    select($(".ul-list").find("li").eq(index));
    $(".ifarm-index").hide();
    $main.find("iframe").hide().eq(numper).show();
    seedNum++;
}
//关闭Ifarme
function IndexColseIFarme($t)
{
    var prev = $t.prev();
    var next = $t.next();
    var num = parseInt($t.find("a").data("num"));
    var index = showTime($main, "iframe", num, "num");
    if (prev.length > 0) {
        prev.addClass("active").find("a").addClass("active");
        prev.find("span").show();
        $main.find("iframe").eq(index).prev().show();
    } else {
        next.find("span").show();
        next.addClass("active").find("a").addClass("active");
        $main.find("iframe").eq(index).next().show();
    }
    $t.remove();
    $main.find("iframe").eq(index).remove();
    if ($list.find("li").length < 1) {
        $dex.addClass("active");
        $(".ifarm-index").show();
    }
}
//当前激活的窗口栏
function ActiveList()
{
    return $list.find("li[class='active']");
}
//取得号码的窗口栏
function GetNumList(num)
{
    return $list.find("li a[data-num=" + num + "]").parent();
}
//去除ifarmeid的字母
function ReplaceIfarmeid(id)
{
    return id.replace("iframeId", "")
}
var DeleteItemDialog;
$(function () {
    var wdate = $(".Wdate");
    if (wdate.length != 0) {
        $(".Wdate").on("click", WdatePicker);
    }
    $(".tbody").find("tr:even").css("background", "#ecfaff");
    var $batch = $(".batch");
    $batch.on("click", function () {
        $batch.toggleClass("hight");
    })
    var height = $("body").height();
    var frameId = window.frameElement && window.frameElement.id || '';
    $(top.frames[frameId]).attr("height", height);
    //原有的js
    InitIsAjax();;
    $(".Reply").click(function () {
        Url = $(this).attr("href");
        var Tr = $(this).closest("tr");
        DeleteItemDialog = parent.ArtDialog({
            title: '温馨提示',
            content: '您确定还原选定的数据吗？',
            icon: 'warning',
            lock: true,
            button: [{
                name: '还原数据',
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
    $(".Deleted").click(function () {
        Url = $(this).attr("href");
        var Tr = $(this).closest("tr");
        //alert(Url);
        DeleteItemDialog = parent.ArtDialog({
            title: '温馨提示',
            content: '彻底删除的数据将<font color="red"><b>无法恢复</b></font>，您确定要操作吗？',
            icon: 'warning',
            lock: true,
            button: [{
                name: '彻底删除',
                callback: function () {
                    $.get(Url, function (data) {
                        if (data < 0) {
                            art.dialog({
                                title: '温馨提示',
                                content: '您没有彻底删除数据权限？',
                                icon: 'warning',
                                lock: true,
                                cancel: function () {
                                    this.close();
                                }
                            });
                        } else {
                            Tr.remove();
                        }
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
    $(".Delete").click(function () {
        Url = $(this).attr("href");
        var Tr = $(this).closest("tr");
        DeleteItemDialog = parent.ArtDialog({
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
    $(".CheckAll").on("click", function () {
        var inputname = $(this).attr("rev");
        var CHK = $(this).prop("checked");
        $("input[name=" + inputname + "][type=checkbox]").prop("checked", CHK);
        if (CHK) {
            $("input[name=" + inputname + "][type=checkbox]").each(function () {
                $(this).parent("span").addClass("checked");
            });
        } else {
            $("input[name=" + inputname + "][type=checkbox]").each(function () {
                $(this).parent("span").removeClass("checked");
            });
        }
    });
    $(".BatSet").on("click", function () {
        var rev = $(this).attr("rev");
        var rel = $(this).attr("rel");
        if (rev == "Reply" || rev == "Delete" || rev == "Deleted") {
            DeleteByList(rev);
        } else {
            ChangeStatus(rev, rel);
        }
        return false;
    });
    $("#XLS").click(function () {
        var par = $("#ListForm").serialize();
        location.href = "XlsDo.html?" + par;
    });
    $(".chkForm").submit(function () {
        if ($(this).validationEngine('validate')) {
            $(this).find("input[type=checkbox].one-check:not(:checked)").each(function () {
                $(this).val("0").prop("checked", true);
            });
        }
    });
});
var InitIsAjax = function () {
    $('.Ajax').unbind();
    $('.Ajax').click(function () {
        var url = $(this)[0].href;
        var frameId = window.frameElement && window.frameElement.id || '';
        var title = $(this).attr("atitle") == undefined ? $(this).attr("title") : $(this).attr("atitle");
        var awidth = $(this).attr("awidth") == undefined ? "0" : $(this).attr("awidth");
        var aheight = $(this).attr("aheight") == undefined ? "0" : $(this).attr("aheight");
        if (url.indexOf("?") != -1) {
            url = url + "&IsAjax=1&iframeId=" + frameId;
        } else {
            url = url + "?IsAjax=1&iframeId=" + frameId;
        }
        if (title == "" || title == null) {
            title = "系统弹窗操作";
        }
        if (awidth == "0" || aheight == "0") {
            var json = { title: title, lock: true };
        } else {
            var json = { width: awidth, height: aheight, title: title, lock: true };
        }
        parent.ArtDialogOpen(url, json);
        return false;
    });
}
var ChangeStatus = function (Clomn, value) {
    var action = arguments[2] ? arguments[2] : "ChangeStatus.html";
    var ListID = "0";
    var allselectText = $('input[name=list]');
    for (var i = 0; i < allselectText.length; i++) {
        if (allselectText[i].checked) {
            ListID += "," + allselectText[i].value;
        }
    }
    if (ListID.split(",").length == 1) {
        parent.ArtDialog({
            title: '温馨提示',
            content: '当前页面信息并没有任何选中项？',
            icon: 'warning',
            lock: true,
            cancel: function () {
                this.close();
            }
        });
        return false;
    }
    ListID = ListID.replace("0,", "");
    var Url = action + '?Clomn=' + Clomn + '&value=' + value + '&InfoList=' + ListID + '&' + Math.random();
    parent.ArtDialog({
        title: '温馨提示',
        lock: true,
        content: "共有" + (ListID.split(",").length) + "条信息将被批量修改,您确定操作吗？",
        icon: 'warning',
        ok: function () {
            $.get(Url, function (data) {
                window.location.reload();
                //GoHref(location.href);
            });
        },
        cancel: function () {
            this.close()
        }
    });
    return false;
}
var DeleteByList = function (DoWhat) {
    var action = arguments[1] ? arguments[1] : "Delete.html";
    var ListID = "0";
    var allselectText = $('input[name=list]');
    for (var i = 0; i < allselectText.length; i++) {
        if (allselectText[i].checked) {
            ListID += "," + allselectText[i].value;
        }
    }
    if (ListID.split(",").length == 1) {
        parent.ArtDialog({
            title: '温馨提示',
            content: '当前页面信息并没有任何选中项？',
            icon: 'warning',
            lock: true,
            cancel: function () {
                this.close();
            }
        });
        return false;
    }
    ListID = ListID.replace("0,", "");
    if (DoWhat == "Deleted") {
        btnname = "彻底删除";
        content = "共有" + (ListID.split(",").length) + "条信息将被彻底删除，删除后将<font color='red'><b>无法恢复</b></font>，您确定要操作吗？";
    } else if (DoWhat == "Delete") {
        btnname = "删除";
        content = "共有" + (ListID.split(",").length) + "条信息将被删除，您确定要操作吗？";
    } else if (DoWhat == "Reply") {
        btnname = "恢复";
        content = "共有" + (ListID.split(",").length) + "条信息将被恢复，您确定要操作吗？";
    } else {
        return false;
    }
    var Url = action + '?InfoList=' + ListID + '&' + Math.random();
    if (DoWhat == "Deleted") {
        Url = action + '?IsDel=1&InfoList=' + ListID + '&' + Math.random();
    }
    parent.ArtDialog({
        title: '温馨提示',
        lock: true,
        content: content,
        icon: 'warning',
        button: [
        {
            name: btnname,
            callback: function () {
                $.get(Url, function (data) {
                    if (DoWhat == "Deleted") {
                        if (data < 0) {
                            art.dialog({
                                title: '温馨提示',
                                content: '您没有彻底删除数据权限？',
                                icon: 'warning',
                                lock: true,
                                cancel: function () {
                                    this.close();
                                }
                            });
                        } else {
                            GoHref(location.href);
                        }
                    } else {
                        GoHref(location.href);
                    }
                });
            },
            focus: true
        }, {
            name: '取消',
            callback: function () {
                this.close();
            }
        }
        ]
    });
    return false;
}
var ShowLoadMark = function () {
    $('#loading-mask').fadeIn().css("top", ($(document).height() - 200) / 2).css("left", ($(document).width() - 400) / 2);
}
var HideLoadMark = function () {
    $('#loading-mask').fadeOut();
}
var NoPurview = function (iframeId) {
    $.each(parent.art.dialog.list, function (index, item) {
        if (item.config.id == iframeId) {
            item.close();
        }
    });
    errordialog();
}
var closeartById = function (iframeId) {
    $.each(parent.art.dialog.list, function (index, item) {
        if (item.config.id == iframeId) {
            item.close();
        }
    });
}
var closeart = function () {
    $.each(parent.art.dialog.list, function (index, item) {
        item.close();
    });
}
var errordialog = function () {
    var baseerrordialog = art.dialog({
        title: '温馨提示',
        content: '对不起，您刚才操作的页面禁止访问，原因是：您没有权限！',
        icon: 'error',
        lock: true,
        follow: document.getElementById('Login'),
        ok: function () {
            baseerrordialog.close();
        }
    });
}
var GoHref = function (href) {
    var referLink = document.createElement('a');
    referLink.href = href;
    document.body.appendChild(referLink);
    referLink.click();
}
var GetHref = function (href, Tr) {
    $.get(href, function (data) {
        Tr.remove();
    });
}
function GetRequest() {
    var url = location.search; //获取url中"?"符后的字串   
    var theRequest = new Object();
    if (url.indexOf("?") != -1) {
        var str = url.substr(1);
        strs = str.split("&");
        for (var i = 0; i < strs.length; i++) {
            theRequest[strs[i].split("=")[0]] = unescape(strs[i].split("=")[1]);
        }
    }
    return theRequest;
}

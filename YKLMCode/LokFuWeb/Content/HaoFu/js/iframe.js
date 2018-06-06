var DeleteItemDialog;
var iframeId = window.frameElement && window.frameElement.id || '';
var tab;
$(function () {
    ////交易时间限制  add gaojin 2016-7-26
    //$("#STime").focus(function () {
    //    WdatePicker({ maxDate: '#F{$dp.$D(\'ETime\')|| \'%y-%M-%d\'}' });
    //});
    //$("#ETime").focus(function () {
    //    WdatePicker({ maxDate: '%y-%M-%d', minDate: '#F{$dp.$D(\'STime\')}' });
    //});
    ////未来日期限制  add gaojin 2016-7-26
    //$("#WSTime").focus(function () {
    //    WdatePicker({ maxDate: '#F{$dp.$D(\'WETime\')}', minDate: '%y-%M-%d' });
    //});
    //$("#WETime").focus(function () {
    //    WdatePicker({ minDate: '#F{$dp.$D(\'WSTime\')|| \'%y-%M-%d\'}' });
    //});
    ////今天之后的时间 add gaojin 2016-7-26
    //$("#DDLastTime").focus(function () {
    //    WdatePicker({ minDate: '%y-%M-%d' });
    //});
    ////没有限制的时间 add gaojin 2016-7-26
    //$("#ExpireTime").focus(function () {
    //    WdatePicker();
    //});
    ////T01名称变动日期限制  add gaojin 2016-7-26
    //$("#SW1sTime").focus(function () {
    //    WdatePicker({ maxDate: '#F{$dp.$D(\'SW1eTime\')}' });
    //});
    //$("#SW1eTime").focus(function () {
    //    WdatePicker({ minDate: '#F{$dp.$D(\'SW1sTime\')}' });
    //});
    ////T02名称变动日期限制  add gaojin 2016-7-26
    //$("#SW2sTime").focus(function () {
    //    WdatePicker({ maxDate: '#F{$dp.$D(\'SW2eTime\')}' });
    //});
    //$("#SW2eTime").focus(function () {
    //    WdatePicker({ minDate: '#F{$dp.$D(\'SW2sTime\')}' });
    //});
    /***********       给日期控件增加时分秒 add anjing 2016-12-3    START     ************************/
    $("#STime").focus(function () {
        WdatePicker({ maxDate: '#F{$dp.$D(\'ETime\')|| \'%y-%M-%d\'}', dateFmt: 'yyyy-MM-dd HH:mm:ss', startDate: '%y-%M-%d 00:00:00' });
    });
    $("#ETime").focus(function () {
        WdatePicker({ maxDate: '%y-%M-%d', minDate: '#F{$dp.$D(\'STime\')}', dateFmt: 'yyyy-MM-dd HH:mm:ss', startDate: '%y-%M-%d %H:%m:%s' });
    });
    $("#StartDT").focus(function () {
        WdatePicker({ maxDate: '#F{$dp.$D(\'EndDT\')|| \'%y-%M-%d\'}', dateFmt: 'yyyy-MM-dd HH:mm:ss', startDate: '%y-%M-%d 00:00:00' });
    });
    $("#EndDT").focus(function () {
        WdatePicker({ maxDate: '%y-%M-%d', minDate: '#F{$dp.$D(\'StartDT\')}', dateFmt: 'yyyy-MM-dd HH:mm:ss', startDate: '%y-%M-%d %H:%m:%s' });
    });
    $("#WSTime").focus(function () {
        WdatePicker({ maxDate: '#F{$dp.$D(\'WETime\')|| \'%y-%M-%d\'}', dateFmt: 'yyyy-MM-dd HH:mm:ss', startDate: '%y-%M-%d 00:00:00' });
    });
    $("#WETime").focus(function () {
        WdatePicker({ maxDate: '%y-%M-%d', minDate: '#F{$dp.$D(\'WSTime\')}', dateFmt: 'yyyy-MM-dd HH:mm:ss', startDate: '%y-%M-%d %H:%m:%s' });
    });
    $("#DDLastTime,#TKLastTime").click(function () {
        WdatePicker({ minDate: '%y-%M-%d', dateFmt: 'yyyy-MM-dd HH:mm:ss', startDate: '%y-%M-%d 00:00:00' });
    });
    $("#SW1sTime").focus(function () {
        WdatePicker({ maxDate: '#F{$dp.$D(\'SW1eTime\')}', dateFmt: 'yyyy-MM-dd HH:mm:ss', startDate: '%y-%M-%d 00:00:00' });
    });
    $("#SW1eTime").focus(function () {
        WdatePicker({ minDate: '#F{$dp.$D(\'SW1sTime\')}', dateFmt: 'yyyy-MM-dd HH:mm:ss', startDate: '%y-%M-%d %H:%m:%s' });
    });
    $("#SW2sTime").focus(function () {
        WdatePicker({ maxDate: '#F{$dp.$D(\'SW2eTime\')}', dateFmt: 'yyyy-MM-dd HH:mm:ss', startDate: '%y-%M-%d 00:00:00' });
    });
    $("#SW2eTime").focus(function () {
        WdatePicker({ minDate: '#F{$dp.$D(\'SW2sTime\')}', dateFmt: 'yyyy-MM-dd HH:mm:ss', startDate: '%y-%M-%d %H:%m:%s' });
    });
    //TaskTimeSet页面的
    $("#SDate").click(function () {
        WdatePicker({ maxDate: '#F{$dp.$D(\'EDate\')}', dateFmt: 'yyyy-MM-dd HH:mm:ss', startDate: '%y-%M-%d 00:00:00' });
    });
    $("#EDate").click(function () {
        WdatePicker({ minDate: '#F{$dp.$D(\'SDate\')}', dateFmt: 'yyyy-MM-dd HH:mm:ss', startDate: '%y-%M-%d %H:%m:%s' });
    });
    $("#ExpireTime").focus(function () {
        WdatePicker({ dateFmt: 'yyyy-MM-dd HH:mm:ss', startDate: '%y-%M-%d 00:00:00' });
    });
    /************      END        *******************/
    //var wdate = $(".Wdate");
    //if (wdate.length != 0) {
    //    $(".Wdate").on("click", WdatePicker);
    //}
    $(".tbody").find("tr:even").css("background", "#ecfaff");
    var $batch = $(".batch");
    $batch.on("click", function () {
        $batch.toggleClass("hight");
    })
    //原有的js
    InitIsAjax();
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

    if ($('#twitter-bootstrap-container').length > 0) {
        tab = $('#twitter-bootstrap-container').easytabs();
        tab.bind('easytabs:after', function (event, $clicked, $targetPanel, settings) {
            tab.height($targetPanel.height() + 80);
            //console.log(tab.height());
        });
    }

});


var InitIsAjax = function () {
    $('.Ajax').unbind();
    $('.Ajax').click(function () {
        var url = $(this)[0].href;
        var title = $(this).attr("atitle") == undefined ? $(this).attr("title") : $(this).attr("atitle");
        var awidth = $(this).attr("awidth") == undefined ? "0" : $(this).attr("awidth");
        var aheight = $(this).attr("aheight") == undefined ? "0" : $(this).attr("aheight");
        var save = $(this).attr("save") == undefined ? true : false;
        if (url.indexOf("?") != -1) {
            url = url + "&IsAjax=1&IFrameId=" + iframeId;
        } else {
            url = url + "?IsAjax=1&IFrameId=" + iframeId;
        }
        if (title == "" || title == null) {
            title = "";
        }
        if (awidth == "0" || aheight == "0") {
            var json = { title: title, lock: true };
        } else {
            var json = { width: awidth, height: aheight, title: title, lock: true };
        }
        if (save) {
            parent.ArtDialogOpen(url, json);
        }
        else {
            parent.currentAtr = parent.ArtDialogOpen(url, json);
        }
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
/*
FileUpLoad最小化初始化
完成时的实现部份自己补充，已返回对象
https://github.com/blueimp/jQuery-File-Upload/wiki/Options#validation-options
*/
var InitUpLoad = function () {
    var url = $(this).attr("data-url") == undefined ? "" : $(this).attr("data-url");
    var maxFileSize = parseInt($(this).attr("data-MaxFileSize") == undefined ? "2000000" : $(this).attr("data-MaxFileSize"));
    var maxNumberOfFiles = parseInt($(this).attr("data-MaxUpNumber") == undefined ? "10" : $(this).attr("data-MaxUpNumber"));
    var acceptFileTypes = $(this).attr("data-FileType") == undefined ? "gif|jpe?g|png" : $(this).attr("data-FileType");
    return $('.UpLoad').fileupload({
        url: url,
        dataType: 'json',
        maxFileSize: maxFileSize,
        maxNumberOfFiles: maxNumberOfFiles,
        acceptFileTypes: new RegExp("(\.|\/)(" + acceptFileTypes + ")$", "i"),
        messages: {
            maxNumberOfFiles: '最多能上传文件' + maxNumberOfFiles + "个",
            acceptFileTypes: '文件类型不正确,格式:gif,jpeg,png',
            maxFileSize: '上传文件最大' + maxFileSize / 1000 / 1000 + "M",
            minFileSize: '上传文件最小1KB'
        },
        processalways: function (e, data) {
            if (data.files.error) {
                $.each(data.files, function (index, file) {
                    alert(file.error);
                });
            }
        }
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
function newGuid() {
    var guid = "";
    for (var i = 1; i <= 32; i++) {
        var n = Math.floor(Math.random() * 16.0).toString(16);
        guid += n;
        if ((i == 8) || (i == 12) || (i == 16) || (i == 20))
            guid += "";
    }
    return guid;
}

//生成table
function TableIni(ElemId) {
    var table = $('#' + ElemId).DataTable({
        "ordering": false,
        "aLengthMenu": [
            [5, 10, 20, -1],
            [5, 10, 20, "所有"] // change per page values here
        ],
        // set the initial value
        "iDisplayLength": 10,
        "sPaginationType": "bootstrap",
        "oLanguage": {
            "sLengthMenu": "_MENU_ 行数",
            "oPaginate": {
                "sPrevious": "Prev",
                "sNext": "Next"
            },
            "sInfo": "共_TOTAL_条数据 ",
            "sInfoEmpty": "共0条数据",
            "sEmptyTable": "没有数据",
            "sSearch": "",
            "sZeroRecords": "没有匹配的数据",
            "sInfoFiltered": "(共搜索了 _MAX_ 条数据)"
        }
    });
    jQuery('#' + ElemId + '_wrapper').css("width", "98%");
    jQuery('#' + ElemId + '_wrapper .dataTables_filter input').addClass("form-control input-medium"); // modify table search input
    jQuery('#' + ElemId + '_wrapper .dataTables_filter').parent("div").removeClass("col-sm-12");
    jQuery('#' + ElemId + '_wrapper .dataTables_length select').addClass("form-control input-xsmall"); // modify table per page dropdown
    jQuery('#' + ElemId + '_wrapper .dataTables_length').parent("div").removeClass("col-sm-12");
    return table;
}


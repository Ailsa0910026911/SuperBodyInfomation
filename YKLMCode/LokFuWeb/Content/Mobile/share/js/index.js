var sec = 0;
var Runing1 = false;
var Runing2 = false;
$(function () {
    $("#img").click(function () {
        $(this).fadeOut(0).attr("src", "/Content/Metronic/img/none.gif").attr("src", "/Home/ImgCode.html?rand=" + Math.random()).fadeIn(500);
    });
    $("#tuCode").focus(function () {
        $("#imga").show();
        if ($("#img").attr("src") == "/Content/Metronic/img/none.gif") {
            $("#img").fadeOut(0).attr("src", "/Home/ImgCode.html?rand=" + Math.random()).fadeIn(500);
            $(this).val("");
        }
    });
    $(".download").click(function () {
        download();
        return false;
    });
    $("#GetCode").click(function () {
        var UserName = $("#UserName").val();
        if (!/^((\(\d{2,3}\))|(\d{3}\-))?1[1-9]\d{9}$/.test(UserName)) {
            ShowInfo("请正确填写您的手机号");
            return false;
        }
        var tuCode = $("#tuCode").val();
        //if (!/^[0-9a-zA-Z]{4}$/.test(tuCode)) {
        //    ShowInfo("请填写图形验证码！");
        //    return false;
        //}
        if (tuCode.length < 4) {
            ShowInfo("请输入图型验证码！");
            return false;
        }
        var data = $.ajax({ url: "/home/CheckCode.html?code=" + tuCode, async: false }).responseText;
        if (data != "1") {
            ShowInfo("图形验证码错误！");
            return false;
        }
        if (sec > 0) {
            ShowInfo("请耐心等待，不要重复发送！");
            return false;
        }
        if (Runing1) {
            ShowInfo("正在提交，请勿重复提交！");
            return false;
        }
        Runing1 = true;
        var Agent = $("#Agent").val();
        $.ajax({
            url: "GetCode.html",
            data: "UserName=" + UserName + "&tuCode=" + tuCode + "&Agent=" + Agent,
            success: function (ret) {
                if (ret == "OK") {
                    sec = 60;
                    SetSec();
                    ShowInfo("验证码已发送至您的手机，请及时查收！");
                } else if (ret == "1") {
                    ShowInfo("该手机号已注册，请更换手机号或取回密码！");
                } else if (ret == "2") {
                    ShowInfo("您今天重试次数已达到上限，请明天再试！");
                } else if (ret == "3") {
                    ShowInfo("请耐心等待，不要重复发送！");
                } else if (ret == "4") {
                    ShowInfo("图形验证码错误！");
                } else if (ret == "5") {
                    ShowInfo("邀请链接已失效，暂不能注册！");
                }
                else if (ret == "6") {
                    ShowInfo("暂不支持您的手机号入网！");
                }
                Runing1 = false;
            }
        });
    });
    $("#regForm").submit(function () {
        var UserName = $("#UserName").val();
        var Code = $("#Code").val();
        var PassWord = $("#PassWord").val();
        var PassWord2 = $("#PassWord2").val();
        var tuCode = $("#tuCode").val();
        
       // var Checkbox = $("#Agreement").attr("checked");
        var Checkbox = $("input[type='checkbox']").is(':checked');
        if (!/^((\(\d{2,3}\))|(\d{3}\-))?1[1-9]\d{9}$/.test(UserName)) {
            ShowInfo("请正确填写您的手机号");
            return false;
        }
        if (tuCode.length < 4) {
            ShowInfo("请输入图型验证码！");
            return false;
        }
        if (!/^[0-9]{4}$/.test(Code)) {
            ShowInfo("请转入短信验证码！");
            return false;
        }
        if (PassWord.length < 8) {
            ShowInfo("请至少填写8位的密码");
            return false;
        }
        if (/^[0-9]{8,20}$/.test(PassWord)) {
            ShowInfo("请设置8-20位由字母与数字组成的登录密码！");
            return false;
        }
        if (!/^(?![0-9]+$)(?![a-zA-Z]+$)[0-9A-Za-z]{8,20}$/.test(PassWord)) {
            ShowInfo("密码必须为8-20位的数字及英文组合");
            return false;
        }
        if (PassWord != PassWord2) {
            ShowInfo("两次输入密码不一至！");
            return false;
        }
        
        if (!Checkbox) {
            ShowInfo("未同意用户使用协议");
            return false;
        }
        var form = $(this).serialize();
        if (Runing2) {
            ShowInfo("正在提交，请勿重复提交！");
            return false;
        }
        Runing2 = true;
        $.post("save.html", form, function (ret) {
            if (ret == "OK") {
                showInfo("恭喜您，注册成功！");
                setTimeout(function () {
                    $(".step").hide();
                    $("#step3").show();
                }, 3000);
            } else if (ret == "0") {
                ShowInfo("信息填写不完整！");
            } else if (ret == "1") {
                ShowInfo("该手机号已注册，请更换手机号或取回密码！");
            } else if (ret == "2") {
                ShowInfo("验证码错误！");
            } else if (ret == "3") {
                ShowInfo("验证码已被使用过！");
            } else if (ret == "4") {
                ShowInfo("验证码已失效！");
            } else if (ret == "5") {
                //ShowInfo("邀请链接已失效，暂不能注册！");
                showInfo("恭喜您，注册成功！");
                setTimeout(function () {
                    $(".step").hide();
                    $("#step3").show();
                    $(".layer-bg").show();
                    $(".reg_cg").show();
                }, 3000);
            }
            Runing2 = false;
        });
        return false;
    });
});
var ShowInfo = function (info) {
    console.log(info);
    $(".text-danger").text(info);
    $(".point").show();
    //$(".point").removeClass("hidden");
}
var showInfo = function (info) {
    $("#showmsg").text(info);
    $(".layer-bg").removeClass("hide");
    $(".reg_cg").removeClass("hide");
}
var SetSec = function () {
    if (sec > 0) {
        sec--;
        $("#GetCode").addClass("noused").text(sec + "秒后重试");
        setTimeout("SetSec()", 1000);
    } else {
        $("#GetCode").removeClass("noused").text("重新发送");
    }
}
/*
* 智能机浏览器版本信息:
*
*/
var browser = {
    versions: function () {
        var u = navigator.userAgent, app = navigator.appVersion;
        return {//移动终端浏览器版本信息 
            trident: u.indexOf('Trident') > -1, //IE内核
            presto: u.indexOf('Presto') > -1, //opera内核
            webKit: u.indexOf('AppleWebKit') > -1, //苹果、谷歌内核
            gecko: u.indexOf('Gecko') > -1 && u.indexOf('KHTML') == -1, //火狐内核
            mobile: !!u.match(/AppleWebKit.*Mobile.*/) || !!u.match(/AppleWebKit/), //是否为移动终端
            ios: !!u.match(/\(i[^;]+;( U;)? CPU.+Mac OS X/), //ios终端
            android: u.indexOf('Android') > -1 || u.indexOf('Linux') > -1, //android终端或者uc浏览器
            iPhone: u.indexOf('iPhone') > -1 || u.indexOf('Mac') > -1, //是否为iPhone或者QQHD浏览器
            iPad: u.indexOf('iPad') > -1, //是否iPad
            webApp: u.indexOf('Safari') == -1, //是否web应该程序，没有头部与底部
            isWX: u.indexOf('MicroMessenger') > -1 //是否微信浏览器
        };
    }(),
    language: (navigator.browserLanguage || navigator.language).toLowerCase()
}
function download() {
    if (browser.versions.ios || browser.versions.iPhone || browser.versions.iPad) {
        if (browser.versions.isWX) {
            window.location = IosYYB;
        } else {
            window.location = IosUrl;
        }
    }
    else if (browser.versions.android) {
        if (browser.versions.isWX) {
            window.location = ApkYYB;
        } else {
            window.location = ApkUrl;
        }
    }
}
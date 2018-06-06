
$(function () {
    Interval = setInterval('GetQrCodeLogin()', 1500);
    var oPc = $(".login-top").find(".pc");
    oPc.on("click", function () {
        var scan = $(".scan");
        scan.addClass("hide");
        $(".userlogin").removeClass("hide");
        $(".scan").addClass("hide");
        clearInterval(Interval);
    })
    $(".shao").on("click", function () {
        $(".scan").removeClass("hide");
        $(".userlogin").addClass("hide");
        Interval = setInterval('GetQrCodeLogin()', 1500);
    })

    $("#Submit").click(function () {
        var un = $("#un").val();
        var pwd = $("#pwd").val();
        var c = $("#code").val();

        if (!/^((\(\d{2,3}\))|(\d{3}\-))?1[3,5,8]\d{9}$/.test(un)) {
            $("#LoginTip").addClass("showtip").text("请正确填写您的手机号！");
            return false;
        }
        if (!/^(?![0-9]+$)(?![a-zA-Z]+$)[0-9A-Za-z]{8,20}$/.test(pwd)) {
            $("#LoginTip").addClass("showtip").text("请正确填写您的密码！");
            return false;
        }
        if (!/^[0-9a-zA-Z]{4}$/.test(c)) {
            $("#LoginTip").addClass("showtip").text("验证码为4位字符！");
            return false;
        }
        var xc = new xxcode(key);
        var en_un = xc.xxcode_encrypt(un);
        var en_pwd = xc.xxcode_encrypt(pwd);
        $("#LoginTip").addClass("showtip").text("正在验证登录~！");
        $.ajax({
            type: "POST",
            dataType: "json",
            url: "/shop/login/chklogin.html",
            data: { "username": en_un, "password": en_pwd, "code": c },
            success: function (data) {
                if (data.msg == "OK") {
                    $("#LoginTip").addClass("showtip").text("登录成功，正在跳转");
                    location.href = "/shop/home/index.html";
                } else if (data.msg == "E0") {
                    $("#LoginTip").addClass("showtip").text("验证码错误！");
                } else if (data.msg == "E1") {
                    $("#LoginTip").addClass("showtip").text("帐户或密码错误！");
                    $("#img").click();
                } else if (data.msg == "E2") {
                    $("#LoginTip").addClass("showtip").text("密码错误，您还能尝试" + data.log + "次");
                    $("#img").click();
                } else if (data.msg == "E3") {
                    $("#LoginTip").addClass("showtip").text("帐户未审核或被锁定！");
                } else if (data.msg == "E4") {
                    $("#LoginTip").addClass("showtip").text("帐号锁止，请明天再试！");
                }
            }
        });
        return false;
    });
    $("#img").click(function () {
        $(this).fadeOut(0).attr("src", "/Content/Metronic/img/none.gif").attr("src", "/Home/ImgCode.html?rand=" + Math.random()).fadeIn(500);
        $("#code").val("");
    });
    $("#code").focus(function () {
        if ($("#img").attr("src") == "/Content/Metronic/img/none.gif") {
            $("#img").fadeOut(0).attr("src", "/Home/ImgCode.html?rand=" + Math.random()).fadeIn(500);
            $(this).val("");
        }
    });
});
var GetQrCodeLogin = function () {
    $.ajax({
        url: "/shop/login/getqrcodelogin.html",
        data: "Sceneid=" + Sceneid + "&timed=" + new Date().getTime(),
        type: "GET",
        timeout: 5000,
        success: function (data) {
            if (data == "OK") {
                $("#CodeTip").addClass("showtip").text("登录成功，正在跳转");
                clearInterval(Interval);
                location.href = "/shop/home/index.html";
            } else if (data == "E0") {
                $("#CodeTip").addClass("showtip").text("二维码失效！");
                clearInterval(Interval);
                location.href = "/shop/login.html";
            } else if (data == "E8") {
                $("#CodeTip").addClass("showtip").text("已扫码，请在确认授权！");
            } else if (data == "E9") {
                $("#CodeTip").removeClass("showtip").text("手机扫码，安全登录");
            }

        }
    });
}
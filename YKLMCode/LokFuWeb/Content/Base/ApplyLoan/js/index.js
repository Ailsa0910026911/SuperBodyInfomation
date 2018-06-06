var n = 0;
var selects;
$(function () {
    $(".Next").click(function () {
        if ($("#chkForm").validationEngine("validate")) {
            n++;
            ShowN();
        }
    });
    $(".Prev").click(function () {
        n--;
        ShowP();
    });
    $("#HasSheBao").change(function () {
        if ($(this).val() == 1) {
            $(".HasSheBao").show();
        } else {
            $(".HasSheBao").hide();
        }
    });
    $("#HasCar").change(function () {
        if ($(this).val() == 1) {
            $(".HasCar").show();
        } else {
            $(".HasCar").hide();
        }
    });
    $("#HasCredit").change(function () {
        if ($(this).val() == 1) {
            $(".HasCredit").show();
        } else {
            $(".HasCredit").hide();
        }
    });
    selects = $("#ComCity").clone();
    $("#ComCity option:gt(0)").remove();
    $("#ComProvince").change(function () {
        $("#ComCity").val("");
        $("#ComCity option:gt(0)").remove();
        if ($(this).val() != "") {
            var rev = selects.find("option:[rev=" + $(this).val() + "]").clone();
            $("#ComCity").append(rev);
        }
    });
    $("#img").click(function () {
        $(this).fadeOut(0).attr("src", "/Content/Metronic/img/none.gif").attr("src", "/Home/ImgCode.html?rand=" + Math.random()).fadeIn(500);
    });
    $("#code").focus(function () {
        if ($("#img").attr("src") == "/Content/Metronic/img/none.gif") {
            $("#img").fadeOut(0).attr("src", "/Home/ImgCode.html?rand=" + Math.random()).fadeIn(500);
            $(this).val("");
        }
    });
    $("#submit").click(function () {
        var ret = true;
        if ($("#chkForm").validationEngine("validate")) {
            var code = $("#code").val();
            var data = $.ajax({url: "/home/CheckCode.html?code=" + code,async: false}).responseText;
            if (data != "1") {
                $("#code").validationEngine('showPrompt', '验证码错误！', 'error')
                ret = false;
            } else {
                ret = true;
            }
        } else {
            ret = false;
        }
        return ret;
    });
});
var ShowN = function () {
    $(".geji").hide();
    $(".geji").eq(n).show();//显示第几个
    $(".jbabc").eq(n).addClass("jbabc2");
    $(".jbzl").eq(n).addClass("yqsm2");
    $(".yitg").eq(n - 1).css({ backgroundColor: "#5e6a94" })
}
var ShowP = function () {
    $(".geji").hide();
    $(".geji").eq(n).show();//显示第几个
    $(".jbabc").eq(n + 1).removeClass("jbabc2");
    $(".jbzl").eq(n + 1).removeClass("yqsm2");
    $(".yitg").eq(n).css({ backgroundColor: "#ebebeb" })
}
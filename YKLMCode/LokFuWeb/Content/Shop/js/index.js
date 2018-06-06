var intputValue = "";
var Amount;
var Tnum;
var PayWay;
$(function () {
    Amount = $("#Amount");
    $(".enter-left>ul>li:not(.del)>a").click(function () {
        var num = $(this).data("num");
        intputValue += "" + num;
        setVal();
    });
    $(".del").on("click", function () {
        intputValue = intputValue.substring(0, intputValue.length - 1);
        setVal();
    });
    Amount.on("keypress", function (event) {
        event = window.event ? window.event : event;
        var k = event.keyCode ? event.keyCode : event.which;
        if ((k >= 48 && k <= 57) || k == 46 || k == 8) {
        } else {
            return false
        }
        if ((k >= 48 && k <= 57) || k == 46) {
            var str = String.fromCharCode(k);
            intputValue += "" + str;
            setVal();
        } else if (k == 8) {
            intputValue = intputValue.substring(0, intputValue.length - 1);
            setVal();
        }
        return false;
    });
    $(".enter-left>ul>li>a").mousedown(function () {
        $(this).addClass("active");
    }).mouseup(function () {
        $(this).removeClass("active");
    });
    setVal();
    $(".selectmode").click(function () {
        $(this).addClass("active").siblings().removeClass("active");
        var index = $(".selectmode").index($(this));
        $(".pull-right ul").hide().eq(index).show();
    });
    $(".QRCode").click(function () {
        var pay = $(this).data("pay");
        var payway = $(this).data("payway");
        var name = $(this).data("name");
        var snum = $(this).data("snum");
        var Enum = $(this).data("enum");
        if (intputValue < snum || intputValue > Enum || intputValue == 0) {
            shopTip("收款金额范围：" + snum + "~" + Enum + "元");
            return false;
        }
        $("#QRCodeBox").find(".payment").text(name);
        $("#QRCodeBox").find(".money").text(intputValue);
        var Amount = $("#Amount").val();
        $.ajax({
            url: "/shop/orders/qrcodepay.html",
            data: { paytype: pay, amount: Amount, payway: payway },
            type: "post",
            dataType: "json",
            success: function (data) {
                if (data.err == 1) {
                    shopTip(data.msg);
                } else {
                    $("#QRCodeImg").attr("src", data.msg);
                    $("#QRCodeBox").show();
                    Tnum = data.tnum;
                    setTimeout("Query()", 3000);
                }
            }
        });
    });
    $(".Sao").click(function () {
        var pay = $(this).data("pay");
        var payway = $(this).data("payway");
        var name = $(this).data("name");
        var snum = $(this).data("snum");
        var Enum = $(this).data("enum");
        if (intputValue < snum || intputValue > Enum || intputValue == 0) {
            shopTip("收款金额范围：" + snum + "~" + Enum + "元");
            return false;
        }
        $("#SaoBox").find(".payment").text(name);
        $("#SaoMa").val("").data("pay", pay);
        $("#SaoBox").show();
        $("#SaoMa").focus();
        Tnum = "";
        PayWay = payway;
    });
    $("#SaoForm").submit(function () {
        var payid = $("#SaoMa").val();
        var pay = $("#SaoMa").data("pay");
        var Amount = $("#Amount").val();
        if (payid == "") {
            shopTip("请扫描条码或输入条码");
            return false;
        }
        $("#showTips").show();
        $.ajax({
            url: "/shop/orders/codepay.html",
            data: { payid: payid, paytype: pay, amount: Amount, payway: PayWay },
            type:"post",
            dataType: "json",
            success: function (data) {
                if (data.err == 1) {
                    shopTip(data.msg);
                    $("#showTips").hide();
                } else {
                    Tnum = data.tnum;
                    if (data.msg == "OK") {
                        $("#showTips").hide();
                        $("#Success").show();
                    } else {
                        setTimeout("Query()", 3000);
                    }
                }
            }
        });
        return false;
    });
    $(".InfoTipsClose").click(function () {
        $("#InfoTipsBox").hide();
    });
    $(".SuccessClose").click(function () {
        $("#Success").hide();
    });
    $(".CancelBtn").click(function () {
        Cancel();
    });

    //==================================
    //切换T0 T5
    $(".mode-select").on("click", function (ev) {
        ev.stopPropagation();
        $(".mode-list").toggleClass("hide");
    })
    $(".mode-list").on("click", "li", function (ev) {
        ev.stopPropagation();
        var val = $(this).data("intype");
        var text = $(this).find(".recorded").text();
        $.get("SetType.html?intype=" + val, function () {
            $(".mode-span").text(text);
            $(".mode-list").addClass("hide");
        });
    });
    $(document).on("click", function () {
        $(".mode-list").addClass("hide");
    })
    //==================================


});
var setVal = function () {
    var index = intputValue.indexOf(".");
    if (index < 0 || intputValue.length < index + 4) {
        Amount.val(intputValue);
    } else {
        intputValue = intputValue.substring(0, index + 3);
        Amount.val(intputValue);
    }
}
var shopTip = function (txt) {
    $("#InfoTipsMsg").text(txt);
    $("#InfoTipsBox").show();
}
var Query = function () {
    $.ajax({
        url: "/shop/orders/query.html",
        data: { tnum: Tnum },
        type: "post",
        dataType:"json",
        success: function (data) {
            if (data.err == 1) {
                shopTip(data.msg);
            } else {
                if (data.msg == "OK") {
                    $("#showTips").hide();
                    $("#SaoBox").hide();
                    $("#QRCodeBox").hide();
                    $("#Success").show();
                } else if (data.msg == "Fail") {
                    $("#showTips").hide();
                } else {
                    setTimeout("Query()", 3000);
                }
            }
        }
    });
}
var Cancel = function () {
    if (Tnum == "") {
        $("#SaoBox").hide();
        $("#QRCodeBox").hide();
    } else {
        $("#SaoBox").hide();
        $("#QRCodeBox").hide();
        $.ajax({
            url: "/shop/orders/cancel.html",
            data: { tnum: Tnum },
            type: "post",
            dataType: "json",
            success: function (data) {
                if (data.err == 1) {
                    shopTip(data.msg);
                } else {
                }
            }
        });
    }
}


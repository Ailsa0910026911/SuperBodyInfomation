$(function () {
    var divwith = $("#ad").width();
    $("#ad").height(divwith / 3.25);

    $(".right_box .sp02").each(function(){
        var h = $(this).height();
        var s = $(this).find("span")
        var sh = s.height();
        s.css("margin-top", (h - sh) / 2);
    });
    //微信单通道
    $("#goPay").on("touchstart", function () {
        var value = $("#amount").html();
        var num = /^[0-9]+([.][0-9]+){0,1}$/;
        if (num.test(value)) {
            if (value > 0) {
                var SNum = $(this).data("snum");
                var ENum = $(this).data("enum");
                if (value < SNum || value > ENum) {
                    alert("收款金额范围：" + SNum + "~" + ENum + "元！");
                    return false;
                } else {
                    return true;
                }
            }
        }
        return false;
    });
    //微信多通道
    $("#goPayMore").on("touchstart", function () {
        var value = $("#amount").html();
        var num = /^[0-9]+([.][0-9]+){0,1}$/;
        if (num.test(value)) {
            if (value > 0) {
                var ons = $("#WeiXinChang span.on");
                var SNum = ons.data("snum");
                var ENum = ons.data("enum");
                if (value < SNum || value > ENum) {
                    alert("收款金额范围：" + SNum + "~" + ENum + "元！");
                    return false;
                }
                return true;
            }
        }
        return false;
    });
    //其它支付通道
    $(".GoPay").on("touchstart", function () {
        var obj = $(this);
        var value = $("#amount").html();
        var payway = obj.data("payway");
        var num = /^[0-9]+([.][0-9]+){0,1}$/;
        if (num.test(value)) {
            if (value > 0) {
                var SNum = obj.data("snum");
                var ENum = obj.data("enum");
                if (value < SNum || value > ENum) {
                    alert("收款金额范围：" + SNum + "~" + ENum + "元！");
                    return false;
                }
                $("#payway").val(payway);
                return true;
            }
        }
        return false;
    });
    var IsClick = false;
    $("#WeiXinChang span").on("touchstart", function () {
        if (!IsClick)
        {
            var obj = $(this);
            if (obj.hasClass("on")) {
                return false;
            }
            obj.addClass("on").siblings().removeClass("on");
            var payway = obj.data("payway");
            IsClick = true;
            location.href = "setpayway.html?uid=" + UsersId + "&payway=" + payway ;
        }
    });
    $("#WeiXinChang span").each(function () {
        var payway = $(this).data("payway");
        if (payway == PayWay) {
            $(this).addClass("on");
        }
    }) 
});



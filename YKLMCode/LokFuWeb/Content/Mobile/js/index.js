$(function () {
    var $form = $(".form"),
	    $phone = $(".phone"),
		  $codes = $(".codes");
    $form.on("submit", function () {
        var $pass = $(".pass"),
                $passoword = $(".password"),
                $submit = $(".submit"),
                $point = $(".point");
        var myreg = "^((13[0-9])|(14[5|7])|(15([0-3]|[5-9]))|(18[0,5-9]))\\d{8}$";
        //var myreg = /^(((13[0-9]{1})|(15[0-9]{1})|(18[0-9]{1}))+\d{8})$/;
        var span = $point.find("span");
        var obj = {
            air: function (event, id, content) {
                var val = id.val();
                if (val == "" || val == null) {
                    $point.removeClass("hidden");
                    span.text(content);
                    event.preventDefault();
                }
            },
            phone: function (event) {
                var val = $phone.val();
                if (!myreg.test(val)) {
                    $point.removeClass("hidden");
                    span.text("请输入有效手机号码！");
                    event.preventDefault();
                }
            },
            pass: function () {
                var val = $pass.val();
                var passval = $passoword.val();
                if (passval !== val) {
                    $point.removeClass("hidden");
                    span.text("两次密码输入不一致！");
                    event.preventDefault();
                }
            },
            passwo: function (event) {
                var val = $pass.val();
                var tex = /^(?![0-9]+$)(?![a-zA-Z]+$)[0-9A-Za-z]{8,20}$/;
                if (!tex.test(val)) {
                    $point.removeClass("hidden");
                    span.text("密码请输入8-20位字母与数字组合！");
                    event.preventDefault();
                }
            }
        }
        obj.pass(event, $passoword, "两次输入密码不一致");
        obj.passwo(event);
        obj.air(event, $codes, "验证码不能为空!");
        obj.phone(event);
        obj.air(event, $phone, "手机号码不能为空!");
    })
    $form.on("input", ".phone , .codes", function () {
        var val = $(this).val();
        val = val.replace(/\D/g, "");
        $(this).val(val);
    })
})
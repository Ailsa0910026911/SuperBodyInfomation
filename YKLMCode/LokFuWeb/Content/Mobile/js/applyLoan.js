$(function(){
	var $list = $(".list");
	var $bank = $(".list-bank");
	$list.find("div").click(function(){
		$(this).addClass("active");
		$(this).parent().parent().siblings().find("div").removeClass("active");
	})
	//$bank.on("click", "li", function () {
	//    $(this).find("i").toggleClass("text-success");
	//})
    $(".list-bank li").toggle(
        function () {
            $(this).find("input[name='Bank']").attr("checked", true);
            $(this).find("i").addClass("text-success");
            if ($bank.find(":checked").length > 0)
            {
                $(".btn-Bank").attr("disabled", false).css("background", "#fe7620");
            }
        },
        function () {
            $(this).find("input[name='Bank']").attr("checked", false);
            $(this).find("i").removeClass("text-success");
            if ($bank.find(":checked").length <= 0) {
                $(".btn-Bank").attr("disabled", true).css("background", "grey");
            }
        }
    );
	var $province = $(".province");
	$province.on("change", function () {
	    if ($(this).val() != "请选择") {
	        $(".city-two").removeClass("hide");
	        $(this).parents("li").addClass("city");
	    } else {
	        $(".city-two").addClass("hide");
	        $(this).parents("li").removeClass("city");
	    }
	});
    //正则
	NumberReg = /^((([0-9]{1,3})([,][0-9]{3})*)|([0-9]+))?([\.]([0-9]+))?$/;
	Chinese = /^[\u4e00-\u9fa5]+$/;
	ChinaId = /^[1-9]\d{5}[1-9]\d{3}(((0[13578]|1[02])(0[1-9]|[12]\d|3[0-1]))|((0[469]|11)(0[1-9]|[12]\d|30))|(02(0[1-9]|[12]\d)))(\d{4}|\d{3}[xX])$/;
	Money = /^(0|[1-9][0-9]*)(.[0-9]{1,2})?$/;
	Phone = /^([\+][0-9]{1,3}[ \.\-])?([\(]{1}[0-9]{2,6}[\)])?([0-9 \.\-\/]{3,20})((x|ext|extension)[ ]?[0-9]{1,4})?$/;
	$(".btn-Bank").click(function () {
	    $('#credit').hide();
	    $('#index').show('normal');
	});
	$(".btn-index").click(function () {
	    var checkkey = new Array("Sex", "Education", "Marry");
	    var datas = $("#index :input").serializeArray();
	    var validateNo = 0;
	    $.each(checkkey, function (i, n) {
	        var isValidate = Required(n, ToArray(datas), datas);
	        if (!isValidate) {
	            ShowMessage("index-message", "*您还有信息未填写");
	        }
	        else { validateNo += 1; }
	    });
	    if (validateNo == checkkey.length)
	    {
	        $('#index').hide();
	        $('#jobs').show('normal');
	    }
	});
	$(".btn-jobs").click(function () {
	    var checkkey = new Array("Company", "CompanyNature", "SheBao");
	    var data = $("#jobs :input").serializeArray();
	    var validateNo = 0;
	    $.each(checkkey, function (i, n) {
	        var isValidate = Required(n, ToArray(data), data);
	        if (!isValidate) {
	            ShowMessage("jobs-message", "*您还有信息未填写");
	        }
	        else { validateNo += 1; }
	    });
	    if (validateNo == checkkey.length) {
	        $('#jobs').hide();
	        $('#assets').show('normal');
	    }
	});
	$(".btn-assets").click(function () {
	    var checkkey = new Array("Income", "HasCar", "House", "HasCredit");
	    var datas = $("#assets :input").serializeArray();
	    var validateNo = 0;
	    $.each(checkkey, function (i, n) {
	        var isValidate = Required(n, ToArray(datas), datas);
	        if (!isValidate) {
	            ShowMessage("assets-message", "*您还有信息未填写");
	        }
	        else {
	            if (n == "Income" && !Money.test(datas[i].value)) {
	                ShowMessage("assets-message", "*请为年收入输入正确数字");
	                return false;
	            }
	            validateNo += 1;
	        }
	    });
	    if (validateNo == checkkey.length) {
	        $('#assets').hide();
	        $('#subdata').show('normal');
	    }
	});
	$(".btn-subdata").click(function () {
	    var checkkey = new Array("TrueName", "IDcard", "ComProvince", "ComCity", "ComAddress", "Mobile", "Amount", "code");
	    var datas = $("#subdata :input").serializeArray();
	    var validateNo = 0;
	    $.each(checkkey, function (i, n) {
	        var isValidate = Required(n, ToArray(datas), datas);
	        if (!isValidate) {
	            ShowMessage("subdata-message", "*您还有信息未填写");
	        }
	        else {
	            var value = datas[i].value;
	            if (n == "TrueName" && !Chinese.test(value)) {
	                ShowMessage("subdata-message", "*请为姓名，输入中文");
	                return false;
	            }
	            if (n == "IDcard" && !ChinaId.test(value)) {
	                ShowMessage("subdata-message", "*请正确输入身份证号码");
	                return false;
	            }
	            if (n == "Mobile" && !Phone.test(value)) {
	                ShowMessage("subdata-message", "*请正确输入手机号码");
	                return false;
	            }
	            if (n == "Amount" && !Money.test(value)) {
	                ShowMessage("subdata-message", "*请正确输入货款金额");
	                return false;
	            }
	            if (n == "code") {
	                var result = true;
	                $.ajax({
	                    type: "get",
	                    url: "/mobile/applyLoan/CodeIsTrue.html",
	                    data: { code: value },
	                    async: false,
	                    dataType:"text",
	                    success: function (o) {
	                        if (o == "False") {
	                            ShowMessage("subdata-message", "*验证码不正确");
	                            result = false;
	                        }
	                    }
	                });
	                if (!result)
	                {
	                    return false;
	                }
	            }
	            validateNo += 1;
	        }
	    });
	    if (validateNo == checkkey.length) {
	        $("#ApplyLoanForm").submit();
	    }
	});
	function ToArray(data)
	{
	    var ret = new Array();
	    $.each(data, function (i, n) {
	        ret.push(n.name);
	    });
	    return ret;
	}
	function Required(key, keys, datas)
	{
	    var index = $.inArray(key, keys);
	    if (index == -1) {
	        return false;
	    }
	    else {
	        if (datas[index].value == "") {
	            return false;
	        }
	        else { return true; }
	    }
	}
	function ShowMessage(id,message)
	{
	    $("#" + id).text(message);
	    $("#" + id).show();
	    setTimeout(function () {
	        $("#" + id).hide();
	    }, 3000);
	}
})
function Prev(oid, nowid) {
    $('#' + nowid).hide();
    $('#' + oid).show('normal');
}
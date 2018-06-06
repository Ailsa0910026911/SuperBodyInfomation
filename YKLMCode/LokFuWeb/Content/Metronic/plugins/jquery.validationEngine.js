(function ($) {
    var methods = {
        init: function (options) { var form = this; if (!form.data('jqv') || form.data('jqv') == null) { methods._saveOptions(form, options); $(".formError").live("click", function () { $(this).fadeOut(150, function () { $(this).remove(); }); }); } }, attach: function (userOptions) {
            var form = this; var options; if (userOptions)
                options = methods._saveOptions(form, userOptions); else
                options = form.data('jqv'); var validateAttribute = (form.find("[data-validation-engine*=validate]")) ? "data-validation-engine" : "class"; if (!options.binded) {
                    if (options.bindMethod == "bind") { form.find("[class*=validate]:not([type=checkbox])").bind(options.validationEventTrigger, methods._onFieldEvent); form.find("[class*=validate][type=checkbox]").bind("click", methods._onFieldEvent); form.bind("submit", methods._onSubmitEvent); } else if (options.bindMethod == "live") { form.find("[class*=validate]:not([type=checkbox])").live(options.validationEventTrigger, methods._onFieldEvent); form.find("[class*=validate][type=checkbox]").live("click", methods._onFieldEvent); form.live("submit", methods._onSubmitEvent); }
                    options.binded = true;
                }
        }, detach: function () { var form = this; var options = form.data('jqv'); if (options.binded) { form.find("[class*=validate]").not("[type=checkbox]").unbind(options.validationEventTrigger, methods._onFieldEvent); form.find("[class*=validate][type=checkbox]").unbind("click", methods._onFieldEvent); form.unbind("submit", methods.onAjaxFormComplete); form.find("[class*=validate]").not("[type=checkbox]").die(options.validationEventTrigger, methods._onFieldEvent); form.find("[class*=validate][type=checkbox]").die("click", methods._onFieldEvent); form.die("submit", methods.onAjaxFormComplete); form.removeData('jqv'); } }, validate: function () { return methods._validateFields(this); }, validateField: function (el) { var options = $(this).data('jqv'); return methods._validateField($(el), options); }, validateform: function () { return methods._onSubmitEvent.call(this); }, showPrompt: function (promptText, type, promptPosition, showArrow) {
            var form = this.closest('form'); var options = form.data('jqv'); if (!options) options = methods._saveOptions(this, options); if (promptPosition)
                options.promptPosition = promptPosition; options.showArrow = showArrow == true; methods._showErrInfo(this, promptText, type, false, options);
        }, hidePrompt: function () { var promptClass = "." + methods._getClassName($(this).attr("id")) + "formError"; $(promptClass).fadeTo("fast", 0.3, function () { $(this).remove(); }); }, hide: function () {
            var closingtag; if ($(this).is("form")) { closingtag = "parentForm" + $(this).attr('id'); } else { closingtag = $(this).attr('id') + "formError"; }
            $('.' + closingtag).fadeTo("fast", 0.3, function () { $(this).remove(); });
        }, hideAll: function () { $('.formError').fadeTo("fast", 0.3, function () { $(this).remove(); }); }, _onFieldEvent: function () { var field = $(this); var form = field.closest('form'); var options = form.data('jqv'); methods._validateField(field, options); }, _onSubmitEvent: function () {
            var form = $(this); var options = form.data('jqv'); var r = methods._validateFields(form, true); if (r && options.ajaxFormValidation) { methods._validateFormWithAjax(form, options); return false; }
            if (options.onValidationComplete) { options.onValidationComplete(form, r); return false; }
            return r;
        }, _checkAjaxStatus: function (options) { var status = true; $.each(options.ajaxValidCache, function (key, value) { if (!value) { status = false; return false; } }); return status; }, _validateFields: function (form, skipAjaxValidation) {
            var options = form.data('jqv'); var errorFound = false; form.trigger("jqv.form.validating"); form.find('[class*=validate]').not(':hidden').each(function () { var field = $(this); errorFound |= methods._validateField(field, options, skipAjaxValidation); }); form.trigger("jqv.form.result", [errorFound]); if (errorFound) {
                if (options.scroll) {
                    var destination = Number.MAX_VALUE; var fixleft = 0; var lst = $(".formError:not('.greenPopup')"); for (var i = 0; i < lst.length; i++) { var d = $(lst[i]).offset().top; if (d < destination) { destination = d; fixleft = $(lst[i]).offset().left; } }
                    if (!options.isOverflown)
                        $("html:not(:animated),body:not(:animated)").animate({ scrollTop: destination, scrollLeft: fixleft }, 1100); else { var overflowDIV = $(options.overflownDIV); var scrollContainerScroll = overflowDIV.scrollTop(); var scrollContainerPos = -parseInt(overflowDIV.offset().top); destination += scrollContainerScroll + scrollContainerPos - 5; var scrollContainer = $(options.overflownDIV + ":not(:animated)"); scrollContainer.animate({ scrollTop: destination }, 1100); $("html:not(:animated),body:not(:animated)").animate({ scrollTop: overflowDIV.offset().top, scrollLeft: fixleft }, 1100); }
                }
                return false;
            }
            return true;
        }, _validateFormWithAjax: function (form, options) {
            var data = form.serialize(); var url = (options.ajaxFormValidationURL) ? options.ajaxFormValidationURL : form.attr("action"); $.ajax({
                type: "GET", url: url, cache: false, dataType: "json", data: data, form: form, methods: methods, options: options, beforeSend: function () { return options.onBeforeAjaxFormValidation(form, options); }, error: function (data, transport) { methods._ajaxError(data, transport); }, success: function (json) {
                    if (json !== true) {
                        var errorInForm = false; for (var i = 0; i < json.length; i++) {
                            var value = json[i]; var errorFieldId = value[0]; var errorField = $($("#" + errorFieldId)[0]); if (errorField.length == 1) {
                                var msg = value[2]; if (value[1] == true) {
                                    if (msg == "" || !msg) { methods._closeErrInfo(errorField); } else {
                                        if (options.allrules[msg]) {
                                            var txt = options.allrules[msg].alertTextOk; if (txt)
                                                msg = txt;
                                        }
                                        methods._showErrInfo(errorField, msg, "pass", false, options, true);
                                    }
                                } else {
                                    errorInForm |= true; if (options.allrules[msg]) {
                                        var txt = options.allrules[msg].alertText; if (txt)
                                            msg = txt;
                                    }
                                    methods._showErrInfo(errorField, msg, "", false, options, true);
                                }
                            }
                        }
                        options.onAjaxFormComplete(!errorInForm, form, json, options);
                    } else
                        options.onAjaxFormComplete(true, form, "", options);
                }
            });
        }, _validateField: function (field, options, skipAjaxValidation) {
            if (!field.attr("id"))
                $.error("jQueryValidate: an ID attribute is required for this field: " + field.attr("name") + " class:" +
                field.attr("class")); var rulesParsing = field.attr('class'); var getRules = /validate\[(.*)\]/.exec(rulesParsing); if (!getRules)
                    return false; var str = getRules[1]; var rules = str.split(/\[|,|\]/); var isAjaxValidator = false; var fieldName = field.attr("name"); var promptText = ""; var required = false; options.isError = false; options.showArrow = true; for (var i = 0; i < rules.length; i++) {
                        var errorMsg = undefined; switch (rules[i]) {
                            case "required": required = true; errorMsg = methods._required(field, rules, i, options); break; case "custom": errorMsg = methods._customRegex(field, rules, i, options); break; case "ajax": if (!skipAjaxValidation) { methods._ajax(field, rules, i, options); isAjaxValidator = true; }
                                break; case "minSize": errorMsg = methods._minSize(field, rules, i, options); break; case "maxSize": errorMsg = methods._maxSize(field, rules, i, options); break; case "min": errorMsg = methods._min(field, rules, i, options); break; case "max": errorMsg = methods._max(field, rules, i, options); break; case "past": errorMsg = methods._past(field, rules, i, options); break; case "future": errorMsg = methods._future(field, rules, i, options); break; case "maxCheckbox": errorMsg = methods._maxCheckbox(field, rules, i, options); field = $($("input[name='" + fieldName + "']")); break; case "minCheckbox": errorMsg = methods._minCheckbox(field, rules, i, options); field = $($("input[name='" + fieldName + "']")); break; case "equals": errorMsg = methods._equals(field, rules, i, options); break; case "funcCall": errorMsg = methods._funcCall(field, rules, i, options); break; default: }
                        if (errorMsg !== undefined) { promptText += errorMsg + "　"; options.isError = true; }
                    }
            if (!required) { if (field.val() == "") options.isError = false; }
            var fieldType = field.attr("type"); if ((fieldType == "radio" || fieldType == "checkbox") && $("input[name='" + fieldName + "']").size() > 1) { field = $($("input[name='" + fieldName + "'][type!=hidden]:first")); options.showArrow = false; }
            if (options.isError) { methods._showErrInfo(field, promptText, "", false, options); } else { if (!isAjaxValidator) methods._closeErrInfo(field); }
            field.trigger("jqv.field.result", [field, options.isError, promptText]); return options.isError;
        }, _required: function (field, rules, i, options) {
            switch (field.attr("type")) {
                case "text": case "password": case "textarea": case "file": default: if (!field.val())
                    return options.allrules[rules[i]].alertText; break; case "radio": case "checkbox": var name = field.attr("name"); if ($("input[name='" + name + "']:checked").size() == 0) {
                        if ($("input[name='" + name + "']").size() == 1)
                            return options.allrules[rules[i]].alertTextCheckboxe; else
                            return options.allrules[rules[i]].alertTextCheckboxMultiple;
                    }
                        break; case "select-one": if (!field.val())
                            return options.allrules[rules[i]].alertSelectFirst; break; case "select-multiple": if (!field.find("option:selected").val())
                                return options.allrules[rules[i]].alertSelectFirst; break; case "select-nofirst": if (field.find("option:selected").index() <= 0)
                                    return options.allrules[rules[i]].alertSelectFirst; break;
            }
        }, _customRegex: function (field, rules, i, options) {
            var customRule = rules[i + 1]; var rule = options.allrules[customRule]; if (!rule) { alert("验证规则未找到 " + customRule); return; }
            var ex = rule.regex; if (!ex) { alert("验证规则未找到 " + customRule); return; }
            var pattern = new RegExp(ex); if (!pattern.test(field.val()))
                return options.allrules[customRule].alertText;
        }, _funcCall: function (field, rules, i, options) {
            var functionName = rules[i + 1]; var fn = window[functionName]; if (typeof (fn) == 'function')
                return fn(field, rules, i, options);
        }, _equals: function (field, rules, i, options) {
            var equalsField = rules[i + 1]; if (field.val() != $("#" + equalsField).val())
                return options.allrules.equals.alertText;
        }, _maxSize: function (field, rules, i, options) { var max = rules[i + 1]; var len = field.val().length; if (len > max) { var rule = options.allrules.maxSize; return rule.alertText + max + rule.alertText2; } }, _minSize: function (field, rules, i, options) { var min = rules[i + 1]; var len = field.val().length; if (len < min) { var rule = options.allrules.minSize; return rule.alertText + min + rule.alertText2; } }, _min: function (field, rules, i, options) { var min = parseFloat(rules[i + 1]); var len = parseFloat(field.val()); if (len < min) { var rule = options.allrules.min; if (rule.alertText2) return rule.alertText + min + rule.alertText2; return rule.alertText + min; } }, _max: function (field, rules, i, options) { var max = parseFloat(rules[i + 1]); var len = parseFloat(field.val()); if (len > max) { var rule = options.allrules.max; if (rule.alertText2) return rule.alertText + max + rule.alertText2; return rule.alertText + max; } }, _past: function (field, rules, i, options) { var p = rules[i + 1]; var pdate = (p.toLowerCase() == "now") ? new Date() : methods._parseDate(p); var vdate = methods._parseDate(field.val()); if (vdate < pdate) { var rule = options.allrules.past; if (rule.alertText2) return rule.alertText + methods._dateToString(pdate) + rule.alertText2; return rule.alertText + methods._dateToString(pdate); } }, _future: function (field, rules, i, options) { var p = rules[i + 1]; var pdate = (p.toLowerCase() == "now") ? new Date() : methods._parseDate(p); var vdate = methods._parseDate(field.val()); if (vdate > pdate) { var rule = options.allrules.future; if (rule.alertText2) return rule.alertText + methods._dateToString(pdate) + rule.alertText2; return rule.alertText + methods._dateToString(pdate); } }, _maxCheckbox: function (field, rules, i, options) { var nbCheck = rules[i + 1]; var groupname = field.attr("name"); var groupSize = $("input[name='" + groupname + "']:checked").size(); if (groupSize > nbCheck) { options.showArrow = false; if (options.allrules.maxCheckbox.alertText2) return options.allrules.maxCheckbox.alertText + " " + nbCheck + " " + options.allrules.maxCheckbox.alertText2; return options.allrules.maxCheckbox.alertText; } }, _minCheckbox: function (field, rules, i, options) {
            var nbCheck = rules[i + 1]; var groupname = field.attr("name"); var groupSize = $("input[name='" + groupname + "']:checked").size(); if (groupSize < nbCheck) {
                options.showArrow = false; return options.allrules.minCheckbox.alertText + " " + nbCheck + " " +
                options.allrules.minCheckbox.alertText2;
            }
        }, _ajax: function (field, rules, i, options) {
            var errorSelector = rules[i + 1]; var rule = options.allrules[errorSelector]; var extraData = rule.extraData; var extraDataDynamic = rule.extraDataDynamic; if (!extraData)
                extraData = field.attr("data-ext"); if (!extraData)
                    extraData = ""; if (extraDataDynamic) {
                        var tmpData = []; var domIds = String(extraDataDynamic).split(","); for (var i = 0; i < domIds.length; i++) { var id = domIds[i]; if ($(id).length) { var inputValue = field.closest("form").find(id).val(); var keyValue = id.replace('#', '') + '=' + escape(inputValue); tmpData.push(keyValue); } }
                        extraDataDynamic = tmpData.join("&");
                    } else { extraDataDynamic = ""; }
            if (!options.isError) {
                $.ajax({
                    type: "GET", url: rule.url, cache: false, dataType: "json", data: "fieldId=" + field.attr("id") + "&fieldValue=" + field.val() + "&extraData=" + extraData + "&" + extraDataDynamic, field: field, rule: rule, methods: methods, options: options, beforeSend: function () {
                        var loadingText = rule.alertTextLoad; if (loadingText)
                            methods._showErrInfo(field, loadingText, "load", true, options);
                    }, error: function (data, transport) { methods._ajaxError(data, transport); }, success: function (json) {
                        var errorFieldId = json[0]; var errorField = $($("#" + errorFieldId)[0]); if (errorField.length == 1) {
                            var status = json[1]; var msg = json[2]; if (!status) {
                                options.ajaxValidCache[errorFieldId] = false; options.isError = true; if (msg) {
                                    if (options.allrules[msg]) {
                                        var txt = options.allrules[msg].alertText; if (txt)
                                            msg = txt;
                                    }
                                }
                                else
                                    msg = rule.alertText; methods._showErrInfo(errorField, msg, "", true, options);
                            } else {
                                if (options.ajaxValidCache[errorFieldId] !== undefined)
                                    options.ajaxValidCache[errorFieldId] = true; if (msg) {
                                        if (options.allrules[msg]) {
                                            var txt = options.allrules[msg].alertTextOk; if (txt)
                                                msg = txt;
                                        }
                                    }
                                    else
                                        msg = rule.alertTextOk; if (msg)
                                            methods._showErrInfo(errorField, msg, "pass", true, options); else
                                            methods._closeErrInfo(errorField);
                            }
                        }
                    }
                });
            }
        }, _showErrInfo: function (field, promptText, type, ajaxed, options, ajaxform) {
            var Alert = field.attr("alert");
            if (!Alert) {
                Alert = field.attr("placeholder");
            }
            if (Alert) {
                promptText = Alert;
            }
            var box = field.closest(".form-group").find(".help-block");
            if (box.length == 0) {
                if (field.closest(".form-group").find(".col-md-4").length > 0) {
                    field.closest(".col-md-4").append("<span class=\"help-block\"></span>");
                    box = field.closest(".col-md-4").find(".help-block");
                }
            }
            box.html(promptText).show();
            if (type == "pass") {
                field.closest(".form-group").removeClass("has-error").addClass("has-success").find(".fa").removeClass("fa-warning").addClass("fa-check");
            } else {
                field.closest(".form-group").removeClass("has-success").addClass("has-error").find(".fa").removeClass("fa-check").addClass("fa-warning");
            }
        }, _closeErrInfo: function (field) {
            field.closest(".form-group").removeClass("has-error").addClass("has-success").find(".fa").removeClass("fa-warning").addClass("fa-check")
                .end().find(".help-block").hide();
        }, _ajaxError: function (data, transport) {
            if (data.status == 0 && transport == null)
                alert("The page is not served from a server! ajax call failed"); else if (typeof console != "undefined")
                    console.log("Ajax error: " + data.status + " " + transport);
        }, _dateToString: function (date) { return date.getFullYear() + "-" + (date.getMonth() + 1) + "-" + date.getDate(); }, _parseDate: function (d) {
            var dateParts = d.split("-"); if (dateParts == d)
                dateParts = d.split("/"); return new Date(dateParts[0], (dateParts[1] - 1), dateParts[2]);
        }, _calculatePosition: function (field, promptElmt, options) {
            var promptTopPosition, promptleftPosition, marginTopSize; var fieldWidth = field.width(); var promptHeight = promptElmt.height(); var overflow = options.isOverflown; if (overflow) { promptTopPosition = promptleftPosition = 0; marginTopSize = -promptHeight; } else { var offset = field.offset(); promptTopPosition = offset.top; promptleftPosition = offset.left; marginTopSize = 0; }
            switch (options.promptPosition) {
                default: case "topRight": if (overflow)
                    promptleftPosition += fieldWidth - 30; else { promptleftPosition += fieldWidth - 30; promptTopPosition += -promptHeight; }
                    break; case "topLeft": promptTopPosition += -promptHeight - 10; break; case "centerRight": promptleftPosition += fieldWidth + 13; break; case "bottomLeft": promptTopPosition = promptTopPosition + field.height() + 5; break; case "bottomRight": promptleftPosition += fieldWidth - 30; promptTopPosition += field.height() + 5;
            }
            return { "callerTopPosition": promptTopPosition + "px", "callerleftPosition": promptleftPosition + "px", "marginTopSize": marginTopSize + "px" };
        }, _saveOptions: function (form, options) {
            if ($.validationEngineLanguage)
                var allRules = $.validationEngineLanguage.allRules; else
                $.error("jQuery.validationEngine rules are not loaded, plz add localization files to the page"); var userOptions = $.extend({ validationEventTrigger: "blur", scroll: true, promptPosition: "bottomLeft", bindMethod: "bind", inlineAjax: false, ajaxFormValidation: false, ajaxFormValidationURL: false, onAjaxFormComplete: $.noop, onBeforeAjaxFormValidation: $.noop, onValidationComplete: false, isOverflown: false, overflownDIV: "", allrules: allRules, binded: false, showArrow: true, isError: false, ajaxValidCache: {} }, options); form.data('jqv', userOptions); return userOptions;
        }, _getClassName: function (className) { return className.replace(":", "_").replace(".", "_"); }
    }; $.fn.validationEngine = function (method) {
        var form = $(this); if (!form[0]) return false; if (typeof (method) == 'string' && method.charAt(0) != '_' && methods[method]) {
            if (method != "showPrompt" && method != "hidePrompt" && method != "hide" && method != "hideAll")
                methods.init.apply(form); return methods[method].apply(form, Array.prototype.slice.call(arguments, 1));
        } else if (typeof method == 'object' || !method) { methods.init.apply(form, arguments); return methods.attach.apply(form); } else { $.error('Method ' + method + ' does not exist in jQuery.validationEngine'); }
    };
})(jQuery);
(function ($) {
    $.fn.validationEngineLanguage = function () { };
    $.validationEngineLanguage = {
        newLang: function () {
            $.validationEngineLanguage.allRules = {
                "username": {
                    "regex": /(^[A-Za-z]\w{2,14}|((\(\d{2,3}\))|(\d{3}\-))?1[3,5,8]\d{9}$)/,
                    "alertText": "* 登录帐户须以字母开头的3～14个字符， "
                },
                "money": {
                    "regex": /^(0|[1-9][0-9]*)(.[0-9]{1,2})?$/,
                    "alertText": "* 请正确输入金额 "
                },
                "IdCard": {
                    "regex": /^[1-9]\d{5}[1-9]\d{3}(((0[13578]|1[02])(0[1-9]|[12]\d|3[0-1]))|((0[469]|11)(0[1-9]|[12]\d|30))|(02(0[1-9]|[12]\d)))(\d{4}|\d{3}[xX])$/,
                    "alertText": "* 请正确输入身份证号码 "
                },
                "phone": {
                    "regex": /^([\+][0-9]{1,3}[ \.\-])?([\(]{1}[0-9]{2,6}[\)])?([0-9 \.\-\/]{3,20})((x|ext|extension)[ ]?[0-9]{1,4})?$/,
                    "alertText": "* 请输入有效的电话号码,如:010-29292929."
                },
                "pwd": {
                    "regex": /^\w{6,20}$/,
                    "alertText": "* 请输入6~20位密码."
                },
                "required": {
                    "regex": "none",
                    "alertText": "* 请输入内容.",
                    "alertTextCheckboxMultiple": "* 请选择一个单选框 ",
                    "alertTextCheckboxe": "* 请至少选择一个复选框 ",
                    "alertSelectFirst": "* 请选择内容"
                },
                "length": {
                    "regex": "none",
                    "alertText": "* 长度必须在 ",
                    "alertText2": " 至 ",
                    "alertText3": " 之间."
                },
                "maxCheckbox": {
                    "regex": "none",
                    "alertText": "* 最多选择 ",
                    "alertText2": " 项."
                },
                "minCheckbox": {
                    "regex": "none",
                    "alertText": "* 至少选择 ",
                    "alertText2": " 项."
                },
                "confirm": {
                    "regex": "none",
                    "alertText": "* 两次输入不一致,请重新输入."
                },
                "mobile": {
                    "regex": /^((\(\d{2,3}\))|(\d{3}\-))?1[3,5,8]\d{9}$/,
                    "alertText": "* 请输入有效的手机号码."
                },
                "email": {
                    "regex": /^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$/,
                    "alertText": "* 请输入有效的邮件地址."
                },
                "date": {
                    "regex": /^\d{4}[\/\-](0?[1-9]|1[012])[\/\-](0?[1-9]|[12][0-9]|3[01])$/,
                    "alertText": "* 请输入有效的日期,如:2011-11-11."
                },
                "ip": {
                    "regex": /^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/,
                    "alertText": "* 请输入有效的IP "
                },
                "chinese": {
                    "regex": /^[\u4e00-\u9fa5]+$/,
                    "alertText": "* 请输入中文 "
                },
                "url": {
                    "regex": /^(https?|ftp):\/\/(((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:)*@)?(((\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5]))|((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?)(:\d*)?)(\/((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)+(\/(([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)*)*)?)?(\?((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|[\uE000-\uF8FF]|\/|\?)*)?(\#((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|\/|\?)*)?$/i,
                    "alertText": "* 请输入有效的网址 "
                },
                "zipcode": {
                    "regex": /^[1-9]\d{5}$/,
                    "alertText": "* 请输入有效的邮政编码 "
                },
                "qq": {
                    "regex": /^[1-9]\d{4,9}$/,
                    "alertText": "* 请输入有效的QQ号码 "
                },
                "minSize": {
                    "regex": "none",
                    "alertText": "* 最少输入 ",
                    "alertText2": " 个字符"
                },
                "maxSize": {
                    "regex": "none",
                    "alertText": "* 最多允许输入 ",
                    "alertText2": " 个字符"
                },
                "min": {
                    "regex": "none",
                    "alertText": "* 最小值为 "
                },
                "max": {
                    "regex": "none",
                    "alertText": "* 最大值为 "
                },
                "past": {
                    "regex": "none",
                    "alertText": "* 请输入未来日期 大于"
                },
                "future": {
                    "regex": "none",
                    "alertText": "* 请输入过去日期 小于"
                },
                "equals": {
                    "regex": "none",
                    "alertText": "* 两次输入不一致,请重新输入"
                },
                "int": {
                    "regex": /^[\-\+]?\d+$/,
                    "alertText": "* 请输入整数 "
                },
                "float": {
                    "regex": /^[\-\+]?(([0-9]+)([\.,]([0-9]+))?|([\.,]([0-9]+))?)$/,
                    "alertText": "* 请输入浮点数 "
                },
                "onlyNumberSp": {
                    "regex": /^[0-9\ ]+$/,
                    "alertText": "* 请输入数字 "
                },
                "onlyLetterSp": {
                    "regex": /^[a-zA-Z\ \']+$/,
                    "alertText": "* 请输入英文字母 "
                },
                "onlyLetterNumber": {
                    "regex": /^[0-9a-zA-Z]+$/,
                    "alertText": "* 请输入英文字母和数字 "
                },
                "check": {
                    "url": "../asyn/check.html",//ajax验证
                    "alertTextOk": "* 登录名可以使用。",
                    "alertTextLoad": "* 检查重复中, 请稍后...",
                    "alertText": "* 登录名重复，不可使用。"
                },
                "telephone": {
                    "regex": /^(\d{3,4}|\d{3,4}-)?\d{7,8}|(400|800)([0-9\\-]{7,10})|((\(\d{2,3}\))|(\d{3}\-))?1[3,5,8]\d{9}$/,
                    "alertText": "* 请输入有效的固定电话号码 "
                },
                'CheckCardNumber': {
                    'url': '../UserBlackList/EditCheckCardNumber.html', /* 验证程序地址 */
                    'alertTextOk': '该号码可以添加',
                    'alertText': '*该限制号码已存在',
                    'alertTextLoad': '* 正在确认该限制号码是否存在。'
                },
                'Checktelephone': {
                    'url': '../SysAgent/EditChecktelephone.html', /* 验证程序地址 */
                    'alertTextOk': '该号码可以添加',
                    'alertText': '*该号码禁止使用',
                    'alertTextLoad': '* 正在确认该号码是否可以使用。'
                }
            };
        }
    };
    $.validationEngineLanguage.newLang();
})(jQuery);
$(function () {
    $(".chkForm").validationEngine();
});
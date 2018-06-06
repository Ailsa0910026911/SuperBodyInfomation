/*	
	//方法名，请按照字母排序
	//日期为最后修改日期
	
	支持的库调用方法：
	2015/12/07 $(obj).scrollIntoView();
	2015/12/07 $(obj).xmsAjax(options);
	2016/01/05 $(obj).xmsAllpass(inputs,customError);
	2015/12/15 $(obj).xmsEnterTrigger(options);
	2015/12/15 $(obj).xmsLoadHtml(options);
	
	//xmsCore的调用方法：
	2016/01/05 xmsCore.basic_method; 
	2015/12/07 xmsCore.xmsAjax; 
	2015/12/24 xmsCore.xmsCustomDate; 
	2015/12/07 xmsCore.xmsDateFormat;
	2015/12/15 xmsCore.xmsEnterTrigger;
	2016/02/05 xmsCore.xmsFillInfos;
	2015/12/09 xmsCore.xmsFormSubmit;
	2015/12/30 xmsCore.xmsLoadHtml;
	2015/12/30 xmsCore.xmsLoadImg;
	2015/12/07 xmsCore.xmsObjToStr;
	2015/12/07 xmsCore.xmsQueryParam;
	2015/12/07 xmsCore.xmsQueryUrl;
	2015/12/07 xmsCore.xmsStrToObj;
	2015/12/15 xmsCore.xmsTmplete;
	
*/

(function(window){
	
	var $ = null;
	
	//给$赋值
	if(typeof jQuery == "function"){
		$ = jQuery;
	}else if(typeof Zepto == "function"){
		$ = Zepto;
	}else{
		//如果没有，则抛出错误
		throw new Error("xmscore.js modal need jQuery/Zepto");
	}
	//把该方法添加到xmsCore对象中去
	var xmsCore = window.xmsCore;
	
	if(!xmsCore && typeof xmsCore != "object"){
		window.xmsCore = {};
		xmsCore = window.xmsCore;
	}
	
	if(!$.fn.extend){
		//Zepto中，$.fn中没有extend方法，这里进行重定义
		//做一层代理
		$.fn.extend = function(obj){
			$.extend($.fn,obj);
		};
	}
	
	//方法扩展到$.fn对象上去
	$.fn.extend({
		scrollIntoView:function(){
			
			var obj = $(this),
				parent = null;
				
			if(obj.size()){
				parent = obj.parent();
					
				parent.scrollTop(parent.scrollTop() + obj.position().top - parent.height() + obj.outerHeight());
			}
				
			return this;
		},
		xmsAjax:function(options){
			$.each($(this),function(){
				var option = $.extend({},options);
				
				option.obj = $(this);
				
				_xmsAjax(option);
			});
			
			return this;
		},
		xmsAllpass:function(inputs,customError){
			return basic_method.allpass($(this),inputs,customError);
		},
		xmsEnterTrigger:function(options){
			var obj = $(this).eq(0);
			
			options.obj = obj;
			
			_xmsEnterTrigger(options);
			
			return this;
		},
		xmsFormSubmit: function(options){
			$.each($(this),function(){
				new xmsFormSubmit($(this),options);
			});
			
			return this;
		},
		xmsLoadHtml:function(options){
			$.each($(this),function(){
				var option = $.extend({},options);
				
				option.obj = $(this);
				_xmsLoadHtml(option);
			});
			
			return this;
		}
	});
	
	xmsObjToStr.trim = $.trim;
	
	/*
		obj格式 {"action":"query","id":"1,2","city":"sh"}
		split字符串的分隔符 默认为"&"
	*/
	function xmsObjToStr(option){
		var obj = "",
			split = "&";

		if(typeof option !== "object"){
			return "";
		}
		if(typeof option.obj !== "object"){
			obj = option;
		}else{
			obj = option.obj || "";
			split = option.split || "&";
		}
		
		var i,
			arr = [];
			
		for(i in obj){
			arr.push(i+"="+xmsObjToStr.trim(""+obj[i]));
		}
		
		return arr.join(split);
	}
	//为了xmsObjToStr方法，定义一个局部变量，作用域链查找更快
	xmsCore.xmsObjToStr = xmsObjToStr;
	
	/*
		str格式 action=query&id=1&id=2&id=3&city=sh
		split字符串的分隔符 默认为"&"
		sep  为空时 key重复时取最后一个值
			 不为空时 key重复时值以sep作为分割符
	*/
	function xmsStrToObj(option){
		var str = "",
			split = "&",
			sep = "";

		if(typeof option == "object"){

			str = option.str || "",
			split = option.split || "&",
			sep = option.sep || "";

		}else if(typeof option == "string"){

			str = option;
		}
		
		if(!str){
			//如果为空，那么直接返回一个空对象
			return {};
		}

		var i,
			arr = str.split(split),
			len = arr.length,
			one = null,
			name = "",
			value = "",
			obj = {};

		for(i=0;i<len;i++){
			one = arr[i].split("=");
			name = one[0];
			value = one[1] || "";
			if(name){
				if(obj[name]){
					// 有相同参数
					if(sep){
						// 以sep为分割符
						if(value){
							obj[name] += sep + value;
						}
					}else{
						// 后一个值覆盖前一个值
						obj[name] = value;
					}
				}else{
					obj[name] = value;
				}
			}
		}
		return obj;
	}
	xmsCore.xmsStrToObj = xmsStrToObj;
	
	function xmsQueryParam(option){
		var data = option.data || "",
			replace = option.replace || "",
			param = {
				"split": option.split,
				"sep": option.sep
			};

		if(!data || !replace){
			param.str = data + replace;
		}else{
			param.str = data+"&"+replace;
		}
		
		return xmsCore.xmsObjToStr(xmsCore.xmsStrToObj(param));
	}
	xmsCore.xmsQueryParam = xmsQueryParam;
	
	//防止多次提交，有做提交信息的频率限制
	function _xmsAjax(options){
		//参数options支持以下参数
		/*
			//obj：必须，触发该ajax提交的对象元素，用于处理提交太快的情况。
			//dataType:可选，取值"json/html",默认为"json",返回值的数据格式
			//type:可选，取值"post/get",默认为"post"，请求的方式。
			//data:"",发出请求时，额外提交的数据。
			//url：ajax提交的地址，
			//	  必须存在，可以直接以options.url保存，
			//    也可以保存在obj.attr("data-url")属性中，options.url优先
			//success:成功后的回调函数，内部this指向obj，
			//    传入一个参数，为Ajax的返回值
			//error:失败时的回调函数，内部this指向obj，
			//    传入参数为ajax时error传入的参数，三个值，
			//    请自行去jQuery的error对象中查看。
			//limit:true/false,是否需要控制提交间隔，默认为true，控制间隔
		*/
		
		var limit = true,
			limitClass = "xms-ajax-limit",
			limitTime = 1000,
			dataType = "json",
			url = "",
			obj = null;
		
		//错误验证，以及赋值。
		try{
			obj = options.obj;
			limit = options.limit === false ? false : true;
			dataType = options.dataType || "json";
			url = options.url || obj.attr("data-url") || "";
		}catch(e){
			throw new TypeError("在调用xmsAjax时，传入的参数有误！");
		}
		
		if(!url){
			throw new Error("在调用xmsAjax时，url属性是必须的，请确认！");
		}
		
		if(limit){
			//限制提交频率的情况下，处理。
			if(obj.hasClass(limitClass)){
				alert("您提交的速度太快了，请稍后！");
				return false;
			}
			obj.addClass(limitClass);
		}
		
		$.ajax({
			url: url,
			type: options.type || "post",
			data: options.data || "",
			dataType: dataType,
			success: function(json) {
				var success = options.success;
				
				if(dataType == "json"){
					json = typeof json == "string"?JSON.parse(json):json;
				}
				
				if(limit){
					obj.removeClass(limitClass);
				}
				
				if (typeof success === "function") {
					success.call(obj, json);
				}
			},
			error: function() {
				var error = options.error;
				
				//如果有自定义的错误提示，那么调用自定义的
				//如果没有，那么直接提示。
				if (typeof error === "function") {
					error.apply(obj,arguments);
				}else{
					alert("由于网络的原因，您刚才的操作没有成功。");
				}
				
				if(limit){
					obj.removeClass(limitClass);
				}
			}
		});
		
		if(limit){
			setTimeout(function(){
				obj.removeClass(limitClass);
			},limitTime);
		}
		
	}
	xmsCore.xmsAjax = _xmsAjax;
	
	//basic_method对象，保存一些基本的可用于该模块的小的逻辑方法，
	//不使用于其他文件，所以，不保存到$对象上。
	var basic_method = {
		
		// 验证必填项--空值
		validEmpty: function(obj){
			var value = "";
			if(!obj.length){
				return true;
			}
			value = $.trim(obj.val());
			if(value === ""){
				return obj.attr("data-empty") || ("请输入" + (obj.attr("data-remind") || "")) + "!";
			}
			return true;
		},
		
		// 验证必填项 -- 数字
		validNum: function(obj){
			//验证是否为数字
			//只验证该处的输入值是否为数字，可以为空，
			//为空的验证，请使用validEmpty自行验证。
			
			var value = $.trim(obj.val()),
				min = "",
				max = "";
			
			if(value === ""){
				//如果输入值为空时，返回空字符串，
				//所以，当直接返回false时，表示通过验证
				return "";
			}
			
			//如果value值不为空，则把对应的需要进行判断的数据转换成数字
			value = value - 0;
			min = obj.attr("data-min") - 0;
			max = obj.attr("data-max") - 0;
			
			// 是否为NAN
			if(isNaN(value)){
				return (obj.attr("data-remind") || "本输入框")+"只支持数字！";
			}
			
			if(!isNaN(min) && !isNaN(max)){
				//如果设置了min和max
				if(max - min < 0){
					//如果min设置值比max大，则出错
					return "配置出错，请找技术进行确认问题！";
				}
				if(max === min){
					//如果设置的最大值和最小值相等
					return "该输入框只支持一个数字："+max + "!";
				}
				
				if(value - min < 0 || value - max > 0){
					return "请输入"+min+"-"+max+"区间的数字！"
				}
			}
			
			if(!isNaN(min) && value - min < 0){
				return "请输入大于等于" + min +"的数字！";
			}
			
			if(!isNaN(max) && value - max > 0){
				return "请输入少于等于" + max +"的数字！";
			}
			
			// 验证通过返回false
			return basic_method.validRegExp(obj);
		},
		
		//验证必填项 -- 正则验证
		validRegExp: function(obj, pattern){
			//验证正则时，不进行是否为空的验证
			//因为正则可以验证空输入的情况，所以不再单独的验证

			var value = "",
				reg = pattern || obj.attr("pattern") || "";
			
			if(reg){
				reg = (reg instanceof RegExp)?reg:new RegExp(reg);
				value = $.trim(obj.val()) || "";
				
				if(!reg.test(value)){
					return "请输入正确的" + (obj.attr("data-remind") || "") + "格式！"
				}
			}
			
			// 验证通过返回true
			return true;
		},
		
		// test if all passed
		allpass: function(obj,inputs,customError) {
			var allPassed = true;
			obj = obj?(obj instanceof $)?obj:$(obj):$(document);
			inputs = (inputs?(inputs instanceof $)?inputs:$(inputs):obj.find("input,textarea,select"));
			
			$.each(inputs,function(index,ele) {
				var obj = $(this),
					tip = "";
				
				if(!basic_method.isVisible(obj)){
					return "";
				}
				
				if (allPassed === true) {
					
					// 输入框有属性requir事，验证是否为空
					//之所以把requir放在这里，是为了验证一些时候
					//支持为空正常，支持如过输入，那么格式需要符合规定
					//比如手机号：可以不输入，但是输入时，就需要符合规则
					tip = basic_method.validEmpty(obj.filter("[requir]"));
					
					if(tip == true){
						switch(obj.attr("data-valid")){
							case "num": 
								tip = basic_method.validNum(obj);
	                            break;
							default:
								// 不为空的输入框才需验证正则
								tip = basic_method.validEmpty(obj);
								if(tip == true){
									tip = basic_method.validRegExp(obj);
								}else{
									tip = "";
								}
						}
					}
					
					if(typeof tip === "string" && tip !== ""){
						//只有tip返回为非空的字符串时，才执行错误提示的操作
						if(typeof customError == "function"){
							customError(obj,tip);
							basic_method.scroll(obj);
						}else{
							basic_method.errorTip(obj,tip);
						}
						
						allPassed = false;
						return allPassed;
					}
				}else{
					allPassed = false;
					return false;
					//结束each循环
				}
			});
			
			return allPassed;
		},
		
		errorTip:function(obj,tip,time){
			var title = obj.attr("title") || "",
				tagName = obj[0].tagName.toUpperCase();
				
			time = time || 1000;
			
			alert(tip);
			if(tagName == "INPUT" || tagName == "TEXTAREA"){
				obj.focus();
			}else{
				obj.attr("title",tip).tooltip("show");
				basic_method.scroll(obj);
				
	            setTimeout(function(){
	            	obj.tooltip("destroy").attr("title",title);
	            },time);
			}
		},
		
		scroll:function(obj){
			var top = 0, 
				win = null,
				p = obj,
				over = "",
				tagName = obj[0].tagName.toUpperCase(),
				winHeight = 0;

			if(!obj.length || tagName == "INPUT" || tagName == "TEXTAREA"){
				return false;
			}
			
			win = $(window);
			winHeight = win.height();
			top = obj.offset().top;
			if(top > winHeight/4 && top < winHeight*3/4){
				// 元素在可视区域内
				return false;
			}
			top = obj.offset().top - winHeight/2;
			while(p = p.parent()){
				over = p.css("overflow-y");
				if(over == "auto" || over == "scroll"){
					p.scrollTop(top);
					break;
				}
				
				if(p[0].tagName.toLowerCase() == "body"){
					win.scrollTop(top);
					break;
				}
			}
		},
		
		isVisible : function(obj){
			var visible = "",
				p = null;
			
			try{
				visible = obj.is(":visible");
				return visible;
			}catch(e){}
			
			p = obj;
			
			while(p.attr("tagName").toLowerCase() != "body"){
				visible = p.css("display");
				
				if(visible == "none"){
					return false;
				}else{
					p = p.parent();
				}
			}
			
			return true;
		}
		
	};
	xmsCore.basic_method = basic_method;
	
	function xmsDateFormat(options){
		//options支持两个参数
		//format，返回字符串的格式,"yyyy/mm/dd hh:ii",
		//除了其中的字母之外，标点，分隔符等，可以随意修改
		//time为一个有效的时间，可以是字符串，可以是对象
		//如果只传入一个参数，那么该参数为format的值
		
		if(!options){
			throw new TypeError("调用xmsDateFormat时，传入的参数有误！");
		}
		
		if(typeof options == "string"){
			options = {format:options};
		}
		
		if(typeof options != "object"){
			throw new TypeError("调用xmsDateFormat时，传入的参数有误！");
		}
		
		var format = options.format,
			t = xmsDateFormat.toDate(options.time),
			tf = xmsDateFormat.toFormatNum;
		
		if(!format || typeof format != "string"){
			throw new TypeError("调用xmsDateFormat时，传入对象的format属性不合法！");
		}
		
		if(t === false){
			throw new TypeError("调用xmsDateFormat时，传入对象的time属性不能转换为合法的时间对象！");
		}
		
        return format.replace(/yyyy|mm|dd|hh|ii/g, function(a){
            switch(a){  
                case 'yyyy':  
                    return tf(t.getFullYear());  
                    break;  
                case 'mm':  
                    return tf(t.getMonth() + 1);  
                    break;  
                case 'dd':  
                    return tf(t.getDate());
                    break; 
				case 'hh':
					return tf(t.getHours());
					break;
				case 'ii':
					return tf(t.getMinutes())
					break;
            }  
        });  
    }  
	
	//把传入的对象，转换为一个有效的时间对象，并返回该事件对象
	xmsDateFormat.toDate = function(date){
		var type = typeof date;
		
		if(!date || type == "undefined"){
			return new Date();
		}else if(date instanceof Date){
			return isNaN(date.getDate()) ? false : date;
		}else if((type == "string" || type == "number") && !isNaN(date)){
			date = date - 0;
		}else if(type == "string"){
			date = date.replace(/[^\d\s\:]+/g,"/");
		}
		
		date = new Date(date);
		
		return isNaN(date.getDate()) ? false : date;
	};
	
	//把数字转换为符合时间的两位字符串
	xmsDateFormat.toFormatNum = function(num){
		return (num < 10 ? '0' : '') + num;
	}
	xmsCore.xmsDateFormat = xmsDateFormat;
	
	//很多时候，当我们点击回车时，会触发其他的一些事件。
	function _xmsEnterTrigger(options){
		/*
		  * obj 目标元素
		  * src obj元素按下enter时，触发src中的type类型的事件
		  * type src元素将被触发的事件类型
		  * ctrl 是否需要按下ctrl，默认为false，
		  * alt 是否需要按下alt键，默认为false，
		  * shift 是否需要按下shift键，默认为false
		  * 上述三个，当为ture时，表示ctrl+enter等联合的时，才能触发
		*/
		var objsize = 0,
			srcsize = 0,
			obj = null,
			src = null,
			type = "",
			ctrl = false,
			alt = false,
			shift = false;
			
		try{
			obj = options.obj;
			src = options.src;
			objsize = obj.size();
			srcsize = src.size();
		}catch(e){
			throw new TypeError("在初始化xmsEnterTrigger时，传入的参数出错！");
		}
		
		if(!objsize || !srcsize){
			throw new TypeError("在初始化xmsEnterTrigger时，传入的参数出错！");
			return false;
		}

		type = options.type || "click";
		ctrl = options.ctrl || false;
		alt = options.alt || false;
		shift = options.shift || false;
		
		obj.on("keydown",function(e){
			e = e || window.event;
			var code = e.which || e.keyCode,
				flag = code == 13;
			
			if(flag){
				//这个时候，判断是否需要altKey键
				flag = alt?e.altKey:!e.altKey;
			}
			
			if(flag){
				//这个时候，判断是否需要ctrlKey键
				flag = ctrl?e.ctrlKey:!e.ctrlKey;
			}
			
			if(flag){
				//这个时候，判断是否需要shiftKey键
				flag = shift?e.shiftKey:!e.shiftKey;
			}

			if(flag){
				src.trigger(type);
				return false;
			}
			
		});
		
		return this;
		
	}
	xmsCore.xmsEnterTrigger = _xmsEnterTrigger;
	
	//动态的去load一部分html代码
	function _xmsLoadHtml(options){
		/*
			options对象包括以下属性
			obj：load的目标元素，load成功后的html，保存到该标签内部
				其中，ajax请求的地址，存放在obj标签的data-url标签中
			fn：请求前，是否会获取额外的数据，string或者function
				string或者function的返回值格式为："name=value&name1=value1"
			success：loadHtml成功之后，执行的回调
				内部this指向obj，传入的值为返回的html代码字符串
			error：loadHtml失败时，执行的回调函数
				内部this指向obj，传入error的三个参数值
		*/
		
		var obj = options.obj,
			newUrl = obj.attr("data-url") || "",
			fn = options.fn,
			fnType = typeof fn,
			data = (fnType == "function")?fn():(fnType == "string")?fn:"",
			loadding = null;

		//判断合法性
		try{
			//只对第一个起作用，顺带判断是否为允许的格式
			//不是的话，那么直接抛出一个类型错误。
			obj = obj.eq(0);
		}catch(e){
			throw new TypeError("没有找到目标元素，或者目标元素不是jQuery和Zepto对象！");
		}
		
		newUrl = obj.attr("data-url") || "";
		if(!newUrl || !obj.size()){
			return false;
		}
		
		$.ajax({
			url: newUrl,
			type: "post",
			data: data,
			timeout: 5000,
			dataType: "html",
			success: function(html) {
				var success = options.success;
				
				obj.html(html);
				
				if(typeof success == "function"){
					success.call(obj,html);
				}
				
				obj.trigger("loadHtmlSucc",html);
			},
			error: function(e1,e2,e3) {
				var error = options.error;
				loadding.addClass("dn");
				if(typeof error == "function"){
					error.call(obj,e1,e2,e3);
				}
			}
		});
		loadding = _xmsLoadHtml.addOverLay(obj);
		loadding.removeClass("dn");
		
		return this;
	}
	_xmsLoadHtml.addOverLay = function(obj){
		var tag = "div",
			loadding = obj.find(".xms-loadhtml-loadding");
		
		if(loadding.size()){
			return loadding;
		}
		
		if(obj.prop("tagName").toLowerCase() == "ul"){
			tag = "li";
		}
		
		loadding = $('<'+tag+' class = "xms-loadhtml-loadding"></'+tag+'>');
		
		if(obj.css("position") == "static"){
			obj.addClass("rel");
		}
		
		obj.append(loadding);
		return loadding;
	};
	xmsCore.xmsLoadHtml = _xmsLoadHtml;
	
	//下载图片相关的方法，需要传入两个参数
	//第一个参数为图片的src地址
	//第二个参数为回调函数
	//会尝试5次下载，5次下载失败，则执行error
	//回调函数的this对象执行成功或者失败的Image对象
	//回调函数内部，可以通过this.type判断，是否成功
	function _xmsLoadImg(options){
		//fn的内部this指向img的object对象，可以控制各种信息
		/*
			options的参数
			{
				src:"", //表示图片的服务器地址
				success:function, //图片从服务器加载成功之后，执行的成功回调
				error:function //图片加载失败时，执行的回调
			}
		*/
		
		var src = "",
			success = null,
			error = null,
			loadTime = 0;
		
		try{
			src = options.src || "";
		}catch(e){
			src = "";
		}
		
		if(!src){
			throw new TypeError("load imgae need src!");
		}
		
		success = options.success;
		error = options.error;
		
		load();
		
		function load(){
			var imgObj = new Image();
			
			imgObj.type = "success";
			imgObj.onload = success;
			imgObj.onerror = _error;
			imgObj.src = src;
			loadTime++;
		}
		
		function _error(p1,p2,p3){
			
			if(loadTime < 5 ){
				//如果图下载失败，则尝试最多五次
				load();
			}else{
				//如果下载图片失败，则也执行回调，并把this传入
				this.type = "error";
				if(typeof error == "function"){
					error.call(this);
				}
			}
			
		}
	}
	xmsCore.xmsLoadImg = _xmsLoadImg;
	
	//根据要求，更新摸个模块的数据
	function _xmsFillInfos(obj,json){
		if(typeof json == "string"){
			json = JSON.parse(json);
		}
		
		if(typeof json != "object"){
			return false;
		}
		
		/*
			type值分为：
			html
			value
			img
			background
			select
			radio
			checkbox
		*/
		
		$.each(obj.find("[data-infoName]"),function(){
			var _obj = $(this),
				infoName = _obj.attr("data-infoName") || "",
				type = "";
				
			if(infoName && ( infoName in json )){
				type = _obj.attr("data-infoType") || "html";
				if(type == "value"){
					_obj.val(json[infoName]);
					_obj.trigger("input");
				}else if(type == "select"){
					_obj.val(json[infoName]);
					_obj.trigger("change");
				}else if(type == "img"){
					xmsCore.xmsLoadHtml(json[infoName],function(){
						_obj.attr("src",this.src);
					});
				}else if(type == "background"){
					xmsCore.xmsLoadHtml(json[infoName],function(){
						_obj.css("background-image","url('"+this.src+"')");
					});
				}else if(type == "radio"){
					var radioEles = _obj.find("[type=radio][name="+infoName+"]"),
						checkedEle = radioEles.filter("[value="+json[infoName]+"]");

					if(radioEles.length){
						radioEles.prop("checked",false);
					}
					if(checkedEle.length){
						//checkedEle.prop("checked",true);
						checkedEle.trigger("click");
					}
				}else if(type == "checkbox"){
					var valueArr = (json[infoName]).split(","),
						checkboxEles = _obj.find("[type=checkbox][name="+infoName+"]"),
						checkedEle = null,
						i = 0,
						len = valueArr.length;
					
					checkboxEles.prop("checkbox",false);
					
					for(i;i<len;i++){
						checkedEle = checkboxEles.filter("[value="+valueArr[i]+"]");
						if(checkedEle.size()){
							checkedEle.trigger("click");
						}
					}
					
				}else{
					_obj.html(json[infoName]);
				}
			}
		});
		
	}
	xmsCore.xmsFillInfos = _xmsFillInfos;
	
	//更改url的一个函数，
	//支持两个参数，第一个参数为url，可以为绝对和相对地址
	//第二个参数为string或者fn(),string或者返回值为"name1=value1&name2=value2"
	//格式的字符串。
	//第二个参数的同名数据，会覆盖url中的同名数据
	function _xmsQueryUrl(options){
		//更改url的链接，需要
		/*
			options支持两个参数
			{
				url:"",//原有的url值
				fn:"",额外需要添加或者更新到url上的数据值
			}
		*/
		var url = "",
			arr = null,
			hostStr = "",
			searchObj = null;

		try{
			url = options.url || "";
		}catch(e){
			url = "";
		}
		
		if(!url){
			throw new TypeError("在使用xmsQueryUrl方法时，传入的对象必须包含url的属性值！");
		}
		
		//前面的判断，会验证options为一个对象，
		//那么如果没有定义fn，则直接返回url
		if(!options.fn){
			return url;
		}
		
		// http://www.xiaomishu.com?key1=1&key2=2
		// 以？作为分割将字符串分为两部分
		arr = url.split("?");
		hostStr = arr[0] || ""; 
		searchObj = xmsCore.xmsStrToObj(arr[1] || ""); 
		
		//把新的数据，覆盖同名原有的数据
		$.extend(searchObj,_xmsQueryUrl.getData(options.fn));
		
		return hostStr+"?"+xmsCore.xmsObjToStr(searchObj);
	}
	_xmsQueryUrl.getData = function(fn){
		//根据传入的fn的值
		var fnType = typeof fn,
			str = "",
			obj = null;
		
		if(fnType == "function"){
			str = fn();
			if(typeof str == "object"){
				return str;
			}
			
		}else if(fnType == "string"){
			str = fn;
		}else if(fnType == "object" && fn){
			return fn;
		}
		
		return xmsCore.xmsStrToObj(str);
		
	};
	xmsCore.xmsQueryUrl = _xmsQueryUrl;
	
	//防止多次提交，有做提交信息的频率限制
	function xmsFormSubmit(obj,options){
		/*
			//obj为需要绑定该方法的Form的jQuery/Zepto对象。
			//options可以为一个函数，也可以是一个对象
			
			//如果options为一个对象，那么它支持以下三个参数
			//success/callback:function,当成功之后执行的回调，
			//	  其内部this指向obj
			//    传入一个参数，为提交返回的JSON对象。
			//error:function,当执行失败时的处理，
			//    如果设置了该方法，那么错误时，按照该方法处理
			//    如果没有设置该方法，那么执行一个默认的错误提示。
			//beforeFn:function,在提交数据之前的处理的验证
			//	  内部this指向obj，做一些特殊的验证，或者处理
			//    如果该方法返回的是false，那么会停止AJAX的提交
			//className:"xms-form-submit-waiting",
			//	  当form处于正在提交状态时，会给form添加该样式。
			//新添加的方法，错误提示方法
			//customError : 当验证失败时的错误提示
			//传入两个参数，分别为错误发生的html元素，
			//和错误的提示信息
		*/
		if(!(this instanceof xmsFormSubmit)){
			return new xmsFormSubmit(obj,options);
		}
		
		try{
			obj.size();
		}catch(e){
			throw new TypeError("xmsFormSubmit初始化时传入的对象不合法。");
		}
		
		this.obj = obj.eq(0);
		this.options = options;
		
		this.initEvent();
	}
	
	xmsFormSubmit.prototype.initEvent = function(){
		var options = this.options,
			obj = this.obj,
			className = "xms-form-submit-waiting",
			success = null,
			error = null,
			beforeFn = null,
			method = obj.attr("method") || "post",
			url = obj.attr("action"),
			submitTimer = false,
			reqOptions = {sep:","},
			customError = null;

		this.initEvent = null;

		if(typeof options == "function"){
			success = options;
		}else{
			success = options.success || options.callback || "";
			error = options.error || "";
			beforeFn = options.beforeFn || "";
			className = options.className || className;
			customError = options.customError || null;
		}

		if(!url || obj.prop("xmsFormSubmit") === "true"){
			return false;
		}
		
		//如果能找到对应的元素，则处理
		obj.on("submit",_submitCb);
		obj.find("[type=submit]").on("click",_triggerSubmitCb);
		obj.find("button").on("click",_triggerSubmitCb);
		obj.prop("xmsFormSubmit","true");
		
		function _triggerSubmitCb(){
			
			if(submitTimer == true){
				return false;
			}
			
			submitTimer = true;
			
			obj.trigger("submit",this);
			
			setTimeout(function(){
				submitTimer = false;
			},500);
			
			return false;
		}
		
		function _submitCb(e){
			var reqStr = "";
			
			if(obj.hasClass(className)){
				alert("正在提交数据，请稍后！");
				return false;
			}
			
			if(!obj.xmsAllpass("",customError)){
				return false;
			}
			
			// 自定义表单提交前操作
			if(typeof beforeFn == "function" && !beforeFn.call(obj)){
				return false;
			}

			reqStr = obj.serialize();
			
			if("xmsQueryParam" in xmsCore){
				reqOptions.data = reqStr;
				reqStr = xmsCore.xmsQueryParam(reqOptions);
			}
			
			$.ajax({
				url:url,
				type:method,
				data:reqStr,
				dataType:"json",
				success:function(data){
					obj.removeClass(className);
					//之所以把该代码放在最前面，是防止后面的代码，
					//抛出异常时，不会影响整个form表单的后续动作
					
					data = ( typeof data == "object" ) ? 
						data : 
						JSON.stringify(data);
					
					if(typeof success == "function"){
						success.call(obj,data);
					}
					
				},
				error:function(){
					obj.removeClass(className);
					
					if(typeof error == "function"){
						error.call(obj,arguments);
					}else{
						alert("由于网络的原因，您刚才的操作没有成功。");
					}
				}
			});
			
			//放在这里提交，也是为了防止一些不确定的异常，导致无法提交。
			obj.addClass(className);
			
			return false;
		}
		
	}
	
	xmsCore.xmsFormSubmit = xmsFormSubmit;
	
	/*
		//str的格式为：“按时间段卢卡斯爱上大声地dajsjdk{{name}}阿斯顿静安寺，{{age}}阿斯顿就卡死了都看见啊”
		//str中，只有“{{name}}”中的值，会被obj[name]的属性值替换
		//所以，请注意格式。
	*/
	function _xmsTmplete(str,obj){
		
		if(!str || ( typeof str != "string") || !obj || (typeof obj != "object")){
			return "";
		}
		
		var reg = /\{\{([^\s]+?)\}\}/g;
		
		str = str.replace(reg,_replace);
		
		function _replace(p1,p2){
			var i = 0,
				key = p2.split("."),
				len = key.length,
				value = obj,
				type = "";
			
			for( i ; i < len ;i += 1){
				value = value[key[i]];
				type = typeof value;
				
				if( type == "string" || type == "number" || type == "boolean"){
					if(i == (len-1)){
						return value;
					}else{
						return p2;
					}
				}
				
				if(type == "undefined" || value === null){
					if( i == (len-1) ){
						return "";
					}else{
						return p2;
					}
				}
				
			}
			
			return p2;
			
		};
		
		return str;
		
	}
	xmsCore.xmsTmplete = _xmsTmplete;
	
	/*
		options是一个对象，支持以下参数
		date:日期值，如果没有，那么作为当天，
			请输入有效的可以转换为时间对象的数组
		length:获取的长度。
		lineFirst:0,
		//表示一行，以周几开始，默认为0，表示一周以周日开始
		//该数据，只有在获取整月时有效。
	*/
	function _xmsCustomDate(){
 
        //定义一些私有变量，用于定义计算农历，以及一些节日的概念
 
        //用于判断，农历的年份，月份，闰月等相关信息的一个配置数组
        var tInfo= [0x04bd8,0x04ae0,0x0a570,0x054d5,0x0d260,0x0d950,0x16554,0x056a0,0x09ad0,0x055d2,0x04ae0,0x0a5b6,0x0a4d0,0x0d250,0x1d255,0x0b540,0x0d6a0,0x0ada2,0x095b0,0x14977,0x04970,0x0a4b0,0x0b4b5,0x06a50,0x06d40,0x1ab54,0x02b60,0x09570,0x052f2,0x04970,0x06566,0x0d4a0,0x0ea50,0x06e95,0x05ad0,0x02b60,0x186e3,0x092e0,0x1c8d7,0x0c950,0x0d4a0,0x1d8a6,0x0b550,0x056a0,0x1a5b4,0x025d0,0x092d0,0x0d2b2,0x0a950,0x0b557,0x06ca0,0x0b550,0x15355,0x04da0,0x0a5b0,0x14573,0x052b0,0x0a9a8,0x0e950,0x06aa0,0x0aea6,0x0ab50,0x04b60,0x0aae4,0x0a570,0x05260,0x0f263,0x0d950,0x05b57,0x056a0,0x096d0,0x04dd5,0x04ad0,0x0a4d0,0x0d4d4,0x0d250,0x0d558,0x0b540,0x0b6a0,0x195a6,0x095b0,0x049b0,0x0a974,0x0a4b0,0x0b27a,0x06a50,0x06d40,0x0af46,0x0ab60,0x09570,0x04af5,0x04970,0x064b0,0x074a3,0x0ea50,0x06b58,0x055c0,0x0ab60,0x096d5,0x092e0,0x0c960,0x0d954,0x0d4a0,0x0da50,0x07552,0x056a0,0x0abb7,0x025d0,0x092d0,0x0cab5,0x0a950,0x0b4a0,0x0baa4,0x0ad50,0x055d9,0x04ba0,0x0a5b0,0x15176,0x052b0,0x0a930,0x07954,0x06aa0,0x0ad50,0x05b52,0x04b60,0x0a6e6,0x0a4e0,0x0d260,0x0ea65,0x0d530,0x05aa0,0x076a3,0x096d0,0x04bd7,0x04ad0,0x0a4d0,0x1d0b6,0x0d250,0x0d520,0x0dd45,0x0b5a0,0x056d0,0x055b2,0x049b0,0x0a577,0x0a4b0,0x0aa50,0x1b255,0x06d20,0x0ada0,0x14b63];
        //公历每月的天数
        var solarMonth = [31,28,31,30,31,30,31,31,30,31,30,31],
            Animals = ["鼠","牛","虎","兔","龙","蛇","马","羊","猴","鸡","狗","猪"],
            solarTerm = ["小寒","大寒","立春","雨水","惊蛰","春分","清明","谷雨","立夏","小满","芒种","夏至","小暑","大暑","立秋","处暑","白露","秋分","寒露","霜降","立冬","小雪","大雪","冬至"],
            sTermInfo = [ 0,21208,42467,63836,85337,107014,128867,150921,173149,195551,218072,240693,263343,285989,308563,331033,353350,375494,397447,419210,440795,462224,483532,504758],
            nStr1 = ['日','一','二','三','四','五','六','七','八','九','十'],
            nStr2 = ['初','十','廿','卅','□'];


            //国历节日 *表示放假日
            var sFtv = {
                "0101" : "元旦&元旦 新年",
                "0106" : "中国第13亿人口日",
                "0108" : "周恩来逝世纪念日",
                "0115" : "释迦如来成道日",
                "0121" : "列宁逝世纪念日 国际声援南非日 弥勒佛诞辰",
                "0202" : "世界湿地日",
                "0207" : "二七大罢工纪念日",
                "0210" : "国际气象节",
                "0214" : "情人节&情人节",
                "0215" : "中国12亿人口日",
                "0219" : "邓小平逝世纪念日",
                "0221" : "国际母语日 反对殖民制度斗争日",
                "0222" : "苗族芦笙节",
                "0224" : "第三世界青年日",
                "0228" : "世界居住条件调查日",
                "0301" : "国际海豹日",
                "0303" : "全国爱耳日",
                "0305" : "学雷锋纪念日 中国青年志愿者服务日",
                "0308" : "妇女节&国际劳动妇女节",
                "0309" : "保护母亲河日",
                "0311" : "国际尊严尊敬日",
                "0312" : "植树节&孙中山逝世纪念日",
                "0314" : "国际警察日 白色情人节",
                "0315" : "消费者权益日",
                "0316" : "手拉手情系贫困小伙伴全国统一行动日",
                "0317" : "中国国医节 国际航海日  爱尔兰圣帕特里克节",
                "0318" : "全国科技人才活动日",
                "0321" : "世界森林日 消除种族歧视国际日 世界儿歌日 世界睡眠日",
                "0322" : "世界水日",
                "0323" : "世界气象日",
                "0324" : "世界防治结核病日",
                "0325" : "全国中小学生安全教育日",
                "0329" : "中国黄花岗七十二烈士殉难纪念",
                "0330" : "巴勒斯坦国土日",
                "0401" : "愚人节&全国爱国卫生运动月(四月) 税收宣传月(四月)",
                "0402" : "国际儿童图书日",
                "0407" : "世界卫生日",
                "0411" : "世界帕金森病日",
                "0421" : "全国企业家活动日",
                "0422" : "世界地球日 世界法律日",
                "0423" : "世界图书和版权日",
                "0424" : "亚非新闻工作者日 世界青年反对殖民主义日",
                "0425" : "全国预防接种宣传日",
                "0426" : "世界知识产权日",
                "0430" : "世界交通安全反思日",
                "0501" : "劳动节&国际劳动节",
                "0503" : "世界哮喘日 世界新闻自由日",
                "0504" : "青年节&中国五四青年节 科技传播日",
                "0505" : "碘缺乏病防治日 日本男孩节",
                "0508" : "世界红十字日",
                "0512" : "国际护士节",
                "0515" : "国际家庭日",
                "0517" : "国际电信日",
                "0518" : "国际博物馆日",
                "0520" : "全国学生营养日 全国母乳喂养宣传日",
                "0523" : "国际牛奶日",
                "0526" : "世界向人体条件挑战日",
                "0530" : "中国“五卅”运动纪念日",
                "0531" : "世界无烟日 英国银行休假日",
                "0601" : "儿童节&国际儿童节",
                "0605" : "世界环境保护日",
                "0614" : "世界献血者日",
                "0617" : "防治荒漠化和干旱日",
                "0620" : "世界难民日",
                "0622" : "中国儿童慈善活动日",
                "0623" : "国际奥林匹克日",
                "0625" : "全国土地日",
                "0626" : "国际禁毒日 国际宪章日 禁止药物滥用和非法贩运国际日 支援酷刑受害者国际日",
                "0630" : "世界青年联欢节",
                "0701" : "建党节&香港回归纪念日 中共诞辰 世界建筑日",
                "0702" : "国际体育记者日",
                "0706" : "朱德逝世纪念日",
                "0707" : "抗日战争纪念日",
                "0711" : "世界人口日 中国航海日",
                "0726" : "世界语创立日",
                "0728" : "第一次世界大战爆发",
                "0730" : "非洲妇女日",
                "0801" : "建军节&中国人民解放军建军节",
                "0805" : "恩格斯逝世纪念日",
                "0806" : "国际电影节",
                "0808" : "中国男子节(爸爸节)",
                "0812" : "国际青年节",
                "0813" : "国际左撇子日",
                "0815" : "抗日战争胜利纪念",
                "0826" : "全国律师咨询日",
                "0902" : "日本签署无条件投降书日",
                "0903" : "中国抗日战争胜利纪念日",
                "0905" : "瑞士萨永中世纪节",
                "0906" : "帕瓦罗蒂去世",
                "0908" : "国际扫盲日 国际新闻工作者日",
                "0909" : "毛泽东逝世纪念日",
                "0910" : "教师节&中国教师节 世界预防自杀日",
                "0914" : "世界清洁地球日",
                "0916" : "国际臭氧层保护日 中国脑健康日",
                "0918" : "九·一八事变纪念日",
                "0920" : "国际爱牙日",
                "0921" : "世界停火日 预防世界老年性痴呆宣传日",
                "0927" : "世界旅游日",
                "0928" : "孔子诞辰",
                "0930" : "国际翻译日",
                "1001" : "国庆节&世界音乐日 国际老人节",
                "1002" : "国庆节假日 国际和平与民主自由斗争日",
                "1003" : "国庆节假日",
                "1004" : "世界动物日",
                "1005" : "国际教师节",
                "1006" : "中国老年节",
                "1008" : "全国高血压日 世界视觉日",
                "1009" : "世界邮政日 万国邮联日",
                "1010" : "辛亥革命纪念日 世界精神卫生日 世界居室卫生日",
                "1013" : "世界保健日 国际教师节 中国少年先锋队诞辰日 世界保健日",
                "1014" : "世界标准日",
                "1015" : "国际盲人节(白手杖节)",
                "1016" : "世界粮食日",
                "1017" : "世界消除贫困日",
                "1020" : "世界骨质疏松日",
                "1022" : "世界传统医药日",
                "1024" : "联合国日 世界发展新闻日",
                "1028" : "中国男性健康日",
                "1031" : "万圣节&万圣节 世界勤俭日",
                "1102" : "达摩祖师圣诞",
                "1106" : "柴科夫斯基逝世悼念日",
                "1107" : "十月社会主义革命纪念日",
                "1108" : "中国记者日",
                "1109" : "全国消防安全宣传教育日",
                "1110" : "世界青年节",
                "1111" : "光棍节&光棍节 国际科学与和平周",
                "1112" : "孙中山诞辰纪念日",
                "1114" : "世界糖尿病日",
                "1115" : "泰国大象节",
                "1117" : "国际大学生节 世界学生节 世界戒烟日",
                "1120" : "世界儿童日",
                "1121" : "世界问候日 世界电视日",
                "1129" : "国际声援巴勒斯坦人民国际日",
                "1201" : "世界艾滋病日",
                "1202" : "废除一切形式奴役世界日",
                "1203" : "世界残疾人日",
                "1204" : "全国法制宣传日",
                "1205" : "国际经济和社会发展志愿人员日 世界弱能人士日",
                "1207" : "国际民航日",
                "1208" : "国际儿童电视日",
                "1209" : "世界足球日 一二·九运动纪念日",
                "1210" : "世界人权日",
                "1211" : "世界防止哮喘日",
                "1212" : "西安事变纪念日",
                "1213" : "南京大屠杀纪念日",
                "1214" : "国际儿童广播电视节",
                "1215" : "世界强化免疫日",
                "1220" : "澳门回归纪念",
                "1221" : "国际篮球日",
                "1224" : "平安夜&平安夜",
                "1225" : "圣诞节&圣诞节",
                "1226" : "毛泽东诞辰纪念日&节礼日",
                "1229" : "国际生物多样性日"
            },
            //农历的节日
            lFtv = {
                "0101" : "春节&春节 大年初一",
                "0102" : "初二&大年初二",
                "0115" : "元宵节&元宵节",
                "0505" : "端午节&端午节",
                "0707" : "七夕节&七夕情人节",
                "0715" : "中元节&中元节",
                "0815" : "中秋节&中秋节",
                "0909" : "重阳节&重阳节",
                "1208" : "腊八节&腊八节",
                "1223" : "小年&小年",
                "0100" : "除夕&除夕"
            },
            //某月的第几个周，的节日
            wFtv = {
                "0150" : "世界麻风日", //一月的最后一个星期日（月倒数第一个星期日）
                "0351" : "全国中小学生安全教育日",
                "0453" : "秘书节",
                "0512" : "国世界哮喘日",
                "0520" : "母亲节&国际母亲节 救助贫困母亲日",
                "0530" : "全国助残日",
                "0532" : "国际牛奶日",
                "0626" : "中国文化遗产日",
                "0630" : "父亲节&国际父亲节",
                "0716" : "国际合作节",
                "0730" : "被奴役国家周",
                "0932" : "国际和平日",
                "0936" : "全民国防教育日",
                "0940" : "国际聋人节 世界儿童日",
                "0950" : "世界海事日 世界心脏病日",
                "1011" : "国际住房日 世界建筑日 世界人居日",
                "1023" : "国际减轻自然灾害日(减灾日)",
                "1024" : "世界视觉日",
                "1144" : "感恩节&感恩节",
                "1220" : "国际儿童电视广播日"
            };
 
		//返回农历 y年的总天数========
        function lYearDays(y) {
            var i, sum = 348;
            for(i=0x8000; i>0x8; i>>=1){
                sum += (tInfo[y-1900] & i)? 1: 0;
            }
            return(sum+leapDays(y));
        }
 
        //返回农历 y年闰月的天数
        function leapDays(y) {
            if(leapMonth(y)) {
                return((tInfo[y-1900] & 0x10000)? 30: 29);
            }else{
                return(0);
            }
        }
 
        //返回农历 y年闰哪个月 1-12 , 没闰返回 0
        function leapMonth(y) {
            return(tInfo[y-1900] & 0xf);
        }
 
        //返回农历 y年m月的总天数
        function monthDays(y,m) {
            return( (tInfo[y-1900] & (0x10000>>m))? 30: 29 );
        }
 
        //中文日期，根据日期
        function cDay(d){
            var s;
	
            switch (d) {
                case 10:
                    s = '初十'; break;
                case 20:
                    s = '二十'; break;
                case 30:
                    s = '三十'; break;
                default :
                    s = nStr2[Math.floor(d/10)];
                    s += nStr1[d%10];
                }
            return(s);
        }
 
        //某年的第n个节气为几日(从0小寒起算)
        function sTerm(y,n) {
            if(y==2009 && n==2){
                sTermInfo[n]=43467
            }
            var offDate = new Date( ( 31556925974.7*(y-1900) + sTermInfo[n]*60000  ) + Date.UTC(1900,0,6,2,5) );
 
            return(offDate.getUTCDate());
        }
 
        //返回该年的复活节(春分后第一次满月周后的第一主日)
        function easter(y) {

            var //取得春分日期
                term2=sTerm(y,5),
 
                //取得春分的公历日期控件(春分一定出现在3月)
                dayTerm2 = new Date(Date.UTC(y,2,term2,0,0,0,0)),
 
                //取得取得春分农历
                lDayTerm2 = lunar(dayTerm2),
 
                //下个月圆的相差天数
                lMlen = 0,
 
                l15 = 0,
 
                //复活节对应的时间对象
                dayEaster = null;
	
            if(lDayTerm2.lDay - 15 < 0){
                lMlen= 15-lDayTerm2.lDay;
            }else{
                lMlen= (lDayTerm2.isLeap? leapDays(y): monthDays(y,lDayTerm2.lMonth)) - lDayTerm2.lDay + 15;
            }
	
            //一天等于 1000*60*60*24 = 86400000 毫秒
            l15 = new Date(dayTerm2.getTime() + 86400000*lMlen );
            //求出第一次月圆为公历几日
 
            dayEaster = new Date(l15.getTime() + 86400000*( 7-l15.getUTCDay() ) );
            //求出下个周日
	
            this.m = dayEaster.getUTCMonth();
            this.d = dayEaster.getUTCDate();

        }
 
        //算出农历, 传入日期控件, 返回农历日期控件
        //该控件属性有 .year .month .day .isLeap
        function lunar(objDate) {
            var i,
                leap=0,
                temp=0,
                offset   = (Date.UTC(objDate.getFullYear(),objDate.getMonth(),objDate.getDate()) - Date.UTC(1900,0,31))/86400000,
                dateObj = {};
 
            for(i=1900; i<2150 && offset>0; i++) {
                temp=lYearDays(i);
                offset-=temp;
            }
 
            if(offset<0) {
                offset+=temp;
                i--;
            }
 
            dateObj.lYear = i;
 
            leap = leapMonth(i); //闰哪个月
            dateObj.isLeap = false;
	
            for(i=1; i<13 && offset>0; i++) {
                //闰月
                if(leap>0 && i==(leap+1) && dateObj.isLeap==false){
                    --i;
                    this.isLeap = true; temp = leapDays(dateObj.lYear);
                }else{
                    temp = monthDays(dateObj.lYear, i);
                }
 
                //解除闰月
                if(dateObj.isLeap==true && i==(leap+1)) {
                    this.isLeap = false;
                }
                offset -= temp;
            }
            if(offset==0 && leap>0 && i==leap+1){
                if(dateObj.isLeap){
                    dateObj.isLeap = false;
                }else{
                    dateObj.isLeap = true; --i;
                }
            }
            if(offset<0){ offset += temp; --i; }
            dateObj.lMonth = _formatNum(i);
            dateObj.lDay = _formatNum(offset + 1);
 
            return dateObj;
        }

        //规范化数字显示
        function _formatNum(num){
 
            if(num < 10){
                num = "0" + num;
            }
 
            return num;
        }
 
        //获取公历的日期等
        //包括
        /*
         year : 年份
         month : 月份
         day : 天
         week : 周（0-6）
         hh : 小时
         ii : 分钟
         isToday : 是否为当日
        */
        function _getDate(date,dateObj){
            //获取公历的日期
            dateObj = dateObj || {}
            dateObj.year = date.getFullYear();
            dateObj.month = _formatNum(date.getMonth() + 1);
            dateObj.day = _formatNum(date.getDate());
            dateObj.week = date.getDay();
            dateObj.hour = _formatNum(date.getHours());
            dateObj.min = _formatNum(date.getMinutes());
 
            //判断是否为今天
            var dateStr = dateObj.year + "/" + dateObj.month + "/" + dateObj.day,
				secs = 86400000;
			
			if(todayStr === dateStr){
                dateObj.isToday = true;
            }else{
                dateObj.isToday = false;
            }
			
			dateObj.distanceToday = Math.floor((date.getTime() - todayNum)/secs);
 
            return dateObj;
        }
 
        //添加节气
        function _solarTerms(dateObj){
            var y = dateObj.year,
                m = dateObj.month,
                day = dateObj.day,
                v = "";
 
            v = sTerm(y,m*2  )-1;
 
            if(v == day){
                dateObj.solarTerms = solarTerm[m*2];
                return "";
            }
 
            v = sTerm(y,m*2+1)-1;
 
            if(v == day){
                dateObj.solarTerms = solarTerm[m*2+1];
            }
 
        }
 
        //复活节
        function _addEstDay(dateObj){
            var y = dateObj.year,
                m = dateObj.month - 1,
                d = dateObj.day,
                estDay = null;
 
            if(!(m == 2 || m == 3)){
                return ""
            }
 
            //复活节只出现在3或4月
            estDay = new easter(y);

            if(m === estDay.m && d == estDay.d){
                dateObj.solarFestival = "复活节";
                dateObj.estDayFestival = "复活节";
            }
 
        }
 
        //添加周节日
        function _weekFestival(dateObj){
            //有点复杂，需要些思考在开始
 
            var month = dateObj.month - 0,
                day = dateObj.day - 0,
                week = dateObj.week - 0,
                i = "",
                one = {},
                weekCeil = 0,
                weekNum = 0,
                key = "",
                tmp = null,
                monthLen = solarMonth[month - 1];
 
            //遍历一遍
            for(i in wFtv){
                one.month = i.substring(0,2) - 0;
                one.week = i.substring(3,4) - 0;
                one.weekNum = i.substring(2,3) - 0;
 
                if(one.month == month && one.week == week){
                    //表示月份和周几，都已经匹配，才做匹配第几周的情况
                    weekCeil = Math.ceil((day+1)/7)*7 - day + week;
 
                    //在周日的情况下，做一个特殊判断，否则可能会导致周加一的
                    if(weekCeil === 7 && !week){
                        weekCeil = 0;
                    }
 
                    weekNum = Math.floor((weekCeil +day)/7);
 
                    if(one.weekNum - 5 < 0){
                        if(one.weekNum == weekNum){
                            key = i;
                            break;
                        }
                    }else{
                        //否则，判断是否为最后一个周
                        weekNum = monthLen - day + week;
 
                        if(weekNum <= 7){
                            key = i;
                            break;
                        }
 
                    }
                }
            }
 
            if(key in wFtv){
                tmp = wFtv[key].split("&");
                dateObj.solarFestival = tmp[0] || "";
                dateObj.weekFestival = tmp[1] || "";
            }
	
        }
 
        //添加公历节日
        function _solarFestival(dateObj){
            var day = dateObj.month+dateObj.day,
                value = "",
                tmp = null;
 
            if(day in sFtv){
                value = sFtv[day];
            }
 
            if(value){
                tmp = value.split("&");
                dateObj.solarFestival = tmp[0] || "";
                dateObj.dateFestival = tmp[1] || "";
            }
        }
 
        //添加农历节日的
        function _lunarFestival(dateObj){
            var day = dateObj.lMonth+dateObj.lDay,
                value = "",
                tmp = null;
 
            if(day in lFtv){
                value = lFtv[day];
            }
 
            tmp = value.split("&");
            dateObj.lunarFestival = ( tmp[0] || "" );
            dateObj.lliFestival = ( tmp[1] || "" );
 
        }
 
        var today = new Date(),
            todayObj = _getDateObject(today),
            todayStr = todayObj.year+"/"+todayObj.month+"/"+todayObj.day,
			todayNum = (new Date(todayStr)).getTime();
 
        /*
         this.isToday = false;
         //公历
         this.year = sYear;   //公元年4位数字
         this.month = sMonth;  //公元月数字
         this.day = sDay;    //公元日数字
         this.week = week;    //星期, 0-6
         
         //时间
         this.hour = hour; //小时
         this.min = min; //分钟
         
         //农历
         this.lYear      = lYear;   //公元年4位数字
         this.lMonth     = lMonth;  //农历月数字
         this.lDay       = lDay;    //农历日数字
         this.isLeap     = isLeap;  //是否为农历闰月?
         
         this.color      = '';
         
         this.lunarFestival = ''; //农历节日
         this.solarFestival = ''; //公历节日
         
         this.estDayFestival = ''; //右侧显示的复活节
         this.weekFestival = ''; //右侧显示的周相关的节日
         this.dateFestival = '';//右侧显示的日相关的公历节日
         this.lliFestival = '';//右侧显示的日相关的农历节日
         
         this.solarTerms    = ''; //节气
        */
		function _getDateObject(time){
			var date = null,
                dateObj = null;
			
			if(time){
				date = new Date(time);
			}else{
				date = new Date();
			}
 
            //获取农历的日期，并返回一个对象
            dateObj = lunar(date);
 
            //获取公历的日期，并添加到传入的对象中去。
            _getDate(date,dateObj);
 
            //添加节气
            _solarTerms(dateObj);
 
            //添加复活节
            _addEstDay(dateObj);
 
            //添加公历节日
            _solarFestival(dateObj);
 
            //添加周纪念日
            _weekFestival(dateObj)
 
            //添加农历的节日
            _lunarFestival(dateObj);
 
            return dateObj;
 
		}
		
		function _toDate(date){
			var type = typeof date;
			
			if(!date || type == "undefined"){
				return new Date();
			}else if(date instanceof Date){
				return isNaN(date.getDate()) ? false : date;
			}else if((type == "string" || type == "number") && !isNaN(date)){
				date = date - 0;
			}else if(type == "string"){
				date = date.replace(/[^\d\s\:]+/g,"/");
			}
			
			date = new Date(date);
			
			return isNaN(date.getDate()) ? false : date;
		}
		
		//按照时间长度，获取规定日期的时间数组
		function _getLatestDate(date,len){
			var i = 0,
				arr = [],
				one = null,
				secs = 86400000;
			
			for(i;i<len;i+=1){
				arr.push(_getDateObject(date));
				date += secs;
			}
			
			return arr;
		}
		
		//按照月份，获取时间数组
		function _getMonthDate(date,lineFirst){
			var curDay = _getDateObject(date),
				fristDay = (new Date(curDay.year+"/"+curDay.month+"/01")).getTime(),
				monthDay = _getDateObject(fristDay),
				arr = [],
				month = monthDay.month,
				one = monthDay,
				i = 1,
				secs = 86400000;
                //一天代表的毫秒数
			
			//添加上月末尾的信息
			while(one.week !== lineFirst){
				one = _getDateObject(fristDay - secs*i);
				i++;
				arr.push(one);
			}
			
			if(arr.length){
				arr.reverse();
			}
			
			//添加本月的信息
			while(monthDay.month == month){
				arr.push(monthDay);
				fristDay += secs;
				monthDay = _getDateObject(fristDay);
			}
			
			//当月最后一天，正好是周六，属于最后一天
			//直接返回
			if(monthDay.week == lineFirst){
				return arr;
			}
			
			//添加下一月开头的一些信息
			while(monthDay.week != lineFirst){
				arr.push(monthDay);
				fristDay += secs;
				monthDay = _getDateObject(fristDay);
			}
			
			return arr;
		}
		
		function xmsCustomDate(options){
	
			if(!options || (typeof options == "function")){
				throw new TypeError("在调用xmsCustomDate时，传入的参数有误！");
			}
			
			var len = options.length || 0,
				date = _toDate(options.date),
				lineFirst = options.lineFirst || 0,
				dateArr = null;
			
			if(date === false){
				throw new TypeError("调用xmsCustomDate时，传入对象的date属性不能转换为合法的时间对象！");
			}
			
			date = date.getTime();
			
			if(len){
				//获取固定的天数
				dateArr = _getLatestDate(date,len);
			}else{
				//获取传入时间所在的月，按“周”获取（会有上月和下月的几天）
				dateArr = _getMonthDate(date,lineFirst);
			}
			
			return dateArr;
		}
		
		return xmsCustomDate;
	}
	xmsCore.xmsCustomDate = _xmsCustomDate();
	//使用闭包了。
	
	
})(window);
KindEditor.plugin('template', function(K) {
	var self = this, name = 'template', lang = self.lang(name + '.'),
		htmlPath = self.pluginsPath + name + '/html/';
	var Days=$("#Days").val();
	function getFilePath(fileName) {
		return htmlPath + fileName + '?ver=' + encodeURIComponent(K.DEBUG ? K.TIME : K.VERSION);
	}
	self.clickToolbar(name, function() {
		var lang = self.lang(name + '.'),
			arr = ['<div class="ke-plugin-template" style="padding:10px 20px;">',
				'<div class="ke-header">',
				'<div class="ke-left">',
				lang. selectTemplate + ' <select>'];
			K.each(lang.fileList, function(key, val) {
				arr.push('<option value="' + key + '">' + val + '</option>');
			});
			html = [arr.join(''),
				'</select>',
				'，<label for="xyDays">行程天数:</label><input id="xyDays" class="ke-input-text ke-input-number" type="text" value="'+Days+'" />',
				'，<label for="Times">时间段数: </label><input id="Times" class="ke-input-text ke-input-number" type="text" value="3" />',
				'</div>',
				'<div class="ke-right">',
				'<input type="checkbox" id="keReplaceFlag" name="replaceFlag" value="1" /> <label for="keReplaceFlag">' + lang.replaceContent + '</label>',
				'</div>',
				'<div class="ke-clearfix"></div>',
				'</div>',
				'<iframe class="ke-textarea" frameborder="0" style="width:758px;height:360px;background-color:#FFF;"></iframe>',
				'</div>'].join('');
		var dialog = self.createDialog({
			name : name,
			width : 800,
			title : self.lang(name),
			body : html,
			yesBtn : {
				name : self.lang('yes'),
				click : function(e) {
					var doc = K.iframeDoc(iframe);
					var D=parseInt($("#xyDays").val());
					var Ts=parseInt($("#Times").val());
					var Html=$(doc.body.innerHTML);
					var DayHtml=Html.find(".dayBox");
					var TimeHtml=DayHtml.find(".timeBox");
					var i;
					for (i=1;i<Ts;i++){
						var Temp=TimeHtml.clone(true);
						DayHtml.append(Temp);
					}
					for (i=2;i<=D;i++){
						var Temp=DayHtml.clone(true);
						Temp.attr("rev",i).find(".dayNum").text(i);
						Temp.find("tr:eq(0)").children("td").css("border-top","1px dashed #CCC");
						Html.append(Temp);
					}
					doc=$("<div></div>");
					doc.append(Html);
					self[checkbox[0].checked ? 'html' : 'insertHtml'](doc.html()).hideDialog().focus();
				}
			}
		});
		var selectBox = K('select', dialog.div),
			checkbox = K('[name="replaceFlag"]', dialog.div),
			iframe = K('iframe', dialog.div);
		checkbox[0].checked = true;
		iframe.attr('src', getFilePath(selectBox.val()));
		selectBox.change(function() {
			iframe.attr('src', getFilePath(this.value));
		});
	});
});

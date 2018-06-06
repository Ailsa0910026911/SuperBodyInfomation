<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<LokFu.Models.FileLoad>" %>
<% 
    string n = Guid.NewGuid().ToString().Replace("-","");
    string neiw = ConfigurationManager.AppSettings["Key"];
    string FileContentTypeList = "text/plain,image/bmp,image/gif,image/pjpeg,image/jpeg,application/x-shockwave-flash";
    Model.FileContentType = Model.FileContentType == null ? new string[] { } : Model.FileContentType;
    if (Model.FileContentType.Length > 0)
    {
        FileContentTypeList = string.Empty;
        for (int i = 0; i < Model.FileContentType.Length; i++)
        {
            if (i == Model.FileContentType.Length - 1)
            {
                FileContentTypeList += Model.FileContentType[i];
            }
            else
            {
                FileContentTypeList += Model.FileContentType[i] + ",";
            }
        }
    }
    FileContentTypeList += ",image/png";
%>
<input class="<%=Model.Class%> form-control input-medium input-inline" id="f_<%=n%>" type="file" Name="<%=Model.BigName%>" title="文件的格式必须为[<%=FileContentTypeList %>]" <%=Model.StandardSize.HasValue ? "onchange='checkfile()'" : ""%>/>
<input class="<%=Model.Class%> form-control input-medium input-inline" style="display:none;" id="t_<%=n%>" disabled="disabled" type="text" Name="<%=Model.BigName%>" title="文件的格式必须为[<%=FileContentTypeList %>]"/>
<input class="t<%=n%>" id="<%=Model.BigName%>_Mode" Name="<%=Model.BigName%>_Mode" type="hidden" value="<%=Model.Mode==null?"Cut":Model.Mode%>"/>
<input class="t<%=n%>" id="<%=Model.BigName%>_IsSamll" Name="<%=Model.BigName%>_IsSamll" type="hidden" value="<%=Model.IsSmall==null?0:(Model.IsSmall==false?0:1)%>"/>
<input class="t<%=n%>" id="<%=Model.BigName%>_Width" Name="<%=Model.BigName%>_Width" type="hidden" value="<%=Model.Width==null?0:Model.Width%>"/>
<input class="t<%=n%>" id="<%=Model.BigName%>_Height" Name="<%=Model.BigName%>_Height" type="hidden" value="<%=Model.Height==null?0:Model.Height%>"/>
<input class="t<%=n%>" id="<%=Model.BigName%>_FileName" Name="<%=Model.BigName%>_FileName" type="hidden" value="<%=Model.FileName==null?"":Model.FileName%>"/>
<input class="t<%=n%>" id="<%=Model.BigName%>_FilePath" Name="<%=Model.BigName%>_FilePath" type="hidden" value="<%=Model.FilePath==null?"Logo":Model.FilePath%>"/>
<input class="t<%=n%>" id="<%=Model.BigName%>_SmallName" Name="<%=Model.BigName%>_SmallName" type="hidden" value="<%=Model.SmallName==null?"###":Model.SmallName%>"/>
<input class="t<%=n%>" id="<%=Model.BigName%>_StandardSize" Name="<%=Model.BigName%>_StandardSize" type="hidden" value="<%=LokFu.Infrastructure.LokFuEncode.LokFuAuthcodeEncode((Model.StandardSize==null?2:Model.StandardSize).ToString(), neiw)%>"/>
<input class="t<%=n%>" id="<%=Model.BigName%>_FileContentType" Name="<%=Model.BigName%>_FileContentType" type="hidden" value="<%=LokFu.Infrastructure.LokFuEncode.LokFuAuthcodeEncode(FileContentTypeList, neiw)%>"/>
<select onchange="GetFile<%=n%>(this.options[selectedIndex].value);" class="bs-select form-control input-xsmall input-inline"><option value="1">本地</option><option value="0">网络</option></select>
<script language="javascript">
    var GetFile<%=n%> = function (val) {
        if (val == 1) {
            $("#t_<%=n%>").prop("disabled", true);
            $("#t_<%=n%>").css("display", "none");
            $("#f_<%=n%>").removeAttr("disabled");
            $("#f_<%=n%>").css("display", "");
            $(".t<%=n%>").each(function(){
                $(this).removeAttr("disabled");
            }
            );
        } else {
            $("#f_<%=n%>").prop("disabled", true);
            $("#f_<%=n%>").css("display", "none");
            $("#t_<%=n%>").removeAttr("disabled");
            $("#t_<%=n%>").css("display", "");
            $(".t<%=n%>").each(function () {
                $(this).prop("disabled", true);
            });
        }
    }

    var StandardSize = <%=Model.StandardSize.HasValue ? Model.StandardSize.Value : 0%>;
    if(StandardSize!=0)
    {
        var maxsize = StandardSize * 1024 * 1024; 
        var errMsg = "上传的附件文件不能超过 "+StandardSize+"M！！！";
        var tipMsg = "您的浏览器暂不支持计算上传文件的大小，确保上传文件不要超过"+StandardSize+"M，建议使用FireFox、Chrome浏览器。";
        var browserCfg = {};
        var ua = window.navigator.userAgent;
        if (ua.indexOf("MSIE") >= 1) {
            browserCfg.ie = true;
        } else if (ua.indexOf("Firefox") >= 1) {
            browserCfg.firefox = true;
        } else if (ua.indexOf("Chrome") >= 1) {
            browserCfg.chrome = true;
        }
    }
    function checkfile() {
        var obj_file = document.getElementById("f_<%=n%>");
        if (obj_file.value == "") {
            showdialog("请先选择上传文件");
            return;
        }
        var filesize = 0;
        if (browserCfg.firefox || browserCfg.chrome) {
            filesize = obj_file.files[0].size;
        }
        else
        {
            showdialog(tipMsg);
            return;
        }

        if (filesize > maxsize) {
            obj_file.value = ""
            showdialog(errMsg);
            return;
        }
    }
</script>



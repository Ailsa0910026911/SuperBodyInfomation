function UpLoadFileHelp(Elm) {
    //文件路径
    this.FilePath = "";
    //上传控件对象
    this.Upload = this.IniFileUpload(Elm);
    //元素
    this.Elm = Elm;
    //删除回调
    this.DelFun = null;
    //上传成功回调
    this.DoneFun = null;
};

//图片模式初始化方法
UpLoadFileHelp.prototype.IniImg = function () {
    var This = this;

    this.Upload.bind('fileuploadprogress', function (e, data) {
        var BindingId = $("#" + This.Elm.attr("BindingId"));
        var progressElem = $(BindingId).find("span[id=" + data.files[0].FileID + "]").find(".progress .progress-bar");
        var progress = parseInt(data.loaded / data.total * 100, 10);
        progressElem.css('width', progress + '%');
    }).bind('fileuploadprocessalways', function (e, data) {
        if (data.files.error) {
            alert(data.files[0].error);
        }
        else {
            data.files[0].FileID = This.newGuid();
            var BindingId = $("#" + $(this).attr("BindingId"));
            This.InsertUploading(BindingId, data.files[0]);
        }
    }).bind('fileuploaddone', function (e, data) {
        if (data.result.Status == true) {
            This.DoneFun(data);
            This.InsertUploadImg(data);
        }
        else { alert(data.result.Message); }
    });

    //绑定删除动作
    $(document).on("click", ".delete", function () {
        var elm = $(this).parents("span");
        This.DelFun(elm);
        elm.remove();
    });
};

UpLoadFileHelp.prototype.IniFile = function () {
    var This = this;
    this.Upload.bind('fileuploaddone', function (e, data) {
        if (data.result.Status == true) {
            This.DoneFun(data);
        }
        else { alert(data.result.Message); }
    });
};

/**
* 插入上传中
* 参数：
* target 插入目标元素
* data 控件数据
* 返回： 无
*/
UpLoadFileHelp.prototype.InsertUploading = function (target, data) {
    var html =
    '<span id="' + data.FileID + '">\
        上传中...\
        <div class="progress">\
            <div class="progress-bar progress-bar-danger" role="progressbar" aria-valuenow="80" aria-valuemin="0" aria-valuemax="100" style="width: 10%"></div>\
        </div>\
    </span>';
    target.append(html);
};

/**
* 插入上传图片
* 参数：
* data 控件数据
* filepath 文件路径
* 返回： 无
*/
UpLoadFileHelp.prototype.InsertUploadImg = function (data) {
    var result = data.result.Result;
    result.Id = data.files[0].FileID;
    var fileID = $("#" + data.files[0].FileID);
    fileID.empty();
    var html =
    '<a href="' + this.FilePath + '/' + result.SaveFileName + '" target="_blank">\
                <img src="' + this.FilePath + '/' + result.SaveFileName + '"  /></a>\
                <a class="delete" href="javascript:"> <i class="fa fa-times color-red"></i></a>';
    fileID.append(html);
}

/**
 *按Id查询数据
 *Data:数据源
 *id:id
*/
UpLoadFileHelp.prototype.FindById = function (Data, id) {
    var result = null;
    $.each(Data, function (i, n) {
        if (n.Id == id) {
            result = n;
            return;
        }
    });
    return result;
}

/**
 *按Id删除数据
 *Data:数据源
 *id:id
 *return 被删除的数据
*/
UpLoadFileHelp.prototype.DelById = function (Data, id) {
    var result = null;
    var reid = null;
    $.each(Data, function (i, n) {
        if (n.Id == id) {
            reid = i;
            result = n;
        }
    });
    if (reid != null) {
        Data.splice(reid, 1);
    }
    return result;
}

UpLoadFileHelp.prototype.newGuid = function () {
    var guid = "";
    for (var i = 1; i <= 32; i++) {
        var n = Math.floor(Math.random() * 16.0).toString(16);
        guid += n;
        if ((i == 8) || (i == 12) || (i == 16) || (i == 20))
            guid += "";
    }
    return guid;
}

/**
 *初始化控件
 *This:元素
*/
UpLoadFileHelp.prototype.IniFileUpload = function (This) {
    var MaxHigh = This.attr("MaxHigh") != null ? This.attr("MaxHigh") : 0;
    var MaxWidth = This.attr("MaxWidth") != null ? This.attr("MaxWidth") : 0;
    var MinHigh = This.attr("MinHigh") != null ? This.attr("MinHigh") : 0;
    var MinWidth = This.attr("MinWidth") != null ? This.attr("MinWidth") : 0;
    var MaxFileSize = This.attr("Size") != null ? This.attr("Size") : 2097152;
    var FileType = This.attr("FileType") != null ? This.attr("FileType") : "gif|jpe?g|png";
    var Url = This.attr("Url") != null ? This.attr("Url") : "";
    var SavePath = This.attr("SavePath") != null ? This.attr("SavePath") : "";
    var Name = This.attr("Name") != null ? This.attr("Name") : "";
    var IsOriginalName = This.attr("IsOriginalName") != null ? true : false;
    return This.fileupload({
        url: Url + "?time" + new Date().getTime(),
        dataType: 'json',
        maxFileSize: MaxFileSize,
        acceptFileTypes: new RegExp("(\.|\/)(" + FileType + ")$", "i"),
        formData: { "MaxHigh": MaxHigh, "MaxWidth": MaxWidth, "MinHigh": MinHigh, "MinWidth": MinWidth, "SavePath": SavePath, "Name": Name, "IsOriginalName": IsOriginalName },
        //多语言支付
        messages: {
            maxNumberOfFiles: '最多能上传文件10个',
            acceptFileTypes: '文件类型不正确',
            maxFileSize: '上传文件最大' + MaxFileSize / 1024 / 1024 + 'M',
            minFileSize: '上传文件最小1KB'
        },
        //处理出错时调用
        processalways: function (e, data) {
            if (data.files.error) {
                $.each(data.files, function (index, file) {
                    alert(file.error);
                });
            }
        }
    });
}
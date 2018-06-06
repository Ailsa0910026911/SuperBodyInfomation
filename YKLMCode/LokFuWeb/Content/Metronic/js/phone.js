function Phone() {
    this.UI = new UI();
    this.Html = new Html();
    this.Data = new Data();

    //初始化数据Html
    this.IniDataHtml = function IniData()
    {
        var $UI = this.UI;
        var $Html = this.Html;
        $.each(Home, function (i, n) {
            var lock = "";
            if(n.IsLock == true)
            {
                lock = $Html.lock;
            }
            $UI.featlist.append(
                '<li class="draglist native" id="' + n.Id + '" data-DisplaySite="1">\
                    <a href="javascript:void(0);">\
                    <img src="' + imgpath + n.PictureUrl + '"/>\
                    '+ lock +
                    '<p>' + n.Name + '</p>\
                    </a>\
                </li>');
        });
        if (IsAdd) {
            this.UI.featlist.append('<li class="addfeat text-center" data-DisplaySite="1"><a href="javascript:">+</a></li>');
        }
        $.each(More, function (i, n) {
            var lock = "";
            if (n.IsLock == true) {
                lock = $Html.lock;
            }
            $UI.movelist.append(
                '<li class="draglist native" id="' + n.Id + '" data-DisplaySite="3">\
                    <a href="javascript:void(0);">\
                    <img src="' + imgpath + n.PictureUrl + '"/>\
                    '+ lock +
                    '<p>' + n.Name + '</p>\
                    </a>\
                </li>');
        });
        if(IsAdd)
        {
            this.UI.movelist.append('<li class="addfeat text-center" data-DisplaySite="3"><a href="javascript:">+</a></li>');
        }
        $.each(Boot, function (i, n) {
            $UI.bottlist.append(
                '<li class="draglist bottomfeat" id="' + n.Id + '" data-DisplaySite="2">\
                    <a href="javascript:void(0);">\
                    <img class="default" src="' + imgpath + n.PictureUrl + '" />\
                    <img class="active" src="' + imgpath + n.PicUrl + '" />\
                    <p>' + n.Name + '</p>\
                    </a>\
                </li>');
        });
    }

    //初始化图标
    this.IniIcon = function IniIcon() {
        var $Phone = this;
        var $UI = this.UI;

        //全局
        $("select[Name='ModuleType']").on("change", function () {//绑定功能类型选择动作
            $Phone.moduleTypeSelect($(this));
        });
        $(".sortsave").on("click", function () {//保存排序
            $(".sortsave").attr("disabled", true);
            var home_array = "";
            var bottom_array = "";
            var movelist_array = "";
            var AppBtnNumber = $("#AppBtnNumber").val();
            var APPHasMore = $("#APPHasMore").val();
            var AgentId = $("#Id").val();
            $.each($UI.featlist.find(".native"), function (i, n) {
                home_array += $(n).attr("id") + ",";
            });
            $.each($UI.bottlist.find(".bottomfeat"), function (i, n) {
                if ($.isNumeric($(n).attr("id"))) {
                    bottom_array += $(n).attr("id") + ",";
                }
            });
            $.each($UI.movelist.find(".native"), function (i, n) {
                movelist_array += $(n).attr("id") + ",";
            });
            home_array = home_array.slice(0, home_array.length - 1);
            bottom_array = bottom_array.slice(0, bottom_array.length - 1);
            movelist_array = movelist_array.slice(0, movelist_array.length - 1);
            $.post("EditSort.html", { "AgentId": AgentId, "homeids": home_array, "bottomids": bottom_array, "movelistids": movelist_array, "AppBtnNumber": AppBtnNumber, "APPHasMore": APPHasMore }, function (data) {
                if (data == "1") {
                    $Phone.showMoreButton(APPHasMore);
                    $(".sortsave").attr("disabled", false);
                    alert("保存成功");
                }
                else {
                    alert("保存失败");
                }
            });
        });
        
        //初始化更多按钮
        $(".movelist").find("li.addfeat").on("click", function () {//更多栏显示添加表单
            $Phone.ShowForm($(".get-features"));
        });
        
        $(".more_features").on("click", function () {//点击更多
            $(".homeright").removeClass("hide");
            $(".homeleft").addClass("hide");
            $(".phone-right").find("div.phone-list").addClass("hide");
        });
        $(".back").find("a").click(function () {//点返回
            $(".homeright").addClass("hide");
            $(".homeleft").removeClass("hide");
            $(".phone-right").find("div.phone-list").addClass("hide");
        });

        //初始化Home
        //绑定排序组件
        var el = document.getElementById("features");
        var move = document.getElementById("movelist");
        var buttlist = document.getElementById("buttlist");
        sequence(el);
        sequence(move);
        sequence(buttlist);
        //图标浮动菜单
        this.FloatIconMenu($UI.featlist);
        this.FloatIconMenu($UI.movelist);
        this.FloatBottomMenu(true);
        //系统图标选择
        var modalLayer = $('.modal-layer');
        var modal = modalLayer.find('.modal-con');
        var modalbtn = modalLayer.find(".modal-btn");
        var modalclose = $(".modal-close");
        $("body").on('click', function () {
            modalLayer.addClass("hide");
        });
        modal.on('click', 'a', function (event) {
            event.stopPropagation();
            modalbtn.attr("disabled", false);
            $(this).addClass("active").parent().siblings().find("a").removeClass("active");
        });
        modalbtn.on('click', function (event) {
            event.stopPropagation();
            var thisModalLayer = $(this).parents(".modal-layer");
            var groupa = thisModalLayer.find('.modal-con').find('a');;
            var inputId = thisModalLayer.attr("id");
            var obj = $("[data-tabel=" + inputId + "]").parents("li");
            var path = "";
            switch (inputId) {
                case "IconHome":
                    path = "Home/";
                    break;
                case "IconBottomDef":
                    path = "bottom/default/";
                    break;
                case "IconBottomAct":
                    path = "bottom/activate/";
                    break;
            }
            var Pic = obj.find(".PicFile");
            var PicFeatimg = obj.find(".featimg");
            PicFeatimg.show();
            $.each(groupa, function (i, n) {
                if ($(this).hasClass('active')) {
                    Pic.val( path + $(n).find("img").attr("alt"));
                    PicFeatimg.attr("href", $(n).find("img").attr("src"));
                    PicFeatimg.find("img").attr("src", $(n).find("img").attr("src"));
                }
            })
            modalLayer.addClass("hide");
        });
        modalclose.on('click', function (event) {
            event.stopPropagation();
            modalLayer.addClass("hide");
        });
        $(".system-icon").on("click", function (event) {
            event.stopPropagation();
            if (modal.find("li > a[class='active']").length == 0)
            {
                modalbtn.attr("disabled", true);
            }
            var id = $(this).attr("data-tabel");
            $("#" + id).removeClass('hide');
        });

        this.quantityFun();//宫格初始化
    }

    //初始化广告及扫码背景
    this.IniAD = function IniAD() {
        var $UI = this.UI;
        var $Phone = this;
        var $Data = this.Data;
        this.FloatADMenu(this.UI.code);
        this.FloatADMenu(this.UI.top);

        //表单
        var ADId = $UI.setcaroumod.find("#ADId");
        ADId.on("change", function () {
            var id = $(this).val();
            $Phone.FormAD(id);
        });
        $(".btn-del").on("click", function () {
            if (confirm("确认要删除？")) {
                var id = ADId.val();
                if (id != 0) {
                    $.post("DeleteAdinfo.html", { Id: id }, function (data) {
                        if (data == "1") {
                            DelById($Phone.Data.bannerInfo, id);
                            $(".imgwarp").find("li[data-id=" + id + "]").remove();
                            $UI.setcaroumod.addClass("hide");
                            $Phone.TidyAD();
                        }
                        else { alert("删除失败"); }
                    });
                }
                else { alert("请选择名称"); }
            }
        });
        
        //扫码背景图
        this.UI.code.find(".modify").on("click", function () {
            $UI.setscan.removeClass("hide").siblings(".phone-list").addClass("hide");
        });
        this.UI.setscan.on("click", ".modify", function () {//浮动菜单
            var val = $(".icont-upload").find(".codebg").val();
            $UI.code.css("background", "url(" + val + ")");
        });

        this.TidyAD();
    }

    //显示更多按钮
    this.showMoreButton = function showMoreButton(APPHasMore) {
        if (APPHasMore == 0) {
            APPHasMore = BasicSetAPPHasMore;
        }
        var htmlstr = '<a class="more_features" href="javascript:"><img src="/Content/Metronic/image/icon-12.png" alt="" /><p>更多</p></a>';
        if (APPHasMore == 2) {
            $(".more_features").remove();
        }
        else if (APPHasMore == 1) {
            $("#features").after(htmlstr);
            //点击更多
            $(".more_features").on("click", function () {
                $(".homeright").removeClass("hide");
                $(".homeleft").addClass("hide");
                $(".phone-right").find("div.phone-list").addClass("hide");
            });
            this.quantityFun();
        }
    }

    //设置宫格
    this.quantityFun = function quantityFun() {
        var val = $("#AppBtnNumber").val();
        if (val == 3) {
            $(".screen").find(".features").find("li").css("width", "33.33%");
            $(".addfeat").css("width", "33.33%");
            $(".more_features").css("width", "33.6%");
        } else if (val == 4) {
            $(".screen").find(".features").find("li").css("width", "25%");
            $(".addfeat").css("width", "25%");
            $(".more_features").css("width", "25.2%");
        }
    }

    //重整广告
    this.TidyAD = function TidyAD() {
        clearInterval(timerID);//停止轮动
        var reundhtml = "";
        $.each(this.Data.bannerInfo, function (i, n) {//按数据加入轮动点
            reundhtml += "<li><a href='#'></a></li>";
        });
        this.UI.reoud.html(reundhtml);
        this.UI.reoud.children().eq(0).find("a").addClass("active");
        carimg(this.Data.bannerInfo.length);//起动轮动
        //图片宽度
        var picWidth = $(".imgwarp").find("li").length * $(".imgwarp").find("li").first().width();
        $(".imgwarp").css("width", picWidth);
        //重新设置浮动菜单宽度
        var menuWidth = parseInt($(".reoud").css("width"));
        $(".reoud").css("margin-left", (-menuWidth / 2));
    }

    //重整底部
    this.TidyBottom = function TidyBottom() {
        if (Boot.length == 0) {
            this.UI.bottlist.html("");
            this.UI.bottlist.append(this.Html.bootAdd);
        }
        //底部列表宽度
        var bott = $(".bott");
        var length = bott.find("li").length;
        bott.find("li").css("width", 100 / length + "%");
    }

    //重整底部浮动菜单
    this.TidyFloatIconMenu = function TidyFloatIconMenu(id) {
        var lasyhtml = "";
        //数量对应的显示方式
        this.Html.botlasy.html("");
        var n = FindById(Boot, id);
        if (n.IsLock == true) {
            lasyhtml += this.Html.lock;
        }

        if (Boot.length > 0 && Boot.length < 5) {
            lasyhtml += this.Html.addstr + this.Html.lasyhtml;
        }
        else if (Boot.length == 5) {
            lasyhtml += this.Html.lasyhtml;
        }
        
        this.Html.botlasy.html(lasyhtml);
    }

    //显示表单
    this.ShowForm = function ShowForm(obj) {
        obj.removeClass("hide").siblings(".phone-list").addClass("hide");
    }

    //图标浮动菜单
    this.FloatIconMenu = function FloatIconMenu(obj) {
        var $Html = this.Html;
        var $UI = this.UI;
        var $Phone = this;
        obj.find("li.native").on("mouseover", function (e) {//浮动菜单
            //http://blog.csdn.net/ltx851201/article/details/6800553
            if (!checkHover(e, this)) {//因为浮动引起的反复触发mouseover
                return;
            }
            $Html.lilasy.html("");
            $Html.lilasy.append($Html.spans);
            //隐藏
            if ($Html.botlasy) {
                $Html.botlasy.remove();
            }
            //如果没有图标了，加入添加按钮
            if ($(this).find(".li-lasy").length < 1) {
                $Html.lilasy.appendTo($(this));
            }
        }).on("click", ".modify", function () {//修改动作
            var This = $(this).parent().parent();
            $Phone.ShowForm($UI.setfeatures);//显示表单
            $Phone.obtain($UI.setfeatures, This);//表单初始化
        }).on("click", ".del", function () {//删除动作
            var parent = $(this).parent().parent();
            var id = parent.attr("id");
            if (confirm("确认要删除？")) {
                $.post("DeleteAPPModule.html", { Id: id },
                  function (data) {
                      if (data == "1") {
                          DelById(Home, id);
                          parent.remove();
                          $(".set-features").addClass("hide");
                      }
                      else { alert("删除失败"); }
                  });
            }
        });
        $(".addfeat").on("click", function () {//显示添加表单
            $Phone.ShowForm($UI.setfeatures);//显示表单
            $Phone.obtain($UI.setfeatures, $(this));//初始化数据
            $(".more_features").css("border-left", "1px solid #e2e2e2");
        })
    }

    //广告浮动菜单
    this.FloatADMenu = function FloatADMenu(obj) {
        var $Html = this.Html;
        var $Phone = this;
        var $Data = this.Data;
        var $UI = this.UI;
        obj.on("mouseover", function () {
            $(this).find(".lasy").removeClass("hide");
            $(".li-lasy").remove();
            if ($Html.botlasy) {
                $Html.botlasy.remove();
            }
        }).on("mouseout", function () {
            $(this).find(".lasy").addClass("hide");
        });
        obj.find(".lasy").on("click", ".add", function () {
            $Phone.ShowForm($UI.setcaroumod);
            $Phone.FormAD(0);
        });
        obj.on("click", ".modify", function () {
            if (obj.attr("name") == "code")
            {
                $Phone.ShowForm($UI.setscan);
            }
            else if (obj.attr("name") == "top")
            {
                if (!$.isEmptyObject($Data.bannerInfo)) {
                    $Phone.ShowForm($UI.setcaroumod);
                    $Phone.FormAD($($Data.bannerInfo).first().attr("Id"));
                }
                else { alert("请先添加广告"); }
            }
        });
    }

    //底部浮动菜单
    this.FloatBottomMenu = function FloatBottomMenu(off) {
        var $Phone = this;
        var $Html = this.Html;
        var $UI = this.UI;
        this.TidyBottom();
        var timer = 0;
        $(".bott").on("mouseover", function () {
            $(".li-lasy").remove();
        }).on("mouseover", ".bottomfeat", function (e) {
            if (!checkHover(e, this)) {//因为浮动引起的反复触发mouseover
                return;
            }
            $Phone.TidyFloatIconMenu($(this).attr("id"));
            $Html.botlasy.appendTo($(this));
            
            if (off) {
                $Html.botlasy.css("display", "block");
            } else {
                $Html.botlasy.css("display", "none");
            }
            if (Boot.length <= 2)
            {
                var num = $(this).width() * 0.25;
                $Html.botlasy.css("left", num);
            }
        }).on("mouseout", ".bottomfeat", function () {
            var This = $(this);
            timer = setTimeout(function () {
                This.find(".lasy").remove();
            }, 500);
        }).on("mouseover", function () {
            clearTimeout(timer);
        }).on("click", ".add", function () {
            $(this).parent().remove();
            $Phone.ShowForm($UI.bootfeatures);
            $Phone.obtain($UI.bootfeatures, $(this));
        }).on("click", ".modify,.bottomFeatAdd", function () {
            var This = "";
            if ($(this).parent().attr("class") == "lasy") {
                var This = $(this).parent().parent();
                $(this).parent().remove();
            }
            else { This = $(this);}
            $Phone.ShowForm($UI.bootfeatures);
            $Phone.obtain($UI.bootfeatures, This);
        }).on("click", ".del", function () {
            var li = $(this).parents("li");
            var id = li.attr("id");
            if (confirm("确认要删除？")) {
                $.post("DeleteAPPModule.html", { Id: id },
                  function (data) {
                      if (data == "1") {
                          DelById(Boot, id);
                          li.remove();
                          $Phone.TidyBottom();
                          $UI.bootfeatures.addClass("hide");
                      }
                      else { alert("删除失败"); }
                  });
            }
        });
    }

    /*图标表单数据初始化
     *obj:表单元素
     *tag:点击事件元素
    */
    this.obtain = function obtain(obj, tag) {
        var inputId = tag.attr("id");
        var ModuleType = obj.find("[Name='ModuleType']");
        var featimg = obj.find(".featimg");
        var DisplaySite = tag.attr("data-DisplaySite");
        var IconName = "";
        obj.find("form")[0].reset();
        obj.find("[Name='PictureUrl']").val("");
        obj.find("[Name='PicUrl']").val("");
        
        //按区域确定数据源
        var datas = null;
        if (DisplaySite == 1) {
            datas = Home;
            IconName = "功能图标";
        }
        else if (DisplaySite == 3) {
            datas = More;
            IconName = "更多图标";
        }
        else if (DisplaySite == 2) {
            datas = Boot;
            IconName = "底部图标";
        }
        if ($.isNumeric(inputId)) {
            //初始化数据
            var data = FindById(datas, inputId);
            obj.find(".formName").text("修改" + IconName);
            obj.find("[Name='id']").val(data.Id);
            obj.find("[Name='Name']").val(data.Name);
            ModuleType.val(data.ModuleType);
            if (data.ModuleType == 1) {
                obj.find(".select").val(data.Value);
            }
            else {
                obj.find(".feaval").val(data.Value);
            }
            if (data.IsLock) {
                obj.find(".IsLock").val("True");
            }
            else { obj.find(".IsLock").val("False"); }
            //图片
            var PictureUrl = obj.find("[Name='PictureUrl']");
            var PictureUrlFeatimg = PictureUrl.parents("li").find(".featimg");
            PictureUrl.val(data.PictureUrl);
            PictureUrlFeatimg.attr("href", imgpath + data.PictureUrl);
            PictureUrlFeatimg.find("img").attr("src", imgpath + data.PictureUrl);

            var PicUrl = obj.find("[Name='PicUrl']");
            var PicUrlFeatimg = PicUrl.parents("li").find(".featimg");
            PicUrl.val(data.PicUrl);
            PicUrlFeatimg.attr("href", imgpath + data.PicUrl);
            PicUrlFeatimg.find("img").attr("src", imgpath + data.PicUrl);
            var action = obj.find(".chkForm").attr("SaveAction");
            obj.find(".chkForm").attr("action", action);
            featimg.show();
        }
        else {
            obj.find("[Name='id']").val(0);
            obj.find(".formName").text("添加" + IconName);
            obj.find(".chkForm")[0].reset();//重置表单内容
            featimg.hide();
            var action = obj.find(".chkForm").attr("AddAction");
            obj.find(".chkForm").attr("action", action);
        }
        obj.find("[Name='DisplaySite']").val(DisplaySite);
        this.moduleTypeSelect(ModuleType);
    }

    //功能类型选择动作与初始化
    this.moduleTypeSelect = function moduleTypeSelect(obj) {
        var optionValue = obj.val();
        var parent = obj.parents("ul");
        if (optionValue == 2) {
            parent.find(".selectdiv").hide();
            parent.find(".inputdiv").show();
            parent.find(".select").attr("disabled", true);
            parent.find(".feaval").attr("disabled", false);
        } else {
            parent.find(".inputdiv").hide();
            parent.find(".selectdiv").show();
            parent.find(".feaval").attr("disabled", true);
            parent.find(".select").attr("disabled", false);
        }
    }

    //广告表单
    this.FormAD = function FormAD(id) {
        var $Phone = this;
        var obj = this.UI.setcaroumod;
        var ADId = obj.find("#ADId");
        var PictureUrlFeatimg = obj.find(".featimg");
        var li = ADId.parents("li");
        //重置表单
        this.UI.setcaroumod.find("form")[0].reset();
        PictureUrlFeatimg.hide();
        
        obj.find("[Name='Pic']").val("");
        if (id == 0) {
            li.hide();
            $(".btn-del").hide();
            $(".btn-del").parent().removeClass("topbtn");
        }
        else {
            li.show();
            $(".btn-del").show();
            $(".btn-del").parent().addClass("topbtn");
            ADId.empty();
            $.each(this.Data.bannerInfo, function (i, n) {
                ADId.append('<option value="' + n.Id + '">' + n.Name + '</option>');
            });
        }
        //加载数据
        if (id != 0) {
            obj.find(".ADFromName").text("修改广告图");
            var n = FindById(this.Data.bannerInfo, id);
            if (n != null) {
                ADId.val(n.Id);
                obj.find(".carou-title").val(n.Name);
                obj.find(".hide-id").val(n.Id);
                obj.find(".genre").val(n.ModuleType);
                if (n.ModuleType == 1) {
                    obj.find(".select").val(n.Url);
                }
                else {
                    obj.find(".feaval").val(n.Url);
                }

                obj.find(".ADSort").val(n.Sort);
                var PictureUrl = obj.find("[Name='Pic']");
                PictureUrl.val(n.Pic);
                //图片
                PictureUrlFeatimg.attr("href", imgpathad + n.Pic);
                PictureUrlFeatimg.find("img").attr("src", imgpathad + n.Pic);
                PictureUrlFeatimg.show();
            }
        }
        else {
            ADId.val(0);
            obj.find(".ADFromName").text("添加广告图");
        }
        this.moduleTypeSelect(this.UI.setcaroumod.find(".genre"));
    }
};

//基础动作绑定
var BaseBinding = function () {
    var $Phone = this;
    var $Html = this.Html;
    var $UI = this.UI;

    //设置更多按钮
    var APPHasMore = $("#APPHasMore").val();
    this.showMoreButton(APPHasMore);//第一次加载更多按钮
    $("#APPHasMore").on("change", function () {//绑定更多按钮动作
        var NewAPPHasMore = $("#APPHasMore").val();
        $Phone.showMoreButton(NewAPPHasMore);
    });

    //设置宫格
    $("#AppBtnNumber").on("change", function () {
        $Phone.quantityFun();
    });

    //上传图片校验
    $(".imgbtn").click(function () {
        var form = $(this).parents(".chkForm");
        var icont = form.find(".icont-upload");
        var PictureUrl = icont.find("[Name='PictureUrl']").val();
        var PicUrl = icont.find("[Name='PicUrl']").val();
        var Pic = icont.find("[Name='Pic']").val();
        var ModuleType = form.find("[Name='ModuleType']").val();
        var Value1 = form.find("#Value1").val();
        var Value2 = form.find("#Value2").val();
        console.log(PictureUrl);
        console.log(PicUrl);
        if (ModuleType == "1" && Value1 == "0") {
            var fealist = form.find("#Value1");
            fealist.validationEngine('showPrompt', '请选择值', 'error');
            return false;
        }
        if (ModuleType == 2 && Value2 == "") {
            var fealist = form.find("#Value2");
            fealist.validationEngine('showPrompt', '请填写值', 'error');
            return false;
        }
        if (PictureUrl == "") {
            icont.validationEngine('showPrompt', '请上传图片', 'error');
            return false;
        }
        if (PicUrl == "") {
            var temp = form.find(".PicUrl");
            temp.validationEngine('showPrompt', '请上传图片', 'error');
            return false;
        }
        if (Pic == "") {
            var temp = form.find(".icont-upload");
            temp.validationEngine('showPrompt', '请上传图片', 'error');
            return false;
        }
        form.submit();
    });

    //清去浮动菜单
    $(".phone-right").on("mouseover", function () {
        $(".li-lasy").remove();
    });

    //上传文件
    $('.upLoadFile').on("click", function () {
        var url = '/Manage/Asyn/AppIco.html';
        var binding = '.upLoadFile';
        var UpLoadFile = Fileupload(url, binding, imgpath);
    });

    $('.upLoadFileAD').on("click", function () {
        var url = '/Manage/Asyn/ADPicture.html';
        var binding = '.upLoadFileAD';
        var UpLoadFile = Fileupload(url, binding, imgpathad);
    });

    $('.upLoadFileADP').on("click", function () {
        var url = '/Manage/Asyn/ADPicture.html';
        var binding = '.upLoadFileADP';
        var UpLoadFile = Fileupload(url, binding, imgpathad);
    });
    
    $('.upLoadFileBoot').on("click", function () {
        var url = '/Manage/Asyn/AppIco.html';
        var binding = '.upLoadFileBoot';
        var UpLoadFile = Fileupload(url, binding, imgpath);
    });
    
};

function UI() {
    //中下部
    this.featlist = $(".featlist");//home列表
    this.movelist = $(".movelist");//更多列表
    this.bottlist = $(".bottlist");//底部列表
    this.setfeatures = $(".set-features");//home、更多表单
    this.bootfeatures = $(".get-foot");//底部表单

    //上部
    this.screen = $(".screen");
    this.top = this.screen.find(".top");//广告图
    this.code = this.screen.find(".code");//扫码背景
    this.setcaroumod = $(".set-carou-modity");//修改banner图表单
    this.setscan = $(".set-scan");//修改/添加 扫码背景图
    this.reoud = $(".reoud");//广告图轮动位
    this.lasy = $(".lasy");//修改或添加广告按钮
};

function Html() {
    this.botlasy = $("<div class='lasy'></div>");

    if (IsAdd) {
        this.addstr = '<a class="btn btn-green add" href="javascript:" data-DisplaySite="2">添加</a>';
    }
    else { this.addstr = ''; }

    this.lasyhtml = '';
    if (IsSave) {
        this.lasyhtml += '<a class="btn btn-yellow modify"href="javascript:">修改</a>';
    }
    if (IsDelete) {
        this.lasyhtml += '<a class="btn btn-pink del"href="javascript:">删除</a>';
    }
    this.lasyhtml += '<span class="triangle"></span>';
    //this.lasyhtml = '<a class="btn btn-yellow modify"href="javascript:">修改</a><a class="btn btn-pink del"href="javascript:">删除</a><span class="triangle"></span>';
    this.spans = '';
    if (IsSave) {
        this.spans += '<span class="modify">修改</span>';
    }
    if (IsDelete) {
        this.spans += '<br/><span class="del">删除</span>';
    }
    //this.spans = '<span class="modify">修改</span><br/><span class="del">删除</span>';
    this.lilasy = $("<div class='li-lasy'></div>");
    this.lock = '<img class="lock_suo" src="/Content/HaoFu/image/lock_32.png" atl="锁定" />';
    this.bootAdd = '';
    if (IsAdd)
    {
    this.bootAdd += '<li class="draglist" >\
        <a href="javascript:void(0);" class="bottomFeatAdd" data-DisplaySite="2">\
            <img class="default" src="/Content/Manage/images/phoneAddGrey.png" />\
            <img class="active" src="/Content/Manage/images/phoneAddOrange.png" />\
            <p>添加</p>\
        </a>\
    </li>';
    }
};

function Data() {
    //广告 Json数据
    this.bannerInfo = jQuery.parseJSON(bannerInfostr);
};

var Ini = function () {//初始化
    //数据Html
    this.IniDataHtml();
    //图标
    this.IniIcon();
    //广告
    this.IniAD();
};

Phone.prototype.BaseBinding = BaseBinding;
Phone.prototype.Ini = Ini;

$(function () {
    var $Phone = new Phone();
    $Phone.BaseBinding();
    $Phone.Ini();
})

function Fileupload(url, binding,imgpath) {
    var This = $(binding);
    var MaxHigh = This.attr("MaxHigh") != null ? This.attr("MaxHigh") : 0;
    var MaxWidth = This.attr("MaxWidth") != null ? This.attr("MaxWidth") : 0;
    var MinHigh = This.attr("MinHigh") != null ? This.attr("MinHigh") : 0;
    var MinWidth = This.attr("MinWidth") != null ? This.attr("MinWidth") : 0;
    return This.fileupload({
        url: url,
        dataType: 'json',
        maxFileSize: 2000000,
        acceptFileTypes: new RegExp("(\.|\/)(png)$", "i"),
        formData: { "MaxHigh": MaxHigh, "MaxWidth": MaxWidth, "MinHigh": MinHigh, "MinWidth": MinWidth },
        //多语言支付
        messages: {
            maxNumberOfFiles: '最多能上传文件10个',
            acceptFileTypes: '文件类型不正确',
            maxFileSize: '上传文件最大2M',
            minFileSize: '上传文件最小1KB'
        },
        //上传成功后调用
        done: function (e, data) {
            var result = data.result;
            var icont = $(this).parents(".icont-upload");
            if (result.Status == true) {
                $(this);
                var PicFile = $(this).parents("li").find(".PicFile");
                var featimg = $(this).parents("li").find(".featimg");
                PicFile.val(result.Result);
                featimg.attr("href", imgpath + result.Result);
                featimg.find("img").attr("src", imgpath + result.Result);
                featimg.show();
            }
            else { alert(result.Message); }
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

var timerID;
function carimg(length) {
    var liwidth = -($(".imgwarp").find("li").width());
    var num = 0;
    var timer;    
    timer = setInterval(function () {
        if (length <= 1) {
            clearInterval(timer);
        }
        if (num > length - 1) {
            num = 0;
        }
        $(".imgwarp").css("left", num * liwidth);
        $(".reoud").find("li").eq(num).find("a").addClass("active").parent().siblings().find("a").removeClass("active");
        num++;
    }, 2000)
    timerID = timer;
}
//排序组件
function sequence(obj) {
    new Sortable(obj, {
        draggable: ".draglist"
    });
}
$(".reoud").ready(function () {
    var width = parseInt($(".reoud").width());
    $(".reoud").css("margin-left", (-width / 2));
})

/*按Id查询数据
 *Data:数据源
 *id:id
*/
function FindById(Data,id)
{
    var result = null;
    $.each(Data, function (i, n) {
        if (n.Id == id)
        {
            result = n;
            return;
        }
    });
    return result;
}
/*按Id删除数据
 *Data:数据源
 *id:id
*/
function DelById(Data, id) {
    for (var i = 0; i < Data.length; i++) {
        if (Data[i].Id == id) {
            Data.splice(i, 1);
        }
    }
}

/** 
 * 下面是一些基础函数，解决mouseover与mouserout事件不停切换的问题（问题不是由冒泡产生的） 
 */
function checkHover(e, target) {
    if (getEvent(e).type == "mouseover") {
        return !contains(target, getEvent(e).relatedTarget
                || getEvent(e).fromElement)
                && !((getEvent(e).relatedTarget || getEvent(e).fromElement) === target);
    } else {
        return !contains(target, getEvent(e).relatedTarget
                || getEvent(e).toElement)
                && !((getEvent(e).relatedTarget || getEvent(e).toElement) === target);
    }
}

function contains(parentNode, childNode) {
    if (parentNode.contains) {
        return parentNode != childNode && parentNode.contains(childNode);
    } else {
        return !!(parentNode.compareDocumentPosition(childNode) & 16);
    }
}
//取得当前window对象的事件  
function getEvent(e) {
    return e || window.event;
}
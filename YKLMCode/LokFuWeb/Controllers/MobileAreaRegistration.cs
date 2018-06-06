using System.Web.Mvc;
namespace LokFu.Areas.Mobile
{
    public class MobileAreaRegistration : AreaRegistrationOrder
    {
        public override string AreaName
        {
            get
            {
                return "Mobile";
            }
        }
        public override int Order
        {
            get
            {
                return 3;
            }
        }
        public override void RegisterAreaOrder(AreaRegistrationContext context)
        {
            string[] controllerNamespaces = new string[] { "LokFu.Areas.Mobile.Controllers" };
            string Pixber = string.Empty;
            string Number = string.Empty;
            context.MapRoute(
               Pixber + "MobileAbout",
               Number + "Mobile/About-{id}.html",
                new { controller = "Home", action = "About" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobileHelp",
               Number + "Mobile/Help-{id}.html",
                new { controller = "Home", action = "Help" }
                 , controllerNamespaces
            );
            #region 店铺
            context.MapRoute(
               Pixber + "MobileDigitalLabelIndex",
               Number + "Mobile/DigitalLabel/Index-{Id}.html",
                new { controller = "DigitalLabel", action = "Index" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobileShopIndex",
               Number + "Mobile/Shop/Index-{Id}.html",
                new { controller = "Shop", action = "Index" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobileShopInfo",
               Number + "Mobile/Shop/Info-{Id}.html",
                new { controller = "Shop", action = "Info" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobileShopPay",
               Number + "Mobile/Shop/Pay-{Id}.html",
                new { controller = "Shop", action = "Pay" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobileFastIndex",
               Number + "Mobile/Fast/Index-{Id}.html",
                new { controller = "Fast", action = "Index" }
                 , controllerNamespaces
            );
            #endregion
            #region 转盘
            context.MapRoute(
               Pixber + "MobileTurntableDefault",
               Number + "Mobile/Turntable/Index.html",
                new { controller = "Turntable", action = "Default" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobileTurntableIndex",
               Number + "Mobile/Turntable/Index-{tid}.html",
                new { controller = "Turntable", action = "Index" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobileTurntableRun",
               Number + "Mobile/Turntable/Run-{tid}.html",
                new { controller = "Turntable", action = "Run" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobileTurntableShare",
               Number + "Mobile/Turntable/Share-{uid}.html",
                new { controller = "Turntable", action = "Share" }
                 , controllerNamespaces
            );
            #endregion
            #region 10000还款
            context.MapRoute(
               Pixber + "MobileCutIndex",
               Number + "Mobile/Cut/Index-{cid}.html",
                new { controller = "Cut", action = "Index" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobileCutGetMoney",
               Number + "Mobile/Cut/GetMoney-{cid}.html",
                new { controller = "Cut", action = "GetMoney" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobileCutCheckMy",
               Number + "Mobile/Cut/CheckMy-{cid}.html",
                new { controller = "Cut", action = "CheckMy" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobileCutTakeMy",
               Number + "Mobile/Cut/TakeMy-{cid}.html",
                new { controller = "Cut", action = "TakeMy" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobileCutJuBao",
               Number + "Mobile/Cut/JuBao-{cid}.html",
                new { controller = "Cut", action = "JuBao" }
                 , controllerNamespaces
            );
            #endregion
            #region 抢IPhone
            context.MapRoute(
               Pixber + "MobileIPhoneIndex",
               Number + "Mobile/IPhone/Index-{cid}.html",
                new { controller = "IPhone", action = "Index" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobileIPhoneGetMoney",
               Number + "Mobile/IPhone/GetMoney-{cid}.html",
                new { controller = "IPhone", action = "GetMoney" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobileIPhoneCheckMy",
               Number + "Mobile/IPhone/CheckMy-{cid}.html",
                new { controller = "IPhone", action = "CheckMy" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobileIPhoneTakeMy",
               Number + "Mobile/IPhone/TakeMy-{cid}.html",
                new { controller = "IPhone", action = "TakeMy" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobileIPhoneJuBao",
               Number + "Mobile/IPhone/JuBao-{cid}.html",
                new { controller = "IPhone", action = "JuBao" }
                 , controllerNamespaces
            );
            #endregion
            #region 抢螃蟹
            context.MapRoute(
               Pixber + "MobilePangXieIndex",
               Number + "Mobile/PangXie/Index-{cid}.html",
                new { controller = "PangXie", action = "Index" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobilePangXieGetMoney",
               Number + "Mobile/PangXie/GetMoney-{cid}.html",
                new { controller = "PangXie", action = "GetMoney" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobilePangXieCheckMy",
               Number + "Mobile/PangXie/CheckMy-{cid}.html",
                new { controller = "PangXie", action = "CheckMy" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobilePangXieTakeMy",
               Number + "Mobile/PangXie/TakeMy-{cid}.html",
                new { controller = "PangXie", action = "TakeMy" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobilePangXieJuBao",
               Number + "Mobile/PangXie/JuBao-{cid}.html",
                new { controller = "PangXie", action = "JuBao" }
                 , controllerNamespaces
            );
            #endregion
            #region HaoWool
            context.MapRoute(
               Pixber + "HaoWoolInfo",
               Number + "Mobile/HaoWool/Info-{Id}.html",
                new { controller = "HaoWool", action = "Info" }
                 , controllerNamespaces
            );
            #endregion
            #region BanKa
            context.MapRoute(
               Pixber + "BanKa",
               Number + "Mobile/BanKa/Info-{Id}.html",
                new { controller = "BanKa", action = "Info" }
                 , controllerNamespaces
            );
            #endregion
            #region 注册
            context.MapRoute(
               Pixber + "MobileShareReg",
               Number + "Mobile/Reg/Index-{MyPId}-{PayConfigId}.html",
                new { controller = "Reg", action = "Index" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobileShareMoney",
               Number + "Mobile/Reg/Money-{MyPId}.html",
                new { controller = "Reg", action = "Money" }
                 , controllerNamespaces
            );
            #endregion
            #region 下载
            context.MapRoute(
               Pixber + "MobileDown",
               Number + "Mobile/Down/Index.html",
                new { controller = "Down", action = "Index" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobileDownAgent",
               Number + "Mobile/Down/Index-{Id}.html",
                new { controller = "Down", action = "Index" }
                 , controllerNamespaces
            );
            #endregion
            context.MapRoute(
               Pixber + "MobileApply",
               Number + "Mobile/Apply.html",
                new { controller = "Home", action = "Index" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobileDefault",
               Number + "Mobile/Index.html",
                new { controller = "Home", action = "Index" }
                 , controllerNamespaces
            );
            #region 错误
            context.MapRoute(
               Pixber + "MobileErrorAction",
               Number + "Mobile/Error.html",
                new { controller = "Home", action = "Error" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobileWeiXinErr",
               Number + "Mobile/WeiXinErr.html",
                new { controller = "Home", action = "WeiXinErr" }
                 , controllerNamespaces
            );
            #endregion
            #region 全局
            context.MapRoute(
               Pixber + "MobileController",
               Number + "Mobile/{controller}",
                new { controller = "Home", action = "Index" },
                controllerNamespaces
            );
            context.MapRoute(
               Pixber + "MobileAction",
               Number + "Mobile/{controller}/{action}.html",
                new { controller = "Home", action = "Index" },
                controllerNamespaces
            );
            context.MapRoute(
                Pixber + "MobileActionId",
                Number + "Mobile/{controller}/{action}/{id}.html",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                controllerNamespaces
            );
            #endregion
        }
    }
}

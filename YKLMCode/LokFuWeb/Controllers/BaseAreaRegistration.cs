using System.Web.Mvc;
namespace LokFu.Areas.Base
{
    public class BaseAreaRegistration : AreaRegistrationOrder
    {
        public override string AreaName
        {
            get
            {
                return "Base";
            }
        }
        public override int Order
        {
            get
            {
                return 10;
            }
        }
        public override void RegisterAreaOrder(AreaRegistrationContext context)
        {
            string[] controllerNamespaces = new string[] { "LokFu.Areas.Base.Controllers" };
            string Pixber = string.Empty;
            string Number = string.Empty;
            //home
            context.MapRoute(
               Pixber + "QrCodeAction",
               Number + "qc.html",
                new { controller = "QrCode", action = "Index" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "HomeAction",
               Number + "",
                new { controller = "Home", action = "Index" }
                 , controllerNamespaces
            );
            context.MapRoute(
              Pixber + "Controller",
              Number + "{controller}",
               new { controller = "Home", action = "Index" },
               controllerNamespaces
           );
            context.MapRoute(
               Pixber + "Action",
               Number + "{controller}/{action}.html",
                new { controller = "Home", action = "Index" },
                controllerNamespaces
            );
            context.MapRoute(
                Pixber + "ActionId",
                Number + "{controller}/{action}/{id}.html",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                controllerNamespaces
            );
            context.MapRoute(
               "ErrorAction",
               "{controller}/{action}.html",
               new { controller = "Exception", action = "Missing" },
               controllerNamespaces
           );
        }
    }
}

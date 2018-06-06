using System.Web.Mvc;
namespace LokFu.Areas.Base
{
    public class ShopAreaRegistration : AreaRegistrationOrder
    {
        public override string AreaName
        {
            get
            {
                return "Shop";
            }
        }
        public override int Order
        {
            get
            {
                return 5;
            }
        }
        public override void RegisterAreaOrder(AreaRegistrationContext context)
        {
            string[] controllerNamespaces = new string[] { "LokFu.Areas.Shop.Controllers" };
            string Pixber = string.Empty;
            string Number = string.Empty;
            context.MapRoute(
                 Pixber + "ShopForLogin",
                 Number + "Shop/Login.html",
                 new { controller = "Login", action = "Index" },
                 controllerNamespaces
             );
            context.MapRoute(
               Pixber + "ShopController",
               Number + "Shop/{controller}",
                new { controller = "Home", action = "Index" },
                controllerNamespaces
            );
            context.MapRoute(
               Pixber + "ShopAction",
               Number + "Shop/{controller}/{action}.html",
                new { controller = "Home", action = "Index" },
                controllerNamespaces
            );
            context.MapRoute(
                Pixber + "ShopActionId",
                Number + "Shop/{controller}/{action}/{id}.html",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                controllerNamespaces
            );
        }
    }
}

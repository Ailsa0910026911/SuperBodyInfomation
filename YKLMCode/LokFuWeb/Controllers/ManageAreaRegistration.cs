using System.Web.Mvc;
namespace LokFu.Areas.Manage
{
    public class ManageAreaRegistration : AreaRegistrationOrder
    {
        public override string AreaName
        {
            get
            {
                return "Manage";
            }
        }
        public override int Order
        {
            get
            {
                return 1;
            }
        }
        public override void RegisterAreaOrder(AreaRegistrationContext context)
        {
            string[] controllerNamespaces = new string[] { "LokFu.Areas.Manage.Controllers" };
            string Pixber = string.Empty;
            string Number = string.Empty;
            context.MapRoute(
                Pixber + "ManageForLogin",
                Number + "Manage/Login.html",
                new { controller = "Login", action = "Index"},
                controllerNamespaces
            );
            context.MapRoute(
               Pixber + "ManageController",
               Number + "Manage/{controller}",
                new { controller = "Home", action = "Index" },
                controllerNamespaces
            );
            context.MapRoute(
               Pixber + "ManageAction",
               Number + "Manage/{controller}/{action}.html",
                new { controller = "Home", action = "Index" },
                controllerNamespaces
            );
            context.MapRoute(
                Pixber + "ManageActionId",
                Number + "Manage/{controller}/{action}/{id}.html",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                controllerNamespaces
            );
        }
    }
}

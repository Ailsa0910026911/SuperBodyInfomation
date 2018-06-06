using System.Web.Mvc;
namespace LokFu.Areas.Agent
{
    public class AgentAreaRegistration : AreaRegistrationOrder
    {
        public override string AreaName
        {
            get
            {
                return "Agent";
            }
        }
        public override int Order
        {
            get
            {
                return 2;
            }
        }
        public override void RegisterAreaOrder(AreaRegistrationContext context)
        {
            string[] controllerNamespaces = new string[] { "LokFu.Areas.Agent.Controllers" };
            string Pixber = string.Empty;
            string Number = string.Empty;
            context.MapRoute(
                Pixber + "AgentForLogin",
                Number + "Agent/Login.html",
                new { controller = "Login", action = "Index" },
                controllerNamespaces
            );
            context.MapRoute(
               Pixber + "AgentController",
               Number + "Agent/{controller}",
                new { controller = "Home", action = "Index" },
                controllerNamespaces
            );
            context.MapRoute(
               Pixber + "AgentAction",
               Number + "Agent/{controller}/{action}.html",
                new { controller = "Home", action = "Index" },
                controllerNamespaces
            );
            context.MapRoute(
                Pixber + "AgentActionId",
                Number + "Agent/{controller}/{action}/{id}.html",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                controllerNamespaces
            );
        }
    }
}

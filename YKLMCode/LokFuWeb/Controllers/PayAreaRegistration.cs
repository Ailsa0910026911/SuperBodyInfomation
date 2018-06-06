using System.Web.Mvc;
namespace LokFu.Areas.Base
{
    public class PayAreaRegistration : AreaRegistrationOrder
    {
        public override string AreaName
        {
            get
            {
                return "PayCenter";
            }
        }
        public override int Order
        {
            get
            {
                return 4;
            }
        }
        public override void RegisterAreaOrder(AreaRegistrationContext context)
        {
            string[] controllerNamespaces = new string[] { "LokFu.Areas.Pay.Controllers" };
            string Pixber = string.Empty;
            string Number = string.Empty;
            context.MapRoute(
               Pixber + "PayDefault0",
               Number + "Pay/{tnum}.html",
                new { controller = "Pay", action = "GoPay" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "PayDefault1",
               Number + "PayCenter/Pay/Index.html",
                new { controller = "Pay", action = "Index" }
                 , controllerNamespaces
            );
            context.MapRoute(
               Pixber + "PayErrorAction",
               Number + "PayCenter/Error.html",
                new { controller = "Home", action = "Error" }
                 , controllerNamespaces
            );
            
            context.MapRoute(
               Pixber + "PayCardPayController",
               Number + "PayCenter/Pay-{Id}.html",
                new { controller = "Home", action = "Pay" },
                controllerNamespaces
            );

            context.MapRoute(
               Pixber + "PaySFPayController",
               Number + "PayCenter/SFPay/{action}-{TNum}-{CardNum}-{MDSign}.html",
                new { controller = "SFPay" },
                controllerNamespaces
            );
            context.MapRoute(
               Pixber + "PayWLBPayController",
               Number + "PayCenter/WLBPay/{action}-{TNum}-{CardNum}-{MDSign}.html",
                new { controller = "WLBPay" },
                controllerNamespaces
            );
            context.MapRoute(
               Pixber + "JiFuFdPayController",
               Number + "PayCenter/JiFuFdPay/{action}-{TNum}-{CardNum}-{MDSign}.html",
                new { controller = "JiFuFdPay" },
                controllerNamespaces
            );

            context.MapRoute(
               Pixber + "PayController",
               Number + "PayCenter/{controller}",
                new { controller = "Home", action = "Index" },
                controllerNamespaces
            );
            context.MapRoute(
               Pixber + "PayAction",
               Number + "PayCenter/{controller}/{action}.html",
                new { controller = "Home", action = "Index" },
                controllerNamespaces
            );
            context.MapRoute(
                Pixber + "PayActionId",
                Number + "PayCenter/{controller}/{action}/{id}.html",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                controllerNamespaces
            );
        }
    }
}

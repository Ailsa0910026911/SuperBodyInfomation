using System.Linq;
using System.Web.Mvc;
using LokFu.Repositories;
namespace LokFu.Areas.Mobile.Controllers
{
    public class AgreementController : BaseController
    {
        /// <summary>
        /// type=1 注册协议 type=3 实名认证协议
        /// </summary>
        public ActionResult Index(int type = 0,int id = 0)
        {
            string Agreement = string.Empty;

            SysSet SysSet = Entity.SysSet.FirstOrDefault();
            Agreement = SysSet.Agreement;
            if (!id.IsNullOrEmpty())
            {
                var SysAgent = Entity.SysAgent.FirstOrDefault(o => o.Id == id && o.IsTeiPai == 1 && o.Agreement != null && o.Agreement != "");
                if(SysAgent!=null)
                {
                    Agreement = SysAgent.Agreement;
                }
            }
            ViewBag.Agreement = Agreement;
            return View();
        }
    }
}
 
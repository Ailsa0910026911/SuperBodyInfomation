using LokFu.Extensions;
using LokFu.Repositories;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class JobSetController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.JobSet = Entity.JobSet.FirstOrNew(); 
            ViewBag.Save = this.checkPower("Save");
            ViewBag.Edit = this.checkPower("Edit");
            return View();
        }
        [ValidateInput(false)]
        public object Save(JobSet JobSet)
        {
            JobSet.Cost = JobSet.Cost / 1000;
            JobSet.VIPCost = JobSet.VIPCost / 1000;
            JobSet baseJobSet = Entity.JobSet.FirstOrNew();
            baseJobSet = Request.ConvertRequestToModel<JobSet>(baseJobSet, JobSet);
            Entity.SaveChanges();
            Response.Redirect("/Manage/JobSet/Index.html");
            return null;
        }
    }
}

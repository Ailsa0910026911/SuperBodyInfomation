using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.Extensions;
using LokFu.Repositories.SqlServer;
using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class UsersNervousController : BaseController
    {
        public ActionResult Index(int IsFirst = 0)
        {
            ViewBag.Xls = this.checkPower("Xls");
            return View();
        }
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="WTimes"></param>
        /// <param name="IsFirst"></param>
        /// <returns></returns>
        //[OutputCache(Duration = 3600)]
        [HttpPost]
        public JsonResult IndexSelect(DateTime? WTimes = null)
        {
            string code = "";
            string msg = "";
            string msgTongji = "";

            if (WTimes == null || WTimes.Value.ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd") || WTimes.Value >= DateTime.Now)
            {
                code = "10004";
                msg = "查询条件有误，请仔细核对！";
                return Json(new { code = code, msg = msg, msgTongji = msgTongji });
            }
            this.Session["UsersNervousList"] = null;
            IList<UsersNervousModel> UsersNervousList = null;
            Dictionary<string, string> dicChar = new Dictionary<string, string>();
            dicChar.Add("Timed", WTimes.Value.ToString("yyyy-MM-dd"));

            UsersNervousList = Entity.GetSPExtensions<UsersNervousModel>("SP_Statistics_Balance", dicChar);
            ViewBag.WTimes = WTimes;
            //ViewBag.UsersNervousList = UsersNervousList;
            this.Session["UsersNervousList"] = UsersNervousList;

            UsersNervousModel modelTongji = new UsersNervousModel();
            modelTongji.BeforeAmonut = UsersNervousList.Sum(x => x.BeforeAmonut);
            modelTongji.BeforeFrozen = UsersNervousList.Sum(x => x.BeforeFrozen);
            modelTongji.O_PayMoney = UsersNervousList.Sum(x => x.O_PayMoney);
            modelTongji.P_Amoney = UsersNervousList.Sum(x => x.P_Amoney);
            modelTongji.FZ_PayMoney = UsersNervousList.Sum(x => x.FZ_PayMoney);
            modelTongji.FW_PayMoney = UsersNervousList.Sum(x => x.FW_PayMoney);
            modelTongji.FN_PayMoney = UsersNervousList.Sum(x => x.FN_PayMoney);
            modelTongji.Share_AMOUNT = UsersNervousList.Sum(x => x.Share_AMOUNT);
            modelTongji.BF_Amount = UsersNervousList.Sum(x => x.BF_Amount);
            modelTongji.C_PayMoney_T1 = UsersNervousList.Sum(x => x.C_PayMoney_T1);
            modelTongji.C_PayMoney_T0 = UsersNervousList.Sum(x => x.C_PayMoney_T0);
            modelTongji.BO_Amount = UsersNervousList.Sum(x => x.BO_Amount);
            modelTongji.OH_PayMoney = UsersNervousList.Sum(x => x.OH_PayMoney);
            modelTongji.AfterAmonut = UsersNervousList.Sum(x => x.AfterAmonut);
            modelTongji.AfterFrozen = UsersNervousList.Sum(x => x.AfterFrozen);
            modelTongji.BL_Amount = UsersNervousList.Sum(x => x.BL_Amount);

            return LokFuWeb.Controllers.Extensions.MVCExtensions.LargeJson(this, new { code = "10000", msg = UsersNervousList, msgTongji = modelTongji });
        }
        public void XlsDo(DateTime? WTimes = null)
        {
            var UsersNervousList = this.Session["UsersNervousList"] as IList<UsersNervousModel>;
            if (UsersNervousList == null || UsersNervousList.Count <= 0)
            {
                Response.Write("暂无符合条件数据");
                return;
            }
            string tempname = "UsersNervous.xlsx";
            string file = Server.MapPath("/template") + "\\" + tempname;
            ExcelPackage package = new ExcelPackage(new FileInfo(file), true);
            var sheet = package.Workbook.Worksheets[1];
            var cells = sheet.Cells;
            int i = 4;
           
            foreach (var model in UsersNervousList)
            {
                cells["A" + i].Value = model.UserName;
                cells["B" + i].Value = model.NeekName;
                cells["C" + i].Value = model.TrueName;
                cells["D" + i].Value = model.State;
                cells["E" + i].Value = model.BeforeAmonut.ToString("F2");
                cells["F" + i].Value = model.BeforeFrozen.ToString("F2");
                cells["G" + i].Value = model.O_PayMoney.ToString("F2");
                cells["H" + i].Value = model.P_Amoney.ToString("F2");
                cells["I" + i].Value = model.FZ_PayMoney.ToString("F2");
                cells["J" + i].Value = model.FW_PayMoney.ToString("F2");
                cells["K" + i].Value = model.FN_PayMoney.ToString("F2");
                cells["L" + i].Value = model.Share_AMOUNT.ToString("F2");
                cells["M" + i].Value = model.BF_Amount.ToString("F2");
                cells["N" + i].Value = model.C_PayMoney_T1.ToString("F2");
                cells["O" + i].Value = model.C_PayMoney_T0.ToString("F2");
                cells["P" + i].Value = model.BO_Amount.ToString("F2");
                cells["Q" + i].Value = model.AfterAmonut.ToString("F2");
                cells["R" + i].Value = model.AfterFrozen.ToString("F2");
                cells["S" + i].Value = model.BL_Amount.ToString("F2");
                i++;
            }
            cells["A" + i].Value = "合计：";
            cells["B" + i].Value = "";
            cells["C" + i].Value = "";
            cells["D" + i].Value = "";
            cells["E" + i].Value = UsersNervousList.Sum(x => x.BeforeAmonut).ToString("F2");
            cells["F" + i].Value = UsersNervousList.Sum(x => x.BeforeFrozen).ToString("F2");
            cells["G" + i].Value = UsersNervousList.Sum(x => x.O_PayMoney).ToString("F2");
            cells["H" + i].Value = UsersNervousList.Sum(x => x.P_Amoney).ToString("F2");
            cells["I" + i].Value = UsersNervousList.Sum(x => x.FZ_PayMoney).ToString("F2");
            cells["J" + i].Value = UsersNervousList.Sum(x => x.FW_PayMoney).ToString("F2");
            cells["K" + i].Value = UsersNervousList.Sum(x => x.FN_PayMoney).ToString("F2");
            cells["L" + i].Value = UsersNervousList.Sum(x => x.Share_AMOUNT).ToString("F2");
            cells["M" + i].Value = UsersNervousList.Sum(x => x.BF_Amount).ToString("F2");
            cells["N" + i].Value = UsersNervousList.Sum(x => x.C_PayMoney_T1).ToString("F2");
            cells["O" + i].Value = UsersNervousList.Sum(x => x.C_PayMoney_T0).ToString("F2");
            cells["P" + i].Value = UsersNervousList.Sum(x => x.BO_Amount).ToString("F2");
            cells["Q" + i].Value = UsersNervousList.Sum(x => x.AfterAmonut).ToString("F2");
            cells["R" + i].Value = UsersNervousList.Sum(x => x.AfterFrozen).ToString("F2");
            cells["S" + i].Value = UsersNervousList.Sum(x => x.BL_Amount).ToString("F2");
            Response.BinaryWrite(package.GetAsByteArray());//输出
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;  filename=" + WTimes.Value.ToString("yyyy-MM-dd") + "号商户进出账明细表" + new Random().Next(10, 99) + ".xlsx"); ;
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Collections;
using System.Data;
using System.IO;
namespace LokFu.Areas.Manage.Controllers
{
    public class ApplyJoinController : BaseController
    {

        public ActionResult Index(ApplyJoin ApplyJoin, EFPagingInfo<ApplyJoin> p, DateTime? STime, DateTime? ETime, int IsFirst = 0)
        {
            if (STime.IsNullOrEmpty())
            {
                STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (ETime.IsNullOrEmpty())
            {
                // ETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                ETime = DateTime.Now;
            }
            ViewBag.ExcelExport = this.checkPower("ExcelExport");
            if (IsFirst==0)
            {
                PageOfItems<ApplyJoin> ApplyJoinList1 = new PageOfItems<ApplyJoin>(new List<ApplyJoin>(), 0, 10, 0, new Hashtable());
                ViewBag.ETime = ETime;
                ViewBag.STime = STime;
                ViewBag.ApplyJoinList = ApplyJoinList1;
                ViewBag.ApplyJoin = ApplyJoin;
                ViewBag.BasicProvinceList = Entity.BasicProvince.ToList();
                ViewBag.BasicCityList = Entity.BasicCity.ToList();
                ViewBag.Save = this.checkPower("Save");
                return View();
            }
            p.SqlWhere.Add(f => f.AddTime > STime && f.AddTime <ETime);
            if (!ApplyJoin.ServiceType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.ServiceType == ApplyJoin.ServiceType); }
            if (!ApplyJoin.ApplyType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.ApplyType == ApplyJoin.ApplyType); }
            if (!ApplyJoin.Linker.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Linker.Contains(ApplyJoin.Linker)); }
            if (!ApplyJoin.Mobile.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Mobile.Contains(ApplyJoin.Mobile)); }
            if (!ApplyJoin.ComName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.ComName.Contains(ApplyJoin.ComName)); }
            if (!ApplyJoin.Province.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Province == ApplyJoin.Province); }
            if (!ApplyJoin.City.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.City == ApplyJoin.City); }
            if (!ApplyJoin.TiePaiAgentId.IsNullOrEmpty())
            {
                if (ApplyJoin.TiePaiAgentId == 1)
                    p.SqlWhere.Add(f => f.TiePaiAgentId != null);
                else
                    p.SqlWhere.Add(f => f.TiePaiAgentId == null);
            }
            if (!ApplyJoin.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == ApplyJoin.State); }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<ApplyJoin> ApplyJoinList = Entity.Selects<ApplyJoin>(p);
            ViewBag.ETime = ETime;
            ViewBag.STime = STime;
            ViewBag.ApplyJoinList = ApplyJoinList;
            ViewBag.ApplyJoin = ApplyJoin;
            ViewBag.BasicProvinceList = Entity.BasicProvince.ToList();
            ViewBag.BasicCityList = Entity.BasicCity.ToList();
            // ViewBag.SysAgentList = Entity.SysAgent.ToList();
          
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(ApplyJoin ApplyJoin)
        {
            ViewBag.Remark = ApplyJoin.Remark;
            if (ApplyJoin.Id != 0) ApplyJoin = Entity.ApplyJoin.FirstOrDefault(n => n.Id == ApplyJoin.Id);
            if (ApplyJoin == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.ApplyJoin = ApplyJoin;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            ViewBag.BasicProvince = Entity.BasicProvince.FirstOrNew(n => n.Id == ApplyJoin.Province);
            ViewBag.BasicCity = Entity.BasicCity.FirstOrNew(n => n.Id == ApplyJoin.City);
            return View();
        }
        [ValidateInput(false)]
        public void Save(ApplyJoin ApplyJoin)
        {
            ApplyJoin baseApplyJoin = Entity.ApplyJoin.FirstOrDefault(n => n.Id == ApplyJoin.Id);
            if (ApplyJoin.Remark.IsNullOrEmpty())
            {
                ApplyJoin.Remark = "无备注";
            }
            string State = "无改变";
            if (ApplyJoin.State == 1)
            {
                baseApplyJoin.State = 2;
            }
            if (ApplyJoin.State == 2)
            {
                State = "有意向";
                baseApplyJoin.State = 2;
            }
            else if (ApplyJoin.State == 3)
            {
                State = "无意向";
                baseApplyJoin.State = 3;
            }
            else if (ApplyJoin.State == 4)
            {
                State = "已合作";
                baseApplyJoin.State = 4;
            }
            string Remark = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "§" + ApplyJoin.Remark + "§" + State + "§" + AdminUser.TrueName; ;
            if (baseApplyJoin.Remark.IsNullOrEmpty())
            {
                baseApplyJoin.Remark = Remark;
            }
            else
            {
                baseApplyJoin.Remark += "№" + Remark;
            }
            // baseApplyJoin = Request.ConvertRequestToModel<ApplyJoin>(baseApplyJoin, ApplyJoin);
            Entity.SaveChanges();
            BaseRedirect();
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="ApplyJoin"></param>
        /// <param name="p"></param>
        /// <param name="STime"></param>
        /// <param name="ETime"></param>
        /// <param name="IsShowSupAgent"></param>
        /// <returns></returns>
        public FileResult ExcelExport(ApplyJoin ApplyJoin, EFPagingInfo<ApplyJoin> p, DateTime? STime, DateTime? ETime, bool? IsShowSupAgent)
        {
            p.PageSize = 9999999;
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            p.SqlWhere.Add(f => f.AddTime > STime && f.AddTime < ETime);
            if (!ApplyJoin.ServiceType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.ServiceType == ApplyJoin.ServiceType); }
            if (!ApplyJoin.ApplyType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.ApplyType == ApplyJoin.ApplyType); }
            if (!ApplyJoin.Linker.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Linker.Contains(ApplyJoin.Linker)); }
            if (!ApplyJoin.Mobile.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Mobile.Contains(ApplyJoin.Mobile)); }
            if (!ApplyJoin.ComName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.ComName.Contains(ApplyJoin.ComName)); }
            if (!ApplyJoin.Province.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Province == ApplyJoin.Province); }
            if (!ApplyJoin.City.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.City == ApplyJoin.City); }
            if (!ApplyJoin.TiePaiAgentId.IsNullOrEmpty())
            {
                if (ApplyJoin.TiePaiAgentId == 1)
                    p.SqlWhere.Add(f => f.TiePaiAgentId != null);
                else
                    p.SqlWhere.Add(f => f.TiePaiAgentId == null);
            }
            if (!ApplyJoin.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == ApplyJoin.State); }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<ApplyJoin> ApplyJoinList = Entity.Selects<ApplyJoin>(p);
            
            DataTable table = new DataTable();
            DataRow row = null;

            // 创建 datatable
            table.Columns.Add(new DataColumn("联系人", typeof(string)));
            table.Columns.Add(new DataColumn("手机号", typeof(string)));
            table.Columns.Add(new DataColumn("公司名称", typeof(string)));
            table.Columns.Add(new DataColumn("所在省", typeof(string)));
            table.Columns.Add(new DataColumn("所在市", typeof(string)));
            table.Columns.Add(new DataColumn("申请时间", typeof(string)));
            table.Columns.Add(new DataColumn("状态", typeof(string)));
            table.Columns.Add(new DataColumn("上级代理名", typeof(string)));
            table.Columns.Add(new DataColumn("是否贴牌", typeof(string)));
           
            // 填充数据
            foreach (var item in ApplyJoinList)
            {
                BasicProvince BasicProvince = Entity.BasicProvince.FirstOrNew(n => n.Id == item.Province);
                BasicCity BasicCity = Entity.BasicCity.FirstOrNew(n => n.Id == item.City);
                string stateName="";
                row = table.NewRow();
                row[0] = item.Linker;
                row[1] = item.Mobile;
                row[2] = item.ComName;
                row[3] = BasicProvince.Name;
                row[4] = BasicCity.Name;
                row[5] = item.AddTime.ToString("yyyy-MM-dd");
                switch(item.State)
                {
                    case 1:
                        stateName="未跟进";
                        break;
                         case 2:
                        stateName="跟进中";
                        break;
                         case 3:
                        stateName="无意向";
                        break;
                         case 4:
                        stateName="已完成";
                        break;
                    default:
                        break;

                }
               
                row[6] = stateName;
                row[7] = item.AgentName;
                row[8] = item.TiePaiAgentId != null ? "是" : "否";
               
                table.Rows.Add(row);
            }
            string Time = STime.IsNullOrEmpty() ? "" : STime.Value.ToString("yyyy-MM-dd") + "至" + (ETime.IsNullOrEmpty() ? "" : ETime.Value.ToString("yyyy-MM-dd"));
            return this.ExportExcelBase(table, "合作申请    " + Time);
        }
    }
}

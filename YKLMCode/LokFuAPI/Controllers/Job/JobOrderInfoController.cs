using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Text;
namespace LokFu.Controllers
{
    public class JobOrderInfoController : InitController
    {
        public JobOrderInfoController()
        {
            if (!InitState)
            {
                DataObj.OutError("8080");
                return;
            }
            if (DataObj == null)
            {
                DataObj.OutError("1000");
                return;
            }
            if (!DataObj.IsReg)
            {
                DataObj.OutError("3002");
                return;
            }
        }
        public void Post()
        {
            string Data = DataObj.GetData();
            if (Data.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(Data);
            }
            catch (Exception Ex)
            {
                Log.Write("[JobOrders]:", "【Data】" + Data, Ex);
                DataObj.OutError("1000");
                return;
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            JobOrders JobOrders = new JobOrders();
            JobOrders = JsonToObject.ConvertJsonToModel(JobOrders, json);
            JobSet JobSet = Entity.JobSet.FirstOrNew();//获取配置

            #region 获取用户信息
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == JobOrders.Token);
            if (baseUsers == null)//用户令牌不存在
            {
                DataObj.OutError("2004");
                return;
            }
            if (baseUsers.State != 1)//用户被锁定
            {
                DataObj.OutError("2003");
                return;
            }
            #endregion

            JobOrders baseJobOrders = this.Entity.JobOrders.FirstOrDefault(o => o.TNum == JobOrders.TNum);
            if (baseJobOrders == null)
            {
                DataObj.OutError("1001");
                return;
            }
            baseJobOrders.Cols = baseJobOrders.Cols + ",Items,UsersCard,HPayMoney,XPayMoney,SetCost,SetCash,XFCount,AdvCost,AdvCash";

            //处理统计
            SP_JobReSet SP_JobReSet = new SP_JobReSet();
            Dictionary<string, string> dicChar = new Dictionary<string, string>();
            dicChar.Add("TNum", baseJobOrders.TNum);
            dicChar.Add("Type", "3");
            var tl = Entity.GetSPExtensions<SP_JobReSet>("SP_JobReSet", dicChar);
            if (tl.Count > 0)
            {
                SP_JobReSet = tl.FirstOrNew();
            }
            baseJobOrders.HPayMoney = SP_JobReSet.HPayMoney.ToString("f2");
            baseJobOrders.XPayMoney = SP_JobReSet.XPayMoney.ToString("f2");

            //处理订单明细
            List<JobItem> JobItemList = this.Entity.JobItem.Where(o => o.TNum == baseJobOrders.TNum).OrderBy(o => o.RunedTime).ToList();
            var JobItemModelList = JobItemList.GroupBy(o => o.RunTime.Date, (x, o) => new JobItemModel 
            { 
                date = x,
                item = o.OrderBy(k => k.RunType).ThenBy(n => n.RunTime).ToList().EntityToJson(),
            }).OrderBy(o => o.date).ToList();
            baseJobOrders.Items = JobItemModelList.EntityToJson();
            baseJobOrders.XFCount = JobItemList.Where(o => o.RunType == 1).Count();
            baseJobOrders.SetCost = baseJobOrders.UPayRate;
            baseJobOrders.SetCash = baseJobOrders.UCashMin;

            //处理银行卡信息
            UserCard UserCard = this.Entity.UserCard.FirstOrNew(o => o.Id == baseJobOrders.UserCardId && o.State == 1);
            string UserCardStr = UserCard.OutJson();
            JObject JS = new JObject();
            try
            {
                JS = (JObject)JsonConvert.DeserializeObject(UserCardStr);
            }
            catch (Exception Ex)
            {
                Log.Write("[JobOrders]:", "【JsStr】" + UserCardStr, Ex);
            }
            baseJobOrders.UsersCard = JS;

            string data = baseJobOrders.OutJson();
            data = data.Replace("\"[{", "[{").Replace("}]\"", "}]").Replace("\\", "").Replace("]\"}","]}");
            DataObj.Data = data;
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }

    public class JobItemModel
    {
        private string cols = "date,item";
        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
        public DateTime date { get; set; }
        public string item { get; set; }
    }

    public class SP_JobReSet
    {
        public decimal Amoney { get; set; }
        public decimal XPayMoney { get; set; }
        public decimal XCashPoundage { get; set; }
        public decimal HPayMoney { get; set; }
        public decimal HCashPoundage { get; set; }
    }
}

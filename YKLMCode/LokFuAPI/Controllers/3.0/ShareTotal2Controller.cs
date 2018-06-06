using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using LokFu;
using LokFu.Repositories;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;
using LokFu.Extensions;
using System.Data.Objects;

namespace LokFu.Controllers
{
    public class ShareTotal2Controller : InitController
    {
        public ShareTotal2Controller()
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
                Log.Write("[ShareTotal2]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            ShareTotal ShareTotal = new ShareTotal();
            ShareTotal = JsonToObject.ConvertJsonToModel(ShareTotal, json);

            //获取用户信息
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == ShareTotal.Token);
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
            if (baseUsers.CardStae != 2)//未实名认证
            {
                DataObj.OutError("2006");
                return;
            }
            if (baseUsers.MiBao != 1)//未设置支付密码
            {
                DataObj.OutError("2008");
                return;
            }

            IList<ShareTotal> STList = new List<ShareTotal>();
            SysSet SysSet = Entity.SysSet.FirstOrNew();
            for (byte i = 1; i <= SysSet.GlobaPromoteMaxLevel; i++) {
                ShareTotal st = Entity.ShareTotal.FirstOrDefault(n => n.UId == baseUsers.Id && n.Tier == i);
                if (st == null)
                {
                    st = new ShareTotal();
                    st.ShareNum = 0;
                    st.Amount = 0;
                    st.Profit = 0;
                    st.Tier = i;
                }
                STList.Add(st);
            }
            STList = STList.OrderBy(n => n.Tier).ToList();
            //计算总的
            int ShareNum = 0;
            decimal Amount = 0, Profit = 0;
            foreach (var p in STList)
            {
                ShareNum += p.ShareNum;
                Amount += p.Amount;
                Profit += p.Profit;
            }

            decimal Today = 0;
            decimal Yesterday = 0;
            DateTime tdate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            DateTime ldate = tdate.AddDays(-1);

            Today = Entity.OrderProfitLog.Where(n => n.UId == baseUsers.Id && n.LogType == 1 && n.AddTime > tdate).Sum(n => (decimal?)n.Profit) ?? 0m;
            Yesterday = Entity.OrderProfitLog.Where(n => n.UId == baseUsers.Id && n.LogType == 1 && n.AddTime > ldate && n.AddTime < tdate).Sum(n => (decimal?)n.Profit) ?? 0m;

            //增加汇总行
            ShareTotal ST = new ShareTotal();
            ST.ShareNum = ShareNum;
            ST.Amount = Amount;
            ST.Profit = Profit;
            ST.Tier = 0;

            ST.Total = Profit;
            ST.Today = Today;
            ST.Yesterday = Yesterday;

            
            //判断是否为代理商
            //SysAgent SysAgent = Entity.SysAgent.FirstOrDefault(o => o.LinkMobile == baseUsers.UserName && o.State == 1);
            //if (SysAgent != null)
            //{
            //    ST.UserType = 2;
            //}
            //if (ST.UserType == 1)
            //{
            //    ST.UserTotal = Entity.Users.Where(o => o.MyPId == baseUsers.Id).Count();
            //}
            //if (ST.UserType == 2)
            //{
            //    ST.UserTotal = Entity.Users.Where(o => o.Agent == SysAgent.Id && o.Id != baseUsers.Id).Count();
            //}

            string STStr = STList.EntityToJson();

            JArray JS = new JArray();
            try
            {
                JS = (JArray)JsonConvert.DeserializeObject(STStr);
            }
            catch (Exception Ex)
            {
                Log.Write("[ShareTotal2]:", "【STStr】" + STStr, Ex);
            }
            if (JS == null)
            {
                DataObj.OutError("1000");
                return;
            }

            ST.Json = JS;
            ST.Cols = "Total,Today,Yesterday,Json,UserType,UserTotal";
            //STList.Add(ST);
            DataObj.Data = ST.OutJson();

            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}

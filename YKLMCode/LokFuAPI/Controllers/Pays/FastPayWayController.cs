using LokFu.Extensions;
using LokFu.FastPay;
using LokFu.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Web.Script.Serialization;

namespace LokFu.Controllers
{
    public class FastPayWayController : InitController
    {
        public FastPayWayController()
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
                Log.Write("[FastPayWay]:", "【Data】" + Data, Ex);
                json = null;
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }

            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Users.Token);
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

            IList<FastPayWay> FastPayWayList = Entity.FastPayWay.Where(n => n.State == 1 && n.ManE == 0).OrderBy(n => n.Sort).ToList();
            if (FastPayWayList == null)
            {
                DataObj.OutError("2079");
                return;
            }
            IList<FastPayWay> PayWayList = new List<FastPayWay>();
            foreach (var p in FastPayWayList)
            {
                if (p.TimeType == 1)//限制时间，模式1
                {
                    DateTime STime = p.STime;
                    DateTime ETime = p.ETime;
                    DateTime NowSTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " " + STime.ToString("HH:mm:ss"));
                    DateTime NowETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " " + ETime.ToString("HH:mm:ss"));
                    if (NowSTime <= DateTime.Now && DateTime.Now <= NowETime)
                    {
                        //当前时间允许交易
                        PayWayList.Add(p);
                    }
                }
                else
                {
                    PayWayList.Add(p);
                }
            }
            if (PayWayList.Count < 1)
            {
                DataObj.OutError("2071");
                return;
            }
            IList<FastPayWay> WxList = PayWayList.Where(n => n.HasWeiXin == 1).ToList();
            IList<FastPayWay> AliList = PayWayList.Where(n => n.HasAliPay == 1).ToList();
            //IList<FastPayWay> BankList = PayWayList.Where(n => n.HasBank == 1).ToList();
            IList<FastPayWay> List = new List<FastPayWay>();
            if (WxList.Count > 0) {
                FastPayWay Wx = new FastPayWay();
                Wx.GroupType = "WeiXin";
                Wx.SNum = WxList.OrderBy(n => n.SNum2).FirstOrDefault().SNum2;
                Wx.ENum = WxList.OrderByDescending(n => n.ENum2).FirstOrDefault().ENum2;
                List.Add(Wx);
            }
            if (AliList.Count > 0)
            {
                FastPayWay Ali = new FastPayWay();
                Ali.GroupType = "AliPay";
                Ali.SNum = AliList.OrderBy(n => n.SNum).FirstOrDefault().SNum;
                Ali.ENum = AliList.OrderByDescending(n => n.ENum).FirstOrDefault().ENum;
                List.Add(Ali);
            }
            //if (BankList.Count > 0)
            //{
            //    FastPayWay Bank = new FastPayWay();
            //    Bank.GroupType = "Bank";
            //    Bank.SNum = BankList.OrderBy(n => n.BankSNum).FirstOrDefault().BankSNum;
            //    Bank.ENum = BankList.OrderByDescending(n => n.BankENum).FirstOrDefault().BankENum;
            //    List.Add(Bank);
            //}
            DataObj.Data = List.EntityToJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }

    }
}
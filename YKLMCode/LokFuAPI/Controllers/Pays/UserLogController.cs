using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LokFu.Controllers
{
    public class UserLogController : InitController
    {
        public UserLogController()
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
                Log.Write("[UserLog]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            UserLog UserLog = new UserLog();
            UserLog = JsonToObject.ConvertJsonToModel(UserLog, json);

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == UserLog.Token);
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
                //DataObj.OutError("2006");
                //return;
            }
            if (baseUsers.MiBao != 1)//未设置支付密码
            {
                //DataObj.OutError("2008");
                //return;
            }

            EFPagingInfo<UserLog> p = new EFPagingInfo<UserLog>();
            if (!UserLog.Pg.IsNullOrEmpty()) { p.PageIndex = UserLog.Pg; }
            if (!UserLog.Pgs.IsNullOrEmpty()) { p.PageSize = UserLog.Pgs; }

            bool IsOld = true;
            #region 版本比较
            //处理贴牌相关
            var SysAgent = this.Entity.SysAgent.FirstOrDefault(o => o.Id == baseUsers.Agent);
            if (SysAgent == null)
            {
                DataObj.OutError("1000");
                return;
            }
            var topSysAgent = SysAgent.GetTopAgent(this.Entity);
            
            if (!Equipment.SoftVer.IsNullOrEmpty())
            {
                Version v1 = new Version(Equipment.SoftVer);//当前版本
                Version v2 = new Version("1.0");

                if (Equipment.RqType.ToLower() == "apple")
                {
                    //苹果
                    if (topSysAgent.IsTeiPai == 0)//好付
                    {
                        v2 = new Version("8.0");
                    }
                    else//贴牌
                    {
                        v2 = new Version("6.0");
                    }

                }
                else if (Equipment.RqType.ToLower() == "android")
                {
                    //安卓
                    if (topSysAgent.IsTeiPai == 0)//好付
                    {
                        v2 = new Version("8.0.0");
                    }
                    else //贴牌
                    {
                        v2 = new Version("6.0");
                    }
                }
                if (v1 >= v2)
                {
                    IsOld = false;
                }
            }
            #endregion
            IPageOfItems<UserLog> List = null;
            if (!IsOld)//新版
            {
                p.SqlWhere.Add(f => f.UId == baseUsers.Id && f.OType != 4);
                var three = new int[] { 3, 5 };
                var five = new int[] { 6, 7, 9, 10, 11, 12 };
                switch(UserLog.OType)
                {
                    case 1:
                        p.SqlWhere.Add(o => o.OType == 1);
                        break;
                    case 2:
                        p.SqlWhere.Add(o => o.OType == 2);
                        break;
                    case 3:
                        p.SqlWhere.Add(o => three.Contains(o.OType));
                        break;
                    case 4:
                        p.SqlWhere.Add(o => o.OType == 8);
                        break;
                    case 5:
                        p.SqlWhere.Add(o => five.Contains(o.OType));      
                        break;
                    case 6:
                        p.SqlWhere.Add(o => o.OType == 15);
                        break;
                }
                p.OrderByList.Add("Id", "DESC");
                List = Entity.Selects<UserLog>(p);
            }
            else//旧版
            {
                p.SqlWhere.Add(f => f.UId == baseUsers.Id && f.OType != 4);
                //搜索
                if (!UserLog.OType.IsNullOrEmpty())
                {
                    if (UserLog.OType == 5)
                    {
                        p.SqlWhere.Add(f => f.OType == UserLog.OType || f.OType == 6 || f.OType == 11);
                    }
                    else if (UserLog.OType == 4)
                    {
                        p.SqlWhere.Add(f => f.OType == 8);
                    }
                    else if (UserLog.OType == 6)
                    {
                        IList<String> OrderList = Entity.OrderProfitLog.Where(o => o.UId == baseUsers.Id && o.LogType == 3).Select(o => o.TNum).ToList();
                        p.SqlWhere.Add(f => OrderList.Contains(f.OId));
                    }
                    else
                    {
                        p.SqlWhere.Add(f => f.OType == UserLog.OType);
                    }
                }
                p.OrderByList.Add("Id", "DESC");
                List = Entity.Selects<UserLog>(p);
                //处理转帐订单
                foreach (var pp in List)
                {
                    if (pp.OType == 6)
                    {
                        pp.OType = 5;
                    }
                    if (pp.OType == 12 || pp.OType == 11)
                    {
                        pp.OType = 2;
                    }
                    //pp.DoRemark(Entity);
                    if (pp.OType == 8)
                    {
                        OrderProfitLog OrderProfitLog = Entity.OrderProfitLog.FirstOrDefault(o => o.UId == pp.UId && o.TNum == pp.OId);
                        if (OrderProfitLog != null)
                        {
                            pp.ProfitName = OrderProfitLog.GetProfitName(Entity).ProfitName;
                        }
                    }
                }
            }
            IList<UserLog> iList = List.ToList();
            StringBuilder sb = new StringBuilder("");
            sb.Append("{");
            sb.Append(List.PageToString());
            sb.Append(",");
            sb.Append(iList.EntityToString());
            sb.Append("}");
            DataObj.Data = sb.ToString();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}

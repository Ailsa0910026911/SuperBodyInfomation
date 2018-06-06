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
    public class FastOrdersController : InitController
    {
        public FastOrdersController()
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
                Log.Write("[FastOrders]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            FastOrder FastOrder = new FastOrder();
            FastOrder = JsonToObject.ConvertJsonToModel(FastOrder, json);

            //var ford = this.Entity.FastOrder.FirstOrDefault(o => o.TNum == "20170606_01_0000024928");
            //if (ford != null)
            //{
            //    FastOrderExtensions.PushMsg(ford, this.Entity);
            //}
            
            //var Ord = new Orders()
            //{
            //    Id = 5555,
            //    TState =1,
            //    TType = 8,
            //    TNum = "20170625111111111111",
            //    Amoney = 50,
            //    Poundage = 8,
            //    AddTime = DateTime.Now,
            //    ComeWay = 1,
            //    UId = 566,
            //    PayState = 1,
            //    IdCardState = 0,
            //};
            //Ord.SendMsg(this.Entity);

            //System.Threading.Thread.Sleep(1000 * 60 * 1);

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == FastOrder.Token);
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

            EFPagingInfo<FastOrder> p = new EFPagingInfo<FastOrder>();
            if (!FastOrder.Pg.IsNullOrEmpty()) { p.PageIndex = FastOrder.Pg; }
            if (!FastOrder.Pgs.IsNullOrEmpty()) { p.PageSize = FastOrder.Pgs; }

            p.SqlWhere.Add(f => f.UId == baseUsers.Id);
            //搜索
            if (!FastOrder.OType.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.OType == FastOrder.OType);
            }
            if (!FastOrder.State.IsNullOrEmpty())
            {
                switch (FastOrder.State) { 
                    case 1:
                        p.SqlWhere.Add(f => f.State == 1 && f.PayState == 0);
                        break;
                    case 2:
                        p.SqlWhere.Add(f => f.State == 1 && f.PayState == 1);
                        break;
                    case 3:
                        p.SqlWhere.Add(f => f.State == 1 && f.PayState == 1 && f.UserState == 0);
                        break;
                    case 4:
                        p.SqlWhere.Add(f => f.State == 1 && f.PayState == 1 && f.UserState == 1);
                        break;
                }
            }
            if (!FastOrder.STime.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.AddTime >= FastOrder.STime);
            }
            if (!FastOrder.ETime.IsNullOrEmpty())
            {
                FastOrder.ETime = FastOrder.ETime.AddHours(24);
                p.SqlWhere.Add(f => f.AddTime <= FastOrder.ETime);
            }

            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<FastOrder> List = Entity.Selects<FastOrder>(p);
            //处理转帐订单
            foreach (var pp in List)
            {
                if (pp.State == 1)
                {
                    if (pp.PayState == 1)
                    {
                        if (pp.UserState == 1)
                        {
                            pp.State = 3;
                        }
                        else {
                            pp.State = 2;
                        }
                    }
                    else {
                        pp.State = 1;
                    }
                }
                else {
                    pp.State = 0;
                }
            }

            IList<FastOrder> iList = List.ToList();
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

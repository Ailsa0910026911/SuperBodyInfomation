using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;


namespace LokFu.Controllers
{
    public class Orders_2_0Controller : InitController
    {
        public Orders_2_0Controller()
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
                Log.Write("[Orders]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            Orders Orders = new Orders();
            Orders = JsonToObject.ConvertJsonToModel(Orders, json);

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Orders.Token);
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

            var result = new OrdersModel2();
            result.orders = new List<OrdersViewModel2>();
            if (Orders.TType.IsNullOrEmpty())
            {   
                string stime = new DateTime(2000,1,1).ToString("yyyy-MM-dd HH:mm:ss");
                if(!Orders.STime.IsNullOrEmpty())
                {
                    stime = Orders.STime.ToString("yyyy-MM-dd HH:mm:ss");
                }
                string etime = new DateTime(2030,1,1).ToString("yyyy-MM-dd HH:mm:ss");
                if(!Orders.ETime.IsNullOrEmpty())
                {
                    etime = Orders.ETime.AddDays(1).AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss");
                }
                Dictionary<string, string> dicChar = new Dictionary<string, string>();
                dicChar.Add("UserId", baseUsers.Id.ToString());
                dicChar.Add("STIME", stime);
                dicChar.Add("ETIME", etime);
                dicChar.Add("Page_Counts", Orders.Pgs.ToString());
                dicChar.Add("Page_Num", (Orders.Pg - 1).ToString());
                result.orders = Entity.GetSPExtensions<OrdersViewModel2>("SP_UserOrders", dicChar).ToList();
                result.orders.ForEach(o =>
                {
                    if (o.ruid == baseUsers.Id)
                    {
                        o.otype = 4;
                    }
                });
            }
            else
            {
                #region 交易订单条件
                if (Orders.PayWay == 1 || Orders.PayWay == 0)
                {
                    //通用
                    var Oquery = this.Entity.Orders.Where(o => o.UId == baseUsers.Id);
                    byte trueTType = 0;
                    switch(Orders.TType)
                    {
                        case 1:
                            trueTType = 8;
                            break;
                        case 2:
                            trueTType = 7;
                            break;
                        case 3:
                            trueTType = 1;
                            break;
                        case 4:
                            trueTType = 2;
                            break;
                        case 5:
                            trueTType = 6;
                            break;
                    }
                    Oquery = Oquery.Where(o => o.TType == trueTType);
                    if (Orders.TState == 1)
                    {
                        Oquery = Oquery.Where(o => o.TState == 0 );
                    }
                    if (Orders.TState == 3)
                    {
                        Oquery = Oquery.Where(o => o.PayState == 1 && (o.TDState != 2 && o.IdCardState != 6) && o.TState != 0);
                    }
                    if (!Orders.STime.IsNullOrEmpty())
                    {
                        Oquery = Oquery.Where(o => o.AddTime >= Orders.STime);
                    }
                    if (!Orders.ETime.IsNullOrEmpty())
                    {
                        Orders.ETime = Orders.ETime.AddDays(1).AddSeconds(-1);
                        Oquery = Oquery.Where(o => o.AddTime <= Orders.ETime);
                    }

                    //专用
                    if (Orders.TType == 4)//提现
                    {
                        if (Orders.TState == 4)
                        {
                            Oquery = Oquery.Where(o => o.TState == 1);
                        }
                        if (Orders.TState == 5)
                        {
                            Oquery = Oquery.Where(o => o.PayState == 1 && o.TState == 2);
                        }
                        if (Orders.TState == 6)
                        {
                            Oquery = Oquery.Where(o => o.PayState == 2);
                        }
                        if (Orders.TState == 7)
                        {
                            Oquery = Oquery.Where(o => o.TState == 3 || o.PayState == 3 || o.PayState == 4);
                        }
                    }
                    else//其它订单
                    {
                        if (Orders.TState == 2)
                        {
                            Oquery = Oquery.Where(o => o.InState == 0 && o.TState != 0 && o.PayState == 1);
                        }
                        if (Orders.TState == 4)
                        {
                            Oquery = Oquery.Where(o => o.TDState == 2 || o.IdCardState == 6);
                        }
                        if (Orders.TState == 5)
                        {
                            Oquery = Oquery.Where(o => o.IdCardState > 0 && o.IdCardState != 3 && o.IdCardState != 6 && o.TDState!=2);
                        }
                    }
                    
                    Entity.NoLockInvokeDB(() =>
                    {
                        result.orders = Oquery.OrderByDescending(o => o.Id).Skip((Orders.Pg - 1) * Orders.Pgs).Take(Orders.Pgs)
                        .Select(o => new OrdersViewModel2()
                        {
                            addtime = o.AddTime,
                            amoney = o.Amoney,
                            idcardstate = o.IdCardState,
                            instate = o.InState,
                            otype = o.RUId == baseUsers.Id ? (byte)4 : o.TType ,
                            paystate = o.PayState,
                            state = o.TState,
                            tdstate = o.TDState,
                            tnum = o.TNum,
                            uid = o.UId,
                            toway = 1,
                            userstate = 0,
                            rowid = 0,
                            lagentryday =  o.TrunType.Value != 0 ? o.TrunType.Value : o.LagEntryDay,
                        }).ToList();
                    });
                }
                #endregion

                #region 直通车订单条件
                if (Orders.PayWay == 2)
                {
                    var Fquery = this.Entity.FastOrder.Where(o => o.UId == baseUsers.Id);
                    byte trueTType = 0;
                    switch (Orders.TType)
                    {
                        case 1:
                            trueTType = 2;
                            break;
                        case 2:
                            trueTType = 1;
                            break;
                        case 3:
                            trueTType = 3;
                            break;
                    }
                    Fquery = Fquery.Where(o => o.OType == trueTType);
                    if (Orders.TState == 1)
                    {
                        Fquery = Fquery.Where(o => o.State == 0 || o.PayState == 0);
                    }
                    else if (Orders.TState == 2)
                    {
                        Fquery = Fquery.Where(o => o.PayState == 1);
                    }
                    else if (Orders.TState == 3)
                    {
                        Fquery = Fquery.Where(o => o.UserState == 0 && o.PayState == 1);
                    }
                    else if (Orders.TState == 4)
                    {
                        Fquery = Fquery.Where(o => o.UserState == 1);
                    }
                    else if (Orders.TState == 5)
                    {
                        Fquery = Fquery.Where(o => o.UserState == 2);
                    }
                    else if (Orders.TState == 6)
                    {
                        Fquery = Fquery.Where(o => o.UserState == 3);
                    }
                    else if (Orders.TState == 7)
                    {
                        Fquery = Fquery.Where(o => o.UserState == 4);
                    }

                    if (!Orders.STime.IsNullOrEmpty())
                    {
                        Fquery = Fquery.Where(o => o.AddTime >= Orders.STime);
                    }
                    if (!Orders.ETime.IsNullOrEmpty())
                    {
                        Orders.ETime = Orders.ETime.AddDays(1).AddSeconds(-1);
                        Fquery = Fquery.Where(o => o.AddTime <= Orders.ETime);
                    }

                    Entity.NoLockInvokeDB(() =>
                    {
                        result.orders = Fquery.OrderByDescending(o => o.Id).Skip((Orders.Pg - 1) * Orders.Pgs).Take(Orders.Pgs)
                        .Select(o => new OrdersViewModel2()
                        {
                            addtime = o.AddTime,
                            amoney = o.Amoney,
                            idcardstate = 0,
                            instate = 0,
                            otype = o.OType,
                            paystate = o.PayState,
                            state = o.State,
                            tdstate = 0,
                            tnum = o.TNum,
                            uid = o.UId,
                            toway = 2,
                            userstate = o.UserState,
                            rowid = 0,
                            lagentryday = (byte)(o.CashType == "D0" ? 0 : 1),
                        }).ToList();
                    });
                }
                #endregion
            }
            result.orders.ToName();

            #region 选项数据
            System.Text.StringBuilder sb = new System.Text.StringBuilder("");
            sb.Append("[{\"key\":\"全部\",\"value\":\"0\"},{\"key\":\"微信\",\"value\":\"1\"},{\"key\":\"支付宝\",\"value\":\"2\"},{\"key\":\"银联卡\",\"value\":\"3\"},{\"key\":\"提现\",\"value\":\"4\"},{\"key\":\"升级\",\"value\":\"5\"}]");
            var ttypeStr = sb.ToString();
            sb.Clear();
            var ttype = this.JsonTo<List<KeyValue>>(ttypeStr);
                                           
            sb.Append("{");
            sb.Append("\"k1\":[{\"key\":\"到APP账户\",\"value\":\"1\"},{\"key\":\"到银行卡\",\"value\":\"2\"}],");
            sb.Append("\"k2\":[{\"key\":\"到APP账户\",\"value\":\"1\"},{\"key\":\"到银行卡\",\"value\":\"2\"}],");
            sb.Append("\"k3\":[{\"key\":\"到APP账户\",\"value\":\"1\"},{\"key\":\"到银行卡\",\"value\":\"2\"}],");
            sb.Append("}");
            var paywayStr = sb.ToString();
            sb.Clear();
            var payway = this.JsonTo<Dictionary<string, List<KeyValue>>>(paywayStr);

            sb.Append("{");
            sb.Append("\"k1_1\":[{\"key\":\"交易关闭\",\"value\":\"1\"},{\"key\":\"待入账\",\"value\":\"2\"},{\"key\":\"已付\",\"value\":\"3\"},{\"key\":\"退单\",\"value\":\"4\"},{\"key\":\"调单\",\"value\":\"5\"}],");
            sb.Append("\"k1_2\":[{\"key\":\"交易关闭\",\"value\":\"1\"},{\"key\":\"已付\",\"value\":\"2\"},{\"key\":\"未结算\",\"value\":\"3\"},{\"key\":\"已结算\",\"value\":\"4\"},{\"key\":\"结算失败\",\"value\":\"5\"},{\"key\":\"处理中\",\"value\":\"6\"},{\"key\":\"待结算\",\"value\":\"7\"}],");
            sb.Append("\"k2_1\":[{\"key\":\"交易关闭\",\"value\":\"1\"},{\"key\":\"待入账\",\"value\":\"2\"},{\"key\":\"已付\",\"value\":\"3\"},{\"key\":\"退单\",\"value\":\"4\"},{\"key\":\"调单\",\"value\":\"5\"}],");
            sb.Append("\"k2_2\":[{\"key\":\"交易关闭\",\"value\":\"1\"},{\"key\":\"已付\",\"value\":\"2\"},{\"key\":\"未结算\",\"value\":\"3\"},{\"key\":\"已结算\",\"value\":\"4\"},{\"key\":\"结算失败\",\"value\":\"5\"},{\"key\":\"处理中\",\"value\":\"6\"},{\"key\":\"待结算\",\"value\":\"7\"}],");
            sb.Append("\"k3_1\":[{\"key\":\"交易关闭\",\"value\":\"1\"},{\"key\":\"待入账\",\"value\":\"2\"},{\"key\":\"已付\",\"value\":\"3\"},{\"key\":\"退单\",\"value\":\"4\"},{\"key\":\"调单\",\"value\":\"5\"}],");
            sb.Append("\"k3_2\":[{\"key\":\"交易关闭\",\"value\":\"1\"},{\"key\":\"已付\",\"value\":\"2\"},{\"key\":\"未结算\",\"value\":\"3\"},{\"key\":\"已结算\",\"value\":\"4\"},{\"key\":\"结算失败\",\"value\":\"5\"},{\"key\":\"处理中\",\"value\":\"6\"},{\"key\":\"待结算\",\"value\":\"7\"}],");
            sb.Append("\"k4\":[{\"key\":\"交易关闭\",\"value\":\"1\"},{\"key\":\"已付\",\"value\":\"3\"},{\"key\":\"处理中\",\"value\":\"4\"},{\"key\":\"出款中\",\"value\":\"5\"},{\"key\":\"已汇出\",\"value\":\"6\"},{\"key\":\"提现失败\",\"value\":\"7\"}],");
            sb.Append("\"k5\":[{\"key\":\"交易关闭\",\"value\":\"1\"},{\"key\":\"已付\",\"value\":\"3\"}]");
            sb.Append("}");
            var tstateStr = sb.ToString();
            sb.Clear();
            var tstate = this.JsonTo<Dictionary<string, List<KeyValue>>>(tstateStr);

            result.ttype = ttype;
            result.payway = payway;
            result.tstate = tstate;
            #endregion

            var page = new Page()
            {
                pg = Orders.Pg,
                Pgs = Orders.Pgs,
                totalpage = 300 / Orders.Pgs
            };
            result.page = page;
            IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            DataObj.Data = JsonConvert.SerializeObject(result, Formatting.Indented, timeFormat);
            DataObj.Code = "0000";
            DataObj.OutString();
        }

        private T JsonTo<T>(string str) where T :class
        {
            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
            var sr = new System.IO.StringReader(str.ToString());
            object o = serializer.Deserialize(new Newtonsoft.Json.JsonTextReader(sr), typeof(T));
            var r = o as T;
            return r;
        }
    }

    public class OrdersModel2
    {
        public List<KeyValue> ttype { get; set; }
        public Dictionary<string, List<KeyValue>> payway { get; set; }
        public Dictionary<string, List<KeyValue>> tstate { get; set; }

        public List<OrdersViewModel2> orders { get; set; }

        public Page page { get; set; }
        
    }

    public class Page
    {
        public int pg { get; set; }

        public int Pgs { get; set; }

        public int totalpage { get; set; }
    }

    public class KeyValue
    {
        public string key { get; set; }
        public string value { get; set; }
    }

    public class OrdersViewModel2
    {
        public Int64 rowid { get; set; }

        public int uid { get; set; }

        public string tnum { get; set; }

        public byte otype { get; set; }

        public decimal amoney { get; set; }

        public byte state { get; set; }

        public DateTime addtime { get; set; }

        public byte paystate { get; set; }

        public byte userstate { get; set; }

        public int toway { get; set; }

        public byte instate { get; set; }

        public byte tdstate { get; set; }

        public byte idcardstate { get; set; }

        public string statename { get; set; }

        public string otypename { get; set; }

        public string colour { get; set; }

        public int ruid { get; set; }

        public byte lagentryday { get; set; }
    }

    public static class OrdersViewModel2Ext
    {
        public static void ToName(this List<OrdersViewModel2> orders)
        {
            orders.ForEach(o =>
            {
                #region 直通车订单
                if (o.toway == 2)
                {
                    o.otypename = Utils.GetFastOrderModel().FirstOrNew(n => n.Id == o.otype).Name;

                    var temp = new FastOrder()
                    {
                        State = o.state,
                        PayState = o.paystate,
                        UserState = o.userstate,
                        OType = o.otype
                    };
                    o.statename = temp.GeStateName();
                    o.colour = temp.GeStateColour();
                }
                #endregion

                #region 交易订单
                if (o.toway == 1)
                {
                    if (o.otype != 4)
                    {
                        IList<OrdersModel> OML = Utils.GetOrdersModel();
                        OrdersModel OrdersModel = OML.FirstOrNew(n => n.Id == o.otype);
                        o.otypename = OrdersModel.Name;
                    }
                    else
                    {
                        o.otypename = "转账";
                    }
                    
                    var temp = new Orders()
                    {
                        TState = o.state,
                        TDState = o.tdstate,
                        TType = o.otype,
                        InState = o.instate,
                        IdCardState = o.idcardstate,
                        PayState = o.paystate,
                    };
                    o.statename = temp.GetState();
                    o.colour = temp.GeStateColour();
                }
                #endregion
            });
        }
    }

    
}

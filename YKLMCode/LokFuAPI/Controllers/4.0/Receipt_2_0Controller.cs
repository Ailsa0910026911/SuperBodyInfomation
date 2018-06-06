using LokFu.Extensions;
using LokFu.FastPay;
using LokFu.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LokFu.Controllers
{
    /// <summary>
    /// 收款接口2.0
    /// </summary>
    public class Receipt_2_0Controller : InitController
    {
        private string[] AllowTag = new string[] { "Alipay", "NFC", "Recharge", "RecMoneyLocal", "RecMoneyMulti", "WeiXin" };

        public Receipt_2_0Controller()
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
            if (!Data.IsNullOrEmpty())
            {
                JObject json = new JObject();
                try
                {
                    json = (JObject)JsonConvert.DeserializeObject(Data);
                }
                catch (Exception Ex)
                {
                    Log.Write("[Receipt]:", "【Data】" + Data, Ex);
                }
                if (json == null)
                {
                    DataObj.OutError("1000");
                    return;
                }
                var Users = new Users();
                Users = JsonToObject.ConvertJsonToModel(Users, json);
                Users BaseUsers = Entity.Users.FirstOrDefault(o => o.Token == Users.Token);
                if (BaseUsers == null)//用户令牌不存在
                {
                    DataObj.OutError("2004");
                    return;
                }
                if (BaseUsers.State != 1)//用户被锁定
                {
                    DataObj.OutError("2003");
                    return;
                }
                if (BaseUsers.CardStae != 2)//未实名认证
                {
                    DataObj.OutError("2006");
                    return;
                }
                //if (BaseUsers.MiBao != 1)//未设置支付密码
                //{
                //    DataObj.OutError("2008");
                //    return;
                //}

                //获取直通车配置及用户直通车配置
                bool IsBindingFast = true;
                FastUser FastUser = Entity.FastUser.FirstOrDefault(n => n.UId == BaseUsers.Id);
                if (FastUser == null)
                {
                    //开通帐户
                    FastUser = new FastUser();
                    FastUser.UId = BaseUsers.Id;
                    FastUser.TrueName = BaseUsers.TrueName;
                    FastUser.CardId = BaseUsers.CardId;
                    FastUser.AddTime = DateTime.Now;
                    Entity.FastUser.AddObject(FastUser);
                    Entity.SaveChanges();
                }

                if (FastUser.Card.IsNullOrEmpty())
                {
                    IsBindingFast = false;
                }
                else
                { 
                    FastConfig FastConfig = Entity.FastConfig.FirstOrNew();
                    //===========================================================
                    //这里要处理商户入驻
                    BusFastPay.AddMer(FastUser, FastConfig, this.Entity);
                    //===========================================================
                }

                var result = new Receipt2Model();

                result.FastUser = new FastUserModel()
                {
                    card = FastUser.Card,
                    bank = FastUser.Bank,
                    bin = FastUser.Bin,
                };
                List<PayWayModel> PayWayModelList = new List<PayWayModel>();
                SysSet SysSet = Entity.SysSet.FirstOrDefault();

                #region 整合数据
                DateTime now = new DateTime(1990,01,01,DateTime.Now.Hour,DateTime.Now.Minute,DateTime.Now.Second);

                #region 交易通道
                var SCquery = Entity.SysControl.Where(o => AllowTag.Contains(o.Tag) && (o.State == 1 || o.State == 2) && ((((now >= o.STime && now <= o.ETime && o.STime < o.ETime) || ((now >= o.STime || now <= o.ETime) && o.STime > o.ETime)) && o.TimeType == 1) || o.TimeType == 0));
                if (SysSet.LagEntry == 0)
                {
                    SCquery = SCquery.Where(o => o.LagEntryNum == 0);
                }
                List<SysControl> SysControlList = SCquery.OrderBy(o=>o.Sort).ToList();
                string UserPayCashName = "UserPay_" + BaseUsers.Id.ToString() + "_" + Equipment.RqType;
                List<UserPay> UserPayList = null;

                if (HasCache)
                {
                    UserPayList = CacheBuilder.EntityCache.Get(UserPayCashName, null) as List<UserPay>;
                    if (UserPayList == null)
                    {
                        UserPayList = Entity.UserPay.Where(n => n.UId == BaseUsers.Id).ToList();
                        CacheBuilder.EntityCache.Remove(UserPayCashName, null);
                        CacheBuilder.EntityCache.Add(UserPayCashName, UserPayList, DateTime.Now.AddHours(1), null);
                    }
                }
                else
                {
                    UserPayList = Entity.UserPay.Where(n => n.UId == BaseUsers.Id).ToList();
                }

                SysControlList.ForEach(o =>
                {
                    var PayWayModel = new PayWayModel()
                    {
                        id = o.Id,
                        name = o.CName,
                        cost = o.LagEntryDay > 0 ? 0m : (decimal)UserPayList.Where(x => x.PId == o.PayWay).Select(x => x.Cost).FirstOrNew(),
                        s_num = o.SNum,
                        e_num = o.ENum,
                        state = o.State,
                        sort = o.Sort,
                        tag = o.Tag,
                        lagentryday = o.LagEntryDay,
                        paywaytype = 1,
                        cash = 0,
                        payway = o.PayWay,
                    };
                    PayWayModelList.Add(PayWayModel);
                });
                #endregion

                #region 直通车通道
                if (IsBindingFast)
                {
                    List<FastPayWay> FastPayWayList = Entity.FastPayWay.Where(o => o.State == 1 && o.ManE == 0 && ((now >= o.STime && now <= o.ETime && o.TimeType == 1) || o.TimeType == 0)).OrderBy(o => o.Sort).ToList();
                    string FastUserPayCashName = "FastUserPay_" + BaseUsers.Id.ToString() + "_" + Equipment.RqType;
                    List<FastUserPay> FastUserPayList = null;
                    if (HasCache)
                    {
                        FastUserPayList = CacheBuilder.EntityCache.Get(FastUserPayCashName, null) as List<FastUserPay>;
                        if (FastUserPayList == null)
                        {
                            FastUserPayList = Entity.FastUserPay.Where(n => n.UId == BaseUsers.Id).ToList();
                            CacheBuilder.EntityCache.Remove(FastUserPayCashName, null);
                            CacheBuilder.EntityCache.Add(FastUserPayCashName, FastUserPayList, DateTime.Now.AddHours(1), null);
                        }
                    }
                    else
                    {
                        FastUserPayList = Entity.FastUserPay.Where(n => n.UId == BaseUsers.Id).ToList();
                    }

                    FastPayWayList.ForEach(o =>
                    {
                        var tempList = new List<PayWayModel>();
                        if (o.HasAliPay == 1)
                        {
                            tempList.Add(new PayWayModel() { tag = "Alipay", name = "支付宝"+o.ShowName, s_num = o.SNum2, e_num = o.ENum2, payway = 1 });
                        }
                        if (o.HasWeiXin == 1)
                        {
                            tempList.Add(new PayWayModel() { tag = "WeiXin", name = "微信" + o.ShowName, s_num = o.SNum, e_num = o.ENum, payway = 2 });
                        }
                        if (o.HasBank == 1)
                        {
                            tempList.Add(new PayWayModel() { tag = "RecMoneyLocal", name = "银联"+o.ShowName, s_num = o.BankSNum, e_num = o.BankENum, payway = 3 });
                        }
                        foreach (var item in tempList)
                        {
                            FastUserPay FastUserPay = FastUserPayList.FirstOrNew(x => x.PayWay == o.Id);
                            item.id = o.Id;
                            //2017-11-12 通道支持微信/支付宝/银联不同费率
                            if (item.payway == 1)
                            {
                                item.cost = FastUserPay.UserCost2;
                            }
                            if (item.payway == 2)
                            {
                                item.cost = FastUserPay.UserCost;
                            }
                            if (item.payway == 3)
                            {
                                item.cost = FastUserPay.UserCost3;
                            }
                            item.cash = FastUserPay.UserCash;

                            item.state = o.State;
                            item.sort = o.Sort;
                            item.paywaytype = 2;
                            
                            if (o.GroupType == "D0")
                            {
                                item.lagentryday = 0;
                            }
                            else if (o.GroupType == "T1")
                            {
                                item.lagentryday = 1;
                            }
                        };
                        PayWayModelList.AddRange(tempList);
                    });
                }
                #endregion

                PayWayModelList = PayWayModelList.OrderBy(n => n.sort).ToList();//排序
                //一条通道都没有的情况处理
                //if (PayWayModelList.Count == 0)
                //{
                //    var none = new PayWayModel()
                //    {
                //        cash = 2,
                //        cost = 0.006m,
                //        s_num = 1000,
                //        e_num = 20000,
                //        id = 99999,
                //        lagentryday = 0,
                //        payway = 0,
                //        paywaytype = 2,
                //        sort = 0,
                //        tag = "RecMoneyLocal",
                //        state = 0,
                //        name = "银联",

                //    };
                //    PayWayModelList.Add(none);
                //}

                result.UserPay = PayWayModelList;
                #endregion
                
                DataObj.Data = JsonConvert.SerializeObject(result);
                DataObj.Code = "0000";
                DataObj.OutString();
            }
        }


        public class Receipt2Model
        {
            public List<PayWayModel> UserPay { get; set; }

            public FastUserModel FastUser { get; set; }
        }

        public class FastUserModel
        {
            public string bank { get; set; }
            public string card { get; set; }

            public string bin { get; set; }
        }

        public class PayWayModel
        {
            public int id { get; set; }
            /// <summary>
            /// 费率
            /// </summary>
            public decimal cost { get; set; }
            /// <summary>
            /// 手续费
            /// </summary>
            public decimal cash { get; set; }
            /// <summary>
            /// 名称
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// 状态 0停用 1正常
            /// </summary>
            public int state { get; set; }

            /// <summary>
            /// 最小交易金额
            /// </summary>
            public decimal s_num { get; set; }

            /// <summary>
            /// 最大交易金额
            /// </summary>
            public decimal e_num { get; set; }
            /// <summary>
            /// 通道ID
            /// </summary>
            public int payway { get; set; }

            /// <summary>
            /// 通道类型
            /// 1.到钱包 2.到银行卡
            /// </summary>
            public int paywaytype { get; set; }
            /// <summary>
            /// 到账天
            /// </summary>
            public int lagentryday { get; set; }

            /// <summary>
            /// 图标
            /// </summary>
            public string tag { get; set; }

            public int sort { get; set; }

        }
    }

   
}

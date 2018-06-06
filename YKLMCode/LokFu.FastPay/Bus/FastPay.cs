using LokFu.Extensions;
using LokFu.HFJS;
using LokFu.HFJS.HFJSModels;
using LokFu.HFJS.HFJSResults;
using LokFu.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LokFu.FastPay
{
    public class BusFastPay
    {
        public static FastUser AddMer(FastUser FastUser, FastConfig FastConfig, LokFuEntity Entity)
        {
            if (FastUser.Card.IsNullOrEmpty())
            {
                //未绑定银行卡
                return FastUser;
            }
            //批量处理所有接口
            IList<FastPayWay> FastPayWayList = Entity.FastPayWay.Where(n => n.State == 1).OrderBy(n => n.Sort).ToList();
            foreach (var p in FastPayWayList)
            {
                try
                { 
                    AddMer(FastUser, p, FastConfig, Entity);
                }
                catch(Exception e)
                {
                    Utils.WriteLog("/FastPay/AddMer" + e.Message + e.StackTrace, "bug", "API");
                }
            }
            return FastUser;
        }
        public static FastUser AddMer(FastUser FastUser, FastPayWay FastPayWay, FastConfig FastConfig, LokFuEntity Entity)
        {
            if (FastUser.Card.IsNullOrEmpty())
            {
                //未绑定银行卡
                return FastUser;
            }
            FastUserPay FastUserPay = Entity.FastUserPay.FirstOrDefault(n => n.UId == FastUser.UId && n.PayWay == FastPayWay.Id);
            if (FastUserPay == null)
            {
                FastUserPay = new FastUserPay();
                FastUserPay.UId = FastUser.UId;
                FastUserPay.PayWay = FastPayWay.Id;
                if (FastPayWay.DllName == "HFPay")
                {
                    //不需要一户一码直接开通商户
                    FastUserPay.MerId = "HFPay" + FastUserPay.UId.ToString();
                    FastUserPay.MerState = 1;
                    FastUserPay.CardState = 1;
                    FastUserPay.BusiState = 1;
                }
                else
                {
                    FastUserPay.MerId = "";
                    FastUserPay.MerState = 2;//状态 0锁定 1正常 2待提交 3审核中 4审核失败
                    FastUserPay.CardState = 2;//状态 0锁定 1正常 2待提交 3审核中 4审核失败
                    FastUserPay.BusiState = 2;//状态 1正常 2待提交 3审核中 4审核失败
                }
                FastUserPay.CardName = FastUser.TrueName;
                FastUserPay.Bank = FastUser.Bank;
                FastUserPay.Card = FastUser.Card;
                FastUserPay.Bin = FastUser.Bin;
                //2017-11-22 修改成取通道配置，且分别计算微信/支付宝/银联
                //FastUserPay.UserCost = FastConfig.UserCost;
                FastUserPay.UserCost = FastPayWay.InCost;
                FastUserPay.UserCost2 = FastPayWay.InCost2;
                FastUserPay.UserCost3 = FastPayWay.InCost3;


                if (FastPayWay.GroupType == "T1")
                {
                    FastUserPay.UserCash = 0;
                }
                else
                {
                    FastUserPay.UserCash = FastConfig.UserCash;
                }
                FastUserPay.AddTime = DateTime.Now;
                Entity.FastUserPay.AddObject(FastUserPay);
                Entity.SaveChanges();
            }
            else
            {
                bool Save = false;
                if (FastUserPay.CardName != FastUser.TrueName)
                {
                    FastUserPay.CardName = FastUser.TrueName;
                    Save = true;
                }
                if (FastUserPay.Bank != FastUser.Bank)
                {
                    FastUserPay.Bank = FastUser.Bank;
                    Save = true;
                }
                if (FastUserPay.Card != FastUser.Card)
                {
                    FastUserPay.Card = FastUser.Card;
                    Save = true;
                }
                if (FastUserPay.Bin != FastUser.Bin)
                {
                    FastUserPay.Bin = FastUser.Bin;
                    Save = true;
                }
                if (Save)
                {
                    Entity.SaveChanges();
                }
            }
            string[] PayConfigArr = FastPayWay.QueryArray.Split(',');
            if (FastPayWay.DllName == "HFJSPay")
            {
                #region 结算系统
                if (PayConfigArr.Length == 3)
                {
                    string Code = PayConfigArr[0];
                    string CodeKey = PayConfigArr[1];
                    string PayWayCode = PayConfigArr[2];
                    #region 进件
                    if (FastUserPay.MerState == 2 || FastUserPay.MerState == 4)
                    {
                        UserCard UserCard = Entity.UserCard.FirstOrNew(n => n.Card == FastUserPay.Card && n.UId == FastUser.UId);
                        string Mobile = UserCard.Mobile;
                        Users Users = Entity.Users.FirstOrNew(n => n.Id == FastUser.UId);
                        if (Mobile.IsNullOrEmpty())
                        {
                            Mobile = Users.UserName;
                        }
                        fastuseraddModel model = new fastuseraddModel()
                        {
                            code = Code,
                            mchid = "HF" + Users.Id.ToString(),
                            mchname = Users.NeekName,
                            truename = Users.TrueName,
                            cardno = Users.CardId,
                            accountcard = UserCard.Card,
                            accountbin = UserCard.Bin,
                            accountmobile = UserCard.Mobile
                        };
                        fastuserResult fastuserResult = HFJSTools.fastuseradd(model, CodeKey);
                        if (fastuserResult.respcode == "00")
                        {
                            bool RunTrue = true;
                            string Msg = "";
                            if (fastuserResult.respmsg == "存在相同外部商户号") { 
                                //这里面要修改一下结算卡
                                fastuseraddModel modelEdit = new fastuseraddModel()
                                {
                                    code = Code,
                                    merid = fastuserResult.merid,
                                    cardno = Users.CardId,
                                    accountcard = UserCard.Card,
                                    accountbin = UserCard.Bin,
                                    accountmobile = UserCard.Mobile
                                };
                                fastuserResult fastuserresult = HFJSTools.fastuseredit(modelEdit, CodeKey);
                                if (fastuserresult.respcode != "00") {
                                    Msg = fastuserresult.respmsg;
                                    RunTrue = false;
                                }
                            }
                            if (RunTrue)
                            {
                                if (fastuserResult.state == 1)
                                {
                                    FastUserPay.MerState = 1;
                                    FastUserPay.CardState = 1;//这里已绑定结算卡
                                    FastUserPay.MerId = fastuserResult.merid;
                                    FastUserPay.MerKey = fastuserResult.merkey;
                                }
                                else if (fastuserResult.state == 2)
                                {
                                    FastUserPay.MerId = fastuserResult.merid;
                                    FastUserPay.MerKey = fastuserResult.merkey;
                                    FastUserPay.MerState = 3;
                                }
                                else
                                {
                                    FastUserPay.MerState = 4;
                                    FastUserPay.MerMsg = fastuserResult.respcode + "[" + fastuserResult.respmsg + "]";
                                }
                            }
                            else {
                                FastUserPay.MerState = 4;
                                FastUserPay.MerMsg = Msg;
                            }
                        }
                        else
                        {
                            FastUserPay.MerState = 4;
                            FastUserPay.MerMsg = fastuserResult.respcode + "[" + fastuserResult.respmsg + "]";
                        }
                        Entity.SaveChanges();
                    }
                    #endregion
                    #region 开通道
                    if (FastUserPay.MerState == 1 && (FastUserPay.BusiState == 2 || FastUserPay.BusiState == 4))
                    {
                        decimal Cost = FastPayWay.InCost3;//刷卡手续费
                        Users Users = Entity.Users.FirstOrNew(n => n.Id == FastUser.UId);
                        if (Users.IsVip == 1)
                        {
                            Cost = 0.005M;
                            FastUserPay.UserCost3 = 0.005M;
                        }
                        decimal Cash = FastConfig.UserCash;//还款手续费
                        userspayopenbModel userspayopenbModel = new userspayopenbModel()
                        {
                            merid = FastUserPay.MerId,
                            paywaycode = PayWayCode,
                            code = Code,
                            bankcost = Cost,
                            //surcharge = 0,
                            cash = Cash,
                            bankcostmin = 0,
                            bankcostmax = 9999999
                        };
                        fastuserResult fastuserResult = HFJSTools.userspayopen(userspayopenbModel, CodeKey);
                        if (fastuserResult.respcode == "00")
                        {
                            if (fastuserResult.state == 1)
                            {
                                FastUserPay.BusiState = 1;
                            }
                            else
                            {
                                FastUserPay.BusiState = 4;
                                FastUserPay.BusiMsg = fastuserResult.respcode + "[" + fastuserResult.respmsg + "]";
                            }
                        }
                        else
                        {
                            FastUserPay.BusiState = 4;
                            FastUserPay.BusiMsg = fastuserResult.respcode + "[" + fastuserResult.respmsg + "]";
                        }
                        Entity.SaveChanges();
                    }

                    #endregion
                }
                #endregion
            }
            return FastUser;
        }
        public static FastUserPay AddCard(FastUser FastUser, FastUserPay FastUserPay, FastPayWay FastPayWay, LokFuEntity Entity)
        {
            //添加/修改结算卡------不需要修改的需到上一步添加
            if (FastUserPay.CardState == 2)
            {
                string[] PayConfigArr = FastPayWay.QueryArray.Split(',');
                if (FastPayWay.DllName == "HFJSPay")
                {
                    #region 结算中心
                    if (PayConfigArr.Length == 3)
                    {
                        UserCard UserCard = Entity.UserCard.FirstOrNew(n => n.Card == FastUserPay.Card && n.UId == FastUser.UId);
                        string Mobile = UserCard.Mobile;
                        Users Users = Entity.Users.FirstOrNew(n => n.Id == FastUser.UId);
                        if (Mobile.IsNullOrEmpty())
                        {
                            Mobile = Users.UserName;
                        }
                        string Code = PayConfigArr[0];
                        string CodeKey = PayConfigArr[1];
                        string PayWayCode = PayConfigArr[2];
                        fastuseraddModel model = new fastuseraddModel()
                        {
                            code = Code,
                            merid = FastUserPay.MerId,
                            cardno = Users.CardId,
                            accountcard = UserCard.Card,
                            accountbin = UserCard.Bin,
                            accountmobile = UserCard.Mobile
                        };
                        fastuserResult fastuserResult = HFJSTools.fastuseredit(model, CodeKey);
                        if (fastuserResult.respcode == "00")
                        {
                            FastUserPay.CardState = 1;
                            Entity.SaveChanges();
                        }
                        else
                        {
                            FastUserPay.CardMsg = fastuserResult.respcode + "[" + fastuserResult.respmsg + "]";
                            FastUserPay.CardState = 4;
                            Entity.SaveChanges();
                        }
                    }
                    #endregion
                }
            }
            return FastUserPay;
        }
    }
}

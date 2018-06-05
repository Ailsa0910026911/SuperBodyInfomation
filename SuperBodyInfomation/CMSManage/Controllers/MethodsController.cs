﻿using CTModel;
using SuperBodyInfomation.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMSManage.Controllers
{
    public class MethodsController : Controller
    {
        //Test
        private CTContext ct = new CTContext();
        public string ChangeAllState()
        {
            string[] ids = Request["IDs"].Split(new char[] { ',' });
            var Now = DateTime.Now;
            try
            {
                foreach (var id in ids)
                {
                    int nid = int.Parse(id);
                    var model = ct.FastOrder.Where(o => o.Id == nid).FirstOrDefault();
                    model.PayState = 1;
                    model.UserState = 1;
                    model.AgentWay = 1;
                    model.PayTime = Now;
                    model.UserTime = Now;
                }
                ct.SaveChanges();
                return "1";
            }
            catch
            {
                return "0";
            }
        }
        public string ChangeState()
        {
            string id = Request["id"];
            var Now = DateTime.Now;
            try
            {
                int nid = int.Parse(id);
                var model = ct.FastOrder.Where(o => o.Id == nid).FirstOrDefault();
                model.PayState = 1;
                model.UserState = 1;
                model.AgentWay = 1;
                model.PayTime = Now;
                model.UserTime = Now;
                ct.SaveChanges();
                return "1";
            }
            catch
            {
                return "0";
            }
        }
        // GET: Methods
        public string GetPoint(decimal num)
        {
            var num1 = num.ToString();
            var nl = num1.IndexOf(".");
            var num2 = num1.Substring(nl);
            return num2;
        }
        //分润算法
        public string ShareProfit(string id = "", decimal? money = 0)
        {
            var a = id;
            //取出分润比例Type=0：信用卡
            var bsp = ct.BusinessShareProfit.Where(o => o.Type == 0).FirstOrDefault();
            var Id = int.Parse(id);
            //查找用户
            var user = ct.Users.Where(o => o.Id == Id && o.IsVip == 1).FirstOrDefault();
            if (user != null)
            {
                //查找代理商
                var sysagent = ct.SysAgent.Where(o => o.Id == user.Agent).ToList();
                OrderProfitLog op = new OrderProfitLog();
                var logType = "1";
                decimal? bspnum = 0;
                //生成订单号
                var Tnum = "FX" + MD5Helper.getMd5Hash(DateTime.Now.ToString());
                //生成订单
                op.AddTime = DateTime.Now;
                op.IsDel = 0;
                op.UId = user.Id;
                op.TNum = Tnum;
                op.UserName = user.UserName;
                op.OrderType = 22; /*(22：分销分润)*/
                op.Amoney = decimal.Parse(money.ToString());

                #region VIP用户分润
                logType = "1";
                op.LogType = byte.Parse(logType);
                op.Tier = 0;
                bspnum = bsp.S0_0;
                op.Agent = 0;
                op.Profit = decimal.Parse((money * bspnum).ToString());
                ct.OrderProfitLog.Add(op);
                //对用户的账户进行操作
                user.Amount = user.Amount + op.Profit;
                user.Sp_Amount = user.Sp_Amount + op.Profit;
                ct.SaveChanges();
                #endregion
                //用户的代理商
                var Agent = ct.SysAgent.Where(o => o.Id == user.Agent).FirstOrDefault();
                if (Agent != null)
                {
                    logType = "2";
                    op.LogType = byte.Parse(logType);
                    op.Tier = Agent.Tier;
                    op.Agent = user.Agent;
                    //代理商一级分润
                    if (Agent.Tier == 4)
                    {
                        bspnum = bsp.S1_4;
                        //对代理商的账户进行操作
                        var S1_4 = ct.Users.Where(o => o.Id == Agent.MyUId).FirstOrDefault();
                        if (S1_4 != null)
                        {
                            op.UserName = user.UserName;
                            op.UId = S1_4.Id;
                            op.Profit = decimal.Parse((money * bspnum).ToString());
                            ct.OrderProfitLog.Add(op);
                            S1_4.Amount = S1_4.Amount + op.Profit;
                            S1_4.Sp_Amount = S1_4.Sp_Amount + op.Profit;
                            ct.SaveChanges();
                            //同级代理
                            #region 同级分润
                            var tagent = ct.SysAgent.Where(o => o.Id == Agent.SameAgent).FirstOrDefault();
                            if (tagent != null)
                            {
                                var S1_1 = ct.Users.Where(o => o.Id == tagent.MyUId).FirstOrDefault();
                                if (S1_1 != null)
                                {
                                    bspnum = bsp.S1_1;
                                    op.Tier = tagent.Tier;
                                    op.UserName = S1_4.UserName;
                                    op.UId = S1_1.Id;
                                    op.Profit = decimal.Parse((money * bspnum).ToString());
                                    ct.OrderProfitLog.Add(op);
                                    //对用户的账户进行操作
                                    S1_1.Amount = S1_1.Amount + op.Profit;
                                    S1_1.Sp_Amount = S1_1.Sp_Amount + op.Profit;
                                    ct.SaveChanges();
                                }
                            }
                            #endregion
                        }
                    }
                    else if (Agent.Tier == 5)
                    {
                        bspnum = bsp.S1_5;
                        //对代理商的账户进行操作
                        var S1_5 = ct.Users.Where(o => o.Id == Agent.MyUId).FirstOrDefault();
                        if (S1_5 != null)
                        {
                            op.UserName = user.UserName;
                            op.UId = S1_5.Id;
                            op.Profit = decimal.Parse((money * bspnum).ToString());
                            ct.OrderProfitLog.Add(op);
                            S1_5.Amount = S1_5.Amount + op.Profit;
                            S1_5.Sp_Amount = S1_5.Sp_Amount + op.Profit;
                            ct.SaveChanges();
                            //同级代理
                            #region 同级分润
                            var tagent = ct.SysAgent.Where(o => o.Id == Agent.SameAgent).FirstOrDefault();
                            if (tagent != null)
                            {
                                var S1_1 = ct.Users.Where(o => o.Id == tagent.MyUId).FirstOrDefault();
                                if (S1_1 != null)
                                {
                                    bspnum = bsp.S1_1;
                                    op.Tier = tagent.Tier;
                                    op.UserName = S1_5.UserName;
                                    op.UId = S1_1.Id;
                                    op.Profit = decimal.Parse((money * bspnum).ToString());
                                    ct.OrderProfitLog.Add(op);
                                    //对用户的账户进行操作
                                    S1_1.Amount = S1_1.Amount + op.Profit;
                                    S1_1.Sp_Amount = S1_1.Sp_Amount + op.Profit;
                                    ct.SaveChanges();
                                }
                            }
                            #endregion
                            //上级代理商
                            var upAgent = ct.SysAgent.Where(o => o.Id == Agent.AgentID).FirstOrDefault();
                            if (upAgent != null)
                            {
                                //判断代理商的层级
                                if (upAgent.Tier == 4)
                                {
                                    op.Tier = upAgent.Tier;
                                    op.Agent = upAgent.Id;
                                    bspnum = bsp.S2_5_4;
                                    //对代理商的账户进行操作
                                    var S2_5_4 = ct.Users.Where(o => o.Id == upAgent.MyUId).FirstOrDefault();
                                    if (S2_5_4 != null)
                                    {
                                        op.UserName = S1_5.UserName;
                                        op.UId = S2_5_4.Id;
                                        op.Profit = decimal.Parse((money * bspnum).ToString());
                                        ct.OrderProfitLog.Add(op);
                                        S2_5_4.Amount = S2_5_4.Amount + op.Profit;
                                        S2_5_4.Sp_Amount = S2_5_4.Sp_Amount + op.Profit;
                                        ct.SaveChanges();
                                        //同级代理
                                        #region 同级分润
                                        var uptagent = ct.SysAgent.Where(o => o.Id == upAgent.SameAgent).FirstOrDefault();
                                        if (uptagent != null)
                                        {
                                            var S1_1 = ct.Users.Where(o => o.Id == uptagent.MyUId).FirstOrDefault();
                                            if (S1_1 != null)
                                            {
                                                bspnum = bsp.S1_1;
                                                op.Tier = uptagent.Tier;
                                                op.UserName = S2_5_4.UserName;
                                                op.UId = S1_1.Id;
                                                op.Profit = decimal.Parse((money * bspnum).ToString());
                                                ct.OrderProfitLog.Add(op);
                                                //对用户的账户进行操作
                                                S1_1.Amount = S1_1.Amount + op.Profit;
                                                S1_1.Sp_Amount = S1_1.Sp_Amount + op.Profit;
                                                ct.SaveChanges();
                                            }
                                        }
                                        #endregion
                                    }
                                }
                            }
                        }
                    }
                    else if (Agent.Tier == 6)
                    {
                        bspnum = bsp.S1_6;
                        var S1_6 = ct.Users.Where(o => o.Id == Agent.MyUId).FirstOrDefault();
                        if (S1_6 != null)
                        {
                            op.UserName = user.UserName;
                            op.UId = S1_6.Id;
                            op.Profit = decimal.Parse((money * bspnum).ToString());
                            ct.OrderProfitLog.Add(op);
                            S1_6.Amount = S1_6.Amount + op.Profit;
                            S1_6.Sp_Amount = S1_6.Sp_Amount + op.Profit;
                            ct.SaveChanges();
                            //同级代理
                            #region 同级分润
                            var tagent = ct.SysAgent.Where(o => o.Id == Agent.SameAgent).FirstOrDefault();
                            if (tagent != null)
                            {
                                var S1_1 = ct.Users.Where(o => o.Id == tagent.MyUId).FirstOrDefault();
                                if (S1_1 != null)
                                {
                                    bspnum = bsp.S1_1;
                                    op.Tier = tagent.Tier;
                                    op.UserName = S1_6.UserName;
                                    op.UId = S1_1.Id;
                                    op.Profit = decimal.Parse((money * bspnum).ToString());
                                    ct.OrderProfitLog.Add(op);
                                    //对用户的账户进行操作
                                    S1_1.Amount = S1_1.Amount + op.Profit;
                                    S1_1.Sp_Amount = S1_1.Sp_Amount + op.Profit;
                                    ct.SaveChanges();
                                }
                            }
                            #endregion
                            //上级代理商
                            var upAgent = ct.SysAgent.Where(o => o.Id == Agent.AgentID).FirstOrDefault();
                            if (upAgent != null)
                            {
                                //判断代理商的层级
                                if (upAgent.Tier == 4)
                                {
                                    op.Tier = upAgent.Tier;
                                    op.Agent = upAgent.Id;
                                    bspnum = bsp.S2_6_4;
                                    //对代理商的账户进行操作
                                    var S2_6_4 = ct.Users.Where(o => o.Id == upAgent.MyUId).FirstOrDefault();
                                    if (S2_6_4 != null)
                                    {
                                        op.UserName = S1_6.UserName;
                                        op.UId = S2_6_4.Id;
                                        op.Profit = decimal.Parse((money * bspnum).ToString());
                                        ct.OrderProfitLog.Add(op);
                                        S2_6_4.Amount = S2_6_4.Amount + op.Profit;
                                        S2_6_4.Sp_Amount = S2_6_4.Sp_Amount + op.Profit;
                                        ct.SaveChanges();
                                        //同级代理
                                        #region 同级分润
                                        var uptagent = ct.SysAgent.Where(o => o.Id == upAgent.SameAgent).FirstOrDefault();
                                        if (uptagent != null)
                                        {
                                            var S1_1 = ct.Users.Where(o => o.Id == uptagent.MyUId).FirstOrDefault();
                                            if (S1_1 != null)
                                            {
                                                bspnum = bsp.S1_1;
                                                op.Tier = uptagent.Tier;
                                                op.UserName = S2_6_4.UserName;
                                                op.UId = S1_1.Id;
                                                op.Profit = decimal.Parse((money * bspnum).ToString());
                                                ct.OrderProfitLog.Add(op);
                                                //对用户的账户进行操作
                                                S1_1.Amount = S1_1.Amount + op.Profit;
                                                S1_1.Sp_Amount = S1_1.Sp_Amount + op.Profit;
                                                ct.SaveChanges();
                                            }
                                        }
                                        #endregion
                                    }
                                }
                                else if (upAgent.Tier == 5)
                                {
                                    op.Tier = upAgent.Tier;
                                    op.Agent = upAgent.Id;
                                    bspnum = bsp.S2_6_5;
                                    //对代理商的账户进行操作
                                    var S2_6_5 = ct.Users.Where(o => o.Id == upAgent.MyUId).FirstOrDefault();
                                    if (S2_6_5 != null)
                                    {
                                        op.UserName = S1_6.UserName;
                                        op.UId = S2_6_5.Id;
                                        op.Profit = decimal.Parse((money * bspnum).ToString());
                                        ct.OrderProfitLog.Add(op);
                                        S2_6_5.Amount = S2_6_5.Amount + op.Profit;
                                        S2_6_5.Sp_Amount = S2_6_5.Sp_Amount + op.Profit;
                                        ct.SaveChanges();
                                        //同级代理
                                        #region 同级分润
                                        var uptagent = ct.SysAgent.Where(o => o.Id == upAgent.SameAgent).FirstOrDefault();
                                        if (uptagent != null)
                                        {
                                            var S1_1 = ct.Users.Where(o => o.Id == uptagent.MyUId).FirstOrDefault();
                                            if (S1_1 != null)
                                            {
                                                bspnum = bsp.S1_1;
                                                op.Tier = uptagent.Tier;
                                                op.UserName = S2_6_5.UserName;
                                                op.UId = S1_1.Id;
                                                op.Profit = decimal.Parse((money * bspnum).ToString());
                                                ct.OrderProfitLog.Add(op);
                                                //对用户的账户进行操作
                                                S1_1.Amount = S1_1.Amount + op.Profit;
                                                S1_1.Sp_Amount = S1_1.Sp_Amount + op.Profit;
                                                ct.SaveChanges();
                                            }
                                        }
                                        #endregion
                                        //上级代理商
                                        var up1Agent = ct.SysAgent.Where(o => o.Id == upAgent.AgentID).FirstOrDefault();
                                        if (up1Agent != null)
                                        {
                                            op.Tier = up1Agent.Tier;
                                            op.Agent = up1Agent.AgentID;
                                            bspnum = bsp.S3_6_5_4;
                                            //对代理商的账户进行操作
                                            var S3_6_5_4 = ct.Users.Where(o => o.Id == up1Agent.MyUId).FirstOrDefault();
                                            if (S3_6_5_4 != null)
                                            {
                                                op.UserName = S3_6_5_4.UserName;
                                                op.UId = S3_6_5_4.Id;
                                                op.Profit = decimal.Parse((money * bspnum).ToString());
                                                ct.OrderProfitLog.Add(op);
                                                S3_6_5_4.Amount = S3_6_5_4.Amount + op.Profit;
                                                S3_6_5_4.Sp_Amount = S3_6_5_4.Sp_Amount + op.Profit;
                                                ct.SaveChanges();
                                                //同级代理
                                                #region 同级分润
                                                var up1tagent = ct.SysAgent.Where(o => o.Id == up1Agent.SameAgent).FirstOrDefault();
                                                if (up1tagent != null)
                                                {
                                                    var S1_1 = ct.Users.Where(o => o.Id == up1tagent.MyUId).FirstOrDefault();
                                                    if (S1_1 != null)
                                                    {
                                                        bspnum = bsp.S1_1;
                                                        op.Tier = up1tagent.Tier;
                                                        op.UserName = S3_6_5_4.UserName;
                                                        op.UId = S1_1.Id;
                                                        op.Profit = decimal.Parse((money * bspnum).ToString());
                                                        ct.OrderProfitLog.Add(op);
                                                        //对用户的账户进行操作
                                                        S1_1.Amount = S1_1.Amount + op.Profit;
                                                        S1_1.Sp_Amount = S1_1.Sp_Amount + op.Profit;
                                                        ct.SaveChanges();
                                                    }
                                                }
                                                #endregion
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }
            else
            {
                return "该用户不是Vip不能参与分润！";
            }
            return "分润成功！";
        }
    }
}
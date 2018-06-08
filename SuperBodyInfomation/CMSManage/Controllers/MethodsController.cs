using CMSManage.Extended;
using CTModel;
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
        //获取分润百分比
        public decimal? GetSplit(int tier)
        {
            decimal? Split = 0;
            var bsp = ct.BusinessShareProfit.Where(o => o.Type == 0).FirstOrDefault();
            if (tier == 4)
            {
                Split = bsp.S1_4;
            }
            else if (tier == 5)
            {
                Split = bsp.S1_5;
            }
            else if (tier == 6)
            {
                Split = bsp.S1_6;
            }
            return Split;
        }
        public int GetEquative(int id)
        {
            var sysagent = ct.SysAgent.Where(o => o.Id == id).FirstOrDefault();
            if (sysagent != null)
                return sysagent.MyUId;
            return 0;
        }

        //获取上级用户
        public List<SysAgentList> GetAllSysAgentList(Users s)
        {
            List<SysAgentList> sl = new List<SysAgentList>();
            try
            {
                var bsp = ct.BusinessShareProfit.Where(o => o.Type == 0).FirstOrDefault();
                var s4 = bsp.S1_4;
                var s5 = bsp.S1_5;
                var s6 = bsp.S1_6;
                var sv = bsp.S0_0;
                var sv1 = bsp.S1_1;
                if (s != null)
                {
                    //注销
                    #region 办卡人自己是vip并有分润
                    //if (s.IsVip == 1)
                    //{
                    //    SysAgentList sa = new SysAgentList();
                    //    sa.Id = s.Id;
                    //    sa.SRat = sv;
                    //    sa.Titer = 0;
                    //    sa.Equative = GetEquative(sa.Id, sa.Titer);
                    //    sl.Add(sa);
                    //    var s1 = ct.SysAgent.Where(o => o.Id == s.Agent).FirstOrDefault();
                    //    if (s1 != null)
                    //    {
                    //        sa = new SysAgentList();
                    //        sa.Id = s1.MyUId;
                    //        sa.SRat = GetSplit(s1.Tier);
                    //        sa.Titer = s1.Tier;
                    //        sa.Equative = GetEquative(sa.Id, sa.Titer);
                    //        sl.Add(sa);
                    //        var s2 = ct.SysAgent.Where(o => o.Id == s1.AgentID).FirstOrDefault();
                    //        if (s2 != null)
                    //        {
                    //            sa = new SysAgentList();
                    //            sa.Id = s2.MyUId;
                    //            sa.SRat = GetSplit(s2.Tier);
                    //            sa.Titer = s2.Tier;
                    //            sa.Equative = GetEquative(sa.Id, sa.Titer);
                    //            sl.Add(sa);
                    //            var s3 = ct.SysAgent.Where(o => o.Id == s2.AgentID).FirstOrDefault();
                    //            if (s3 != null)
                    //            {
                    //                sa = new SysAgentList();
                    //                sa.Id = s3.MyUId;
                    //                sa.SRat = GetSplit(s3.Tier);
                    //                sa.Titer = s3.Tier;
                    //                sa.Equative = GetEquative(sa.Id, sa.Titer);
                    //                sl.Add(sa);
                    //            }

                    //        }
                    //    }
                    //} 
                    #endregion
                    var pid = s.MyPId;
                    //推荐用户必须是Vip才有收益
                    var us = ct.Users.Where(o => o.Id == pid && o.IsVip == 1).FirstOrDefault();
                    if (us != null)
                    {
                        SysAgentList sa = new SysAgentList();
                        sa.Id = us.Id;
                        sa.SRat = sv;
                        sa.Titer = 0;
                        sa.Equative = 0;
                        sl.Add(sa);

                        var s1 = ct.SysAgent.Where(o => o.Id == us.Agent).FirstOrDefault();
                        if (s1 != null)
                        {
                            sa = new SysAgentList();
                            sa.Id = s1.MyUId;
                            sa.SRat = GetSplit(s1.Tier);
                            sa.Titer = s1.Tier;
                            sa.Equative = GetEquative(s1.SameAgent);
                            sl.Add(sa);
                            var s2 = ct.SysAgent.Where(o => o.Id == s1.AgentID).FirstOrDefault();
                            if (s2 != null)
                            {
                                sa = new SysAgentList();
                                sa.Id = s2.MyUId;
                                sa.SRat = GetSplit(s2.Tier);
                                sa.Titer = s2.Tier;
                                sa.Equative = GetEquative(s2.SameAgent);
                                sl.Add(sa);
                                var s3 = ct.SysAgent.Where(o => o.Id == s2.AgentID).FirstOrDefault();
                                if (s3 != null)
                                {
                                    sa = new SysAgentList();
                                    sa.Id = s3.MyUId;
                                    sa.SRat = GetSplit(s3.Tier);
                                    sa.Titer = s3.Tier;
                                    sa.Equative = GetEquative(s3.SameAgent);
                                    sl.Add(sa);
                                }
                            }
                        }
                    }
                }
                if (sl != null)
                {
                    sl[0].Rat = sl[0].SRat;
                    var num = sl.Count - 1;
                    //如果有下一级vip
                    if (num > 0)
                    {
                        sl[1].Rat = sl[1].SRat - sl[0].SRat;
                        num = num - 1;
                        //如果有下一级
                        if (num > 0)
                        {
                            sl[2].Rat = sl[2].SRat - sl[1].SRat;
                            num = num - 1;
                            if (num > 0)
                            {
                                sl[3].Rat = sl[3].SRat - sl[2].SRat;
                            }
                        }
                    }
                }
                return sl;
            }
            catch (Exception e)
            {
                Log.LoggerHelper.Error("获取上级用户失败，原因：" + e.Message);
                return sl;
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
        public string ShareProfit1(string id = "", decimal? money = 0)
        {
            //取出分润比例Type=0：信用卡
            var bsp = ct.BusinessShareProfit.Where(o => o.Type == 0).FirstOrDefault();
            var Id = int.Parse(id);
            //普通用户
            var user1 = ct.Users.Where(o => o.Id == Id).FirstOrDefault();
            if (user1 != null)
            {
                #region 生成订单
                OrdersPayOnly opo = new OrdersPayOnly();
                //生成订单号
                var Tnum = "FX" + MD5Helper.getMd5Hash(DateTime.Now.ToString());
                try
                {
                    //添加订单
                    opo.TNum = Tnum;
                    opo.AddTime = DateTime.Now;
                    opo.IsDel = 0;
                    ct.OrdersPayOnly.Add(opo);
                    ct.SaveChanges();
                }
                catch (Exception e)
                {
                    Log.LoggerHelper.Error("添加订单失败，原因：" + e.Message);
                }
                #endregion
                try
                {
                    List<SysAgentList> list = GetAllSysAgentList(user1);
                    if (list.Count() > 0)
                    {
                        foreach (SysAgentList i in list)
                        {
                            OrderProfitLog op = new OrderProfitLog();
                            //生成分润日志
                            op.AddTime = DateTime.Now;
                            op.IsDel = 0;
                            op.UId = i.Id;
                            op.TNum = Tnum;
                            op.UserName = user1.UserName;
                            op.OrderType = 22; /*(22：分销分润)*/
                            op.Amoney = decimal.Parse(money.ToString());
                            op.Profit = decimal.Parse((money * i.Rat).ToString());
                            ct.OrderProfitLog.Add(op);
                            var user = ct.Users.Where(o => o.Id == i.Id).FirstOrDefault();
                            user.Amount = user.Amount + op.Profit;
                            user.Sp_Amount = user.Sp_Amount + op.Profit;
                            //同级分润
                            if (i.Equative != 0)
                            {
                                var euser = ct.Users.Where(o => o.Id == i.Equative).FirstOrDefault();
                                op = new OrderProfitLog();
                                //生成分润日志
                                op.AddTime = DateTime.Now;
                                op.IsDel = 0;
                                op.UId = euser.Id;
                                op.TNum = Tnum;
                                op.UserName = user1.UserName;
                                op.OrderType = 22; /*(22：分销分润)*/
                                op.Amoney = decimal.Parse(money.ToString());
                                op.Profit = decimal.Parse((money * i.Rat * bsp.S1_1).ToString());
                                ct.OrderProfitLog.Add(op);
                                euser.Amount = euser.Amount + op.Profit;
                                euser.Sp_Amount = euser.Sp_Amount + op.Profit;
                            }
                            ct.SaveChanges();
                        }

                    }
                    else
                    {
                        return "该用户没有上级！";
                    }
                }
                catch (Exception e)
                {
                    Log.LoggerHelper.Error("添加分润失败，原因：" + e.Message);
                    return "分润失败！";
                }

            }
            return "分润成功！";
        }
        public string ShareProfit(string id = "", decimal? money = 0)
        {
            //取出分润比例Type=0：信用卡
            var bsp = ct.BusinessShareProfit.Where(o => o.Type == 0).FirstOrDefault();
            var Id = int.Parse(id);
            //普通用户
            var user1 = ct.Users.Where(o => o.Id == Id).FirstOrDefault();
            if (user1 != null)
            {
                #region 生成订单
                OrderProfitLog op = new OrderProfitLog();
                OrdersPayOnly opo = new OrdersPayOnly();
                var logType = "1";
                decimal? bspnum = 0;
                //生成订单号
                var Tnum = "FX" + MD5Helper.getMd5Hash(DateTime.Now.ToString());
                try
                {
                    //添加订单
                    opo.TNum = Tnum;
                    opo.AddTime = DateTime.Now;
                    opo.IsDel = 0;
                    ct.OrdersPayOnly.Add(opo);
                    ct.SaveChanges();
                }
                catch (Exception e)
                {
                    Log.LoggerHelper.Error("添加订单失败，原因：" + e);
                }
                #endregion
                //查找VIP
                var user = ct.Users.Where(o => o.Id == user1.MyPId && o.IsVip == 1).FirstOrDefault();
                if (user != null)
                {
                    GetAllSysAgentList(user);
                    //生成分润日志
                    op.AddTime = DateTime.Now;
                    op.IsDel = 0;
                    op.UId = user.Id;
                    op.TNum = Tnum;
                    op.UserName = user.UserName;
                    op.OrderType = 22; /*(22：分销分润)*/
                    op.Amoney = decimal.Parse(money.ToString());

                    #region VIP用户分润
                    try
                    {
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
                    }
                    catch (Exception e)
                    {
                        Log.LoggerHelper.Error("添加VIP用户分润日志失败，原因：" + e);
                    }
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
                            #region 分销商S1_4
                            try
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
                            catch (Exception e)
                            {
                                Log.LoggerHelper.Error("添加(S1_4)代理商一级分润日志失败，原因：" + e);
                            }
                            #endregion
                        }
                        else if (Agent.Tier == 5)
                        {
                            bspnum = bsp.S1_5;
                            //对代理商的账户进行操作
                            var S1_5 = ct.Users.Where(o => o.Id == Agent.MyUId).FirstOrDefault();
                            if (S1_5 != null)
                            {
                                #region 分销商S1_5
                                try
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
                                }
                                catch (Exception e)
                                {
                                    Log.LoggerHelper.Error("添加(S1_5)代理商一级分润日志失败，原因：" + e);
                                }
                                #endregion
                                //上级代理商
                                var upAgent = ct.SysAgent.Where(o => o.Id == Agent.AgentID).FirstOrDefault();
                                if (upAgent != null)
                                {
                                    //判断代理商的层级
                                    if (upAgent.Tier == 4)
                                    {
                                        #region 分销商S2_5_4
                                        try
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
                                        catch (Exception e)
                                        {
                                            Log.LoggerHelper.Error("添加(S2_5_4)代理商二级分润日志失败，原因：" + e);
                                        }
                                        #endregion
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
                                #region 分销商S1_6
                                try
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
                                }
                                catch (Exception e)
                                {
                                    Log.LoggerHelper.Error("添加(S1_6)分销商一级分润日志失败，原因：" + e);
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
                                            #region 分销商S2_6_4
                                            try
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
                                            catch (Exception e)
                                            {
                                                Log.LoggerHelper.Error("添加(S2_6_4)分销商二级分润日志失败，原因：" + e);
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
                                            #region 分销商S2_6_5
                                            try
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
                                            }
                                            catch (Exception e)
                                            {
                                                Log.LoggerHelper.Error("添加(S2_6_5)分销商二级分润日志失败，原因：" + e);
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
                                                    try
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
                                                    catch (Exception e)
                                                    {
                                                        Log.LoggerHelper.Error("添加(S3_6_5_4)分销商三级分润日志失败，原因：" + e);
                                                    }
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
            }

            return "分润成功！";
        }

    }
}
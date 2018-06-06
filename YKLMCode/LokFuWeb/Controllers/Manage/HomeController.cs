using LokFu.Base;
using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController() {
            ViewBag.Authorization = true;//允许权限
        }
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Error(string msg)
        {
            ViewBag.ErrorMsg = msg;
            return View();
        }
        public void Fast() {
            //string tnums = "20180103_03_0057906013,20180103_03_0057850673,20180103_03_0057845167,20180103_03_0057832815,20180103_03_0057823180,20180103_03_0057821015,20180103_03_0057808141,20180103_03_0057794687,20180103_03_0057764734,20180103_03_0057757442,20180103_03_0057752288,20180103_03_0057750306,20180103_03_0057735071,20180103_03_0057725155,20180103_03_0057696399,20180103_03_0057683588,20180103_03_0057665179,20180103_03_0057663455,20180103_03_0057661337,20180103_03_0057634023,20180103_03_0057626311,20180103_03_0057608868,20180103_03_0057598024,20180103_03_0057587764,20180103_03_0057579464,20180103_03_0057514907,20180103_03_0057508842,20180103_03_0057502180,20180103_03_0057499255,20180103_03_0057484029,20180103_03_0057441168,20180103_03_0057439971,20180103_03_0057376666,20180103_03_0057357966,20180103_03_0057351994,20180102_03_0057129896,20180102_03_0057125729,20180102_03_0057124570,20180102_03_0057108208,20180102_03_0057082885,20180102_03_0057025782,20180102_03_0057021853,20180102_03_0057020758,20180102_03_0056969606,20180102_03_0056965449,20180102_03_0056964313,20180102_03_0056956418,20180102_03_0056906120,20180102_03_0056900310,20180102_03_0056891668,20180102_03_0056888921,20180102_03_0056664079,20180102_03_0056559176,20180102_03_0056481986,20180102_03_0056481363,20180102_03_0056417352,20180102_03_0056412127,20180102_03_0056411459,20180102_03_0056386925,20180102_03_0056383728,20180102_03_0056343357,20180102_03_0056326845,20180102_03_0056324876,20180102_03_0056323956,20180102_03_0056320913,20180102_03_0056296968,20180102_03_0056291412,20180102_03_0056290817,20180102_03_0056290700,20180102_03_0056288255,20180102_03_0056286190,20180102_03_0056284170,20180102_03_0056284045,20180102_03_0056259654,20180102_03_0056243841,20180102_03_0056240355,20180102_03_0056238731,20180102_03_0056238328,20180102_03_0056237007,20180102_03_0056233483,20180102_03_0056231799,20180102_03_0056221146,20180102_03_0056218796,20180102_03_0056210798,20180102_03_0056195443,20180102_03_0056198028,20180102_03_0056192009,20180102_03_0056190143,20180102_03_0056179858,20171228_03_0052710018,20171228_03_0052702434,20171228_03_0052700773,20171228_03_0052697178,20171228_03_0052691379,20171228_03_0052628928,20171228_03_0052615436,20171228_03_0052596825,20171228_03_0052578664,20171228_03_0052519870,20171228_03_0052508433,20171228_03_0052504624,20171228_03_0052413220,20171228_03_0052393579,20171228_03_0052365030";
            //string[] tnumArr = tnums.Split(',');
            //foreach (var tnum in tnumArr)
            //{
            //    FastOrder FastOrder = Entity.FastOrder.FirstOrDefault(n => n.TNum == tnum);

            //    SysAgent SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == FastOrder.Agent);
            //    IList<SysAgent> SysAgentList = SysAgent.GetAgentsById(Entity);
            //    SysAgent TopAgent = SysAgentList.FirstOrNew(n => n.Tier == 1);

            //    FastOrder.UserRate = 0.003M;

            //    //用户手续费
            //    decimal Poundage = FastOrder.Amoney * FastOrder.UserRate + FastOrder.UserCash;
            //    Poundage = Poundage.Ceiling();
            //    FastOrder.Poundage = Poundage;
            //    //用户最终金额
            //    FastOrder.PayMoney = FastOrder.Amoney - FastOrder.Poundage;

            //    //计算手续费差
            //    decimal PayGet = FastOrder.Amoney * (FastOrder.UserRate - FastOrder.AgentRate);
            //    PayGet = PayGet.Floor();
            //    //一级代理利润
            //    decimal AgentPayGet = PayGet * (decimal)TopAgent.PayGet;
            //    AgentPayGet = AgentPayGet.Floor();
            //    FastOrder.AgentPayGet = AgentPayGet;
            //    string AgentPath = "|";
            //    string Split = "|";
            //    decimal MyGet = PayGet;
            //    foreach (var p in SysAgentList)
            //    {
            //        AgentPath += p.Id + "|";
            //        MyGet = MyGet * (decimal)p.PayGet;//各级代理分润
            //        MyGet = MyGet.Floor();
            //        Split += MyGet.ToString("F2") + "|";
            //    }
            //    FastOrder.AgentPath = AgentPath;
            //    FastOrder.Split = Split;

            //    decimal BankMoney = FastOrder.Amoney * FastOrder.SysRate;
            //    BankMoney = BankMoney.Floor();
            //    //用户手续费(含代付手续费)-代付分润-银行手续费-银行代付成本
            //    decimal HFGet = Poundage - AgentPayGet - BankMoney - FastOrder.UserCash;
            //    FastOrder.HFGet = HFGet;
            //    Entity.SaveChanges();
            //}
            //foreach (var tnum in tnumArr)
            //{
            //    IList<UserLog> List = Entity.UserLog.Where(n => n.OId == tnum).ToList();//按分润记录退佣金
            //    foreach (var p in List)
            //    {
            //        //帐户变动记录
            //        string SP_Ret = Entity.SP_UsersMoney(p.UId, tnum, p.Amount, 12, "分润异常退单", 0);
            //        if (SP_Ret != "3")
            //        {
            //            Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", p.UId, tnum, 12, p.Amount, SP_Ret), "SP_UsersMoney12");
            //        }
            //    }
            //    FastOrder FastOrder = Entity.FastOrder.FirstOrDefault(n => n.TNum == tnum);
            //    FastOrder.State = 0;
            //    FastOrder.PayState = 0;
            //    FastOrder.UserState = 0;
            //    FastOrder.AgentState = 0;
            //    FastOrder.Remark = "上游异常实际未支付";
            //    Entity.SaveChanges();
            //}
        }
        //public void ModUser() {
        //    IList<UserTrack> List = Entity.UserTrack.Where(n => n.GPSAddress.Contains("超级收银台")).ToList();
        //    foreach (var p in List) {
        //        string str = p.GPSAddress.Replace("超级收银台:", "");
        //        if (str.IndexOf("[") != -1)
        //        {
        //            string IP = str.Split('[')[1].Replace("]", "");
        //            string Address = str.Split('[')[0];
        //            p.IP = IP;
        //            p.IPAddress = Address;
        //        }
        //    }
        //    List = Entity.UserTrack.Where(n => n.GPSAddress.Contains("网店收银台，IP:") && n.GPSAddress.Contains("]")).ToList();
        //    foreach (var p in List)
        //    {
        //        string str = p.GPSAddress.Replace("网店收银台，IP:", "");
        //        if (str.IndexOf("[") != -1)
        //        {
        //            string IP = str.Split('[')[1].Replace("]", "");
        //            string Address = str.Split('[')[0];
        //            p.IP = IP;
        //            p.IPAddress = Address;
        //        }
        //    }
        //    Entity.SaveChanges();
        //}
        public void UpFile()
        {
            string str = Upload.ProcessRequest();
            Response.Write(str);
            return;
        }
        //初始化余额理材数据
        public void FormatBao() {
            //IList<BaoStory> BaoStoryList = Entity.BaoStory.OrderBy(n => n.SDate).ToList();
            //foreach (var p in BaoStoryList) {

            //    DateTime Today = p.SDate;
            //    DateTime EDate = Today.AddDays(1);

            //    DateTime sTime = Today.AddHours(-1);
            //    DateTime eTime = Today.AddHours(23);
                
            //    p.InMoney = 0;
            //    p.OutMoney = 0;
            //    p.BfAllMoney = 0;
            //    p.BfActMoney = 0;
            //    p.BfInMoney = 0;
            //    try
            //    {
            //        p.InMoney = Entity.BaoLog.Where(n => n.AddTime > sTime && n.AddTime <= eTime && n.LType == 1).Sum(n => n.Amount);
            //    }
            //    catch (Exception)
            //    {
            //        Utils.WriteLog("统计转入出错", "FormatBao_2016");
            //    }
            //    try
            //    {
            //        p.OutMoney = Entity.BaoLog.Where(n => n.AddTime > sTime && n.AddTime <= eTime && n.LType == 2).Sum(n => n.Amount);
            //    }
            //    catch (Exception)
            //    {
            //        Utils.WriteLog("统计转出金额出错", "FormatBao_2016");
            //    }
            //    try
            //    {
            //        p.BfActMoney = Entity.BaoLog.Where(n => n.AddTime >= Today && n.AddTime < EDate && n.LType == 3).Sum(n => n.BeforAmount);
            //    }
            //    catch (Exception)
            //    {
            //        Utils.WriteLog("统计计息金额出错", "FormatBao_2016");
            //    }
            //    try
            //    {
            //        p.BfInMoney = Entity.BaoLog.Where(n => n.AddTime >= Today && n.AddTime < EDate && n.LType == 3).Sum(n => n.BeforFrozen); ;
            //    }
            //    catch (Exception)
            //    {
            //        Utils.WriteLog("统计未计息金额出错", "FormatBao_2016");
            //    }
            //    try
            //    {
            //        p.BfAllMoney = p.BfActMoney + p.BfInMoney;
            //    }
            //    catch (Exception)
            //    {
            //        Utils.WriteLog("统计总金额出错", "FormatBao_2016");
            //    }
            //    try
            //    {
            //        p.Interest = Entity.BaoLog.Where(n => n.AddTime >= Today && n.AddTime < EDate && n.LType == 3).Sum(n => n.Amount);//利息
            //    }
            //    catch (Exception)
            //    {
            //        Utils.WriteLog("统计利息出错", "FormatBao_2016");
            //    }
            //    p.AfAllMoney = p.BfAllMoney + p.Interest;
            //    p.AfActMoney = p.BfActMoney + p.AfInMoney + p.Interest;
            //    p.AfInMoney = 0;
            //    Entity.SaveChanges();
            //}
        }

        public void Bao(DateTime Today)
        {
            //余额理财昨天收益归0
            //=============================================================================================
            Entity.ExecuteStoreCommand("Update BaoUsers Set LastRec=0 Where LastRec>0");
            //余额理财计息程序
            //=============================================================================================
            Response.Write("余额理财任务开始执行！");

            DateTime EDate = Today.AddDays(1);//今天24点

            int BaoTime = 23;

            DateTime sTime = Today.AddHours(BaoTime - 24);//计息前节点
            DateTime eTime = Today.AddHours(BaoTime);//计息后节点

            BaoStory BaoStory = Entity.BaoStory.FirstOrDefault(n => n.SDate == Today && n.LType == 1);
            if (BaoStory == null)
            {
                BaoConfig BaoConfig = Entity.BaoConfig.FirstOrDefault();
                //添加总日志，后期可形成曲线图
                BaoStory = new BaoStory();
                BaoStory.SDate = Today;
                BaoStory.GetCost = BaoConfig.GetCost;
                BaoStory.YearPer = BaoConfig.YearPer;

                BaoStory.InMoney = 0;
                BaoStory.OutMoney = 0;
                BaoStory.BfAllMoney = 0;
                BaoStory.BfActMoney = 0;
                BaoStory.BfInMoney = 0;
                BaoStory.LType = 1;
                try
                {
                    BaoStory.InMoney = Entity.BaoLog.Where(n => n.AddTime > sTime && n.AddTime <= eTime && n.LType == 1).Sum(n => n.Amount);
                }
                catch (Exception)
                {
                    Response.Write("统计转入出错");
                }
                try
                {
                    BaoStory.OutMoney = Entity.BaoLog.Where(n => n.AddTime > sTime && n.AddTime <= eTime && n.LType == 2).Sum(n => n.Amount);
                }
                catch (Exception)
                {
                    Response.Write("统计转出金额出错");
                }
                try
                {
                    BaoStory.BfAllMoney = Entity.BaoUsers.Sum(n => n.AllMoney);
                }
                catch (Exception)
                {
                    Response.Write("统计总金额出错");
                }
                try
                {
                    BaoStory.BfActMoney = Entity.BaoUsers.Sum(n => n.ActMoney);
                }
                catch (Exception)
                {
                    Response.Write("统计计息金额出错");
                }
                try
                {
                    BaoStory.BfInMoney = Entity.BaoUsers.Sum(n => n.InMoney);
                }
                catch (Exception)
                {
                    Response.Write("统计未计息金额出错");
                }
                Entity.BaoStory.AddObject(BaoStory);
                Entity.SaveChanges();
                IList<BaoUsers> BaoUsersList = Entity.BaoUsers.Where(n => n.InMoney > 0 || n.ActMoney > 0).ToList();
                foreach (var p in BaoUsersList)
                {
                    p.PayLIXI(BaoConfig, Entity);
                }
                try
                {
                    BaoStory.Interest = Entity.BaoLog.Where(n => n.AddTime >= Today && n.AddTime < EDate && n.LType == 3).Sum(n => n.Amount);//利息
                }
                catch (Exception)
                {
                    Response.Write("统计利息出错");
                }
                BaoStory.AfAllMoney = BaoStory.BfAllMoney + BaoStory.Interest;
                BaoStory.AfActMoney = BaoStory.BfActMoney + BaoStory.AfInMoney + BaoStory.Interest;
                BaoStory.AfInMoney = 0;
                Entity.SaveChanges();
                Response.Write("余额理财！执行数量：" + BaoUsersList.Count());
            }
            Response.Write("余额理财任务执行结束！");
            //=============================================================================================
        }
    }
}

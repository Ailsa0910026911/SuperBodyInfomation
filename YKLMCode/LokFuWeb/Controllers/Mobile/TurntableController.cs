using System;
using System.Collections.Generic;
using System.Linq;
using LokFu.Repositories;
using LokFu.Extensions;
using LokFu.Infrastructure;
using System.Web.Mvc;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using LokFu.Repositories.SqlServer;
namespace LokFu.Areas.Mobile.Controllers
{
    public class TurntableController : BaseController
    {
        public TurntableController() {
            bool IsLokFu = ViewBag.IsLokFu;
            //if (!IsLokFu)
            //{
            //    System.Web.HttpContext.Current.Response.Write("请下载好付钱包再参与活动");
            //    return;
            //}
        }
        public void Default()
        {
            Response.Redirect("index-3.html");
        }
        public ActionResult Index(int TId)
        {
            Turntable Turntable = Entity.Turntable.FirstOrDefault(n => n.Id == TId);
            #region 活动状态
            if (Turntable == null)
            {
                ViewBag.ErrorMsg = "活动不存在，请重试！";
                return View("Error");
            }
            if (Turntable.State == 2)
            {//活动结束
                ViewBag.ErrorMsg = "活动已结束，请留意我们更多活动！";
                return View("Error");
            }
            if (Turntable.State != 1)
            {//状态异常
                ViewBag.ErrorMsg = "活动异常，请重试！";
                return View("Error");
            }
            if (Turntable.STime > DateTime.Now)
            {//未开始
                ViewBag.Message = "活动尚未开始，请继续关注我们的活动！";
                return View("Error");
            }
            if (Turntable.ETime < DateTime.Now)
            {//已结束
                ViewBag.Message = "活动已结束，请留意我们更多活动！";
                return View("Error");
            }
            #endregion
            ViewBag.Turntable = Turntable;
            return View();
        }
        public ActionResult Run(int TId)
        {
            Turntable Turntable = Entity.Turntable.FirstOrDefault(n => n.Id == TId);
            #region 活动状态
            if (Turntable == null)
            {
                ViewBag.Message = "活动不存在，请重试！";
                return View("Error");
            }
            if (Turntable.State == 2)
            {//活动结束
                ViewBag.Message = "活动已结束，请留意我们更多活动！";
                return View("Error");
            }
            if (Turntable.State != 1)
            {//状态异常
                ViewBag.Message = "活动异常，请重试！";
                return View("Error");
            }
            if (Turntable.STime > DateTime.Now)
            {//未开始
                ViewBag.Message = "活动尚未开始，请继续关注我们的活动！";
                return View("Error");
            }
            if (Turntable.ETime < DateTime.Now)
            {//已结束
                ViewBag.Message = "活动已结束，请留意我们更多活动！";
                return View("Error");
            }
            #endregion
            IList<TurnProc> TurnProcList = Entity.TurnProc.Where(n => n.TId == TId).OrderBy(n => n.Id).ToList();
            if (BasicUsers.Id.IsNullOrEmpty()) {//未登录
                ViewBag.Message = "您的登录信息有误，请重新登录后再试";
                return View("Error");
            }
            TurnUsers TurnUsers = Entity.TurnUsers.FirstOrDefault(n => n.UId == BasicUsers.Id);
            if (TurnUsers == null) { //第一次抽
                TurnUsers = new TurnUsers();
                TurnUsers.UId = BasicUsers.Id;
                TurnUsers.Times = 1;
                TurnUsers.UsedTimes = 0;
                TurnUsers.InTimes = 0;
                TurnUsers.TId = TId;
                Entity.TurnUsers.AddObject(TurnUsers);
                Entity.SaveChanges();
            }
            ViewBag.TurnProcList = TurnProcList;
            ViewBag.Turntable = Turntable;
            ViewBag.TurnUsers = TurnUsers;
            IList<TurnLog> TurnLogList = Entity.TurnLog.Where(n => n.TId == TId && n.Amoney >= 8).OrderByDescending(n => n.Id).Take(5).ToList();
            ViewBag.TurnLogList = TurnLogList;
            return View();
        }
        public ActionResult My()
        {
            if (BasicUsers.Id.IsNullOrEmpty())
            {//未登录
                ViewBag.Message = "您的登录信息有误，请重新登录后再试";
                return View("Error");
            }
            IList<TurnLog> TurnLogList = Entity.TurnLog.Where(n => n.UId == BasicUsers.Id && n.TPId > 0).OrderByDescending(n => n.Id).ToList();
            if (TurnLogList.Count < 1)
            {//未登录
                ViewBag.MsgTitle = "还未中奖哟";
                ViewBag.BigTitle = "";
                ViewBag.SmallTitle = "亲，您还未中奖<br />继续努力吧~~~";
                ViewBag.Type = "type1";
                return View("None");
            }
            ViewBag.TurnLogList = TurnLogList;
            return View();
        }
        public ActionResult YaoQing()
        {
            if (BasicUsers.Id.IsNullOrEmpty())
            {//未登录
                ViewBag.Message = "您的登录信息有误，请重新登录后再试";
                return View("Error");
            }
            IList<Users> UsersList = Entity.Users.Where(n => n.MyPId == BasicUsers.Id && n.ShareType == 1 && n.CardStae == 2).OrderByDescending(n => n.Id).ToList();
            if (UsersList.Count < 1)
            {//未登录
                ViewBag.MsgTitle = "还未邀请好友";
                ViewBag.BigTitle = "";
                ViewBag.SmallTitle = "亲，您还未成功邀请好友<br />继续分享给你的朋友吧~~~";
                ViewBag.Type = "type2";
                return View("None");
            }
            ViewBag.UsersList = UsersList;
            return View();
        }
        public void Ajax(int TId)
        {
            try
            {
                System.Web.HttpContext.Current.Response.AddHeader("content-type", "application/json");
            }
            catch (Exception) { }
            Turntable Turntable = Entity.Turntable.FirstOrDefault(n => n.Id == TId);
            #region 活动状态
            if (Turntable == null) {
                Response.Write("{\"index\":\"-1\",\"state\":\"99\",\"times\":\"0\"}");
                return;
            }
            if (Turntable.State == 2)
            {//活动结束
                Response.Write("{\"index\":\"-1\",\"state\":\"3\",\"times\":\"0\"}");
                return;
            }
            if (Turntable.State != 1)
            {//状态异常
                Response.Write("{\"index\":\"-1\",\"state\":\"99\",\"times\":\"0\"}");
                return;
            }
            if (Turntable.STime > DateTime.Now)
            {//未开始
                Response.Write("{\"index\":\"-1\",\"state\":\"4\",\"times\":\"0\"}");
                return;
            }
            if (Turntable.ETime < DateTime.Now)
            {//已结束
                Response.Write("{\"index\":\"-1\",\"state\":\"3\",\"times\":\"0\"}");
                return;
            }
            #endregion
            if (BasicUsers.Id.IsNullOrEmpty())
            {//未登录
                Response.Write("{\"index\":\"-1\",\"state\":\"2\",\"times\":\"0\"}");
                return;
            }
            TurnUsers TurnUsers = Entity.TurnUsers.FirstOrDefault(n => n.UId == BasicUsers.Id);
            if (TurnUsers.Times == 0) {
                Response.Write("{\"index\":\"-1\",\"state\":\"1\",\"times\":\"0\"}");
                return;
            }
            //抽奖开始
            //获取奖品列表
            IList<TurnProc> List = Entity.TurnProc.Where(n => n.TId == TId).OrderBy(n => n.Id).ToList();
            int i = 1; int num = Turntable.BaseNum;
            int stnum = 0;
            foreach (var p in List)
            {//生成范围
                p.Index = i;
                num = num - (p.Num - p.UNum);
                p.StNum = stnum;//不含
                p.EnNum = stnum + (p.Num - p.UNum);//含
                stnum = p.EnNum;
                i++;
            }
            //添加谢谢参与项
            TurnProc Temp = new TurnProc();
            Temp.Id = 0;
            Temp.TId = 0;
            Temp.Name = "谢谢参与";
            Temp.Amoney = 0;
            Temp.Num = num;
            Temp.UNum = 0;
            Temp.Index = i;
            Temp.StNum = stnum;
            Temp.EnNum = stnum + num;
            List.Add(Temp);
            //生成随机数
            Random R = new Random();
            int Ran = R.Next(1, Turntable.BaseNum);
            //读取对应奖项
            TurnProc TurnProc = List.FirstOrDefault(n => n.StNum < Ran && n.EnNum >= Ran);
            if (TurnProc == null) {
                TurnProc = List.FirstOrNew(n => n.Id == 0);
            }
            //这里可以加一条规则，中奖超过多少次以后不再中奖~~
            TurnUsers.Times--;//抽奖次数减少
            TurnUsers.UsedTimes++;//抽奖资料增加
            if (TurnProc.Id == 0)
            {//未中奖处理 
            }
            else
            {//中奖处理 
                TurnUsers.InTimes++;//中奖资料增加
                TurnProc.UNum++;//奖品送出数量增加
            }
            //添加抽奖记录
            TurnLog TurnLog = new TurnLog();
            TurnLog.UId = BasicUsers.Id;
            TurnLog.TId = Turntable.Id;
            TurnLog.TPId = TurnProc.Id;
            TurnLog.Name = TurnProc.Name;
            TurnLog.Amoney = TurnProc.Amoney;
            TurnLog.Num = 1;
            TurnLog.State = 1;
            TurnLog.AddTime = DateTime.Now;
            Entity.TurnLog.AddObject(TurnLog);
            Entity.SaveChanges();
            Response.Write("{\"index\":\"" + TurnProc.Index + "\",\"state\":\"0\",\"times\":\"" + TurnUsers.Times + "\",\"log\":\"" + TurnLog.Id + "\"}");
        }
        public void Take(int LId) {
            if (BasicUsers.Id.IsNullOrEmpty())
            {//未登录
                Response.Write("2");
                return;
            }
            TurnLog TurnLog = Entity.TurnLog.FirstOrDefault(n => n.Id == LId);
            if (TurnLog == null)
            {
                Response.Write("2");
                return;
            }
            if (TurnLog.UId != BasicUsers.Id) {
                Response.Write("2");
                return;
            }
            if (TurnLog.State != 1)
            {
                Response.Write("1");
                return;
            }
            TurnLog.TakeTime = DateTime.Now;
            TurnLog.State = 2;
            //开始
            //获取用户信息
            if (BasicUsers != null)
            {
                //帐户变动记录
                int USERSID = BasicUsers.Id;
                string TNUM = TurnLog.Id.ToString();
                decimal PAYMONEY = TurnLog.Amoney;
                string SP_Ret = Entity.SP_UsersMoney(USERSID, TNUM, PAYMONEY, 7, "");
                if (SP_Ret != "3")
                {
                    Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", USERSID, TNUM, 7, PAYMONEY, SP_Ret), "SP_UsersMoney");
                }
            }
            //结束
            Response.Write("0");
        }
        public ActionResult Share(int UId) {
            Users Users = Entity.Users.FirstOrNew(n => n.Id == UId);
            ViewBag.Users = Users;
            return View();
        }
        public void GetCard(int UId)
        {
            string CardBg = "/template/card2.png";
            DateTime DT = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")).AddDays(8);
            Users Users = Entity.Users.FirstOrNew(n => n.Id == UId);
            string MyCard = Request.Cookies.GetMyCard();
            Card Card = new Card();
            bool NeedGet = false;
            if (!MyCard.IsNullOrEmpty())
            { //如果已经获取过卡片
                Card myC = Entity.Card.FirstOrNew(n => n.Code == MyCard);
                if (myC.Id.IsNullOrEmpty())
                { //卡片不存在
                    NeedGet = true;
                }
                else if (myC.State != 1)//已使用
                {
                    NeedGet = true;
                }
                else if (myC.PUId != Users.Id)//其它人员推广
                {
                    if (myC.AdminId == Users.AId && myC.AId == Users.Agent)//同代理商名下，直接更换推广人员
                    {
                        myC.PUId = Users.Id;
                        myC.ActTime = DT;
                        Entity.SaveChanges();
                        NeedGet = false;
                        Card = myC;
                    }
                    else
                    {//不同代理商，则需要重新获取卡片
                        NeedGet = true;
                    }
                }
                else if (myC.ActTime < DateTime.Now)
                { //过期了，更新日期
                    myC.ActTime = DT;
                    Entity.SaveChanges();
                    NeedGet = false;
                    Card = myC;
                }
                else
                {
                    NeedGet = false;
                    Card = myC;
                }
            }
            else {
                NeedGet = true;
            }
            if (NeedGet)//重新获取卡片
            {
                Card = Entity.Card.FirstOrNew(n => n.AId == Users.Agent && n.AdminId == Users.AId && n.Auto == 1 && (n.PUId == 0 || n.PUId == null) && n.State == 1);//获取同业务员名下卡片
                if (Card.Id.IsNullOrEmpty()) {
                    Card = Entity.Card.FirstOrNew(n => n.AId == Users.Agent && n.Auto == 1 && (n.PUId == 0 || n.PUId == null) && n.State == 1);//获取同代理商卡片
                }
                Card.PUId = Users.Id;
                Card.ActTime = DT;
                Entity.SaveChanges();
            }
            if (!Users.Id.IsNullOrEmpty())
            {
                SysAgent SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == Users.Agent && n.State == 1 && n.IsTeiPai == 1);
                if (!SysAgent.Id.IsNullOrEmpty())
                {
                    CardBg = Utils.ImageUrl("SysAgent/" + SysAgent.Id, SysAgent.CradBg);
                }
            } 
            if (Card.Id.IsNullOrEmpty()) { //未获取到卡片，直接返回空白卡片
                Response.Write(CardBg);
                return;
            }
            Bitmap bitmap;
            Graphics g;
            //加载图片
            string BackgroundImage = Server.MapPath(CardBg);
            string CardNum = Card.Code;
            string CardPwd = Card.PasWd;
            string CardDate = ((DateTime)Card.ActTime).ToString("yyyy.MM.dd");
            bool Shadow = true;//阴影
            Response.Cookies.SetMyCard(CardNum);
            //卡号+_+有效日期+_+UId
            //String newFileName = DateTime.Now.ToString("yyyyMMddHHmmss_ffff", DateTimeFormatInfo.InvariantInfo) + ".png";
            string newFileName = string.Format("{0}_{1}_{2}.png", CardNum, CardDate, Users.Id);
            string ResultImage = Server.MapPath("/UpLoadFiles/Cards") + "\\" + newFileName;
            if (!System.IO.File.Exists(ResultImage))//文件不存在，生成文件
            {
                bitmap = new Bitmap(Image.FromFile(BackgroundImage));
                g = Graphics.FromImage(bitmap);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.CompositingQuality = CompositingQuality.HighQuality;
                Font f = new Font("宋体", 18, FontStyle.Bold);//字体 字号 样式
                Brush b = new SolidBrush(Color.FromArgb(180, 50, 50, 05));//字体颜色
                Brush b2 = new SolidBrush(Color.FromArgb(80, 50, 50, 50));//阴影字体颜色
                StringFormat strFormat = new StringFormat();
                strFormat.Alignment = StringAlignment.Near;
                if (Shadow)
                {
                    g.DrawString(CardNum, f, b2, 102, 283);//位移
                    g.DrawString(CardPwd, f, b2, 102, 309);//位移
                }
                g.DrawString(CardNum, f, b, new PointF(100, 281), strFormat);//位置
                g.DrawString(CardPwd, f, b, new PointF(100, 306), strFormat);//位置
                Font f3 = new Font("宋体", 12, FontStyle.Regular);//有效期字体
                Brush b3 = new SolidBrush(Color.FromArgb(200, 254, 254, 254));//有效期颜色
                g.DrawString(CardDate, f3, b3, new PointF(100, 342), strFormat);//位置
                bitmap.Save(ResultImage, ImageFormat.Png);
                bitmap.Dispose();
                g.Dispose();
            }
            Response.Write("/UpLoadFiles/Cards/" + newFileName);
        }
    }
}

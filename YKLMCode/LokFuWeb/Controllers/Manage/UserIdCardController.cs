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
    public class UserIdCardController : BaseController
    {

        public ActionResult Index(Users Users, EFPagingInfo<Users> p, int IsFirst = 0)
        {
            if (IsFirst == 0)
            {
                PageOfItems<Users> UsersList1 = new PageOfItems<Users>(new List<Users>(), 0, 10, 0, new Hashtable());
                ViewBag.UsersList = UsersList1;
                ViewBag.Users = Users;
                ViewBag.Log = this.checkPower("Log");
                return View();
            }
            if (Users.CardStae.IsNullOrEmpty())
            {
                Users.CardStae = 1;
            }
            if (!Users.UserName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UserName.Contains(Users.UserName)); }
            if (!Users.TrueName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TrueName.Contains(Users.TrueName) || f.NeekName.Contains(Users.TrueName)); }
            if (!Users.Mobile.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Mobile.Contains(Users.Mobile)); }
            if (!Users.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == (Users.State == 99 ? 0 : Users.State)); }
            if (!Users.CardNum.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.CardNum.Contains(Users.CardNum)); }
            // if (!Users.CardStae.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.CardStae == (Users.CardStae == 99 ? 0 : Users.CardStae)); }
            p.SqlWhere.Add(f => f.CardStae > 0);//0未申请1申请2已审核3审核失败
            if (!Users.CardStae.IsNullOrEmpty())
            {
                if (Users.CardStae != 99)
                {
                    p.SqlWhere.Add(f => f.CardStae == Users.CardStae);
                }
            }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<Users> UsersList = Entity.Selects<Users>(p);
            ViewBag.UsersList = UsersList;
            ViewBag.Users = Users;
            ViewBag.Log = this.checkPower("Log");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(Users Users)
        {
            ViewBag.BasicDescList = GetBasicDescList(BasicCodeEnum.Shrz);

            if (Users.Id != 0) Users = Entity.Users.FirstOrDefault(n => n.Id == Users.Id);
            if (Users == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == Users.Agent);
            ViewBag.Users = Users;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Save(Users Users)
        {
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Id == Users.Id);
            baseUsers = Request.ConvertRequestToModel<Users>(baseUsers, Users);
            baseUsers.CardType = 0;

            //审核通过
            Card Card = null;
            if (!baseUsers.CardNum.IsNullOrEmpty())
            {
                Card = this.Entity.Card.Where(o => o.Code == baseUsers.CardNum && o.State == 3 && o.Auto == 1).FirstOrDefault();
            }
            if (baseUsers.CardStae == 2)
            {
                #region 推广人处理
                if (!baseUsers.MyPId.IsNullOrEmpty())
                {
                    #region 分享
                    if (baseUsers.ShareType == 2)
                    {
                        //支付通道升级推广
                        PayConfigChange PCC = Entity.PayConfigChange.FirstOrDefault(n => n.Id == baseUsers.PayConfigId && n.State == 1 && n.ShareNumber > 0);
                        if (PCC != null)
                        {
                            //仅统计降费率的人数
                            int count = Entity.Users.Count(n => n.CardStae == 2 && n.State == 1 && n.MyPId == baseUsers.MyPId && n.PayConfigId == baseUsers.PayConfigId && n.ShareType == 2);
                            count++;//这里统计会漏掉当前用户的，需要增加1个。
                            if (count == PCC.ShareNumber)
                            {//相等的那一次调整，避免超出了还一直调整
                                Users UP = Entity.Users.FirstOrDefault(n => n.Id == baseUsers.MyPId);//读取分享人，不能加ShareType
                                if (PCC.CState == 1)
                                {
                                    if (UP.Cash0 > PCC.Cash0)
                                    {
                                        UP.Cash0 = PCC.Cash0;
                                    }
                                    if (UP.ECash0 > PCC.ECash0)
                                    {
                                        UP.ECash0 = PCC.ECash0;
                                    }
                                }
                                if (PCC.EState == 1)
                                {
                                    if (UP.Cash1 > PCC.Cash1)
                                    {
                                        UP.Cash1 = PCC.Cash1;
                                    }
                                    if (UP.ECash1 > PCC.ECash1)
                                    {
                                        UP.ECash1 = PCC.ECash1;
                                    }
                                }
                                IList<UserPay> List = Entity.UserPay.Where(n => n.UId == UP.Id).ToList();//获取用户配置
                                foreach (var p in List)
                                {
                                    PayConfigTemp PCT = Entity.PayConfigTemp.FirstOrDefault(n => n.PId == p.PId && n.PCCId == PCC.Id);
                                    //获取套餐配置
                                    if (PCT != null)
                                    {
                                        if (PCT.State == 1)
                                        {
                                            if (p.Cost > PCT.Cost)
                                            {
                                                p.Cost = PCT.Cost;
                                            }
                                        }
                                    }
                                }
                                UP.ClearCache(this.Entity);
                            }
                        }

                    }
                    if (baseUsers.ShareType == 1)
                    {
                        //增加推广抽奖次数
                        TurnUsers TurnUsers = Entity.TurnUsers.FirstOrNew(n => n.UId == baseUsers.MyPId);
                        if (!TurnUsers.Id.IsNullOrEmpty())
                        {
                            TurnUsers.Times++;
                        }
                    }
                    #endregion

                    #region 分享统计
                    //===================================增加分享统计数===================================
                    //获取用户所属各级分润配置
                    IList<UserPromoteGet> UserPromoteGetList = Entity.UserPromoteGet.Where(n => n.AgentID == baseUsers.Agent).OrderBy(n => n.PromoteLevel).ToList();
                    int MaxLevel = UserPromoteGetList.Count();
                    //获取用户各级关系，最大级不超过用户配置级数。返回数据包含当前用户，当前用户级数标识Tier为0
                    IList<Users> UsersList = baseUsers.GetUsersById(Entity, MaxLevel);
                    foreach (var U in UsersList.Where(n => n.Tier > 0))
                    {
                        ShareTotal ShareTotal = Entity.ShareTotal.FirstOrDefault(n => n.UId == U.Id && n.Tier == U.Tier);
                        if (ShareTotal == null)
                        {
                            ShareTotal = new ShareTotal();
                            ShareTotal.UId = U.Id;
                            ShareTotal.AddTime = DateTime.Now;
                            ShareTotal.ShareNum = 1;
                            ShareTotal.Amount = 0;
                            ShareTotal.Profit = 0;
                            ShareTotal.Tier = U.Tier;
                            Entity.ShareTotal.AddObject(ShareTotal);
                        }
                        else
                        {
                            ShareTotal.ShareNum += 1;
                        }
                        Entity.SaveChanges();
                    }
                    //===================================增加分享统计数 End===================================
                    #endregion
                }
                #endregion

                #region 激活码处理
                if (Card != null)
                {
                    //代理商信息
                    baseUsers.Agent = Card.AId;
                    baseUsers.AId = Card.AdminId;
                    //推广信息
                    if (!Card.PUId.IsNullOrEmpty())
                    {
                        if (baseUsers.MyPId.IsNullOrEmpty())
                        {//不是费率升级推广才有效
                            baseUsers.MyPId = Card.PUId;
                            baseUsers.PayConfigId = 0;
                            baseUsers.ShareType = 1;
                        }
                    }
                    Card.State = 2;
                }
                #endregion
            }
            else//审核失败
            {
                baseUsers.CardNum = "";
                if (Card!=null)
                {
                    Card.State = 1;
                }
            }
            Entity.SaveChanges();
            baseUsers.PushMsg();//推送消息
            BaseRedirect();
        }

        public ActionResult Log(Users Users, EFPagingInfo<SysLog> p)
        {
            Users = Entity.Users.FirstOrNew(n => n.Id == Users.Id);
            ViewBag.Users = Users;
            if (Users.CardType == 0)
            {
                string uid = string.Format("Id={0}&", Users.Id);
                p.SqlWhere.Add(f => f.ControllerName == "UserIdCard");
                p.SqlWhere.Add(f => f.ActionName == "Save");
                p.SqlWhere.Add(f => f.POSTData.Contains(uid) || f.RQData.Contains(uid));
                p.OrderByList.Add("Id", "DESC");
                p.PageSize = 9999;
                IPageOfItems<SysLog> SysLogList = Entity.Selects<SysLog>(p);
                ViewBag.SysLogList = SysLogList;
            }
            if (Users.CardType == 1)
            {
                IList<UserAuth> UserAuthList = Entity.UserAuth.Where(n => n.UId == Users.Id).OrderByDescending(n => n.Id).ToList();
                ViewBag.UserAuthList = UserAuthList;
            }
            return View();
        }
    }
}

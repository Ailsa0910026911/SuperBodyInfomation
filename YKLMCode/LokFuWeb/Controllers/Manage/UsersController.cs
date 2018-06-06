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
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace LokFu.Areas.Manage.Controllers
{
    public class UsersController : BaseController
    {

        public ActionResult Index(Users Users, EFPagingInfo<Users> p, bool? IsShowSupAgent, int IsFirst = 0)
        {
            #region 条件
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }

            if (Users.CardStae.IsNullOrEmpty())
            {
                Users.CardStae = 2;
            }
            if (!Users.UserName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UserName == Users.UserName); }
            if (!Users.TrueName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TrueName.Contains(Users.TrueName) || f.NeekName.Contains(Users.TrueName)); }
            if (!Users.Mobile.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Mobile == Users.Mobile); }
            if (!Users.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == (Users.State == 99 ? 0 : Users.State)); }
            if (!Users.CardNum.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.CardNum == Users.CardNum); }
            if (!Users.Agent.IsNullOrEmpty())
            {
                //是否要显示下级
                if ((bool)IsShowSupAgent)
                {
                    SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == Users.Agent).FirstOrNew();
                    IList<SysAgent> SysAgentList = LowerLevelAgent.GetSupAgent(Entity, true);
                    IList<int> UID = SysAgentList.Select(o => o.Id).ToList();
                    p.SqlWhere.Add(f => UID.Contains(f.Agent));
                }
                else
                {
                    p.SqlWhere.Add(f => f.Agent == Users.Agent);
                }
            }
            if (!Users.CardId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.CardId == Users.CardId); }
            if (!Users.MyPId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.MyPId == Users.MyPId && f.ShareType == 1); }
            if (!Users.RegAddress.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.RegAddress.Contains(Users.RegAddress)); }
            if (!Users.CardStae.IsNullOrEmpty())
            {
                int CardStae = Users.CardStae;
                if (CardStae == 99)
                {
                    CardStae = 0;
                }
                if (CardStae != 88)
                {
                    p.SqlWhere.Add(f => f.CardStae == CardStae);
                }
            }
            if (!Users.HasT0.IsNullOrEmpty())
            {
                int HasT0 = Users.HasT0;
                if (HasT0 == 2)
                {
                    HasT0 = 0;
                }
                p.SqlWhere.Add(f => f.HasT0 == HasT0);
            }
            #endregion
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<Users> UsersList = null;
            if (IsFirst == 0)
            {

                UsersList = new PageOfItems<Users>(new List<Users>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                UsersList = Entity.Selects<Users>(p);
            }
            ViewBag.UsersList = UsersList;
            ViewBag.Users = Users;
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.State==1).ToList();
            //显示上级名字
            var ulids = UsersList.Select(o => o.Id).Distinct().ToArray();
            ViewBag.PName = Entity.Users.Select(o => new { o.Id, o.TrueName }).ToDictionary(o => o.Id, o => o.TrueName);
            ViewBag.IsShowSupAgent = IsShowSupAgent;//是否显示下级
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.DeductMoney = this.checkPower("DeductMoney");
            ViewBag.MyUsers = this.checkPower("MyUsers");
            ViewBag.Save = this.checkPower("Save");
            ViewBag.ClearPId = this.checkPower("ClearPId");
            ViewBag.StopPay = this.checkPower("StopPay");
            ViewBag.StopPaySave = this.checkPower("StopPaySave");
            ViewBag.UnFrozenList = this.checkPower("UnFrozenList");
            ViewBag.UnFrozenAudit = this.checkPower("UnFrozenAudit");
            ViewBag.Clear = this.checkPower("Clear");
            ViewBag.ClearRZ = this.checkPower("ClearRZ");
            ViewBag.UserTrail = this.checkPower("UserTrail", "Index");
            ViewBag.UserMaillist = this.checkPower("UserMaillist", "Index");
            ViewBag.UserIdCard = this.checkPower("UserIdCard", "Index");
            ViewBag.UserIdLog = this.checkPower("UserIdCard", "Log");
            ViewBag.UserLog = this.checkPower("UserLog", "Index");
            ViewBag.ChangeT0BlackList = this.checkPower("Users", "ChangeT0BlackList");
            ViewBag.ExcelInport = this.checkPower("ExcelInport");
            ViewBag.Download = this.checkPower("Download");
            return View();
        }
        public ActionResult Info(Users Users)
        {
            ViewBag.Remark = Users.Remark;
            if (Users.Id != 0) Users = Entity.Users.FirstOrDefault(n => n.Id == Users.Id);
            if (Users == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.Users = Users;
            //费率
            ViewBag.UserPayList = Entity.UserPay.Where(n => n.UId == Users.Id).ToList();
            ViewBag.PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
            ViewBag.SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == Users.Agent);
            return View();
        }
        public ActionResult Edit(Users Users)
        {
            ViewBag.Remark = Users.Remark;
            if (Users.Id != 0) Users = Entity.Users.FirstOrDefault(n => n.Id == Users.Id);
            if (Users == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.Users = Users;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public ActionResult Save(Users Users)
        {
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Id == Users.Id);
            if (baseUsers == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            if (Users.PassWord.IsNullOrEmpty())
                Users.PassWord = baseUsers.PassWord;
            else
                Users.PassWord = Users.PassWord.GetMD5();
            if (Users.Remark.IsNullOrEmpty())
            {
                Users.Remark = "无备注";
            }
            string State = "无改变";
            if (Users.State != baseUsers.State)
            {
                if (baseUsers.State == 1)
                {
                    State = "正常→锁定";
                }
                else
                {
                    State = "锁定→正常";
                }
            }
            string Remark = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "§" + Users.Remark + "§" + State + "§" + AdminUser.TrueName; ;
            if (baseUsers.Remark.IsNullOrEmpty())
            {
                Users.Remark = Remark;
            }
            else
            {
                Users.Remark = baseUsers.Remark + "№" + Remark;
            }
            baseUsers = Request.ConvertRequestToModel<Users>(baseUsers, Users);
            Entity.SaveChanges();
            BaseRedirect();
            return null;
        }
        [HttpGet]
        public ActionResult StopPaySave(int Id, string Remark)
        {
            ViewBag.Remark = Remark;
            Users users = new Users();
            if (Id != 0) users = Entity.Users.FirstOrDefault(n => n.Id == Id);
            if (users == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.Users = users;
            return View();
        }
        [HttpPost]
        public ActionResult StopPaySave(Users Users)
        {
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Id == Users.Id);
            if (baseUsers == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            if (Users.Remark.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请填写备注";
                return View("Error");
            }
            string State = "无改变";
            int OpType = 0;
            decimal StopPayMoney = 0m;

            #region 解止付
            if (Users.StopPayState == 0)//解
            {
                OpType = 2;
                var StopPayAudit = Entity.StopPayAudit.FirstOrDefault(o => o.UId == Users.Id && (o.TState == 1 || o.TState == 3));
                if (baseUsers.StopPayState == 0)
                {
                    ViewBag.ErrorMsg = "当前没有止付，无需解止付！";
                    return View("Error");
                }
                else if (baseUsers.StopPayState == 1)
                {
                    StopPayMoney = baseUsers.StopPayMoney;
                    if (StopPayMoney > 0)
                    {
                        //帐户变动记录
                        string SP_Ret = Entity.SP_UsersMoney(baseUsers.Id, "解除部分止付", StopPayMoney, 10, "解除部分止付");
                        if (SP_Ret != "3")
                        {
                            Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", baseUsers.Id, "解除部分止付", 10, StopPayMoney, SP_Ret), "SP_UsersMoney");
                        }
                        baseUsers = this.Entity.Users.FirstOrDefault(o => o.Id == baseUsers.Id);
                        baseUsers.StopPayMoney = 0;
                        baseUsers.StopPayState = 0;
                        baseUsers.StopPayAuditState = 2;
                    }
                    State = "部分止付→正常";
                }
                else if (baseUsers.StopPayState == 2)
                {
                    baseUsers.StopPayState = 0;
                    baseUsers.StopPayAuditState = 2;
                    State = "全止付→正常";
                }

                if (baseUsers.StopPayState == 0 && StopPayAudit != null)
                {
                    StopPayAudit.TState = 2;
                    StopPayAudit.AuditAdminId = this.AdminUser.Id;
                    StopPayAudit.AuditAdminName = this.AdminUser.TrueName;
                    StopPayAudit.AuditRemark = Users.Remark;
                    StopPayAudit.AuditInteriorRemark = Users.CardRemark;
                    StopPayAudit.AuditTime = DateTime.Now;
                }
            }
            #endregion
            #region 部分止付
            if (Users.StopPayState == 1)//部份
            {
                OpType = 3;
                StopPayMoney = Users.StopPayMoney;
                if (StopPayMoney.IsNullOrEmpty())
                {
                    ViewBag.ErrorMsg = "止付金额设置错误！";
                    return View("Error");
                }

                //帐户变动记录
                string SP_Ret = Entity.SP_UsersMoney(baseUsers.Id, "部分止付", StopPayMoney, 9, "部分止付");
                if (SP_Ret != "3")
                {
                    Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", baseUsers.Id, "部分止付", 9, StopPayMoney, SP_Ret), "SP_UsersMoney");
                }
                baseUsers = this.Entity.Users.FirstOrDefault(o => o.Id == baseUsers.Id);
                if (baseUsers.StopPayState == 0)
                {
                    baseUsers.StopPayMoney = StopPayMoney;
                    State = "正常→部分止付";
                }
                else if (baseUsers.StopPayState == 1)
                {
                    baseUsers.StopPayMoney += StopPayMoney;
                    State = "部分止付→部分止付";
                }
                else if (baseUsers.StopPayState == 2)
                {
                    baseUsers.StopPayMoney = StopPayMoney;
                    State = "全止付→部分止付";
                }
                baseUsers.StopPayState = 1;
                //baseUsers.Frozen += StopPayMoney;
                //baseUsers.Amount -= StopPayMoney;
            }
            #endregion
            #region 全止付
            if (Users.StopPayState == 2)//全帐户
            {
                OpType = 1;
                if (baseUsers.StopPayState == 0)
                {
                    baseUsers.StopPayState = 2;
                    baseUsers.AutoBao = 0;
                    baseUsers.AutoCash = 0;
                    State = "正常→全止付";
                }
                else if (baseUsers.StopPayState == 1)
                {
                    StopPayMoney = baseUsers.StopPayMoney;
                    if (StopPayMoney > 0)
                    {
                        //帐户变动记录
                        string SP_Ret = Entity.SP_UsersMoney(baseUsers.Id, "解除部分止付", StopPayMoney, 10, "解除部分止付");
                        if (SP_Ret != "3")
                        {
                            Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", baseUsers.Id, "解除部分止付", 10, StopPayMoney, SP_Ret), "SP_UsersMoney");
                        }
                        baseUsers = this.Entity.Users.FirstOrDefault(o => o.Id == baseUsers.Id);

                        baseUsers.StopPayState = 2;
                        baseUsers.AutoBao = 0;
                        baseUsers.AutoCash = 0;
                        baseUsers.StopPayMoney = 0;
                    }
                    State = "部分止付→全止付";
                }
                else if (baseUsers.StopPayState == 2)
                {
                    ViewBag.ErrorMsg = "当前状态无变动！";
                    return View("Error");
                }
            }
            #endregion

            var UserFrozenLog = new UserFrozenLog()
            {
                AddTime = DateTime.Now,
                OpName = AdminUser.TrueName,
                AId = AdminUser.Id,
                OpType = OpType,
                LogType = 2,
                UId = Users.Id,
                StopPayMoney = StopPayMoney,
                Remark = Users.Remark,
                Platform = 2,
                InteriorRemark = Users.CardRemark,
            };
            Entity.UserFrozenLog.AddObject(UserFrozenLog);
            Entity.SaveChanges();
            ViewBag.Msg = State;
            return View("Succeed");
        }

        public void ChangeStatus(Users Users, string InfoList)
        {
            if (InfoList.IsNullOrEmpty())
            {
                return;
            }
            string[] Ids = InfoList.Split(',');
            List<int> IDS = new List<int>();
            foreach (var p in Ids)
            {
                if (!p.IsNullOrEmpty())
                {
                    int id = int.Parse(p);
                    IDS.Add(id);
                }
            }
            if (Users.Remark.IsNullOrEmpty())
            {
                Users.Remark = "无备注";
            }
            IList<Users> UsersList = Entity.Users.Where(n => IDS.Contains(n.Id)).ToList();
            foreach (var p in UsersList)
            {
                string State = "无改变";
               
                    if (Users.State != p.State)
                    {
                        if (p.State == 1)
                        {
                            State = "正常→锁定";
                        }
                        else
                        {
                            State = "锁定→正常";
                        }
                    }
                    p.State = Users.State;
                
                string Remark = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "§" + Users.Remark + "§" + State + "§" + AdminUser.TrueName; ;
                if (p.Remark.IsNullOrEmpty())
                {
                    p.Remark = Remark;
                }
                else
                {
                    p.Remark = p.Remark + "№" + Remark;
                }
            }
            Entity.SaveChanges();
            Response.Write(1);
        }

        public void ChangeT0BlackList(Users Users, string InfoList)
        {
            if (InfoList.IsNullOrEmpty())
            {
                return;
            }
            string[] Ids = InfoList.Split(',');
            List<int> IDS = new List<int>();
            foreach (var p in Ids)
            {
                if (!p.IsNullOrEmpty())
                {
                    int id = int.Parse(p);
                    IDS.Add(id);
                }
            }
            if (Users.Remark.IsNullOrEmpty())
            {
                Users.Remark = "无备注";
            }
            IList<Users> UsersList = Entity.Users.Where(n => IDS.Contains(n.Id)).ToList();
            foreach (var p in UsersList)
            {
                string State = "无改变";
                if (!Users.HasT0.IsNullOrEmpty())
                {
                    if (Users.HasT0 != p.HasT0)
                    {
                        if (p.HasT0 == 1)
                        {
                            State = "白名单→黑名单";
                        }
                        else
                        {
                            State = "黑名单→白名单";
                        }
                    }
                    if (Users.HasT0 == 2)
                    {
                        p.HasT0 = 0;
                    }
                    if (Users.HasT0 == 1)
                    {
                        p.HasT0 = 1;
                    }
                }
                string Remark = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "§" + Users.Remark + "§" + State + "§" + AdminUser.TrueName; ;
                if (p.Remark.IsNullOrEmpty())
                {
                    p.Remark = Remark;
                }
                else
                {
                    p.Remark = p.Remark + "№" + Remark;
                }
            }
            Entity.SaveChanges();
            Response.Write(1);
        }

        public void Delete(Users Users, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = Users.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<Users>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void ClearPId(Users Users)
        {
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Id == Users.Id);
            if (baseUsers == null)
            {
                Response.Write("商户不存在");
                return;
            }
            baseUsers.MyPId = 0;
            Entity.SaveChanges();
            Response.Write("OK");
        }
        public void Clear(Users Users)
        {
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Id == Users.Id);
            if (baseUsers == null)
            {
                Response.Write("商户不存在");
                return;
            }
            baseUsers.MiBao = 0;
            Entity.SaveChanges();
            Response.Write("OK");
        }
        public void ClearRZ(Users Users)
        {
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Id == Users.Id);
            if (baseUsers == null)
            {
                Response.Write("商户不存在");
                return;
            }
            baseUsers.CardStae = 0;
            Entity.SaveChanges();
            Response.Write("OK");
        }

        public ActionResult MyUsers(Users Users, EFPagingInfo<Users> p, int IsFirst = 0)
        {
            if (IsFirst == 0)
            {
                PageOfItems<Users> UsersList1 = new PageOfItems<Users>(new List<Users>(), 0, 10, 0, new Hashtable());
                ViewBag.UsersList = UsersList1;
                ViewBag.Users = Users;
                return View();
            }
            if (!Users.UserName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UserName.Contains(Users.UserName)); }
            if (!Users.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == (Users.State == 99 ? 0 : Users.State)); }
            if (!Users.MyPId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.MyPId == Users.MyPId); }
            if (!Users.ShareType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.ShareType == Users.ShareType); }
            if (!Users.CardStae.IsNullOrEmpty())
            {
                int CardStae = Users.CardStae;
                if (CardStae == 99)
                {
                    CardStae = 0;
                }
                if (CardStae != 88)
                {
                    p.SqlWhere.Add(f => f.CardStae == CardStae);
                }
            }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<Users> UsersList = Entity.Selects<Users>(p);
            var pcids = UsersList.Select(o => o.PayConfigId).Distinct().ToArray();
            List<PayConfigChange> PayConfigChangeList = new List<PayConfigChange>();
            if (pcids.Length > 0)
            {
                string pcidsStr = string.Join(",", pcids);
                PayConfigChangeList = this.Entity.ExecuteStoreQuery<PayConfigChange>("SELECT * FROM PayConfigChange Where Id in(" + pcidsStr + ")").ToList();
            }
            ViewBag.PayConfigChangeList = PayConfigChangeList;
            ViewBag.UsersList = UsersList;
            ViewBag.Users = Users;
            //显示上级名字
            return View();
        }

        [HttpGet]
        public ActionResult DeductMoney(int id)
        {
            var Users = this.Entity.Users.FirstOrDefault(o => o.Id == id);
            if (Users == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.Users = Users;
            return View();
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult DeductMoney(DeductMoney DeductMoney)
        {
            if (DeductMoney.Remark.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请填写备注";
                return View("Error");
            }
            if (DeductMoney.Amoney < 0)
            {
                ViewBag.ErrorMsg = "请填写正确的金额";
                return View("Error");
            }
            //商户扣款
            var Users = this.Entity.Users.FirstOrDefault(o => o.Id == DeductMoney.UId);
            if (Users == null)
            {
                ViewBag.ErrorMsg = "用户不存在";
                return View("Error");
            }

            //交易审核信息
            DeductMoney.Amoney = DeductMoney.Amoney;
            DeductMoney.UserName = Users.TrueName;
            DeductMoney.AddTime = DateTime.Now;
            DeductMoney.CreateAdminId = this.AdminUser.Id;
            DeductMoney.CreateAdminName = this.AdminUser.TrueName;
            DeductMoney.TState = 1;
            Entity.DeductMoney.AddObject(DeductMoney);
            Entity.SaveChanges();
            ViewBag.Msg = "操作成功，请等待审核";
            return View("Succeed");
        }

        public ActionResult InfoAuto(Users Users)
        {
            Users = Entity.Users.FirstOrDefault(n => n.Id == Users.Id);
            ViewBag.Users = Users;
            return View();
        }
        /// <summary>
        /// 身份认证信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult IndexUserAuthentication(int id)
        {
            if (id == 0)
            {
                ViewBag.ErrorMsg = "参数错误";
                return View("Error");
            }
            Users Users = Entity.Users.FirstOrDefault(U => U.Id == id);
            this.ViewBag.Users = Users;
            return View();
        }
        /// <summary>
        /// 止付操作记录
        /// </summary>
        public ActionResult IndexUserFrozenLog(int id)
        {
            if (id == 0)
            {
                ViewBag.ErrorMsg = "参数错误";
                return View("Error");
            }
            List<UserFrozenLog> UserFrozenLog = this.Entity.UserFrozenLog.OrderByDescending(o => o.AddTime).Where(o => o.UId == id).ToList();
            this.ViewBag.UserFrozenLog = UserFrozenLog;
            return View();
        }

        /// <summary>
        /// 商户银卡行
        /// </summary>
        public ActionResult IndexUserCard(int id)
        {
            ViewBag.UserCardList = Entity.UserCard.Where(n => n.UId == id).OrderByDescending(o=>o.Id).ToList();
            Users Users=Entity.Users.FirstOrDefault(n => n.Id == id);
            if (Users == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.Users = Users;

            return View();
        }

        /// <summary>
        /// 绑定的二维码
        /// </summary>
        public ActionResult IndexQRCode(int id)
        {
            List<QRCode> QRCodeList = this.Entity.QRCode.Where(o => o.UId == id).OrderByDescending(o => o.Id).ToList();
            ViewBag.QRCodeList = QRCodeList;
            return View();
        }

        /// <summary>
        /// 调动记录
        /// </summary>
        public ActionResult IndexUsersMoveLog(int id)
        {
            List<UsersMoveLog> UsersMoveLogList = this.Entity.UsersMoveLog.Where(o => o.UId == id).OrderByDescending(o => o.Id).ToList();
            ViewBag.UsersMoveLogList = UsersMoveLogList;
            return View();
        }

        /// <summary>
        /// 商户通讯录
        /// </summary>
        public ActionResult IndexUserMaillist(int id)
        {
            List<UserMaillist> UserMaillist = this.Entity.UserMaillist.Where(o => o.UId == id).OrderByDescending(o => o.Id).ToList();
            ViewBag.UserMaillist = UserMaillist;
            return View();
        }
        /// <summary>
        /// 模板下载
        /// </summary>
        public void Download()
        {
            string tempname = "Users.xlsx";
            string file = Server.MapPath("/template") + "\\" + tempname;
            ExcelPackage package = new ExcelPackage(new FileInfo(file), true);
            var sheet = package.Workbook.Worksheets[1];
            Response.BinaryWrite(package.GetAsByteArray());//输出
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;  filename=商户止付模板.xlsx");
        }

        /// <summary>
        /// 上传
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditUpLoadUsers()
        {
            JsonResult json = new JsonResult();
            try
            {
                //上传
                var file = HttpContext.Request.Files.Get("UpLoadFile");
                var savePath = "/UpLoadFiles/Users/";
                var param = new UpLoadFileParam(file, savePath, false);
                param.AllowType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                json.Data = UpLoadFileHelpOld.UpLoadFileExcel(param);
                //var sjson = JsonConvert.SerializeObject(json.Data);
                //var djson = JsonConvert.DeserializeObject(sjson) as JObject;
                //JToken Result = djson["Result"];
                //string filename = Result["SaveFileName"].ToString();
                string filename = param.SaveFileName;
                //获取数据
                string path = Server.MapPath("//UpLoadFiles/Users") + "\\" + filename;
                ExcelPackage package = new ExcelPackage(new FileInfo(path), true);
                var sheet = package.Workbook.Worksheets[1];
                int rows = sheet.Dimension.End.Row;
                int OpType = 0;
                for (int i = 2; i < rows + 1; i++)
                {
                    Users baseUsers = new Users();
                    if (sheet.Cells[i,1].Value != null)
                    {
                        string mobile=sheet.Cells[i, 1].Value.ToString().Replace(" ", "");
                        baseUsers = Entity.Users.FirstOrNew(o => o.Mobile == mobile);
                    }
                    else if(sheet.Cells[i,2].Value!=null)
                    {
                        string cardid = sheet.Cells[i, 2].Value.ToString().Replace(" ", "");
                        baseUsers = Entity.Users.FirstOrNew(o => o.CardId == cardid && o.CardStae == 2);
                    }
                    if (!baseUsers.Id.IsNullOrEmpty()&&sheet.Cells[i,3].Value!=null)
                    {
                        decimal StopPayMoney = 0;
                        #region 部分止付
                        if (sheet.Cells[i, 3].Value.ToString() == "部分止付")//部份
                        {
                            OpType = 3;
                          
                            if (sheet.Cells[i, 4].Value != null)
                            {
                                StopPayMoney =Math.Round(Convert.ToDecimal(sheet.Cells[i, 4].Value),2);
                                if (StopPayMoney < 0)
                                { continue; }
                            }

                            //帐户变动记录
                            string SP_Ret = Entity.SP_UsersMoney(baseUsers.Id, "部分止付", StopPayMoney, 9, "部分止付");
                            if (SP_Ret != "3")
                            {
                                Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", baseUsers.Id, "部分止付", 9, StopPayMoney, SP_Ret), "SP_UsersMoney");
                            }
                            baseUsers = this.Entity.Users.FirstOrDefault(o => o.Id == baseUsers.Id);
                            if (baseUsers.StopPayState == 0)
                            {
                                baseUsers.StopPayMoney = StopPayMoney;
                            }
                            else if (baseUsers.StopPayState == 1)
                            {
                                baseUsers.StopPayMoney += StopPayMoney;
                            }
                            else if (baseUsers.StopPayState == 2)
                            {
                                baseUsers.StopPayMoney = StopPayMoney;
                            }
                            baseUsers.StopPayState = 1;
                            //日记写入
                            var UserFrozenLog = new UserFrozenLog()
                            {
                                AddTime = DateTime.Now,
                                OpName = AdminUser.TrueName,
                                AId = AdminUser.Id,
                                OpType = OpType,
                                LogType = 2,
                                UId = baseUsers.Id,
                                StopPayMoney = StopPayMoney,
                                Remark = sheet.Cells[i, 5].Value == null ? "" : sheet.Cells[i, 5].Value.ToString().Length > 1999 ? sheet.Cells[i, 5].Value.ToString().Substring(0, 1999) : sheet.Cells[i, 5].Value.ToString(),
                                Platform = 2,
                                InteriorRemark = sheet.Cells[i, 6].Value == null ? "" : sheet.Cells[i, 6].Value.ToString().Length > 1999 ? sheet.Cells[i, 6].Value.ToString().Substring(0, 1999) : sheet.Cells[i, 6].Value.ToString(),
                            };
                            Entity.UserFrozenLog.AddObject(UserFrozenLog);
                            //Entity.SaveChanges();
                        }
                        #endregion
                        #region 全止付
                        if (sheet.Cells[i, 3].Value.ToString() == "全止付")//全帐户
                        {
                            OpType = 1;
                            if (baseUsers.StopPayState == 0)
                            {
                                baseUsers.StopPayState = 2;
                                baseUsers.AutoBao = 0;
                                baseUsers.AutoCash = 0;
                            }
                            else if (baseUsers.StopPayState == 1)
                            {
                                StopPayMoney = baseUsers.StopPayMoney;
                                if (StopPayMoney > 0)
                                {
                                    //帐户变动记录
                                    string SP_Ret = Entity.SP_UsersMoney(baseUsers.Id, "解除部分止付", StopPayMoney, 10, "解除部分止付");
                                    if (SP_Ret != "3")
                                    {
                                        Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", baseUsers.Id, "解除部分止付", 10, StopPayMoney, SP_Ret), "SP_UsersMoney");
                                    }
                                    baseUsers = this.Entity.Users.FirstOrDefault(o => o.Id == baseUsers.Id);

                                    baseUsers.StopPayState = 2;
                                    baseUsers.AutoBao = 0;
                                    baseUsers.AutoCash = 0;
                                    baseUsers.StopPayMoney = 0;
                                }
                            }
                            else if (baseUsers.StopPayState == 2)
                            {
                                continue;
                            }
                            
                            //日记写入
                            var UserFrozenLog = new UserFrozenLog()
                            {
                                AddTime = DateTime.Now,
                                OpName = AdminUser.TrueName,
                                AId = AdminUser.Id,
                                OpType = OpType,
                                LogType = 2,
                                UId = baseUsers.Id,
                                StopPayMoney = StopPayMoney,
                                Remark = sheet.Cells[i, 5].Value == null ? "" : sheet.Cells[i, 5].Value.ToString().Length>1999?sheet.Cells[i, 5].Value.ToString().Substring(0,1999):sheet.Cells[i, 5].Value.ToString(),
                                Platform = 2,
                                InteriorRemark = sheet.Cells[i, 6].Value == null ? "" : sheet.Cells[i, 6].Value.ToString().Length > 1999 ? sheet.Cells[i, 6].Value.ToString().Substring(0, 1999) : sheet.Cells[i, 6].Value.ToString(),
                            };
                            Entity.UserFrozenLog.AddObject(UserFrozenLog);
                           
                        }
                        #endregion   
                    }
                }
                Entity.SaveChanges();
              return json;
            }
            catch
            {
                json = new JsonResult();
                return json;
            }
        }

    }
}

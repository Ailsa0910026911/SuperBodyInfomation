﻿using LokFu.Base;
using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class SysAgentController : BaseController
    {

        public ActionResult Index(SysAgent SysAgent, EFPagingInfo<SysAgent> p, int IsFirst = 0)
        {
            bool PayEdit = false;
            if (checkPower("PayEdit"))
            {
                PayEdit = true;
            }
            ViewBag.PayEdit = PayEdit;

            if (IsFirst == 0)
            {
                PageOfItems<SysAgent> SysAgentList1 = new PageOfItems<SysAgent>(new List<SysAgent>(), 0, 10, 0, new Hashtable());
                ViewBag.SysAgentList = SysAgentList1;
                ViewBag.SysAgent = SysAgent;
                ViewBag.Add = this.checkPower("Add");
                ViewBag.Info = this.checkPower("Info");
                ViewBag.Edit = this.checkPower("Edit");
                ViewBag.Save = this.checkPower("Save");
                ViewBag.Users = this.checkPower("Users", "Index");
                ViewBag.Orders = this.checkPower("Orders", "Index");
                return View();
            }
            if (!SysAgent.Name.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Name.Contains(SysAgent.Name)); }
            if (!SysAgent.LinkMobile.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.LinkMobile == SysAgent.LinkMobile); }
            if (!SysAgent.APPName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.APPName.Contains(SysAgent.APPName)); }
            if (!SysAgent.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == SysAgent.State); }
            if (!SysAgent.ExpireTime.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.ExpireTime == SysAgent.ExpireTime); }
            if (!SysAgent.Salesman.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Salesman.Contains(SysAgent.Salesman)); }
            if (!SysAgent.IsTeiPai.IsNullOrEmpty())
            {
                int IsTeiPai = SysAgent.IsTeiPai == 99 ? 0 : SysAgent.IsTeiPai;
                p.SqlWhere.Add(f => f.IsTeiPai == IsTeiPai);
            }
            //if (!SysAgent.Id.IsNullOrEmpty())
            //{
            //    p.SqlWhere.Add(f => f.AgentID == SysAgent.Id);
            //}
            //else
            //{
            //    p.SqlWhere.Add(o => o.AgentID == 0);
            //}
            if (!SysAgent.Tier.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.Tier == SysAgent.Tier);
            }
            
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<SysAgent> SysAgentList = Entity.Selects<SysAgent>(p);
            ViewBag.SysAgentList = SysAgentList;
            ViewBag.SysAgent = SysAgent;
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Info = this.checkPower("Info");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Save = this.checkPower("Save");
            ViewBag.Users = this.checkPower("Users", "Index");
            ViewBag.Orders = this.checkPower("Orders", "Index");
            return View();
        }
        public ActionResult Info(SysAgent SysAgent)
        {
            if (SysAgent.Id != 0) SysAgent = Entity.SysAgent.FirstOrDefault(n => n.Id == SysAgent.Id);
            if (SysAgent == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.SysAgent = SysAgent;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            SysAdmin sysAdmin = null;
            if (SysAgent.AdminId != 0)
            {
                sysAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == SysAgent.AdminId);
            }
            ViewBag.SysAdmin = sysAdmin;
            ViewBag.SysPowerList = Entity.SysPower.Where(n => n.PType == 2 && n.State == 1).OrderBy(n => n.Sort).ToList();
            var Contract = new List<Attach>();
            var Certificate = new List<Attach>();
            var IDCard_Front = new Attach();
            var IDCard_Hand = new Attach();
            var IDCard_Reverse = new Attach();
            if (SysAgent.Id != 0)
            {
                var attachs = Entity.Attach.Where(o => o.AgentId == SysAgent.Id && o.State == 1).ToList();
                Contract = attachs.Where(o => o.AType == (byte)AttachType.Contract).ToList();
                Certificate = attachs.Where(o => o.AType == (byte)AttachType.Certificate).ToList();
                IDCard_Front = attachs.FirstOrNew(o => o.AType == (byte)AttachType.IDCard_Front);
                IDCard_Hand = attachs.FirstOrNew(o => o.AType == (byte)AttachType.IDCard_Hand);
                IDCard_Reverse = attachs.FirstOrNew(o => o.AType == (byte)AttachType.IDCard_Reverse);
            }
            this.ViewBag.Contract = Contract;
            this.ViewBag.Certificate = Certificate;
            this.ViewBag.IDCard_Front = IDCard_Front;
            this.ViewBag.IDCard_Hand = IDCard_Hand;
            this.ViewBag.IDCard_Reverse = IDCard_Reverse;
            return View();
        }

        public ActionResult Edit(SysAgent SysAgent)
        {
            if (SysAgent.Id != 0) SysAgent = Entity.SysAgent.FirstOrDefault(n => n.Id == SysAgent.Id);
            if (SysAgent == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.SysAgent = SysAgent;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            SysAdmin sysAdmin = null;
            if (SysAgent.AdminId != 0)
            {
                sysAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == SysAgent.AdminId);
            }
            ViewBag.SysAdmin = sysAdmin;
            ViewBag.SysPowerList = Entity.SysPower.Where(n => n.PType == 2 && n.State == 1).OrderBy(n => n.Sort).ToList();
            var Contract = new List<Attach>();
            var Certificate = new List<Attach>();
            var IDCard_Front = new Attach();
            var IDCard_Hand = new Attach();
            var IDCard_Reverse = new Attach();
            if (SysAgent.Id != 0)
            {
                var attachs = Entity.Attach.Where(o => o.AgentId == SysAgent.Id && o.State == 1).ToList();
                Contract = attachs.Where(o => o.AType == (byte)AttachType.Contract).ToList();
                Certificate = attachs.Where(o => o.AType == (byte)AttachType.Certificate).ToList();
                IDCard_Front = attachs.FirstOrNew(o => o.AType == (byte)AttachType.IDCard_Front);
                IDCard_Hand = attachs.FirstOrNew(o => o.AType == (byte)AttachType.IDCard_Hand);
                IDCard_Reverse = attachs.FirstOrNew(o => o.AType == (byte)AttachType.IDCard_Reverse);
            }
            this.ViewBag.Contract = Contract;
            this.ViewBag.Certificate = Certificate;
            this.ViewBag.IDCard_Front = IDCard_Front;
            this.ViewBag.IDCard_Hand = IDCard_Hand;
            this.ViewBag.IDCard_Reverse = IDCard_Reverse;
            return View();
        }
        [ValidateInput(false)]
        public object Add(SysAgent SysAgent, List<string> PId, List<Attach> Contract, Attach IDCard_Front, Attach IDCard_Reverse, Attach IDCard_Hand, List<Attach> Certificate, List<int> DelIds)
        {
            //验证号码格式
            //var rx = new System.Text.RegularExpressions.Regex(@"^(13[0-9]|14[5|7]|15[0|1|2|3|5|6|7|8|9]|18[0|1|2|3|4|5|6|7|8|9])\d{8}$");
            //if (!rx.IsMatch(SysAgent.LinkMobile))
            //{
            //    ViewBag.ErrorMsg = "请正确填写联系手机号格式";
            //    return View("Error");
            //}
            //电信手机号码正则        
            string dianxin = @"^1[3578][01379]\d{8}$";
            Regex dReg = new Regex(dianxin);
            //联通手机号正则        
            string liantong = @"^1[34578][01256]\d{8}$";
            Regex tReg = new Regex(liantong);
            //移动手机号正则        
            string yidong = @"^(134[012345678]\d{7}|1[34578][012356789]\d{8})$";
            Regex yReg = new Regex(yidong);

            if (!dReg.IsMatch(SysAgent.LinkMobile) && !tReg.IsMatch(SysAgent.LinkMobile) && !yReg.IsMatch(SysAgent.LinkMobile))
            {
                ViewBag.ErrorMsg = "请正确填写联系手机号格式";
                return View("Error");
            }
            //验证是否重复
            SysAdmin Old = Entity.SysAdmin.FirstOrDefault(n => n.UserName == SysAgent.LinkMobile);
            if (Old != null)
            {
                ViewBag.ErrorMsg = "“联系手机号”已在系统中存在，无法开通管理员！";
                return View("Error");
            }
            if (Entity.UserBlackList.FirstOrDefault(UBL => UBL.CardNumber == SysAgent.LinkMobile && UBL.State == 1) != null)
            {
                ViewBag.ErrorMsg = "暂不支持您的手机号入网！";
                return View("Error");
            }
            if (Contract == null || Contract.Count <= 0)
            {
                ViewBag.ErrorMsg = "请上传合同";
                return View("Error");
            }
            if (IDCard_Front == null || IDCard_Front.AFile.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请上传身份证正面";
                return View("Error");
            }
            if (IDCard_Reverse == null || IDCard_Reverse.AFile.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请上传身份证反面";
                return View("Error");
            }
            if (IDCard_Hand == null || IDCard_Hand.AFile.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请上传手持身份证";
                return View("Error");
            }
            if (Certificate == null || Certificate.Count <= 0)
            {
                ViewBag.ErrorMsg = "请上传营业执照或其它证件";
                return View("Error");
            }
            //整理代理商信息
            SysAgent.AddTime = DateTime.Now;
            SysAgent.PayGet = SysAgent.PayGet.IsNullOrEmpty() ? 0 : SysAgent.PayGet / 100;
            SysAgent.CashGet = 0;// SysAgent.CashGet.IsNullOrEmpty() ? 0 : SysAgent.CashGet / 100;
            SysAgent.LoanGet = SysAgent.LoanGet.IsNullOrEmpty() ? 0 : SysAgent.LoanGet / 100;
            SysAgent.Credit = SysAgent.Credit.IsNullOrEmpty() ? 0 : SysAgent.Credit / 100;
            SysAgent.AdminId = 0;
            //SysAgent.AgentID = 0;//上级代理Id
            SysAgent.IsPromote = 1;//默认开启
            SysAgent.PromoteGet = 0;//默认为于推广用户返佣为0
            SysAgent.AgentLevelMax = 0;
            //SysAgent.Tier = 1;//系统开通出来的为第一级代理
            SysAgent.NoteDownload = "";
            SysAgent.AgentState = 0; //默认为显示好付

            SysAgent.Cash0 = SysAgent.Cash0 / 1000;
            SysAgent.Cash1 = SysAgent.Cash1 / 1000;

            //处理推广代理佣金
            if (SysAgent.DaiLiGetType == 1)
            {
                SysAgent.DaiLiGet = SysAgent.DaiLiGet / 100;
            }

            //处理设置
            if (SysAgent.IsTeiPai != 1)
            {
                SysAgent.Set3 = 0;
                SysAgent.Set4 = 0;
            }


            Entity.SysAgent.AddObject(SysAgent);
            //先保存拿agentId
            Entity.SaveChanges();
            //保存销售记录为了取得id
            var salesLog = new SalesLog()
            {
                AddTime = DateTime.Now,
                AgentFee = SysAgent.AgentFee,
                ExpireTime = SysAgent.ExpireTime,
                State = 1,
                AgentId = SysAgent.Id,
                Salesman = SysAgent.Salesman,
            };
            Entity.SalesLog.AddObject(salesLog);
            //再保存拿到logid
            Entity.SaveChanges();
            this.ProcessAttach(SysAgent, salesLog, Contract, IDCard_Front, IDCard_Reverse, IDCard_Hand, Certificate, DelIds);
            Entity.SaveChanges();
            //权限
            string Str = string.Empty;
            if (PId != null)
            {
                foreach (var p in PId)
                {
                    if (p != string.Empty)
                    {
                        Str += "," + p;
                    }
                }
                Str += ",";
            }

            //自动配置代理商的用户入网费率
            IList<PayConfig> PCList = Entity.PayConfig.Where(n => n.State == 1).ToList();
            foreach (var PC in PCList)
            {
                UserPayAgent UserPayAgent = new UserPayAgent();
                UserPayAgent.AId = SysAgent.Id;
                UserPayAgent.PId = PC.Id;
                UserPayAgent.Cost = (double)PC.CostUser;
                Entity.UserPayAgent.AddObject(UserPayAgent);
            }
            Entity.SaveChanges();

            //新添加这个里面会执行savecheng
            OpenOrBindUser(SysAgent.Id, Str);


            BaseRedirect();
            return true;
        }
        [ValidateInput(false)]
        public void Save(SysAgent SysAgent, List<string> PId, List<Attach> Contract, Attach IDCard_Front, Attach IDCard_Reverse, Attach IDCard_Hand, List<Attach> Certificate, List<int> DelIds, string PassWord, int? AnsyCash0, int? AnsyNext0, int? AnsyCash1, int? AnsyNext1, int sameid)
        {
            SysAgent.PayGet = SysAgent.PayGet.IsNullOrEmpty() ? 0 : SysAgent.PayGet / 100;
            SysAgent.CashGet = 0;//SysAgent.CashGet.IsNullOrEmpty() ? 0 : SysAgent.CashGet / 100;
            SysAgent.LoanGet = SysAgent.LoanGet.IsNullOrEmpty() ? 0 : SysAgent.LoanGet / 100;
            SysAgent.Credit = SysAgent.Credit.IsNullOrEmpty() ? 0 : SysAgent.Credit / 100;
            //已绑定，手机不能改
            SysAgent baseSysAgent = Entity.SysAgent.FirstOrDefault(n => n.Id == SysAgent.Id);
            if (!baseSysAgent.MyUId.IsNullOrEmpty())
            {
                SysAgent.LinkMobile = baseSysAgent.LinkMobile;
            }
            bool IsLock = false;
            if (baseSysAgent.State == 1 && SysAgent.State == 0)
            {
                IsLock = true;
            }
            //保存销售记录为了取得id
            var salesLog = new SalesLog()
            {
                AddTime = DateTime.Now,
                AgentFee = SysAgent.AgentFee,
                ExpireTime = SysAgent.ExpireTime,
                State = 1,
                AgentId = SysAgent.Id,
                Salesman = SysAgent.Salesman,
            };
            Entity.SalesLog.AddObject(salesLog);
            Entity.SaveChanges();

            SysAgent.Cash0 = SysAgent.Cash0 / 1000;
            SysAgent.Cash1 = SysAgent.Cash1 / 1000;

            //判断是否更新级数
            bool IsUpMaxTier = false;
            if (SysAgent.AgentLevelMax > baseSysAgent.AgentLevelMax)
            {
                IsUpMaxTier = true;
            }
            else {
                SysAgent.AgentLevelMax = baseSysAgent.AgentLevelMax;//不大于的情况下不能修改
            }
            baseSysAgent = Request.ConvertRequestToModel<SysAgent>(baseSysAgent, SysAgent);
            //处理推广代理佣金
            if (baseSysAgent.DaiLiGetType == 1)
            {
                baseSysAgent.DaiLiGet = baseSysAgent.DaiLiGet / 100;
            }

            //默认代理不能被关闭
            if (baseSysAgent.State == 0 && baseSysAgent.Id == 1)
            {
                baseSysAgent.State = 1;
            }

            //处理设置
            if (SysAgent.IsTeiPai != 1)
            {
                SysAgent.Set3 = 0;
                SysAgent.Set4 = 0;
            }

            //权限
            string Str = string.Empty;
            if (PId != null)
            {
                foreach (var p in PId)
                {
                    if (p != string.Empty)
                    {
                        Str += "," + p;
                    }
                }
                Str += ",";
            }
            string AgentIds = baseSysAgent.Id.ToString();//用户下方批量更新下级代理
            IList<SysAgent> AgentList = null;
            //取得之前的权限
            string powerChar = Entity.SysAdmin.FirstOrDefault(s => s.Id == baseSysAgent.AdminId).PowerID;
            //取得之前的权限跟修改后的权限不同的权限
            string[] strSpile = Str.Split(',');
            string[] powerSpile = powerChar.Split(',');
            strSpile = strSpile.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            powerSpile = powerSpile.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            //只有减少权限的时候才执行
            if (strSpile.Length < powerSpile.Length)
            {
                string retChar = "";

                for (int i = 0; i < powerSpile.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(powerSpile[i]))
                    {
                        if (!strSpile.Contains(powerSpile[i]))
                        {
                            retChar += powerSpile[i] + ",";
                        }
                    }
                }

                //取得代理商所有的下级
                AgentList = SysAgent.GetSupAgent(Entity);
                
                foreach (var agentItem in AgentList)
                {
                    //根据商户Id获取管理员信息
                    IList<SysAdmin> SysAdminList = Entity.SysAdmin.Where(x => x.AgentId == agentItem.Id).ToList();
                    string[] retSplit = retChar.Split(',');
                    foreach (var modelAdmin in SysAdminList)
                    {
                        foreach (var item in retSplit)
                        {
                            string formatChar = string.Format(",{0},", item);
                            if (modelAdmin.PowerID.Contains(formatChar))
                            {
                                modelAdmin.PowerID = modelAdmin.PowerID.Replace(formatChar, ",");
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(modelAdmin.PowerID) && !modelAdmin.PowerID.StartsWith(","))
                        {
                            modelAdmin.PowerID = "," + modelAdmin.PowerID;
                        }
                        string SQL = "Update SysAdmin set PowerID = '" + modelAdmin.PowerID + "' where Id=" + modelAdmin.Id;
                        Entity.ExecuteStoreCommand(SQL);
                    }
                }
            }

            SysAdmin SysAdmin = Entity.SysAdmin.FirstOrDefault(n => n.Id == baseSysAgent.AdminId);
            if (SysAdmin != null)
            {
                SysAdmin.PowerID = Str;
                if (!PassWord.IsNullOrEmpty())
                {
                    SysAdmin.PassWord = PassWord.GetAdminMD5();
                }
                SysAdmin.State = SysAgent.State;
            }
            this.ProcessAttach(SysAgent, salesLog, Contract, IDCard_Front, IDCard_Reverse, IDCard_Hand, Certificate, DelIds);
            Entity.SaveChanges();
            //如果是修改的话这个应该不会执行
            OpenOrBindUser(SysAgent.Id, Str);
            if (IsLock)
            {
                //锁定其下级所有代理 2016-07-05 Lin
                this.LockAllAgent(baseSysAgent);
            }
            Entity.SaveChanges();

            //if (SysAgent)

            if (!AnsyCash0.IsNullOrEmpty() || !AnsyCash1.IsNullOrEmpty() || !AnsyNext0.IsNullOrEmpty() || !AnsyNext1.IsNullOrEmpty() || IsUpMaxTier)
            {
                if (AgentList == null)
                {
                    //取得代理商所有的下级
                    AgentList = SysAgent.GetSupAgent(Entity, true);
                }
                foreach (var p in AgentList)
                {
                    AgentIds = AgentIds + "," + p.Id;
                }
            }
            if (IsUpMaxTier)
            {
                string SQL = "Update SysAgent Set AgentLevelMax=" + baseSysAgent.AgentLevelMax + " where Id in(" + AgentIds + ")";
                Entity.ExecuteStoreCommand(SQL);
            }
            if (AnsyCash0 == 1)//同步到用户
            {
                string SQL = "Update Users Set ECash0=" + baseSysAgent.ECash0 + ",Cash0=" + baseSysAgent.Cash0 + " where Agent in(" + AgentIds + ")";
                Entity.ExecuteStoreCommand(SQL);
            }
            if (AnsyCash1 == 1)//同步到用户
            {
                string SQL = "Update Users Set ECash1=" + baseSysAgent.ECash1 + ",Cash1=" + baseSysAgent.Cash1 + " where Agent in(" + AgentIds + ")";
                Entity.ExecuteStoreCommand(SQL);
            }
            if (AnsyNext0 == 1)//同步到代理
            {
                string SQL = "Update SysAgent Set ECash0=" + baseSysAgent.ECash0 + ",Cash0=" + baseSysAgent.Cash0 + ",Cash0Times=" + baseSysAgent.Cash0Times + " where Id in(" + AgentIds + ")";
                Entity.ExecuteStoreCommand(SQL);
            }
            if (AnsyNext0 == 1)//同步到代理
            {
                string SQL = "Update SysAgent Set ECash1=" + baseSysAgent.ECash1 + ",Cash1=" + baseSysAgent.Cash1 + ",Cash1Times=" + baseSysAgent.Cash1Times + " where Id in(" + AgentIds + ")";
                Entity.ExecuteStoreCommand(SQL);
            }
            if (sameid != SysAgent.SameAgent)
            {
                SysAgent tempAgent = Entity.SysAgent.FirstOrNew(o => o.Id == SysAgent.SameAgent);//调入商户
                SysAgent Agengt = Entity.SysAgent.FirstOrNew(o => o.Id == sameid);//调出商户
                UsersMoveLog UsersMoveLog = new UsersMoveLog()
                {
                    AddTime = DateTime.Now,
                    ToSAId = tempAgent.Id,
                    ToName = tempAgent.Name.IsNullOrEmpty()?"":tempAgent.Name,
                    FromName = Agengt.Name.IsNullOrEmpty()?"":Agengt.Name,
                    FromSAId = Agengt.Id,
                    UId = baseSysAgent.Id,
                    UTrueName = baseSysAgent.Name.IsNullOrEmpty() ? "" : baseSysAgent.Name,
                    OpName = AdminUser.TrueName,
                    Type = 3,
                    Tel=baseSysAgent.LinkMobile,
                };
                this.Entity.UsersMoveLog.AddObject(UsersMoveLog);
                Entity.SaveChanges();
            }

            BaseRedirect();
        }
        public void OpenOrBindUser(int id, string PowerID = "")
        {
            SysAgent SysAgent = Entity.SysAgent.FirstOrDefault(n => n.Id == id);
            if (!SysAgent.MyUId.IsNullOrEmpty())
            {
                //已绑定
                return;
            }
            SysAdmin SysAdmin = new SysAdmin();
            SysSet ss = new SysSet();
            ss.SMSEnd = BasicSet.SMSEnd;
            SysAgent SA = SysAgent.GetTopAgent(Entity);
            if (SA.IsTeiPai == 1)
            {
                if (!SA.MsgExt.IsNullOrEmpty())
                {
                    ss.SMSEnd = SA.MsgExt;
                }
            }
            if (SysAgent.AdminId.IsNullOrEmpty())
            {
                //未绑定管理员
                SysAdmin = Entity.SysAdmin.Where(n => n.UserName == SysAgent.LinkMobile).OrderBy(n => n.Id).FirstOrDefault();
                if (SysAdmin == null)
                {
                    //获取是否开通过管理员
                    SysAdmin = Entity.SysAdmin.Where(n => n.AgentId == SysAgent.Id).FirstOrDefault();
                    if (SysAdmin == null)
                    {
                        //开通管理员帐号
                        SysAdmin = new SysAdmin();
                        SysAdmin.UserName = SysAgent.LinkMobile;
                        SysAdmin.TrueName = SysAgent.Linker;
                        SysAdmin.Mobile = SysAgent.LinkMobile;
                        SysAdmin.Email = "";
                        SysAdmin.State = SysAgent.State;
                        SysAdmin.LoginTimes = 0;
                        SysAdmin.AddTime = DateTime.Now;
                        SysAdmin.AgentId = SysAgent.Id;
                        SysAdmin.PowerID = PowerID;
                        string PassWord = Utils.GetCode();
                        SysAdmin.PassWord = PassWord.GetAdminMD5();
                        Entity.SysAdmin.AddObject(SysAdmin);
                        Entity.SaveChanges();
                        SysAgent.AdminId = SysAdmin.Id;
                        Entity.SaveChanges();
                        if (SysAgent.AdminId.IsNullOrEmpty())
                        { //绑定第一个管理员
                            SysAgent.AdminId = SysAdmin.Id;
                        }
                        Entity.SaveChanges();
                        //开通后发送短信
                        string SendText = "您的服务商管理账户已经开通，帐号：{0}，密码：{1}。";
                        SendText = string.Format(SendText, SysAdmin.UserName, PassWord);
                        this.SendSMS(SendText, SysAdmin.Mobile, ss);
                    }
                }
                if (SysAdmin.AgentId.IsNullOrEmpty())
                {
                    //所属手机号是管理员
                    return;
                }
                if (SysAdmin.AgentId != SysAgent.Id)
                {
                    //手机号所属于管理员不属于本代理商
                    return;
                }
                SysAgent.AdminId = SysAdmin.Id;
                Entity.SaveChanges();
            }
            Users Users = Entity.Users.FirstOrDefault(n => n.UserName == SysAgent.LinkMobile);
            if (Users == null)
            {
                //注册用户
                Users = new Users();
                Users.UserName = SysAgent.LinkMobile;
                Users.Mobile = SysAgent.LinkMobile;
                string PassWord = Utils.GetCode();
                Users.PassWord = PassWord.GetMD5();
                Users.RegAddress = "后台自动开通";
                Users.X = "0";
                Users.Y = "0";
                Users.MobileState = 2;
                Users.EmailState = 0;
                Users.CardStae = 0;
                Users.State = 0;
                Users.Amount = 0;
                Users.Frozen = 0;
                Users.AddTime = DateTime.Now;
                Users.PayPwd = "";
                Users.Agent = SysAgent.Id;//默认指定
                Users.SAId = SysAgent.Id;
                //Users.SALevel = SysAgent.Levels;
                Users.AId = SysAdmin.Id;//默认指定
                Entity.Users.AddObject(Users);
                Entity.SaveChanges();
                //=======================================
                UserTrack UserTrack = new UserTrack();
                UserTrack.ENo = string.Empty;
                UserTrack.OPType = "一级代理开通";
                UserTrack.IfYY = string.Empty;
                UserTrack.EqMobile = string.Empty;
                UserTrack.SysVer = string.Empty;
                UserTrack.SoftVer = string.Empty;
                UserTrack.SignalType = string.Empty;
                UserTrack.GPSAddress = string.Empty;
                UserTrack.GPSX = "0";
                UserTrack.GPSY = "0";
                Users.SeavGPSLog(UserTrack, Entity);
                //=======================================
                SysAgent.MyUId = Users.Id;
                Entity.SaveChanges();

                //自动开通
                //IList<PayConfig> PCList = Entity.PayConfig.Where(n => n.State == 1).ToList();
                //foreach (var PC in PCList)
                //{
                //    UserPay UserPay = new UserPay();
                //    UserPay.UId = Users.Id;
                //    UserPay.PId = PC.Id;
                //    UserPay.Cost = (double)PC.CostUser;
                //    Entity.UserPay.AddObject(UserPay);
                //}

                //使用代理配置
                IList<UserPayAgent> UPAList = Entity.UserPayAgent.Where(n => n.AId == SysAgent.Id).OrderBy(n => n.PId).ToList();
                foreach (var p in UPAList)
                {
                    UserPay UserPay = new UserPay();
                    UserPay.UId = Users.Id;
                    UserPay.PId = p.PId;
                    UserPay.Cost = p.Cost;
                    Entity.UserPay.AddObject(UserPay);
                }

                //SysSet Sys = Entity.SysSet.FirstOrDefault();
                //Users.Cash0 = Sys.Cash0;
                //Users.ECash0 = Sys.ECash0;
                //Users.Cash1 = Sys.Cash1;
                //Users.ECash1 = Sys.ECash1;

                //使用代理配置
                Users.Cash0 = SysAgent.Cash0;
                Users.Cash1 = SysAgent.Cash1;
                Users.ECash0 = SysAgent.ECash0;
                Users.ECash1 = SysAgent.ECash1;

                Users.State = 1;
                Entity.SaveChanges();
                //自动开通End
                //开通后发送短信
                string SendText = "您的钱包账户已经开通，帐号：{0}，密码：{1}。";
                SendText = string.Format(SendText, Users.UserName, PassWord);
                this.SendSMS(SendText, Users.UserName, ss);
            }
            else
            {
                SysAgent.MyUId = Users.Id;
                //钱包帐户所属于代理商改成自己，避免推广用户属于原代理商
                Users.Agent = SysAgent.Id;//默认指定
                Users.SAId = SysAgent.Id;
                Users.AId = SysAdmin.Id;//默认指定
                Users.MyPId = 0;
                Entity.SaveChanges();
                string SendText = "您的钱包账户“{0}”，绑定成为{1}的结算帐户。";
                SendText = string.Format(SendText, Users.UserName, SysAgent.Name);
                this.SendSMS(SendText, Users.UserName, ss);
            }
        }
        private void SendSMS(string SendText, string Mobile, SysSet SS)
        {
            SMSLog SMSLog = new SMSLog();
            SMSLog.SendText = SendText;
            SMSLog.Mobile = Mobile;
            SMSLog.SendSMS(SS, Entity);
        }
        /// <summary>
        /// 上传
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditUpLoadAttach()
        {
            var file = HttpContext.Request.Files.Get("UpLoadFile");
            JsonResult json = new JsonResult() { ContentType = "text/html" };
            string allowType = "image/gif,image/png,image/jpeg";
            var savePath = "/UpLoadFiles/Attach/";
            var param = new UpLoadFileParam(file, savePath, false)
            {
                AllowType = allowType,
            };
            json.Data = UpLoadFileHelpOld.UpLoadFile(param);
            return json;
        }
        /// <summary>
        /// 处理附件
        /// </summary>
        /// <param name="Contract">合同</param>
        /// <param name="IDCard_Front">身份证-正面</param>
        /// <param name="IDCard_Reverse">身份证-反面</param>
        /// <param name="IDCard_Hand">身份证-手持</param>
        /// <param name="Certificate">营业执照或其它证件</param>
        private void ProcessAttach(SysAgent SysAgent, SalesLog salesLog, List<Attach> Contract, Attach IDCard_Front, Attach IDCard_Reverse, Attach IDCard_Hand, List<Attach> Certificate, List<int> DelIds)
        {
            #region 处理附件
            if (IDCard_Front != null)
            {
                IDCard_Front.State = 1;
                this.SaveOrAdd(IDCard_Front, AttachType.IDCard_Front, SysAgent.Id, salesLog.Id);
            }
            if (IDCard_Hand != null)
            {
                IDCard_Hand.State = 1;
                this.SaveOrAdd(IDCard_Hand, AttachType.IDCard_Hand, SysAgent.Id, salesLog.Id);
            }
            if (IDCard_Reverse != null)
            {
                IDCard_Reverse.State = 1;
                this.SaveOrAdd(IDCard_Reverse, AttachType.IDCard_Reverse, SysAgent.Id, salesLog.Id);
            }
            if (Contract != null && Contract.Count > 0)
            {
                foreach (var item in Contract)
                {
                    item.State = 1;
                    this.SaveOrAdd(item, AttachType.Contract, SysAgent.Id, salesLog.Id);
                }
            }
            if (Certificate != null && Certificate.Count() > 0)
            {
                foreach (var item in Certificate)
                {
                    item.State = 1;
                    this.SaveOrAdd(item, AttachType.Certificate, SysAgent.Id, salesLog.Id);
                }
            }
            if (DelIds != null && DelIds.Count > 0)
            {
                string sql = "UPDATE Attach SET State = 0, RemoveSLogId= " + salesLog.Id + " WHERE Id in(" + string.Join(",", DelIds) + ");";
                Entity.ExecuteStoreCommand(sql);
            }
            #endregion
        }
        private void SaveOrAdd(Attach Attach, AttachType AType, int AgentId, int SLogId)
        {
            var entity = Entity.Attach.FirstOrDefault(o => o.Id == Attach.Id);
            if (entity == null)
            {
                entity = new Attach()
                {
                    AFile = Attach.AFile,
                    AddTime = DateTime.Now,
                    AType = (byte)AType,
                    AgentId = AgentId,
                    SAId = this.AdminUser.Id,
                    State = 1,
                    SLogId = SLogId,
                    UpLoadFile = Attach.UpLoadFile
                };
                Entity.Attach.AddObject(entity);
            }
            else
            {
                entity = Request.ConvertRequestToModel<Attach>(entity, Attach);
                entity.SAId = this.AdminUser.Id;
            }
        }
        public void ChangeStatus(SysAgent SysAgent, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = SysAgent.Id.ToString(); }
            int Ret = Entity.ChangeEntity<SysAgent>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(SysAgent SysAgent, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = SysAgent.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<SysAgent>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        /// <summary>
        /// 销售信息记录
        /// </summary>
        /// <returns></returns>
        public ActionResult EditSalesLogIndex(SalesLog SalesLog, EFPagingInfo<SalesLog> p)
        {
            if (!SalesLog.AgentId.IsNullOrEmpty()) { p.SqlWhere.Add(o => o.AgentId == SalesLog.AgentId); }
            else
            {
                ViewBag.ErrorMsg = "查询不到对应的数据";
                return View("Error");
            }
            p.OrderByList.Add("AddTime", "DESC");
            IPageOfItems<SalesLog> SalesLogList = Entity.Selects<SalesLog>(p);
            ViewBag.SalesLogList = SalesLogList;
            ViewBag.SalesLog = SalesLog;
            return View("SalesLogIndex");
        }
        public ActionResult EditSalesLog(SalesLog SalesLog)
        {
            if (SalesLog.Id != 0) SalesLog = Entity.SalesLog.FirstOrDefault(o => o.Id == SalesLog.Id);
            ViewBag.SalesLog = SalesLog;
            var Contract = new List<Attach>();
            var Certificate = new List<Attach>();
            var IDCard_Front = new List<Attach>();
            var IDCard_Hand = new List<Attach>();
            var IDCard_Reverse = new List<Attach>();
            if (SalesLog.AgentId != 0)
            {
                var attachs = Entity.Attach.Where(o => o.SLogId == SalesLog.Id || o.RemoveSLogId == SalesLog.Id).ToList();
                Contract = attachs.Where(o => o.AType == (byte)AttachType.Contract).ToList();
                Certificate = attachs.Where(o => o.AType == (byte)AttachType.Certificate).ToList();
                IDCard_Front = attachs.Where(o => o.AType == (byte)AttachType.IDCard_Front).ToList();
                IDCard_Hand = attachs.Where(o => o.AType == (byte)AttachType.IDCard_Hand).ToList();
                IDCard_Reverse = attachs.Where(o => o.AType == (byte)AttachType.IDCard_Reverse).ToList();
            }
            this.ViewBag.Contract = Contract;
            this.ViewBag.Certificate = Certificate;
            this.ViewBag.IDCard_Front = IDCard_Front;
            this.ViewBag.IDCard_Hand = IDCard_Hand;
            this.ViewBag.IDCard_Reverse = IDCard_Reverse;
            return View("EditSalesLog");
        }
        private void LockAllAgent(SysAgent SA)
        {
            //递归锁定所有下级
            IList<SysAgent> List = Entity.SysAgent.Where(n => n.State == 1 && n.AgentID == SA.Id).ToList();
            foreach (var p in List)
            {
                p.State = 0;
                this.LockAllAgent(p);
            }
        }

        /// <summary>
        /// 代理商配置入网费率
        /// </summary>
        /// <returns></returns>
        public ActionResult PayEdit(SysAgent SysAgent)
        {
            if (SysAgent.Id != 0) SysAgent = Entity.SysAgent.FirstOrDefault(n => n.Id == SysAgent.Id);
            if (SysAgent == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.SysAgent = SysAgent;
            ViewBag.PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
            ViewBag.UserPayAgentList = Entity.UserPayAgent.Where(n => n.AId == SysAgent.Id).ToList();
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            ViewBag.PayEdit = this.checkPower("PayEdit");
            ViewBag.PaySave = this.checkPower("PaySave");
            return View();
        }
        [ValidateInput(false)]
        public void PaySave(SysAgent SysAgent, int[] PId, double[] Cost, int[] PState, int? AnsyCash, int? AnsyNext)
        {
            if (SysAgent.Id != 0) SysAgent = Entity.SysAgent.FirstOrDefault(n => n.Id == SysAgent.Id);
            if (SysAgent == null) {
                return;
            }
            for (int i = 0; i < PId.Length; i++)
            {
                int Pid = PId[i];
                UserPayAgent PCT = Entity.UserPayAgent.FirstOrNew(n => n.AId == SysAgent.Id && n.PId == Pid);
                PCT.Cost = Cost[i] / 1000;
                if (PCT.Id.IsNullOrEmpty())
                {
                    PCT.PId = PId[i];
                    PCT.AId = SysAgent.Id;
                    Entity.UserPayAgent.AddObject(PCT);
                }
            }
            Entity.SaveChanges();
            
            IList<PayConfig> PayConfigList=new List<PayConfig>();
            IList<UserPayAgent> UserPayAgentList=new List<UserPayAgent>();
            IList<SysAgent> AgentList=new List<SysAgent>();
            string AgentIds = "0";
            if(AnsyCash==1 || AnsyNext==1){
                PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
                UserPayAgentList = Entity.UserPayAgent.Where(n => n.AId == SysAgent.Id).ToList();
                //取得代理商所有的下级
                AgentList = SysAgent.GetSupAgent(Entity);
                foreach (var p in AgentList) {
                    AgentIds = AgentIds + "," + p.Id;
                }
            }
            if (AnsyCash == 1)
            {
                //使用删除全部后根据用户表生成，有效解决了因接口关闭或新增加接口，老用户没有配置问题
                string SQL = "Delete UserPay Where UId in(Select Id From Users Where Agent in(" + AgentIds + "))";
                Entity.ExecuteStoreCommand(SQL);
                foreach (var p in PayConfigList)
                {
                    double? cost = p.CostUser;
                    UserPayAgent PCT = UserPayAgentList.FirstOrNew(n => n.AId == SysAgent.Id && n.PId == p.Id);
                    if (!PCT.Id.IsNullOrEmpty())
                    {
                        cost = PCT.Cost;
                    }
                    SQL = "INSERT INTO UserPay(UId,PId,Cost,IsDel) Select ID," + p.Id + " As PId," + cost + " As Cost, 0 As IsDel From Users where Id in(Select Id From Users Where Agent in(" + AgentIds + "))";
                    Entity.ExecuteStoreCommand(SQL);
                }
            }
            if (AnsyNext == 1)
            {
                //使用删除全部后根据用户表生成，有效解决了因接口关闭或新增加接口，老用户没有配置问题
                string SQL = "Delete UserPayAgent Where AId in (" + AgentIds + ")";
                Entity.ExecuteStoreCommand(SQL);
                foreach (var p in PayConfigList)
                {
                    double? cost = p.CostUser;
                    UserPayAgent PCT = UserPayAgentList.FirstOrNew(n => n.AId == SysAgent.Id && n.PId == p.Id);
                    if (!PCT.Id.IsNullOrEmpty())
                    {
                        cost = PCT.Cost;
                    }
                    SQL = "INSERT INTO UserPayAgent(AId,PId,Cost,IsDel) Select ID," + p.Id + " As PId," + cost + " As Cost, 0 As IsDel From SysAgent where Id in(" + AgentIds + ")";
                    Entity.ExecuteStoreCommand(SQL);
                }
            }

            BaseRedirect();
        }
        [ValidateInput(false)]
        public JsonResult EditChecktelephone(string fieldId, string fieldValue)
        {
            if (fieldValue.Contains("400-608-6765") || fieldValue.Contains("23769678") || fieldValue.Contains("22220076") || fieldValue.Contains("4006086765"))
            {
                return Json(new object[] { "Tel", false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new object[] { "Tel", true }, JsonRequestBehavior.AllowGet);
        }
    }
}
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
using System.Web.Mvc;
namespace LokFu.Areas.Agent.Controllers
{
    public class CardController : BaseController
    {
        
        public ActionResult Index(Card Card, EFPagingInfo<Card> p, string Num0, string Num1, int IsFirst = 0)
        {
            bool SetSave = false;
            if (checkPower("SetSave"))
            {
                SetSave = true;
            }
            ViewBag.SetSave = SetSave;

            if (!Card.Code.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Code == Card.Code); }
            if (!Num0.IsNullOrEmpty() && !Num1.IsNullOrEmpty())
            {
                long num0 = Int64.Parse(Num0);
                long num1 = Int64.Parse(Num1);
                num0 = num0 - 1000000000;
                num1 = num1 - 1000000000;
                p.SqlWhere.Add(f => f.Id >= num0 && f.Id <= num1);
            }
            else { 
                if (!Num0.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Code == Num0); }
            }
            if (!Card.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == (Card.State == 99 ? 0 : Card.State)); }
            if (!Card.AId.IsNullOrEmpty())
            {
                if (Card.AId == 99)
                {
                    p.SqlWhere.Add(f => f.AdminId == 0);
                }
                if (Card.AId == 1)
                {
                    p.SqlWhere.Add(f => f.AdminId > 0);
                }
            }
            if (!Card.AdminId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AdminId == Card.AdminId); }
            if (BasicAgent.LinkMobile == AdminUser.UserName)
            {
                p.SqlWhere.Add(f => f.AId == BasicAgent.Id);//读取全部分支机构
                ViewBag.SysAdminList = Entity.SysAdmin.Where(n => n.AgentId == BasicAgent.Id && n.State == 1).ToList();
            }
            else
            {
                p.SqlWhere.Add(f => f.AdminId == AdminUser.Id);//读取用户
                ViewBag.SysAdminList = new List<SysAdmin>();
            }
            if (p.PageSize.IsNullOrEmpty())
            {
                p.PageSize = 20;
            }
            if (p.PageSize == 10) {
                p.PageSize = 20;
            }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<Card> CardList = null;
            if (IsFirst == 0)
            {
                CardList = new PageOfItems<Card>(new List<Card>(), 0, 10, 0, new Hashtable());
            }
            else
            { 
                CardList = Entity.Selects<Card>(p);
            }
            ViewBag.CardList = CardList;
            ViewBag.Card = Card;
            ViewBag.Num0 = Num0;
            ViewBag.Num1 = Num1;
            IList<SysAgent> SysAgentList = new List<SysAgent>();
            if (BasicAgent.Tier < BasicAgent.AgentLevelMax)
            {
                SysAgentList = Entity.SysAgent.Where(n =>  n.AgentID == BasicAgent.Id).ToList();
            }
            ViewBag.SysAgentList = SysAgentList;
            ViewBag.Save = this.checkPower("Save");

            return View();
        }
        public void Save(string InfoList, string Value, string Type,string start,string end)
        {
            Card Card = new Card();
            int Ret = 0;
            if (Type == "User")
            {
                Ret = Entity.ChangeEntity<Card>(InfoList, "AdminId", Value);
            }
            if (Type == "Agent")
            {
                Ret = Entity.ChangeEntity<Card>(InfoList, "AId", Value);
            }
            if (Type == "BatchAgent")
            {
                string SQL = "update Card set AId='" + Value + "' where Code>='" + start + "' and Code<='" + end + "'  and State=1 and AId=" + BasicAgent.Id+ " and AdminId=0";
                Ret=Entity.ExecuteStoreCommand(SQL);
            }
            if (Type == "BatchUser")
            {
                string SQL = "update Card set AdminId='" + Value + "' where Code>='" + start + "' and Code<='" + end + "'  and State=1 and AId=" + BasicAgent.Id + " and AdminId=0";
                Ret=Entity.ExecuteStoreCommand(SQL);
            }
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void SetSave(byte Set3)
        {
            if (BasicAgent.IsTeiPai == 1 && BasicAgent.Tier == 1)
            {
                BasicAgent.Set3 = Set3;
            }
            Entity.SaveChanges();
            Response.Write("OK");
        }

        public void ChangeStatus(Card Card, string InfoList, string Clomn, string Value)
        {
            Clomn = "Auto";
            if (string.IsNullOrEmpty(InfoList)) { InfoList = Card.Id.ToString(); }
            int Ret = Entity.ChangeEntity<Card>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}

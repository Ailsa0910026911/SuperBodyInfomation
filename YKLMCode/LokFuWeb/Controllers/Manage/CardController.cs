using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class CardController : BaseController
    {

        public ActionResult Index(Card Card, EFPagingInfo<Card> p, string Num0, string Num1, bool? IsShowSupAgent, int IsFirst = 0)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            if (IsFirst == 0)
            {
                PageOfItems<Card> CardList1 = new PageOfItems<Card>(new List<Card>(), 0, 10, 0, new Hashtable());
                ViewBag.CardList = CardList1;
                ViewBag.Card = Card;
                ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.State == 1).ToList();
                ViewBag.Num0 = Num0;
                ViewBag.Num1 = Num1;
                ViewBag.IsShowSupAgent = IsShowSupAgent;
                ViewBag.Add = this.checkPower("Add");
                ViewBag.Xls = this.checkPower("Xls");
                ViewBag.Save = this.checkPower("Save");
                return View();
            }
            if (!Card.Code.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Code == Card.Code); }
            if (!Card.AId.IsNullOrEmpty())
            {
                //是否要显示下级
                if ((bool)IsShowSupAgent)
                {
                    SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == Card.AId).FirstOrNew();
                    IList<SysAgent> SysAgentList = LowerLevelAgent.GetSupAgent(Entity);
                    IList<int> UID = new List<int>();
                    foreach (var s in SysAgentList)
                    {
                        UID.Add(s.Id);
                    }
                    p.SqlWhere.Add(f => UID.Contains(f.AId));
                }
                else
                {
                    p.SqlWhere.Add(f => f.AId == Card.AId);
                }
            }
            if (!Card.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == (Card.State == 99 ? 0 : Card.State)); }
            if (!Card.Auto.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Auto == (Card.Auto == 99 ? 0 : Card.Auto)); }
            if (!Num0.IsNullOrEmpty() && !Num1.IsNullOrEmpty())
            {
                long num0 = Int64.Parse(Num0);
                long num1 = Int64.Parse(Num1);
                num0 = num0 - 1000000000;
                num1 = num1 - 1000000000;
                p.SqlWhere.Add(f => f.Id >= num0 && f.Id <= num1);
            }
            else
            {
                if (!Num0.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Code == Num0); }
            }
            p.OrderByList.Add("Id", "DESC");
            if (p.PageSize.IsNullOrEmpty())
            {
                p.PageSize = 20;
            }
            if (p.PageSize == 10)
            {
                p.PageSize = 20;
            }
            IPageOfItems<Card> CardList = Entity.Selects<Card>(p);
            ViewBag.CardList = CardList;
            ViewBag.Card = Card;
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.State == 1).ToList();
            ViewBag.Num0 = Num0;
            ViewBag.Num1 = Num1;
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Xls = this.checkPower("Xls");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit()
        {
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.State == 1).ToList();
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Add(int AId, int Num, byte Auto)
        {
            if (AId.IsNullOrEmpty())
            {
                AId = 0;
            }
            SysAdmin sysadmin = Entity.SysAdmin.FirstOrNew(o => o.AgentId == AId);
            DateTime Now = DateTime.Now;
            for (int i = 0; i < Num; i++)
            {
                Card Obj = new Card();
                Obj.State = 1;
                Obj.LState = 0;
                Obj.AddTime = Now;
                Obj.AdminId = sysadmin.Id;
                Obj.AId = AId;
                Obj.Auto = Auto;
                Obj.PasWd = GetCardPasWd();
                Entity.Card.AddObject(Obj);
                Entity.SaveChanges();
                Obj.Code = (1000000000 + Obj.Id).ToString();
                Entity.SaveChanges();
            }
            BaseRedirect();
        }
        public void ChangeStatus(Card Card, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = Card.Id.ToString(); }
            int Ret = Entity.ChangeEntity<Card>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(Card Card, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = Card.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<Card>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        //public string GetCardCode()
        //{
        //    int W = 10;
        //    bool Run = true;
        //    string Code = "";
        //    while (Run)
        //    {
        //        Code = GetCode(W);
        //        Card C = Entity.Card.Where(n => n.Code == Code).FirstOrDefault();
        //        if (C == null)
        //        {
        //            Run = false;
        //        }
        //    }
        //    return Code;
        //}
        public string GetCardPasWd()
        {
            int W = 6;
            return GetCode(W);
        }
        public string GetCode(int W)
        {
            char[] Char = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            int length = Char.Length;
            StringBuilder newRandom = new StringBuilder(W);
            for (int i = 0; i < W; i++)
            {
                int seed = GetRandomSeed();
                Random rd = new Random(seed);
                newRandom.Append(Char[rd.Next(length)]);
            }
            return newRandom.ToString();
        }
        public int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
        public ActionResult Xls()
        {
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.State == 1).ToList();
            ViewBag.SysAdminList = Entity.SysAdmin.Where(n => n.AgentId > 0 && n.State == 1).ToList();
            return View();
        }
        public void XLSDo(Card Card, EFPagingInfo<Card> p, DateTime? FInTime, DateTime? FOutTime, string Num0, string Num1, string Password)
        {
            if (!Card.State.IsNullOrEmpty())
            {
                if (Card.State == 99)
                {
                    Card.State = 0;
                }
                p.SqlWhere.Add(f => f.State == Card.State);
            }
            if (!Card.Auto.IsNullOrEmpty())
            {
                if (Card.Auto == 99)
                {
                    Card.Auto = 0;
                }
                p.SqlWhere.Add(f => f.Auto == Card.Auto);
            }
            if (!Card.AId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AId == Card.AId); }
            if (!Card.AdminId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AdminId == Card.AdminId); }
            if (!FInTime.IsNullOrEmpty() && !FOutTime.IsNullOrEmpty())
            {
               // FOutTime = ((DateTime)FOutTime).AddDays(1);
                p.SqlWhere.Add(f => f.AddTime > FInTime && f.AddTime < FOutTime);
            }
            if (!Num0.IsNullOrEmpty() && !Num1.IsNullOrEmpty())
            {
                long num0 = Int64.Parse(Num0);
                long num1 = Int64.Parse(Num1);
                num0 = num0 - 1000000000;
                num1 = num1 - 1000000000;
                p.SqlWhere.Add(f => f.Id >= num0 && f.Id <= num1);
            }
            p.PageSize = 100000;
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<Card> cardList = Entity.Selects<Card>(p);
            if (cardList.Count() > 0)
            {
                string file = Server.MapPath("/template") + "\\card.xlsx";
                ExcelPackage package = new ExcelPackage(new FileInfo(file), true);
                var sheet = package.Workbook.Worksheets[1];
                var cells = sheet.Cells;
                int Rows = cardList.Count();
                //设置数据开始行
                int Befor = 2;
                sheet.InsertRow(Befor + 1, Rows - 1, Befor);
                int i = Befor;
                sheet.Row(i - 1).Height = 22;//设置行高
                string[] State = new string[] { "已失效", "正常", "已使用" };
                string[] Auto = new string[] { "自动", "保留" };
                int maxCol = 0;
                //获取机构及管理员
                IList<SysAgent> SAList = Entity.SysAgent.Where(n => n.State == 1).ToList();
                IList<SysAdmin> AdminList = Entity.SysAdmin.Where(n => n.State == 1 && n.AgentId > 0).ToList();
                foreach (var item in cardList)
                {
                    string AName = "--";
                    if (item.AId > 0)
                    {
                        SysAgent SA = SAList.FirstOrNew(n => n.Id == item.AId);
                        if (!SA.Name.IsNullOrEmpty())
                        {
                            AName = SA.Name;
                        }
                    }
                    string BName = "--";
                    if (item.AdminId > 0)
                    {
                        SysAdmin SAA = AdminList.FirstOrNew(n => n.Id == item.AdminId);
                        if (!SAA.TrueName.IsNullOrEmpty())
                        {
                            BName = SAA.TrueName;
                        }
                    }
                    sheet.Row(i).Height = 20;//设置行高
                    //机构
                    cells["A" + i].Value = AName;
                    //编号
                    cells["B" + i].Value = BName;
                    //卡号
                    cells["C" + i].Value = item.Code;
                    //密码
                    cells["D" + i].Value = item.PasWd;
                    //时间
                    cells["E" + i].Value = item.AddTime;
                    //状态
                    int state = (int)item.State;
                    if (state > 2)
                    {
                        state = 0;
                    }
                    cells["F" + i].Value = State[state];
                    //保留
                    byte auto = item.Auto;
                    if (auto > 2)
                    {
                        auto = 0;
                    }
                    cells["G" + i].Value = Auto[auto];
                    i++;
                }
                i--;
                maxCol = 7;
                //cells["B" + (i + 2)].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                sheet.Cells[Befor, 1, i, maxCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                sheet.Cells[Befor, 1, i, maxCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                sheet.Cells[Befor, 1, i, maxCol].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                sheet.Cells[Befor, 1, i, maxCol].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                sheet.Cells[Befor, 1, i, maxCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;//居中
                sheet.Cells[Befor, 5, i, 5].Style.Numberformat.Format = "yyyy-mm-dd HH:mm:ss";
                for (int j = i + 1; j <= i + 2; j++)
                {
                    sheet.Row(j).Height = 20;//设置行高
                }
                //设置密码
                if (!Password.IsNullOrEmpty())
                {
                    Response.BinaryWrite(package.GetAsByteArray(Password));
                }
                else
                {
                    Response.BinaryWrite(package.GetAsByteArray());
                }
                //输出
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;  filename=" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10, 99) + ".xlsx");
            }
            else
            {
                Response.Write("暂无符合条件数据");
            }
        }
    }
}

using LokFu.Extensions;
using LokFu.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class TaskTimeSetController : BaseController
    {
        private int TId = 1;
        public TaskTimeSetController()
        {
            ViewBag.Authorization = true;//允许权限
        }
        public ActionResult Edit()
        {
            return View();
        }
        [ValidateInput(false)]
        public void Save(int SendBy, string[] Week, string[] SendDay, DateTime SDate, DateTime EDate, int sH, int sM, int sS, int eH, int eM, int eS, decimal AllMoney)
        {
            SDate = SDate.Date;
            if (SendBy == 1)//按周
            {
                List<int> WKList = new List<int>();
                foreach (var p in Week)
                {
                    if (!p.IsNullOrEmpty())
                    {
                        int W = Int32.Parse(p);
                        WKList.Add(W);
                    }
                }
                if (WKList.Count > 0)
                {
                    DateTime nowday = SDate;
                    while (nowday <= EDate)
                    {
                        int W = Convert.ToInt32(nowday.DayOfWeek);
                        if (WKList.Contains(W))
                        {
                            AddDay(nowday, sH, sM, sS, eH, eM, eS, AllMoney);
                        }
                        nowday = nowday.AddDays(1);
                    }
                }
            }
            if (SendBy == 2)//按月
            {
                List<int> DSList = new List<int>();
                foreach (var p in SendDay)
                {
                    if (!p.IsNullOrEmpty())
                    {
                        int W = Int32.Parse(p);
                        DSList.Add(W);
                    }
                }
                if (DSList.Count > 0)
                {
                    DateTime nowday = SDate;
                    while (nowday <= EDate)
                    {
                        int D = nowday.Day;
                        if (DSList.Contains(D))
                        {
                            AddDay(nowday, sH, sM, sS, eH, eM, eS, AllMoney);
                        }
                        nowday = nowday.AddDays(1);
                    }
                }
            }
            if (SendBy == 0) {
                AddDay(SDate, sH, sM, sS, eH, eM, eS, AllMoney);
            }
            Entity.SaveChanges();
            Response.Write("ok");
        }
        private void AddDay(DateTime ODate, int sH, int sM, int sS, int eH, int eM, int eS, decimal AllMoney)
        {
            TaskTimeSet TaskTimeSet = Entity.TaskTimeSet.FirstOrDefault(n => n.TId == TId && n.ODate == ODate);
            if (TaskTimeSet == null)
            {
                TaskTimeSet = new TaskTimeSet();
                TaskTimeSet.ODate = ODate;
            }
            DateTime STime = TaskTimeSet.ODate;
            DateTime ETime = TaskTimeSet.ODate;
            if (!sH.IsNullOrEmpty()) {
                STime = STime.AddHours(sH);
            }
            if (!sM.IsNullOrEmpty())
            {
                STime = STime.AddMinutes(sM);
            }
            if (!sS.IsNullOrEmpty())
            {
                STime = STime.AddSeconds(sS);
            }
            if (!eH.IsNullOrEmpty())
            {
                ETime = ETime.AddHours(eH);
            }
            if (!eM.IsNullOrEmpty())
            {
                ETime = ETime.AddMinutes(eM);
            }
            if (!eS.IsNullOrEmpty())
            {
                ETime = ETime.AddSeconds(eS);
            }
            TaskTimeSet.STime = STime;
            TaskTimeSet.ETime = ETime;
            TaskTimeSet.AllMoney = AllMoney;
            if (TaskTimeSet.Id == 0)
            {
                TaskTimeSet.ODate = ODate;
                TaskTimeSet.AddTime = DateTime.Now;
                TaskTimeSet.TId = TId;
                Entity.TaskTimeSet.AddObject(TaskTimeSet);
            }
        }
        public ActionResult GetDay(int year, int month) {
            TaskTimeSet TaskTimeSet = new TaskTimeSet();
            DateTime ODate = new DateTime(year, month, 1);
            TaskTimeSet.ODate = ODate;
            ViewBag.TaskTimeSet = TaskTimeSet;
            return View();
        }
        public void Delete(string date)
        {
            int Ret = 0;
            if (date.IsNullOrEmpty())
            {
                Ret = Entity.ExecuteStoreCommand("Delete TaskTimeSet Where TId=" + TId);
            }
            else {
                Ret = Entity.ExecuteStoreCommand("Delete TaskTimeSet Where ODate='" + date + "' and TId=" + TId);
            }
            Response.Write(Ret);
        }
    }
}

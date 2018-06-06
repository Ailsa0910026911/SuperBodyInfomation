using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Text;
namespace LokFu.Controllers
{
    public class JobOrdersController : InitController
    {
        public JobOrdersController()
        {
            if (!InitState)
            {
                DataObj.OutError("8080");
                return;
            }
            if (DataObj == null)
            {
                DataObj.OutError("1000");
                return;
            }
            if (!DataObj.IsReg)
            {
                DataObj.OutError("3002");
                return;
            }
        }
        public void Post()
        {
            string Data = DataObj.GetData();
            if (Data.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(Data);
            }
            catch (Exception Ex)
            {
                Log.Write("[JobOrders]:", "【Data】" + Data, Ex);
                DataObj.OutError("1000");
                return;
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            JobOrders JobOrders = new JobOrders();
            JobOrders = JsonToObject.ConvertJsonToModel(JobOrders, json);

            #region 获取用户信息
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == JobOrders.Token);
            if (baseUsers == null)//用户令牌不存在
            {
                DataObj.OutError("2004");
                return;
            }
            if (baseUsers.State != 1)//用户被锁定
            {
                DataObj.OutError("2003");
                return;
            }
            #endregion

            EFPagingInfo<JobOrders> p = new EFPagingInfo<JobOrders>();
            if (!JobOrders.Pg.IsNullOrEmpty()) { p.PageIndex = JobOrders.Pg; }
            if (!JobOrders.Pgs.IsNullOrEmpty()) { p.PageSize = JobOrders.Pgs; }
            p.SqlWhere.Add(f => f.UId == baseUsers.Id);
            
            //这里JobOrders.State=1进行中 JobOrders.State=2已完成
            if (JobOrders.State == 1)
            {
                p.SqlWhere.Add(f => f.State == 2 || f.State == 3);
            }
            else if (JobOrders.State == 2)
            {
                p.SqlWhere.Add(f => f.State == 4 || f.State == 5);
            }

            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<JobOrders> List = Entity.Selects<JobOrders>(p);
            var tnums = List.Select(o => o.TNum).ToList();
            var UserCardIds = List.Select(o => o.UserCardId).ToList();
            List<JobItem> JobItemList = Entity.JobItem.Where(n => tnums.Contains(n.TNum)).ToList();
            List<UserCard> UserCardList = Entity.UserCard.Where(n => UserCardIds.Contains(n.Id)).ToList();
            foreach (var item in List)
            {
                item.Cols = item.Cols + ",Items,UsersCard";

                var itemList = JobItemList.Where(o => o.TNum == item.TNum).ToList();
                var JobItemModelList = itemList.GroupBy(o => o.RunTime.Date, (x, o) => new JobItemModel
                {
                    date = x,
                    item = o.OrderBy(k => k.RunType).ToList().EntityToJson(),
                }).OrderBy(o=>o.date).ToList();
                item.Items = JobItemModelList.EntityToJson();

                var UserCard = UserCardList.FirstOrNew(o=>o.Id == item.UserCardId);
                string UserCardStr = UserCard.OutJson();
                JObject JS = new JObject();
                try
                {
                    JS = (JObject)JsonConvert.DeserializeObject(UserCardStr);
                }
                catch (Exception Ex)
                {
                    Log.Write("[JobOrders]:", "【JsStr】" + UserCardStr, Ex);
                }
                item.UsersCard = JS;
            }
            IList<JobOrders> iList = List.ToList();
            StringBuilder sb = new StringBuilder("");
            sb.Append("{");
            sb.Append(List.PageToString());
            sb.Append(",");
            sb.Append(iList.EntityToString());
            sb.Append("}");
            string data = sb.ToString();
            data = data.Replace("\"[{", "[{").Replace("}]\"", "}]").Replace("\\", "").Replace("]\"}", "]}");
            DataObj.Data = data;
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}

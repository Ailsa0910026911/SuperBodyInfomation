using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using LokFu;
using LokFu.Repositories;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;
using LokFu.Extensions;

namespace LokFu.Controllers
{
    public class ShopInfosController : InitController
    {
        public ShopInfosController()
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
                Log.Write("[ShopInfos]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            //JObject pp = (JObject)p;
            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);
            if (Users.Token.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Users.Token);
            if (baseUsers == null)//用户令牌不存在
            {
                DataObj.OutError("2004");
                return;
            }
            if (baseUsers.State != 1)
            {
                DataObj.OutError("2003");
                return;
            }

            IList<UserPic> List = Entity.UserPic.Where(n => n.UId == baseUsers.Id).OrderBy(n => n.Sort).ToList();
            foreach (var p in List) {
                p.Pic = Utils.ImageUrl("UserPic", p.Pic, AppImgPath);
            }
            StringBuilder sb = new StringBuilder("");
            sb.Append("{");
            baseUsers.Cols = "ShopInfo,ShopTel,ShopNotice";
            sb.Append(baseUsers.ToStr());
            sb.Append(",");
            sb.Append(List.EntityToString());
            sb.Append("}");
            
            DataObj.Data = sb.ToString();

            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}

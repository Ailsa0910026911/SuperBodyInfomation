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
    public class ShopPicController : InitController
    {
        public ShopPicController()
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
                Log.Write("[ShopPic]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            //JObject pp = (JObject)p;
            UserPic UserPic = new UserPic();
            UserPic = JsonToObject.ConvertJsonToModel(UserPic, json);
            UserPic.Pic = Utils.Base64StringToImage(UserPic.Pic, "UserPic");
            if (UserPic.Pic.IsNullOrEmpty() || UserPic.Pic == "Err")
            {
                DataObj.OutError("4001");
                return;
            }
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == UserPic.Token);
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

            UserPic.UId = baseUsers.Id;
            UserPic.AddTime = DateTime.Now;
            Entity.UserPic.AddObject(UserPic);
            Entity.SaveChanges();

            UserPic.Sort = UserPic.Id;
            Entity.SaveChanges();

            UserPic.Pic = Utils.ImageUrl("UserPic", UserPic.Pic, AppImgPath);

            DataObj.Data = UserPic.OutJson();

            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}

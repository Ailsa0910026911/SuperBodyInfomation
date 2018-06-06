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
    public class UsersTrueNameController : InitController
    {
        public UsersTrueNameController()
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
            //1.0接口停止使用 20150921，返回参数错误
            DataObj.OutError("1000");
            return;
            /*
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
                Log.Write("[UsersTrueName]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            //JObject pp = (JObject)p;
            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);
            //Users.CardPic = System.Web.HttpContext.Current.Request.Form["cardpic"];
            
            if (Users.CardPic.IsNullOrEmpty() || Users.CardPic == "Err")
            {
                DataObj.OutError("4001");
                return;
            }
            if (Users.TrueName.IsNullOrEmpty() || Users.CardId.IsNullOrEmpty())
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
            if (baseUsers.CardStae == 2) { //已实名认证
                DataObj.OutError("2007");
                return;
            }
            int CardIdCount = Entity.Users.Count(n => n.CardId == Users.CardId && n.CardStae == 2);
            if (CardIdCount > 0) //身份证已用过
            {
                DataObj.OutError("2020");
                return;
            }

            Users.CardPic = Utils.Base64StringToImage(Users.CardPic, "Users");
            Users.CardFace = Utils.Base64StringToImage(Users.CardFace, "Users");
            Users.CardBack = Utils.Base64StringToImage(Users.CardBack, "Users");

            baseUsers.CardStae = 1;
            baseUsers.TrueName = Users.TrueName;
            baseUsers.NeekName = Users.NeekName;
            baseUsers.CardId = Users.CardId;
            baseUsers.CardPic = Users.CardPic;
            baseUsers.CardFace = Users.CardFace;
            baseUsers.CardBack = Users.CardBack;
            Entity.SaveChanges();

            baseUsers.CardPic = Utils.ImageUrl("Users", baseUsers.CardPic, ApiPath);
            baseUsers.CardFace = Utils.ImageUrl("Users", baseUsers.CardFace, ApiPath);
            baseUsers.CardBack = Utils.ImageUrl("Users", baseUsers.CardBack, ApiPath);

            baseUsers.Cols = "TrueName,CardStae,CardPic,CardFace,CardBack,CardId";

            DataObj.Data = baseUsers.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
           */
        }
    }
}

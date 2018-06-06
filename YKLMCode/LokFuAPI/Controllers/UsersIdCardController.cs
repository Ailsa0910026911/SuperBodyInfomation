using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Data.Objects;
using System.Linq;
using System.Text;
using System.Web;

namespace LokFu.Controllers
{
    public class UsersIdCardController : InitController
    {
        public UsersIdCardController()
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
                Log.Write("[UsersIdCardController]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
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
            if (baseUsers.CardStae == 2)
            {
                if (baseUsers.CardId.ToLower() != Users.CardId.ToLower())
                {
                    DataObj.OutError("1101");
                    return;
                }
            }


            Users.CardPic = Utils.Base64StringToImage(Users.CardPic, "Users");
            if (Users.CardPic.IsNullOrEmpty() || Users.CardPic == "Err")
            {
                DataObj.OutError("4001");
                return;
            }
            Users.CardBack = Utils.Base64StringToImage(Users.CardBack, "Users");
            if (Users.CardBack.IsNullOrEmpty() || Users.CardBack == "Err")
            {
                DataObj.OutError("4001");
                return;
            }
            if (!Users.CardValidEDate.HasValue)
            {
                if (Users.CardValidEDate < DateTime.Now)
                {
                    DataObj.OutError("2028");
                    return;
                }
            }
            
            baseUsers.CardId = Users.CardId;
            baseUsers.CardPic = Users.CardPic;
            baseUsers.CardBack = Users.CardBack;
            baseUsers.CardGender = Users.CardGender;
            baseUsers.CardNation = Users.CardNation;
            baseUsers.CardAddress = Users.CardAddress;
            baseUsers.CardIssue = Users.CardIssue;
            baseUsers.CardValidSDate = Users.CardValidSDate;
            baseUsers.CardValidEDate = Users.CardValidEDate;
            baseUsers.CardTrueName = Users.CardTrueName;
            baseUsers.CardBackMode = Users.CardBackMode;
            baseUsers.CardFrontMode = Users.CardFrontMode;
            Entity.SaveChanges();

            DataObj.Data = "";
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}

using LokFu.Extensions;
using LokFu.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace LokFu.Controllers
{
    /// <summary>
    /// 银行卡号信息
    /// </summary>
    public class CardBinController : InitController
    {
        public CardBinController()
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
                Log.Write("[CreditCardAdd]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            UserCard UserCard = new UserCard();
            UserCard = JsonToObject.ConvertJsonToModel(UserCard, json);
            DataObj.Data = "";
            UserCard.Card = UserCard.Card.Replace(" ", "");
            if (!UserCard.Card.IsNullOrEmpty() && UserCard.Card.Length >= 6)
            {
                string wei6 = UserCard.Card.Substring(0, 6);
                BasicCardBin BasicCardBin = Entity.BasicCardBin.FirstOrDefault(o => o.BIN == wei6);
                if (BasicCardBin != null)
                {
                    BasicCardBin.Card = UserCard.Card;
                    BasicCardBin.Cols = "BankName,CardType,Card";
                    DataObj.Data = BasicCardBin.OutJson();
                }
            }
            DataObj.Code = "0000";
            DataObj.OutString();
            
        }
    }
}

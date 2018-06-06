using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using LokFu;
using LokFu.Repositories;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace LokFu.Controllers
{
    public class UsersRegController : InitController
    {
        public UsersRegController()
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
            //1.0接口不开放，关闭处理2016-06-27
            DataObj.OutError("9999");
            return;
            //string Data = DataObj.GetData();
            //if (Data.IsNullOrEmpty())
            //{
            //    DataObj.OutError("1000");
            //    return;
            //}
            //JObject json = new JObject();
            //try
            //{
            //    json = (JObject)JsonConvert.DeserializeObject(Data);
            //}
            //catch (Exception Ex)
            //{
            //    Log.Write("[UsersReg]:", "【Data】" + Data, Ex);
            //}
            //if (json == null) {
            //    DataObj.OutError("1000");
            //    return;
            //}
            ////JObject pp = (JObject)p;
            //Users Users = new Users();
            //Users = JsonToObject.ConvertJsonToModel(Users, json);
            //if (Users.UserName.IsNullOrEmpty() || Users.PassWord.IsNullOrEmpty() || Users.Mobile.IsNullOrEmpty() || Users.X.IsNullOrEmpty() || Users.Y.IsNullOrEmpty() || Users.RegAddress.IsNullOrEmpty()) { 
            //    //
            //    DataObj.OutError("1000");
            //    return;
            //}

            ////验证是否重复
            //Users Old = Entity.Users.FirstOrDefault(n => n.UserName == Users.UserName);
            //if (Old != null)
            //{
            //    DataObj.OutError("2005");
            //    return;
            //}

            //Card Card = Entity.Card.FirstOrDefault(n => n.Code == Users.CardNum && n.PasWd == Users.CardPWD);
            //if (Card == null) {
            //    DataObj.OutError("5001");
            //    return;
            //}
            //if (Card.State != 1) {
            //    DataObj.OutError("5002");
            //    return;
            //}
            //if (Card.AId.IsNullOrEmpty())
            //{
            //    DataObj.OutError("5002");
            //    return;
            //}
            //if (Card.AdminId.IsNullOrEmpty())
            //{
            //    DataObj.OutError("5002");
            //    return;
            //}

            //SysSet SysSet = Entity.SysSet.FirstOrDefault();

            //Users.PassWord = Users.PassWord.GetMD5();
            //Users.MobileState = 0;
            //Users.EmailState = 0;
            //Users.CardStae = 0;
            //Users.State = 0;
            //Users.Amount = 0;
            //Users.Frozen = 0;
            //Users.AddTime = DateTime.Now;
            //Users.PayPwd = "";
            //Users.Agent = Card.AId;
            //Users.AId = Card.AdminId;
            //Entity.Users.AddObject(Users);
            //Entity.SaveChanges();
            //if (!Users.Id.IsNullOrEmpty()) {
            //    Card.State = 2;
            //    Entity.SaveChanges();
            //}
            ////自动开通
            //IList<PayConfig> PCList = Entity.PayConfig.Where(n => n.State == 1).ToList();
            //foreach (var PC in PCList)
            //{
            //    UserPay UserPay = new UserPay();
            //    UserPay.UId = Users.Id;
            //    UserPay.PId = PC.Id;
            //    UserPay.Cost = (double)PC.CostUser;
            //    Entity.UserPay.AddObject(UserPay);
            //}
            //SysSet Sys = Entity.SysSet.FirstOrDefault();
            //Users.Cash0 = Sys.Cash0;
            //Users.ECash0 = Sys.ECash0;
            //Users.Cash1 = Sys.Cash1;
            //Users.ECash1 = Sys.ECash1;
            //Users.State = 1;
            //Entity.SaveChanges();
            ////自动开通End
            //DataObj.Data = "";
            //DataObj.Code = "0000";
            //DataObj.OutString();
        }
    }
}

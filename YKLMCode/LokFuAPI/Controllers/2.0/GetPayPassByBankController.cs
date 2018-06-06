using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using LokFu;
using LokFu.Repositories;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using LokFu.Extensions;
using System.Configuration;
using System.Data.Objects;
using LokFu.Repositories.SqlServer;
using LokFu.Infrastructure;
using System.Web;
using System.Text.RegularExpressions;

namespace LokFu.Controllers
{
    public class GetPayPassByBankController : InitController
    {
        public GetPayPassByBankController()
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
                Log.Write("[GetPayPassByBank]:", "【Data】" + Data, Ex);
            }
            if (json == null) {
                DataObj.OutError("1000");
                return;
            }


            Users Users = new Users();
            UserAuth UA = new UserAuth();
            Users = JsonToObject.ConvertJsonToModel(Users, json);
            UA = JsonToObject.ConvertJsonToModel(UA, json);

            if (Users.UserName.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            if (Users.X.IsNullOrEmpty() || Users.Y.IsNullOrEmpty())
            {
                //DataObj.OutError("1000");
                //return;
            }
            ////手机号码黑名单验证
            //if (Entity.UserBlackList.FirstOrDefault(UBL => UBL.CardNumber == Users.UserName && UBL.State == 1) != null)
            //{
            //    //提示暂不支持您手机号入网
            //    DataObj.OutError("2026");
            //    return;
            //}
            //if (Entity.UserBlackList.FirstOrDefault(UBL => UBL.CardNumber == Users.CardId && UBL.State == 2) != null)
            //{
            //    //提示暂不支持您手机号入网
            //    DataObj.OutError("2027");
            //    return;
            //}
            Users BaseUsers = Entity.Users.FirstOrDefault(n => n.UserName == Users.UserName);
            if (BaseUsers == null)//用户不存在
            {
                DataObj.OutError("2001");
                return;
            }
            if (BaseUsers.State != 1)
            {
                DataObj.OutError("2003");
                return;
            }
            if (BaseUsers.CardStae != 2)//未实名认证
            {
                DataObj.OutError("2006");
                return;
            }
            if (BaseUsers.MiBao != 1)
            {
                DataObj.OutError("2008");
                return;
            }

            SysSet SysSet = Entity.SysSet.FirstOrNew();
            if (SysSet.AuthType != 3 && SysSet.AuthType != 2)
            {
                DataObj.OutError("2061");//----------------
                return;
            }
            if (BaseUsers.Amount < SysSet.AuthPrice)
            {
                DataObj.OutError("6026");
                return;
            }

            UserAuth UserAuth = new UserAuth();

            UserAuth.UId=BaseUsers.Id;
            UserAuth.AccountName=Users.TrueName;
            UserAuth.IdentityCode=Users.CardId;
            UserAuth.BankAccount=Users.CardNum;
            UserAuth.Mobile=Users.Mobile;
            UserAuth.CVV=UA.CVV;
            UserAuth.EndDate=UA.EndDate;
            UserAuth.AddTime=DateTime.Now;

            UserTrack UserTrack = new UserTrack();
            UserTrack = JsonToObject.ConvertJsonToModel(UserTrack, json);

            //处理走的通道及支持验证类型
            string HaoFu_Auth_Open = ConfigurationManager.AppSettings["HaoFu_Auth_Open"].ToString();
            byte CardItemNum = 0;
            if (Equipment.RqType == "Apple")
            {
                CardItemNum = SysSet.IosSet10;
            }
            else {
                CardItemNum = SysSet.ApkSet10;
            }
            if (HaoFu_Auth_Open != "true") {
                CardItemNum = 6;//兼容直连六要素接口
            }
            if (CardItemNum == 6)
            {
                if (UserAuth.BankAccount.IsNullOrEmpty() || UserAuth.AccountName.IsNullOrEmpty() || UserAuth.IdentityCode.IsNullOrEmpty() || UserAuth.Mobile.IsNullOrEmpty() || UserAuth.CVV.IsNullOrEmpty() || UserAuth.EndDate.IsNullOrEmpty())
                {
                    DataObj.OutError("1000");
                    return;
                }
            }
            else if (CardItemNum == 4)
            {
                if (UserAuth.BankAccount.IsNullOrEmpty() || UserAuth.AccountName.IsNullOrEmpty() || UserAuth.IdentityCode.IsNullOrEmpty() || UserAuth.Mobile.IsNullOrEmpty())
                {
                    DataObj.OutError("1000");
                    return;
                }
            }
            else if (CardItemNum == 3)
            {
                if (UserAuth.BankAccount.IsNullOrEmpty() || UserAuth.AccountName.IsNullOrEmpty() || UserAuth.IdentityCode.IsNullOrEmpty())
                {
                    DataObj.OutError("1000");
                    return;
                }
            }
            else
            {
                DataObj.OutError("1000");
                return;
            }

            //实名信息验证，支付密码密段是实名认证的
            if (Users.TrueName != BaseUsers.TrueName) { 
                DataObj.OutError("2011");//2011	真实姓名不正确
                return;
            }
            if (Users.CardId != BaseUsers.CardId)
            {
                DataObj.OutError("2012");//2012	身份证不正确
                return;
            }

            if (!UserAuth.EndDate.IsNullOrEmpty())
            {
                //处理年月问题传上来是MMYY
                //20151112调整成YYMM
                if (UserAuth.EndDate.Length == 4)
                {
                    string MM = UserAuth.EndDate.Substring(0, 2);
                    string YY = UserAuth.EndDate.Substring(2, 2);
                    UserAuth.EndDate = YY + MM;
                }
            }
            UserAuth.AuthType=1;
            UserAuth.AuthPrice=SysSet.AuthPrice;
            UserAuth.IsCharge = 0;
            Entity.UserAuth.AddObject(UserAuth);
            Entity.SaveChanges();
            Entity.Refresh(RefreshMode.StoreWins, UserAuth);

            int USERSID = UserAuth.UId.GetValueOrDefault();
            string TNUM = UserAuth.OId;
            decimal PAYMONEY = UserAuth.AuthPrice;
            string SP_Ret = Entity.SP_UsersMoney(USERSID, TNUM, PAYMONEY, 2, "鉴权取回支付密码");
            if (SP_Ret != "3")
            {
                DataObj.OutError("8888");
                return;
            }

            string ret_code = "";
            string ret_msg = "";
            string isCharge = "";

            string CONTENT = "";
            if(HaoFu_Auth_Open =="true"){
                //提交到结算中心去认证
                string HaoFu_Auth_MerId = ConfigurationManager.AppSettings["HaoFu_Auth_MerId"].ToString();
                string HaoFu_Auth_MerKey = ConfigurationManager.AppSettings["HaoFu_Auth_MerKey"].ToString();
                string HaoFu_Auth_Url = ConfigurationManager.AppSettings["HaoFu_Auth_Url"].ToString();

                string data = "{\"action\":\"authuser\",\"merid\":\"" + HaoFu_Auth_MerId + "\",\"orderid\":\"" + UserAuth.OId + "\",\"bankaccount\":\"" + UserAuth.BankAccount + "\",\"accountname\":\"" + UserAuth.AccountName + "\",\"identitycode\":\"" + UserAuth.IdentityCode + "\",\"mobile\":\"" + UserAuth.Mobile + "\",\"cvv\":\"" + UserAuth.CVV + "\",\"enddate\":\"" + UserAuth.EndDate + "\"}";
                string DataBase64 = LokFuEncode.Base64Encode(data, "utf-8");
                string Sign = (DataBase64 + HaoFu_Auth_MerKey).GetMD5();

                DataBase64 = HttpUtility.UrlEncode(DataBase64, Encoding.UTF8);
                string postdata = "req=" + DataBase64 + "&sign=" + Sign;

                CONTENT = Utils.PostRequest(HaoFu_Auth_Url, postdata, "utf-8");

                JObject JS = new JObject();
                try
                {
                    JS = (JObject)JsonConvert.DeserializeObject(CONTENT);
                }
                catch (Exception Ex)
                {
                    Log.Write("[UsersTrueNameByAPI]:", "【CONTENT】" + CONTENT, Ex);
                }
                if (JS == null)
                {
                    DataObj.OutError("2021");
                    return;
                }
                string resp = JS["resp"].ToString();
                CONTENT = LokFuEncode.Base64Decode(resp, "utf-8");
                try
                {
                    JS = (JObject)JsonConvert.DeserializeObject(CONTENT);
                }
                catch (Exception Ex)
                {
                    Log.Write("[UsersTrueNameByAPI]:", "【CONTENT2】" + CONTENT, Ex);
                }
                if (JS == null)
                {
                    DataObj.OutError("2021");
                    return;
                }
                ret_code = JS["respcode"].ToString();
                ret_msg = JS["respmsg"].ToString();
                if (ret_code == "0000")
                {
                    isCharge = JS["ischarge"].ToString();
                }
                if (isCharge == "1")
                {
                    UserAuth.IsCharge = 1;
                }
                else
                {
                    UserAuth.IsCharge = 0;
                }
            }
            UserAuth.RetCode = ret_code;
            UserAuth.RetMsg = ret_msg;
            UserAuth.RetLog = CONTENT;

            if (ret_code != "0000")
            {
                DataObj.OutError("2021");
                return;
            }
            //=======================================
            UserTrack.ENo = DataObj.ENo;
            UserTrack.OPType = "鉴权取回支付密码";
            UserTrack.GPSAddress = Users.RegAddress;
            UserTrack.GPSX = Users.X;
            UserTrack.GPSY = Users.Y;
            BaseUsers.SeavGPSLog(UserTrack, Entity);
            //=======================================
            Entity.SaveChanges();

            DateTime now = DateTime.Now;
            Guid Gid = Guid.NewGuid();
            string mdstr = Users.Id + "|" + Users.UserName + "|" + Gid.ToString() + "|" + now.ToString();
            string taken = mdstr.GetMD5();
            BaseUsers.Token = "pppp" + taken;

            Entity.SaveChanges();
            BaseUsers.Cols = "Token";
            DataObj.Data = BaseUsers.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}

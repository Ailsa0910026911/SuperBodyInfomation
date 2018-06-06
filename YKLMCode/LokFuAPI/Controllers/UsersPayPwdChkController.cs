using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using LokFu;
using LokFu.Repositories;
using LokFu.Extensions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace LokFu.Controllers
{
    public class UsersPayPwdChkController : InitController
    {
        public UsersPayPwdChkController()
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
                Log.Write("[UsersPayPwdChk]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);
            if (Users.PassWord.IsNullOrEmpty())//使用PassWord字段
            {
                DataObj.OutError("1000");
                return;
            }
            if (Users.PassWord.Length < 6)//6位及以上
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
            if (baseUsers.CardStae != 2)
            { //未实名认证
                DataObj.OutError("2006");
                return;
            }
            if (baseUsers.MiBao != 1)
            { //未设置支付密码
                DataObj.OutError("2008");
                return;
            }
            if (baseUsers.PayLock == 1)//密码错误太多次锁定
            {
                DataObj.OutError("2050");
                return;
            }

            //这里是执行指纹解锁
            bool IfCheckOk = true;
            if (Users.PassWord.Substring(0, 3) == "HF_")
            {
                string PassWord = Users.PassWord;
                PassWord = PassWord.Replace("HF_", "");
                string Token = baseUsers.Token;
                Token = Token + "GoodPay";
                string Md5Token = Token.GetMD5().ToUpper();
                string Pass = Md5Token.Substring(0, 4) + Md5Token.Substring(Md5Token.Length - 4, 4);
                if (Pass != PassWord)
                {
                    IfCheckOk = false;
                }
            }
            else if (baseUsers.PayPwd != Users.PassWord.GetPayMD5())
            {
                //原支付密码错误
                IfCheckOk = false;
            }
            if (!IfCheckOk)
            {
                //付密码错误
                SysSet SysSet = Entity.SysSet.FirstOrNew();
                //系统统一修改标识SAME002
                baseUsers.PayErr++;
                if (baseUsers.PayErr >= SysSet.PayLock)
                {
                    baseUsers.PayLock = 1;
                }
                Entity.SaveChanges();
                Users Out = new Users();
                Out.PayErr = SysSet.PayLock - baseUsers.PayErr;
                Out.Cols = "PayErr";
                DataObj.Data = Out.OutJson();
                //DataObj.Code = "2010";
                //DataObj.OutString();
                DataObj.Code = "2002";
                if (Out.PayErr == 0)
                {
                    DataObj.Msg = "用户支付密码不正确，请明日再试或取回支付密码";
                }
                else
                {
                    DataObj.Msg = "用户支付密码不正确，您还可以尝试" + Out.PayErr + "次";
                }
                DataObj.OutString();
                return;
            }

            baseUsers.PayErr = 0;
            Entity.SaveChanges();

            DataObj.Data = "";
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}

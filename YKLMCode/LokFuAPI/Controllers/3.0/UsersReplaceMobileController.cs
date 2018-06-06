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
using System.Data.Objects;

namespace LokFu.Controllers
{
    public class UsersReplaceMobileController : InitController
    {
        public UsersReplaceMobileController()
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
                Log.Write("[UsersReplaceMobile]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);

            UserTrack UserTrack = new UserTrack();
            UserTrack = JsonToObject.ConvertJsonToModel(UserTrack, json);

            if (Users.PayPwd.IsNullOrEmpty())//使用PayPwd字段
            {
                DataObj.OutError("1000");
                return;
            }
            if (Users.X.IsNullOrEmpty() || Users.Y.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }

            //手机号码黑名单验证
            if (Entity.UserBlackList.FirstOrDefault(UBL => UBL.CardNumber == Users.UserName && UBL.State == 1) != null)
            {
                //提示暂不支持您手机号入网
                DataObj.OutError("2026");
                return;
            }
            if (Users.PayPwd.Length < 6)//6位及以上
            {
                DataObj.OutError("1000");
                return;
            }

            #region 校验
            //获取用户信息
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Users.Token);
            if (baseUsers == null)//用户令牌不存在
            {
                DataObj.OutError("2004");
                return;
            }
            if (baseUsers.SAId != 0)
            {
                DataObj.OutError("2054");
                return;
            }
            if (baseUsers.State != 1)//用户被锁定
            {
                DataObj.OutError("2003");
                return;
            }
            if (baseUsers.MiBao != 1)//未设置支付密码
            {
                DataObj.OutError("2008");
                return;
            }

            SysSet SysSet = Entity.SysSet.FirstOrNew();

            //这里是执行指纹解锁
            bool IfCheckOk = true;
            if (Users.PayPwd.Substring(0, 3) == "HF_")
            {
                string PassWord = Users.PayPwd;
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
            else if (baseUsers.PayPwd != Users.PayPwd.GetPayMD5())
            {
                //原支付密码错误
                IfCheckOk = false;
            }
            if (!IfCheckOk)
            {
                //付密码错误
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
                DataObj.Code = "2010";
                DataObj.OutString();
                return;
            }

            baseUsers.PayErr = 0;
            Entity.SaveChanges();

            Users oldUsers = Entity.Users.FirstOrDefault(n => n.UserName == Users.Mobile);
            if (oldUsers != null)//用户已存在
            {
                DataObj.OutError("2005");
                return;
            }

            //手机验证码
            //失效之前获取验证码
            SMSCode SMSCode = Entity.SMSCode.OrderByDescending(n => n.Id).FirstOrDefault(n => n.UId == baseUsers.Id && n.CType == 4 && n.Code == Users.Code);
            if (SMSCode == null)
            {
                DataObj.OutError("2033");
                return;
            }
            
            if (SMSCode.State != 1)
            {
                DataObj.OutError("2034");
                return;
            }
            if (SMSCode.AddTime.AddMinutes(SysSet.SMSActives) < DateTime.Now)
            {
                DataObj.OutError("2034");
                return;
            }
            #endregion

            //修改用户信息
            baseUsers.UserName = Users.UserName;
            baseUsers.Mobile = Users.UserName;
            SMSCode.State = 2;

            //添加跟踪信息
            //=======================================
            UserTrack.ENo = DataObj.ENo;
            UserTrack.OPType = "更换手机号";
            UserTrack.GPSAddress = Users.RegAddress;
            UserTrack.GPSX = Users.X;
            UserTrack.GPSY = Users.Y;
            baseUsers.SeavGPSLog(UserTrack, Entity);
            //=======================================
            
            DataObj.Data = "";
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}

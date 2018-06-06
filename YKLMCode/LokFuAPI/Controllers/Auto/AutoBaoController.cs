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
    public class AutoBaoController : InitController
    {
        public AutoBaoController()
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
            DataObj.OutError("1020");
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
                Log.Write("[AutoBao]:", "【Data】" + Data, Ex);
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

            if (Users.PassWord.IsNullOrEmpty() || Users.Token.IsNullOrEmpty())//使用PassWord字段
            {
                DataObj.OutError("1000");
                return;
            }
            if (Users.PassWord.Length < 6)//6位及以上
            {
                DataObj.OutError("1000");
                return;
            }
            if (Users.AutoBao != 1 && Users.AutoBao != 0) { 
                DataObj.OutError("1000");
                return;
            }
            if (Users.X.IsNullOrEmpty() || Users.Y.IsNullOrEmpty())
            {
                //DataObj.OutError("1000");
                //return;
            }


            #region 校验
            //获取用户信息
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Users.Token);
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
            if (baseUsers.MiBao != 1)//未设置支付密码
            {
                DataObj.OutError("2008");
                return;
            }
            if (baseUsers.AutoCash == 1)
            {
                DataObj.OutError("6072");
                return;
            }
            if (baseUsers.PayLock == 1)//密码错误太多次锁定
            {
                DataObj.OutError("2050");
                return;
            }
            if (baseUsers.StopPayState == 2)//禁止支付
            {
                DataObj.OutError("6060");
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
                SysSet SysSet = Entity.SysSet.FirstOrNew();
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
            #endregion
            //修改用户信息
            baseUsers.AutoBao = Users.AutoBao;

            Entity.SaveChanges();

            string optype = "关闭";
            if (Users.AutoBao == 1)
            {
                optype = "开启";
            }

            //添加跟踪信息
            //=======================================
            UserTrack.ENo = DataObj.ENo;
            UserTrack.OPType = optype + "余额自动输入余额生息";
            UserTrack.GPSAddress = Users.RegAddress;
            UserTrack.GPSX = Users.X;
            UserTrack.GPSY = Users.Y;
            baseUsers.SeavGPSLog(UserTrack, Entity);
            //=======================================
            
            DataObj.Data = "";
            DataObj.Code = "0000";
            DataObj.OutString();
           */
        }
    }
}

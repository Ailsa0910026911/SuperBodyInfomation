using LokFu.Extensions;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Text;
using System.Web;
namespace LokFu.Controllers
{
    public class JobPayController : InitController
    {
        public JobPayController()
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
                Log.Write("[JobPay]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            JobOrders JobOrders = new JobOrders();
            JobOrders = JsonToObject.ConvertJsonToModel(JobOrders, json);

            UserTrack UserTrack = new UserTrack();
            UserTrack = JsonToObject.ConvertJsonToModel(UserTrack, json);
            if (UserTrack.X.IsNullOrEmpty() || UserTrack.Y.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }

            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);
            #region 初始化与校验
            if ( JobOrders.TNum.IsNullOrEmpty() || JobOrders.CardId.IsNullOrEmpty() || Users.PayPwd.IsNullOrEmpty() || Users.Token.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            #region 用户验证
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
            if (baseUsers.MiBao != 1)//未设置支付密码
            {
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
            if (Users.PayPwd.Substring(0, 3) == "HF_")
            {
                string PassWord = Users.PayPwd;
                PassWord = PassWord.Replace("HF_", "");
                string token = baseUsers.Token;
                token = token + "GoodPay";
                string Md5Token = token.GetMD5().ToUpper();
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
                DataObj.Code = "2010";
                if (Out.PayErr == 0)
                {
                    DataObj.Msg = "支付密码不正确，请明日再试或取回支付密码";
                }
                else
                {
                    DataObj.Msg = "支付密码不正确，您还可以尝试" + Out.PayErr + "次";
                }
                DataObj.OutString();
                return;
            }
            baseUsers.PayErr = 0;
            #endregion

            JobOrders baseJobOrders = this.Entity.JobOrders.Where(o => o.TNum == JobOrders.TNum && o.UId == baseUsers.Id).FirstOrDefault();
            if (baseJobOrders== null)
            {
                DataObj.OutError("1001");
                return;
            }
            if (baseJobOrders.State != 1)
            {
                DataObj.OutError("1001");
                return;
            }
            JobItem FirstJobItem = this.Entity.JobItem.Where(o => o.TNum == baseJobOrders.TNum && o.RunType == 1).OrderBy(o => o.RunTime).FirstOrDefault();
            if (FirstJobItem == null)
            {
                DataObj.OutError("1001");
                return;
            }
            FirstJobItem.UserCardId = JobOrders.CardId;
            FirstJobItem.RunedTime = DateTime.Now;
            FirstJobItem.State = 2;//按Job的流程，这里需设置执行中才能进入支付
            this.Entity.SaveChanges();
            #endregion
            //=======================================
            UserTrack.ENo = DataObj.ENo;
            UserTrack.OPType = "支付还款订单";
            UserTrack.GPSAddress = UserTrack.GPSAddress;
            UserTrack.GPSX = UserTrack.X;
            UserTrack.GPSY = UserTrack.Y;
            UserTrack.UserName = FirstJobItem.TNum;
            UserTrack.UId = baseUsers.Id;
            UserTrack.SeavGPSLog(Entity);
            //=======================================
            FirstJobItem = FirstJobItem.Pay(Entity);
            //0取消 1待执行 2执行中 3执行完成 4执行失败
            if (FirstJobItem.State == 3)
            {
                FirstJobItem.RunTime = DateTime.Now;
                Entity.SaveChanges();
                DataObj.OutError("0000");
            }
            else if (FirstJobItem.State == 2) {
                DataObj.OutError("6028");
            }
            else
            {
                //DataObj.Msg = FirstJobItem.Remark;
                DataObj.OutError("6027");
            }
        }
    }

}

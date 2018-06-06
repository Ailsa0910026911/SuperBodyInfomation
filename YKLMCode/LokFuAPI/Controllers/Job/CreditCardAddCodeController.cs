using LokFu.Extensions;
using LokFu.HFJS;
using LokFu.HFJS.HFJSModels;
using LokFu.HFJS.HFJSResults;
using LokFu.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LokFu.Controllers
{
    /// <summary>
    /// 添加信用卡短信
    /// </summary>
    public class CreditCardAddCodeController : InitController
    {
        public CreditCardAddCodeController()
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

            UserTrack UserTrack = new UserTrack();
            UserTrack = JsonToObject.ConvertJsonToModel(UserTrack, json);
            if (UserTrack.X.IsNullOrEmpty() || UserTrack.Y.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            #region 基础验证
            //获取用户信息
            string Token = UserCard.Token;
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Token);
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
            if (baseUsers.CardStae != 2)//未实名认证
            {
                DataObj.OutError("2006");
                return;
            }
            string cardbin = UserCard.Card.Substring(0, 6);
            BasicCardBin BasicCardBin = this.Entity.BasicCardBin.Where(o => o.BIN == cardbin).FirstOrDefault();
            if (BasicCardBin != null)
            {
                if (BasicCardBin.CardType != 2)
                {
                    DataObj.Msg = "请使用信用卡绑定";
                    DataObj.OutError("1000");
                    return;
                }
            }
            else
            {
                DataObj.OutError("1103");
                return;
            }
            #endregion
            //=======================================
            UserTrack.ENo = DataObj.ENo;
            UserTrack.OPType = "信用卡验证码";
            UserTrack.GPSAddress = UserTrack.GPSAddress;
            UserTrack.GPSX = UserTrack.X;
            UserTrack.GPSY = UserTrack.Y;
            baseUsers.SeavGPSLog(UserTrack, Entity);
            //=======================================

            JobPayWay JobPayWay = Entity.JobPayWay.Where(n => n.State == 1 && n.GroupType == "Pay").OrderBy(n => n.Sort).FirstOrDefault();//目前只支持一条
            if (JobPayWay == null) {
                DataObj.Msg = "暂无可用通道";
                DataObj.OutError("1000");
                return;
            }
            if (UserCard.ValidYear.Length == 4)
            {
                UserCard.ValidYear = UserCard.ValidYear.Substring(2, 2);
            }
            #region 第三方接口
            string[] JobPayWayArr = JobPayWay.QueryArray.Split(',');
            bool Result = false;
            string RetMsg = "";
            if (JobPayWay.DllName == "HLBPay" && JobPayWayArr.Length == 2)
            {
                #region 合利宝
                string MerId = JobPayWayArr[0];
                string MerKey = JobPayWayArr[1];
                string postUrl = "http://pay.trx.helipay.com/trx/quickPayApi/interface.action";

                string orderId = Guid.NewGuid().ToString("N");
                Dictionary<string, string> map = new Dictionary<string, string>();
                map.Add("P1_bizType", "QuickPayBindCardValidateCode");
                map.Add("P2_customerNumber", MerId);
                map.Add("P3_userId", "HF_" + baseUsers.Id.ToString());
                map.Add("P4_orderId", orderId);
                map.Add("P5_timestamp", DateTime.Now.ToString("yyyyMMddHHmmss"));
                map.Add("P6_cardNo", UserCard.Card);
                map.Add("P7_phone", UserCard.Mobile);

                //签名串，把参数值拼接          
                string data = Utils.CreateLinkString(map, false);
                //MD5签名KEY
                string sign = ("&" + data + "&" + MerKey).GetMD5();
                map.Add("sign", sign);
                string send_data = Utils.CreateLinkString(map);
                string RetString = Utils.PostRequest(postUrl, send_data, "utf-8");
                //================================================
                //这里记录日志
                JobLog JobLog = new JobLog();
                JobLog.PayWay = JobPayWay.Id;
                JobLog.ReqNo = orderId;
                JobLog.TNum = "";
                JobLog.Trade = "";
                JobLog.Amount = 0;
                JobLog.Way = "SendCode";
                JobLog.AddTime = DateTime.Now;
                JobLog.Data = RetString;
                JobLog.State = 1;
                Entity.JobLog.AddObject(JobLog);
                Entity.SaveChanges();
                //================================================
                JObject obj = new JObject();
                try
                {
                    obj = (JObject)JsonConvert.DeserializeObject(RetString);
                }
                catch (Exception)
                {
                    Utils.WriteLog("[CreditCardAddCode]:" + RetString, "HLBPayError");
                    DataObj.OutError("1000");
                    return;
                }
                string rt2_retCode = obj["rt2_retCode"].ToString();
                string rt3_retMsg = obj["rt3_retMsg"].ToString();
                if (rt2_retCode == "0000")
                {
                    Result = true;
                }
                else
                {
                    Result = false;
                    RetMsg = rt3_retMsg;
                    Utils.WriteLog("[CreditCardAddCode]:" + RetString + "【" + send_data + "】", "HLBPayError");
                }
                #endregion
            }else if (JobPayWay.DllName == "GHTPay" && JobPayWayArr.Length == 3)
            {
                #region
                //检测是否已开通商户
                DateTime Today = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                SysSet SysSet = Entity.SysSet.FirstOrNew();
                //统计今天已经发送注册验证码次数
                int Times = Entity.SMSCode.Count(n => n.UId == baseUsers.Id && n.Mobile == UserCard.Mobile && n.CType == 31 && n.AddTime >= Today);
                if (Times >= SysSet.SMSTimes)
                {
                    DataObj.Msg = "获取验证码超过" + SysSet.SMSTimes + "次，请明天再试。";
                    DataObj.OutError("1000");
                    return;
                }
                if (Times > 0)
                {
                    //第一次发送不获取，以节少系统资源
                    SMSCode SMSCode = Entity.SMSCode.Where(n => n.UId == baseUsers.Id && n.Mobile == UserCard.Mobile && n.CType == 31 && n.AddTime >= Today).OrderByDescending(n => n.Id).FirstOrDefault();
                    if (SMSCode.AddTime.AddMinutes(1) >= DateTime.Now)
                    {
                        //最后一次发送到现在不足1分钟
                        DataObj.Msg = "您操作太快了。";
                        DataObj.OutError("1000"); ;
                        return;
                    }
                }
                //失效之前获取验证码
                IList<SMSCode> List = Entity.SMSCode.Where(n => n.UId == baseUsers.Id && n.Mobile == UserCard.Mobile && n.CType == 31 && n.State == 1).ToList();
                foreach (var p in List)
                {
                    p.State = 0;
                }
                Entity.SaveChanges();

                //生成验证码
                string Code = Utils.RandomSMSCode(4);
                SMSCode SSC = new SMSCode();
                SSC.CType = 31;
                SSC.UId = baseUsers.Id;
                SSC.Mobile = UserCard.Mobile;
                SSC.Code = Code;
                SSC.AddTime = DateTime.Now;
                SSC.State = 1;
                Entity.SMSCode.AddObject(SSC);
                Entity.SaveChanges();

                string Info = "您正在授权尾号{2}开通授权交易，验证码为{0}，为了保护您的账户安全，验证码请勿转发他人，有效时间{1}分钟。";
                Info = string.Format(Info, Code, SysSet.SMSActives, UserCard.Card.Substring(UserCard.Card.Length - 4, 4));
                Info += "【好支付】";
                SMSLog SMSLog = new SMSLog();
                SMSLog.UId = baseUsers.Id;
                SMSLog.Mobile = UserCard.Mobile;
                SMSLog.SendText = Info;
                SMSLog.State = 1;
                SMSLog.AddTime = DateTime.Now;
                Entity.SMSLog.AddObject(SMSLog);
                Entity.SaveChanges();

                Result = true;

                #endregion
            }
            else if (JobPayWay.DllName == "HFJSPay" && JobPayWayArr.Length == 3)
            {
                #region 结算系统
                string Code = JobPayWayArr[0];
                string CodeKey = JobPayWayArr[1];
                string PayWayCode = JobPayWayArr[2];
                #region 进件
                JobUserPay JobUserPay = Entity.JobUserPay.FirstOrDefault(n => n.UId == baseUsers.Id && n.PayWay == JobPayWay.Id);
                if (JobUserPay == null)
                {
                    JobUserPay = new JobUserPay();
                    JobUserPay.UId = baseUsers.Id;
                    JobUserPay.PayWay = JobPayWay.Id;
                    JobUserPay.AddTime = DateTime.Now;
                    JobUserPay.MerState = 2;//状态 0锁定 1正常 2待提交 3审核中 4审核失败
                    JobUserPay.CardState = 2;//状态 0锁定 1正常 2待提交 3审核中 4审核失败
                    JobUserPay.BusiState = 2;//状态 1正常 2待提交 3审核中 4审核失败
                    Entity.JobUserPay.AddObject(JobUserPay);
                    Entity.SaveChanges();
                }
                if (JobUserPay.MerState == 2 || JobUserPay.MerState == 4)
                {
                    fastuseraddModel model = new fastuseraddModel()
                    {
                        code = Code,
                        mchid = "HF" + baseUsers.Id.ToString(),
                        mchname = baseUsers.NeekName,
                        truename = baseUsers.TrueName,
                        cardno = baseUsers.CardId,
                        accountcard = UserCard.Card,
                        accountbin = BasicCardBin.BankCode,
                        accountmobile = UserCard.Mobile
                    };
                    fastuserResult fastuserResult = HFJSTools.fastuseradd(model, CodeKey);
                    if (fastuserResult.respcode == "00")
                    {
                        if (fastuserResult.state == 1)
                        {
                            JobUserPay.MerState = 1;
                            JobUserPay.CardState = 1;//这里已绑定结算卡
                            JobUserPay.MerId = fastuserResult.merid;
                            JobUserPay.MerKey = fastuserResult.merkey;
                        }
                        else if (fastuserResult.state == 2)
                        {
                            JobUserPay.MerId = fastuserResult.merid;
                            JobUserPay.MerKey = fastuserResult.merkey;
                            JobUserPay.MerState = 3;
                        }
                        else
                        {
                            JobUserPay.MerState = 4;
                        }
                    }
                    else
                    {
                        JobUserPay.MerState = 4;
                        JobUserPay.MerMsg = fastuserResult.respcode + "[" + fastuserResult.respmsg + "]";
                    }
                    Entity.SaveChanges();
                }
                #endregion
                #region 开通道
                if (JobUserPay.MerState == 1 && (JobUserPay.BusiState == 2 || JobUserPay.BusiState == 4))
                {
                    JobSet JobSet = Entity.JobSet.FirstOrNew();//获取配置
                    decimal Cost = JobSet.Cost;//刷卡手续费
                    decimal Cash = JobSet.Cash;//还款手续费
                    userspayopenbModel userspayopenbModel = new userspayopenbModel()
                    {
                        merid = JobUserPay.MerId,
                        paywaycode = PayWayCode,
                        code = Code,
                        bankcost = Cost,
                        surcharge = Cash,
                        cash = 0,
                        bankcostmin = 1.2M,
                        bankcostmax = 9999999
                    };
                    fastuserResult fastuserResult = HFJSTools.userspayopen(userspayopenbModel, CodeKey);
                    if (fastuserResult.respcode == "00")
                    {
                        if (fastuserResult.state == 1)
                        {
                            JobUserPay.BusiState = 1;
                        }
                        else
                        {
                            JobUserPay.BusiState = 4;
                        }
                    }
                    else
                    {
                        JobUserPay.BusiMsg = fastuserResult.respcode + "[" + fastuserResult.respmsg + "]";
                    }
                    Entity.SaveChanges();
                }
                #endregion
                #region 绑卡
                if (JobUserPay.MerState == 1 && JobUserPay.CardState == 1 && JobUserPay.BusiState == 1)
                {
                    fastcardbindModel fastcardbindModel = new fastcardbindModel()
                    {
                        action = "Add",
                        merid = JobUserPay.MerId,
                        bankcard = UserCard.Card,
                        cvv2 = UserCard.CVV,
                        mobile = UserCard.Mobile,
                        month = UserCard.ValidMonth,
                        year = UserCard.ValidYear
                    };
                    ErrorCode errorCode = HFJSTools.fastcardbind(fastcardbindModel, JobUserPay.MerKey);
                    if (errorCode.respcode == "00")
                    {
                        #region 发验证码
                        if (JobUserPay.MerState == 1 && JobUserPay.BusiState == 1 && JobUserPay.CardState == 1)
                        {
                            fastcardcodeModel fastcardcodeModel = new fastcardcodeModel()
                            {
                                bankcard = UserCard.Card,
                                merid = JobUserPay.MerId,
                                paywaycode = PayWayCode
                            };
                            ErrorCode errorcode = HFJSTools.fastcardcode(fastcardcodeModel, JobUserPay.MerKey);
                            if (errorcode.respcode == "00")
                            {
                                Result = true;
                            }
                            else
                            {
                                Result = false;
                                RetMsg = errorcode.respmsg;
                            }
                        }
                        else
                        {
                            Result = false;
                            RetMsg = errorCode.respmsg;
                        }
                        #endregion
                    }
                    else
                    {
                        Result = false;
                        RetMsg = errorCode.respmsg;
                    }
                    Entity.SaveChanges();
                }
                else {
                    Result = false;
                    RetMsg = "商户入驻失败";
                }
                #endregion
                #endregion
            }

            #endregion
            if (Result)
            {
                DataObj.OutError("0000");
            }
            else
            {
                DataObj.Msg = RetMsg;
                DataObj.OutError("1010");
            }
        }
    }
}

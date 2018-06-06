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
using System.Web;

namespace LokFu.Controllers
{
    /// <summary>
    /// 添加信用卡
    /// </summary>
    public class CreditCardAddController : InitController
    {
        public CreditCardAddController()
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

            string Token = UserCard.Token;
            if (!UserCard.Card.IsNullOrEmpty() && !UserCard.ValidYear.IsNullOrEmpty() && !UserCard.ValidMonth.IsNullOrEmpty() && !UserCard.CVV.IsNullOrEmpty() && !UserCard.Mobile.IsNullOrEmpty() && !UserCard.BillDay.IsNullOrEmpty() && !UserCard.BillDay.IsNullOrEmpty() && !UserCard.ValidateCode.IsNullOrEmpty())
            {
                if (UserCard.ValidYear.Length == 4)
                {
                    UserCard.ValidYear = UserCard.ValidYear.Substring(UserCard.ValidYear.Length - 2, 2);
                }
            }
            else
            {
                DataObj.OutError("1000");
                return;
            }
            if (UserCard.Card.Length < 6)
            {
                DataObj.OutError("1000");
                return;
            }

            #region 基础验证
            //获取用户信息
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
            UserBlackList UserBlackList = Entity.UserBlackList.FirstOrDefault(UBL => UBL.CardNumber == UserCard.Card && UBL.State == 3);
            if (UserBlackList != null)
            {
                //提示暂不支持该卡绑定
                DataObj.OutError("2017");
                return;
            }
            UserCard UserCard_ = Entity.UserCard.FirstOrDefault(n => n.UId == baseUsers.Id && n.Card == UserCard.Card && n.Type == 2 && n.State == 1);//信用卡已绑定
            if (UserCard_ != null)
            {
                DataObj.OutError("2015");
                return;
            }

            UserCard.Card = UserCard.Card.Replace(" ", "");
            string cardbin = UserCard.Card.Substring(0, 6);
            var BasicCardBin = this.Entity.BasicCardBin.Where(o => o.BIN == cardbin).FirstOrDefault();
            if (BasicCardBin == null)
            {
                byte UsedCardType = this.GetCardType(UserCard.Card);
                if (UsedCardType == 0)
                {
                    DataObj.OutError("7005");//查询次数用完了
                    return;
                }
                else if (UsedCardType != 2)
                {
                    DataObj.Msg = "请使用信用卡绑定";
                    DataObj.OutError("1000");
                    return;
                }
            }
            else
            {
                if (BasicCardBin.CardType != 2)
                {
                    DataObj.Msg = "请使用信用卡绑定";
                    DataObj.OutError("1000");
                    return;
                }
            }
            BasicBank BasicBank = Entity.BasicBank.FirstOrDefault(n => n.Name == UserCard.Bank);
            if (BasicBank == null)
            {
                DataObj.OutError("1000");
                return;
            }
            else {
                if (BasicBank.BIN.IsNullOrEmpty())
                {
                    DataObj.Msg = "卡BIN未设置";
                    DataObj.OutError("1000");
                    return;
                }
            }
            #endregion

            UserCard.Type = 2;
            UserCard.BId = BasicBank.Id;
            UserCard.Bin = BasicBank.BIN;
            UserCard.UId = baseUsers.Id;
            UserCard.Name = baseUsers.TrueName;

            //=======================================
            UserTrack.ENo = DataObj.ENo;
            UserTrack.OPType = "添加信用卡";
            UserTrack.GPSAddress = UserTrack.GPSAddress;
            UserTrack.GPSX = UserTrack.X;
            UserTrack.GPSY = UserTrack.Y;
            baseUsers.SeavGPSLog(UserTrack, Entity);
            //=======================================
            string RetString = "";//三方接口返回数据
            string bindId = string.Empty;//绑卡需记录字符串
            bool IsSuccess = false;//绑卡状态

            JobPayWay JobPayWay = Entity.JobPayWay.Where(n => n.State == 1 && n.GroupType == "Pay").OrderBy(n => n.Sort).FirstOrDefault();//目前只支持一条
            if (JobPayWay == null)
            {
                DataObj.Msg = "暂无可用通道";
                DataObj.OutError("1000");
                return;
            }
            #region 第三方鉴权
            string[] JobPayWayArr = JobPayWay.QueryArray.Split(',');
            if (JobPayWay.DllName == "HLBPay" && JobPayWayArr.Length == 2)
            {
                #region 合利宝
                string MerId = JobPayWayArr[0];
                string MerKey = JobPayWayArr[1];
                string postUrl = "http://pay.trx.helipay.com/trx/quickPayApi/interface.action";
                string orderId = Guid.NewGuid().ToString("N");
                Dictionary<string, string> map = new Dictionary<string, string>();
                map.Add("P1_bizType", "QuickPayBindCard");
                map.Add("P2_customerNumber", MerId);
                map.Add("P3_userId", "HF_" + baseUsers.Id.ToString()); //用户ID唯一
                map.Add("P4_orderId", orderId);
                map.Add("P5_timestamp", DateTime.Now.ToString("yyyyMMddHHmmss"));
                map.Add("P6_payerName", baseUsers.TrueName);
                map.Add("P7_idCardType", "IDCARD"); //IDCARD：身份证
                map.Add("P8_idCardNo", baseUsers.CardId); //身份证
                map.Add("P9_cardNo", UserCard.Card);//银行卡
                map.Add("P10_year", UserCard.ValidYear); //当银行卡是信用卡时必输 信用卡有效期年
                map.Add("P11_month", UserCard.ValidMonth);//当银行卡是信用卡时必输 信用卡有效期月
                map.Add("P12_cvv2", UserCard.CVV);//当银行卡是信用卡时必输 信用卡有效期月
                map.Add("P13_phone", UserCard.Mobile);
                map.Add("P14_validateCode", UserCard.ValidateCode); //选填 鉴权绑卡短信接口下发给用户的短信

                //签名串，把参数值拼接          
                string data = Utils.CreateLinkString(map, false);
                //MD5签名KEY
                string sign = ("&" + data + "&" + MerKey).GetMD5();
                map.Add("sign", sign);
                if (map["P6_payerName"] != null) //编码
                {
                    map.Remove("P6_payerName");
                    map.Add("P6_payerName", HttpUtility.UrlEncode(baseUsers.TrueName));
                }
                string send_data = Utils.CreateLinkString(map);
                RetString = Utils.PostRequest(postUrl, send_data, "utf-8");
                //================================================
                //这里记录日志
                JobLog JobLog = new JobLog();
                JobLog.PayWay = JobPayWay.Id;
                JobLog.ReqNo = orderId;
                JobLog.TNum = baseUsers.Mobile;
                JobLog.Trade = "";
                JobLog.Amount = 0;
                JobLog.Way = "CardAdd";
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
                    Utils.WriteLog("[CreditCardAdd]:" + RetString, "HLBPayError");
                    DataObj.OutError("1000");
                    return;
                }
                string rt2_retCode = obj["rt2_retCode"].ToString();
                string rt3_retMsg = obj["rt3_retMsg"].ToString();
                if (rt2_retCode == "0000")
                {
                    string rt7_bindStatus = obj["rt7_bindStatus"].ToString();  //绑卡状态结果  /成功/失败
                    if (rt7_bindStatus == "SUCCESS")
                    {
                        IsSuccess = true;
                        bindId = obj["rt10_bindId"].ToString();
                    }
                    else
                    {
                        DataObj.Msg = rt3_retMsg;
                        DataObj.OutError("1010");
                        return;
                    }
                }
                else
                {
                    DataObj.Msg = rt3_retMsg;
                    DataObj.OutError("1010");
                    return;
                }
                #endregion
            }
            else if (JobPayWay.DllName == "HFJSPay" && JobPayWayArr.Length == 3)
            {
                #region 结算系统
                string Code = JobPayWayArr[0];
                string CodeKey = JobPayWayArr[1];
                string PayWayCode = JobPayWayArr[2];
                JobUserPay JobUserPay = Entity.JobUserPay.FirstOrDefault(n => n.UId == baseUsers.Id && n.PayWay == JobPayWay.Id);
                fastcardauthModel fastcardauthModel = new fastcardauthModel()
                {
                    bankcard = UserCard.Card,
                    mcode = UserCard.ValidateCode,
                    merid = JobUserPay.MerId,
                    paywaycode = PayWayCode
                };
                ErrorCode errorCode = HFJSTools.fastcardauth(fastcardauthModel, JobUserPay.MerKey);
                if (errorCode.respcode == "00")
                {
                    IsSuccess = true;
                }
                else
                {
                    DataObj.Msg = errorCode.respmsg;
                    DataObj.OutError("1010");
                    return;
                }
                #endregion
            }
            #endregion
            if (IsSuccess)
            {
                //系统要限制每条通道每张卡只能存在一个授权，所以这里要处理
                UserCardOpen UserCardOpen = Entity.UserCardOpen.FirstOrDefault(n => n.CardNum == UserCard.Card && n.UId == baseUsers.Id && n.PayWay == JobPayWay.Id);
                if (UserCardOpen == null)
                {
                    UserCardOpen = new UserCardOpen()
                    {
                        UId = baseUsers.Id,
                        CardNum = UserCard.Card,
                        Mobile = UserCard.Mobile,
                        PayWay = JobPayWay.Id,
                        State = 1,
                        STime = new DateTime(1990, 1, 1),
                        ETime = new DateTime(2099, 1, 1),
                        Token = bindId,
                        RqData = RetString
                    };
                    Entity.UserCardOpen.AddObject(UserCardOpen);
                }
                else
                {
                    UserCardOpen.Mobile = UserCard.Mobile;
                    UserCardOpen.State = 1;
                    UserCardOpen.Token = bindId;
                    UserCardOpen.RqData = RetString;
                }
                UserCard.Pic = string.Empty;
                UserCard.ScanNo = string.Empty;
                UserCard.State = 1;
                UserCard.Deposit = string.Empty;
                Entity.UserCard.AddObject(UserCard);
                Entity.SaveChanges();
            }
            DataObj.OutError("0000");
        }
    }
}

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
    public class SMSCodeController : InitController
    {
        public SMSCodeController()
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
                Log.Write("[SMSCode]:", "【Data】" + Data, Ex);
            }
            if (json == null) {
                DataObj.OutError("1000");
                return;
            }
            //JObject pp = (JObject)p;
            SMSCode SMSCode = new SMSCode();

            SMSCode = JsonToObject.ConvertJsonToModel(SMSCode, json);
            if (SMSCode.Mobile.IsNullOrEmpty() || SMSCode.CType.IsNullOrEmpty() || SMSCode.Code.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            //手机号码黑名单验证
            if (Entity.UserBlackList.FirstOrDefault(UBL => UBL.CardNumber == SMSCode.Mobile && UBL.State == 1) != null)
            {
                //提示暂不支持您手机号入网
                DataObj.OutError("2026");
                return;
            }
            //手机验证码
            //失效之前获取验证码
            SMSCode baseSMSCode = Entity.SMSCode.OrderByDescending(n => n.Id).FirstOrDefault(n => n.Mobile == SMSCode.Mobile && n.CType == SMSCode.CType && n.Code == SMSCode.Code);
            if (baseSMSCode == null)
            {
                DataObj.OutError("2033");
                return;
            }
            if (baseSMSCode.State != 1)
            {
                DataObj.OutError("2034");
                return;
            }
            SysSet SysSet = Entity.SysSet.FirstOrNew();
            if (baseSMSCode.AddTime.AddMinutes(SysSet.SMSActives) < DateTime.Now)
            {
                DataObj.OutError("2034");
                return;
            }

            DataObj.Data = "";
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}

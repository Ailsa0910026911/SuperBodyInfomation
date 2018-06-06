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
    public class MyQRCodeController : InitController
    {
        public MyQRCodeController()
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
                Log.Write("[MyQRCode]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            QRCode QRCode = new QRCode();
            QRCode = JsonToObject.ConvertJsonToModel(QRCode, json);
            if (QRCode.Token.IsNullOrEmpty())
            { 
                DataObj.OutError("1000");
                return;
            }
            //获取用户信息
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == QRCode.Token);
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
            ///qc.html?n=1000000002#gp_3zt5do
            List<QRCode> QRCodeList = Entity.QRCode.Where(n => n.UId == baseUsers.Id && n.State == 2).OrderByDescending(n => n.State).ThenByDescending(n => n.EditTime).ToList();
            QRCodeList.ForEach(o =>
            {
                o.UrlPam = string.Format("http://i.kkapay.com/qc.html?n={0}#gp_{1}", o.Num, o.Code);
            });
            DataObj.Data = QRCodeList.EntityToJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}

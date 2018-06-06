﻿using System.Collections.Generic;
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
    public class QRCodeBindController : InitController
    {
        public QRCodeBindController()
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
                Log.Write("[QRCodeBind]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            QRCode QRCode = new QRCode();
            QRCode = JsonToObject.ConvertJsonToModel(QRCode, json);
            if (QRCode.Token.IsNullOrEmpty() || QRCode.Code.IsNullOrEmpty() || QRCode.Num.IsNullOrEmpty())
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

            QRCode BaseQRCode = Entity.QRCode.FirstOrDefault(n => n.Num == QRCode.Num && n.Code == QRCode.Code);
            if (BaseQRCode == null)
            {
                //不存在
                DataObj.OutError("2040");
                return;
            }
            if (!BaseQRCode.UId.IsNullOrEmpty())
            {
                //已使用
                DataObj.OutError("2044");
                return;
            }
            if (BaseQRCode.State == 0)
            {
                //失效
                DataObj.OutError("2040");
                return;
            }
            if (BaseQRCode.State == 2)
            {
                //已使用
                DataObj.OutError("2044");
                return;
            }
            BaseQRCode.UId = baseUsers.Id;
            BaseQRCode.State = 2;
            BaseQRCode.EditTime = DateTime.Now;

            string Url = string.Format("{0}/mobile/shop/index-{1}.html", SysPath, baseUsers.Id);
            BaseQRCode.Url = Url;

            Entity.SaveChanges();
            DataObj.Data = "";
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}

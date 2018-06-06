using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using LokFu;
using LokFu.Repositories;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using LokFu.Extensions;

namespace LokFu.Controllers
{
    public class RegEquipmentController : InitController
    {
        public RegEquipmentController()
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
            if (DataObj.IsReg)
            {
                DataObj.OutError("3003");
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
                Log.Write("[Equipment]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }

            Equipment equipment = new Equipment();
            equipment = JsonToObject.ConvertJsonToModel(equipment, json);
            if (equipment.RqType.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            if (equipment.RqType != "Android" && equipment.RqType != "Apple")
            {
                DataObj.OutError("1000");
                return;
            }
            if (equipment.IMEI.IsNullOrEmpty())
            {
                DataObj.OutError("3004");
                return;
            }
            if (equipment.RqType == "Android") {
                //if (!Regex.IsMatch(equipment.IMEI, @"^[0-9]{15,17}$"))
                //{
                //    Utils.WriteLog("IMEI:[" + equipment.IMEI + "]" + Data, "equipment_err");
                //    DataObj.OutError("3004");
                //    return;
                //}
            }
            //if (equipment.MobiType.IsNullOrEmpty())
            //{
            //    Utils.WriteLog("MobiType:[" + equipment.MobiType + "]" + Data, "equipment_err");
            //    DataObj.OutError("3004");
            //    return;
            //}
            //if (equipment.SysVer.IsNullOrEmpty())
            //{
            //    Utils.WriteLog("SysVer:[" + equipment.SysVer + "]" + Data, "equipment_err");
            //    DataObj.OutError("3004");
            //    return;
            //}
            //if (equipment.SoftVer.IsNullOrEmpty())
            //{
            //    Utils.WriteLog("SoftVer:[" + equipment.SoftVer + "]" + Data, "equipment_err");
            //    DataObj.OutError("3004");
            //    return;
            //}
            //if (equipment.SignalType.IsNullOrEmpty())
            //{
            //    Utils.WriteLog("SignalType:[" + equipment.SignalType + "]" + Data, "equipment_err");
            //    DataObj.OutError("3004");
            //    return;
            //}
            //if (equipment.SignalType != "Wifi" && equipment.SignalType != "4G" && equipment.SignalType != "3G" && equipment.SignalType != "2G")
            //{
            //    Utils.WriteLog("SignalType:[" + equipment.SignalType + "]" + Data, "equipment_err");
            //    DataObj.OutError("3004");
            //    return;
            //}

            Equipment Equipment = Entity.Equipment.FirstOrDefault(n => n.IMEI == equipment.IMEI);
            if (Equipment == null)
            {
                if (equipment.RqType == "Apple" && equipment.IMEI.IsNullOrEmpty())
                {//处理苹果空IMEI问题
                    equipment.IMEI = "Apple_none";
                }
                Equipment = new Equipment();
                Equipment.No = Guid.NewGuid().ToString();
                Equipment.Keys = Guid.NewGuid().ToString().GetMD5() + Guid.NewGuid().ToString().GetMD5();
                Equipment.IP = Tools.GetIp();

                Equipment.Mobile = equipment.Mobile;
                Equipment.IfYY = equipment.IfYY;
                Equipment.MobiType = equipment.MobiType;
                Equipment.SysVer = equipment.SysVer;
                Equipment.SoftVer = equipment.SoftVer;
                Equipment.SignalType = equipment.SignalType;

                Equipment.AddTime = DateTime.Now;
                Equipment.IMEI = equipment.IMEI;
                Equipment.RqTimes = 1;
                Equipment.RqType = equipment.RqType;
                Entity.Equipment.AddObject(Equipment);
            }
            else {
                Equipment.IP = Tools.GetIp();
                Equipment.Mobile = equipment.Mobile;
                Equipment.IfYY = equipment.IfYY;
                Equipment.MobiType = equipment.MobiType;
                Equipment.SysVer = equipment.SysVer;
                Equipment.SoftVer = equipment.SoftVer;
                Equipment.SignalType = equipment.SignalType;

                Equipment.RqTimes++;
            }
            Entity.SaveChanges();
            DataObj.Data = Equipment.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
            //Tools.OutString(ErrInfo.Return("0000"));
        }
    }
}

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
using System.Text.RegularExpressions;
using LokFu.Extensions;

namespace LokFu.Controllers
{
    /// <summary>
    /// 用户通讯录API
    /// </summary>
    public class UserMaillistController : InitController
    {
        public UserMaillistController()
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
                Log.Write("[UserMaillist]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            //JObject pp = (JObject)p;
            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Users.Token);
            if (Users.Token.IsNullOrEmpty())
            {
                DataObj.OutError("0000");
                return;
            }
            if (baseUsers == null)//用户令牌不存在
            {
                DataObj.OutError("2004");
                return;
            }

            #region 缓存
            string CashName = Users.Token + "_" + baseUsers.Id.ToString();
            string StringJson = CacheBuilder.EntityCache.Get(CashName, null) as string;
            if (StringJson == "OK")
            {
                DataObj.OutError("0000");
                return;
            }
            CacheBuilder.EntityCache.Remove(CashName, null);
            CacheBuilder.EntityCache.Add(CashName, "OK", DateTime.Now.AddDays(1), null);
            #endregion

            //参数信息错误
            if (string.IsNullOrWhiteSpace(Users.UserName) || string.IsNullOrWhiteSpace(Users.Mobile))
            {
                DataObj.OutError("0000");
                //Utils.WriteLog("参数信息不完整" + "【" + Data + "】", "UserMaillist");
                return;
            }
            //用户名
            string[] nameSplit = Users.UserName.Split(',');
            //手机号
            string[] mobileSplit = Users.Mobile.Split(',');
            //用户跟手机号码列表不匹配
            if (nameSplit.Length != mobileSplit.Length)
            {
                DataObj.OutError("0000");
                Utils.WriteLog("用户跟手机号码列表不匹配" + "【" + Data + "】", "UserMaillist");
                return;
            }
            //有为空的情况
            //if (nameSplit.Count(x => x == "") > 0 || mobileSplit.Count(x => x == "") > 0)
            //{
            //    DataObj.OutError("0000");
            //    Utils.WriteLog("参数信息不完整" + "【" + Data + "】", "UserMaillist");
            //    return;
            //}

            //初始化数据
            var MobileReg = new System.Text.RegularExpressions.Regex(@"^(13[0-9]|14[5|7]|15[0|1|2|3|5|6|7|8|9]|17[0|1|2|6|]|18[0|1|2|3|5|6|7|8|9])\d{8}$");
            var TelephoneReg = new System.Text.RegularExpressions.Regex(@"^(\d{3,4}-?)?\d{7,8}$");//0755-2345678,07552345678
            var ContactsList = new List<Contacts>();
            for (int i = 0; i < mobileSplit.Length; i++)
            {
                string username = nameSplit[i];
                string mobile = mobileSplit[i].Replace("+86", "");
                mobile = mobile.Replace("-", "");
                mobile = mobile.Replace(" ", "");
                if ((MobileReg.IsMatch(mobile) || TelephoneReg.IsMatch(mobile)) && !mobile.IsNullOrEmpty() && !username.IsNullOrEmpty() && mobile.Length <= 200 && username.Length <= 200)
                {
                    ContactsList.Add(new Contacts() { Mobile = mobile, Name = username });
                }
                else if (mobile.Length > 200 || username.Length > 200)
                {
                    Utils.WriteLog("过长数据 mobile:" + mobile + " username:" + username, "UserMaillist");
                }
            }
            //o.IMEI == "" || o.IMEI == null 条件过段时间后可以删除
            var OldUserMaillist = Entity.UserMaillist.Where(o => o.UId == baseUsers.Id && o.State == 1 && ( o.IMEI == Equipment.IMEI || o.IMEI == "" || o.IMEI == null) ).ToList();
            var OldMailList = OldUserMaillist.Select(o => new Contacts() { Mobile = o.Mobile, Name = o.UserName }).ToList();
            //求差集
            var AddExcept = ContactsList.Except(OldMailList).ToList();//添加集
            var DelExcept = OldMailList.Except(ContactsList).ToList();//删除集
            var UpdateList = new List<Contacts>();//更新集
            var UpdateMobile = AddExcept.Select(o => o.Mobile).Intersect(DelExcept.Select(o => o.Mobile)).ToList();
            if (UpdateMobile.Count > 0)
            {
                AddExcept.RemoveAll(o => UpdateMobile.Contains(o.Mobile));
                DelExcept.RemoveAll(o => UpdateMobile.Contains(o.Mobile));
                UpdateList = ContactsList.Where(o => UpdateMobile.Contains(o.Mobile)).ToList();
            }

            //添加
            if (AddExcept.Count > 0)
            {
                foreach (var item in AddExcept)
                {
                    var UserMaillist = new UserMaillist()
                    {
                        AddTime = DateTime.Now,
                        Mobile = item.Mobile,
                        UserName = item.Name,
                        State = 1,
                        UId = baseUsers.Id,
                        UpTime = DateTime.Now,
                        IMEI = Equipment.IMEI,
                    };
                    Entity.UserMaillist.AddObject(UserMaillist);
                }
            }

            //删除
            //if (DelExcept.Count > 0)
            //{
            //    var DelMobile = DelExcept.Select(x => x.Mobile).ToList();
            //    var Ids = OldUserMaillist.Where(o => DelMobile.Contains(o.Mobile)).Select(o => o.Id).ToList();
            //    var IdsStr = string.Join(",", Ids);
            //    string SQL = "Delete  From  UserMaillist Where Id In (" + IdsStr + ")";
            //    Entity.ExecuteStoreCommand(SQL);
            //}
            if (DelExcept.Count > 0)
            {
                var DelMobile = DelExcept.Select(x => x.Mobile).ToList();
                var Ids = OldUserMaillist.Where(o => DelMobile.Contains(o.Mobile)).Select(o => o.Id).ToList();
                var IdsStr = string.Join(",", Ids);
                string SQL = "Update UserMaillist Set State=0 Where Id In (" + IdsStr + ")";
                Entity.ExecuteStoreCommand(SQL);
            }


            //更新
            if (UpdateList.Count > 0)
            {
                foreach (var item in UpdateList)
                {
                    var temp = OldUserMaillist.FirstOrDefault(o => item.Mobile == o.Mobile);
                    if (temp != null)
                    {
                        temp.UserName = item.Name;
                        temp.UpTime = DateTime.Now;
                    }
                }
            }
            var upIMEI = OldUserMaillist.Where(o=>o.IMEI == string.Empty || o.IMEI == null);
            foreach (var item in upIMEI)
            {
                item.IMEI = Equipment.IMEI;
            }
            Entity.SaveChanges();
            
            DataObj.Data = "";
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }

    //实现比较器
    public class Contacts : System.IEquatable<Contacts>
    {
        public string Name { get; set; }
        public string Mobile { get; set; }

        public bool Equals(Contacts other)
        {
            if (System.Object.ReferenceEquals(other, null)) return false;
            if (System.Object.ReferenceEquals(this, other)) return true;
            return Name.Equals(other.Name) && Mobile.Equals(other.Mobile);
        }

        public override int GetHashCode()
        {
            //Get hash code for the Name field if it is not null.
            int hashName = Name == null ? 0 : Name.GetHashCode();

            //Get hash code for the Code field.
            int hashMobile = Mobile.GetHashCode();

            //Calculate the hash code for the product.
            return hashName ^ hashMobile;
        }

    }  
}

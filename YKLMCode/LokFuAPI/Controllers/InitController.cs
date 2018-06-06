using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;

namespace LokFu
{
    public class InitController : ContainerController
    {
        public bool InitState = true;

        public string ApiPath;
        public string ApkPath;
        public string AppPath;
        public string SysPath;
        public string PayPath;
        public string NoticePath;
            
        public string ApiImgPath;
        public string ApkImgPath;
        public string AppImgPath;
        public string SysImgPath;

        public Equipment Equipment;
        public bool HasCache = false;
        public InitController()
        {
            if (!DBState)
            {
                DataObj.OutError("8001");
                InitState = false;
                return;
            }
            string GetStr = System.Web.HttpContext.Current.Request.QueryString.ToString();
            string PostStr = System.Web.HttpContext.Current.Request.Form.ToString();
            string WriteLog = ConfigurationManager.AppSettings["WriteLog"].ToString();
            string ControllerCloseLog = ConfigurationManager.AppSettings["ControllerCloseLog"].ToString();
            if (WriteLog == "true")
            {
                string AbsolutePath = System.Web.HttpContext.Current.Request.Url.AbsolutePath;
                var pathSplit = AbsolutePath.Split('/');
                var controllerName = pathSplit.Count() >= 3 ? pathSplit[2] : "";
                bool islog = true;
                var controllers = ControllerCloseLog.Split(',');
                islog = controllers.Contains(controllerName) ? false : true ;
                if (islog)
                {
                    Log.Write(GetStr, PostStr);
                }
            }
            DataObj.IsReg = false;
            DataObj.ENo = System.Web.HttpContext.Current.Request.Form["eno"];
            DataObj.Data = System.Web.HttpContext.Current.Request.Form["data"];
            DataObj.Code = System.Web.HttpContext.Current.Request.Form["code"];
            DataObj.Key = ConfigurationManager.AppSettings["regkey"].ToString();
            Equipment = new Equipment();
            if (DataObj.ENo != "0")//已注册
            {
                Equipment = Entity.Equipment.FirstOrDefault(n => n.No == DataObj.ENo);
                if (Equipment == null) {
                    DataObj.OutError("3001");
                    return;
                }
                if (DataObj.Code != "0000" && !DataObj.Code.IsNullOrEmpty())
                {
                    string Ver = "";
                    if (DataObj.Code.IndexOf("android_") != -1)
                    {
                        Ver = DataObj.Code.Replace("android_", "");
                    }
                    if (DataObj.Code.IndexOf("apple_") != -1)
                    {
                        Ver = DataObj.Code.Replace("apple_", "");
                    }
                    if (!Ver.IsNullOrEmpty())
                    {
                        if (Ver != Equipment.SoftVer)
                        {
                            Equipment.SoftVer = Ver;
                            Entity.SaveChanges();
                        }
                    }
                }
                //if (Equipment.RqType == "Android")
                //{
                //    DataObj.OutError("3001");
                //    return;
                //}
                DataObj.Key = Equipment.Keys;
                DataObj.IsReg = true;
            }

            AppPath = Utils.GetHost();
            ApiPath = ConfigurationManager.AppSettings["ApiPath"].ToString();
            ApkPath = ConfigurationManager.AppSettings["ApkPath"].ToString();
            SysPath = ConfigurationManager.AppSettings["SysPath"].ToString();
            PayPath = ConfigurationManager.AppSettings["PayPath"].ToString();
            NoticePath = ConfigurationManager.AppSettings["NoticePath"].ToString();
            if (AppPath.IsNullOrEmpty())
            {
                if (Equipment.RqType == "Android")
                {
                    AppPath = ApkPath;
                }
                else if (Equipment.RqType == "Apple")
                {
                    AppPath = ApiPath;
                }
                else {
                    AppPath = string.Empty;
                }
            }
            ApiImgPath = ConfigurationManager.AppSettings["ApiImgPath"].ToString();
            ApkImgPath = ConfigurationManager.AppSettings["ApkImgPath"].ToString();
            SysImgPath = ConfigurationManager.AppSettings["SysImgPath"].ToString();
            if (Equipment.RqType == "Android")
            {
                AppImgPath = ApkImgPath;
            }
            else if (Equipment.RqType == "Apple")
            {
                AppImgPath = ApiImgPath;
            }
            else
            {
                AppImgPath = string.Empty;
            }

            if (ConfigurationManager.AppSettings["Cache"] != null)
            {
                string hascache = ConfigurationManager.AppSettings["Cache"].ToString();
                if (hascache == "true")
                {
                    HasCache = true;
                }
            }
        }
        //无GET参数时返回信息
        public void Get()
        {
            DataObj.OutError("1000");
        }
        #region 查询卡类型
        /// <summary>
        /// 查询卡类型
        /// </summary>
        /// <param name="BankNum">银行卡号</param>
        /// <returns>1:借记卡 2:贷记卡 0:查询失败</returns>
        protected byte GetCardType(string BankNum)
        {
            string HaoFu_Auth_MerId = ConfigurationManager.AppSettings["HaoFu_Auth_MerId"].ToString();
            string HaoFu_Auth_MerKey = ConfigurationManager.AppSettings["HaoFu_Auth_MerKey"].ToString();

            string data = "{\"action\":\"bankcard\",\"merid\":\"" + HaoFu_Auth_MerId + "\",\"bankaccount\":\"" + BankNum + "\"}";
            string DataBase64 = LokFuEncode.Base64Encode(data, "utf-8");
            string Sign = (DataBase64 + HaoFu_Auth_MerKey).GetMD5();

            DataBase64 = HttpUtility.UrlEncode(DataBase64, Encoding.UTF8);
            string postdata = "req=" + DataBase64 + "&sign=" + Sign;

            string CONTENT = Utils.PostRequest("https://api.zhifujiekou.com/api/bankcardtype", postdata);

            JObject JS = new JObject();
            try
            {
                JS = (JObject)JsonConvert.DeserializeObject(CONTENT);
            }
            catch (Exception Ex)
            {
                Log.Write("[GetCardType]:", "【CONTENT】" + CONTENT, Ex);
            }
            if (JS == null)
            {
                return 0;
            }
            string resp = JS["resp"].ToString();
            CONTENT = LokFuEncode.Base64Decode(resp, "utf-8");
            try
            {
                JS = (JObject)JsonConvert.DeserializeObject(CONTENT);
            }
            catch (Exception Ex)
            {
                Log.Write("[GetCardType]:", "【CONTENT2】" + CONTENT, Ex);
            }
            if (JS == null)
            {
                return 0;
            }
            string ret_code = JS["respcode"].ToString();
            if (ret_code == "0000")
            {
                string CardType = JS["cardtype"].ToString();
                if (CardType == "1")
                {
                    return 1;
                }
                if (CardType == "2")
                {
                    return 2;
                }
                return 0;
            }
            else
            {
                return 0;
            }
        }
        #endregion
    }
    public class ContainerController : ApiController
    {
        public LokFuEntity Entity;
        public DataObj DataObj = new DataObj();
        public bool DBState = true;
        public ContainerController()
        {
            Entity = new LokFuEntity(ConfigurationManager.ConnectionStrings["LokFuEntity"].ToString());
            try
            {
                if (!Entity.DatabaseExists())
                {
                    DataObj.OutError("8002");
                    Log.Write("[ContainerController.ContainerController]:", "无法打开数据库链接", null);
                    DBState = false;
                }
                else
                {
                    DBState = true;
                }
            }
            catch (Exception Ex)
            {
                DataObj.OutError("8000");
                Log.Write("[ContainerController.ContainerController]:", "", Ex);
                DBState = false;
                return;
            }
        }
        
        
    }
}
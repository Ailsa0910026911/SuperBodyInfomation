using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace LokFu
{

    public class EntityTools
    {
        /// <summary>
        /// 把接收到得参数复制到实体中
        /// </summary>
        /// <typeparam name="T">实体</typeparam>
        /// <param name="_request"></param>
        /// <param name="_ShopMenu"></param>
        /// <returns></returns>
        public static T ConvertRequestToModel<T>(T _SaveModel, T _ShopMenu)
        {
            System.Web.HttpRequest _request = System.Web.HttpContext.Current.Request;
            Type myType = _ShopMenu.GetType();
            Type saveType = _SaveModel.GetType();
            //复制Post的参数
            for (int i = 0; i < _request.Form.Count; i++)
            {
                if (_request.Form.Keys[i] == null) continue;
                PropertyInfo pinfo = myType.GetProperty(_request.Form.Keys[i]);
                PropertyInfo saveInfo = saveType.GetProperty(_request.Form.Keys[i]);
                if (saveInfo != null)
                {
                    object v = pinfo.GetValue(_ShopMenu, null);
                    try
                    {
                        saveInfo.SetValue(_SaveModel, v, null);
                    }
                    catch (Exception) { }
                }

            }

            //复制Get的参数
            for (int i = 0; i < _request.QueryString.Count; i++)
            {
                if (_request.QueryString.Keys[i] != null)
                {
                    PropertyInfo pinfo = myType.GetProperty(_request.QueryString.Keys[i]);
                    PropertyInfo saveInfo = saveType.GetProperty(_request.QueryString.Keys[i]);
                    if (saveInfo != null)
                    {
                        object v = pinfo.GetValue(_ShopMenu, null);
                        if (v != null)
                        {
                            saveInfo.SetValue(_SaveModel, v, null);
                        }
                    }
                }
            }
            return _SaveModel;
        }
    }
    class PageInfo
    {
        public static string Get(object list)
        {
            int PageIndex, PageSize, TotalCount, TotalPage;
            Type myType = list.GetType();
            PageIndex = Convert.ToInt32(myType.GetProperty("PageIndex").GetValue(list, null));
            PageSize = Convert.ToInt32(myType.GetProperty("PageSize").GetValue(list, null));
            TotalCount = Convert.ToInt32(myType.GetProperty("TotalCount").GetValue(list, null));
            TotalPage = Convert.ToInt32(myType.GetProperty("TotalPage").GetValue(list, null));
            string Ret="";
            Ret=",\"page\":{\"pageindex\":" + PageIndex + ",\"pagesize\":" + PageSize + ",\"totalcount\":" + TotalCount + ",\"totalpage\":" + TotalPage + "}"; 
            return Ret;
        }
    }

    public static class Log
    {
        public static void Write(string URL, string Data, Exception Ex, string ext = "")
        {
            try
            {
                string filename = DateTime.Now.ToString("yyyyMMdd");
                string file = System.Web.HttpContext.Current.Server.MapPath("/log/" + ext + "err_" + filename + ".log");
                System.IO.StreamWriter log = new System.IO.StreamWriter(file, true);
                log.WriteLine("=============================================================================");
                log.WriteLine("TIME:" + System.DateTime.Now.ToLongTimeString());
                log.WriteLine("URL:" + URL);
                log.WriteLine("DATA:" + Data);
                string ErrInfos = "null";
                if (Ex != null) ErrInfos = Ex.ToString();
                log.WriteLine("ErrInfo:" + ErrInfos);
                log.Close();
            }
            catch (Exception) {
                Write(URL, Data, Ex, "Ex_");
            }
        }
        public static void Write(string Get, string Post, string ext = "")
        {
            try
            {
                string filename = DateTime.Now.ToString("yyyyMMdd");
                string file = System.Web.HttpContext.Current.Server.MapPath("/log/" + ext + "log_" + filename + ".log");
                System.IO.StreamWriter log = new System.IO.StreamWriter(file, true);
                log.WriteLine("=============================================================================");
                log.WriteLine("PATH:" + System.Web.HttpContext.Current.Request.Path);
                log.WriteLine("TIME:" + System.DateTime.Now.ToLongTimeString());
                log.WriteLine("GET:" + Get);
                log.WriteLine("POST:" + Post);
                log.Close();
            }
            catch (Exception) {
                Write(Get, Post, "Ex_");
            }
        }
    }
}
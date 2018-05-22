using SBIModel;
using SuperBodyInfomation.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace SuperBodyInfomation.Control
{
    public class MethodController : Controller
    {
        // GET: Method
        public void ExportOrder(string startTime, string endTime, string phone)
        {
            if (startTime != "" && endTime != "")
            {
                DateTime StartTime = DateTime.Parse(startTime);
                DateTime EndTime = DateTime.Parse(endTime);
                using (SBIContext sc = new SBIContext())
                {
                    //查询已经付款成功的订单
                    var model = sc.ordersinfo.Where(o => o.PayStatus == 1).OrderBy(o => o.DateTime).ToList();
                    if (StartTime != null)
                        model = model.Where(o => o.DateTime >= StartTime).ToList();
                    if (EndTime != null)
                        model = model.Where(o => o.DateTime <= EndTime).ToList();
                    if (phone != "")
                        model = model.Where(o => o.Phone == phone).ToList();
                    if (model != null)
                    {
                        try
                        {
                            Dictionary<string, string> cellheader = new Dictionary<string, string> {
                    { "ID", "订单ID" },
                    { "Name", "姓名" },
                    { "Phone", "手机号码" },
                    { "Adress", "地址" },
                };
                            // 3.进行Excel转换操作，并返回转换的文件下载链接
                            string urlPath = ExcelHelper.EntityListToExcel2003(cellheader, model, "Order");
                            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
                            Response.ContentType = "text/plain";
                            Response.Write(js.Serialize(urlPath)); // 返回Json格式的内容
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
            }
        }
    }
}
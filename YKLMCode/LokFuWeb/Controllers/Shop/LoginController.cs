using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Extensions;
using LokFu.Repositories.SqlServer;
using System.Linq;
using System.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Drawing.Imaging;
using ThoughtWorks.QRCode.Codec;
using LokFu.Base;
using System.Configuration;
namespace LokFu.Areas.Shop.Controllers
{
    public class LoginController : InitController
    {
        public ActionResult Index()
        {
            GetQRCodeByRandom();
            return View();
        }
        public void CHKLogin(string userName, string passWord,string Code)
        {
            Response.AddHeader("content-type", "application/json");
            if (Code.ToUpper() != Session.GetCheckCode())
            {
                Response.Write("{\"msg\":\"E0\",\"log\":\"0\"}");
                return;
            }
            Session.ClearCheckCode();
            if (userName.IsNullOrEmpty() || passWord.IsNullOrEmpty()) {
                return;
            }
            string Sceneid = Request.Cookies.GetQRCodeSceneid();
            if (!Sceneid.IsNullOrEmpty()) {
                Sceneid = Sceneid.Split(',')[0];
            }
            string key = Sceneid.GetMD5();
            string un = XXCode.Decrypt(userName, key);
            string pwd = XXCode.Decrypt(passWord, key);
            pwd = pwd.GetMD5();
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.UserName == un);
            if (baseUsers == null)//用户不存在
            {
                Response.Write("{\"msg\":\"E1\",\"log\":\"0\"}");
                return;
            }
            if (baseUsers.State != 1)
            {
                Response.Write("{\"msg\":\"E3\",\"log\":\"0\"}");
                return;
            }
            if (baseUsers.LoginLock == 1)//临时锁定
            {
                Response.Write("{\"msg\":\"E4\",\"log\":\"0\"}");
                return;
            }
            if (baseUsers.PassWord != pwd)
            {
                SysSet SysSet = Entity.SysSet.FirstOrNew();
                //系统统一修改标识SAME001
                baseUsers.LoginErr++;
                if (baseUsers.LoginErr >= SysSet.LoginLock)
                {
                    baseUsers.LoginLock = 1;
                }
                Entity.SaveChanges();
                int LoginErr = SysSet.LoginLock - baseUsers.LoginErr;
                Response.Write("{\"msg\":\"E2\",\"log\":\"" + LoginErr + "\"}");
                return;
            }
            string neiw = System.Configuration.ConfigurationManager.AppSettings["key"].ToString();
            string UserNameAndPassWord = LokFuEncode.LokFuAuthcodeEncode(string.Format("{0}|{1}|{2}", baseUsers.Id, un, pwd), neiw);
            Response.Cookies.SetUsers(UserNameAndPassWord);
            baseUsers.LoginErr = 0;
            Entity.SaveChanges();
            Response.Write("{\"msg\":\"OK\",\"log\":\"0\"}");
            return;
        }
        //方式1:普通长轮询 Ajax方式，服务端运用System.Threading.Thread，客户端运用无限Ajax请求
        //巨大缺点：采用Thread hold住程序会严重占用资源,影响处理请求并发。
        //方式2：ajax长连接，服务端无Thread，客户端Ajax采用timeout，隔时无限加载。
        //缺点：产生很多无效请求，包含过多的http头部信息，增加数据量。
        public void GetQrCodeLogin(string Sceneid)
        {
            UserLoginSceneid Log = Entity.UserLoginSceneid.FirstOrNew(n => n.Sceneid == Sceneid);
            if (Log.AddTime.AddMinutes(5) < DateTime.Now)//失效
            {
                Response.Cookies.SetQRCodeSceneid(string.Empty);
                Response.Write("E0");
                return;
            }
            if (Log.UId.IsNullOrEmpty())//未登录
            {
                if (!Log.Token.IsNullOrEmpty())//已扫码
                {
                    Response.Write("E8");
                    return;
                }
                else
                {
                    Response.Write("E9");
                    return;
                }
            }
            Users BaseUsers = Entity.Users.FirstOrNew(n => n.Id == Log.UId);
            if (BaseUsers.Id.IsNullOrEmpty())
            {
                Response.Write("E1");
                return;
            }
            string neiw = System.Configuration.ConfigurationManager.AppSettings["key"].ToString();
            string UserNameAndPassWord = LokFuEncode.LokFuAuthcodeEncode(string.Format("{0}|{1}|{2}", BaseUsers.Id, BaseUsers.UserName, BaseUsers.PassWord), neiw);
            Response.Cookies.SetUsers(UserNameAndPassWord);
            BaseUsers.LoginErr = 0;
            Entity.SaveChanges();
            //清除扫码记录cookie，删除临时保存的数据库二维码参数
            Response.Cookies.SetQRCodeSceneid(string.Empty);
            Entity.DeleteObject(Log);
            Entity.SaveChanges();
            Response.Write("OK");
            Response.End();
        }
        public object RemoveLogin()
        {
            Response.Cookies.SetUsers(string.Empty);
            return Redirect("/Shop/login.html");
        }
        /// <summary>
        /// 登录生成二维码 
        /// </summary>
        public void GetQRCodeByRandom()
        {
            string Sceneid = "000000"; string QRCodePicUrl = string.Empty;
            if (Request.Cookies.GetQRCodeSceneid().IsNullOrEmpty())
            {
                //删除过期随机参数记录
                DateTime Ptime = DateTime.Now.AddSeconds(-600);
                List<UserLoginSceneid> List = Entity.UserLoginSceneid.Where(n => n.AddTime < Ptime).ToList();
                foreach (var p in List)
                {
                    Entity.DeleteObject(p);
                }
                if (List.Count() > 0) { Entity.SaveChanges(); }
                //生成并保存随机参数
                int rid = new Random().Next(100001, Int32.MaxValue);
                Sceneid = rid.ToString();
                while (Entity.UserLoginSceneid.Count(n => n.Sceneid == Sceneid) != 0)
                {
                    rid = new Random().Next(100001, Int32.MaxValue);
                    Sceneid = rid.ToString();
                }
                QRCodePicUrl = "/UpLoadFiles/UserLoginSceneid/" + Sceneid + ".gif";
                string webFilePath = Server.MapPath(string.Format(QRCodePicUrl));        //服务器端文件路径
                if (!System.IO.File.Exists(webFilePath))
                {
                    QRCodeEncoder Encoder = new QRCodeEncoder();
                    //QRCode("Byte", 4, 7, "M", "A");
                    Encoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;//二维码编码方式
                    Encoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;//纠错码等级
                    Encoder.QRCodeScale = 5;//每个小方格的宽度
                    Encoder.QRCodeVersion = 5;//二维码版本号

                    int AgentId = Utils.GetAgentIdByDomain();
                    if (!AgentId.IsNullOrEmpty()) {
                        SysAgent SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == AgentId);
                        if (SysAgent.Tier != 1) {
                            AgentId = 0;
                        }
                        if (SysAgent.IsTeiPai == 0) {
                            AgentId = 0;
                        }
                    }
                    //动态调整二维码版本号,上限40，过长返回空白图片，编码后字符最大字节长度2953
                    string URL = string.Format("{0}/mobile/down/index-{2}.html#hfQCL{1}", Utils.GetHost(), Sceneid, AgentId);
                    Bitmap image = Encoder.Encode(URL, Encoding.UTF8);
                    image.Save(webFilePath, ImageFormat.Gif);
                }
                UserLoginSceneid UserLoginSceneid = new UserLoginSceneid();
                UserLoginSceneid.UId = 0;
                UserLoginSceneid.AddTime = DateTime.Now;
                UserLoginSceneid.Sceneid = Sceneid;
                UserLoginSceneid.Pic = Sceneid + ".gif";
                Entity.UserLoginSceneid.AddObject(UserLoginSceneid);
                Entity.SaveChanges();
                //记录到cookie，防止恶性刷新
                Response.Cookies.SetQRCodeSceneid(Sceneid + "," + QRCodePicUrl);
            }
            else
            {
                string SceneidAndUrl = Request.Cookies.GetQRCodeSceneid();
                Sceneid = SceneidAndUrl.Split(',')[0]; QRCodePicUrl = SceneidAndUrl.Split(',')[1];
            }
            ViewBag.QRCodePicUrl = QRCodePicUrl;
            ViewBag.Sceneid = Sceneid;
        }
    }
}

using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using ThoughtWorks.QRCode.Codec;

namespace LokFu.Areas.Base.Controllers
{
    /// <summary>
    /// 用于结算中心使用好付账号扫码登录
    /// </summary>
    public class AuthorizedLoginController : InitController
    {
        /// <summary>
        /// 生成授权码
        /// </summary>
        public void CreateAuthorizedCode()
        {
            string Sceneid = "000000"; string QRCodePicUrl = string.Empty;
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
                Encoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;//二维码编码方式
                Encoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;//纠错码等级
                Encoder.QRCodeScale = 5;//每个小方格的宽度
                Encoder.QRCodeVersion = 5;//二维码版本号

                int AgentId = Utils.GetAgentIdByDomain();
                if (!AgentId.IsNullOrEmpty())
                {
                    SysAgent SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == AgentId);
                    if (SysAgent.Tier != 1)
                    {
                        AgentId = 0;
                    }
                    if (SysAgent.IsTeiPai == 0)
                    {
                        AgentId = 0;
                    }
                }
                //动态调整二维码版本号,上限40，过长返回空白图片，编码后字符最大字节长度2953
                string URL = string.Format("{0}/mobile/down/index-{2}.html#hfQCL{1}", SysPath, Sceneid, AgentId);
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
           
            Response.Write(QRCodePicUrl); 
        }
        /// <summary>
        /// 验证授权并返回结果
        /// </summary>
        /// <param name="Sceneid">登录授权码</param>
        public void AuthorizedResult(string Sceneid)
        {
            UserLoginSceneid Log = Entity.UserLoginSceneid.FirstOrDefault(n => n.Sceneid == Sceneid);
            if (Log == null)
            {
                Response.Write("E0");
                return;
            }
            if (Log.AddTime.AddMinutes(5) < DateTime.Now)//失效
            {
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
            Users BaseUsers = Entity.Users.FirstOrDefault(n => n.Id == Log.UId);
            if (BaseUsers == null)
            {
                Response.Write("E1");
                return;
            }
            //清除扫码记录cookie，删除临时保存的数据库二维码参数
            Entity.DeleteObject(Log);
            Entity.SaveChanges();
            //返回用户信息
            BaseUsers.Cols = "TrueName,UserName,CardStae,CardId";
            string json = "{\"UserName\":\"" + BaseUsers.UserName + "\",\"CardStae\":\"" + BaseUsers.CardStae + "\",\"CardId\":\"" + BaseUsers.CardId + "\",\"TrueName\":\"" + BaseUsers.TrueName + "\"}";
            string AuthorizedKey = ConfigurationManager.AppSettings["AuthorizedKey"].ToString();
            json = LokFuEncode.LokFuAPIEncode(json, AuthorizedKey);
            Response.Write(json);
        }
        /// <summary>
        /// 用户点击重新扫码之后原二维码失效
        /// </summary>
        /// <param name="Sceneid"></param>
        public void AuthorizedDelete(string Sceneid)
        {
            UserLoginSceneid Log = Entity.UserLoginSceneid.FirstOrDefault(n => n.Sceneid == Sceneid);
            if (Log != null)
            {
                Entity.UserLoginSceneid.DeleteObject(Log);
                Entity.SaveChanges();
            }
        }
    }
}

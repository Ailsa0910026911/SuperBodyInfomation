using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LokFu.Infrastructure;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections;

namespace LokFu
{
    public class DataObj
    {
        private string eno;
        private string data;
        private string code;
        private string msg;
        private string key;
        private bool isreg;
        private string cols = "ENo,Data,Code,Msg";
        /// <summary>
        /// 设备号
        /// </summary>
        public string ENo
        {
            get { return eno; }
            set { eno = value; }
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        public string Data
        {
            get { return data; }
            set { data = value; }
        }
        /// <summary>
        /// 状态码
        /// </summary>
        public string Code
        {
            get { return code; }
            set { code = value; }
        }
        /// <summary>
        /// 状态消息
        /// </summary>
        public string Msg
        {
            get
            {
                if (msg.IsNullOrEmpty())
                {
                    Hashtable HT = new Hashtable();
                    HT.Add("0000", "成功");
                    HT.Add("1000", "接口参数错误");
                    HT.Add("1001", "数据不存在");
                    HT.Add("1002", "提交数据不全");
                    HT.Add("1003", "删除失败");
                    HT.Add("1004", "JSON格式错误");
                    HT.Add("1005", "接口维护中");
                    HT.Add("1006", "交易金额超限");
                    HT.Add("1007", "支付通道不可用");
                    HT.Add("1008", "请使用上次选择方式进行支付");
                    HT.Add("1009", "收银台暂不支持自定义到帐方式");

                    HT.Add("1020", "功能暂停使用");


                    HT.Add("1101", "您提供的证件信息与认证信息不符");
                    HT.Add("1102", "银行所属支行不存在");
                    HT.Add("1103", "银行数据不存在");
                    HT.Add("1104", "您的年龄超过入网限制");
                    HT.Add("1105", "此银行卡已用于交易结算,请先更换结算卡");

                    HT.Add("1016", "提现失败：超过单日单卡到账限额");

                    HT.Add("2001", "用户不存在");
                    HT.Add("2002", "用户帐号密码不正确");
                    HT.Add("2003", "用户被锁定");
                    HT.Add("2103", "密码错误太多次被锁定,请立即找回或次日重试！");
                    HT.Add("2004", "你的帐号在其他设备上登录了");
                    HT.Add("2005", "手机号已被注册");
                    HT.Add("2006", "未实名认证");
                    HT.Add("2007", "已实名认证");
                    HT.Add("2008", "未设置支付密码");
                    HT.Add("2009", "已设置支付密码");
                    HT.Add("2010", "支付密码错误");

                    HT.Add("2011", "真实姓名不正确");
                    HT.Add("2012", "身份证不正确");
                    HT.Add("2013", "银行卡不正确");
                    HT.Add("2014", "令牌错误");
                    HT.Add("2015", "您已经绑定本银行卡");
                    HT.Add("2016", "暂不支持该银行卡绑定");
                    HT.Add("2017", "暂不支持该银行卡提现");
                    HT.Add("2020", "该身份证已认证了其它用户");
                    HT.Add("2021", "您提交的相关信息不符，核验未通过!");
                    HT.Add("2022", "自助审核超限");
                    HT.Add("2023", "不支持该银行卡认证");
                    HT.Add("2024", "商户名称已被使用");
                    HT.Add("2025", "商户名称含有禁用词汇");
                    HT.Add("2026", "暂不支持您的手机号入网");
                    HT.Add("2027", "暂不支持您入网");
                    HT.Add("2028", "身份证已过期");
                    HT.Add("2031", "今天重发次数过多，请明天再试");
                    HT.Add("2032", "重发过快，请稍后再试");
                    HT.Add("2033", "验证码错误");
                    HT.Add("2034", "验证码已失效");
                    HT.Add("2035", "暂不支持该手机号入网");
                    HT.Add("2040", "二维码已失效");
                    HT.Add("2041", "未上传头像");
                    HT.Add("2044", "二维码已被绑定");
                    HT.Add("2050", "支付密码错误太多次被锁定,请立即找回或次日重试");
                    HT.Add("2054", "代理商账户不支持更换手机号");
                    HT.Add("2061", "不支持本操作方式");

                    HT.Add("2070", "未开通收付直通车");
                    HT.Add("2071", "请在可交易时间内进行交易");
                    HT.Add("2072", "非常抱歉，您暂时使用不了此通道，请更换其他通道收款！");
                    HT.Add("2073", "非常抱歉，此通道维护中，请稍候再试或更换其他通道收款！");
                    HT.Add("2074", "非常抱歉，此通道暂不可交易，请稍后再试");
                    //HT.Add("2075", "通道暂不可用，请稍后再试");
                    HT.Add("2076", "结算金额出现负数");
                    HT.Add("2077", "通道配置有误");
                    HT.Add("2078", "请求接口失败");
                    HT.Add("2079", "当前没有可用的渠道");
                    HT.Add("2080", "数据未改变");
                    HT.Add("2081", "订单未支付");

                    HT.Add("2096", "未能生成二维码");
                    HT.Add("2099", "未绑定银行卡");

                    HT.Add("2101", "操作过于频繁,请稍后再试!");
                    HT.Add("2102", "由于您短信邀请的好友太多没有实名认证，请使用其他方式邀请或通知已邀请好友实名认证后再试!");

                    HT.Add("3001", "设备不存在");
                    HT.Add("3002", "设备未注册");
                    HT.Add("3003", "设备已注册");
                    HT.Add("3004", "请上传串号");
                    HT.Add("3005", "APP不可用");

                    HT.Add("4001", "图片上传失败");
                    HT.Add("4002", "未上传银行卡照片");
                    HT.Add("4003", "请上传银行卡照片");

                    HT.Add("5001", "激活码或激活码密码错误");
                    HT.Add("5002", "激活码已被使用");
                    HT.Add("5003", "激活码或激活码密码错误");

                    HT.Add("6001", "余额不足");
                    HT.Add("6002", "收款用户被锁定");
                    HT.Add("6004", "收款用户不存在");
                    HT.Add("6006", "收款用户未认证");
                    HT.Add("6010", "订单不能取消");
                    HT.Add("6018", "非快速到账时段，请在快速到账时间内提现");
                    HT.Add("6019", "快速提现额度已用光、明天早点来哦");
                    HT.Add("6020", "您不符合马上到账快速提现");
                    HT.Add("6021", "非本人交易");
                    HT.Add("6022", "不能使用余额支付");
                    HT.Add("6023", "交易非选择余额支付");
                    HT.Add("6024", "交易成功或已取消");
                    HT.Add("6025", "交易已支付");
                    HT.Add("6026", "余额不足");
                    HT.Add("6027", "支付失败");
                    HT.Add("6028", "交易处理中，请稍后查看处理结果");

                    HT.Add("6031", "不能给自己转账");
                    HT.Add("6032", "银行卡不存在");
                    HT.Add("6033", "哎呀，小编脑力不足，请再试一次");
                    HT.Add("6041", "不需要传身份证及银行卡");
                    HT.Add("6042", "正在审核中，请不要重复提交");
                    HT.Add("6043", "审核成功，不需要提交");
                    HT.Add("6051", "以购买过服务");
                    HT.Add("6052", "未购买该服务");
                    HT.Add("6060", "账户禁止支付");
                    HT.Add("6061", "账户异常");

                    HT.Add("6071", "已开启自动提现，不能再开启自动转入余额理财");
                    HT.Add("6072", "已开启自动转入余额理财，不能再开启自动提现");
                    HT.Add("6074", "自动提现金额需大于系统设置金额");
                    HT.Add("6075", "请先绑定银行卡");
                    HT.Add("6076", "银行卡信息有误");

                    HT.Add("6081", "暂时没有可用支付通道");

                    HT.Add("7001", "您已经绑定该信用卡");
                    HT.Add("7002", "请使用借记卡绑定");
                    HT.Add("7003", "请输入验证码");
                    HT.Add("7004", "银行卡正在执行还款计划,不能删除");
                    HT.Add("7005", "暂不支持该银行卡绑定");
                    HT.Add("7006", "该银行卡正在执行任务");

                    HT.Add("7070", "该文案不存在");

                    HT.Add("8000", "服务器错误[8000]");
                    HT.Add("8001", "服务器错误[8001]");
                    HT.Add("8002", "因合同款未支付完成，试用期已结束");
                    HT.Add("8080", "服务器错误[8080]");
                    
                    HT.Add("8888", "交易异常");

                    HT.Add("9000", "不支持降级开通代理");
                    HT.Add("9001", "您已经是VIP用户");
                    HT.Add("9002", "用户类型不正确");
                    HT.Add("9003", "该用户没有代理商权限");
                    if (!code.IsNullOrEmpty())
                    {
                        if (HT.Contains(code))
                        {
                            msg = HT[code].ToString();
                        }
                        else
                        {
                            msg = "发生错误，代码[" + code + "]";
                        }
                    }
                    else
                    {
                        msg = "未知错误！";
                    }
                }
                return msg;
            }
            set { msg = value; }
        }

        /// <summary>
        /// 加密串
        /// </summary>
        public string Key
        {
            get { return key; }
            set { key = value; }
        }
        /// <summary>
        /// 是否已注册
        /// </summary>
        public bool IsReg
        {
            get { return isreg; }
            set { isreg = value; }
        }
        /// <summary>
        /// 输出列
        /// </summary>
        public string Cols
        {
            get { return cols; }
            set { cols = value; }
        }
        public void OutString()
        {
            if (!this.Data.IsNullOrEmpty())
            {
                //this.Data = ChinaseToUnicode(this.Data);
                this.Data = LokFuEncode.LokFuAPIEncode(this.Data, this.Key);
            }
            string Ret = this.OutJson();
            try
            {
                System.Web.HttpContext.Current.Response.AddHeader("content-type", "application/json");
            }
            catch (Exception) { }
            System.Web.HttpContext.Current.Response.Write(Ret);
            System.Web.HttpContext.Current.Response.End();
        }
        public void OutError(string ErrCode)
        {
            this.Data = "";
            this.Code = ErrCode;
            this.OutString();
        }
        public string GetData()
        {
            //string Data = HttpUtility.UrlDecode(this.Data, Encoding.UTF8);
            return LokFuEncode.LokFuAPIDecode(this.Data, this.Key);
        }
        public string ChinaseToUnicode(string str)
        {
            char[] c = str.ToCharArray();
            string newstr = "";
            for (int i = 0; i < c.Length; i++)
            {
                //转为16进制的unicode
                if (Encoding.Default.GetByteCount(c[i].ToString()) != 1)
                {
                    var u = ((int)c[i]).ToString("x");
                    newstr += "\\u" + u;
                }
                else
                {
                    newstr += c[i];
                }
            }
            return newstr;
        }
    }
}
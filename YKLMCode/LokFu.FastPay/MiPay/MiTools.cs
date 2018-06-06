using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace LokFu.FastPay
{
    public class MiTools
    {
        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="toEncrypt"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string DesEncrypt(string toEncrypt, string key, string iv)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
            byte[] ivArray = UTF8Encoding.UTF8.GetBytes(iv);
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.BlockSize = 128;
            rDel.KeySize = 256;
            rDel.FeedbackSize = 128;
            rDel.Padding = PaddingMode.PKCS7;
            rDel.Key = keyArray;
            rDel.IV = ivArray;
            rDel.Mode = CipherMode.CBC;
            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="toDecrypt"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string DesDecrypt(string toDecrypt, string key, string iv)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
            byte[] ivArray = UTF8Encoding.UTF8.GetBytes(iv);
            byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.BlockSize = 128;
            rDel.KeySize = 256;
            rDel.FeedbackSize = 128;
            rDel.Padding = PaddingMode.PKCS7;
            rDel.Key = keyArray;
            rDel.IV = ivArray;
            rDel.Mode = CipherMode.CBC;
            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return UTF8Encoding.UTF8.GetString(resultArray);
        }
        /**
         * 代付银行号转换
         * @param bin 联行号
         * @return 代付银行代码
         */
        public static string GetBankCode(string bin)
        {
            string ret = bin;
            string Left = bin.Substring(0, 3);
            switch (Left)
            {
                case "102":
                    ret = "102100099996";//中国工商银行|工商银行
                    break;
                case "103":
                    ret = "103100000026";//中国农业银行股份有限公司|农业银行
                    break;
                case "104":
                    ret = "104100000004";//中国银行总行|中国银行
                    break;
                case "105":
                    ret = "105100000017";//中国建设银行股份有限公司总行|建设银行
                    break;
                case "301":
                    ret = "301290000007";//交通银行|交通银行
                    break;
                case "302":
                    ret = "302100011000";//中信银行股份有限公司|中信银行
                    break;
                case "303":
                    ret = "303100000006";//中国光大银行|光大银行
                    break;
                case "304":
                    ret = "304100040000";//华夏银行股份有限公司总行|华夏银行
                    break;
                case "305":
                    ret = "305100000013";//中国民生银行|民生银行
                    break;
                case "306":
                    ret = "306581000003";//广发银行股份有限公司|广发银行
                    break;
                case "307":
                    ret = "307584007998";//平安银行（原深圳发展银行）|平安银行（原深圳发展银行）
                    break;
                case "308":
                    ret = "308584000013";//招商银行股份有限公司|招商银行
                    break;
                case "309":
                    ret = "309391000011";//兴业银行|兴业银行
                    break;
                case "310":
                    ret = "310290000013";//上海浦东发展银行|浦发银行
                    break;

                case "315":
                    ret = "315456000105";//恒丰银行
                    break;
                case "316":
                    ret = "316331000018";//浙商银行
                    break;
                case "317":
                    ret = "317110010019";//天津农村商业银行
                    break;
                case "318":
                    ret = "318110000014";//渤海银行
                    break;
                case "319":
                    ret = "319361000013";//徽商银行
                    break;
                case "321":
                    ret = "321667090019";//重庆三峡银行
                    break;
                case "322":
                    ret = "322290000011";//上海农商银行
                    break;
                case "325":
                    ret = "325290000012";//上海银行
                    break;

                case "403":
                    ret = "403100000004";//中国邮政储蓄银行有限责任公司|邮政银行
                    break;
            }
            return ret;
        }
    }
}

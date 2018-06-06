using LokFu.FastPay;
using LokFu.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace LokFu.Areas.Mobile.Controllers
{
    public class FPayController : BaseController
    {
        public void Pay()
        {
            string[] IdArr = "12715,12873,13267,13349,13655,13740,14421,14443,14961,15250,15251,15833,16218,17416,17695,17696,17703,17778,18076,19020,19092,20256,20422,20528,20529,21292,21293,21464,21486,22510,23328,23659,23660,24069,24123,24346,24478,24989,24990,25181,25483,26387,26644,27177,27239,27240,27552,27731,29083,29990,30209,30840,31152,31269,31831,32229,32387,32564,32641,32642,32650,33001,33040,33041,33056,33096,33191,33783,34060,34125,34239,34245,34606,34742,34982,34983,35314,35870,35903,36398,36806,36807,37141,37571,37585,37705,37782,37809,37988,38109,38110,38195,38316,39092,39093,39149,39513,39543,39614,40033,40255,40930,43375,43376,43598,44631,45373,45374,45517,46271,46403,46878,46923,46924,47753,47754,47915,47985,48608,49486,49712,49713,49881,50040,50537,50567,50947,51702,52040,52519,52603,52982,53722,54276,55672,55689,56117,56333,56472,56934,57382,58289,58619,58822,58823,59565,60919,60957,60984,60992,61129,61526,62028,62942,62951,63285,64497,64679,64685,64796,74721,75683,77491,77545".Split(',');
            foreach (var p in IdArr)
            {
                int Id = Int32.Parse(p);
                FastUserPay FastUserPay = Entity.FastUserPay.FirstOrDefault(n => n.Id == Id);
                FastPayWay FastPayWay = Entity.FastPayWay.FirstOrDefault(n => n.Id == FastUserPay.PayWay);
                FastUser FastUser = Entity.FastUser.FirstOrDefault(n => n.UId == FastUserPay.UId);
                FastUserPay.Bank = FastUser.Bank;
                FastUserPay.Card = FastUser.Card;
                FastUserPay.Bin = FastUser.Bin;
                FastUserPay.CardState = 2;
                Entity.SaveChanges();
                string[] PayConfigArr = FastPayWay.QueryArray.Split(',');
                if (FastPayWay.DllName == "MiShua")
                {
                    //米刷一户一码，标识删除，重新绑卡？坐等接口
                    if (PayConfigArr.Length == 4)
                    {
                        string MchNo = PayConfigArr[0];//商户号
                        string EncryptKey = PayConfigArr[1];//加密key
                        string EncryptIV = PayConfigArr[2];//偏移量
                        string SignKey = PayConfigArr[3];//签名key

                        #region 修改商户

                        string MiPayUrl = "http://pay.mishua.cn/zhonlinepay/service/down/trans/editMerchant";

                        SortedDictionary<string, string> sortDic = new SortedDictionary<string, string>();
                        sortDic.Add("versionNo", "1");
                        sortDic.Add("mchNo", MchNo);
                        sortDic.Add("subMchNo", FastUserPay.MerId);

                        sortDic.Add("accNo", FastUserPay.Card);
                        sortDic.Add("accName", FastUserPay.CardName);
                        sortDic.Add("bankName", FastUserPay.Bank);
                        sortDic.Add("bankType", FastUserPay.Bin);

                        Dictionary<string, string> Dic = MiTools.FilterPara(sortDic);
                        string PostJson = new JavaScriptSerializer().Serialize(Dic);
                        //AES加密           
                        string EnString = MiTools.DesEncrypt(PostJson, EncryptKey, EncryptIV);
                        //签名
                        string Sign = (EnString + SignKey).GetMD5().ToUpper();
                        string jsonString = "{\"mchNo\": \"" + MchNo + "\",\"payload\": \"" + EnString + "\",\"sign\": \"" + Sign + "\"}";
                        string result = MiTools.JsonPost(MiPayUrl, jsonString);
                        JObject obj = new JObject();
                        try
                        {
                            obj = (JObject)JsonConvert.DeserializeObject(result);
                        }
                        catch (Exception)
                        {
                            obj = null;
                        }
                        if (obj != null)
                        {
                            string state = "";
                            if (obj["state"] != null)
                            {
                                state = obj["state"].ToString();
                            }
                            if (state == "Successful")
                            {
                                string EnReturn = obj["payload"].ToString();
                                string resultStr = MiTools.DesDecrypt(EnReturn, EncryptKey, EncryptIV);
                                JObject json = (JObject)JsonConvert.DeserializeObject(resultStr);
                                string status = json["status"].ToString();
                                string subMchNo = json["subMchNo"].ToString();
                                FastUserPay.MerId = subMchNo;
                                if (status == "00")//成功
                                {
                                    FastUserPay.CardState = 1;
                                }
                                if (status == "01")//处理中
                                {
                                    FastUserPay.CardState = 3;
                                }
                                if (status == "02")//失败
                                {
                                    FastUserPay.CardState = 4;
                                }
                            }
                            else
                            {
                                string message = obj["message"].ToString();
                                FastUserPay.CardMsg = message;
                            }
                            Entity.SaveChanges();
                        }
                        #endregion
                    }
                }
            }
        }

    }
}

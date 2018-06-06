using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories.Extensions
{
    public class UsersNervousModel
    {
        /// <summary>
        /// 账户
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 商户名
        /// </summary>
        public string NeekName { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string TrueName { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// 前天可用余额
        /// </summary>
        public decimal BeforeAmonut { get; set; }
        /// <summary>
        /// 前天冻结余额
        /// </summary>
        public decimal BeforeFrozen { get; set; }
        /// <summary>
        /// 银联卡
        /// </summary>
        public decimal O_PayMoney { get; set; }
        /// <summary>
        /// 升级
        /// </summary>
        public decimal P_Amoney { get; set; }
        /// <summary>
        /// 支付宝
        /// </summary>
        public decimal FZ_PayMoney { get; set; }
        /// <summary>
        /// 微信
        /// </summary>
        public decimal FW_PayMoney { get; set; }
        /// <summary>
        /// NFC
        /// </summary>
        public decimal FN_PayMoney { get; set; }
        /// <summary>
        /// 理财转出
        /// </summary>
        public decimal BF_Amount { get; set; }
        /// <summary>
        /// 分润
        /// </summary>
        public decimal Share_AMOUNT { get; set; }
        /// <summary>
        /// T0提现
        /// </summary>
        public decimal C_PayMoney_T0 { get; set; }
        /// <summary>
        /// T1提现
        /// </summary>
        public decimal C_PayMoney_T1 { get; set; }
        /// <summary>
        /// 理财转入
        /// </summary>
        public decimal BO_Amount { get; set; }
        /// <summary>
        /// 房租
        /// </summary>
        public decimal OH_PayMoney { get; set; }
        /// <summary>
        /// 当天可用余额
        /// </summary>
        public decimal AfterAmonut { get; set; }
        /// <summary>
        /// 当天冻结金额
        /// </summary>
        public decimal AfterFrozen { get; set; }
        /// <summary>
        /// 理财余额
        /// </summary>
        public decimal BL_Amount { get; set; }
    }
}

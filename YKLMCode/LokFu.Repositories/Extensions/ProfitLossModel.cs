using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories.Extensions
{
    public class ProfitLossModel
    {
        /// <summary>
        /// 支付方式代码
        /// </summary>
        public string PayType { get; set; }
        /// <summary>
        /// 支付方式
        /// </summary>
        public string PayWay { get; set; }
        /// <summary>
        /// 交易金额
        /// </summary>
        public decimal Amoney { get; set; }
        /// <summary>
        /// 到账金额
        /// </summary>
        public decimal PayMoney { get; set; }
        /// <summary>
        /// 用户手续费
        /// </summary>
        public decimal Poundage { get; set; }
        /// <summary>
        /// 支出手续费
        /// </summary>
        public decimal SysRate { get; set; }
        /// <summary>
        /// 结算金额
        /// </summary>
        public decimal AgentPayGet { get; set; }
        /// <summary>
        /// 利润
        /// </summary>
        public decimal Profie { get; set; }
        /// <summary>
        /// 支出统计
        /// </summary>
        public decimal Amount { get; set; }
    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace YKLMModel.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class OrderProfitLog
    {
        public int Id { get; set; }
        public System.DateTime AddTime { get; set; }
        public byte IsDel { get; set; }
        public int UId { get; set; }
        public string TNum { get; set; }
        public byte LogType { get; set; }
        public byte Tier { get; set; }
        public decimal Profit { get; set; }
        public string UserName { get; set; }
        public byte OrderType { get; set; }
        public decimal Amoney { get; set; }
        public int Agent { get; set; }
    }
}

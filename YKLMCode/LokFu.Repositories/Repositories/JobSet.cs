//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace LokFu.Repositories.Repositories
{
    using System;
    using System.Collections.Generic;
    
    public partial class JobSet
    {
        public int Id { get; set; }
        public decimal Cost { get; set; }
        public decimal VIPCost { get; set; }
        public decimal Cash { get; set; }
        public decimal VIPCash { get; set; }
        public int EqDays { get; set; }
        public int MaxDay { get; set; }
        public int MaxPay { get; set; }
        public decimal MinMoney { get; set; }
        public decimal MaxMoney { get; set; }
        public decimal Floated { get; set; }
        public byte AdvCost { get; set; }
        public byte AdvCash { get; set; }
        public int MaxRand { get; set; }
        public decimal MinFloated { get; set; }
        public decimal AdvFloated { get; set; }
        public decimal MaxFloated { get; set; }
        public decimal DayMoney { get; set; }
        public byte SetZhiNeng { get; set; }
    }
}
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
    
    public partial class BaoStory
    {
        public int Id { get; set; }
        public System.DateTime SDate { get; set; }
        public decimal GetCost { get; set; }
        public byte IsDel { get; set; }
        public decimal YearPer { get; set; }
        public decimal InMoney { get; set; }
        public decimal OutMoney { get; set; }
        public decimal BfAllMoney { get; set; }
        public decimal BfActMoney { get; set; }
        public decimal BfInMoney { get; set; }
        public decimal Interest { get; set; }
        public decimal AfAllMoney { get; set; }
        public decimal AfActMoney { get; set; }
        public decimal AfInMoney { get; set; }
        public byte LType { get; set; }
    }
}
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
    
    public partial class OrdersRec
    {
        public int Id { get; set; }
        public string TNum { get; set; }
        public int UId { get; set; }
        public string TName { get; set; }
        public int RUId { get; set; }
        public string RName { get; set; }
        public byte TState { get; set; }
        public decimal Amoney { get; set; }
        public System.DateTime AddTime { get; set; }
        public Nullable<System.DateTime> PayTime { get; set; }
        public int PayWay { get; set; }
        public string Remark { get; set; }
        public string OrderAddress { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public decimal Poundage { get; set; }
        public byte InState { get; set; }
    }
}

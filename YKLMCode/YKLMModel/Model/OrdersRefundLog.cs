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
    
    public partial class OrdersRefundLog
    {
        public int Id { get; set; }
        public string TNum { get; set; }
        public byte LogType { get; set; }
        public string Remark { get; set; }
        public string Img { get; set; }
        public string AdminName { get; set; }
        public System.DateTime AddTime { get; set; }
        public byte IsDel { get; set; }
        public int AdminId { get; set; }
    }
}

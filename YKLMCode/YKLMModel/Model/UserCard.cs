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
    
    public partial class UserCard
    {
        public int Id { get; set; }
        public int UId { get; set; }
        public short Type { get; set; }
        public string Name { get; set; }
        public string Bank { get; set; }
        public string Card { get; set; }
        public Nullable<int> Province { get; set; }
        public Nullable<int> City { get; set; }
        public Nullable<int> District { get; set; }
        public string Bin { get; set; }
        public string Deposit { get; set; }
        public byte IsDel { get; set; }
        public Nullable<int> BId { get; set; }
        public string Mobile { get; set; }
        public string Pic { get; set; }
        public string ScanNo { get; set; }
        public byte State { get; set; }
        public string CVV { get; set; }
        public Nullable<byte> BillDay { get; set; }
        public Nullable<byte> RefundDay { get; set; }
        public string ValidYear { get; set; }
        public string ValidMonth { get; set; }
        public Nullable<System.DateTime> UnBindingTime { get; set; }
        public Nullable<System.DateTime> AddTime { get; set; }
    }
}

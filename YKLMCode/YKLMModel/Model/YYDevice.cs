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
    
    public partial class YYDevice
    {
        public int Id { get; set; }
        public int UId { get; set; }
        public string DevId { get; set; }
        public string PageId { get; set; }
        public string Comment { get; set; }
        public string UUID { get; set; }
        public string Major { get; set; }
        public string Minor { get; set; }
        public System.DateTime AddTime { get; set; }
        public byte ActState { get; set; }
        public byte State { get; set; }
        public string Poi_Id { get; set; }
        public string Poi_Appid { get; set; }
        public byte IsDel { get; set; }
    }
}

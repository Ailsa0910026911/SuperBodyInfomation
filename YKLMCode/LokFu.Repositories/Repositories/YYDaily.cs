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
    
    public partial class YYDaily
    {
        public int Id { get; set; }
        public int UId { get; set; }
        public string DevId { get; set; }
        public System.DateTime OutDate { get; set; }
        public string PageId { get; set; }
        public System.DateTime AddTime { get; set; }
        public byte State { get; set; }
        public int ClickPV { get; set; }
        public int ClickUV { get; set; }
        public int ShakePV { get; set; }
        public int ShakeUV { get; set; }
    }
}

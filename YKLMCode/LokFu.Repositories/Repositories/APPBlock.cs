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
    
    public partial class APPBlock
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SubName { get; set; }
        public string IconUrl { get; set; }
        public string PicUrl { get; set; }
        public string LinkName1 { get; set; }
        public string LinkUrl1 { get; set; }
        public Nullable<byte> LinkType1 { get; set; }
        public string LinkName2 { get; set; }
        public string LinkUrl2 { get; set; }
        public Nullable<byte> LinkType2 { get; set; }
        public int AgentId { get; set; }
        public int Sort { get; set; }
        public byte State { get; set; }
        public System.DateTime AddTime { get; set; }
        public string LinkUrl { get; set; }
        public Nullable<byte> LinkType { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }
}

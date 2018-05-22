namespace Test
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("superbodyinfomation.goods")]
    public partial class goods
    {
        [StringLength(32)]
        public string ID { get; set; }

        [StringLength(512)]
        public string Name { get; set; }

        [StringLength(51200)]
        public string Describe { get; set; }

        [StringLength(512)]
        public string Img { get; set; }

        public double? Price { get; set; }

        public double? VipPrice { get; set; }

        public double? LogisticsPrice { get; set; }

        public DateTime? CreateTime { get; set; }

        [StringLength(512)]
        public string Extension { get; set; }
    }
}

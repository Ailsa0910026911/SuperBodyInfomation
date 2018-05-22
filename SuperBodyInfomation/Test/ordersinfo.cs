namespace Test
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("superbodyinfomation.ordersinfo")]
    public partial class ordersinfo
    {
        [StringLength(32)]
        public string ID { get; set; }

        [Required]
        [StringLength(11)]
        public string Phone { get; set; }

        [StringLength(512)]
        public string Name { get; set; }

        [StringLength(1024)]
        public string Adress { get; set; }

        public DateTime? DateTime { get; set; }

        [StringLength(1024)]
        public string OExtension { get; set; }

        public double? Money { get; set; }

        [Column(TypeName = "bit")]
        public bool PayStatus { get; set; }

        [StringLength(512)]
        public string Remark { get; set; }
    }
}

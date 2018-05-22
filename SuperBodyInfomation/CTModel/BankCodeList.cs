namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BankCodeList")]
    public partial class BankCodeList
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string BankCode { get; set; }

        [StringLength(50)]
        public string BankName { get; set; }

        public int? PayWayType { get; set; }
    }
}

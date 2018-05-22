namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserPayChange")]
    public partial class UserPayChange
    {
        public int Id { get; set; }

        public int UId { get; set; }

        public int SId { get; set; }

        public int SAId { get; set; }

        [StringLength(200)]
        public string Pic { get; set; }

        public DateTime AddTime { get; set; }

        [StringLength(2000)]
        public string Remark { get; set; }

        public int AId { get; set; }

        public DateTime? EditTime { get; set; }

        [StringLength(2000)]
        public string EditRemark { get; set; }

        public byte? State { get; set; }

        public byte IsDel { get; set; }

        [Column(TypeName = "money")]
        public decimal Cash0 { get; set; }

        [Column(TypeName = "money")]
        public decimal ECash0 { get; set; }

        [Column(TypeName = "money")]
        public decimal CashNew0 { get; set; }

        [Column(TypeName = "money")]
        public decimal ECashNew0 { get; set; }

        [Column(TypeName = "money")]
        public decimal Cash1 { get; set; }

        [Column(TypeName = "money")]
        public decimal ECash1 { get; set; }

        [Column(TypeName = "money")]
        public decimal CashNew1 { get; set; }

        [Column(TypeName = "money")]
        public decimal ECashNew1 { get; set; }

        [StringLength(200)]
        public string ShowTip { get; set; }

        [StringLength(200)]
        public string SubTitle { get; set; }
    }
}

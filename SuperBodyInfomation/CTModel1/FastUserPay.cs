namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FastUserPay")]
    public partial class FastUserPay
    {
        public int Id { get; set; }

        public int UId { get; set; }

        public int PayWay { get; set; }

        [StringLength(50)]
        public string MerId { get; set; }

        [StringLength(2000)]
        public string MerKey { get; set; }

        public decimal UserCost { get; set; }

        [Column(TypeName = "money")]
        public decimal UserCash { get; set; }

        public byte MerState { get; set; }

        [StringLength(500)]
        public string MerMsg { get; set; }

        public byte CardState { get; set; }

        [StringLength(500)]
        public string CardMsg { get; set; }

        public byte BusiState { get; set; }

        [StringLength(500)]
        public string BusiMsg { get; set; }

        public string ArrayInfo { get; set; }

        [StringLength(10)]
        public string CardName { get; set; }

        [StringLength(20)]
        public string Bank { get; set; }

        [StringLength(20)]
        public string Card { get; set; }

        [StringLength(20)]
        public string Bin { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        public decimal UserCost2 { get; set; }

        public decimal UserCost3 { get; set; }
    }
}

namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PayConfigChange")]
    public partial class PayConfigChange
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string Title { get; set; }

        [Column(TypeName = "money")]
        public decimal Cash0 { get; set; }

        [Column(TypeName = "money")]
        public decimal ECash0 { get; set; }

        [Column(TypeName = "money")]
        public decimal Cash1 { get; set; }

        [Column(TypeName = "money")]
        public decimal ECash1 { get; set; }

        [StringLength(2000)]
        public string Remark { get; set; }

        public byte? State { get; set; }

        [Column(TypeName = "money")]
        public decimal? APrice { get; set; }

        [Column(TypeName = "money")]
        public decimal? BPrice { get; set; }

        [Column(TypeName = "money")]
        public decimal? CPrice { get; set; }

        public byte IsDel { get; set; }

        public byte CState { get; set; }

        public byte EState { get; set; }

        public int ShareNumber { get; set; }

        [StringLength(100)]
        public string ShowTip { get; set; }

        [StringLength(100)]
        public string SubTitle { get; set; }

        public int? AgentId { get; set; }
    }
}

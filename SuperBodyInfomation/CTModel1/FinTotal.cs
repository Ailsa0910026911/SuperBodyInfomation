namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FinTotal")]
    public partial class FinTotal
    {
        public int Id { get; set; }

        public DateTime? Update { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        [Column(TypeName = "money")]
        public decimal? TotalAmoney { get; set; }

        [Column(TypeName = "money")]
        public decimal? TotlaPoundage { get; set; }

        [Column(TypeName = "money")]
        public decimal? Amoney1 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Poundage1 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Amoney2 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Poundage2 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Amoney2_0 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Poundage2_0 { get; set; }

        public int? Number2 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Poundage2_1 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Amoney3 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Poundage3 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Amoney4 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Poundage4 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Amoney5 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Poundage5 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Amoney5_0 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Poundage5_0 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Amoney6 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Poundage6 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Amoney7 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Poundage7 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Amoney8 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Poundage8 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Amoney9 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Poundage9 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Amoney10 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Poundage10 { get; set; }
    }
}

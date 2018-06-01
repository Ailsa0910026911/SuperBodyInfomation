namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BusinessShareProfit")]
    public partial class BusinessShareProfit
    {
        public int Id { get; set; }

        public decimal? S1_4 { get; set; }

        public decimal? S1_5 { get; set; }

        public decimal? S1_6 { get; set; }

        public decimal? S1_1 { get; set; }

        public decimal? S0_0 { get; set; }

        public decimal? S2_6_5 { get; set; }

        public decimal? S2_6_4 { get; set; }

        public decimal? S2_5_4 { get; set; }

        public decimal? S3_6_5_4 { get; set; }

        public int? Type { get; set; }
        [StringLength(50)]
        public string Extention { get; set; }
    }
}

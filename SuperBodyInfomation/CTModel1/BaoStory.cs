namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BaoStory")]
    public partial class BaoStory
    {
        public int Id { get; set; }

        public DateTime SDate { get; set; }

        [Column(TypeName = "money")]
        public decimal GetCost { get; set; }

        public byte IsDel { get; set; }

        public decimal YearPer { get; set; }

        [Column(TypeName = "money")]
        public decimal InMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal OutMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal BfAllMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal BfActMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal BfInMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal Interest { get; set; }

        [Column(TypeName = "money")]
        public decimal AfAllMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal AfActMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal AfInMoney { get; set; }

        public byte LType { get; set; }
    }
}

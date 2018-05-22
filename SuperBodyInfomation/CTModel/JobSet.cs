namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("JobSet")]
    public partial class JobSet
    {
        public int Id { get; set; }

        public decimal Cost { get; set; }

        public decimal VIPCost { get; set; }

        [Column(TypeName = "money")]
        public decimal Cash { get; set; }

        [Column(TypeName = "money")]
        public decimal VIPCash { get; set; }

        public int EqDays { get; set; }

        public int MaxDay { get; set; }

        public int MaxPay { get; set; }

        [Column(TypeName = "money")]
        public decimal MinMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal MaxMoney { get; set; }

        public decimal Floated { get; set; }

        public byte AdvCost { get; set; }

        public byte AdvCash { get; set; }

        public int MaxRand { get; set; }

        public byte IsDel { get; set; }

        public decimal MinFloated { get; set; }

        public decimal AdvFloated { get; set; }

        public decimal MaxFloated { get; set; }

        [Column(TypeName = "money")]
        public decimal DayMoney { get; set; }

        public byte SetZhiNeng { get; set; }
    }
}

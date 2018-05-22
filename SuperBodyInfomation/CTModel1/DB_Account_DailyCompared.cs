namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DB_Account_DailyCompared
    {
        public int Id { get; set; }

        [Column(TypeName = "money")]
        public decimal? ORDERS_1 { get; set; }

        [Column(TypeName = "money")]
        public decimal? ORDERS_P1 { get; set; }

        [Column(TypeName = "money")]
        public decimal? ORDERS_7 { get; set; }

        [Column(TypeName = "money")]
        public decimal? ORDERS_P7 { get; set; }

        [Column(TypeName = "money")]
        public decimal? ORDERS_8 { get; set; }

        [Column(TypeName = "money")]
        public decimal? ORDERS_P8 { get; set; }

        [Column(TypeName = "money")]
        public decimal? ORDERS_9 { get; set; }

        [Column(TypeName = "money")]
        public decimal? ORDERS_P9 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Baglog { get; set; }

        [Column(TypeName = "money")]
        public decimal? TurnLog { get; set; }

        [Column(TypeName = "money")]
        public decimal? OrderProfitLog { get; set; }

        [Column(TypeName = "money")]
        public decimal? ORDERS_3 { get; set; }

        [Column(TypeName = "money")]
        public decimal? ORDERS_P3 { get; set; }

        [Column(TypeName = "money")]
        public decimal? Userlog15 { get; set; }

        [Column(TypeName = "money")]
        public decimal? ORDERS_2 { get; set; }

        [Column(TypeName = "money")]
        public decimal? ORDERS_5 { get; set; }

        [Column(TypeName = "money")]
        public decimal? ORDERS_6 { get; set; }

        [Column(TypeName = "money")]
        public decimal? ORDERS_12 { get; set; }

        [Column(TypeName = "money")]
        public decimal? UserAuth { get; set; }

        [Column(TypeName = "money")]
        public decimal? U_Amony { get; set; }

        [Column(TypeName = "money")]
        public decimal? U_Frozen { get; set; }

        [Column(TypeName = "money")]
        public decimal? B_Amony { get; set; }

        [Column(TypeName = "money")]
        public decimal? DiffResult { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DATED { get; set; }
    }
}

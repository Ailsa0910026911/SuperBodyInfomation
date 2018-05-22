namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class BaoUsers
    {
        public int Id { get; set; }

        public int UId { get; set; }

        [Column(TypeName = "money")]
        public decimal AllMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal ActMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal InMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal AllRec { get; set; }

        [Column(TypeName = "money")]
        public decimal LastRec { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }
    }
}

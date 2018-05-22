namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CheckUserMoney")]
    public partial class CheckUserMoney
    {
        public int Id { get; set; }

        public int Progress { get; set; }

        [Column(TypeName = "money")]
        public decimal ChangeMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal ChangeFrozen { get; set; }

        [Column(TypeName = "money")]
        public decimal ProgressMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal RecordMoney { get; set; }

        public DateTime TaskDate { get; set; }

        public int UId { get; set; }
    }
}

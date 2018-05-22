namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BasicBank")]
    public partial class BasicBank
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Logo { get; set; }

        public byte State { get; set; }

        public byte IsDel { get; set; }

        [StringLength(10)]
        public string Code { get; set; }

        public byte IsApply { get; set; }

        public string Actives { get; set; }

        public string Info { get; set; }

        public byte IsJieJi { get; set; }

        public byte IsDaiJi { get; set; }

        public byte IsPayCard { get; set; }

        [StringLength(100)]
        public string Logo1 { get; set; }

        [StringLength(100)]
        public string Logo2 { get; set; }

        [StringLength(100)]
        public string Logo3 { get; set; }

        [StringLength(500)]
        public string CreditCardUrl { get; set; }

        [StringLength(20)]
        public string BIN { get; set; }

        public byte CanCredit { get; set; }
    }
}

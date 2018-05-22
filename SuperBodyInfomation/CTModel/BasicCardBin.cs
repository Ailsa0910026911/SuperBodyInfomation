namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BasicCardBin")]
    public partial class BasicCardBin
    {
        public int Id { get; set; }

        [StringLength(20)]
        public string BankName { get; set; }

        [StringLength(20)]
        public string BankCode { get; set; }

        public byte? CardType { get; set; }

        public int? Length { get; set; }

        [StringLength(10)]
        public string BIN { get; set; }

        public int BankId { get; set; }

        public byte IsDel { get; set; }
    }
}

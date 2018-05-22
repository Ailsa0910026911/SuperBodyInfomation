namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BasicBankCard")]
    public partial class BasicBankCard
    {
        public int Id { get; set; }

        public int BId { get; set; }

        [StringLength(10)]
        public string BIN { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        public int CType { get; set; }

        public int Length { get; set; }

        public byte State { get; set; }

        public byte IsDel { get; set; }
    }
}

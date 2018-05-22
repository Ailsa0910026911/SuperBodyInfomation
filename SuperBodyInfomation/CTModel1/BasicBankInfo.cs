namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BasicBankInfo")]
    public partial class BasicBankInfo
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        public int BId { get; set; }

        public byte State { get; set; }

        public byte IsDel { get; set; }

        public int SId { get; set; }

        public int CId { get; set; }

        public int? DId { get; set; }

        [StringLength(50)]
        public string BIN { get; set; }
    }
}

namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CutLog")]
    public partial class CutLog
    {
        public int Id { get; set; }

        public int CAId { get; set; }

        [Column(TypeName = "money")]
        public decimal Amoney { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        public int CUId { get; set; }

        public int CUIded { get; set; }

        [StringLength(50)]
        public string NickName { get; set; }

        [StringLength(50)]
        public string NickNamed { get; set; }
    }
}

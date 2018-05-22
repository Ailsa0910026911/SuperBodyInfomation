namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserAsk")]
    public partial class UserAsk
    {
        public int Id { get; set; }

        public int? UId { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Answer { get; set; }

        public byte? State { get; set; }

        public byte IsDel { get; set; }
    }
}

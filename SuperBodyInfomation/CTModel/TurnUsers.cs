namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TurnUsers
    {
        public int Id { get; set; }

        public int UId { get; set; }

        public int Times { get; set; }

        public int UsedTimes { get; set; }

        public byte InTimes { get; set; }

        public int? TId { get; set; }

        public byte IsDel { get; set; }
    }
}

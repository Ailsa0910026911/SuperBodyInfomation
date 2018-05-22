namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MsgTemplate")]
    public partial class MsgTemplate
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public byte? SendWay { get; set; }

        [StringLength(20)]
        public string Tag { get; set; }

        public byte OrderType { get; set; }

        public byte OrderState { get; set; }

        public byte PayState { get; set; }

        public byte IdCardState { get; set; }

        [StringLength(200)]
        public string Title { get; set; }

        public string Info { get; set; }

        public byte State { get; set; }

        public byte? IsManage { get; set; }

        [Required]
        [StringLength(200)]
        public string MLink { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }
    }
}

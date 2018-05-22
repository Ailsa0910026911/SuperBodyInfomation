namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CutUsers
    {
        public int Id { get; set; }

        public int? CAId { get; set; }

        public int? WXId { get; set; }

        [Required]
        [StringLength(50)]
        public string NickName { get; set; }

        public int Times { get; set; }

        [Column(TypeName = "money")]
        public decimal AllPrice { get; set; }

        [Column(TypeName = "money")]
        public decimal MyPrice { get; set; }

        [Column(TypeName = "money")]
        public decimal CutPrice { get; set; }

        public DateTime AddTime { get; set; }

        public byte State { get; set; }

        public byte IsDel { get; set; }
    }
}

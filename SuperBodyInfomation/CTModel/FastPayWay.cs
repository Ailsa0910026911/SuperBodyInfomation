namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FastPayWay")]
    public partial class FastPayWay
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public decimal Cost { get; set; }

        public decimal BankCost { get; set; }

        public decimal BankCost2 { get; set; }

        [Column(TypeName = "money")]
        public decimal Cash { get; set; }

        [Required]
        [StringLength(100)]
        public string DllName { get; set; }

        [Required]
        [StringLength(100)]
        public string GroupType { get; set; }

        [Required]
        [StringLength(100)]
        public string Version { get; set; }

        public int Sort { get; set; }

        [StringLength(1000)]
        public string QueryArray { get; set; }

        public byte NeekReg { get; set; }

        public byte NeekCard { get; set; }

        public byte State { get; set; }

        public byte IsDel { get; set; }

        public byte HasWeiXin { get; set; }

        public byte HasAliPay { get; set; }

        [Column(TypeName = "money")]
        public decimal SNum { get; set; }

        [Column(TypeName = "money")]
        public decimal ENum { get; set; }

        [Column(TypeName = "money")]
        public decimal SNum2 { get; set; }

        [Column(TypeName = "money")]
        public decimal ENum2 { get; set; }

        public byte TimeType { get; set; }

        public DateTime STime { get; set; }

        public DateTime ETime { get; set; }

        public byte HasBank { get; set; }

        [Column(TypeName = "money")]
        public decimal BankSNum { get; set; }

        [Column(TypeName = "money")]
        public decimal BankENum { get; set; }

        public decimal BankCost3 { get; set; }

        public DateTime AddTime { get; set; }

        [StringLength(100)]
        public string ShowName { get; set; }

        public byte CanOpenAliPay { get; set; }

        public byte CanOpenWeiXin { get; set; }

        public byte CanOpenBank { get; set; }

        [Column(TypeName = "money")]
        public decimal MinCost { get; set; }

        [Column(TypeName = "money")]
        public decimal MaxCost { get; set; }

        [Column(TypeName = "money")]
        public decimal MinCost2 { get; set; }

        [Column(TypeName = "money")]
        public decimal MaxCost2 { get; set; }

        [Column(TypeName = "money")]
        public decimal MinCost3 { get; set; }

        [Column(TypeName = "money")]
        public decimal MaxCost3 { get; set; }

        public decimal InCost { get; set; }

        public decimal InCost2 { get; set; }

        public decimal InCost3 { get; set; }

        public decimal Cost2 { get; set; }

        public decimal Cost3 { get; set; }

        public byte ManE { get; set; }

        public int? Type { get; set; }
    }
}

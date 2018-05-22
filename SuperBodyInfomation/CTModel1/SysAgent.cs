namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SysAgent")]
    public partial class SysAgent
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Address { get; set; }

        [StringLength(20)]
        public string Tel { get; set; }

        [StringLength(20)]
        public string Fax { get; set; }

        [StringLength(20)]
        public string Linker { get; set; }

        [StringLength(20)]
        public string LinkMobile { get; set; }

        public int? AdminId { get; set; }

        public byte State { get; set; }

        public DateTime AddTime { get; set; }

        public double PayGet { get; set; }

        public double CashGet { get; set; }

        public byte IsDel { get; set; }

        public double LoanGet { get; set; }

        public double Credit { get; set; }

        [StringLength(20)]
        public string ApkVer { get; set; }

        public int? ApkInt { get; set; }

        [StringLength(200)]
        public string ApkUrl { get; set; }

        [StringLength(200)]
        public string ApkYYB { get; set; }

        public string ApkInfo { get; set; }

        [StringLength(20)]
        public string IosVer { get; set; }

        public int? IosInt { get; set; }

        [StringLength(200)]
        public string IosUrl { get; set; }

        [StringLength(200)]
        public string IosYYB { get; set; }

        public string IosInfo { get; set; }

        public byte IsTeiPai { get; set; }

        [StringLength(200)]
        public string ShareIco { get; set; }

        [StringLength(200)]
        public string CradBg { get; set; }

        [StringLength(200)]
        public string ShareBg { get; set; }

        [StringLength(200)]
        public string DownBg1 { get; set; }

        [StringLength(200)]
        public string DownBg2 { get; set; }

        [StringLength(200)]
        public string DownBg3 { get; set; }

        public double DaiLiGet { get; set; }

        public byte DaiLiGetType { get; set; }

        public int MyUId { get; set; }

        [StringLength(50)]
        public string APPTag { get; set; }

        public int AgentID { get; set; }

        public byte IsPromote { get; set; }

        public double PromoteGet { get; set; }

        public byte Tier { get; set; }

        public byte Levels { get; set; }

        public int AgentTypeID { get; set; }

        public byte IsShowWord { get; set; }

        [StringLength(200)]
        public string CompanyLogo { get; set; }

        [Required]
        [StringLength(50)]
        public string Salesman { get; set; }

        public DateTime ExpireTime { get; set; }

        [Column(TypeName = "money")]
        public decimal AgentFee { get; set; }

        [StringLength(10)]
        public string MsgExt { get; set; }

        [StringLength(200)]
        public string APPName { get; set; }

        [Required]
        [StringLength(500)]
        public string NoteDownload { get; set; }

        public byte APPHasMore { get; set; }

        public int AppBtnNumber { get; set; }

        public int AppMenuHome { get; set; }

        public int AppMenuMore { get; set; }

        public int AppMenuBottom { get; set; }

        [StringLength(20)]
        public string ApkColor { get; set; }

        [StringLength(20)]
        public string IosColor { get; set; }

        public int? AgentState { get; set; }

        [StringLength(200)]
        public string AgentRegion { get; set; }

        [Column(TypeName = "money")]
        public decimal ECash0 { get; set; }

        [Column(TypeName = "money")]
        public decimal Cash0 { get; set; }

        [Column(TypeName = "money")]
        public decimal ECash1 { get; set; }

        [Column(TypeName = "money")]
        public decimal Cash1 { get; set; }

        public int Cash0Times { get; set; }

        public int Cash1Times { get; set; }

        public byte Set3 { get; set; }

        public byte MaxTier { get; set; }

        public byte Set4 { get; set; }

        public byte AgentLevelMax { get; set; }

        public string Agreement { get; set; }

        public int SameAgent { get; set; }
    }
}

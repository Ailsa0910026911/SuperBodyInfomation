namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Users
    {
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string UserName { get; set; }

        [StringLength(32)]
        public string PassWord { get; set; }

        [StringLength(50)]
        public string NeekName { get; set; }

        [StringLength(20)]
        public string TrueName { get; set; }

        [StringLength(20)]
        public string Mobile { get; set; }

        public byte MobileState { get; set; }

        [StringLength(20)]
        public string QQ { get; set; }

        [StringLength(50)]
        public string Email { get; set; }

        public byte EmailState { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(20)]
        public string CardId { get; set; }

        [StringLength(200)]
        public string CardPic { get; set; }

        public byte CardStae { get; set; }

        public byte State { get; set; }

        [Column(TypeName = "money")]
        public decimal Amount { get; set; }

        [Column(TypeName = "money")]
        public decimal Frozen { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        [StringLength(200)]
        public string RegAddress { get; set; }

        [StringLength(20)]
        public string X { get; set; }

        [StringLength(20)]
        public string Y { get; set; }

        [StringLength(32)]
        public string PayPwd { get; set; }

        public byte MiBao { get; set; }

        [StringLength(20)]
        public string CardNum { get; set; }

        public int Agent { get; set; }

        public int AId { get; set; }

        [StringLength(200)]
        public string Token { get; set; }

        [StringLength(200)]
        public string Pic { get; set; }

        [StringLength(200)]
        public string CardImg1 { get; set; }

        [StringLength(200)]
        public string CardImg2 { get; set; }

        [StringLength(200)]
        public string CardFace { get; set; }

        [StringLength(200)]
        public string CardBack { get; set; }

        [StringLength(2000)]
        public string CardRemark { get; set; }

        public int LoginErr { get; set; }

        public int LoginLock { get; set; }

        public int PayErr { get; set; }

        public int PayLock { get; set; }

        public int? MyPId { get; set; }

        [StringLength(200)]
        public string ENo { get; set; }

        public int PayConfigId { get; set; }

        public byte CardType { get; set; }

        public int SAId { get; set; }

        public byte SALevel { get; set; }

        public string ShopInfo { get; set; }

        [StringLength(50)]
        public string ShopTel { get; set; }

        [StringLength(2000)]
        public string ShopNotice { get; set; }

        public string Remark { get; set; }

        public byte? ShareType { get; set; }

        [Column(TypeName = "money")]
        public decimal Cash0 { get; set; }

        [Column(TypeName = "money")]
        public decimal ECash0 { get; set; }

        [Column(TypeName = "money")]
        public decimal Cash1 { get; set; }

        [Column(TypeName = "money")]
        public decimal ECash1 { get; set; }

        public byte StopPayState { get; set; }

        [StringLength(2000)]
        public string StopPayRemark { get; set; }

        [Column(TypeName = "money")]
        public decimal StopPayMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal CashFastPer { get; set; }

        [Column(TypeName = "money")]
        public decimal CashFastMax { get; set; }

        [Column(TypeName = "money")]
        public decimal TodayMoney { get; set; }

        public byte YYOpenState { get; set; }

        public byte InTypeMobile { get; set; }

        public byte InTypePC { get; set; }

        [StringLength(100)]
        public string AssureImgName { get; set; }

        public byte HasT0 { get; set; }

        public int T0Times { get; set; }

        public int T1Times { get; set; }

        [StringLength(200)]
        public string PrintToken { get; set; }

        public DateTime? AddAuthTime { get; set; }

        public byte AutoBao { get; set; }

        public byte AutoCash { get; set; }

        [Column(TypeName = "money")]
        public decimal AutoCashMoney { get; set; }

        public int? AutoCashBank { get; set; }

        public byte StopPayAuditState { get; set; }

        [StringLength(200)]
        public string CarLicensePic { get; set; }

        [StringLength(200)]
        public string CarLocationPic { get; set; }

        [StringLength(200)]
        public string CarOther { get; set; }

        [Column(TypeName = "money")]
        public decimal YAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal AllRec { get; set; }

        [Column(TypeName = "money")]
        public decimal LastRec { get; set; }

        public byte IfCanIn { get; set; }

        [StringLength(10)]
        public string CardTrueName { get; set; }

        [StringLength(10)]
        public string CardGender { get; set; }

        [StringLength(10)]
        public string CardNation { get; set; }

        [StringLength(100)]
        public string CardAddress { get; set; }

        [StringLength(50)]
        public string CardIssue { get; set; }

        public DateTime? CardValidSDate { get; set; }

        public DateTime? CardValidEDate { get; set; }

        [Column(TypeName = "money")]
        public decimal? Sp_Amount { get; set; }

        public byte CardBackMode { get; set; }

        public byte CardFrontMode { get; set; }

        public byte IsVip { get; set; }

        public DateTime? VipETime { get; set; }
    }
}

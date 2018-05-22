namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SysSet")]
    public partial class SysSet
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(50)]
        public string AgentName { get; set; }

        [StringLength(20)]
        public string ICP { get; set; }

        [StringLength(200)]
        public string Tel { get; set; }

        public string Agreement { get; set; }

        public double House { get; set; }

        public byte CashChecked { get; set; }

        public byte CashPayWay { get; set; }

        [Column(TypeName = "money")]
        public decimal Cash0 { get; set; }

        [Column(TypeName = "money")]
        public decimal ECash0 { get; set; }

        [Column(TypeName = "money")]
        public decimal QCash0 { get; set; }

        [Column(TypeName = "money")]
        public decimal Cash1 { get; set; }

        [Column(TypeName = "money")]
        public decimal ECash1 { get; set; }

        public int? CashDay { get; set; }

        public int? CashNum { get; set; }

        [Column(TypeName = "money")]
        public decimal? CashMoney { get; set; }

        public byte IsDel { get; set; }

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

        public int SMSTimes { get; set; }

        public int SMSActives { get; set; }

        [StringLength(20)]
        public string SMSEnd { get; set; }

        public int LoginLock { get; set; }

        public int PayLock { get; set; }

        public byte? APKState { get; set; }

        public byte? IOSState { get; set; }

        [Column(TypeName = "money")]
        public decimal CtrlMoney1 { get; set; }

        public byte ApkSet1 { get; set; }

        public byte IosSet1 { get; set; }

        public byte ApkSet2 { get; set; }

        public byte IosSet2 { get; set; }

        public byte ApkSet3 { get; set; }

        public byte IosSet3 { get; set; }

        public byte ApkSet4 { get; set; }

        public byte IosSet4 { get; set; }

        public byte ApkSet5 { get; set; }

        public byte IosSet5 { get; set; }

        public byte ApkSet6 { get; set; }

        public byte IosSet6 { get; set; }

        public byte ApkSet7 { get; set; }

        public byte IosSet7 { get; set; }

        public byte ApkSet8 { get; set; }

        public byte IosSet8 { get; set; }

        public byte ApkSet9 { get; set; }

        public byte IosSet9 { get; set; }

        public byte ApkSet10 { get; set; }

        public byte IosSet10 { get; set; }

        public byte ApkSet11 { get; set; }

        public byte IosSet11 { get; set; }

        public byte ApkSet12 { get; set; }

        public byte IosSet12 { get; set; }

        public byte ApkSet13 { get; set; }

        public byte IosSet13 { get; set; }

        public byte ApkSet14 { get; set; }

        public byte IosSet14 { get; set; }

        public byte ApkSet15 { get; set; }

        public byte IosSet15 { get; set; }

        [Column(TypeName = "money")]
        public decimal Level1Price { get; set; }

        [Column(TypeName = "money")]
        public decimal Level2Price { get; set; }

        [Column(TypeName = "money")]
        public decimal Level3Price { get; set; }

        [Column(TypeName = "money")]
        public decimal Level4Price { get; set; }

        [Column(TypeName = "money")]
        public decimal Level5Price { get; set; }

        public byte AutoPay { get; set; }

        public byte GlobalAgentMaxLevel { get; set; }

        public byte GlobaPromoteMaxLevel { get; set; }

        public int AppMenuHome { get; set; }

        public int AppMenuMore { get; set; }

        public int AppMenuBottom { get; set; }

        [StringLength(200)]
        public string AppWord { get; set; }

        [StringLength(200)]
        public string SW1Key { get; set; }

        [StringLength(200)]
        public string SW1KeyT { get; set; }

        public DateTime? SW1sTime { get; set; }

        public DateTime? SW1eTime { get; set; }

        [StringLength(200)]
        public string SW2Key { get; set; }

        [StringLength(200)]
        public string SW2KeyT { get; set; }

        public DateTime? SW2sTime { get; set; }

        public DateTime? SW2eTime { get; set; }

        public int AppBtnNumber { get; set; }

        [StringLength(20)]
        public string ApkColor { get; set; }

        [StringLength(20)]
        public string IosColor { get; set; }

        public byte APPHasMore { get; set; }

        public byte LagEntry { get; set; }

        public byte LagEntryNum { get; set; }

        public byte LagEntryDay { get; set; }

        public decimal AgentGet { get; set; }

        public int Cash0Times { get; set; }

        public int Cash1Times { get; set; }

        [Column(TypeName = "money")]
        public decimal PayConfigAgent { get; set; }

        [Column(TypeName = "money")]
        public decimal AutoCashMoney { get; set; }

        public string NoWord { get; set; }

        [Column(TypeName = "money")]
        public decimal SysECash0 { get; set; }

        [Column(TypeName = "money")]
        public decimal SysCash0 { get; set; }

        [Column(TypeName = "money")]
        public decimal SysECash1 { get; set; }

        [Column(TypeName = "money")]
        public decimal SysCash1 { get; set; }

        [Column(TypeName = "money")]
        public decimal AgentECash0 { get; set; }

        [Column(TypeName = "money")]
        public decimal AgentCash0 { get; set; }

        [Column(TypeName = "money")]
        public decimal AgentECash1 { get; set; }

        [Column(TypeName = "money")]
        public decimal AgentCash1 { get; set; }

        public int AgentCash0Times { get; set; }

        public int AgentCash1Times { get; set; }

        public byte AuthType { get; set; }

        [Column(TypeName = "money")]
        public decimal AuthPrice { get; set; }

        public byte ShanHuZiXuan { get; set; }

        public byte ShouYinTai { get; set; }

        public byte ZhiTongChe { get; set; }

        [Required]
        public string CashAlertMsgT0 { get; set; }

        [Required]
        public string CashAlertMsgT1 { get; set; }

        public byte GetOrderWay { get; set; }

        public byte ApkAutoUpdate { get; set; }

        public byte IosAutoUpdate { get; set; }

        public int YaoQingIntervalTime { get; set; }

        public int YaoQingIntervalNumber { get; set; }

        [Required]
        public string BaoUserAlert { get; set; }

        public decimal EveryDayMaxCash { get; set; }

        public int AuthMinAge { get; set; }

        public int AuthMaxAge { get; set; }

        public byte AgentWay { get; set; }

        public byte OpenAutoAuthAndroid { get; set; }

        [Required]
        [StringLength(50)]
        public string AutoAuthAppKeyAndroid { get; set; }

        [Required]
        [StringLength(100)]
        public string AutoAuthAppSecretAndroid { get; set; }

        [Required]
        [StringLength(100)]
        public string AutoAuthAppCodeAndroid { get; set; }

        public byte OpenAutoAuthIOS { get; set; }

        [Required]
        [StringLength(50)]
        public string AutoAuthAppKeyIOS { get; set; }

        [Required]
        [StringLength(100)]
        public string AutoAuthAppSecretIOS { get; set; }

        [Required]
        [StringLength(100)]
        public string AutoAuthAppCodeIOS { get; set; }
    }
}

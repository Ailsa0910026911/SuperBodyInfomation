//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace LokFu.Repositories.Repositories
{
    using System;
    using System.Collections.Generic;
    
    public partial class ApplyCredit
    {
        public int Id { get; set; }
        public Nullable<int> UId { get; set; }
        public string TrueName { get; set; }
        public string Sex { get; set; }
        public string IDcard { get; set; }
        public int BankId { get; set; }
        public string Mobile { get; set; }
        public string Company { get; set; }
        public Nullable<int> ComProvince { get; set; }
        public Nullable<int> ComCity { get; set; }
        public Nullable<int> ComDistrict { get; set; }
        public string ComAddress { get; set; }
        public string ComTel { get; set; }
        public string Position { get; set; }
        public string Education { get; set; }
        public byte HasSheBao { get; set; }
        public string SheBaoTime { get; set; }
        public byte Marry { get; set; }
        public byte HasCar { get; set; }
        public Nullable<int> CarBrand { get; set; }
        public string House { get; set; }
        public Nullable<double> Income { get; set; }
        public string Payment { get; set; }
        public byte HasCredit { get; set; }
        public Nullable<int> CreditBankId { get; set; }
        public Nullable<int> MyScore { get; set; }
        public byte State { get; set; }
        public int AId { get; set; }
        public System.DateTime AddTime { get; set; }
        public int AgentId { get; set; }
        public int AgentAId { get; set; }
        public decimal Amoney { get; set; }
        public byte PayState { get; set; }
        public Nullable<System.DateTime> PayTime { get; set; }
        public byte AgentPay { get; set; }
        public decimal AgentMoney { get; set; }
        public decimal AIdMoney { get; set; }
        public string CompanyNature { get; set; }
        public string SheBao { get; set; }
    }
}

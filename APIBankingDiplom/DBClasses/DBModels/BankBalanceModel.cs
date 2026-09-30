using APIBankingDiplom.DBClasses.DBModels.BaseClasses;
using APIBankingDiplom.Security.Utilities;
using System.ComponentModel.DataAnnotations;

namespace APIBankingDiplom.DBClasses.DBModels
{
    public class BankBalanceModel : BaseModel
    {
        [StringLength(3)]
        public string Currency { get; set; } = string.Empty;

        [StringLength(18 + SecurityMeasures.AESPotentialSizeIncrease)]
        public string MoneyEncrypt { get; set; } = SecurityMeasures.AESEncrypt("0");
    }
}

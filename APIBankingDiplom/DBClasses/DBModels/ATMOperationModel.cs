using APIBankingDiplom.CommonResources.Utilities;
using APIBankingDiplom.DBClasses.DBModels.BaseClasses;
using APIBankingDiplom.DBClasses.Enums;
using System.Text.Json.Serialization;

namespace APIBankingDiplom.DBClasses.DBModels
{
    public class ATMOperationModel : BaseModel
    {
        public decimal Money { get; set; }

        public BankingEnums.ATMOperationType ATMOperationType { get; set; }

        public DateTime Date { get; set; } = DateUtility.GetCurrentTime();

        public int CardId { get; set; }

        [JsonIgnore]
        public CardModel? Card { get; set; }

        public int? CardBalanceId { get; set; }

        [JsonIgnore]
        public CardBalanceModel? CardBalance { get; set; }
    }
}

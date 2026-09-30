using APIBankingDiplom.CommonResources.Utilities;
using APIBankingDiplom.DBClasses.DBModels.BaseClasses;
using APIBankingDiplom.Security.Utilities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APIBankingDiplom.DBClasses.DBModels
{
    public class TransactionModel : BaseModel
    {
        [StringLength(32 + SecurityMeasures.AESPotentialSizeIncrease)]
        public string TransactionTypeEncrypt { get; set; } = string.Empty;

        [StringLength(18 + SecurityMeasures.AESPotentialSizeIncrease)]
        public string AmountSentEncrypt { get; set; } = string.Empty;

        [StringLength(18 + SecurityMeasures.AESPotentialSizeIncrease)]
        public string AmountReceivedEncrypt { get; set; } = string.Empty;

        [StringLength(3)]
        public string SenderCurrency { get; set; } = string.Empty;

        [StringLength(3)]
        public string ReceiverCurrency { get; set; } = string.Empty;

        public DateTime TransactionDate { get; set; } = DateUtility.GetCurrentTime();

        [StringLength(512 + SecurityMeasures.AESPotentialSizeIncrease)]
        public string DescriptionEncrypt { get; set; } = string.Empty;

        public int SenderCardId {  get; set; }

        [JsonIgnore]
        public CardModel? SenderCard { get; set; }

        public int SenderBalanceId { get; set; }

        [JsonIgnore]
        public CardBalanceModel? SenderBalance { get; set; }


        public int ReceiverCardId { get; set; }

        [JsonIgnore]
        public CardModel? ReceiverCard { get; set; }

        public int ReceiverBalanceId { get; set; }

        [JsonIgnore]
        public CardBalanceModel? ReceiverBalance { get; set; }
    }
}

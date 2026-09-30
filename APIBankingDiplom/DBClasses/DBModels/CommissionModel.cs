using APIBankingDiplom.DBClasses.DBModels.BaseClasses;
using APIBankingDiplom.Security.Utilities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APIBankingDiplom.DBClasses.DBModels
{
    public class CommissionModel : BaseModel
    {
        [StringLength(3)]
        public string Currency { get; set; } = string.Empty;

        [StringLength(18 + SecurityMeasures.AESPotentialSizeIncrease)]
        public string AmountEncrypt { get; set; } = string.Empty;

        // We will keep it as string because it occupies less space than decimal datatype
        [StringLength(4)]
        public string CommissionPercentage { get; set; } = string.Empty;

        public bool IsSentToBankAccount { get; set; } = false;

        [JsonIgnore]
        public int CardId { get; set; }
        [JsonIgnore]
        public CardModel? Card { get; set; }
    }
}

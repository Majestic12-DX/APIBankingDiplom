using APIBankingDiplom.CommonResources.Utilities;
using APIBankingDiplom.DBClasses.DBModels.BaseClasses;
using APIBankingDiplom.Security.Utilities;
using System.ComponentModel.DataAnnotations;

namespace APIBankingDiplom.DBClasses.DBModels
{
    public class LogModel : BaseModel
    {
        [StringLength(32 + SecurityMeasures.AESPotentialSizeIncrease)]
        public string ActionTypeEncrypt { get; set; } = string.Empty;

        [StringLength(512 + SecurityMeasures.AESPotentialSizeIncrease)]
        public string ActionDescriptionEncrypt { get; set; } = string.Empty;

        public DateTime Time { get; set; } = DateUtility.GetCurrentTime();

        public int UserId { get; set; }
        public UserModel? User { get; set; }
    }
}

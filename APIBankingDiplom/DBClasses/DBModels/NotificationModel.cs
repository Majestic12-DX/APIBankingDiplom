using APIBankingDiplom.CommonResources.Utilities;
using APIBankingDiplom.DBClasses.DBModels.BaseClasses;
using APIBankingDiplom.Security.Utilities;
using System.ComponentModel.DataAnnotations;

namespace APIBankingDiplom.DBClasses.DBModels
{
    public class NotificationModel : BaseModel
    {
        [StringLength(32 + SecurityMeasures.AESPotentialSizeIncrease)]
        public string TypeEncrypt { get; set; } = string.Empty;

        [StringLength(512 + SecurityMeasures.AESPotentialSizeIncrease)]
        public string MessageEncrypt { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime TimeSent { get; set; } = DateUtility.GetCurrentTime();

        public int UserId { get; set; }
        public UserModel? User { get; set; }
    }
}

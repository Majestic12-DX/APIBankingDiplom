using APIBankingDiplom.DBClasses.DBModels.BaseClasses;
using System.ComponentModel.DataAnnotations;

namespace APIBankingDiplom.DBClasses.DBModels
{
    public class SettingModel : BaseModel
    {
        [StringLength(2)]
        public string Language { get; set; } = string.Empty;

        [StringLength(32)]
        public string Theme { get; set; } = string.Empty;

        public int UserId { get; set; }
        public UserModel? User { get; set; }
    }
}

using APIBankingDiplom.CommonResources.Utilities;
using APIBankingDiplom.DBClasses.DBModels.BaseClasses;
using APIBankingDiplom.Security.Utilities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APIBankingDiplom.DBClasses.DBModels
{
    /* ABOUT SAME DATA HASHING AND ENCRYPTION (Email and PhoneNumber fields):
       This is redundancy, but we can't replicate a string if we use AES with IVs
       And we also need to keep email and phone number unique for each user
       Without creating horrendous and unoptimized methods of checking each encryption...
       Soo... well, this is a small trade off for using IVs */
    public class UserModel : BaseModel
    {
        [JsonIgnore]
        public byte[]? ProfilePicture { get; set; }

        [StringLength(64 + SecurityMeasures.AESPotentialSizeIncrease)]
        public string NameEncrypt { get; set; } = string.Empty;

        [StringLength(SecurityMeasures.BCryptLength), JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(255 + SecurityMeasures.AESPotentialSizeIncrease)]
        public string EmailEncrypt { get; set; } = string.Empty;

        [StringLength(SecurityMeasures.SHA384Length), JsonIgnore]
        public string EmailHash { get; set; } = string.Empty;

        [JsonIgnore]
        public bool IsEmailVerified { get; set; } = false;

        [StringLength(16 + SecurityMeasures.AESPotentialSizeIncrease)]
        public string PhoneNumberEncrypt { get; set; } = string.Empty;

        [StringLength(SecurityMeasures.SHA384Length), JsonIgnore]
        public string PhoneNumberHash { get; set;} = string.Empty;

        [StringLength(3 + SecurityMeasures.AESPotentialSizeIncrease), JsonIgnore]
        public string PINEncrypt { get; set; } = string.Empty;

        public DateTime LastLoginTime { get; set; } = DateUtility.GetCurrentTime();
    }
}
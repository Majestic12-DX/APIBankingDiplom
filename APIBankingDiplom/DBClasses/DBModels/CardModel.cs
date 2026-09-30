using APIBankingDiplom.CommonResources.Utilities;
using APIBankingDiplom.DBClasses.DBModels.BaseClasses;
using APIBankingDiplom.DBClasses.Enums;
using APIBankingDiplom.Security.Utilities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APIBankingDiplom.DBClasses.DBModels
{
    /* ABOUT SAME DATA HASHING AND ENCRYPTION (Card Number fields):
       This is redundancy, but we can't replicate a string if we use AES with IVs
       And we also need to keep email and phone number unique for each user
       Without creating horrendous and unoptimized methods of checking each encryption...
       Soo... well, this is a small trade off for using IVs */
    public class CardModel : BaseModel
    {
        [StringLength(SecurityMeasures.SHA384Length), JsonIgnore]
        public string CardNumberHash { get; set; } = string.Empty;
        
        [StringLength(16 + SecurityMeasures.AESPotentialSizeIncrease)]
        public string CardNumberEncrypt { get; set; } = string.Empty;

        public BankingEnums.CardType CardType { get; set; } = BankingEnums.CardType.Debit;

        // Search purposes for BackgroundServices/CardOperationsService.cs
        [StringLength(SecurityMeasures.SHA384Length), JsonIgnore]
        public string ExpireDateHash { get; set; }

        // Stored as a string for encryption
        [StringLength(10 + SecurityMeasures.AESPotentialSizeIncrease)]
        public string ExpireDateEncrypt { get; set; }

        [StringLength(3 + SecurityMeasures.AESPotentialSizeIncrease)]
        public string CVVEncrypt { get; set; } = string.Empty;

        public BankingEnums.CardStatus Status { get; set; } = BankingEnums.CardStatus.Active;

        [JsonIgnore]
        public int UserId { get; set; }
        [JsonIgnore]
        public UserModel? User { get; set; }

        // Let's set value for ExpireDateEncrypt in a constructor for more readability
        public CardModel() : base()
        {
            DateTime expireDate = DateUtility.GetCurrentTime().AddYears(6).AddMonths(6);
            string expireDateInString = expireDate.ToShortDateString();

            ExpireDateEncrypt = SecurityMeasures.AESEncrypt(expireDateInString);
            ExpireDateHash = SecurityMeasures.HashStringSHA384(expireDateInString);
        }

        public bool IsActive() => Status is BankingEnums.CardStatus.Active;
        public bool IsBlocked() => Status is BankingEnums.CardStatus.Blocked;
        public bool IsExpired() => Status is BankingEnums.CardStatus.Expired;
    }

}

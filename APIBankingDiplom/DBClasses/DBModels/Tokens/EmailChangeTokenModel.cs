using APIBankingDiplom.DBClasses.DBModels.BaseModels;
using APIBankingDiplom.Security.Utilities;
using APIBankingDiplom.TokenWorkings;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIBankingDiplom.DBClasses.DBModels.Tokens
{
    public class EmailChangeTokenModel : BaseUserTokenModel
    {
        [StringLength(SecurityMeasures.SHA384Length), NotMapped]
        public string Email
        {
            get { return SecurityMeasures.AESDecrypt(EmailEncrypt); }
            set
            {
                EmailEncrypt = SecurityMeasures.AESEncrypt(value);
                EmailHash = SecurityMeasures.HashStringSHA384(value);
            }
        }

        [StringLength(255 + SecurityMeasures.AESPotentialSizeIncrease)]
        public string EmailEncrypt { get; private set; } = string.Empty;

        [StringLength(SecurityMeasures.SHA384Length)]
        public string EmailHash { get; private set; } = string.Empty;

        public EmailChangeTokenModel() : base()
        {
            LifeTime = TokenOptions.GetEmailChangeTokenLifetime();
        }
    }
}

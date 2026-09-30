using APIBankingDiplom.DBClasses.DBModels.BaseModels;
using APIBankingDiplom.TokenWorkings;

namespace APIBankingDiplom.DBClasses.DBModels.Tokens
{
    public class PasswordResetTokenModel : BaseUserTokenModel
    {
        public PasswordResetTokenModel() : base()
        {
            LifeTime = TokenOptions.GetPasswordResetTokenLifetime();
        }
    }
}

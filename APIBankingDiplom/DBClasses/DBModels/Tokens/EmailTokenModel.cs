using APIBankingDiplom.DBClasses.DBModels.BaseModels;
using APIBankingDiplom.TokenWorkings;

namespace APIBankingDiplom.DBClasses.DBModels.Tokens
{
    public class EmailTokenModel : BaseUserTokenModel
    {
        public EmailTokenModel() : base()
        {
            LifeTime = TokenOptions.GetEmailTokenLifetime();
        }
    }
}

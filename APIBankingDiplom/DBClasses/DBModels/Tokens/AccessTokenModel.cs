using APIBankingDiplom.DBClasses.DBModels.BaseModels;
using APIBankingDiplom.TokenWorkings;

namespace APIBankingDiplom.DBClasses.DBModels.Tokens
{
    public class AccessTokenModel : BaseUserTokenModel
    {
        public AccessTokenModel() : base()
        {
            LifeTime = TokenOptions.GetAccessTokenLifetime();
        }
    }
}

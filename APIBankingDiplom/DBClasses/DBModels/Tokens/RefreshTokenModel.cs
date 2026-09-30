using APIBankingDiplom.DBClasses.DBModels.BaseModels;
using APIBankingDiplom.TokenWorkings;

namespace APIBankingDiplom.DBClasses.DBModels.Tokens
{
    public class RefreshTokenModel : BaseUserTokenModel
    {
        public RefreshTokenModel() : base()
        {
            LifeTime = TokenOptions.GetRefreshTokenLifetime();
        }
    }
}

using APIBankingDiplom.CommonResources.Utilities;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIBankingDiplom.DBClasses.DBModels.BaseModels
{
    public abstract class BaseUserTokenModel : BaseTokenModel
    {
        public int UserId { get; set; }
        public UserModel? User { get; set; }

        private TimeSpan _lifeTime = TimeSpan.Zero;
        [NotMapped] // Let's not map this into EF migration for obvious reasons
        public TimeSpan LifeTime
        {
            get { return _lifeTime; }
            protected set
            {
                _lifeTime = value;
                UpdateExpireDate();
            }
        }

        protected void UpdateExpireDate()
        {
            ExpireDate = DateUtility.GetCurrentTime().Add(LifeTime);
        }
    }
}

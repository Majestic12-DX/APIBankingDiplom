using APIBankingDiplom.DBClasses.DBModels.BaseClasses;
using APIBankingDiplom.Security.Utilities;
using System.ComponentModel.DataAnnotations;

namespace APIBankingDiplom.DBClasses.DBModels.BaseModels
{
    public abstract class BaseTokenModel : BaseModel
    {
        private string _tokenHash = string.Empty;
        [StringLength(SecurityMeasures.SHA384Length)]
        public string TokenHash
        {
            get { return _tokenHash; }
            set { _tokenHash = SecurityMeasures.HashStringSHA384(value); }
        }

        public DateTime ExpireDate { get; set; }

        public bool IsInvalid { get; set; }
    }
}

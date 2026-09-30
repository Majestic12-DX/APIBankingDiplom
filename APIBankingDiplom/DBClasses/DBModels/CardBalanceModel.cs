using APIBankingDiplom.DBClasses.DBModels.BaseClasses;
using APIBankingDiplom.DBClasses.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APIBankingDiplom.DBClasses.DBModels
{
    public class CardBalanceModel : BaseModel
    {
        [StringLength(3)]
        public string Currency { get; set; } = string.Empty;

        public decimal Balance { get; set; } = decimal.Zero;

        public BankingEnums.CardBalanceStatus Status { get; set; } = BankingEnums.CardBalanceStatus.Active;

        public decimal ATM_DepositedToday { get; set; } = decimal.Zero;
        public decimal ATM_DepositLimit { get; set; } = decimal.Zero;

        public decimal ATM_WithdrawnToday { get; set; } = decimal.Zero;
        public decimal ATM_WithdrawalLimit { get; set; } = decimal.Zero;

        public decimal TransactedToday { get; set; } = decimal.Zero;
        public decimal TransactionLimit { get; set; } = decimal.Zero;

        public DateTime? LastDepositDate { get; set; }
        public DateTime? LastWithdrawalDate { get; set; }
        public DateTime? LastTransactionDate { get; set; }

        [JsonIgnore]
        public int CardId { get; set; }
        [JsonIgnore]
        public CardModel? Card { get; set; }

        public bool IsActive() => Status is BankingEnums.CardBalanceStatus.Active;
        public bool IsLimited() => Status is BankingEnums.CardBalanceStatus.Limited;
        public bool IsBlocked() => Status is BankingEnums.CardBalanceStatus.Blocked;
    }
}

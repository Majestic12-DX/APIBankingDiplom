using APIBankingDiplom.DBClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace APIBankingDiplom.Controllers.DTO
{
    public class BalanceOperationDTO
    {
        [Required] public string cardNumber { get; set; } = string.Empty;
        [Required] public BankingEnums.Currency currency { get; set; }
    }

    public class BalanceStatusOperationDTO
    {
        [Required] public string cardNumber { get; set; } = string.Empty;
        [Required] public BankingEnums.Currency currency { get; set; }
        [Required] public BankingEnums.CardBalanceStatus status { get; set; }
    }

    public class BalanceLimitOperationDTO
    {
        [Required] public string cardNumber { get; set; } = string.Empty;
        [Required] public BankingEnums.Currency currency { get; set; }
        [Required, Range(0, int.MaxValue)] public int limit { get; set; }
    }
}

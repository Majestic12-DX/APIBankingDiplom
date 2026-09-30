using APIBankingDiplom.DBClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace APIBankingDiplom.Controllers.DTO
{
    public class ATMOperationDTO
    {
        [Required] public string cardNumber { get; set; } = string.Empty;
        [Required] public string expireDate { get; set; } = string.Empty;
        [Required] public string CVV { get; set; } = string.Empty;
        [Required] public BankingEnums.CardType cardType { get; set; }
        [Required] public string PIN { get; set; } = string.Empty;
        [Required] public BankingEnums.Currency currency { get; set; }
        [Required] public decimal money { get; set; } = decimal.Zero;
    }

    public class ATMOperationPageDTO
    {
        [Required] public string cardNumber { get; set; } = string.Empty;
        public BankingEnums.Currency? currency { get; set; }
        [Required] public int page { get; set; }
        public bool ascending { get; set; } = false;
    }

    public class ATMPageCountDTO
    {
        [Required] public string cardNumber { get; set; } = string.Empty;
        public BankingEnums.Currency? Currency { get; set; }
    }
}

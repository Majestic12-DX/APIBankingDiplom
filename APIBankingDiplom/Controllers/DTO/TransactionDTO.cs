using APIBankingDiplom.DBClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace APIBankingDiplom.Controllers.DTO
{
    public class TransactionOperationDTO
    {
        [Required] public string senderCardNumber { get; set; } = string.Empty;
        [Required] public string receiverCardNumber { get; set; } = string.Empty;
        [Required] public BankingEnums.Currency senderCurrency { get; set; }
        [Required] public BankingEnums.Currency receiverCurrency { get; set; }
        [Required] public decimal money { get; set; }
        public string description { get; set; } = string.Empty;
    }

    public class TransactionPageDTO
    {
        [Required] public string cardNumber { get; set; } = string.Empty;
        public BankingEnums.Currency? currency { get; set; }
        [Required] public int page { get; set; }
        public bool ascending { get; set; } = false;
    }
    
    public class TransactionPageCountDTO
    {
        [Required] public string cardNumber { get; set; } = string.Empty;
        public BankingEnums.Currency? Currency { get; set; }
    }
}

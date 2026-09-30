using APIBankingDiplom.DBClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace APIBankingDiplom.Controllers.DTO
{
    public class CardStatusDTO
    {
        [Required] public string cardNumber { get; set; } = string.Empty;
        [Required] public BankingEnums.CardStatus status { get; set; }
    }
}

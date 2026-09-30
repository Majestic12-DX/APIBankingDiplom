using APIBankingDiplom.Controllers.BaseControllers;
using APIBankingDiplom.DBClasses.DBContext;
using APIBankingDiplom.DBClasses.DBModels;
using APIBankingDiplom.DBClasses.Enums;
using APIBankingDiplom.Security.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIBankingDiplom.Controllers.DTO;

namespace APIBankingDiplom.Controllers
{
    [ApiController]
    public class CardController : BaseController
    {
        public CardController(BankingContext context, ILogger<CardController> logger, IConfiguration configuration) : base(context, logger, configuration) { }
        #region Issuing a Card
        private const byte maxCardsOnOneUser = 5;
        [HttpPost("IssueCard")]
        public async Task<IActionResult> AddCardToUser([FromQuery] BankingEnums.CardType cardType)
        {
            UserModel user = GetAuthenticatedUser();

            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                if (await _bankingContext.Cards.CountAsync(c => c.UserId == user.Id && c.Status != BankingEnums.CardStatus.Expired) >= maxCardsOnOneUser)
                    return Conflict(SecurityMeasures.GenerateErrorObject("You have reached maximum card amount."));

                // Issue User new PIN every time a new card has been issued
                string PIN = _bankingContext.GeneratePersonalIdentificationNumber();
                user.PINEncrypt = SecurityMeasures.AESEncrypt(PIN);

                // Expire Date and Status are already predetermined in DBClasses/DBModels/CardModel.cs
                string cardNumber = await _bankingContext.GenerateCardNumberAsync(user.Id);
                await _bankingContext.Cards.AddAsync(new CardModel()
                {
                    CardNumberHash = SecurityMeasures.HashStringSHA384(cardNumber),
                    CardNumberEncrypt = SecurityMeasures.AESEncrypt(cardNumber),
                    CardType = cardType,
                    CVVEncrypt = SecurityMeasures.AESEncrypt(_bankingContext.GenerateCardVerificationValue()),
                    UserId = user.Id,
                    User = user,
                });

                await _bankingContext.SaveChangesAsync();

                await SecurityMeasures.SendEmailAsync(SecurityMeasures.AESDecrypt(user.EmailEncrypt), "PIN has been updated",
                    $"Hello, {SecurityMeasures.AESDecrypt(user.NameEncrypt)}. Your Personal Identification Number has been updated.\n\n New PIN: {PIN}");

                return Ok();
            }, null, user);
        }
        #endregion
        #region Get User Cards
        [HttpGet("GetUserCards")]
        public async Task<IActionResult> GetUserCardsAsync()
        {
            UserModel user = GetAuthenticatedUser();

            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                List<CardModel> cardsList = await _bankingContext.Cards.Where(c => c.UserId == user.Id).ToListAsync();
                foreach (CardModel card in cardsList)
                {
                    card.CardNumberEncrypt = SecurityMeasures.AESDecrypt(card.CardNumberEncrypt);
                    card.CVVEncrypt = SecurityMeasures.AESDecrypt(card.CVVEncrypt);
                    card.ExpireDateEncrypt = SecurityMeasures.AESDecrypt(card.ExpireDateEncrypt);
                }

                return Ok(new { cardsList });
            }, null, user);
        }
        #endregion
        #region Set Card Status
        [HttpPatch("SetCardStatus")]
        public async Task<IActionResult> ChangeCardStatusAsync([FromBody] CardStatusDTO cardStatusModel)
        {
            if (cardStatusModel.status is BankingEnums.CardStatus.Expired)
                return BadRequest(SecurityMeasures.GenerateErrorObject("User cannot expire cards."));

            UserModel user = GetAuthenticatedUser();

            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                CardModel card = await _bankingContext.GetUserCardAsync(cardStatusModel.cardNumber, user.Id);
                if (card is null)
                    return BadRequest(SecurityMeasures.CardNotOwnedOrNull);

                if (card.IsExpired())
                    return StatusCode(StatusCodes.Status403Forbidden, SecurityMeasures.CardIsExpired);

                card.Status = cardStatusModel.status;
                await _bankingContext.SaveChangesAsync();

                return Ok();
            }, null, user);
        }
        #endregion
    }
}

using APIBankingDiplom.Controllers.BaseControllers;
using APIBankingDiplom.DBClasses.DBContext;
using APIBankingDiplom.DBClasses.DBModels;
using APIBankingDiplom.DBClasses.Enums;
using APIBankingDiplom.GeneralUtilities.Factories;
using APIBankingDiplom.GeneralUtilities.Models;
using APIBankingDiplom.Security.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIBankingDiplom.Controllers.DTO;

namespace APIBankingDiplom.Controllers
{
    [ApiController]
    public class CardBalanceController : BaseController
    {
        public CardBalanceController(BankingContext context, ILogger<CardBalanceController> logger, IConfiguration configuration) : base(context, logger, configuration) { }
        #region Add Card Balance
        private readonly ObjectResult CantCreateMultipleBalancesWithSameCurrency = ResponseFactory.Create(StatusCodes.Status409Conflict, SecurityMeasures.GenerateErrorObject("Unable to create 2 balances with same currency."));
        [HttpPost("AddCardBalance")]
        public async Task<IActionResult> AddCardBalanceAsync([FromBody] BalanceOperationDTO balanceOperation)
        {
            UserModel user = GetAuthenticatedUser();

            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                var cardAvailabilityResult = await _bankingContext.CheckCardAvailabilityAndGetAsync(balanceOperation.cardNumber, user.Id);
                if (!cardAvailabilityResult.IsSuccess)
                    return cardAvailabilityResult.Response;

                var card = cardAvailabilityResult.Item;

                if (await _bankingContext.GetCardBalanceAsync(card.Id, balanceOperation.currency) is not null)
                    return CantCreateMultipleBalancesWithSameCurrency;

                await _bankingContext.CardBalances.AddAsync(new CardBalanceModel()
                {
                    Currency = BankingEnums.GetCurrencyString(balanceOperation.currency),
                    CardId = card.Id,
                });

                await _bankingContext.SaveChangesAsync();
                return Ok();
            }, null, user);
        }
        #endregion
        #region Get Card Balances
        [HttpGet("GetCardBalances")]
        public async Task<IActionResult> GetCardBalancesAsync([FromQuery] string cardNumber)
        {
            UserModel user = GetAuthenticatedUser();

            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                CardModel card = await _bankingContext.GetUserCardAsync(cardNumber, user.Id);
                if (card is null)
                    return BadRequest(SecurityMeasures.CardNotOwnedOrNull);

                List<CardBalanceModel> balances = await _bankingContext.CardBalances.Where(b => b.CardId == card.Id).ToListAsync();
                return Ok(new { balancesList = balances });
            }, null, user);
        }
        #endregion
        #region Remove Card Balance
        private readonly ObjectResult CannotRemoveEmptyBalance = ResponseFactory.Create(StatusCodes.Status403Forbidden, SecurityMeasures.GenerateErrorObject("Cannot remove non-empty balance."));
        [HttpDelete("RemoveCardBalance")]
        public async Task<IActionResult> RemoveCardBalanceAsync([FromBody] BalanceOperationDTO balanceOperation)
        {
            UserModel user = GetAuthenticatedUser();

            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                AvailabilityResult<CardModel> result = await _bankingContext.CheckCardAvailabilityAndGetAsync(balanceOperation.cardNumber, user.Id);
                if (!result.IsSuccess)
                    return result.Response;

                CardModel card = result.Item;

                CardBalanceModel balance = await _bankingContext.GetCardBalanceAsync(card.Id, balanceOperation.currency);
                if (balance is null)
                    return BadRequest(SecurityMeasures.CardBalanceDoesNotExist);

                if (balance.Balance != decimal.Zero)
                    return CannotRemoveEmptyBalance;

                _bankingContext.CardBalances.Remove(balance);

                await _bankingContext.SaveChangesAsync();
                return Ok();
            }, null, user);
        }
        #endregion
        #region Set Card Balance Status
        [HttpPatch("SetCardBalanceStatus")]
        public async Task<IActionResult> ChangeBalanceStatusAsync([FromBody] BalanceStatusOperationDTO balanceStatusChange)
        {
            UserModel user = GetAuthenticatedUser();

            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                var card = await _bankingContext.GetUserCardAsync(balanceStatusChange.cardNumber, user.Id);
                if (card is null)
                    return NotFound(SecurityMeasures.CardNotOwnedOrNull);

                var balanceAvailabilityResult = await _bankingContext.GetCardBalanceAsync(card!.Id, balanceStatusChange.currency);
                if (balanceAvailabilityResult is null)
                    return NotFound(SecurityMeasures.CardBalanceDoesNotExist);

                balanceAvailabilityResult.Status = balanceStatusChange.status;
                await _bankingContext.SaveChangesAsync();

                return Ok();
            }, null, user);
        }
        #endregion
        #region Limited Status Related Calls
        [HttpPatch("SetDepositLimit")]
        public async Task<IActionResult> ChangeDepositLimitAsync([FromBody] BalanceLimitOperationDTO balanceStatusChange)
        {
            UserModel user = GetAuthenticatedUser();

            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                var balanceAvailabilityResult = await _bankingContext.CheckBalanceAvailabilityAndGetAsync(balanceStatusChange.cardNumber, user.Id, balanceStatusChange.currency);
                if (!balanceAvailabilityResult.IsSuccess)
                    return balanceAvailabilityResult.Response;

                balanceAvailabilityResult.Item.ATM_DepositLimit = balanceStatusChange.limit;

                await _bankingContext.SaveChangesAsync();
                return Ok();
            }, null, user);
        }

        [HttpPatch("SetWithdrawalLimit")]
        public async Task<IActionResult> ChangeWithdrawalLimitAsync([FromBody] BalanceLimitOperationDTO balanceStatusChange)
        {
            UserModel user = GetAuthenticatedUser();

            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                var balanceAvailabilityResult = await _bankingContext.CheckBalanceAvailabilityAndGetAsync(balanceStatusChange.cardNumber, user.Id, balanceStatusChange.currency);
                if (!balanceAvailabilityResult.IsSuccess)
                    return balanceAvailabilityResult.Response;

                balanceAvailabilityResult.Item.ATM_WithdrawalLimit = balanceStatusChange.limit;

                await _bankingContext.SaveChangesAsync();
                return Ok();
            }, null, user);
        }

        [HttpPatch("SetTransactionLimit")]
        public async Task<IActionResult> ChangeTransactionLimitAsync([FromBody] BalanceLimitOperationDTO balanceStatusChange)
        {
            UserModel user = GetAuthenticatedUser();

            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                var balanceAvailabilityResult = await _bankingContext.CheckBalanceAvailabilityAndGetAsync(balanceStatusChange.cardNumber, user.Id, balanceStatusChange.currency);
                if (!balanceAvailabilityResult.IsSuccess)
                    return balanceAvailabilityResult.Response;

                balanceAvailabilityResult.Item.TransactionLimit = balanceStatusChange.limit;

                await _bankingContext.SaveChangesAsync();
                return Ok();
            }, null, user);
        }

        private static readonly ObjectResult CantLimitUnlimitedCard = ResponseFactory.Create(StatusCodes.Status403Forbidden, SecurityMeasures.GenerateErrorObject("Can't limit unlimited card balance"));
        private static readonly AvailabilityResult<CardBalanceModel> CardIsNotLimited = AvailabilityFactory.Create<CardBalanceModel>(null, CantLimitUnlimitedCard);
        private async Task<AvailabilityResult<CardBalanceModel>> CheckLimitedBalanceAvailabilityAndGet(string cardNumber, int userId, BankingEnums.Currency currency, bool hash = true)
        {
            var cardBalanceResult = await _bankingContext.CheckBalanceAvailabilityAndGetAsync(cardNumber, userId, currency, 0, hash);

            if (!cardBalanceResult.Item!.IsLimited())
                return CardIsNotLimited;

            return cardBalanceResult;
        }
        #endregion
    }
}

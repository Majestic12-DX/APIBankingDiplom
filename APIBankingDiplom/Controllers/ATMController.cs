using APIBankingDiplom.CommonResources.Utilities;
using APIBankingDiplom.Controllers.BaseControllers;
using APIBankingDiplom.DBClasses.DBContext;
using APIBankingDiplom.DBClasses.DBModels;
using APIBankingDiplom.DBClasses.Enums;
using APIBankingDiplom.GeneralUtilities.Factories;
using APIBankingDiplom.Security.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIBankingDiplom.Controllers.DTO;

namespace APIBankingDiplom.Controllers
{
    [ApiController]
    [Authorize]
    public class ATMController : BaseController
    {
        public ATMController(BankingContext context, ILogger<ATMController> logger, IConfiguration configuration) : base(context, logger, configuration) { }
        #region ATM Money operations
        // This is a "simulation" of real-life alike ATM operations
        // It is a simplified version of actual ATM operations
        [HttpPatch("ATM_Deposit")]
        [AllowAnonymous]
        public async Task<IActionResult> DepositAsync([FromBody] ATMOperationDTO ATMOperation)
        {
            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                return await HandleATMOperationAsync(ATMOperation, BankingEnums.ATMOperationType.Deposit);
            }, null, null, SecurityMeasures.DatabaseConnectionIssue);
        }

        [HttpPatch("ATM_Withdraw")]
        [AllowAnonymous]
        public async Task<IActionResult> WithdrawAsync([FromBody] ATMOperationDTO ATMOperation)
        {
            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                return await HandleATMOperationAsync(ATMOperation, BankingEnums.ATMOperationType.Withdraw);
            }, null, null, SecurityMeasures.DatabaseConnectionIssue);
        }

        // This clearly does not belong anywhere else and absolutely cannot be reused anywhere else than here
        // So this method stay here
        private readonly ObjectResult ATMDepositLimit = ResponseFactory.Create(StatusCodes.Status403Forbidden, SecurityMeasures.GenerateErrorObject("ATM Deposit card limit hit!"));
        private readonly ObjectResult ATMWithdrawLimit = ResponseFactory.Create(StatusCodes.Status403Forbidden, SecurityMeasures.GenerateErrorObject("ATM Withdraw card limit hit!"));
        private readonly object InvalidParameters = SecurityMeasures.GenerateErrorObject("One or more parameters are invalid");
        private async Task<IActionResult> HandleATMOperationAsync(ATMOperationDTO ATMOperation, BankingEnums.ATMOperationType aTMOperationType)
        {
            string hashedCardNumber = SecurityMeasures.HashStringSHA384(ATMOperation.cardNumber);
            CardModel? card = await _bankingContext.Cards.Include(c => c.User).FirstOrDefaultAsync(c => c.CardNumberHash.Equals(hashedCardNumber));

            // For now there are only withdraw and deposit ATM operations, but of course, it can be easily extended if needed
            bool withdraw = aTMOperationType is BankingEnums.ATMOperationType.Withdraw;

            if (!withdraw && ATMOperation.money < 0)
                return BadRequest(SecurityMeasures.CardBalanceMoneyMustBePositive);

            // Get required card balance, if balance and card are available
            var balanceAvailability = await _bankingContext.CheckBalanceAvailabilityAndGetAsync(card, ATMOperation.currency, withdraw ? ATMOperation.money : 0);
            if (!balanceAvailability.IsSuccess)
                return balanceAvailability.Response;

            // We check if all card parameters and PIN are correct, first let's decrypt the data
            string cardExpireDate = SecurityMeasures.AESDecrypt(card!.ExpireDateEncrypt);
            string cardCVV = SecurityMeasures.AESDecrypt(card!.CVVEncrypt);
            string userPIN = SecurityMeasures.AESDecrypt(card.User!.PINEncrypt);

            if (!cardExpireDate.Equals(ATMOperation.expireDate) || !cardCVV.Equals(ATMOperation.CVV)
                || card.CardType != ATMOperation.cardType || !userPIN.Equals(ATMOperation.PIN))
                return BadRequest(InvalidParameters);

            var balance = balanceAvailability.Item;
            decimal amount = ATMOperation.money;
            decimal todayAmount = withdraw ? balance.ATM_WithdrawnToday + amount : balance.ATM_DepositedToday + amount;
            decimal limit = withdraw ? balance.ATM_WithdrawalLimit : balance.ATM_DepositLimit;

            if (balance.IsLimited() && todayAmount > limit)
                return withdraw ? ATMWithdrawLimit : ATMDepositLimit;

            if (withdraw)
            {
                balance.ATM_WithdrawnToday = todayAmount;
                balance.LastWithdrawalDate = DateUtility.GetCurrentTime();

                balance.Balance -= amount;
            }
            else
            {
                balance.ATM_DepositedToday = todayAmount;
                balance.LastDepositDate = DateUtility.GetCurrentTime();

                balanceAvailability.Item.Balance += amount;
            }

            await _bankingContext.ATMOperations.AddAsync(new ATMOperationModel()
            {
                Money = ATMOperation.money,
                ATMOperationType = aTMOperationType,

                CardId = balance.CardId,
                CardBalanceId = balance.Id
            });

            await _bankingContext.SaveChangesAsync();
            return Ok();
        }
        #endregion
        #region Get ATM Operations
        private const byte atmPageSize = 5;
        [HttpGet("GetATMOperationsByTime")]
        public async Task<IActionResult> GetTransactionsByTimeAsync([FromQuery] ATMOperationPageDTO operationPage)
        {
            UserModel user = GetAuthenticatedUser();

            if (operationPage.page <= 0)
                return BadRequest(SecurityMeasures.PageNumberMustBePositive);

            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                CardModel? card = await _bankingContext.GetUserCardAsync(operationPage.cardNumber, user.Id);
                if (card is null)
                    return NotFound(SecurityMeasures.CardNotOwnedOrNull);

                CardBalanceModel? cardBalance = null;
                if (operationPage.currency.HasValue)
                {
                    cardBalance = await _bankingContext.GetCardBalanceAsync(card.Id, operationPage.currency.Value);
                    if (cardBalance is null)
                        return NotFound(SecurityMeasures.CardBalanceDoesNotExist);
                }

                var operationsQueryUnorganized = await _bankingContext.ATMOperations.Where(o => o.CardId == card.Id && (cardBalance == null || o.CardBalanceId == cardBalance.Id)).ToListAsync();

                var operationsQuery = operationPage.ascending
                    ? operationsQueryUnorganized.OrderBy(t => t.Date)
                    : operationsQueryUnorganized.OrderByDescending(t => t.Date);

                List<ATMOperationModel> operations = operationsQuery.Skip((operationPage.page - 1) * atmPageSize)
                                                                             .Take(atmPageSize)
                                                                             .ToList();

                return Ok(new { operations });
            }, null, user);
        }

        [HttpGet("GetATMOperationsPageCount")]
        public async Task<IActionResult> GetATMOperationsPageCountAsync([FromQuery] ATMPageCountDTO pageCount)
        {
            UserModel user = GetAuthenticatedUser();

            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                CardModel? card = await _bankingContext.GetUserCardAsync(pageCount.cardNumber, user.Id);
                if (card is null)
                    return NotFound(SecurityMeasures.CardNotOwnedOrNull);

                CardBalanceModel? cardBalance = null;
                if (pageCount.Currency.HasValue)
                    cardBalance = await _bankingContext.GetCardBalanceAsync(card.Id, pageCount.Currency.Value);

                int atmOperationsCount = await _bankingContext.ATMOperations.Where(t => t.CardId == card.Id && (cardBalance == null || t.CardBalanceId == cardBalance.Id)).CountAsync();

                // It only works with decimals and doubles, we will use double. We divide amount of ATM operations by page size and get our pages count
                double pagesCount = Math.Ceiling((double)atmOperationsCount / atmPageSize);

                return Ok(new { pagesCount });
            }, null, user);
        }
        #endregion
    }
}

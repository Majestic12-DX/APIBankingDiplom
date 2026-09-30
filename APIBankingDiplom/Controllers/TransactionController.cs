using APIBankingDiplom.CommonResources.Utilities;
using APIBankingDiplom.Controllers.BaseControllers;
using APIBankingDiplom.DBClasses.DBContext;
using APIBankingDiplom.DBClasses.DBModels;
using APIBankingDiplom.Security.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIBankingDiplom.Controllers.DTO;
using APIBankingDiplom.DBClasses.Enums;
using APIBankingDiplom.CommonResources.Extensions;

namespace APIBankingDiplom.Controllers
{
    [ApiController]
    public class TransactionController : BaseController
    {
        public TransactionController(BankingContext context, ILogger<TransactionController> logger, IConfiguration configuration) : base(context, logger, configuration) { }
        #region Transfer Money
        private const string Transaction_CardToCard = "Card-To-Card";
        private const decimal CardToCardFee = 0.02m;

        private readonly object CannotTransferMoneyToSameCardBalance = SecurityMeasures.GenerateErrorObject("Cannot transfer money to the same card balance");
        private readonly object CannotTransferZero = SecurityMeasures.GenerateErrorObject("Transfer amount must be greater than zero");
        private readonly object CardOwnerNotFound = SecurityMeasures.GenerateErrorObject("Could not find receiver card owner");
        private readonly object ConversionRateNotFound = SecurityMeasures.GenerateErrorObject("Conversion rate for sender currency to receiver currency not found!");
        [HttpPost("TransferMoney")]
        public async Task<IActionResult> TransferMoney([FromBody] TransactionOperationDTO transactionOperation)
        {
            if (transactionOperation.money <= decimal.Zero)
                return BadRequest(CannotTransferZero);

            bool isSameCard = transactionOperation.senderCardNumber.Equals(transactionOperation.receiverCardNumber);

            if (isSameCard && transactionOperation.senderCurrency == transactionOperation.receiverCurrency)
                return BadRequest(CannotTransferMoneyToSameCardBalance);

            UserModel user = GetAuthenticatedUser();
            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                decimal moneySent = transactionOperation.money;
                decimal moneyReceived = transactionOperation.money;

                // Search for Receiver Card Owner, if not found, cancel transaction
                var findReceiverCardUser = GetReceiverCardUserIdAsync(transactionOperation.receiverCardNumber);

                // Handle currency conversion
                var getCurrencyConversion = HandleAndReturnCurrencyConversionAsync(transactionOperation.senderCurrency, transactionOperation.receiverCurrency, moneySent);

                // Run them in parallel
                await Task.WhenAll(findReceiverCardUser, getCurrencyConversion);

                // Check if something went wrong
                var receiverCardUser = findReceiverCardUser.Result;
                if (!receiverCardUser.HasValue)
                    return NotFound(CardOwnerNotFound);

                var currencyConversion = getCurrencyConversion.Result;
                if (!currencyConversion.HasValue)
                    return NotFound(ConversionRateNotFound);
                // Check if something went wrong

                // Apply currency conversion
                moneyReceived = currencyConversion.Value;

                // Try withdrawing money, and apply calculated fee
                var senderWithdrawal = await _bankingContext.WithdrawAsync(transactionOperation.senderCardNumber, user.Id, transactionOperation.senderCurrency, moneySent, CalculateFee(isSameCard));
                if (!senderWithdrawal.IsSuccess)
                    return senderWithdrawal.Response;

                // Try depositing the money
                var receiverDeposit = await _bankingContext.DepositAsync(transactionOperation.receiverCardNumber, receiverCardUser.Value, transactionOperation.receiverCurrency, moneyReceived);
                if (!receiverDeposit.IsSuccess)
                    return receiverDeposit.Response;

                // Now we just add transaction to database, update transaction related values in those card balances, and ensure that transaction limit was not hit!
                var RecordResult = await _bankingContext.RecordTransactionAndProcessBalances(Transaction_CardToCard, moneySent, moneyReceived, transactionOperation.senderCurrency,
                                                          transactionOperation.receiverCurrency, transactionOperation.description, senderWithdrawal.Item, receiverDeposit.Item);
                if (!RecordResult.IsSuccess())
                    return RecordResult;

                await _bankingContext.SaveChangesAsync();
                return Ok();
            }, null, user);
        }

        #region Helper Methods
        private async Task<decimal?> HandleAndReturnCurrencyConversionAsync(BankingEnums.Currency senderCurrency, BankingEnums.Currency receiverCurrency, decimal moneySent)
        {
            if (!senderCurrency.Equals(receiverCurrency))
            {
                decimal? conversionRate = await CurrencyConvertionUtility.GetConversionRateAsync(senderCurrency, receiverCurrency);
                if (!conversionRate.HasValue)
                    return null;

                return Math.Round(moneySent * conversionRate.Value, 2);
            }

            return moneySent;
        }

        private async Task<int?> GetReceiverCardUserIdAsync(string receiverCardNumber)
        {
            string receiverCardNumberHashed = SecurityMeasures.HashStringSHA384(receiverCardNumber);
            return await _bankingContext.Cards
                .Where(c => c.CardNumberHash.Equals(receiverCardNumberHashed))
                .Select(c => (int?)c.UserId)
                .FirstOrDefaultAsync();
        }

        // Suitable for expansion
        private decimal CalculateFee(bool sameCard)
        {
            return sameCard ? 0m : CardToCardFee;
        }
        #endregion
        #endregion
        #region Get Transactions
        private const byte transactionPageSize = 5;
        [HttpGet("GetTransactionsByTime")]
        public async Task<IActionResult> GetTransactionsByTimeAsync([FromQuery] TransactionPageDTO transactionPage)
        {
            UserModel user = GetAuthenticatedUser();

            if (transactionPage.page <= 0)
                return BadRequest(SecurityMeasures.PageNumberMustBePositive);

            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                CardModel? card = await _bankingContext.GetUserCardAsync(transactionPage.cardNumber, user.Id);
                if (card is null)
                    return NotFound(SecurityMeasures.CardNotOwnedOrNull);

                CardBalanceModel? cardBalance = null;
                if (transactionPage.currency.HasValue)
                {
                    cardBalance = await _bankingContext.GetCardBalanceAsync(card.Id, transactionPage.currency.Value);
                    if (cardBalance is null)
                        return NotFound(SecurityMeasures.CardBalanceDoesNotExist);
                }

                var transactionsQueryUnorganized = await _bankingContext.Transactions.Where(t => (t.SenderCardId == card.Id || t.ReceiverCardId == card.Id) &&
                                                    (cardBalance == null || t.SenderBalanceId == cardBalance.Id || t.ReceiverBalanceId == cardBalance.Id)).ToListAsync();

                var transactionsQuery = transactionPage.ascending
                    ? transactionsQueryUnorganized.OrderBy(t => t.TransactionDate)
                    : transactionsQueryUnorganized.OrderByDescending(t => t.TransactionDate);

                List<TransactionModel> transactions = transactionsQuery.Skip((transactionPage.page - 1) * transactionPageSize)
                                                                             .Take(transactionPageSize)
                                                                             .ToList();

                foreach (var transaction in transactions)
                {
                    transaction.TransactionTypeEncrypt = SecurityMeasures.AESDecrypt(transaction.TransactionTypeEncrypt);
                    transaction.AmountSentEncrypt = SecurityMeasures.AESDecrypt(transaction.AmountSentEncrypt);
                    transaction.AmountReceivedEncrypt = SecurityMeasures.AESDecrypt(transaction.AmountReceivedEncrypt);
                    transaction.DescriptionEncrypt = SecurityMeasures.AESDecrypt(transaction.DescriptionEncrypt);
                }

                return Ok(new { transactions });
            }, null, user);
        }

        [HttpGet("GetTransactionsPageCount")]
        public async Task<IActionResult> GetTransactionsPageCountAsync([FromQuery] TransactionPageCountDTO pageCount)
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

                double transactionsCount = await _bankingContext.Transactions.Where(t => (t.SenderCardId == card.Id || t.ReceiverCardId == card.Id)
                                                                                   && (cardBalance == null || t.SenderBalanceId == cardBalance.Id || t.ReceiverBalanceId == cardBalance.Id))
                                                                                   .CountAsync();

                // It only works with decimals and doubles, we will use double. We divide amount of transactions by page size and get our pages count
                double pagesCount = Math.Ceiling(transactionsCount / transactionPageSize);

                return Ok(new { pagesCount });
            }, null, user);
        }
        #endregion
    }
}

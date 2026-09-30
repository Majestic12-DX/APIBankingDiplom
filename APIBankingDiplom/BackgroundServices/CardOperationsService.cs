using APIBankingDiplom.CommonResources.Utilities;
using APIBankingDiplom.DBClasses.DBContext;
using APIBankingDiplom.DBClasses.DBModels;
using APIBankingDiplom.DBClasses.Enums;
using APIBankingDiplom.Security.Utilities;
using Microsoft.EntityFrameworkCore;

namespace APIBankingDiplom.BackgroundServices
{
    public class CardOperationsService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public CardOperationsService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        #region Timer Function
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessCards();
                await Task.Delay(DateUtility.GetTimeTillNextMidnight(), stoppingToken); // Run every midnight
            }
        }
        #endregion

        #region Expiring cards and resetting daily limits
        private async Task ProcessCards()
        {
            // Ensure of proper disposal
            // Scoped service is great for DbContext
            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                DateTime now = DateUtility.GetCurrentTime();
                await SecurityMeasures.TryExecutingAsync(async () =>
                {
                    BankingContext bankingContext = scope.ServiceProvider.GetRequiredService<BankingContext>();

                    #region Expire Expired Cards
                    string dateHash = SecurityMeasures.HashStringSHA384( DateUtility.GetCurrentDateInShortString() );
                    CardModel[] cardsToExpire = await bankingContext.Cards.Where(c => c.ExpireDateHash.Equals(dateHash)).ToArrayAsync();
                    foreach (var card in cardsToExpire)
                        card.Status = BankingEnums.CardStatus.Expired;
                    #endregion

                    #region Clear Today statistic values for Card Balances
                    CardBalanceModel[] cardBalances = await bankingContext.CardBalances.Where(c => c.ATM_DepositedToday != 0 || c.ATM_WithdrawnToday != 0 || c.TransactedToday != 0).ToArrayAsync();
                    foreach (var balance in cardBalances)
                    {
                        balance.ATM_DepositedToday = 0;
                        balance.ATM_WithdrawnToday = 0;
                        balance.TransactedToday = 0;
                    }
                    #endregion

                    await bankingContext.SaveChangesAsync();
                });
            }
        }
        #endregion

    }
}

using APIBankingDiplom.CommonResources.Utilities;
using APIBankingDiplom.DBClasses.DBContext;
using APIBankingDiplom.DBClasses.DBModels;
using APIBankingDiplom.Security.Utilities;
using Microsoft.EntityFrameworkCore;

namespace APIBankingDiplom.BackgroundServices
{
    public class CommissionCollectingService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public CommissionCollectingService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        #region Timer Function
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CollectCommissions();
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Run every minute
            }
        }
        #endregion

        #region Cleaning up expired tokens
        private async Task CollectCommissions()
        {
            // Ensure of proper disposal
            // Scoped service is great for DbContext
            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                DateTime now = DateUtility.GetCurrentTime();
                await SecurityMeasures.TryExecutingAsync(async () =>
                {
                    BankingContext bankingContext = scope.ServiceProvider.GetRequiredService<BankingContext>();

                    CommissionModel[] unsentCommissions = await bankingContext.Commissions.Where(c => !c.IsSentToBankAccount).ToArrayAsync();

                    var commissionsByCurrency = unsentCommissions.GroupBy(c => c.Currency)
                                                                 .ToDictionary(
                                                                    g => g.Key,
                                                                    g => g.ToArray()
                                                                  );

                    foreach (var entry in commissionsByCurrency)
                    {
                        var currency = entry.Key;
                        var commissions = entry.Value;

                        var bankBalance = await bankingContext.BankBalances.FirstOrDefaultAsync(c => c.Currency == currency);

                        if (bankBalance is null)
                        {
                            bankBalance = new BankBalanceModel() { Currency = currency };
                            await bankingContext.BankBalances.AddAsync(bankBalance);
                        }

                        decimal totalCommission = 0;
                        string decryptedAmount;
                        foreach (var commission in commissions) 
                        {
                            decryptedAmount = SecurityMeasures.AESDecrypt(commission.AmountEncrypt);

                            totalCommission += Convert.ToDecimal(decryptedAmount);
                            commission.IsSentToBankAccount = true;
                        }

                        decimal bankBalanceMoney = Convert.ToDecimal( SecurityMeasures.AESDecrypt(bankBalance!.MoneyEncrypt) );
                        bankBalanceMoney += totalCommission;

                        bankBalance.MoneyEncrypt = SecurityMeasures.AESEncrypt(bankBalanceMoney.ToString());
                    }

                    await bankingContext.SaveChangesAsync();
                });
            }
        }
        #endregion

    }
}

using APIBankingDiplom.CommonResources.Utilities;
using APIBankingDiplom.DBClasses.DBContext;
using APIBankingDiplom.DBClasses.DBModels;
using APIBankingDiplom.DBClasses.DBModels.Tokens;
using APIBankingDiplom.Security.Utilities;
using Microsoft.EntityFrameworkCore;

namespace APIBankingDiplom.BackgroundServices
{
    public class InvalidTokensCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public InvalidTokensCleanupService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        #region Timer Function
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CleanUpExpiredTokens();
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Run every minute
            }
        }
        #endregion

        #region Cleaning up expired tokens
        private async Task CleanUpExpiredTokens()
        {
            // Ensure of proper disposal
            // Scoped service is great for DbContext
            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                DateTime now = DateUtility.GetCurrentTime();
                await SecurityMeasures.TryExecutingAsync(async () =>
                {
                    BankingContext bankingContext = scope.ServiceProvider.GetRequiredService<BankingContext>();

                    #region Deleting Expired Access Tokens
                    // Revoked tokens must survive until ExpireDate, ValidateIdentityFilter proves revocation by finding the row
                    AccessTokenModel[] expiredAccessTokens = await bankingContext.AccessTokens.Where(t => t.ExpireDate < now).ToArrayAsync();
                    bankingContext.AccessTokens.RemoveRange(expiredAccessTokens);
                    #endregion

                    #region Deleting Expired Refresh Tokens
                    // Check JWT/AuthOptions.cs for the lifetime
                    RefreshTokenModel[] expiredRefreshTokens = await bankingContext.RefreshTokens.Where(t => t.ExpireDate < now || t.IsInvalid).ToArrayAsync();
                    bankingContext.RefreshTokens.RemoveRange(expiredRefreshTokens);
                    #endregion

                    #region Deleting Expired Email Tokens
                    // Check JWT/AuthOptions.cs for the lifetime
                    EmailTokenModel[] expiredEmailTokens = await bankingContext.EmailTokens.Include(t => t.User).Where(t => t.ExpireDate < now || t.IsInvalid).ToArrayAsync();
                    bankingContext.EmailTokens.RemoveRange(expiredEmailTokens);
                    #endregion

                    #region Deleting Users that have not validated their Email in time
                    // A verified User can still own a stale email token, deleting on token expiry alone would destroy a live account
                    UserModel[] invalidatedUsers = expiredEmailTokens.Where(t => t.User is not null && !t.User.IsEmailVerified)
                                                                     .Select(t => t.User!)
                                                                     .ToArray();

                    if (invalidatedUsers.Length > 0)
                    {
                        int[] invalidatedUserIds = invalidatedUsers.Select(u => u.Id).ToArray();

                        // ResetPassword is anonymous and does not require a verified email, so these can exist and every User foreign key is Restrict
                        PasswordResetTokenModel[] orphanedResetTokens = await bankingContext.PasswordResetTokens.Where(t => invalidatedUserIds.Contains(t.UserId)).ToArrayAsync();
                        bankingContext.PasswordResetTokens.RemoveRange(orphanedResetTokens);

                        bankingContext.Users.RemoveRange(invalidatedUsers);
                    }
                    #endregion

                    #region Deleting Expired Email Change Tokens
                    // Check JWT/AuthOptions.cs for the lifetime
                    EmailChangeTokenModel[] expiredEmailChangeTokens = await bankingContext.EmailChangeTokens.Where(t => t.ExpireDate < now || t.IsInvalid).ToArrayAsync();
                    bankingContext.EmailChangeTokens.RemoveRange(expiredEmailChangeTokens);
                    #endregion

                    #region Deleting Expired Password Reset Tokens
                    PasswordResetTokenModel[] expiredPasswordResetTokens = await bankingContext.PasswordResetTokens.Where(t => t.ExpireDate < now || t.IsInvalid).ToArrayAsync();
                    bankingContext.PasswordResetTokens.RemoveRange(expiredPasswordResetTokens);
                    #endregion

                    await bankingContext.SaveChangesAsync();
                });
            }
        }
        #endregion

    }
}

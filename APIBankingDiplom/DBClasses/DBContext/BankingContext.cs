using APIBankingDiplom.CommonResources.Extensions;
using APIBankingDiplom.CommonResources.Utilities;
using APIBankingDiplom.DBClasses.DBModels;
using APIBankingDiplom.DBClasses.DBModels.Tokens;
using APIBankingDiplom.DBClasses.Enums;
using APIBankingDiplom.GeneralUtilities.Factories;
using APIBankingDiplom.GeneralUtilities.Models;
using APIBankingDiplom.Security.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace APIBankingDiplom.DBClasses.DBContext
{
    public sealed class BankingContext : DbContext
    {
        #region DB Tables
        public DbSet<UserModel> Users { get; set; }
        public DbSet<TransactionModel> Transactions { get; set; }
        public DbSet<ATMOperationModel> ATMOperations { get; set; }
        public DbSet<CardModel> Cards { get; set; }
        public DbSet<CardBalanceModel> CardBalances { get; set; }
        public DbSet<NotificationModel> Notifications { get; set; }
        public DbSet<SettingModel> Settings { get; set; }
        public DbSet<LogModel> Logs { get; set; }
        public DbSet<CommissionModel> Commissions { get; set; }
        public DbSet<BankBalanceModel> BankBalances { get; set; }
        public DbSet<AccessTokenModel> AccessTokens { get; set; }
        public DbSet<RefreshTokenModel> RefreshTokens { get; set; }
        public DbSet<EmailTokenModel> EmailTokens { get; set; }
        public DbSet<EmailChangeTokenModel> EmailChangeTokens { get; set; }
        public DbSet<PasswordResetTokenModel> PasswordResetTokens { get; set; }
        #endregion
        #region Connect to database
        private static readonly string connectionString = Environment.GetEnvironmentVariable("API_DATABASE_CONNECTION_STRING") ?? throw new InvalidOperationException("API_DATABASE_CONNECTION_STRING is missing!");
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
        #endregion
        #region Setting up Foreign Keys and other minor things
        // Foreign keys will be done like this because shadow properties allow values to be NULLs, not good for our case!
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransactionModel>()
                .HasOne(t => t.SenderCard)
                .WithMany()
                .HasForeignKey(t => t.SenderCardId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransactionModel>()
                .HasOne(t => t.SenderBalance)
                .WithMany()
                .HasForeignKey(t => t.SenderBalanceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransactionModel>()
                .HasOne(t => t.ReceiverCard)
                .WithMany()
                .HasForeignKey(t => t.ReceiverCardId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransactionModel>()
                .HasOne(t => t.ReceiverBalance)
                .WithMany()
                .HasForeignKey(t => t.ReceiverBalanceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ATMOperationModel>()
                .HasOne(t => t.Card)
                .WithMany()
                .HasForeignKey(t => t.CardId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ATMOperationModel>()
                .HasOne(t => t.CardBalance)
                .WithMany()
                .HasForeignKey(t => t.CardBalanceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ATMOperationModel>()
                .Property(t => t.Money)
                .HasColumnType("decimal(15,2)");

            modelBuilder.Entity<CardModel>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CardBalanceModel>()
                .HasOne(t => t.Card)
                .WithMany()
                .HasForeignKey(t => t.CardId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CardBalanceModel>()
                .Property(t => t.Balance)
                .HasColumnType("decimal(15,2)");

            modelBuilder.Entity<CardBalanceModel>()
                .Property(t => t.ATM_DepositedToday)
                .HasColumnType("decimal(15,2)");

            modelBuilder.Entity<CardBalanceModel>()
                .Property(t => t.ATM_DepositLimit)
                .HasColumnType("decimal(15,2)");

            modelBuilder.Entity<CardBalanceModel>()
                .Property(t => t.ATM_WithdrawnToday)
                .HasColumnType("decimal(15,2)");

            modelBuilder.Entity<CardBalanceModel>()
                .Property(t => t.ATM_WithdrawalLimit)
                .HasColumnType("decimal(15,2)");

            modelBuilder.Entity<CardBalanceModel>()
                .Property(t => t.TransactedToday)
                .HasColumnType("decimal(15,2)");

            modelBuilder.Entity<CardBalanceModel>()
                .Property(t => t.TransactionLimit)
                .HasColumnType("decimal(15,2)");

            modelBuilder.Entity<NotificationModel>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SettingModel>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LogModel>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CommissionModel>()
                .HasOne(t => t.Card)
                .WithMany()
                .HasForeignKey(t => t.CardId)
                .OnDelete(DeleteBehavior.Restrict);

            // We keep commission percentage as a 4 char string, it takes less space than a whole decimal
            //modelBuilder.Entity<CommissionModel>()
            //    .Property(t => t.CommissionPercentage)
            //    .HasColumnType("decimal(15,2)");

            modelBuilder.Entity<AccessTokenModel>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RefreshTokenModel>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmailTokenModel>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmailChangeTokenModel>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PasswordResetTokenModel>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        #endregion
        #region Extended DB functions
        // Most of these functions are used in only one controller and I could place them
        // Inside their related controllers to reduce memory consumption
        // But I dont want to hinder code readability
        #region User Tokens workings
        // Adds new user access token to database
        public async Task AddUserAccessTokenAsync(UserModel user, JwtSecurityToken accessToken)
        {
            if (user is null || accessToken is null)
                return;

            await AccessTokens.AddAsync(new AccessTokenModel()
            {
                TokenHash = new JwtSecurityTokenHandler().WriteToken(accessToken),
                ExpireDate = accessToken.ValidTo,
                UserId = user.Id
            });
        }

        // Adds new user refresh token to database
        public async Task AddUserRefreshTokenAsync(UserModel user, JwtSecurityToken refreshToken)
        {
            if (user is null || refreshToken is null)
                return;

            await RefreshTokens.AddAsync(new RefreshTokenModel()
            {
                TokenHash = new JwtSecurityTokenHandler().WriteToken(refreshToken),
                ExpireDate = refreshToken.ValidTo,
                UserId = user.Id
            });
        }

        // Invalidates all tokens connected to the User
        // Returns false if operation was not successful
        public async Task<bool> InvalidateUserTokensAsync(UserModel? user)
        {
            if (user is null)
                return false;

            AccessTokenModel[] accessTokens = await AccessTokens.Where(t => t.UserId == user.Id).ToArrayAsync();
            if (accessTokens.Length > 0)
                foreach (var accessToken in accessTokens)
                    accessToken.IsInvalid = true;

            RefreshTokenModel[] refreshTokens = await RefreshTokens.Where(t => t.UserId == user.Id).ToArrayAsync();
            if (refreshTokens.Length > 0)
                RefreshTokens.RemoveRange(refreshTokens);

            return accessTokens.Length > 0 && refreshTokens.Length > 0;
        }

        // Creates Email Verification token and returns the token (unhashed)
        public async Task<string> CreateEmailVerificationTokenAsync(UserModel user)
        {
            ThrowExceptionForNullUser(user);

            // We check if user exists in database or in change tracker to cover all edge cases
            bool userInDatabase = await Users.AnyAsync(u => u.EmailHash.Equals(user.EmailHash));
            bool userInChangeTracker = ChangeTracker.Entries<UserModel>().Any(e => e.Entity.EmailHash.Equals(user.EmailHash));

            if (!userInDatabase && !userInChangeTracker)
                throw new ArgumentException("User with such ID does not exist", nameof(user));

            string token = GetNewToken();

            // Expire Date is declared in DBClasses/DBModels/EmailToken.cs
            await EmailTokens.AddAsync(new EmailTokenModel()
            {
                TokenHash = token,
                User = user
            });

            return token;
        }

        // Creates Email Change token and returns the token (unhashed)
        public async Task<string> CreateEmailChangeTokenAsync(UserModel user, string email)
        {
            ThrowExceptionForNullUser(user);

            string token = GetNewToken();

            // Expire Date is declared in DBClasses/DBModels/EmailChangeToken.cs
            await EmailChangeTokens.AddAsync(new EmailChangeTokenModel()
            {
                TokenHash = token,
                Email = email,
                User = user
            });

            return token;
        }

        // Creates Password Reset token and returns the token (unhashed)
        public async Task<string> CreatePasswordResetTokenAsync(UserModel user)
        {
            ThrowExceptionForNullUser(user);

            string token = GetNewToken();

            // Expire Date is declared in DBClasses/DBModels/PasswordResetToken.cs
            await PasswordResetTokens.AddAsync(new PasswordResetTokenModel()
            {
                TokenHash = token,
                User = user
            });

            return token;
        }

        private void ThrowExceptionForNullUser(UserModel user)
        {
            if (user.IsNullOrDefault())
                throw new ArgumentException("User is null or default", nameof(user));
        }
        private string GetNewToken() => Guid.NewGuid().ToString();
        #endregion

        #region Get/Exists card-related operations
        // Searches card by Card Number Hash and User ID
        public async Task<CardModel> GetUserCardAsync(string cardNumber, int userId, bool hash = true)
        {
            if (hash)
                cardNumber = SecurityMeasures.HashStringSHA384(cardNumber);

            return (await Cards.FirstOrDefaultAsync(c => c.UserId == userId && c.CardNumberHash.Equals(cardNumber)))!;
        }

        // Searches card balance by Currency and card ID
        public async Task<CardBalanceModel> GetCardBalanceAsync(int cardId, BankingEnums.Currency currency)
        {
            string currencyCode = BankingEnums.GetCurrencyString(currency);
            return (await CardBalances.FirstOrDefaultAsync(b => b.CardId == cardId && b.Currency.Equals(currencyCode)))!;
        }

        // Checks if card exists by Card Number Hash
        public async Task<bool> CardExistsAsync(string cardNumber, bool hash = true)
        {
            if (hash)
                cardNumber = SecurityMeasures.HashStringSHA384(cardNumber);

            return await Cards.AnyAsync(c => c.CardNumberHash.Equals(cardNumber));
        }
        #endregion

        #region Checks to allow card-related operations
        private static readonly ObjectResult CardNotFound = ResponseFactory.Create(StatusCodes.Status404NotFound, SecurityMeasures.CardNotOwnedOrNull);
        private static readonly ObjectResult CardIsBlocked = ResponseFactory.Create(StatusCodes.Status403Forbidden, SecurityMeasures.CardIsBlocked);
        private static readonly ObjectResult CardIsExpired = ResponseFactory.Create(StatusCodes.Status403Forbidden, SecurityMeasures.CardIsExpired);

        // Checks if card exists and is available, returns card (or null) with response
        public AvailabilityResult<CardModel> CheckCardAvailability(CardModel? card)
        {
            if (card is null)
                return AvailabilityFactory.Create<CardModel>(null, CardNotFound);

            if (card.IsBlocked())
                return AvailabilityFactory.Create<CardModel>(null, CardIsBlocked);

            if (card.IsExpired())
                return AvailabilityFactory.Create<CardModel>(null, CardIsExpired);

            return AvailabilityFactory.Create(card, ResponseFactory.Success);
        }
        public async Task<AvailabilityResult<CardModel>> CheckCardAvailabilityAndGetAsync(string cardNumber, int userId, bool hash = true)
        {
            CardModel card = await GetUserCardAsync(cardNumber, userId, hash);

            return CheckCardAvailability(card);
        }

        private static readonly ObjectResult CardBalanceDoesNotExist = ResponseFactory.Create(StatusCodes.Status400BadRequest, SecurityMeasures.CardBalanceDoesNotExist);
        private static readonly ObjectResult CardBalanceInsufficientFunds = ResponseFactory.Create(StatusCodes.Status400BadRequest, SecurityMeasures.CardBalanceInsufficientFunds);
        private static readonly ObjectResult CardBalanceIsBlocked = ResponseFactory.Create(StatusCodes.Status403Forbidden, SecurityMeasures.CardBalanceIsBlocked);
        // Checks if Card Balance exists and is available, returns card balance (or null) with response
        public AvailabilityResult<CardBalanceModel> CheckBalanceAvailability(CardBalanceModel? cardBalance, decimal withdrawMoney = decimal.Zero)
        {
            ObjectResult moneyValidation = ProcessMoneyAmount(ref withdrawMoney);
            if (!moneyValidation.IsSuccess())
                return AvailabilityFactory.Create<CardBalanceModel>(null, moneyValidation);

            if (cardBalance is null)
                return AvailabilityFactory.Create<CardBalanceModel>(null, CardBalanceDoesNotExist);

            if (cardBalance.IsBlocked())
                return AvailabilityFactory.Create<CardBalanceModel>(null, CardBalanceIsBlocked);

            if (cardBalance.Balance < withdrawMoney)
                return AvailabilityFactory.Create<CardBalanceModel>(null, CardBalanceInsufficientFunds);

            return AvailabilityFactory.Create(cardBalance, ResponseFactory.Success);
        }

        // Searches for Card Balance and performs CheckBalanceAvailability on it, returning its result
        public async Task<AvailabilityResult<CardBalanceModel>> CheckBalanceAvailabilityAndGetAsync(string cardNumber, int userId, BankingEnums.Currency currency, decimal withdrawMoney = decimal.Zero, bool hash = true)
        {
            AvailabilityResult<CardModel> cardResult = await CheckCardAvailabilityAndGetAsync(cardNumber, userId, hash);
            if (!cardResult.IsSuccess)
                return AvailabilityFactory.Create<CardBalanceModel>(null, cardResult.Response);

            CardBalanceModel cardBalance = await GetCardBalanceAsync(cardResult.Item.Id, currency);
            return CheckBalanceAvailability(cardBalance, withdrawMoney);
        }

        public async Task<AvailabilityResult<CardBalanceModel>> CheckBalanceAvailabilityAndGetAsync(CardModel? card, BankingEnums.Currency currency, decimal withdrawMoney = decimal.Zero)
        {
            AvailabilityResult<CardModel> cardResult = CheckCardAvailability(card);
            if (!cardResult.IsSuccess)
                return AvailabilityFactory.Create<CardBalanceModel>(null, cardResult.Response);

            CardBalanceModel cardBalance = await GetCardBalanceAsync(cardResult.Item.Id, currency);
            return CheckBalanceAvailability(cardBalance, withdrawMoney);
        }

        private static readonly ObjectResult CardBalanceMoneyMustBePositive = ResponseFactory.Create(StatusCodes.Status400BadRequest, SecurityMeasures.CardBalanceMoneyMustBePositive);
        
        // Self-explanatory method, returns API response
        public ObjectResult ProcessMoneyAmount(ref decimal money)
        {
            if (money < 0)
                return CardBalanceMoneyMustBePositive;

            money = Math.Round(money, 2);

            return ResponseFactory.Success;
        }
        #endregion

        #region Transaction related operations
        // Does deposit operation if card and card balance exist and are available, returns card balance (or null) with API response\
        public async Task<AvailabilityResult<CardBalanceModel>> DepositAsync(string cardNumber, int userId, BankingEnums.Currency currency, decimal money, bool hash = true)
        {
            ObjectResult result = ProcessMoneyAmount(ref money);
            if (!result.IsSuccess())
                return AvailabilityFactory.Create<CardBalanceModel>(null, result);

            AvailabilityResult<CardBalanceModel> balanceResult = await CheckBalanceAvailabilityAndGetAsync(cardNumber, userId, currency, 0, hash);
            if (!balanceResult.IsSuccess)
                return AvailabilityFactory.Create<CardBalanceModel>(null, balanceResult.Response);

            CardBalanceModel balance = balanceResult.Item;
            balance.Balance = balance.Balance + money;

            return AvailabilityFactory.Create(balance, balanceResult.Response);
        }

        // Does withdraw operation if card and card balance exist and are available, returns card balance (or null) with API response
        public async Task<AvailabilityResult<CardBalanceModel>> WithdrawAsync(string cardNumber, int userId, BankingEnums.Currency currency, decimal money, decimal fee = decimal.Zero, bool hash = true)
        {
            // Ensure 2 decimals at max
            decimal commission = Math.Round(money * fee, 2);
            money = money + commission;

            ObjectResult result = ProcessMoneyAmount(ref money);
            if (!result.IsSuccess())
                return AvailabilityFactory.Create<CardBalanceModel>(null, result);

            AvailabilityResult<CardBalanceModel> balanceResult = await CheckBalanceAvailabilityAndGetAsync(cardNumber, userId, currency, money, hash);
            if (!balanceResult.IsSuccess)
                return AvailabilityFactory.Create<CardBalanceModel>(null, balanceResult.Response);

            CardBalanceModel balance = balanceResult.Item;
            balance.Balance = balance.Balance - money;

            if (fee != decimal.Zero)
            {
                await Commissions.AddAsync(new CommissionModel()
                {
                    Currency = BankingEnums.GetCurrencyString(currency),
                    AmountEncrypt = SecurityMeasures.AESEncrypt(commission.ToString()),
                    CommissionPercentage = fee.ToString(),

                    CardId = balance.CardId,
                });
            }

            return AvailabilityFactory.Create(balance, balanceResult.Response);
        }
        #endregion

        #region Helper Methods
        private readonly ObjectResult TransactionLimit = ResponseFactory.Create(StatusCodes.Status403Forbidden, SecurityMeasures.GenerateErrorObject("Transaction card limit hit!"));
        public async Task<ObjectResult> RecordTransactionAndProcessBalances(string transactionType, 
                                                decimal amountSent, 
                                                decimal amountReceived, 
                                                BankingEnums.Currency senderCurrency, 
                                                BankingEnums.Currency receiverCurrency, 
                                                string description, 
                                                CardBalanceModel senderBalance,
                                                CardBalanceModel receiverBalance)
        {
            var currentTime = DateUtility.GetCurrentTime();

            senderBalance.LastTransactionDate = currentTime;
            senderBalance.TransactedToday += amountSent;
            if (senderBalance.IsLimited() && senderBalance.TransactionLimit < senderBalance.TransactedToday)
                return TransactionLimit;

            receiverBalance.LastTransactionDate = currentTime;

            await Transactions.AddAsync(new TransactionModel()
            {
                TransactionTypeEncrypt = SecurityMeasures.AESEncrypt(transactionType),

                AmountSentEncrypt = SecurityMeasures.AESEncrypt(amountSent.ToString()),
                AmountReceivedEncrypt = SecurityMeasures.AESEncrypt(amountReceived.ToString()),

                SenderCurrency = BankingEnums.GetCurrencyString(senderCurrency),
                ReceiverCurrency = BankingEnums.GetCurrencyString(receiverCurrency),

                TransactionDate = DateUtility.GetCurrentTime(),
                DescriptionEncrypt = SecurityMeasures.AESEncrypt(description),

                SenderCardId = senderBalance.CardId,
                SenderBalanceId = senderBalance.Id,

                ReceiverCardId = receiverBalance.CardId,
                ReceiverBalanceId = receiverBalance.Id,
            });

            return ResponseFactory.Success;
        }
        #endregion
        #endregion
        #region Card Numbers Generation
        private const string IIN_START = "25";
        private const int IIN_END_MIN = 3510;
        private const int IIN_END_MAX = 3941;
        private const byte CARDNUMBER_USERID_LENGTH = 9;
        // Calculates Luhn Check Digit which is then placed at the end of the card number
        private int CalculateLuhnCheckDigit(string cardNumber)
        {
            int sum = 0;
            int digit;
            bool doubleDigit = true;

            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                digit = int.Parse(cardNumber[i].ToString());

                if (doubleDigit)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit -= 9;
                }

                sum += digit;
                doubleDigit = !doubleDigit;
            }

            return (10 - (sum % 10)) % 10;
        }
        // This format fills the string with CARDNUMBER_USERID_LENGTH amount of zeros if the user ID is too small
        private string GetUserIdFormat()
        {
            return "D" + CARDNUMBER_USERID_LENGTH;
        }
        // Main Card Number generation func, returns card number and also ensures that the card number is unique
        public async Task<string> GenerateCardNumberAsync(int userId)
        {
            if (await Users.FirstOrDefaultAsync(user => user.Id.Equals(userId)) is null)
                return string.Empty;

            StringBuilder stringBuilder = new StringBuilder();
            string cardNumber;
            int luhnCheckDigit;
            do
            {
                stringBuilder.Clear();
                cardNumber = stringBuilder.Append(IIN_START) // BANK ID START
                                          .Append(RandomNumberUtility.Generate(IIN_END_MIN, IIN_END_MAX)) // Randomization, BANK ID END
                                          .Append(userId.ToString(GetUserIdFormat())) // User ID in the card
                                          .ToString();

                luhnCheckDigit = CalculateLuhnCheckDigit(cardNumber);
                cardNumber = cardNumber + luhnCheckDigit;
            } while (await CardExistsAsync(cardNumber));

            return cardNumber;
        }
        // Self explanatory
        public string GenerateCardVerificationValue()
        {
            return RandomNumberUtility.Generate(0, 1000).ToString("D3");
        }
        // Self explanatory
        public string GeneratePersonalIdentificationNumber()
        {
            return RandomNumberUtility.Generate(0, 10000).ToString("D4");
        }
        #endregion
    }
}
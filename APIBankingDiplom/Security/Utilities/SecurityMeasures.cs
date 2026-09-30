using APIBankingDiplom.CommonResources.Utilities;
using APIBankingDiplom.DBClasses.DBContext;
using APIBankingDiplom.DBClasses.DBModels;
using APIBankingDiplom.GeneralUtilities.Factories;
using APIBankingDiplom.Security.Models;
using APIBankingDiplom.TokenWorkings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BC = BCrypt.Net.BCrypt;

namespace APIBankingDiplom.Security.Utilities
{
    public static class SecurityMeasures
    {
        #region General User Data methods
        private const int _maxNameLength = 64;
        public static bool ProcessRegistrationData(ref string name, string email)
        {
            name = name.Substring(0, Math.Min(name.Length, _maxNameLength));

            if (!IsValidEmail(email))
                return false;

            return true;
        }

        public static bool IsValidEmail(string email)
        {
            // It throws exception on initialization if email address is invalid.
            // This is literally the best way to validate email addresses
            // This checks more edge cases than [EmailAddress] attribute
            // Slow, but the most secure.
            // See https://github.com/dotnet/runtime/blob/main/src/libraries/System.Net.Mail/src/System/Net/Mail/MailAddress.cs
            try
            {
                new MailAddress(email);
                return true;
            }
            catch
            {
                // We already know what went wrong... No reason to log or do anything
                return false;
            }
        }
        #endregion
        #region Hashing and Encrypting data
        // Slow hashing method useful to slow down brute-force attacks, amazing for passwords
        public const byte BCryptLength = 72;
        public static string HashStringBCrypt(string stringToHash) // Uses Salt Range from 10 to 13
        {
            int workFactor = GenerateBCryptSaltNumber();
            return BC.HashPassword(stringToHash, workFactor);
        }

        public static bool VerifyBCrypt(string stringToVerify, string hashedString)
        {
            return BC.Verify(stringToVerify, hashedString);
        }

        private static int GenerateBCryptSaltNumber()
        {
            return RandomNumberUtility.Generate(11, 15);
        }

        // Fast, resistant to many attacks and is 96 characters long.
        // Is used here for general things that need to be hashed
        public const byte SHA384Length = 96;
        private static readonly byte[] SHA384SecretKey = StringToBytes(Environment.GetEnvironmentVariable("API_SHA384_SECRETKEY")
                                                              ?? throw new InvalidOperationException("API_SHA384_SECRETKEY is missing!"));
        public static string HashStringSHA384(string? stringToHash)
        {
            /* // No native support for some reason?... On .NET 8? //
            using (HMACSHA3_384 SHA384WithSecretKey = new HMACSHA3_384(HashingSecretKey))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(stringToHash);
                bytes = SHA384WithSecretKey.ComputeHash(bytes);

                return BytesToString(bytes);
            }*/

            if (stringToHash.IsNullOrEmpty())
                return string.Empty;

            HMac hmac = new HMac(new Sha3Digest(384)); // HMAC SHA3-384
            hmac.Init(new KeyParameter(SHA384SecretKey));

            byte[] result = new byte[hmac.GetMacSize()];
            byte[] hashStringBytes = Encoding.UTF8.GetBytes(stringToHash!);

            hmac.BlockUpdate(hashStringBytes, 0, hashStringBytes.Length);
            hmac.DoFinal(result, 0);

            return BytesToString(result);
        }

        public const byte AESPotentialSizeIncrease = 64; // IV + Maximum Padding + IV prepend, and in Base64 ((16 + 16 + 16) * 1.33 = 63.84 characters, rounded up to 64)
        private static readonly byte[] AESSecretKey = StringToBytes(Environment.GetEnvironmentVariable("API_AES_SECRETKEY")
                                                            ?? throw new InvalidOperationException("API_AES_SECRETKEY is missing!"));
        public static string AESEncrypt(string? stringToEncrypt)
        {
            if (stringToEncrypt.IsNullOrEmpty())
                return string.Empty;

            string encryptedString;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = AESSecretKey;
                aesAlg.GenerateIV();

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // This is ugly, but using statements only clear up their own variables...
                using (MemoryStream msEncrypt = new MemoryStream())
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    // Write Initialization Vector directly into memory stream so it's not encrypted
                    msEncrypt.Write(aesAlg.IV);

                    swEncrypt.Write(stringToEncrypt);

                    // Making sure its passed down to cryptostream, and then to memorystream
                    swEncrypt.Flush();
                    csEncrypt.FlushFinalBlock();

                    encryptedString = BytesToString(msEncrypt.ToArray());
                }
            }

            return encryptedString;
        }

        public static string AESDecrypt(string stringToDecrypt)
        {
            if (stringToDecrypt.IsNullOrEmpty()) return string.Empty;

            string decryptedString;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = AESSecretKey;

                // Finding our Initialization Vector
                // AES IVs are 16 bytes long, we prepended them in encryption
                // First 16 bytes are IV bytes
                byte[] combinedData = StringToBytes(stringToDecrypt);
                byte[] IV = new byte[16];
                byte[] encryptedBytes = new byte[combinedData.Length - IV.Length];

                Buffer.BlockCopy(combinedData, 0, IV, 0, IV.Length);
                Buffer.BlockCopy(combinedData, IV.Length, encryptedBytes, 0, encryptedBytes.Length);

                aesAlg.IV = IV; // Could've just put aesAlg.IV into Buffer.BlockCopy, but will make it less readable

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // This is ugly, but using statements only clear up their own variables...
                using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {
                    decryptedString = srDecrypt.ReadToEnd();
                }
            }

            return decryptedString;
        }

        private static string BytesToString(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        private static byte[] StringToBytes(string encodedString)
        {
            return Convert.FromBase64String(encodedString);
        }
        #endregion
        #region Validation of User Identity
        private static readonly object Validation_NotAuthenticated = GenerateErrorObject("User is not authenticated");
        private static readonly object Validation_InvalidIDFormat = GenerateErrorObject("Invalid ID Format");
        private static readonly object Validation_UserIsNull = GenerateErrorObject("User is NULL");
        public static async Task<UserValidationResult> ValidateIdentityAsync(BankingContext _bankingContext, ClaimsPrincipal userToValidate)
        {
            if (userToValidate.Identity is null || !userToValidate.Identity.IsAuthenticated)
                return UserValidationResult.Failure(Validation_NotAuthenticated);

            int userId;
            if (!int.TryParse(userToValidate.FindFirstValue("id"), out userId))
                return UserValidationResult.Failure(Validation_InvalidIDFormat);

            UserModel? user = await _bankingContext.Users.FirstOrDefaultAsync(v => v.Id == userId);
            if (user is null)
                return UserValidationResult.Failure(Validation_UserIsNull);

            return UserValidationResult.Success(user);
        }
        #endregion
        #region Generate Error Object
        public static readonly object InternalServerError = GenerateErrorObject("Internal server error has occured.");
        public static readonly object UserIsNull = GenerateErrorObject("User does not exist or was not found.");
        public static readonly object DatabaseConnectionIssue = GenerateErrorObject("Could not connect to database.");
        public static readonly object CardNotOwnedOrNull = GenerateErrorObject("Requested card does not exist or is not owned by User.");
        public static readonly object CardIsNull = GenerateErrorObject("Card does not exist.");
        public static readonly object CardLimitHit = GenerateErrorObject("Operation exceeds card daily limit.");
        public static readonly object CardIsBlocked = GenerateErrorObject("Card is blocked.");
        public static readonly object CardIsExpired = GenerateErrorObject("Card has expired.");
        public static readonly object CardBalanceDoesNotExist = GenerateErrorObject("Card balance does not exist.");
        public static readonly object CardBalanceMoneyMustBePositive = GenerateErrorObject("Amout of money cannot be negative.");
        public static readonly object CardBalanceInsufficientFunds = GenerateErrorObject("Insuffient funds for this operation on chosen card balance.");
        public static readonly object CardBalanceIsBlocked = GenerateErrorObject("Card Balance is blocked.");
        public static readonly object PageNumberMustBePositive = GenerateErrorObject("Page number must be bigger than 0");
        public static object GenerateErrorObject(string errorMessage)
        {
            return new { errorMessage };
        }
        #endregion
        #region General Exception Processing
        public static void LogException(Exception e)
        {
            Console.WriteLine("Exception Message:\n" + e.Message + "\n");
            Console.WriteLine("Inner Exception Message:\n" + e.InnerException + "\n");
            Console.WriteLine("Stack Trace:\n" + e.StackTrace + "\n");
        }

        public static object GeneralExceptionReply(Exception e, UserModel? user)
        {
            return user is null ? UserIsNull : DatabaseConnectionIssue;
        }

        public static async Task<IActionResult> TryExecutingAsync(Func<Task<IActionResult>> action, Func<Exception, object?, Task>? onException, UserModel? user, object? errorObject = null)
        {
            try
            {
                return await action();
            }
            catch (Exception e)
            {
                if (onException is not null)
                    await onException(e, errorObject);

                LogException(e);
                return ResponseFactory.Create(StatusCodes.Status500InternalServerError, errorObject is null ? GeneralExceptionReply(e, user) : errorObject);
            }
        }

        public static async Task<bool> TryExecutingAsync(Func<Task<bool>> action, Func<Exception, Task>? onException = null)
        {
            try
            {
                return await action();
            }
            catch (Exception e)
            {
                if (onException is not null)
                    await onException(e);

                LogException(e);
                return false;
            }
        }
        public static async Task TryExecutingAsync(Func<Task> action, Func<Exception, Task>? onException = null)
        {
            try
            {
                await action();
            }
            catch (Exception e)
            {
                if (onException is not null)
                    await onException(e);

                LogException(e);
            }
        }
        #endregion
        #region JWT Token workings
        private static JwtSecurityToken GenerateJWTToken(RsaSecurityKey secretKey, TimeSpan expiration, IEnumerable<Claim>? identity = null)
        {
            DateTime now = DateUtility.GetCurrentTime();
            return new JwtSecurityToken(
                    issuer: TokenOptions.ISSUER,
                    audience: TokenOptions.AUDIENCE,
                    notBefore: now,
                    claims: identity,
                    expires: now.Add(expiration),
                    signingCredentials: new SigningCredentials(secretKey, SecurityAlgorithms.RsaSha384));
        }

        public static JwtSecurityToken GenerateAccessToken(IEnumerable<Claim> identity)
        {
            return GenerateJWTToken(TokenOptions.GetAccessTokenPrivateKey(), TokenOptions.GetAccessTokenLifetime(), identity);
        }

        public static JwtSecurityToken GenerateRefreshToken(IEnumerable<Claim> identity)
        {
            return GenerateJWTToken(TokenOptions.GetRefreshTokenPrivateKey(), TokenOptions.GetRefreshTokenLifetime(), identity);
        }

        public static (JwtSecurityToken, JwtSecurityToken) GenerateAccessAndRefreshToken(IEnumerable<Claim> identity)
        {
            return (GenerateAccessToken(identity), GenerateRefreshToken(identity));
        }

        public static DateTime GetJWTExpireTime(string jwtToken)
        {
            return new JwtSecurityTokenHandler().ReadToken(jwtToken).ValidTo;
        }

        private static ClaimsIdentity defaultClaimsIdentity = default(ClaimsIdentity)!;
        public static ClaimsIdentity GenerateIdentityClaims(UserModel user)
        {
            if (user is null)
                return defaultClaimsIdentity;

            List<Claim> claims = new List<Claim>
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim("name", user.NameEncrypt),
                };

            return new ClaimsIdentity(claims, "Token");
        }
        #endregion
        #region Sending Emails
        private static string APIEmail_Address = Environment.GetEnvironmentVariable("API_EMAIL_ADDRESS") ?? throw new InvalidOperationException("API_EMAIL_ADDRESS is missing!");
        private static string APIEmail_Password = Environment.GetEnvironmentVariable("API_EMAIL_PASSWORD") ?? throw new InvalidOperationException("API_EMAIL_PASSWORD is missing!");
        private static string APIEmail_SubjectPrefix = "Veridion Bank: ";
        // This is done due to email sending rate limit
        private static double APIEmail_LastExecutionTime = 0;
        private static short APIEmail_ExecutionDelayMilliseconds = 2000;
        private static int APIEmail_SendsInQueue = 0;
        public static async Task SendEmailAsync(string toEmailAddress, string subject, string body)
        {
            // Calculate the delay that we need, and if we actually need it
            // Because if we'll just spam, we'll end up being rate limited
            double currentTimeWithDelay = TimeInMilliseconds() - APIEmail_ExecutionDelayMilliseconds;
            if (APIEmail_LastExecutionTime > currentTimeWithDelay)
            {
                // Calculate the delay in milliseconds and add execution delay time multiplied by amount of messages in queue, if needed
                APIEmail_SendsInQueue++;
                int delay = Math.Max(0, (int)(APIEmail_LastExecutionTime - currentTimeWithDelay)) + APIEmail_ExecutionDelayMilliseconds * (APIEmail_SendsInQueue - 1);
                Console.WriteLine(delay);
                await Task.Delay(delay);
            }

            APIEmail_LastExecutionTime = TimeInMilliseconds();
            if (APIEmail_SendsInQueue > 0)
                APIEmail_SendsInQueue--;

            if (Convert.TryFromBase64String(toEmailAddress, null, out _))
                toEmailAddress = AESDecrypt(toEmailAddress);

            SmtpClient client = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(APIEmail_Address, APIEmail_Password),
                EnableSsl = true
            };

            MailMessage mailMessage = new MailMessage()
            {
                From = new MailAddress(APIEmail_Address),
                Subject = APIEmail_SubjectPrefix + subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmailAddress);

            await client.SendMailAsync(mailMessage);
        }

        private static double TimeInMilliseconds()
        {
            return (DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds;
        }
        #endregion
    }
}

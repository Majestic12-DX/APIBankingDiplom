using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using APIBankingDiplom.DBClasses.DBModels;
using APIBankingDiplom.DBClasses.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using APIBankingDiplom.TokenWorkings;
using APIBankingDiplom.Security.Utilities;
using APIBankingDiplom.CommonResources.Utilities;
using APIBankingDiplom.GeneralUtilities.Factories;
using APIBankingDiplom.Controllers.DTO;
using APIBankingDiplom.Controllers.BaseControllers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using APIBankingDiplom.DBClasses.DBModels.Tokens;

namespace APIBankingDiplom.Controllers
{
    [ApiController]
    public class AuthController : BaseController
    {
        public AuthController(BankingContext context, ILogger<AuthController> logger, IConfiguration configuration) : base(context, logger, configuration) { }
        #region Registration and Email Verification
        private readonly ObjectResult DatabaseConnectionIssue = ResponseFactory.Create(StatusCodes.Status500InternalServerError, SecurityMeasures.DatabaseConnectionIssue);
        private readonly ObjectResult InvalidRegistrationData = ResponseFactory.Create(StatusCodes.Status400BadRequest, SecurityMeasures.GenerateErrorObject("All parts of the registration must be clarified correctly."));
        private readonly ObjectResult UniqueInformationTaken = ResponseFactory.Create(StatusCodes.Status400BadRequest, SecurityMeasures.GenerateErrorObject("This email or phone number is already taken."));
        [HttpPost("Register")]
        [AllowAnonymous]
        // ProcessRegistrationData handles email string, rather than [EmailAddress] attribute
        // Because [EmailAddress] does not cover all cases of invalid email addresses, like " @gmail.com"
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDTO registerModel)
        {
            string name = registerModel.name;
            if (!SecurityMeasures.ProcessRegistrationData(ref name, registerModel.email))
                return InvalidRegistrationData;

            string emailHash = SecurityMeasures.HashStringSHA384(registerModel.email);
            string phoneHash = SecurityMeasures.HashStringSHA384(registerModel.phoneNumber);
            if (await _bankingContext.Users.FirstOrDefaultAsync(u => u.EmailHash.Equals(emailHash) || u.PhoneNumberHash.Equals(phoneHash)) is not null)
                return UniqueInformationTaken;

            // If you have questions about encrypting and hashing same data check DBClasses/DBModels/UserModel.cs
            UserModel newUser = new UserModel()
            {
                NameEncrypt = SecurityMeasures.AESEncrypt(name),
                PasswordHash = SecurityMeasures.HashStringBCrypt(registerModel.password),
                EmailEncrypt = SecurityMeasures.AESEncrypt(registerModel.email),
                EmailHash = emailHash,
                PhoneNumberEncrypt = SecurityMeasures.AESEncrypt(registerModel.phoneNumber),
                PhoneNumberHash = phoneHash
            };

            return await SecurityMeasures.TryExecutingAsync(async () =>
                {
                    await _bankingContext.Users.AddAsync(newUser);
                    string emailVerificationToken = await _bankingContext.CreateEmailVerificationTokenAsync(newUser);

                    await _bankingContext.SaveChangesAsync();

                    string link = Url.Action("VerifyEmail", "Auth", new { token = emailVerificationToken }, Request.Scheme)!;
                    string emailBody = $@"
                                            <h2>Confirm Your Email Address, {name}!</h2>
                                            <p>Thank you for choosing Veridion. To complete your registration, please confirm your email address by clicking the link below:</p>
                                            <p><a href='{link}'>Confirm Your Email</a></p>
                                            <br />
                                            <p>Please note that this link is valid for {TokenOptions.GetEmailTokenLifetime().Minutes} minutes.</p>
                                            <br />
                                            <p>If you did not initiate this request, please disregard this email.</p>
                                            <br />
                                            <p>Best regards,<br />The Veridion Team</p>";

                    await SecurityMeasures.SendEmailAsync(registerModel.email, "Email Confirmation", emailBody);

                    return Ok();
                }, null, null, DatabaseConnectionIssue);
        }
        #endregion
        #region Email Related API Endpoints
        [HttpPost("VerifyEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmailAsync(string token)
        {
            var now = DateUtility.GetCurrentTime();
            token = SecurityMeasures.HashStringSHA384(token);

            EmailTokenModel? emailToken = await _bankingContext.EmailTokens.Include(et => et.User).FirstOrDefaultAsync(et => et.TokenHash.Equals(token) && et.ExpireDate >= now);
            if (emailToken is null)
                return BadRequest(SecurityMeasures.GenerateErrorObject("Email verification is expired or invalid, or email token has already been validated"));

            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                if (emailToken.User is null)
                    return NotFound(SecurityMeasures.UserIsNull);

                emailToken.User.IsEmailVerified = true;

                _bankingContext.EmailTokens.Remove(emailToken);
                await _bankingContext.SaveChangesAsync();
                return Ok();
            }, null, null, DatabaseConnectionIssue);
        }

        [HttpPost("VerifyEmailChange")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmailChangeAsync(string token)
        {
            var now = DateUtility.GetCurrentTime();
            token = SecurityMeasures.HashStringSHA384(token);

            EmailChangeTokenModel? emailChangeToken = await _bankingContext.EmailChangeTokens.Include(et => et.User).FirstOrDefaultAsync(et => et.TokenHash.Equals(token) && et.ExpireDate >= now);
            if (emailChangeToken is null)
                return BadRequest(SecurityMeasures.GenerateErrorObject("Email change verification is expired or invalid, or email change token has already been validated"));

            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                if (emailChangeToken.User is null)
                    return NotFound(SecurityMeasures.UserIsNull);

                if (await _bankingContext.Users.AnyAsync(u => u.EmailHash.Equals(emailChangeToken.EmailHash)))
                    return UniqueInformationTaken;

                emailChangeToken.User.EmailEncrypt = emailChangeToken.EmailEncrypt;
                emailChangeToken.User.EmailHash = emailChangeToken.EmailHash;

                _bankingContext.EmailChangeTokens.Remove(emailChangeToken);

                await _bankingContext.SaveChangesAsync();
                return Ok();
            }, null, null, DatabaseConnectionIssue);
        }

        #region Reset Password
        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordRequestAsync(string email)
        {
            email = SecurityMeasures.HashStringSHA384(email);

            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                UserModel? user = await _bankingContext.Users.FirstOrDefaultAsync(u => u.EmailHash.Equals(email));
                if (user is null)
                    return NotFound(SecurityMeasures.UserIsNull);

                string passwordResetToken = await _bankingContext.CreatePasswordResetTokenAsync(user);
                await _bankingContext.SaveChangesAsync();

                string link = Url.Action("VerifyResetPassword", "Auth", new { token = passwordResetToken }, Request.Scheme)!;
                string emailBody = $@"
                    <h2>Reset Your Password, {SecurityMeasures.AESDecrypt(user.NameEncrypt)}!</h2>
                    <p>We received a request to reset the password for your account. To complete the process, please reset your password by clicking the link below:</p>
                    <p><a href='{link}'>Reset Password</a></p>
                    <br />
                    <p>This link is only valid for {TokenOptions.GetPasswordResetTokenLifetime().Minutes} minutes.</p>
                    <br />
                    <p>If you did not request this change, you can ignore this email, and your password will remain unchanged.</p>
                    <br />
                    <p>Best regards,<br />The Veridion Team</p>";

                await SecurityMeasures.SendEmailAsync(SecurityMeasures.AESDecrypt(user.EmailEncrypt), "Password Reset", emailBody);
                return Ok();
            }, null, null, DatabaseConnectionIssue);
        }

        [HttpPost("VerifyResetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyResetPasswordAsync(string token, string password)
        {
            var now = DateUtility.GetCurrentTime();
            token = SecurityMeasures.HashStringSHA384(token);

            PasswordResetTokenModel? passwordResetToken = await _bankingContext.PasswordResetTokens.Include(et => et.User).FirstOrDefaultAsync(et => et.TokenHash.Equals(token) && et.ExpireDate >= now);
            if (passwordResetToken is null)
                return BadRequest(SecurityMeasures.GenerateErrorObject("Password Reset verification is expired or invalid, or password reset token has already been validated"));

            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                if (passwordResetToken.User is null)
                    return NotFound(SecurityMeasures.UserIsNull);

                passwordResetToken.User.PasswordHash = SecurityMeasures.HashStringBCrypt(password);

                await _bankingContext.InvalidateUserTokensAsync(passwordResetToken.User);

                _bankingContext.PasswordResetTokens.Remove(passwordResetToken);

                await _bankingContext.SaveChangesAsync();
                return Ok();
            }, null, null, DatabaseConnectionIssue);
        }
        #endregion
        #endregion
        #region Login
        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDTO loginModel)
        {
            ClaimsIdentity? identity = null;
            UserModel? user = null;

            loginModel.email = SecurityMeasures.HashStringSHA384(loginModel.email);
            await SecurityMeasures.TryExecutingAsync(async () =>
            {
                user = await _bankingContext.Users.FirstOrDefaultAsync(u => u.EmailHash.Equals(loginModel.email));

                if (user is null)
                    return;

                if (SecurityMeasures.VerifyBCrypt(loginModel.password, user.PasswordHash))
                    identity = SecurityMeasures.GenerateIdentityClaims(user);
            });

            if (user is null || identity is null)
                return BadRequest(SecurityMeasures.GenerateErrorObject("Invalid email or password, or troubles with database connectivity."));

            if (!user.IsEmailVerified)
                return ResponseFactory.Create(StatusCodes.Status403Forbidden, SecurityMeasures.GenerateErrorObject("Email must be verified in order to log in"));

            JwtSecurityToken accessToken, refreshToken;
            (accessToken, refreshToken) = SecurityMeasures.GenerateAccessAndRefreshToken(identity.Claims);

            user.LastLoginTime = DateUtility.GetCurrentTime();
            bool tokensAddedToDB = await SecurityMeasures.TryExecutingAsync(async () =>
            {
                await _bankingContext.InvalidateUserTokensAsync(user);

                await _bankingContext.AddUserRefreshTokenAsync(user, refreshToken);
                await _bankingContext.AddUserAccessTokenAsync(user, accessToken);

                await _bankingContext.SaveChangesAsync();
                return true;
            });

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            string refreshTokenEncodedString = handler.WriteToken(refreshToken);
            string accessTokenEncodedString = handler.WriteToken(accessToken);

            return tokensAddedToDB ? Ok(new { user.Id, accessToken = accessTokenEncodedString, refreshToken = refreshTokenEncodedString }) 
                                              : StatusCode(StatusCodes.Status500InternalServerError, SecurityMeasures.GenerateErrorObject("Could not save tokens to database!"));
        }
        #endregion
        #region Refresh Access Key
        [HttpPost("RefreshAccessKey")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshAsync(string refreshToken)
        {
            // Check if Refresh Token exists in database (therefore is valid)
            // And perform access token replace operation
            JwtSecurityToken? accessToken = null;
            bool isRefreshOperationSuccessful = await SecurityMeasures.TryExecutingAsync(async () =>
            {
                string hashedRefreshToken = SecurityMeasures.HashStringSHA384(refreshToken);

                RefreshTokenModel? databaseRefreshToken = await _bankingContext.RefreshTokens.Include(t => t.User).FirstOrDefaultAsync(t => t.TokenHash.Equals(hashedRefreshToken));
                if (databaseRefreshToken is null || databaseRefreshToken.User is null)
                    return false;

                UserModel user = databaseRefreshToken.User;

                AccessTokenModel[] databaseAccessTokens = await _bankingContext.AccessTokens.Where(t => t.UserId == user.Id).ToArrayAsync();
                foreach (var databaseAccessToken in databaseAccessTokens)
                    databaseAccessToken.IsInvalid = true;

                accessToken = SecurityMeasures.GenerateAccessToken(SecurityMeasures.GenerateIdentityClaims(user).Claims);
                await _bankingContext.AddUserAccessTokenAsync(user, accessToken);

                await _bankingContext.SaveChangesAsync();
                return true;
            });

            if (!isRefreshOperationSuccessful) 
                return BadRequest(SecurityMeasures.GenerateErrorObject("Refresh Token is invalid."));

            return Ok(new { accessToken = new JwtSecurityTokenHandler().WriteToken(accessToken) });
        }
        #endregion
        #region Logout (Invalidate access and refresh token)
        [HttpPost("Logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            UserModel user = GetAuthenticatedUser();
            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                await _bankingContext.InvalidateUserTokensAsync(user);
                await _bankingContext.SaveChangesAsync();

                return Ok();
            }, null, user);
        }
        #endregion
        #region Get User Data
        private readonly ObjectResult AvatarIsNull = ResponseFactory.Create(StatusCodes.Status404NotFound, SecurityMeasures.GenerateErrorObject("User is NULL or has no avatar"));
        [HttpGet("GetUserProfilePicture")]
        public async Task<IActionResult> GetAvatarAsync(int id)
        {
            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                UserModel? searchedUser = await _bankingContext.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (searchedUser is null || searchedUser.ProfilePicture is null)
                    return AvatarIsNull;

                return File(searchedUser.ProfilePicture, Image.DetectFormat(searchedUser.ProfilePicture).DefaultMimeType);
            }, null, GetAuthenticatedUser());
        }

        [HttpGet("GetUserInfo")]
        public async Task<IActionResult> GetUserInfoAsync(int id)
        {
            var user = GetAuthenticatedUser();
            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                UserModel? searchedUser = await _bankingContext.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (searchedUser is null)
                    return NotFound(SecurityMeasures.UserIsNull);

                UserInfoDTO userInfo = new UserInfoDTO()
                {
                    Id = searchedUser.Id,
                    Name = SecurityMeasures.AESDecrypt(searchedUser.NameEncrypt),
                    LastLoginTime = searchedUser.LastLoginTime
                };

                if (searchedUser.Equals(user))
                {
                    userInfo.Email = SecurityMeasures.AESDecrypt(user.EmailEncrypt);
                    userInfo.PhoneNumber = SecurityMeasures.AESDecrypt(user.PhoneNumberEncrypt);
                }

                return Ok(userInfo);
            }, null, user);
        }
        #endregion
        #region Update User Profile API Calls
        private const int MegabyteInBytes = 1048576;
        private const int MaximumImageSizeInBytes = MegabyteInBytes * 6;
        private const byte ImageSizeInPixels = 128;
        private readonly ObjectResult InvalidImageSize = ResponseFactory.Create(StatusCodes.Status413RequestEntityTooLarge, SecurityMeasures.GenerateErrorObject($"File must not be larger than {MaximumImageSizeInBytes / MegabyteInBytes} megabytes and have valid size."));
        private readonly ObjectResult InvalidFileExtension = ResponseFactory.Create(StatusCodes.Status415UnsupportedMediaType, SecurityMeasures.GenerateErrorObject("Invalid file extension. Try png, jpg, webp or gif."));

        private readonly Dictionary<string, Func<Image, Stream, Task>> SupportedFileExtensions = new Dictionary<string, Func<Image, Stream, Task>>()
        {
            { ".png", async (image, stream) => await image.SaveAsPngAsync(stream) },
            { ".jpg", async (image, stream) => await image.SaveAsJpegAsync(stream) },
            { ".jpeg", async (image, stream) => await image.SaveAsJpegAsync(stream) },
            { ".webp", async (image, stream) => await image.SaveAsWebpAsync(stream) },
            { ".gif", async (image, stream) => await image.SaveAsGifAsync(stream) }
        };
        [HttpPatch("SetProfilePicture")]
        public async Task<IActionResult> UploadAvatarAsync(IFormFile avatar)
        {
            var user = GetAuthenticatedUser();

            if (avatar.Length == 0 || avatar.Length > MaximumImageSizeInBytes)
                return InvalidImageSize;

            string fileExtension = Path.GetExtension(avatar.FileName.ToLower());

            if (!SupportedFileExtensions.ContainsKey(fileExtension))
                return InvalidFileExtension;

            using (var stream = avatar.OpenReadStream())
            {
                using (Image image = await Image.LoadAsync(stream))
                {
                    image.Mutate(i => i.Resize(ImageSizeInPixels, ImageSizeInPixels));
                    
                    using (var memoryStream = new MemoryStream())
                    {
                        await SupportedFileExtensions[fileExtension](image, memoryStream);

                        user.ProfilePicture = memoryStream.ToArray();
                    }
                }
            }

            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                await _bankingContext.SaveChangesAsync();
                return Ok();
            }, null, user);
        }

        private readonly ObjectResult InvalidPassword = ResponseFactory.Create(StatusCodes.Status400BadRequest, SecurityMeasures.GenerateErrorObject("Invalid password."));
        private readonly ObjectResult InvalidPhoneNumber = ResponseFactory.Create(StatusCodes.Status400BadRequest, SecurityMeasures.GenerateErrorObject("Invalid phone number."));
        [HttpPatch("UpdateUserData")]
        public async Task<IActionResult> UpdateUserDataAsync([FromBody] UserInfoUpdateDTO userInfoUpdate)
        {
            var user = GetAuthenticatedUser();
            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                if (!SecurityMeasures.VerifyBCrypt(userInfoUpdate.Password, user.PasswordHash))
                    return InvalidPassword;

                if (userInfoUpdate.NewPassword is not null)
                {
                    user.PasswordHash = SecurityMeasures.HashStringBCrypt(userInfoUpdate.NewPassword);

                    user.UpdatedAt = DateUtility.GetCurrentTime();
                }

                if (userInfoUpdate.Name is not null)
                {
                    user.NameEncrypt = SecurityMeasures.AESEncrypt(userInfoUpdate.Name);

                    user.UpdatedAt = DateUtility.GetCurrentTime();
                }

                if (userInfoUpdate.PhoneNumber is not null)
                {
                    string phoneNumberHash = SecurityMeasures.HashStringSHA384(userInfoUpdate.PhoneNumber);

                    // Check if someone already has this phone number
                    UserModel? queryCheckResult = await _bankingContext.Users.FirstOrDefaultAsync(u => u.PhoneNumberHash.Equals(phoneNumberHash) && u.Id != user.Id);
                    if (queryCheckResult is not null)
                        return InvalidPhoneNumber;

                    user.PhoneNumberHash = phoneNumberHash;
                    user.PhoneNumberEncrypt = SecurityMeasures.AESEncrypt(userInfoUpdate.PhoneNumber);

                    user.UpdatedAt = DateUtility.GetCurrentTime();
                }

                await _bankingContext.SaveChangesAsync();
                return Ok();
            }, null, user);
        }

        // This could be improved with MFA including phone number, but Twillio services are not for free
        private readonly ObjectResult InvalidEmail = ResponseFactory.Create(StatusCodes.Status400BadRequest, SecurityMeasures.GenerateErrorObject("Invalid email address."));
        [HttpPatch("UpdateUserEmail")]
        public async Task<IActionResult> UpdateUserEmailAsync([FromBody] UserEmailUpdateDTO userEmailUpdate)
        {
            var user = GetAuthenticatedUser();

            if (!SecurityMeasures.IsValidEmail(userEmailUpdate.Email))
                return InvalidEmail;

            return await SecurityMeasures.TryExecutingAsync(async () =>
            {
                if (!SecurityMeasures.VerifyBCrypt(userEmailUpdate.Password, user.PasswordHash))
                    return InvalidPassword;

                string emailHash = SecurityMeasures.HashStringSHA384(userEmailUpdate.Email);

                // Check if this Email might've been taken
                UserModel? queryCheckResult = await _bankingContext.Users.FirstOrDefaultAsync(u => u.EmailHash.Equals(emailHash));
                if (queryCheckResult is not null)
                    return InvalidEmail;

                string emailChangeToken = await _bankingContext.CreateEmailChangeTokenAsync(user, userEmailUpdate.Email);

                await _bankingContext.SaveChangesAsync();

                string link = Url.Action("VerifyEmailChange", "Auth", new { token = emailChangeToken }, Request.Scheme)!;
                string emailBody = $@"
                                        <h2>Confirm Your New Email Address, {SecurityMeasures.AESDecrypt(user.NameEncrypt)}!</h2>
                                        <p>We received a request to change the email address associated with your account. To complete the process, please confirm your new email address by clicking the link below:</p>
                                        <p><a href='{link}'>Confirm New Email Address</a></p>
                                        <br />
                                        <p>This link is only valid for {TokenOptions.GetEmailChangeTokenLifetime().Minutes} minutes.</p>
                                        <br />
                                        <p>If you did not request this change, you can ignore this email, and your current email address will remain unchanged.</p>
                                        <br />
                                        <p>Best regards,<br />The Veridion Team</p>";

                await SecurityMeasures.SendEmailAsync(userEmailUpdate.Email, "Email Change Confirmation", emailBody);

                return Ok();
            }, null, user);
        }
        #endregion
    }
}

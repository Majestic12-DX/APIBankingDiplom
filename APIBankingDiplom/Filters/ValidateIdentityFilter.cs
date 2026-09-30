using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using APIBankingDiplom.DBClasses.DBContext;
using Microsoft.EntityFrameworkCore;
using APIBankingDiplom.GeneralUtilities.Factories;
using APIBankingDiplom.Security.Models;
using APIBankingDiplom.Security.Utilities;
using APIBankingDiplom.DBClasses.DBModels.Tokens;

namespace APIBankingDiplom.Filters
{
    public class ValidateIdentityFilter : IAsyncActionFilter
    {
        private readonly BankingContext _bankingContext;
        private readonly string _validatedUserKey;
        private const string BearerAuthSuffix = "Bearer ";
        public ValidateIdentityFilter(BankingContext bankingContext, IConfiguration configuration)
        {
            _bankingContext = bankingContext;
            _validatedUserKey = configuration["ValidatedUserKey"]!;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
            {
                await next();
                return;
            }

            string? authHeader = context.HttpContext.Request.Headers["Authorization"];
            // JwtBearerHandler matches the scheme case-insensitively, so this must too or the request authenticates with no validated user
            if (authHeader?.StartsWith(BearerAuthSuffix, StringComparison.OrdinalIgnoreCase) is not true)
            {
                context.Result = new UnauthorizedObjectResult(SecurityMeasures.GenerateErrorObject("Invalid or missing Authorization header"));
                return;
            }

            // Checking that the JWT token sent is not invalidated in database
            authHeader = SecurityMeasures.HashStringSHA384(authHeader.Substring(BearerAuthSuffix.Length).Trim());

            UserValidationResult validationResult = UserValidationResult.Failure(SecurityMeasures.UserIsNull);
            bool TokenIsValid = await SecurityMeasures.TryExecutingAsync(async () =>
            {
                validationResult = await SecurityMeasures.ValidateIdentityAsync(_bankingContext, context.HttpContext.User);

                AccessTokenModel? token = await _bankingContext.AccessTokens.FirstOrDefaultAsync(t => t.TokenHash.Equals(authHeader) && t.IsInvalid);
                return token is null;
            },
            async (Exception e) => {
                // add logging later.. generally this whole thing needs a lot of logging

                context.Result = ResponseFactory.Create(StatusCodes.Status500InternalServerError, SecurityMeasures.InternalServerError);
            });

            if (!TokenIsValid)
            {
                context.Result = new UnauthorizedObjectResult(SecurityMeasures.GenerateErrorObject("This token has been invalidated"));
                return;
            }

            if (!validationResult.IsValid)
            {
                context.Result = new UnauthorizedObjectResult(validationResult.ErrorObject);
                return;
            }

            context.HttpContext.Items[_validatedUserKey] = validationResult.User;

            await next();
        }
    }
}

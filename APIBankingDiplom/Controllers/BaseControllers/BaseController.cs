using APIBankingDiplom.DBClasses.DBContext;
using APIBankingDiplom.DBClasses.DBModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIBankingDiplom.Controllers.BaseControllers
{
    [Route("[controller]")]
    [Authorize]
    public abstract class BaseController : ControllerBase
    {
        protected readonly BankingContext _bankingContext;
        protected readonly ILogger<BaseController> _logger;
        private readonly string _validatedUserKey;
        public BaseController(BankingContext context, ILogger<BaseController> logger, IConfiguration configuration)
        {
            _bankingContext = context;
            _logger = logger;
            _validatedUserKey = configuration["ValidatedUserKey"]!;
        }
        protected UserModel GetAuthenticatedUser()
        {
            return HttpContext.Items[_validatedUserKey] as UserModel
                ?? throw new InvalidOperationException("Authenticated user was not set by ValidateIdentityFilter");
        }
    }
}

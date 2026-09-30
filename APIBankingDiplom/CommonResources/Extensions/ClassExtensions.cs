using Microsoft.AspNetCore.Mvc;

namespace APIBankingDiplom.CommonResources.Extensions
{
    public static class ClassExtensions
    {
        #region Common Extensions
        public static bool IsNullOrDefault<T>(this T instance) where T : class
        {
            return instance is null || Equals(instance, default(T));
        }
        public static T GetDefault<T>(this T instance) where T : class
        {
            return default(T)!;
        }
        #endregion

        #region ObjectResult Extensions
        public static bool IsSuccess(this ObjectResult result)
        {
            return result.StatusCode is StatusCodes.Status200OK;
        }
        #endregion
    }
}

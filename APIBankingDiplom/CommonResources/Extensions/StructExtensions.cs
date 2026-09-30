namespace APIBankingDiplom.CommonResources.Extensions
{
    public static class StructExtensions
    {
        #region Common Extensions
        public static bool IsDefault<T>(this T value) where T : struct
        {
            return value.Equals(default(T));
        }
        #endregion
    }
}

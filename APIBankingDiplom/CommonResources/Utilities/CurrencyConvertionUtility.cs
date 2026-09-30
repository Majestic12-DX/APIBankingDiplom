using APIBankingDiplom.DBClasses.Enums;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Concurrent;
using System.Text.Json;

namespace APIBankingDiplom.CommonResources.Utilities
{
    public static class CurrencyConvertionUtility
    {
        private static readonly string ExchangeRateApiKey = Environment.GetEnvironmentVariable("API_EXCHANGERATE_KEY")
                                                                ?? throw new InvalidOperationException("API_EXCHANGERATE_KEY is missing!");

        private static readonly HttpClient _httpClient = new HttpClient()
        {
            BaseAddress = new Uri($"https://v6.exchangerate-api.com/v6/{ExchangeRateApiKey}/latest/")
        };

        private class CachedRate
        {
            public JsonElement Rates { get; set; }

            // UTC Time when conversion rate is updated (grabbed from exchange rate api response)
            public DateTime NextUpdateTimeUTC { get; set; }
        }

        private static readonly ConcurrentDictionary<string, CachedRate> _cachedConversionRates = new ConcurrentDictionary<string, CachedRate>();

        public static async Task<decimal?> GetConversionRateAsync(string currency, string conversionCurrency)
        {
            // See if we have the rate already cached and use if it's not the time past the update
            if (_cachedConversionRates.TryGetValue(currency, out CachedRate? cacheItem) && cacheItem.NextUpdateTimeUTC > DateUtility.GetCurrentTime())
                return ConvertConversionRateToDecimal(cacheItem.Rates, conversionCurrency);

            // Fetch fresh data if not cached or cache expired
            string responseBody = await _httpClient.GetStringAsync(currency);
            using (JsonDocument jsonDocument = JsonDocument.Parse(responseBody))
            {
                JsonElement rates = jsonDocument.RootElement.GetProperty("conversion_rates").Clone();

                // Get next Update Time so we know when our cache needs to be updated
                DateTime updateTimeUTC = DateTime.Parse(jsonDocument.RootElement.GetString("time_next_update_utc")!).ToUniversalTime();

                // Cache the rates and also the next update time
                CachedRate cachedCurrencyRate = new CachedRate { Rates = rates, NextUpdateTimeUTC = updateTimeUTC };

                _cachedConversionRates.AddOrUpdate(currency, cachedCurrencyRate, (key, oldValue) => cachedCurrencyRate);

                // Get and return the wanted conversion currency rate
                return ConvertConversionRateToDecimal(rates, conversionCurrency);
            }
        }

        public static async Task<decimal?> GetConversionRateAsync(BankingEnums.Currency currency, BankingEnums.Currency conversionCurrency)
        {
            string currencyString = BankingEnums.GetCurrencyString(currency);
            string conversionCurrencyString = BankingEnums.GetCurrencyString(conversionCurrency);

            return await GetConversionRateAsync(currencyString, conversionCurrencyString);
        }

        private static decimal? ConvertConversionRateToDecimal(JsonElement rates, string conversionCurrency)
        {
            return rates.TryGetProperty(conversionCurrency, out JsonElement conversionRate)
                ? conversionRate.GetDecimal()
                : null;
        }
    }
}

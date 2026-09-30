namespace APIBankingDiplom.DBClasses.Enums
{
    public static class BankingEnums
    {
        public enum CardType
        {
            Debit = 0,
            Credit = 1,
        }

        public enum ATMOperationType
        {
            Deposit = 0,
            Withdraw = 1,
        }

        public enum CardStatus
        {
            Active = 0,
            Blocked = 1,
            Expired = 2,
        }

        public enum CardBalanceStatus
        {
            Active = 0,
            Limited = 1,
            Blocked = 2,
        }

        public enum Currency
        {
            UAH = 0,
            USD = 1,
            PLN = 2,
        }

        // How hideous!
        // 2 years later: Enum.TryParse ?
        public static string GetCurrencyString(Currency currency)
        {
            switch(currency)
            {
                case Currency.UAH:
                    return "UAH";
                case Currency.USD:
                    return "USD";
                case Currency.PLN:
                    return "PLN";
                default:
                    throw new ArgumentOutOfRangeException(nameof(currency), $"Invalid currency enum {currency}");
            }
        }

        public static Currency GetStringCurrency(string currency)
        {
            switch (currency)
            {
                case "UAH":
                    return Currency.UAH;
                case "USD":
                    return Currency.USD;
                case "PLN":
                    return Currency.PLN;
                default:
                    throw new ArgumentOutOfRangeException(nameof(currency), $"Invalid currency string {currency}");

            }
        }
    }
}

namespace APIBankingDiplom.CommonResources.Utilities
{
    public static class DateUtility
    {
        // Every persisted timestamp is UTC so expiry comparisons survive a DST shift or a server in another timezone
        public static DateTime GetCurrentTime()
        {
            return DateTime.UtcNow;
        }

        public static TimeSpan GetTimeTillNextMidnight()
        {
            var now = GetCurrentTime();

            return now.Date.AddDays(1) - now;
        }
        
        public static string GetCurrentDateInShortString()
        {
            return GetCurrentTime().ToShortDateString();
        }

        public static DateTime DateFromShortDateString(string shortdate)
        {
            // Short date is Day . Month . Year format
            string[] date = shortdate.Split('.');

            if (date.Length != 3)
                return DateTime.MinValue;

            // It does not use try catch internally so it's not slow
            if (int.TryParse(date[0], out int day) && int.TryParse(date[1], out int month) && int.TryParse(date[2], out int year))
            {
                if (1 <= day && day <= 31 && 1 <= month && month <= 12 && 0 < year)
                    return new DateTime(year, month, day);

                return DateTime.MinValue;
            }
            
            return DateTime.MinValue;
        }
    }
}

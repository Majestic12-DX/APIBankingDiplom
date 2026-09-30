using System.Security.Cryptography;

namespace APIBankingDiplom.CommonResources.Utilities
{
    public static class RandomNumberUtility
    {
        private static readonly RandomNumberGenerator RNG = RandomNumberGenerator.Create();
        public static int Generate(int minValue, int maxValue)
        {
            if (minValue >= maxValue)
                throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue must be greater than minValue.");

            byte[] randomNumberBytes = new byte[2];
            RNG.GetBytes(randomNumberBytes);

            int randomNumber = BitConverter.ToInt16(randomNumberBytes, 0);
            return Math.Abs(randomNumber % (maxValue - minValue)) + minValue;
        }
    }
}

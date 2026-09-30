using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace APIBankingDiplom.TokenWorkings
{
    public static class TokenOptions
    {
        #region JWT Tokens General settings
        public static readonly string ISSUER = "Veridion Bank";
        public static readonly string AUDIENCE = "Veridion Users";
        #endregion

        #region Signing key files
        private const string ACCESS_PRIVATEKEY_PATH = "RSAKeys/AccessKeys/private.pem";
        private const string ACCESS_PUBLICKEY_PATH = "RSAKeys/AccessKeys/public.pem";
        private const string REFRESH_PRIVATEKEY_PATH = "RSAKeys/RefreshKeys/private.pem";
        private const string REFRESH_PUBLICKEY_PATH = "RSAKeys/RefreshKeys/public.pem";
        private const int KEY_SIZE = 4096;

        private static readonly bool KeysReady = EnsureKeyPairsExist();

        private static bool EnsureKeyPairsExist()
        {
            EnsureKeyPairExists(ACCESS_PRIVATEKEY_PATH, ACCESS_PUBLICKEY_PATH);
            EnsureKeyPairExists(REFRESH_PRIVATEKEY_PATH, REFRESH_PUBLICKEY_PATH);

            return true;
        }

        private static void EnsureKeyPairExists(string privateKeyPath, string publicKeyPath)
        {
            if (File.Exists(privateKeyPath) && File.Exists(publicKeyPath))
                return;

            // Generating outside Development would not be great, since the keys should be mounted as secrets in production
            if (!string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase))
                throw new FileNotFoundException($"Signing key \"{privateKeyPath}\" is missing", privateKeyPath);

            Directory.CreateDirectory(Path.GetDirectoryName(privateKeyPath)!);

            using RSA rsa = RSA.Create(KEY_SIZE);
            File.WriteAllText(privateKeyPath, rsa.ExportPkcs8PrivateKeyPem());
            File.WriteAllText(publicKeyPath, rsa.ExportSubjectPublicKeyInfoPem());
        }

        private static RsaSecurityKey LoadKey(string path)
        {
            RSA rsa = RSA.Create();
            rsa.ImportFromPem(File.ReadAllText(path));

            return new RsaSecurityKey(rsa);
        }
        #endregion

        #region JWT Access Tokens options
        private const int ACCESSTOKEN_LIFETIME = 5; // In minutes
        public static readonly TokenValidationParameters AccessTokenValidationParams = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidIssuer = ISSUER,

            ValidateAudience = true,
            ValidAudience = AUDIENCE,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = GetAccessTokenPublicKey(),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
        #endregion

        #region JWT Refresh Tokens options
        // No need to add Token Validation Params for Refresh Tokens, since they are validated by existing in database
        private const int REFRESHTOKEN_LIFETIME = 90; // In days
        #endregion

        #region Get Keys methods
        public static RsaSecurityKey GetAccessTokenPrivateKey() => LoadKey(ACCESS_PRIVATEKEY_PATH);
        public static RsaSecurityKey GetAccessTokenPublicKey() => LoadKey(ACCESS_PUBLICKEY_PATH);

        // Refresh Tokens don't need those as much as Access Tokens
        // But let's still use asymmetric security keys for less predictability
        public static RsaSecurityKey GetRefreshTokenPrivateKey() => LoadKey(REFRESH_PRIVATEKEY_PATH);
        public static RsaSecurityKey GetRefreshTokenPublicKey() => LoadKey(REFRESH_PUBLICKEY_PATH);
        #endregion

        #region Get Tokens Lifetime methods
        public static TimeSpan GetAccessTokenLifetime()
        {
            return TimeSpan.FromMinutes(ACCESSTOKEN_LIFETIME);
        }

        public static TimeSpan GetRefreshTokenLifetime() 
        {
            return TimeSpan.FromDays(REFRESHTOKEN_LIFETIME);
        }
        #endregion

        #region Non-JWT Tokens
        private const int EMAILTOKEN_LIFETIME = 5; // In Minutes
        public static TimeSpan GetEmailTokenLifetime()
        {
            return TimeSpan.FromMinutes(EMAILTOKEN_LIFETIME);
        }

        private const int EMAILCHANGETOKEN_LIFETIME = 5; // In Minutes
        public static TimeSpan GetEmailChangeTokenLifetime()
        {
            return TimeSpan.FromMinutes(EMAILCHANGETOKEN_LIFETIME);
        }

        private const int PASSWORDRESETTOKEN_LIFETIME = 5; // In Minutes
        public static TimeSpan GetPasswordResetTokenLifetime()
        {
            return TimeSpan.FromMinutes(PASSWORDRESETTOKEN_LIFETIME);
        }
        #endregion
    }
}

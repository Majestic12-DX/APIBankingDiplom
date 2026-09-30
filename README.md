# Veridion Banking API

An academy final project of mine written under time pressure (second half of 2024, ~1.5 months): multi-currency card accounts, card-to-card transfers with live exchange rates and commission, ATM operations simulation,
and a JWT auth stack with email-verified registration and database-backed token revocation

Published as an item for a resume

**.NET 8 · ASP.NET Core · Entity Framework Core · SQL Server · BCrypt · BouncyCastle**

## Includes

- **Multi-currency card balances** - a card holds one balance per currency (UAH/USD/PLN), each with
  its own status and optional daily limits
- **Card-to-card transfers** - live rates from exchangerate-api with an in-memory cache keyed on the
  API's own `time_next_update_utc`, 2% commission on cross-card transfers, commission money sent to a
  bank balance by a background service
- **ATM simulation** - deposit/withdraw requiring of card number, expire date, CVV and PIN
- **Card issuance** - Luhn-valid 16-digit numbers under a fixed IIN, generated with a uniqueness
  check, max 5 active cards per user
- **Encryption and hashing** - every personal field is AES encrypted, and encrypted fields that also need to be looked up have their HMAC SHA3-384 hashed counterparts alongside. Passwords are BCrypt-ed
- **RS384 JWT auth** - asymmetric signing, 5-minute access tokens, database-backed revocation, email
  verification required before first login, password re-entry for sensitive changes in user profile
- **Background services** - token cleanup, midnight card expiring and daily-limit reset, commission
  collection

## Security design

### Searchable sensitive fields are stored twice

Fields that must be both searchable and recoverable are stored in two representations: encrypted and hashed. Encryption for the value, hash for the lookup. For example, email and phone numbers are both encrypted and hashed

### Tokens

| | |
|---|---|
| Access token | RS384, 5 min |
| Refresh token | RS384, 90 days, validated by existence in the database |
| Storage | Stored as HMAC SHA3-384 hashes |
| Revocation | logout and password reset invalidate access tokens and delete refresh tokens |
| Passwords | BCrypt, work factor randomised 11–14 |

Access and refresh tokens are signed with separate RSA key pairs. In development the key pairs are generated on first run, outside development the app throws an exception if they are absent rather than generating, 
since silently generating in "actual" production would most likely be harmful

## Request flow

```
Request -> JwtBearer -> ValidateIdentityFilter -> Controller -> BankingContext -> SQL Server
               │                   │                     │               │
          signature +         DB revocation          DTO in,        queries + the
          lifetime            check, loads           ObjectResult   deposit/withdraw
                              user into Items        out            logic
```

`ValidateIdentityFilter` does the part JWT doesn't: checks the token hasn't been revoked in the database, loads the `UserModel`, and puts it in `HttpContext.Items`. Controllers read it via `GetAuthenticatedUser()`

## Endpoints

30 endpoints across five controllers

| Controller | Covers |
|---|---|
| `Auth` | register, verify email, email change, verify email change, password reset, verify password reset, login, refreshing access key, logout, getting/setting profile picture, getting/updating user data |
| `Card` | issue, list, block/unblock |
| `CardBalance` | add/remove balance, status, deposit/withdrawal/transaction limits |
| `Transaction` | transfer money, paged history |
| `ATM` | deposit, withdraw, paged operation history |

## Running

Requires the .NET 8 SDK and a SQL Server. Six environment variables - see [`envvars.example`](envvars.example) for the full list

```bash
dotnet ef database update --project APIBankingDiplom
dotnet run --project APIBankingDiplom --launch-profile https
```

RSA signing keys generate themselves on first run in Development

Swagger should be at https://localhost:7235/swagger (development only)

## Limitations

- **No rate limiting anywhere**
- **Card numbers embed the user id** - plaintext digits, so a known user id narrows the card number space to a few hundred candidates. Convenient for generation but bad for privacy
- **PIN and CVV are stored reversibly encrypted, not hashed**
- **Refresh tokens are not rotated**
- **`AESPotentialSizeIncrease` is a fixed addend** - Base64 expansion affects the whole string, so the larger encrypted columns are undersized
- **No proper logging** - `LogModel` exists in the schema and is never written to
- **No automated tests, no CI**
- Gmail SMTP is not a production mail transport
- Secrets are flat environment variables — the AES, HMAC and RSA keys belong in a managed secret store

## Fixes

Before publishing this project here I have re-tested it and made a few patches and fixes. The architecture of the project is the same as it was in 2024, all of this is just bug fixes, QOL at most:
- Token cleanup service no longer deletes token database rows with "IsInvalid" being true, because that nullified token invalidation
- Token cleanup service no longer deletes email-verified users with expired email verification token (didn't work anyway, threw 547 due to DeleteBehavior.Restrict)
- ValidateIdentityFilter now identifies bearer token prefix case-insensitively
- Logout actually invalidates tokens
- Password reset actually invalidates tokens
- Password reset email subject is now proper
- Email change token now uses proper lifetime
- Refreshing an access token now provides a token with actual user claims
- Confirming email change re-checks if new email is free
- Profile pictures are served with proper image type instead of "image/jpeg" all the time
- Timestamps now use UTC time instead of local time
- Currency conversion cache invalidation condition is no longer inverted
- Avatar upload now handles names without a dot for the file extension
- Money transfers no longer record the sender balance as receiver and receiver balance as sender
- Transaction history is now filtered correctly by currency and respects selected by user sort order
- Zero-amount money transfers are now prevented
- Commission now records the commission instead of the whole withdrawal
- ATM deposits no longer accept negative money
- Exchangerate-api key is now a secret variable
- Exchange rates are now parsed culture invariantly
- Exchange rates are no longer rounded to 2 decimals
- Cached exchange rates no longer reference a disposed JSON document (it's cloned now)
- Transfers to non-existent cards now return a proper error
- Card number generation no longer skips 0 Luhn digit
- Expired cards can no longer be reactivated by user
- Card balance limits can no longer be negative
- Duplicate currency balance error http code is now 409 (Conflict) instead of 403 (Forbidden)
- RSA signing keys are now automatically generated in development
- RSA key format moved from .xml to .pem, and from 2048 bit to 4096 bit
- SixLabors.ImageSharp updated due to having a discovered vulnerability
- Removed a large portion of null-forgiving operators by making the authenticated user non-nullable and annotating AvailabilityResult
- Comprehensive exception messages if secret variable(s) is/are missing

## What I'd change now

Two years on, the things I'd do differently are less about features and more about structure:

- **Separate business logic from `BankingContext`** - deposit, withdraw, card generation and limit checks all
  live on the `DbContext`. Persistence and domain logic should be separate
- **Split `SecurityMeasures`** - hashing, encryption, JWT work, SMTP, error
  objects and exception wrappers in one class. That should be multiple classes, each having their own single purpose, not one big class
- **No `TryExecutingAsync` abuse** - here it swallows all crashes and makes every exception a "Could not connect to
  database" error. Exception handling middleware that provides problem details is the better approach.
- **`ILogger` usage** - actually log stuff, pretty obvious

The security primitives I picked are probably fine. The operational layer around them is where the inexperience shows

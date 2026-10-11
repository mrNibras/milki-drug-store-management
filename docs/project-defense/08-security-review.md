# Security review

This is a source review and limited public HTTP probe, not a penetration test or compliance certification.

## Controls found

- JWT Bearer authentication validates signing key and claims, zero clock skew, role/name claims. Token validation rechecks the user’s active status through `IUserActivityService`.
- BCrypt password verification/hashing is used in AuthService.
- Controller/action `[Authorize]`, role checks, and AdminOnly/PharmacistAllowed policies protect many endpoints.
- Server-side response mapping removes supplier purchase-cost data for non-admin users; related integration coverage exists.
- Password reset response is generic, reset token is one hour and single-use in the service flow.
- Global exception filter hides unexpected production details and returns a correlation ID.
- Security headers middleware and Vercel headers are configured.

These are code observations; production configuration and runtime behavior remain only partly verified.

## Findings and priority

| Priority | Finding | Impact | Evidence | Recommendation |
|---|---|---|---|---|
| High | Login action logs full response data, including access/refresh tokens | Tokens can be copied from browser console/log capture | `frontend/src/store/appStore.ts` login action | Remove payload logging; test production build/source for token redaction |
| High | JWT secret has a code fallback if configuration is absent; production behavior must fail closed | Predictable signing secret could invalidate token security if deployed without override | `Infrastructure/Services/JwtTokenService.cs`, Program config | Remove fallback, enforce required strong secret and fail startup; rotate any exposed production key |
| High | API host returned `404` with `x-render-routing: no-server` | No verified API runtime/end-to-end availability | Live probe 2026-10-10; `render.yaml` only describes desired deployment | Check Render service state, deploy logs, health checks and DB from dashboard |
| High | JWT and password-reset tokens are stored raw in DB; browser token storage is localStorage | Database/XSS compromise can expose bearer credentials | RefreshToken/PasswordReset entities, AuthService, frontend storage | Hash refresh/reset tokens at rest; prefer HttpOnly secure cookies/BFF where suitable |
| Medium-high | No server logout/revoke endpoint found; browser logout only clears local values | A copied refresh token can remain usable until rotation/expiry/revocation | AuthController and `appStore.logout` | Add authenticated revocation/logout and session-management tests |
| Medium-high | `AllowedOrigins` in Render YAML does not match Program's `Cors:AllowedOrigins` binding | Environment override likely ignored; default config might still permit frontend origin | `render.yaml`, `Program.cs`, CorsSettings | Align environment key to `Cors__AllowedOrigins`; verify preflight from deployed host |
| Medium | Controller local catch blocks may expose exception messages and return 400 for server errors | Leaks internals/inaccurate client semantics | AuthController/SalesController and others | Centralize mapping; allowlist client errors and return 500 for unexpected errors |
| Medium | Validators exist but registration/pipeline was not found | Some rules may not run at HTTP boundary | Application Validators and ServiceCollectionExtensions | Register validators or use explicit service validation; add invalid DTO tests |
| Medium | Access token is in localStorage and page logs contain sale/purchase values | XSS or shared console capture can expose data | frontend api/appStore/pages | Minimize sensitive data in browser, deploy CSP and XSS-safe rendering, redact logs |
| Medium | JWT secret missing configuration may fall back to source default; options only call ValidateOnStart without visible required-value validation | Misconfiguration can start with weak/default signing key | Program.cs, JwtSettings, JwtTokenService | Require minimum entropy/non-placeholder secret and no fallback |
| Medium | Sale allocation does not appear to exclude expired batches in query before daily expiry sweep | Expired stock might be sold in the interval before job | SaleService + daily expiry job | Add explicit expiry filter and deterministic tests |
| Medium | No optimistic concurrency token/stock lock found for competing batch updates | Concurrent cashiers may oversell stock | batch config/service | Add concurrency control or atomic conditional decrement; race tests |
| Medium | Data Protection XML keys persisted without application-level encryption | Key file compromise may affect protected payloads | Program comments/config | Use certificate/key vault encryption and restrict disk access for production security needs |
| Low-medium | EF analyzer warns about interpolated ExecuteSqlRaw in sequence-lock queries | Analyzer concern and future misuse risk; current value is numeric lock key | PurchaseRepository/SaleRepository | Use parameterized SQL to remove analyzer warning |

## Secret/config handling

This guide intentionally omits all secret values. Configuration files and Render YAML were inspected by key/name only. Production secret values and dashboard overrides were not inspected. Keep JWT, database, SMTP, and AWS credentials in hosting secret storage; rotate any credential exposed through source/logs.

## Defense answer

“The API validates JWTs and checks role authorization server-side; it also rechecks whether the user is active. The UI route guards are only convenience. I found important gaps: the frontend logs the full login response, tokens are held in localStorage and raw refresh/reset token values are stored server-side, and production hosting was not confirmed. I would first remove sensitive logs, require a non-default secret, hash stored tokens, and verify the live API and CORS settings.”

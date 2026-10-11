# Safe live demonstration script

## Preconditions

The Render API returned no-server on 10 October 2026. Therefore do not plan a production-hosted walkthrough until an authorized operator confirms the service is running. Use an approved local or staging environment with a disposable PostgreSQL database, synthetic products/suppliers/users, and non-production credentials. Do not show real credentials, customer information, tokens, environment values or the local DB artifact.

If no working environment is available, present a short screen recording or screenshots explicitly labeled as captures from a prior/local run, and state that current deployment was unavailable. Do not imply a screenshot proves live status.

## Demonstration path

| Step | Screen/action | Expected outcome | API/rule to explain | Recovery |
|---|---|---|---|---|
| 1 | Sign in with dedicated Admin demo account | Dashboard appears | `POST /api/auth/login`; BCrypt verification, account active/approved, JWT issued | Check API health and test account only; never reveal real password |
| 2 | View dashboard | Summary cards/charts load | `GET /api/dashboard/summary` or report endpoints; Admin restriction | If slow, use prepared local screenshots labeled as such; avoid repeated submits |
| 3 | Search a medicine/cosmetic | Product detail and available stock shown | Authenticated catalog GET/search; batch balances | Use a seed product with known stock; refresh after a transient error |
| 4 | Open supplier list and detail | Supplier information displayed | Admin supplier endpoints; supplier associated with purchase/batch | Skip if role/environment unavailable; do not edit production supplier data |
| 5 | Record a purchase in test DB | Purchase and batch added | `POST /api/purchases`; service transaction, purchase items and batch creation | Use small synthetic quantities; verify response and refresh; test DB only |
| 6 | Inspect inventory/batch/expiry | Balance and batch dates shown | Batch balance formula and branch context | If stale, trigger a server refetch and compare API response; do not claim persisted based only on UI |
| 7 | Record a small sale | Sale receipt/summary and stock decreases | `POST /api/sales`; role-based discounts, FEFO selection, transaction | Use known test batch; if sale fails, check current batch balance and do not retry blindly |
| 8 | Show sales history/report | Sale appears in list/report | GET `/api/sales`, Admin reporting routes and business-period timezone | State reports may be branch/memory scoped imperfectly; do not treat report discrepancy as data loss without checking DB |
| 9 | Show damage/expiry area | History and notifications | `POST /api/damages`, GET `/api/expired`; background expiry job | Avoid actually expiring a product by changing system dates; use prepared test fixture |
| 10 | Show role boundary | Pharmacist cannot access Admin-only endpoint | Backend should return 403; UI hiding alone is insufficient | Use separate test token/accounts, never alter a real role during demo |

The repository has sales/purchase screens and APIs, but live availability is not established. If a step cannot be run, call it unavailable and explain the intended code path; do not claim the flow is currently operating.

## Fallback plan

- **Internet/hosting failure:** switch to a local disposable environment if already prepared; otherwise present diagrams and clearly dated screenshots/recording.
- **API down:** show source route and deployment probe evidence; do not make repeated writes.
- **Database unavailable:** stop mutation attempts and explain startup migration/connection dependency.
- **Expired authentication:** use the approved demo login; do not show or copy token values.
- **Incorrect demo data:** use seeded disposable fixtures; do not improvise production edits.
- **Slow page:** wait once, check network/console privately, then move to a prepared capture.
- **Feature error:** state the observed result, identify likely boundary, and use the troubleshooting method from 11; do not conceal it.

Prepare a short recording and 4–6 screenshots from a test account. Label environment and capture date in the presenter’s notes. Store them in an approved location, and confirm they contain no secrets.

# Seven-day study plan

Spend roughly 60–90 focused minutes daily. Write explanations in your own words rather than memorizing this guide.

| Day | Study and source files | Concepts / practice | Rehearsal and finish line |
|---|---|---|---|
| 1 — Product and scope | `00-project-overview.md`, README, `App.tsx`, controllers list | Roles, pharmacy workflow, scope versus proposal | Explain the project in 30 seconds; completion: state 5 implemented modules and 3 unverified items without notes |
| 2 — Frontend | `App.tsx`, `ProtectedRoute.tsx`, `services/api.ts`, `store/appStore.ts`, POSPage | Router, Zustand, Axios interceptors, localStorage, 401 vs 403 | Trace login and one sale click; completion: draw browser → API flow and explain UI vs server auth |
| 3 — Backend | `Program.cs`, service registration, SalesController, AuthController, GlobalExceptionFilter | DI, middleware, DTO, role claims, errors, MediatR | Trace a request; completion: identify where authentication, authorization, validation and exception mapping happen |
| 4 — Database | `AppDbContext`, `Configurations/`, `Migrations/`, ER diagram | PK/FK/index/unique, batches, transactions, migrations, shadow UserId | Explain sale header/lines and product batches; completion: describe 8 entity relations and one schema caveat |
| 5 — Business rules | `SaleService.cs`, `PurchaseService.cs`, `InventoryService.cs`, `ReportService.cs` | FEFO, discounts, payments, expiry, branch and report boundaries | Work one sample purchase and sale on paper; completion: explain formulas and 4 edge cases |
| 6 — Tests, security, deployment | `09-testing-and-quality.md`, `08-security-review.md`, `10-deployment-and-persistence.md`, TRX summary | Unit vs integration, Testcontainers, token safety, source vs live evidence | Practice the failure explanation; completion: state exact counts and deployment probe with no overclaim |
| 7 — Full rehearsal | Slides, demo script, Q&A bank, revision sheet | Presentation pacing, fallback, hard questions | Rehearse 12-minute talk and 5-minute Q&A; completion: answer 10 random questions and deliver demo fallback calmly |

## One-day emergency revision

1. 25 minutes: read revision sheet and overview; memorize exact honest test/deployment status.
2. 35 minutes: trace `SaleService` from SalesController to UnitOfWork and diagram the database path.
3. 25 minutes: rehearse authentication/authorization and one main security weakness.
4. 25 minutes: rehearse 30-second and 1-minute introductions plus demo fallback.
5. 20 minutes: answer 10 Q&A items aloud; mark weak answers and cite the file path.

Do not trade accuracy for speed. If asked about uncertain production status, answer with the dated evidence and say which dashboard proof is missing.

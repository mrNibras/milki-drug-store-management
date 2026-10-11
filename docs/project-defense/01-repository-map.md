# Repository map

## Structure

~~~text
backend/
  MilkiDrugStore.sln
  Dockerfile, docker-compose.yml, docker-entrypoint.sh
  src/
    MilkiDrugStore.Domain/       Entities, enums, domain interfaces/events
    MilkiDrugStore.Application/ DTOs, services, commands, queries, handlers, validators
    MilkiDrugStore.Persistence/ AppDbContext, EF configs, migrations, repositories
    MilkiDrugStore.Infrastructure/ email, storage, JWT, logging, background jobs
    MilkiDrugStore.Api/          Program.cs, controllers, middleware, configuration
    MilkiDrugStore.Tests/        unit, API, PostgreSQL/Testcontainers tests
frontend/
  package.json, vite.config.ts, tsconfig*.json, vercel.json
  src/App.tsx, main.tsx, services/, store/, pages/, components/, layouts/, types/
docs/
  ER, architecture, module, use-case, class and activity/sequence diagrams
  project-defense/               this evidence-based study guide
render.yaml
README.md and deployment/readme material
~~~

`rg --files` was used to inspect the source tree; generated build directories and the local `.db` file were not read. The root contains a local database artifact under the API project; its contents were deliberately not inspected.

## High-value file map

| Path | Responsibility and key declarations | Called by / calls | Business significance and failure risks |
|---|---|---|---|
| `frontend/src/main.tsx` | React entry point; renders application and initializes theme | Browser loads entry → App | Startup boundary; browser storage availability must be handled |
| `frontend/src/App.tsx` | Route tree, auth hydration, public settings fetch, periodic refresh | Browser Router; Zustand | Navigation and route guards; local token/user data is trusted for initial UI state until API request |
| `frontend/src/services/api.ts` | Axios base URL, bearer injection, 401 refresh queue, logout on refresh failure | Store actions and page APIs | Central API auth/error path; localStorage token exposure and retry edge cases |
| `frontend/src/store/appStore.ts` | Zustand state/actions for auth, products, sales, inventory and more | Pages call selectors/actions; actions call Axios | Shared UI data flow; contains login response and sale console logging |
| `frontend/src/components/ProtectedRoute.tsx` | Client-side authenticated/role route check | App route elements | UX restriction only; server still must enforce roles |
| `backend/src/MilkiDrugStore.Api/Program.cs` | App configuration, DI, JWT, CORS, middleware, migrations/seeding, routes, hosted jobs | Process entry point | Startup can fail on data-dir/DB/schema issue; runs DB migrations on startup |
| `backend/src/MilkiDrugStore.Api/Extensions/ServiceCollectionExtensions.cs` | Registers application services, repositories and MediatR handlers | Program invokes `AddApplicationServices` | Determines runtime dependency graph; test fixtures need same dependencies |
| `backend/src/MilkiDrugStore.Api/Controllers/` | HTTP endpoint and authorization boundaries | ASP.NET routing → services/MediatR | HTTP status/role logic; broad catches can misclassify failures |
| `backend/src/MilkiDrugStore.Api/Middleware/GlobalExceptionFilter.cs` | Maps uncaught exceptions to `ApiResponse` and correlation ID | MVC exception filter | Masks unexpected production details; some controllers bypass it with local catches |
| `backend/src/MilkiDrugStore.Application/Services/SaleService.cs` | Transaction, batch allocation, pricing, discounts, payment and sale persistence | SalesController and other services | Core POS calculation; audit follows commit, and expired batch filtering needs review |
| `backend/src/MilkiDrugStore.Application/Services/PurchaseService.cs` | Validates purchase inputs, creates header, lines and batches | PurchasesController | Stock intake and costs; requested payment status can override derived status |
| `backend/src/MilkiDrugStore.Application/Services/InventoryService.cs` | Damage records, current stock and expired stock write-offs | Damages/Expired endpoints and expiry job | Stock reduction and traceability; errors should preserve atomic writes |
| `backend/src/MilkiDrugStore.Application/Services/AuthService.cs` | Login, registration, approval, token refresh, password reset, settings | AuthController | Account lifecycle, BCrypt, refresh rotation and reset tokens |
| `backend/src/MilkiDrugStore.Application/Services/ReportService.cs` | Dashboard, sales/inventory/supplier/staff reports and period logic | Reports/Dashboard controllers and handlers | Reporting correctness, branch scope and memory use need review |
| `backend/src/MilkiDrugStore.Persistence/Context/AppDbContext.cs` | 24 DbSets and applies EF configuration assembly | DI creates scoped context | Canonical persistence model; compare migrations and current model snapshot |
| `backend/src/MilkiDrugStore.Persistence/Context/DbProviderResolver.cs` | Reads configured connection string or DATABASE_URL, uses Npgsql | Program configures EF Core | Only PostgreSQL provider is configured here; no SQLite resolver path |
| `backend/src/MilkiDrugStore.Persistence/Repositories/UnitOfWork.cs` | Shares context, repositories, SaveChanges, transactions | Services via IUnitOfWork | Coordinates atomic writes; disposal matters |
| `backend/src/MilkiDrugStore.Persistence/Configurations/` | Fluent constraints, indexes, precision, relations | AppDbContext applies assembly | Database integrity contract |
| `backend/src/MilkiDrugStore.Persistence/Migrations/` | Five migration generations plus model snapshot | Program runs Migrate on relational DB | Schema evolution; production startup DDL is an operational risk |
| `backend/src/MilkiDrugStore.Infrastructure/BackgroundJobs/` | Expiry, notification, retention hosted services | Program `AddHostedService` | Periodic maintenance; job cadence and multi-instance duplication matter |
| `backend/src/MilkiDrugStore.Tests/` | xUnit unit/API/integration/PostgreSQL tests | `dotnet test` | Reproducible behavior; two fixtures currently fail DI before assertions |
| `backend/Dockerfile` | Multi-stage .NET 8 image, runtime environment | Render config references it | Reproducible container build; URL env syntax merits correction/validation |
| `render.yaml` | Render web service, managed DB declaration, disk, env names, health path | Render blueprint/config | Declares intended topology, does not prove active service/dashboard state |
| `frontend/vercel.json` | SPA rewrite and security headers | Vercel deployment | Supports client-side deep links; config is not proof of deployment contents |

## Project references and architecture boundaries

- Domain has entities and abstractions.
- Persistence references Domain.
- Application references **both Domain and Persistence**, which creates coupling and means this is not strict Clean Architecture.
- Infrastructure references application/domain/persistence abstractions and implements technical services.
- API composes the parts and references Application/Infrastructure.
- Tests reference all projects.

## Git evidence and safe reading order

HEAD is `fbe48983` on `master`, equal to `origin/master` at inspection; the repository had pre-existing untracked documentation artifacts. Recent history includes cost redaction authorization tests (`6f8153f8`), cosmetic category integration coverage (`6e6afdeb`), responsive report fixes, and a health-route fix (`6958d075`). Commit messages are evidence of changes in shared history, not individual authorship or proof of production deployment.

Study in this order: `Program.cs` → `AppDbContext` and configurations → `AuthService` → `SaleService` → `PurchaseService` → `InventoryService` → controllers → `frontend/src/App.tsx` → `api.ts` → `appStore.ts` → tests. Keep secrets and the local DB contents out of screenshots and notes.

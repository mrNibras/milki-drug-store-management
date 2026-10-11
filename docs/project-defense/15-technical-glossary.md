# Technical glossary tied to MDSMS

| Term | Simple / technical definition | MDSMS example and location | Possible evaluator question |
|---|---|---|---|
| API | A program interface; HTTP endpoints for client/server | Controllers under `MilkiDrugStore.Api/Controllers` | How does POS call the backend? |
| REST | HTTP resource-oriented design using verbs/status codes | `/api/sales`, `/api/medicines` | Why REST over GraphQL? |
| HTTP | Request/response protocol | Axios requests to ASP.NET | What is 401 vs 403? |
| JSON | Text format for structured data | DTO request/response; camelCase in Program | Why serialize as JSON? |
| DTO | Data Transfer Object, request/response shape | Application DTOs | Why not return EF entity directly? |
| ORM | Object-relational mapper between classes and tables | EF Core | What does an ORM do? |
| EF Core | .NET ORM with tracking, LINQ, migrations, SaveChanges | Persistence project | How is a sale saved? |
| DbContext | Scoped EF unit for query/change tracking | `AppDbContext` | What is its lifetime? |
| Dependency injection | Runtime supplies registered dependencies | Program/ServiceCollectionExtensions | Why register interface and implementation? |
| Middleware | Ordered HTTP pipeline components | Authentication, authorization, CORS, security headers | Why middleware order matters? |
| MediatR | In-process mediator dispatching requests to handlers | Selective query/command handlers | Is all code CQRS? No |
| CQRS | Separates read and write models/requests | Present selectively, not uniformly | Why call it partial? |
| JWT | Signed token carrying claims | AuthService and JwtBearer setup | Can a role claim alone revoke a user? |
| Authentication | Establishes identity | JWT signature and active-user check | How does login work? |
| Authorization | Decides access to endpoint/resource | `[Authorize(Roles="Admin")]` | Can UI hiding enforce it? |
| RBAC | Access decisions based on roles | Admin and Pharmacist policies | What is restricted to Admin? |
| Primary key | Unique row identifier | `SaleId`, `ProductId` | Why use one? |
| Foreign key | Database reference to another row | SaleItem → Sale | Which product fields lack FK? |
| Migration | Versioned schema change | Five Persistence migrations | How is schema deployed? |
| Transaction | Atomic group of database operations | Sale/Purchase UnitOfWork | What happens on exception? |
| ACID | Atomicity, consistency, isolation, durability | DB transaction protects sale rows | Does it include external audit/email? |
| CORS | Browser policy controlling cross-origin requests | Program Cors settings | Why must the configured origin match? |
| Docker | Container image packaging/runtime isolation | `backend/Dockerfile` | What is in the image? |
| CI/CD | Automated build/test/deploy pipeline | No workflow file found in inspected repo; hosting integration unverified | Is deployment automatic? |
| Unit test | Tests one component, commonly with mocks | SaleService/AuthService tests | What does a passing unit test prove? |
| Integration test | Exercises multiple components/provider/API | WebApplicationFactory/Testcontainers | Why use PostgreSQL container? |
| Entity | Domain/persistence object representing a business concept | Sale, batch, purchase | Entity vs DTO? |
| Repository | Data access abstraction around queries/writes | Generic/specialized repositories | Why use repository with EF? |
| Service layer | Application business logic | SaleService/PurchaseService | Why not put rules in controller? |
| State management | Holds UI data across components | Zustand `appStore` | What survives refresh? |
| Environment variable | Runtime/deploy configuration input | Connection string, JWT config, origins | Which setting controls CORS? |
| FEFO | First-expiring-first-out batch allocation | SaleService ordering; test covers ordering | Does it exclude expired lots? |
| Unit of Work | Groups repository operations and commit/rollback | `UnitOfWork` over one DbContext | What does SaveChanges do? |
| CORS preflight | Browser OPTIONS check before some cross-origin calls | API CORS policy | What breaks if origins mismatch? |
| Idempotency | Repeating an operation does not duplicate effect | Expiry sweep has rerun test | Is sale POST idempotent? Not established |
| Soft delete | Marks record inactive instead of removing row | Medicine/Cosmetic delete service | Why preserve history? |
| Testcontainers | Starts disposable containerized dependencies for tests | PostgreSQL test fixtures | Was a production DB used? No |
| Npgsql | .NET PostgreSQL EF provider | `DbProviderResolver` | Does the app configure SQLite? No |
| BCrypt | Password hashing/verification library | AuthService | Why hash passwords? |
| C# async/await | Nonblocking asynchronous I/O flow | EF service methods | Does async mean parallel? No |
| LINQ | .NET query syntax/operators over collections/providers | Repository and report queries | When does a query run? At materialization |
| Eager loading | Loads related data explicitly with Include | Medicine/batch/supplier response queries | Does the project use lazy loading? Not found |
| Claim | Identity attribute carried in principal/token | subject, role, branch claims | How is branch read? |
| Branch scope | Data associated with store branch | sale/purchase/batch branch IDs | Is every report correctly filtered? Not confirmed |
| Caching | Reuse prior data to reduce work | No general server cache found | Does local Zustand data prove persistence? No |
| S3 | Object storage API | `S3FileStorageService` selection | Is production S3 verified? No |
| Observability | Logs/metrics/traces/health useful in operation | ILogger, health endpoint, DB monitoring service | Can logs expose secrets? Avoid logging tokens |
| Data Protection | ASP.NET key ring protecting payloads | Persisted under configured data directory | Are XML keys encrypted by app? No encryptor configured |
| BFF | Backend-for-frontend session boundary | Not implemented | How could it reduce token exposure? |
| FEFO | Expiry-priority allocation | Sale batches ordered by expiry | What is the edge case? Past-expired lots |

General concepts like GraphQL, a BFF, CI/CD and browser E2E testing are comparison concepts, not confirmed implemented features here.

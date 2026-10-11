# Evaluator questions and answers

Each answer is a rehearsal prompt. Personalize context and contribution only from your own records. Evidence paths are repository-relative. “Confidence” reflects evidence level, not how certain the speaker sounds.

## A. Project fundamentals

**1. Question:** What is MDSMS? **Short answer:** A pharmacy operations web application. **Technical explanation:** It models products, batches, purchases, sales, users, stock events and reports. **Repository evidence:** `frontend/src/App.tsx`; API controllers; `AppDbContext.cs`. **Likely follow-up:** Is it live? **Common mistake:** Claiming verified production operation. **Confidence:** VERIFIED

**2. Question:** Why was it developed? **Short answer:** To support connected pharmacy workflows. **Technical explanation:** Batch, branch, supplier, expiry and transaction data are represented together. **Repository evidence:** Domain entities and services. **Likely follow-up:** Did it improve efficiency? **Common mistake:** Inventing measured outcomes. **Confidence:** INFERRED

**3. Question:** What problem does it address? **Short answer:** Tracking inventory and pharmacy transactions. **Technical explanation:** Products have batch-level quantities; purchases, sales, damage and expiry change stock. **Repository evidence:** `SaleService.cs`; `PurchaseService.cs`; `InventoryService.cs`. **Likely follow-up:** Is stock always accurate? **Common mistake:** Saying without concurrency caveats. **Confidence:** VERIFIED

**4. Question:** Who are the main users? **Short answer:** Administrators and pharmacist/staff users. **Technical explanation:** API attributes and UI routes distinguish roles. **Repository evidence:** Controllers and `ProtectedRoute.tsx`. **Likely follow-up:** What can pharmacists not do? **Common mistake:** Assuming UI hiding is sufficient. **Confidence:** VERIFIED

**5. Question:** What are the project objectives? **Short answer:** Organize core store operations in one system. **Technical explanation:** Catalog, purchases, batch inventory, POS, damage/expiry and reports are present. **Repository evidence:** Routes/pages/services. **Likely follow-up:** Which is most important? **Common mistake:** Claiming an approved objective document if absent. **Confidence:** INFERRED

**6. Question:** What is in scope? **Short answer:** Product, supplier, purchase, sale, inventory, user and report workflows. **Technical explanation:** These have screens and API/service paths. **Repository evidence:** `frontend/src/pages/`; API controllers. **Likely follow-up:** Is accounting included? **Common mistake:** Treating reports as a full accounting ledger. **Confidence:** VERIFIED

**7. Question:** What is out of scope? **Short answer:** No patient record, prescription or payment gateway was found. **Technical explanation:** Current sale stores payment method/status/amount values only. **Repository evidence:** Sale DTO/entity and controller search. **Likely follow-up:** Could one be added? **Common mistake:** Calling payment status card settlement. **Confidence:** NOT IMPLEMENTED

**8. Question:** What is a medicine batch? **Short answer:** A lot of medicine with quantity, branch and expiry details. **Technical explanation:** Separate rows track received, issued, damaged and expired amounts. **Repository evidence:** `MedicineBatch.cs`. **Likely follow-up:** Why not product quantity only? **Common mistake:** Ignoring lot traceability. **Confidence:** VERIFIED

**9. Question:** Why track cosmetics separately? **Short answer:** They have distinct catalog and batch data. **Technical explanation:** Cosmetic batches support their own supplier, prices, expiry and stock fields. **Repository evidence:** `Cosmetic.cs`, `CosmeticBatch.cs`. **Likely follow-up:** Can cosmetics have no expiry? **Common mistake:** Assuming all fields mirror medicines. **Confidence:** VERIFIED

**10. Question:** What is FEFO? **Short answer:** First-expiring stock is selected first. **Technical explanation:** Sale batch query orders by expiry, and a test checks priority. **Repository evidence:** `SaleService.cs`; `FEFO_Should_Prioritize_Earliest_Expiry`. **Likely follow-up:** Are past-expired lots excluded? **Common mistake:** Equating order with expiry filtering. **Confidence:** VERIFIED

**11. Question:** What is the expected benefit? **Short answer:** Better visibility and traceability of pharmacy operations. **Technical explanation:** This is an intended benefit, not an evaluated result. **Repository evidence:** Data model/workflows. **Likely follow-up:** How was it measured? **Common mistake:** Claiming measured savings. **Confidence:** INFERRED

**12. Question:** What is your contribution? **Short answer:** State the work you can personally verify. **Technical explanation:** Shared Git history does not prove individual authorship. **Repository evidence:** Your commits/issues/internship record, not inferred here. **Likely follow-up:** Show a change you made. **Common mistake:** Claiming all repository features. **Confidence:** NOT VERIFIED

**13. Question:** What is the biggest limitation? **Short answer:** Current test failures and unavailable API host are important. **Technical explanation:** 17 test fixture failures and a no-server health response limit confidence. **Repository evidence:** TRX and dated curl output. **Likely follow-up:** What would you do next? **Common mistake:** Hiding those facts. **Confidence:** VERIFIED

## B. Requirements and analysis

**14. Question:** How were requirements gathered? **Short answer:** The repository does not establish the original method. **Technical explanation:** No interview, observation or signed requirements record was verified. **Repository evidence:** README/docs scan. **Likely follow-up:** Who approved them? **Common mistake:** Inventing stakeholders or methods. **Confidence:** NOT VERIFIED

**15. Question:** Name functional requirements. **Short answer:** Auth, catalog, purchasing, sales, stock, damage/expiry and reports. **Technical explanation:** Each maps to API/controller and UI modules. **Repository evidence:** `07-api-reference.md`, controllers. **Likely follow-up:** Which are Admin-only? **Common mistake:** Presenting proposed features as implemented. **Confidence:** VERIFIED

**16. Question:** Name nonfunctional requirements. **Short answer:** Security, maintainability, persistence and responsive UI are design concerns. **Technical explanation:** Code includes JWT, layering, relational DB and responsive components, but no formal NFR acceptance suite was found. **Repository evidence:** Program, UI, configs. **Likely follow-up:** What performance target? **Common mistake:** Inventing SLAs. **Confidence:** PARTIALLY VERIFIED

**17. Question:** How do requirements map to modules? **Short answer:** Each operational workflow has a UI and API/service area. **Technical explanation:** POS maps to SalesController/SaleService; intake maps to PurchasesController/PurchaseService. **Repository evidence:** pages/controllers/services. **Likely follow-up:** Show the sale path. **Common mistake:** Mapping only screen names. **Confidence:** VERIFIED

**18. Question:** What assumptions does the system make? **Short answer:** Users, roles, branches and stock data are configured. **Technical explanation:** Many operations depend on authenticated user/branch claims and seeded roles. **Repository evidence:** `AuthService`, `CurrentUserService`, `DbSeeder`. **Likely follow-up:** What if branch is missing? **Common mistake:** Claiming every fallback is safe. **Confidence:** INFERRED

**19. Question:** What constraints exist? **Short answer:** .NET 8, PostgreSQL provider and browser client are concrete constraints. **Technical explanation:** Project targets net8 and resolver configures Npgsql. **Repository evidence:** csproj and `DbProviderResolver.cs`. **Likely follow-up:** Can it run on SQLite? **Common mistake:** Repeating stale README language as runtime fact. **Confidence:** VERIFIED

**20. Question:** How do you distinguish requirement from implementation? **Short answer:** A requirement states needed behavior; implementation is code evidence. **Technical explanation:** A route/config proves some code exists, not user acceptance. **Repository evidence:** tests/specs versus source. **Likely follow-up:** How was acceptance confirmed? **Common mistake:** Calling a button proof of completion. **Confidence:** VERIFIED

**21. Question:** What does Admin do? **Short answer:** Manages catalogs, users, suppliers, branches, purchases and reports. **Technical explanation:** Role attributes protect these endpoints. **Repository evidence:** `UsersController`, `PurchasesController`, `ReportsController`. **Likely follow-up:** Can pharmacist call them directly? **Common mistake:** Trusting frontend hiding. **Confidence:** VERIFIED

**22. Question:** What does pharmacist do? **Short answer:** Uses authenticated operational flows such as search, sales and damage. **Technical explanation:** API routes permit authenticated users in these controllers, with role-specific behavior. **Repository evidence:** Sales/Damages/Notifications controllers. **Likely follow-up:** Are all reads branch-limited? **Common mistake:** Assuming branch isolation everywhere. **Confidence:** VERIFIED

**23. Question:** Does the app handle customers? **Short answer:** No customer/patient model was found. **Technical explanation:** Sale rows attach to user/branch but not a customer entity. **Repository evidence:** `Sale.cs`, DbSets. **Likely follow-up:** Could it support customers later? **Common mistake:** Calling cashier a customer. **Confidence:** NOT IMPLEMENTED

**24. Question:** Is it a regulatory dispensing system? **Short answer:** No such certification or prescription workflow was verified. **Technical explanation:** Source review is not a regulatory assessment. **Repository evidence:** No prescription module found. **Likely follow-up:** What approvals would be needed? **Common mistake:** Claiming compliance. **Confidence:** NOT VERIFIED

**25. Question:** What future scope is reasonable? **Short answer:** Safer sessions, validation, concurrency, tests and deployment verification. **Technical explanation:** These address observed risks before expanding domain features. **Repository evidence:** `18-findings-and-verification-status.md`. **Likely follow-up:** What comes first? **Common mistake:** Prioritizing features over correctness/security. **Confidence:** INFERRED

## C. Architecture

**26. Question:** Explain the architecture. **Short answer:** React client calls a layered ASP.NET Core API backed by PostgreSQL. **Technical explanation:** API controllers delegate to services/selected handlers and EF Core repositories. **Repository evidence:** `Program.cs`, project references. **Likely follow-up:** Is it microservices? **Common mistake:** Saying yes. **Confidence:** VERIFIED

**27. Question:** Is it client-server? **Short answer:** Yes. **Technical explanation:** Browser uses HTTP JSON calls to a separate API. **Repository evidence:** Axios service and controllers. **Likely follow-up:** What does that add? **Common mistake:** Confusing client/server with deployment health. **Confidence:** VERIFIED

**28. Question:** Is it three-tier? **Short answer:** It has presentation, application and persistence responsibilities. **Technical explanation:** Those responsibilities are split across React and backend projects, though boundaries are not strict. **Repository evidence:** project tree/dependencies. **Likely follow-up:** Are all tiers separately deployed? **Common mistake:** Assuming yes. **Confidence:** VERIFIED

**29. Question:** Why not microservices? **Short answer:** A modular monolith is simpler for this scale and transaction scope. **Technical explanation:** One API and shared relational database reduce distributed transaction/operation complexity. **Repository evidence:** single API Dockerfile and solution. **Likely follow-up:** When split? **Common mistake:** Saying microservices are always more scalable. **Confidence:** INFERRED

**30. Question:** Is this Clean Architecture? **Short answer:** Not strictly. **Technical explanation:** Application directly references Persistence, which weakens dependency inversion. **Repository evidence:** Application csproj. **Likely follow-up:** What change would improve it? **Common mistake:** Judging only by folder names. **Confidence:** VERIFIED

**31. Question:** Does it use CQRS? **Short answer:** Selectively, not consistently. **Technical explanation:** MediatR handlers exist, while controllers also call services directly. **Repository evidence:** controllers, handlers, service registration. **Likely follow-up:** What is a query handler? **Common mistake:** Claiming all writes/reads are separated. **Confidence:** VERIFIED

**32. Question:** What is the repository pattern? **Short answer:** An abstraction for querying and persisting entity data. **Technical explanation:** Generic/specialized repositories use the shared EF context. **Repository evidence:** Persistence/Repositories. **Likely follow-up:** Is it redundant with EF? **Common mistake:** Claiming repositories eliminate SQL. **Confidence:** VERIFIED

**33. Question:** What does UnitOfWork do? **Short answer:** Shares one DbContext and coordinates save/transaction operations. **Technical explanation:** Sale/purchase service begins, commits or rolls back through it. **Repository evidence:** `UnitOfWork.cs`. **Likely follow-up:** What is outside its transaction? **Common mistake:** Including post-commit audit automatically. **Confidence:** VERIFIED

**34. Question:** Why React? **Short answer:** It supports interactive, component-based screens. **Technical explanation:** This is a defensible technical rationale; historical choice is not documented. **Repository evidence:** React app/package. **Likely follow-up:** What are alternatives? **Common mistake:** Claiming the original reason is known. **Confidence:** INFERRED

**35. Question:** Why ASP.NET Core? **Short answer:** It provides a structured API framework with DI and auth middleware. **Technical explanation:** Those are used by the implementation; historical motivation is unknown. **Repository evidence:** Program and controllers. **Likely follow-up:** Could Node do it? **Common mistake:** Claiming no alternatives. **Confidence:** INFERRED

**36. Question:** Why REST? **Short answer:** Resource routes and HTTP verbs fit CRUD and operational requests. **Technical explanation:** The client exchanges JSON over `/api` endpoints. **Repository evidence:** Controllers and Axios. **Likely follow-up:** Why not GraphQL? **Common mistake:** Saying GraphQL is inferior. **Confidence:** VERIFIED

**37. Question:** Why EF Core? **Short answer:** It maps C# entities to PostgreSQL and supports queries/migrations/transactions. **Technical explanation:** It reduces repetitive mapping but still needs careful query and schema design. **Repository evidence:** Persistence project. **Likely follow-up:** When use SQL? **Common mistake:** Assuming ORM queries are always optimal. **Confidence:** VERIFIED

**38. Question:** What is the architecture’s main tradeoff? **Short answer:** Simpler deployment and transactions, but shared code can become coupled. **Technical explanation:** Application’s direct Persistence reference is an example. **Repository evidence:** project references. **Likely follow-up:** What boundary would you improve? **Common mistake:** Describing no downsides. **Confidence:** VERIFIED

## D. Programming and source code

**39. Question:** What happens at startup? **Short answer:** Program binds config, registers services, builds pipeline, migrates/seeds DB and maps routes. **Technical explanation:** Data dir checks and DB initialization can prevent startup. **Repository evidence:** API `Program.cs`. **Likely follow-up:** Is migration production-safe? **Common mistake:** Treating startup as configuration only. **Confidence:** VERIFIED

**40. Question:** What is dependency injection? **Short answer:** The framework supplies a class’s dependencies. **Technical explanation:** API registers scoped interfaces and implementations; controllers/services receive them through constructors. **Repository evidence:** `ServiceCollectionExtensions.cs`. **Likely follow-up:** Why scoped? **Common mistake:** Confusing DI with service discovery. **Confidence:** VERIFIED

**41. Question:** What does a controller do? **Short answer:** Accepts HTTP, applies route/auth rules and calls application behavior. **Technical explanation:** It binds DTOs, invokes services/mediator and returns status/body. **Repository evidence:** SalesController. **Likely follow-up:** Where should sale math live? **Common mistake:** Putting all rules in React/controller. **Confidence:** VERIFIED

**42. Question:** What is a DTO? **Short answer:** A request/response object crossing the API boundary. **Technical explanation:** It separates wire shape from EF entities. **Repository evidence:** `Application/DTOs`. **Likely follow-up:** Does every endpoint map safely? **Common mistake:** Assuming DTO use guarantees redaction. **Confidence:** VERIFIED

**43. Question:** Why asynchronous methods? **Short answer:** Database and network I/O can wait without blocking request threads. **Technical explanation:** `await` yields until task completion; it does not automatically parallelize. **Repository evidence:** service methods. **Likely follow-up:** Is every async method correct? **Common mistake:** Saying async means faster always. **Confidence:** VERIFIED

**44. Question:** What does SaveChangesAsync do? **Short answer:** Persists tracked changes through EF Core. **Technical explanation:** It sends insert/update/delete operations; a surrounding transaction controls commit grouping. **Repository evidence:** UnitOfWork. **Likely follow-up:** Does it commit external emails? **Common mistake:** Treating it as entire workflow commit. **Confidence:** VERIFIED

**45. Question:** What is EF tracking? **Short answer:** DbContext remembers entity state changes. **Technical explanation:** Mutated loaded batch entities are written at SaveChanges. **Repository evidence:** SaleService batch updates. **Likely follow-up:** What about AsNoTracking? **Common mistake:** Assuming all queries track. **Confidence:** VERIFIED

**46. Question:** What is Include? **Short answer:** Explicit eager loading of a related entity/collection. **Technical explanation:** It asks EF to load navigation data with the query. **Repository evidence:** medicine/report queries. **Likely follow-up:** Can it cause large results? **Common mistake:** Claiming it is lazy loading. **Confidence:** VERIFIED

**47. Question:** What does middleware do? **Short answer:** Processes requests/responses in an ordered pipeline. **Technical explanation:** AuthN establishes identity before AuthZ evaluates roles. **Repository evidence:** Program.cs. **Likely follow-up:** Why order matters? **Common mistake:** Treating middleware as controller. **Confidence:** VERIFIED

**48. Question:** How are errors returned? **Short answer:** A global filter maps uncaught exceptions, but local catches differ. **Technical explanation:** Some argument/state errors become 400; unexpected errors generally 500 with correlation ID. **Repository evidence:** GlobalExceptionFilter and controllers. **Likely follow-up:** Can messages leak? **Common mistake:** Saying all errors are uniform. **Confidence:** VERIFIED

**49. Question:** What is a migration? **Short answer:** A versioned schema change. **Technical explanation:** EF applies migration classes to evolve PostgreSQL schema. **Repository evidence:** Persistence/Migrations; Program Migrate. **Likely follow-up:** What if it fails? **Common mistake:** Assuming automatic rollback of all deployed data. **Confidence:** VERIFIED

**50. Question:** How does logging help debugging? **Short answer:** It records server-side events and failures. **Technical explanation:** ILogger captures exception context; client correlation ID helps locate logs. **Repository evidence:** GlobalExceptionFilter/Program. **Likely follow-up:** What must never be logged? **Common mistake:** Logging tokens or secrets. **Confidence:** VERIFIED

**51. Question:** What does MediatR do here? **Short answer:** Dispatches selected request types to handlers. **Technical explanation:** Handlers are registered by scanning Application, but not all endpoints use them. **Repository evidence:** ServiceCollectionExtensions, handlers. **Likely follow-up:** Is this full CQRS? **Common mistake:** Equating package presence with architecture. **Confidence:** VERIFIED

## E. Database

**52. Question:** How many DbSets exist? **Short answer:** Twenty-four. **Technical explanation:** AppDbContext exposes roles/users/branches, products/batches, transactions/history and archives. **Repository evidence:** `AppDbContext.cs`. **Likely follow-up:** Name the transaction tables. **Common mistake:** Counting old diagram only. **Confidence:** VERIFIED

**53. Question:** What is a primary key? **Short answer:** A unique identifier for a row. **Technical explanation:** EF uses fields such as SaleId to identify persisted records. **Repository evidence:** entity classes/config. **Likely follow-up:** Why not use sale number? **Common mistake:** Confusing business key and row key. **Confidence:** VERIFIED

**54. Question:** What is a foreign key? **Short answer:** A database reference enforcing a relation. **Technical explanation:** SaleItem.SaleId links a line to its sale. **Repository evidence:** SaleItemConfiguration. **Likely follow-up:** Are CategoryId and UnitTypeId FKs? **Common mistake:** Claiming they are. **Confidence:** VERIFIED

**55. Question:** Describe Sale and SaleItem. **Short answer:** Sale is header; SaleItem is each product/batch line. **Technical explanation:** Lines retain quantity, prices, discounts and profit fields. **Repository evidence:** Sale entities/configs. **Likely follow-up:** Why separate them? **Common mistake:** Duplicating header for every item. **Confidence:** VERIFIED

**56. Question:** Why batches? **Short answer:** To track lot quantities, suppliers and expiry separately. **Technical explanation:** A product can have many branch-specific batches. **Repository evidence:** MedicineBatch/CosmeticBatch configs. **Likely follow-up:** How is available stock calculated? **Common mistake:** One product-level number only. **Confidence:** VERIFIED

**57. Question:** What prevents duplicate products? **Short answer:** Configured unique indexes cover selected identifiers. **Technical explanation:** Medicine code/barcode and user email are examples. **Repository evidence:** MedicineConfiguration/UserConfiguration. **Likely follow-up:** What about categories? **Common mistake:** Assuming every field unique. **Confidence:** VERIFIED

**58. Question:** What is normalization? **Short answer:** Structuring relational data to reduce avoidable duplication. **Technical explanation:** Sale headers and lines, and products and batches, are separated. **Repository evidence:** entity relations. **Likely follow-up:** Is schema formally 3NF? **Common mistake:** Claiming without dependency analysis. **Confidence:** PARTIALLY VERIFIED

**59. Question:** What indexes exist? **Short answer:** Indexes support common lookups and unique constraints. **Technical explanation:** Configs index branch, dates, product/batch, notifications and IDs. **Repository evidence:** Persistence/Configurations. **Likely follow-up:** Do indexes cost anything? **Common mistake:** Saying indexes are free. **Confidence:** VERIFIED

**60. Question:** Why decimal for money? **Short answer:** Decimal preserves base-10 monetary precision better than binary float. **Technical explanation:** EF configs specify decimal precision. **Repository evidence:** Sale/Purchase configurations. **Likely follow-up:** What rounding policy? **Common mistake:** Assuming precision config defines every rounding decision. **Confidence:** VERIFIED

**61. Question:** How are transactions used? **Short answer:** Sale and purchase group multiple database writes. **Technical explanation:** Errors call rollback before commit, keeping related rows together. **Repository evidence:** services and UnitOfWork. **Likely follow-up:** Is audit in transaction? **Common mistake:** Saying all side effects are atomic. **Confidence:** VERIFIED

**62. Question:** What is a shadow FK? **Short answer:** EF model property without a CLR property. **Technical explanation:** InventoryTransaction has nullable UserId navigation FK separate from CreatedBy. **Repository evidence:** model snapshot and User navigation. **Likely follow-up:** Which value do services populate? **Common mistake:** Treating CreatedBy as same column. **Confidence:** VERIFIED

**63. Question:** Are category/unit relationships database-enforced? **Short answer:** No, product IDs are application-managed. **Technical explanation:** No FK configuration exists for Medicine/Cosmetic CategoryId/UnitTypeId. **Repository evidence:** configurations and ER note. **Likely follow-up:** What could improve? **Common mistake:** Drawing FK lines as database constraints. **Confidence:** VERIFIED

**64. Question:** Is production data persistence proven? **Short answer:** No. **Technical explanation:** Tests prove isolated DB behavior; live API/DB was not accessible. **Repository evidence:** persistence tests and no-server probe. **Likely follow-up:** How prove it? **Common mistake:** Equating code with deployment. **Confidence:** NOT VERIFIED

## F. Pharmacy business rules

**65. Question:** How is batch stock calculated? **Short answer:** Received minus issued, damaged and expired. **Technical explanation:** Batch balance is used for availability and write-offs. **Repository evidence:** batch models/services. **Likely follow-up:** Can balance go negative? **Common mistake:** Ignoring concurrent updates. **Confidence:** VERIFIED

**66. Question:** How does purchase affect stock? **Short answer:** It creates purchase rows, batches and inventory transactions. **Technical explanation:** The operation is wrapped in a DB transaction. **Repository evidence:** PurchaseService. **Likely follow-up:** What if line 2 fails? **Common mistake:** Claiming partial commit. **Confidence:** VERIFIED

**67. Question:** How does sale affect stock? **Short answer:** It increments issued quantity on chosen batches. **Technical explanation:** Sale items and inventory transactions are saved in the sale transaction. **Repository evidence:** SaleService. **Likely follow-up:** What prevents oversell? **Common mistake:** Ignoring concurrency races. **Confidence:** VERIFIED

**68. Question:** How are batches selected? **Short answer:** Manual selection can be supported; automatic mode orders by expiry. **Technical explanation:** Service reads settings and available branch batches. **Repository evidence:** SaleService helper/settings. **Likely follow-up:** Does query remove expired stock? **Common mistake:** Assuming FEFO implies filtering. **Confidence:** VERIFIED

**69. Question:** What if stock is insufficient? **Short answer:** Service throws and rolls back the transaction. **Technical explanation:** It checks remaining quantities across eligible batches. **Repository evidence:** SaleService. **Likely follow-up:** What does controller return? **Common mistake:** Assuming status mapping is always ideal. **Confidence:** VERIFIED

**70. Question:** How is a pharmacist discount limited? **Short answer:** At most 5% of batch selling price per unit. **Technical explanation:** Admin may discount up to the selling price; discounts need a reason. **Repository evidence:** SaleService and tests. **Likely follow-up:** Is discount total or per line? **Common mistake:** Omitting per-unit detail. **Confidence:** VERIFIED

**71. Question:** How is sale profit calculated? **Short answer:** Effective selling amount minus purchase/buying cost. **Technical explanation:** Service multiplies per-unit margin by quantity and persists line/total profit. **Repository evidence:** SaleService/SaleItem. **Likely follow-up:** Does this include overhead/tax? **Common mistake:** Calling it accounting net profit. **Confidence:** VERIFIED

**72. Question:** How does sale payment status work? **Short answer:** Zero/nonpositive is unpaid, between total and zero is partial, at least total is paid. **Technical explanation:** Amount due is set from total and paid amount. **Repository evidence:** SaleService. **Likely follow-up:** Are negative/overpayment bounds rejected? **Common mistake:** Saying all payment amounts validated. **Confidence:** VERIFIED

**73. Question:** Does the system process bank/card payments? **Short answer:** No gateway integration found. **Technical explanation:** It stores payment method, reference and amount/status fields. **Repository evidence:** Sale DTO/entity/controller. **Likely follow-up:** Can one be integrated? **Common mistake:** Equating a “cash/card” field with transaction processing. **Confidence:** NOT IMPLEMENTED

**74. Question:** How is damage recorded? **Short answer:** One batch reference, positive quantity, reason and available balance are required. **Technical explanation:** Stock and damage/history records are persisted together. **Repository evidence:** InventoryService and passing tests. **Likely follow-up:** How do cosmetics work? **Common mistake:** Forgetting separate cosmetic batch ID. **Confidence:** VERIFIED

**75. Question:** How is expiry handled? **Short answer:** Daily job writes off remaining expired quantity and records it. **Technical explanation:** Reruns should not duplicate because balance becomes zero. **Repository evidence:** ExpiryCheckBackgroundService; inventory tests. **Likely follow-up:** Can sale occur before sweep? **Common mistake:** Assuming daily job is immediate. **Confidence:** VERIFIED

**76. Question:** How are purchase payment statuses determined? **Short answer:** Derived from paid versus total unless a status is supplied. **Technical explanation:** Supplied status can override calculation in current code. **Repository evidence:** PurchaseService. **Likely follow-up:** Is input status validated? **Common mistake:** Claiming it always matches amounts. **Confidence:** VERIFIED

**77. Question:** What timezone do reports use? **Short answer:** Africa/Addis_Ababa business time. **Technical explanation:** BusinessClock defines day/week/month/fiscal period boundaries. **Repository evidence:** BusinessClock/ReportService/tests. **Likely follow-up:** What begins a week? **Common mistake:** Using UTC date labels as local business period. **Confidence:** VERIFIED

## G. Security

**78. Question:** How does login work? **Short answer:** Email is normalized, BCrypt verifies password, active/approved checks gate tokens. **Technical explanation:** AuthService returns JWT and refresh token. **Repository evidence:** AuthService.LoginAsync. **Likely follow-up:** What happens if inactive? **Common mistake:** Saying password alone is enough. **Confidence:** VERIFIED

**79. Question:** Authentication vs authorization? **Short answer:** Authentication establishes who; authorization checks what they may do. **Technical explanation:** JWT middleware authenticates; role attributes authorize. **Repository evidence:** Program/controllers. **Likely follow-up:** What is 401 vs 403? **Common mistake:** Mixing the terms. **Confidence:** VERIFIED

**80. Question:** Where are passwords stored? **Short answer:** As BCrypt hashes, not plaintext. **Technical explanation:** AuthService calls BCrypt hash/verify routines. **Repository evidence:** AuthService/User entity. **Likely follow-up:** Are tokens hashed too? **Common mistake:** Saying all credentials are hashed. **Confidence:** VERIFIED

**81. Question:** How are roles enforced? **Short answer:** ASP.NET authorization attributes/policies protect API actions. **Technical explanation:** Admin-only routes require role claim; active state is rechecked. **Repository evidence:** Program and controllers. **Likely follow-up:** Can client bypass? **Common mistake:** Treating hidden buttons as security. **Confidence:** VERIFIED

**82. Question:** What happens after user deactivation? **Short answer:** Token validation checks current active status. **Technical explanation:** Existing JWT can be rejected before expiry by database-backed user activity. **Repository evidence:** Program OnTokenValidated; tests. **Likely follow-up:** What if DB is down? **Common mistake:** Assuming offline validation still succeeds. **Confidence:** VERIFIED

**83. Question:** Where are tokens stored in browser? **Short answer:** localStorage. **Technical explanation:** Access and refresh tokens are manually persisted. **Repository evidence:** appStore/api/App. **Likely follow-up:** What risk? **Common mistake:** Calling it HttpOnly storage. **Confidence:** VERIFIED

**84. Question:** Is login response logged? **Short answer:** Yes, current frontend logs the full response. **Technical explanation:** It can include token values in the browser console. **Repository evidence:** `appStore.ts` login action. **Likely follow-up:** What fix? **Common mistake:** Dismissing console logs as harmless. **Confidence:** VERIFIED

**85. Question:** Is there server logout? **Short answer:** No logout endpoint was found. **Technical explanation:** Current logout clears local browser state; refresh server record may remain valid. **Repository evidence:** AuthController/appStore/AuthService. **Likely follow-up:** How revoke? **Common mistake:** Equating local clear with revocation. **Confidence:** NOT IMPLEMENTED

**86. Question:** How are reset tokens protected? **Short answer:** They expire and are single-use, but raw storage was found. **Technical explanation:** One-hour reset flow still leaves DB compromise risk. **Repository evidence:** AuthService/PasswordReset. **Likely follow-up:** What improve? **Common mistake:** Treating expiry as at-rest protection. **Confidence:** VERIFIED

**87. Question:** How are supplier costs protected? **Short answer:** Backend response mapping redacts costs for non-admins. **Technical explanation:** Security does not rely only on UI hiding. **Repository evidence:** Medicine/Cosmetic services; InventoryCostRedaction tests. **Likely follow-up:** What about reports? **Common mistake:** Assuming every route automatically redacted. **Confidence:** VERIFIED

**88. Question:** How should JWT secrets be configured? **Short answer:** Strong secret from protected runtime configuration, required at startup. **Technical explanation:** Source currently has a fallback risk if config is absent. **Repository evidence:** JwtTokenService and Program. **Likely follow-up:** Was production key checked? **Common mistake:** Exposing secret values. **Confidence:** PARTIALLY VERIFIED

**89. Question:** What does CORS do? **Short answer:** It controls which browser origins may call the API. **Technical explanation:** Program binds Cors:AllowedOrigins; Render YAML key appears mismatched. **Repository evidence:** Program/CorsSettings/render.yaml. **Likely follow-up:** Is CORS authorization? **Common mistake:** Treating it as API authentication. **Confidence:** VERIFIED

**90. Question:** Is the system penetration-tested? **Short answer:** No penetration test was performed in this task. **Technical explanation:** This was source/config review and public health probe only. **Repository evidence:** review scope. **Likely follow-up:** What would you test? **Common mistake:** Calling source review certification. **Confidence:** VERIFIED

## H. Frontend

**91. Question:** What frontend stack is used? **Short answer:** React, TypeScript, Vite, Router, Zustand and Axios. **Technical explanation:** Package manifest confirms versions. **Repository evidence:** `frontend/package.json`. **Likely follow-up:** Why Zustand? **Common mistake:** Claiming documented historical rationale. **Confidence:** VERIFIED

**92. Question:** How are pages routed? **Short answer:** React Router routes public and protected pages. **Technical explanation:** App defines auth routes and MainLayout children. **Repository evidence:** `App.tsx`. **Likely follow-up:** Does a protected route secure API? **Common mistake:** Saying yes. **Confidence:** VERIFIED

**93. Question:** What does Zustand do? **Short answer:** Shares application state and actions between components. **Technical explanation:** Store contains auth and operational state plus API actions. **Repository evidence:** `store/appStore.ts`. **Likely follow-up:** Does state survive reload? **Common mistake:** Assuming all store state persists. **Confidence:** VERIFIED

**94. Question:** How does Axios attach auth? **Short answer:** Request interceptor reads access token and adds Bearer header. **Technical explanation:** Response interceptor handles 401 refresh flow. **Repository evidence:** `services/api.ts`. **Likely follow-up:** What happens on 403? **Common mistake:** Treating all errors as logout. **Confidence:** VERIFIED

**95. Question:** How does refresh work? **Short answer:** 401 handling coordinates refresh and retries requests. **Technical explanation:** It uses the stored refresh token and clears session if refresh fails. **Repository evidence:** api.ts/appStore refresh action. **Likely follow-up:** Is refresh atomic server-side? **Common mistake:** Assuming no race edge cases. **Confidence:** VERIFIED

**96. Question:** What happens on page refresh? **Short answer:** App hydrates auth from localStorage. **Technical explanation:** It restores token/user/branch before protected pages render. **Repository evidence:** App.tsx. **Likely follow-up:** Is token checked immediately? **Common mistake:** Saying local data proves valid token. **Confidence:** VERIFIED

**97. Question:** How are forms validated? **Short answer:** UI checks exist, but server/service checks remain essential. **Technical explanation:** FluentValidation classes are not visibly registered in a pipeline. **Repository evidence:** page components, validators, DI. **Likely follow-up:** Can invalid client be blocked? **Common mistake:** Claiming browser validation secures API. **Confidence:** PARTIALLY VERIFIED

**98. Question:** How does UI update after write? **Short answer:** Store action processes response and may refresh data. **Technical explanation:** UI state change is separate from durable DB commit. **Repository evidence:** appStore actions. **Likely follow-up:** How prove persistence? **Common mistake:** Treating a toast as DB proof. **Confidence:** VERIFIED

**99. Question:** What are loading/error states? **Short answer:** Store tracks loading and errors; pages render them. **Technical explanation:** API failure is mapped into user-facing messages in actions. **Repository evidence:** appStore and pages. **Likely follow-up:** Are all errors consistent? **Common mistake:** Claiming every screen has same behavior. **Confidence:** PARTIALLY VERIFIED

**100. Question:** What is responsive design? **Short answer:** Layout adapts to screen sizes. **Technical explanation:** Tailwind classes and responsive table/layout components support it. **Repository evidence:** index.css, ResponsiveTable, report commits. **Likely follow-up:** Is accessibility audited? **Common mistake:** Equating responsive with accessible. **Confidence:** PARTIALLY VERIFIED

**101. Question:** Are there frontend tests? **Short answer:** None were found; package has no test script. **Technical explanation:** Backend tests do not cover React behavior. **Repository evidence:** package.json and source file scan. **Likely follow-up:** What add first? **Common mistake:** Claiming npm build is test coverage. **Confidence:** VERIFIED

**102. Question:** Did the frontend build? **Short answer:** Yes, Vite build succeeded with warnings. **Technical explanation:** Mixed static/dynamic settings import and large main chunk warnings remain. **Repository evidence:** `npm run build` 2026-10-10. **Likely follow-up:** What improve? **Common mistake:** Calling warnings failures or ignoring them. **Confidence:** VERIFIED

## I. Testing

**103. Question:** What test framework is used? **Short answer:** xUnit with Moq, FluentAssertions and Testcontainers. **Technical explanation:** EF InMemory and WebApplicationFactory also appear in test project. **Repository evidence:** test csproj. **Likely follow-up:** What database did fresh run use? **Common mistake:** Forgetting PostgreSQL containers. **Confidence:** VERIFIED

**104. Question:** How many tests passed? **Short answer:** 235 of 252 passed; 17 failed, zero skipped. **Technical explanation:** Fresh Release test run exited 1 on 2026-10-10. **Repository evidence:** TRX path in testing doc. **Likely follow-up:** Why failures? **Common mistake:** Repeating old 171-pass count. **Confidence:** VERIFIED

**105. Question:** Were tests run against production? **Short answer:** No, isolated Testcontainers DBs were used. **Technical explanation:** No production connection was configured/accessed for this run. **Repository evidence:** test command/env check/TRX. **Likely follow-up:** Is it production-equivalent? **Common mistake:** Calling it production proof. **Confidence:** VERIFIED

**106. Question:** What caused 17 failures? **Short answer:** Two test fixtures omit ICurrentUserService. **Technical explanation:** Service construction fails before test assertions execute. **Repository evidence:** failing test groups and constructors. **Likely follow-up:** Is feature broken? **Common mistake:** Claiming failures prove feature failure or pass. **Confidence:** VERIFIED

**107. Question:** What is a unit test? **Short answer:** Tests one unit in relative isolation. **Technical explanation:** Service tests use mocks to test calculations and edge cases. **Repository evidence:** SaleServiceTests, InventoryServiceTests. **Likely follow-up:** What can mocks miss? **Common mistake:** Claiming DB constraints are tested. **Confidence:** VERIFIED

**108. Question:** What is an integration test? **Short answer:** Exercises multiple parts together. **Technical explanation:** API/EF/PostgreSQL tests cover routes, migrations and persistence. **Repository evidence:** Integration folders and Testcontainers fixtures. **Likely follow-up:** Why Docker? **Common mistake:** Treating InMemory as PostgreSQL. **Confidence:** VERIFIED

**109. Question:** What does Testcontainers do? **Short answer:** Starts disposable real service containers for tests. **Technical explanation:** PostgreSQL integration tests can apply actual migrations. **Repository evidence:** Testcontainers packages/fixtures. **Likely follow-up:** Can it touch prod? **Common mistake:** Assuming any configured URL is safe. **Confidence:** VERIFIED

**110. Question:** What did persistence tests show? **Short answer:** Selected records survive a fresh context in isolated DB. **Technical explanation:** Tests cover context recycle for sale/product/purchase rows. **Repository evidence:** PersistenceTests passed. **Likely follow-up:** Does that prove production backup? **Common mistake:** Saying yes. **Confidence:** VERIFIED

**111. Question:** What is not tested? **Short answer:** Frontend automation absent; concurrency and production workflows unverified. **Technical explanation:** No frontend test runner, and live API was unavailable. **Repository evidence:** package scan/live probe. **Likely follow-up:** What test next? **Common mistake:** Claiming full coverage. **Confidence:** PARTIALLY VERIFIED

**112. Question:** What did the frontend build prove? **Short answer:** Source bundles successfully for production. **Technical explanation:** It does not prove runtime API behavior or UI correctness. **Repository evidence:** Vite output. **Likely follow-up:** Is it a test? **Common mistake:** Equating compile with acceptance test. **Confidence:** VERIFIED

**113. Question:** How would you fix failing fixtures? **Short answer:** Register a deterministic ICurrentUserService implementation/mock. **Technical explanation:** Match the new constructor dependency, rerun failing groups and full suite. **Repository evidence:** service constructor/fixture setup. **Likely follow-up:** Would code change be needed? **Common mistake:** Changing production DI to mask test setup. **Confidence:** INFERRED

**114. Question:** What is a regression test? **Short answer:** A test that prevents a fixed defect returning. **Technical explanation:** Add coverage for exact missing dependency setup/feature behavior after fixture repair. **Repository evidence:** current failing tests and new service dependency. **Likely follow-up:** What should it assert? **Common mistake:** Testing only that DI resolves. **Confidence:** VERIFIED

**115. Question:** Can you claim all tests passed? **Short answer:** No. **Technical explanation:** The recorded full run has 17 failures. **Repository evidence:** TRX counters. **Likely follow-up:** Why is report corrected? **Common mistake:** Relying on stale docs. **Confidence:** VERIFIED

## J. Deployment

**116. Question:** Where is frontend configured? **Short answer:** Vercel configuration defines the SPA rewrite and headers; live URL returned 200. **Technical explanation:** The Vite API base is built from VITE_API_BASE_URL and falls back to localhost if absent, so the deployed frontend may still lack a usable API target. **Repository evidence:** frontend/vercel.json, frontend/src/config.ts and dated curl. **Likely follow-up:** Is current content/API target verified? **Common mistake:** Equating HTTP 200 with full app operation. **Confidence:** PARTIALLY VERIFIED

**117. Question:** Where is backend configured? **Short answer:** Render Docker service in render.yaml. **Technical explanation:** Blueprint declares API, DB, disk and health route. **Repository evidence:** `render.yaml`. **Likely follow-up:** Was deployment active? **Common mistake:** Confusing config with running service. **Confidence:** DOCUMENTED ONLY

**118. Question:** Is backend currently running? **Short answer:** Last probe said no server. **Technical explanation:** Health URL returned 404 with `x-render-routing: no-server`. **Repository evidence:** dated curl output. **Likely follow-up:** Could it change later? **Common mistake:** Stating timeless status. **Confidence:** VERIFIED

**119. Question:** What does Docker do here? **Short answer:** Packages the API into a .NET runtime image. **Technical explanation:** Multi-stage Dockerfile restores/builds/publishes then copies runtime artifacts. **Repository evidence:** `backend/Dockerfile`. **Likely follow-up:** Does it include PostgreSQL? **Common mistake:** Saying database is in API image. **Confidence:** VERIFIED

**120. Question:** How is DB configured? **Short answer:** ConnectionStrings:DefaultConnection or DATABASE_URL, normalized for Npgsql. **Technical explanation:** Resolver configures PostgreSQL provider. **Repository evidence:** DbProviderResolver. **Likely follow-up:** Is SQLite supported? **Common mistake:** Saying yes based on a local file. **Confidence:** VERIFIED

**121. Question:** What is a health check? **Short answer:** Endpoint to report service health. **Technical explanation:** Source maps anonymous `/api/health`; Render config uses it. **Repository evidence:** HealthController/render.yaml. **Likely follow-up:** What did live route return? **Common mistake:** Saying it passed because code exists. **Confidence:** PARTIALLY VERIFIED

**122. Question:** How does deployment happen on a commit? **Short answer:** Automation is not verified in this repository. **Technical explanation:** Hosting configuration exists but no workflow/dashboard evidence establishes automatic deploy. **Repository evidence:** no workflow found; render.yaml/vercel config. **Likely follow-up:** Is Git connected? **Common mistake:** Claiming CI/CD without proof. **Confidence:** NOT VERIFIED

**123. Question:** Does the API persist data across restart? **Short answer:** Source configures PostgreSQL, but production persistence is unverified. **Technical explanation:** Separate DB service is declared; no live DB test/restore occurred. **Repository evidence:** resolver/render config; persistence tests. **Likely follow-up:** How verify safely? **Common mistake:** Treating disk as DB backup. **Confidence:** NOT VERIFIED

**124. Question:** Are backups working? **Short answer:** Not verified. **Technical explanation:** Backup/restore code and tools exist but no successful restore was performed. **Repository evidence:** settings endpoints and Dockerfile. **Likely follow-up:** What is proof? **Common mistake:** Confusing endpoint presence with tested recovery. **Confidence:** NOT VERIFIED

**125. Question:** What is wrong with CORS config? **Short answer:** Render key likely does not match bound section. **Technical explanation:** `AllowedOrigins` differs from `Cors:AllowedOrigins`. **Repository evidence:** render.yaml/Program.cs. **Likely follow-up:** Correct env key? **Common mistake:** Treating CORS as authentication. **Confidence:** VERIFIED

**126. Question:** What about ASPNETCORE_URLS? **Short answer:** Config uses a nonstandard form and should be checked. **Technical explanation:** Program also appends a URL from PORT; exact deployed precedence is unknown. **Repository evidence:** Dockerfile/render.yaml/Program.cs. **Likely follow-up:** Did it cause 404? **Common mistake:** Asserting causal link without logs. **Confidence:** PARTIALLY VERIFIED

**127. Question:** What is the deployment conclusion? **Short answer:** Frontend responds; complete system does not have verified API operation. **Technical explanation:** Vercel 200 and Render no-server 404 are separate observations. **Repository evidence:** dated probes. **Likely follow-up:** What to ask operator? **Common mistake:** Saying “deployed and working.” **Confidence:** VERIFIED

## K. Project challenges

**128. Question:** What is the main confirmed testing challenge? **Short answer:** Fixture DI setup is stale after service dependency changes. **Technical explanation:** Constructors need ICurrentUserService absent in two fixture registrations. **Repository evidence:** failures and test setup. **Likely follow-up:** How resolve? **Common mistake:** Calling it a business bug without assertion execution. **Confidence:** VERIFIED

**129. Question:** What does commit history prove? **Short answer:** Shared repository changes and their messages. **Technical explanation:** It does not identify personal authorship or deployment. **Repository evidence:** git log. **Likely follow-up:** How prove your contribution? **Common mistake:** Claiming others’ commits. **Confidence:** VERIFIED

**130. Question:** Was cosmetic category support tested? **Short answer:** Tests exist, but 9 current fixture cases fail before assertions. **Technical explanation:** There is other coverage, but these specific results are inconclusive. **Repository evidence:** TRX and CosmeticCategoryIntegrationTests. **Likely follow-up:** What next? **Common mistake:** Saying all category cases passed. **Confidence:** VERIFIED

**131. Question:** Was supplier mapping tested? **Short answer:** Tests exist, but 8 current fixture cases fail before assertions. **Technical explanation:** Constructor DI prevents those assertions from running. **Repository evidence:** InventorySupplierTests/TRX. **Likely follow-up:** Does code still map suppliers? **Common mistake:** Treating test failure as proof either way. **Confidence:** VERIFIED

**132. Question:** What issue appears in deployment configuration? **Short answer:** CORS environment key mismatch and API no-server response. **Technical explanation:** Source declares the setting but live route currently has no server. **Repository evidence:** render.yaml, Program, curl. **Likely follow-up:** What is root cause? **Common mistake:** Asserting cause without dashboard logs. **Confidence:** PARTIALLY VERIFIED

**133. Question:** What is a source-level security challenge? **Short answer:** Login response logging exposes token fields. **Technical explanation:** Browser console can disclose bearer tokens. **Repository evidence:** appStore login method. **Likely follow-up:** Mitigation? **Common mistake:** Calling browser console private. **Confidence:** VERIFIED

**134. Question:** What is a data-model tradeoff? **Short answer:** Product category/unit IDs are not FK-enforced. **Technical explanation:** App resolves built-in/custom values; DB can still contain invalid IDs. **Repository evidence:** configs and ER note. **Likely follow-up:** How redesign? **Common mistake:** Drawing unenforced FK relation as actual constraint. **Confidence:** VERIFIED

**135. Question:** How do you prioritize findings? **Short answer:** Fix exposure/availability/test correctness before feature expansion. **Technical explanation:** Address token logs/secret fallback, current API deployment and failing test setup first. **Repository evidence:** ranked findings. **Likely follow-up:** Which single first? **Common mistake:** Choosing cosmetic UI polish. **Confidence:** INFERRED

**136. Question:** How would you debug a failed sale? **Short answer:** Trace request, auth, batch query, transaction, DB and response. **Technical explanation:** Inspect correlation/logs and verify stock in a disposable DB. **Repository evidence:** SalesController/SaleService/UnitOfWork. **Likely follow-up:** What if response failed after commit? **Common mistake:** Retrying blindly. **Confidence:** VERIFIED

**137. Question:** Was a problem personally fixed by you? **Short answer:** Answer only from personal records. **Technical explanation:** Git repository is shared and cannot establish authorship. **Repository evidence:** personal commits/issues/mentor confirmation. **Likely follow-up:** Can you show code? **Common mistake:** Taking credit from commit message alone. **Confidence:** NOT VERIFIED

**138. Question:** What would you do differently? **Short answer:** Add automated validation, tests, safer token handling and deployment checks earlier. **Technical explanation:** These reduce boundary gaps and improve confidence. **Repository evidence:** current findings. **Likely follow-up:** What is tradeoff? **Common mistake:** Claiming this was original intent. **Confidence:** INFERRED

## L. Critical and difficult questions

**139. Question:** Why should we trust test numbers? **Short answer:** They come from the actual TRX counters and command. **Technical explanation:** I report the failing exit code and all failures, not a cherry-picked pass count. **Repository evidence:** `/tmp/.../project-defense.trx`. **Likely follow-up:** Can you reproduce? **Common mistake:** Hiding 17 failures. **Confidence:** VERIFIED

**140. Question:** Can two users oversell a batch? **Short answer:** It is a plausible race not prevented by a visible concurrency token. **Technical explanation:** Both requests may read the same balance before writing. **Repository evidence:** batch config/SaleService. **Likely follow-up:** What control add? **Common mistake:** Claiming transaction alone prevents all races. **Confidence:** INFERRED

**141. Question:** Can a sale partly commit? **Short answer:** Core sale rows are transactional; post-commit audit is separate. **Technical explanation:** Failure after commit can leave sale persisted while API appears failed. **Repository evidence:** SaleService order. **Likely follow-up:** How avoid duplicate retry? **Common mistake:** Saying whole workflow is atomic. **Confidence:** VERIFIED

**142. Question:** What if database is unavailable? **Short answer:** Startup migration/seed can fail startup; requests also depend on DB. **Technical explanation:** Active-user checks and persistence queries require DB availability. **Repository evidence:** Program startup; OnTokenValidated. **Likely follow-up:** Is there offline mode? **Common mistake:** Claiming local fallback. **Confidence:** VERIFIED

**143. Question:** Can pharmacist call Admin API directly? **Short answer:** API role attributes should reject it with 403. **Technical explanation:** Authorization is server-side, independent of client route. **Repository evidence:** controller attributes. **Likely follow-up:** Is every endpoint protected? **Common mistake:** Assuming all controllers have same roles. **Confidence:** VERIFIED

**144. Question:** What if UI stock is stale? **Short answer:** Server must recheck available stock during sale. **Technical explanation:** SaleService queries batches before deduction; UI quantity is not authoritative. **Repository evidence:** SaleService. **Likely follow-up:** What about concurrent race? **Common mistake:** Trusting displayed count. **Confidence:** VERIFIED

**145. Question:** How would you move to another database? **Short answer:** Migrate EF provider/config/schema and retest provider-specific behavior. **Technical explanation:** Current resolver is explicitly Npgsql and raw SQL uses PostgreSQL advisory locks. **Repository evidence:** DbProviderResolver/repositories. **Likely follow-up:** Why not just change URL? **Common mistake:** Treating providers as interchangeable. **Confidence:** VERIFIED

**146. Question:** How would you scale it? **Short answer:** First profile and fix queries, concurrency and operational health. **Technical explanation:** Report materialization and background jobs may become costly; multiple replicas need coordinated jobs. **Repository evidence:** ReportService/hosted jobs. **Likely follow-up:** Would microservices solve it? **Common mistake:** Scaling by architecture label alone. **Confidence:** INFERRED

**147. Question:** What if report totals differ by branch? **Short answer:** Verify API filters and query scope; branch filtering is inconsistent. **Technical explanation:** Some dashboard medicine metrics are not branch-filtered. **Repository evidence:** ReportService.GetDashboardSummaryAsync. **Likely follow-up:** How regression test? **Common mistake:** Assuming the chart is right because it rendered. **Confidence:** VERIFIED

**148. Question:** What if sale uses expired stock? **Short answer:** Current batch query needs explicit expired-date filtering review. **Technical explanation:** Daily expiry job leaves a time window; ordering alone is not filtering. **Repository evidence:** SaleService/ExpiryCheckBackgroundService. **Likely follow-up:** What test add? **Common mistake:** Saying FEFO guarantees safety. **Confidence:** VERIFIED

**149. Question:** Is the current production system safe to use? **Short answer:** This review cannot establish production readiness. **Technical explanation:** Live API no-server result, token logging and other gaps require attention. **Repository evidence:** live probe/security review. **Likely follow-up:** What approval is needed? **Common mistake:** Giving assurance beyond evidence. **Confidence:** NOT VERIFIED

**150. Question:** What should you say if you do not know? **Short answer:** State what evidence exists and what remains unverified. **Technical explanation:** Offer a concrete check without inventing a cause. **Repository evidence:** This knowledge base’s status labels. **Likely follow-up:** Who can confirm? **Common mistake:** Guessing to sound confident. **Confidence:** VERIFIED

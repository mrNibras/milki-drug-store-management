# Backend deep dive

## Startup and dependency injection

`backend/src/MilkiDrugStore.Api/Program.cs` is the composition root. At process startup it creates and checks a writable data directory and a Data Protection key directory, binds strongly typed options, resolves the PostgreSQL connection string, registers `AppDbContext`, configures AutoMapper, calls `AddApplicationServices`, registers current-user access, configures JWT bearer authentication and role policies, sets CORS/JSON/Swagger, adds hosted jobs and the global exception filter.

After building the app, a service scope applies relational EF migrations, seeds roles/admin data, runs catalog migration/backfill logic, and then builds middleware/routes. A database connection or migration error can abort startup. Running schema-changing migrations automatically at every instance startup deserves a controlled deployment plan, especially with multiple replicas.

`ServiceCollectionExtensions` registers services and repository implementations as scoped. Scoped means one instance per HTTP request/scope; the DbContext and UnitOfWork share that scope. It registers MediatR handlers by scanning the Application assembly. There is no validation pipeline behavior or `AddValidatorsFromAssembly` registration found, despite validator classes in Application.

## Request path

Typical path: browser JSON request → ASP.NET routing → authentication middleware verifies JWT → authorization checks role → controller binds DTO → direct service call or MediatR request → application service/handler applies rules → repositories and EF Core query/update the shared DbContext → `SaveChangesAsync` and optionally transaction commit → DTO response serialized as camelCase JSON → Axios/store updates page state.

Controllers are not uniform: some send queries/commands through MediatR, while others call services directly. Thus the project uses MediatR selectively; it is not a fully enforced CQRS system. `Application` has a direct reference to `Persistence`, so do not call the layering strict Clean Architecture.

## Core concepts using repository examples

- **Dependency injection:** Program registers `ISaleService`/`SaleService`; SalesController receives dependencies in its constructor. This avoids constructing concrete services manually and allows test doubles.
- **Middleware:** `UseAuthentication` establishes principal, `UseAuthorization` checks policies, CORS handles browser-origin rules, and `SecurityHeadersMiddleware` adds response headers. Middleware order is important: routing/CORS/auth/authz/endpoints must be in the intended sequence.
- **REST/HTTP:** controllers use resource routes and verbs (GET reads, POST creates or triggers, PUT updates); response codes communicate success, unauthorized, forbidden, client error, or server error.
- **DTO/entity:** API DTOs describe request/response shapes. Domain entities are EF-persisted objects. Keeping these separate avoids exposing every database field directly, although a response mapper still needs careful sensitive-field filtering.
- **EF tracking / LINQ / Include:** DbContext tracks modified entity instances; `SaveChangesAsync` writes tracked changes. LINQ composes query filters. `Include` eagerly loads selected navigations, for example product batches and suppliers. Repository methods often return `IQueryable`, so materialization happens later.
- **Async/await:** service calls await database/network operations without blocking a request thread. `LocalFileStorageService` contains a method marked async without await (compiler warning); asynchronous syntax only helps when an operation is actually awaited.
- **Transactions:** `UnitOfWork` holds one DbContext and transaction. Sale/Purchase use `BeginTransactionAsync`, save, then commit; exceptions call rollback. That gives atomicity for operations inside the database transaction. Audit logging is done after the sale/purchase commit, so audit failure can make the request appear failed despite persisted business rows.
- **Validation:** There are FluentValidation classes, but no central registration/pipeline was found. Backend services do some manual validation. Do not say every DTO is automatically validated.
- **Exceptions:** uncaught errors reach `GlobalExceptionFilter`; it logs and returns a correlation ID. Some controller methods catch exceptions and return a 400 with exception text, which bypasses the filter's safe generic 500 behavior.

## Major service responsibilities

| Service | Main behavior | Inputs/outputs and failure cases |
|---|---|---|
| `AuthService` | Login, register, approve, password change, refresh, reset and settings | DTOs in; login/token/user response out. Rejects inactive/unapproved users; email errors are swallowed in some flows |
| `MedicineService` / `CosmeticService` | Catalog CRUD, category/unit resolution, batch intake, response mapping | Request DTO in; response DTO out. User permissions influence cost redaction; category/type IDs are app-managed |
| `PurchaseService` | Purchase header, line items, medicine/cosmetic batches and inventory transactions | Validates key amounts/quantities and wraps writes in transaction; supplied payment status can override derived status |
| `SaleService` | Batch availability, stock deduction, discounts, payment state, sale records and inventory transactions | Rejects missing stock; caps pharmacist discount; out-of-stock rolls back. Payment amount bounds need additional validation |
| `InventoryService` | Current balance, damage/loss, expiry write-off and history queries | Requires a positive quantity and one batch reference for damage; persists stock and record together |
| `ReportService` | Dashboard/report calculations and business periods | Reads stored sales/products and aggregates; branch filters and materialization scope need review |
| `RetentionService` | Deletes old audit logs and old read notifications according to retention configuration | Invoked by daily hosted job; it does not copy those rows into the archive tables in the inspected code |

## EF Core and persistence details

`AppDbContext` exposes 24 DbSets and applies configuration classes from the Persistence assembly. `DbProviderResolver` accepts `ConnectionStrings:DefaultConnection` or `DATABASE_URL`, converts URL-style PostgreSQL strings, and configures Npgsql. No SQLite provider is selected by this resolver. `UnitOfWork` wraps the context and generic/specialized repositories. PostgreSQL migrations are in `MilkiDrugStore.Persistence/Migrations`.

A transaction is a database boundary, not a promise that all external side effects roll back. Email, file storage, logging and post-commit audit calls are separate concerns. The app accumulates some sale domain-event objects, but no event publisher/dispatcher was found in the checked path; do not claim they are delivered.

## Error/status behavior

`GlobalExceptionFilter` maps `ArgumentException`/`InvalidOperationException` to 400, most other unhandled exceptions to 500, logs server-side details, and returns a correlation ID. Controllers can locally catch and map broader failures to 400, making status semantics less consistent. A connection failure during startup is logged and rethrown. Health endpoint is `/api/health`; source route does not establish the remote server is currently running.

## How to defend this design

“The backend is a .NET 8 API with a clear separation of HTTP controllers, application services, persistence, and infrastructure. The separation helps keep the sale and purchase rules outside page code. EF Core and a shared scoped DbContext handle relational persistence, while explicit transactions protect multi-row inventory workflows. The implementation uses MediatR for some request paths, but the application layer still references Persistence and several controllers call services directly, so I describe it as layered modular monolith rather than strict Clean Architecture or full CQRS.”

## Evidence paths

`backend/src/MilkiDrugStore.Api/Program.cs`, `Extensions/ServiceCollectionExtensions.cs`, `Middleware/GlobalExceptionFilter.cs`, `Controllers/`, `backend/src/MilkiDrugStore.Application/Services/`, `Application/Validators/Validators.cs`, `backend/src/MilkiDrugStore.Persistence/Context/AppDbContext.cs`, `Context/DbProviderResolver.cs`, `Repositories/UnitOfWork.cs`.

## Representative source-code walkthrough (with line references)

The excerpts are short navigation aids. Open the linked source for surrounding context and current line numbers. In code blocks, an ellipsis explicitly means source lines are omitted.

### 1. Startup composition and request pipeline

Source: [Program.cs](../../backend/src/MilkiDrugStore.Api/Program.cs#L121), lines 121–130 and 406–410.

~~~csharp
builder.Services.AddDbContext<AppDbContext>(options =>
{
    DbProviderResolver.ConfigureAppDbContext(options, builder.Configuration);
});
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddApplicationServices();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<MilkiDrugStore.Application.Interfaces.ICurrentUserService, MilkiDrugStore.Api.Services.CurrentUserService>();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
~~~

The first block composes PostgreSQL persistence and application services; the second allows browser origins, establishes identity, authorizes requests, and maps endpoints. Removing an application registration can make startup/request DI fail. Reordering authorization before authentication can prevent the principal from being available. Debug by checking the exception/DI graph and middleware order, then make a request with a known role.

**Say aloud:** “Program is the composition root: it wires the database, business services and security pipeline before controllers receive requests.”

### 2. Login and token issuance

Source: [AuthService.cs](../../backend/src/MilkiDrugStore.Application/Services/AuthService.cs#L54), lines 54–82 and 93–107.

~~~csharp
var normalizedEmail = NormalizeEmail(request.Email);
var users = await _userRepo.FindAsync(u => u.Email == normalizedEmail);
var user = users.FirstOrDefault();

if (user == null)
{
    _logger.LogWarning("Login failed: user not found for email {Email}", normalizedEmail);
    return null;
}
if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
{
    return null;
}
if (!user.IsActive)
{
    return null;
}
if (!user.IsApproved)
{
    return null;
}

var token = _jwtTokenService.GenerateToken(user);
var refreshToken = new RefreshToken
{
    Token = Guid.NewGuid().ToString(),
    UserId = user.UserId,
    ExpiryDate = DateTime.UtcNow.AddDays(7)
};
await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
await _unitOfWork.SaveChangesAsync();
~~~

Normalization supports consistent email lookup. BCrypt compares a password with its stored hash. The active/approved checks gate token generation. Removing either check could permit a disabled or unapproved account. The refresh token is persisted, but its current raw storage is a security limitation. Debug with a synthetic account and inspect status/role without printing the returned token.

**Say aloud:** “Authentication verifies the password and account state first, then issues a short-lived access token and a persisted refresh token.”

### 3. Medicine and cosmetic creation

Sources: [MedicineService.cs](../../backend/src/MilkiDrugStore.Application/Services/MedicineService.cs#L88), lines 88–115; [CosmeticService.cs](../../backend/src/MilkiDrugStore.Application/Services/CosmeticService.cs#L86), lines 86–110.

~~~csharp
var categoryId = await ResolveCategoryIdAsync(request.CategoryId, request.NewCategoryName, userId);
var unitTypeId = await ResolveUnitTypeIdAsync(request.UnitTypeId, request.NewUnitTypeName, userId);
var medicine = new Medicine
{
    ProductCode = await ResolveProductCodeAsync(request.ProductCode),
    BrandName = request.BrandName,
    GenericName = request.GenericName,
    CategoryId = categoryId,
    UnitTypeId = unitTypeId,
    PurchasePrice = request.PurchasePrice,
    SellingPrice = request.SellingPrice,
    IsActive = true,
    CreatedDate = DateTime.UtcNow
};
await _medicineRepo.AddAsync(medicine);
await _unitOfWork.SaveChangesAsync();
~~~

Both services resolve category/unit IDs before creating records, then save and audit. Cosmetic creation additionally assigns branch and optional supplier. Removing resolution can leave product display IDs unresolved; removing SaveChanges makes the returned state non-durable. Because those catalog IDs are not FKs, test custom/built-in resolution and inspect the mapped response. Audit is a later call, so it can fail after the product was saved.

**Say aloud:** “The service translates the form DTO into the correct entity, resolves catalog values, saves it, then records an audit entry.”

### 4. Purchase creation

Source: [PurchaseService.cs](../../backend/src/MilkiDrugStore.Application/Services/PurchaseService.cs#L57), validation lines 57–73; transaction and writes lines 90–190.

~~~csharp
if (request.Items.Count == 0)
    throw new InvalidOperationException("At least one purchase item is required.");
foreach (var item in request.Items)
{
    if (item.Quantity <= 0)
        throw new InvalidOperationException("Quantity must be positive for all items.");
    if (item.PurchasePrice <= 0)
        throw new InvalidOperationException("Purchase price must be positive for all items.");
}
await _unitOfWork.BeginTransactionAsync();
...
await _unitOfWork.SaveChangesAsync();
await _unitOfWork.CommitTransactionAsync();
await _auditLog.LogAsync(createdBy, "Created Purchase", "Purchases", purchase.PurchaseId);
~~~

The omitted branch processes medicine and cosmetic items separately. Removing the transaction can leave a partial header or stock ledger after an error. The audit occurs after commit, so the database write can succeed even when the API reports an error from audit. Debug a failed request by checking whether it reached commit before retrying; use an isolated DB and verify header, lines, batches, and inventory movements together.

**Say aloud:** “Purchase validation rejects empty or nonpositive lines, then one database transaction records the purchase and the stock batches it creates.”

### 5. Sale endpoint, mediator and service

Sources: [SalesController.cs](../../backend/src/MilkiDrugStore.Api/Controllers/SalesController.cs#L40), lines 40–57; [CreateSaleCommandHandler.cs](../../backend/src/MilkiDrugStore.Application/CommandHandlers/Sales/CreateSaleCommandHandler.cs#L8), lines 8–20; [SaleService.cs](../../backend/src/MilkiDrugStore.Application/Services/SaleService.cs#L57), lines 57–59, 97–104, 201–244, 317–341.

~~~csharp
var result = await _mediator.Send(new Application.Commands.Sales.CreateSaleCommand(
    request, userId, userRole, branchId > 0 ? branchId : null));
return Ok(result);
~~~

~~~csharp
await _unitOfWork.BeginTransactionAsync();
var maxDiscountPerUnit = isAdmin
    ? batch.SellingPrice
    : batch.SellingPrice * MaxPharmacistDiscountRate;
var effectiveUnitPrice = batch.SellingPrice - discountPerUnit;
~~~

The controller gets identity from claims, creates a command, and sends it to a handler; the handler delegates to SaleService. The service starts a transaction, chooses stock, computes line/total/payment values and persists updates. FEFO helper orders by expiry but the current query does not filter dates already in the past. Removing the transaction/rollback can desynchronize sale and stock. Debug with one synthetic batch, trace the selected BatchId, and compare all changed rows after commit.

**Important edge:** the controller catches all exceptions and returns HTTP 400 with the exception message. That can expose error text or misclassify server faults. A post-commit audit failure is also possible. **Say aloud:** “The POS request carries no authority by itself; the controller adds the authenticated user, role and branch before the sale handler runs.”

### 6. Damage and expiry processing

Source: [InventoryService.cs](../../backend/src/MilkiDrugStore.Application/Services/InventoryService.cs#L48), lines 48–69 and 90–117.

~~~csharp
if (batchId.HasValue == cosmeticBatchId.HasValue)
    throw new ArgumentException("Provide either a medicine batch or a cosmetic batch, not both.");
if (quantity <= 0)
    throw new ArgumentException("Damage quantity must be greater than zero.");
if (string.IsNullOrWhiteSpace(reason))
{
    throw new ArgumentException("A reason is required for damage.");
}
...
batch.QuantityDamaged += quantity;
await _unitOfWork.DamageRecords.AddAsync(damageRecord);
await _transactionRepo.AddAsync(inventoryTransaction);
await _unitOfWork.SaveChangesAsync();
~~~

The equality check rejects both IDs present and neither present. A single SaveChanges persists the tracked batch update and inserted records in EF's save transaction. Removing the availability check risks negative stock. Audit logging comes after SaveChanges. For expiry, inspect ProcessExpiredInventoryAsync lines 172–273 and the daily ExpiryCheckBackgroundService; test reruns and past/future boundaries with fixed data.

**Say aloud:** “A write-off must identify exactly one batch and cannot exceed its remaining stock; the database save records the balance change and trace history together.”

### 7. Reporting scope and calculation

Source: [ReportService.cs](../../backend/src/MilkiDrugStore.Application/Services/ReportService.cs#L168), lines 168–208; timezone helper lines 22–35. Business clock: [BusinessClock.cs](../../backend/src/MilkiDrugStore.Domain/Common/BusinessClock.cs).

~~~csharp
var medicines = (await _medicineRepo.GetAllAsync()).ToList();
var sales = (await _saleRepo.GetAllAsync()).ToList();
var cosmetics = (await _cosmeticRepo.GetAllAsync()).ToList();
if (branchId.HasValue)
{
    sales = sales.Where(s => s.BranchId == branchId.Value).ToList();
    cosmetics = cosmetics.Where(c => c.BranchId == branchId.Value).ToList();
}
var inventoryValue = medicines.Sum(m =>
    m.Batches.Sum(b => b.RemainingQuantity * b.PurchasePrice));
~~~

This makes some aggregation in application memory. The branch filter applies to sales/cosmetics in this excerpt but not the medicine list; medicine inventory value and some low-stock metrics therefore need branch-scope review. The cosmetic low-stock expression also calls Min on branch-filtered batches without an empty-set guard, so products with no batches could throw. Removing the timezone conversion risks grouping near-midnight sales into the wrong local business day. Debug with two branches and fixed UTC timestamps, then compare API output to the expected Ethiopian local period.

**Say aloud:** “The report uses local business-time boundaries, but I found a branch-filter inconsistency that needs correction before trusting every inventory metric.”

### 8. Exception mapping

Source: [GlobalExceptionFilter.cs](../../backend/src/MilkiDrugStore.Api/Middleware/GlobalExceptionFilter.cs#L21), lines 21–69.

~~~csharp
var statusCode = isClientError
    ? StatusCodes.Status400BadRequest
    : StatusCodes.Status500InternalServerError;
_logger.LogError(context.Exception,
    "Unhandled exception (CorrelationId: {CorrelationId}, Status: {StatusCode})",
    correlationId, statusCode);
var response = ApiResponse.Fail(message, statusCode);
response.CorrelationId = correlationId;
~~~

The filter logs full server-side diagnostics while hiding unexpected production details from the client. Removing the correlation ID makes support harder; changing all errors to 400 hides server faults. It only handles exceptions that reach MVC; controller-local catches can bypass it. Debug with server correlation ID and logs, never by exposing secrets in the response.

**Say aloud:** “Unexpected errors are logged on the server and returned with a correlation ID, while client-facing errors should use intentional status codes.”


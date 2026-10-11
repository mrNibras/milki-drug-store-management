# Project overview

## Identity and purpose

**Milki Drug Store Management System (MDSMS)** is a browser-based pharmacy operations application. Its intended purpose is to centralize product catalogs, batch-level inventory, purchasing, point-of-sale transactions, damage/expiry records, user access, notifications, and reports. The repository confirms these are represented in the UI/API and backend code; it does not prove that the system is currently usable end to end in production.

**Problem addressed (scope statement, not a measured outcome):** a drug store needs to relate what it buys and sells to stock batches, branches, suppliers, expiry dates, prices, and staff actions. MDSMS models those workflows in one application. The repository contains no measured before/after study, user survey, or verified business outcome, so say “designed to support” rather than claiming quantified efficiency gains.

## Users and boundaries

- **Administrator:** manages users, catalog records, branches, suppliers, purchases, settings, and reports where routes require Admin.
- **Pharmacist/staff user:** can sign in, search/read product and stock data, process sales, record damage, and view notifications within API authorization rules.
- **External systems:** PostgreSQL is configured as persistence; an S3 provider is available by configuration. Actual production credentials, bucket access, and active connections are not visible from this repository.

The system is not a payment gateway, accounting ledger, e-prescription platform, customer/patient record system, or verified regulatory dispensing system. No customer identity/prescription workflow or external payment processor was found.

## Main modules

1. Authentication and account administration.
2. Medicine and cosmetics catalog.
3. Categories and unit types.
4. Supplier and branch management.
5. Purchase entry with items and stock batches.
6. Point of sale with batch depletion, payment fields, and discounts.
7. Inventory balances, damage/loss, and expiry write-off.
8. Notifications, audit history, settings, backup/restore services.
9. Dashboard and sales, inventory, supplier, and staff reports.

## Pharmacy vocabulary as implemented

| Concept | Meaning in this repository | Why it matters |
|---|---|---|
| Medicine / cosmetic | Separate catalog entities, with their own batch models | Distinguishes products that follow different field and expiry expectations |
| Category / unit type | Catalog identifiers resolved by application services; built-in catalog values can be represented by negative IDs | Supports UI grouping and unit labels; these identifiers are not relational FKs on the product tables |
| Supplier | Vendor linked to purchases and optionally to batches | Preserves origin/cost context for replenishment |
| Purchase / purchase item | Header plus product-specific line records | Captures quantity and purchase costs and creates/increments batches |
| Batch | A dated, branch-specific lot with received, issued, damaged, expired quantities | Makes stock and expiry decisions traceable by lot |
| Balance | Medicine batch: received − issued − damaged − expired; cosmetic batch exposes a corresponding balance | Used for availability, reporting, sale, and damage checks |
| Sale / sale item | Sale header plus one or more medicine or cosmetic line records | Persists what was sold, price, discount, batch, payment, and profit fields |
| Payment fields | Payment method/status, amount paid, amount due | Tracks partial/unpaid/paid states; this is not an external payment settlement |
| Damage / expiry record | A stock write-off record tied to branch and product/batch references | Makes non-sale stock reductions auditable |
| User / role | User belongs to role and branch; backend checks claims and current active status | Separates sign-in from permission decisions |

## Functional scope and caveats

Implemented code includes APIs and screens for the features above. Some behavior is incomplete or carries risks: validators exist but appear not to be registered; 17 current tests fail before their assertions because fixture DI lacks `ICurrentUserService`; the live Render host returned no-server during the dated check; frontend logs include authentication response data; report branch scoping has inconsistencies. See [findings](18-findings-and-verification-status.md).

**Not established:** number of real users, store adoption, regulatory approval, uptime, backup success, production DB contents, or customer satisfaction.

## Presentation introductions

Replace bracketed text only with facts you can personally confirm.

### 30 seconds

“Milki Drug Store Management System is a web application designed to help a pharmacy manage medicine and cosmetics, supplier purchases, batch inventory, sales, damage and expiry records, and reports. The browser frontend is built with React and TypeScript, and it communicates with an ASP.NET Core API backed by PostgreSQL through Entity Framework Core. The repository also includes role-based access and automated backend tests. My contribution was [state your verified work]. I’ll show the architecture and one workflow, then explain the implementation and the limitations I verified.”

### 1 minute

“Pharmacy stock is not just a total count: the store needs to know which supplier and batch it came from, which branch owns it, what it cost, and whether it is near expiry. MDSMS models those operations for medicine and cosmetics. Staff can search stock and process sales; administrators have additional catalog, purchasing, user, supplier, branch, and reporting functions. The frontend is React, TypeScript, Vite, Zustand, and Axios. The backend is a .NET 8 ASP.NET Core API using services, repositories, EF Core, and PostgreSQL. It is a layered modular monolith, with MediatR used selectively. I verified a frontend production build and ran the backend suite: 235 of 252 tests passed and 17 failed due to missing test-fixture service registration. The configured frontend host answered, but the API health URL returned a no-server 404 on 10 October 2026, so I cannot claim a working live deployment. My contribution was [verified specifics].”

### 3 minutes

“Milki Drug Store Management System addresses pharmacy workflows where product identity, branch, batch, supplier, price, and expiry all affect a stock decision. The system has separate medicine and cosmetic catalog and batch entities, purchase and sale headers with line items, and records for inventory transactions, damage, and expired stock. It also includes users and roles, reports, settings, notifications, and audit history.

“The browser client uses React and TypeScript with Vite. React Router maps pages, Zustand stores application state, and Axios sends JSON requests to the API. The client stores access and refresh tokens in localStorage, and its route guards improve navigation; actual access control is enforced by ASP.NET authorization attributes and policies. The server is an ASP.NET Core API. Controllers receive HTTP requests, selected reads/writes pass through MediatR, and application services implement business logic. EF Core maps entities through AppDbContext and configuration classes to PostgreSQL. Transactions wrap sale and purchase persistence. The solution is layered but not strict Clean Architecture because Application directly references Persistence. MediatR is used, but not every endpoint follows CQRS.

“One workflow is a sale: the POS collects line items and payment details, the client posts them to SalesController, authorization supplies the user role and branch, SaleService chooses available batches and calculates amounts, and EF Core saves the sale, line items, inventory transactions, and stock changes within a transaction. The API returns a response and the client updates or reloads UI state. A test name confirms FEFO ordering is covered; however, the sale batch query should be reviewed carefully because expired lots may not be excluded until the background write-off executes.

“I verified the source structure, live host responses, frontend build, and backend tests. The test run used isolated PostgreSQL Testcontainers and produced 252 tests: 235 passed and 17 failed before exercising the assertions because two fixtures do not register ICurrentUserService. The Vercel URL returned HTTP 200, while the Render API health URL returned HTTP 404 with no server. So the repository demonstrates a substantial implementation, but live end-to-end operation is not proven. My contribution was [list only work supported by your own records].”

## Terms to explain carefully

- “Supports batch-level inventory” is true of the data model and services; it is not a claim that every edge case is safe.
- “Payment tracking” means payment fields and amount calculations, not card processing or bank settlement.
- “Deployment configured” is supported by source files; “currently deployed and working” is not.
- “Tested” must include the actual count and failures, not the older stale 171-pass statement.

# Business rules catalogue

Statuses describe checked-out implementation. “Test evidence” is the fresh local .NET test run from 10 October 2026, unless noted. A passing test proves only the covered case.

| ID | Rule | Why | Implementation | Validation layer | Test/evidence | Status |
|---|---|---|---|---|---|---|
| CAT-01 | Medicine/cosmetic categories and unit types resolve from built-in or custom catalogs | Consistent product grouping | MedicineService/CosmeticService category and unit resolution; CatalogService | Service mapping; not FK-backed | CosmeticCategory integration tests; 17 fixture failures block two fixture groups | PARTIALLY VERIFIED |
| CAT-02 | Medicine product code and configured barcode uniqueness | Product identification | MedicineConfiguration and medicine service | DB unique index + service code generation | Config/test suite | VERIFIED in model |
| CAT-03 | Medicine delete deactivates rather than physically removes | Preserve historical lines | MedicineService.DeleteAsync | Service | Source path | VERIFIED in code |
| CAT-04 | Cosmetic expiry can be optional on purchase/batch flow | Some cosmetic stock has no expiry | PurchaseService/CosmeticBatch | Service and nullable field | `CosmeticPurchase_Should_Not_Require_Expiry_Date` passed | VERIFIED for test case |
| PUR-01 | Purchase must have item lines; quantity and purchase price must be positive | Prevent invalid stock intake | PurchaseService.CreateAsync | Service; FluentValidation not wired | DB integration tests | VERIFIED in code |
| PUR-02 | Medicine purchase requires batch number and future expiry; cosmetic expiry may be null | Medicine lot traceability; cosmetic flexibility | PurchaseService | Service | DB integration case for nullable cosmetic expiry passed | VERIFIED in code/tested case |
| PUR-03 | Purchase total is quantity × purchase price across items | Correct payable amount | PurchaseService | Service calculation | Payment tests in suite | VERIFIED |
| PUR-04 | Amount paid must be between 0 and total | Avoid invalid payable balance | PurchaseService.CreateAsync | Service | Test suite includes payment cases | VERIFIED |
| PUR-05 | Purchase creates purchase lines, batches and inventory transaction in one transaction | Keep stock ledger aligned | PurchaseService + UnitOfWork | DB transaction | PostgreSQL purchase/persistence tests | VERIFIED in code/tested cases |
| PUR-06 | Request-supplied payment status can override derived status | Flexible/manual status in current code | PurchaseService | No matching strict service validation found | `Validators.cs` does not appear registered | RISK: inconsistent status possible |
| SAL-01 | Sale selects available batch stock, optionally using manual batch mode; automatic ordering is expiry-first | Reduce stock and support FEFO | SaleService.GetAvailableBatchesAsync / settings | Service; DB transaction | `FEFO_Should_Prioritize_Earliest_Expiry` passed | VERIFIED for ordering test |
| SAL-02 | Stock balance derives from received minus issued, damaged and expired | Keep available quantity correct | batch models/services | Service/model | Inventory tests passed | VERIFIED |
| SAL-03 | If requested units cannot be fulfilled, sale throws and transaction rolls back | Avoid partial sale | SaleService.CreateAsync | Service + transaction | Relevant unit tests | VERIFIED in code; inspect exact edge test before claiming |
| SAL-04 | Negative discount is clamped to zero; discount requires reason when resulting discount > 0 | Explain sale adjustment | SaleService | Service | Sale discount tests passed | VERIFIED for tested cases |
| SAL-05 | Pharmacist discount cap is 5% of batch selling price/unit; Admin cap allows price down to zero | Role-based pricing | SaleService | Role passed from authenticated user | `CreateAsync_WithDiscount_CapsDiscountForPharmacist`, admin test passed | VERIFIED |
| SAL-06 | Sale item stores base unit price, discount, purchase price, subtotal and profit | Preserve transaction economics | SaleService/SaleItem | Service calculation | Sale tests/integration | VERIFIED in code |
| SAL-07 | Payment status is derived as unpaid, partial, paid by amount paid vs total | Payment tracking | SaleService | Service | payment status tests passed | VERIFIED; negative/overpayment bound not fully enforced |
| SAL-08 | Sale writes sale, lines, stock updates and inventory movement in a DB transaction | Atomic inventory sale | SaleService/UnitOfWork | EF transaction | PostgreSQL tests | VERIFIED in code |
| SAL-09 | Sale audit is written after DB commit | Audit activity | SaleService/AuditLogService | Separate operation | Source order | RISK: audit failure after successful commit can surface as failed request |
| SAL-10 | FEFO query should avoid expired stock | Prevent dispensing expired items | SaleService available-batch helper | Query currently appears to order by expiry and balance but not filter already-expired dates | Expiry job runs daily; code review | RISK: expired batch could be sold before sweep |
| INV-01 | Damage must reference exactly one of medicine or cosmetic batch, quantity > 0, and a reason | Keep write-off meaningful | InventoryService.RecordDamageAsync | Service | invalid/both/none/over-stock tests passed | VERIFIED |
| INV-02 | Damage is limited to available batch balance and updates batch plus records | Prevent negative stock and retain history | InventoryService | Transaction/SaveChanges | medicine/cosmetic damage tests passed | VERIFIED |
| INV-03 | Expired remaining balance is written off and recorded; repeated sweep should not duplicate | Keep expired stock out of balance | InventoryService.ProcessExpiredInventoryAsync | Batch counters and DB writes | idempotency/re-run test passed | VERIFIED |
| INV-04 | Expiry job runs daily; low-stock notification check runs every 30 minutes; retention runs daily | Automated maintenance | Infrastructure BackgroundJobs | Hosted services | Code registration/cadence | VERIFIED in code; live scheduling not verified |
| AUTH-01 | Login normalizes email, verifies BCrypt, and rejects inactive/unapproved users | Safe account access | AuthService.LoginAsync | Service + JWT validation | Auth tests passed | VERIFIED |
| AUTH-02 | Registration creates an unapproved pharmacist; admin approval required | Controlled onboarding | AuthService/Register and controller | Service/role endpoint | Source | VERIFIED |
| AUTH-03 | Refresh token is rotated/revoked; access token activity checks current user active state | Revoke sessions promptly | AuthService + JwtBearer OnTokenValidated | API middleware | deleted-user/token tests passed | VERIFIED for tested path |
| AUTH-04 | Password reset token expires in one hour and is single use | Bound reset validity | AuthService | Service | Source; token storage is raw | VERIFIED in code; storage risk |
| AUTH-05 | Pharmacist API responses should hide purchase cost values | Restrict financial data | Medicine/Cosmetic response mapping and current-user service | Server response mapping | `InventoryCostRedactionTests` and commit `6f8153f8` | VERIFIED for tested response |
| REP-01 | Business clock uses Africa/Addis_Ababa; weeks start Monday and yearly period follows Sep 1–Aug 31 | Match local pharmacy period | BusinessClock/ReportService | Calculation | report period tests | VERIFIED in code/tested cases |
| REP-02 | Dashboard branch scoping should consistently filter inventory metrics | Branch isolation | ReportService.GetDashboardSummaryAsync | Service queries | Source review finds medicine metrics not fully branch-filtered | RISK |
| REP-03 | Report service currently materializes broad collections before some aggregation | Simplicity; scale tradeoff | ReportService | Query structure | Source review | RISK: memory/latency growth |
| REP-04 | Cosmetic dashboard low-stock calculation calls Min over branch-filtered batches without an empty-set guard | Avoid error on products with no batches | ReportService.GetDashboardSummaryAsync | LINQ calculation | Source line 205; no test for empty branch batches found | RISK: report can throw when the filtered set is empty |

## Important answer caveat

A validator class proves the rule is described in code, not that the API executes it. Search found FluentValidation validators but no assembly registration or MediatR validation behavior. In a defense, say “the service validates these fields” only where the specific service actually checks them; do not claim automatic validation for all requests.

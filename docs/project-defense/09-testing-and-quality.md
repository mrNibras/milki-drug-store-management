# Testing and quality

## Approach and tools found

The backend test project targets .NET 8 and references xUnit, Moq, FluentAssertions, EF Core InMemory, ASP.NET Core `WebApplicationFactory`, Testcontainers and its PostgreSQL module. Tests include service/domain unit tests, API integration tests and PostgreSQL-backed integration tests. This describes available test code, not complete coverage. The frontend has no `npm test` script or checked-in test files in the inspected tree.

## Fresh executed results

**Executed 10 October 2026** from repository root:

~~~text
dotnet test backend/MilkiDrugStore.sln --configuration Release --no-restore \
  --logger 'trx;LogFileName=project-defense.trx' \
  --results-directory /tmp/mdsms-project-defense-testresults
~~~

- **252 total; 235 passed; 17 failed; 0 skipped.** Exit code 1.
- Docker was available and PostgreSQL Testcontainers started isolated test databases. The test override environment variable was unset, so this run did not target an external/production DB.
- Every failure came from the `CosmeticCategoryIntegrationTests` or `InventorySupplierTests` fixtures resolving `MedicineService`/`CosmeticService` without registering `ICurrentUserService`. Those failures occur during test fixture service construction before the named assertions run. This indicates stale/incomplete test DI setup following the service constructor dependency change; it does not prove those assertions or feature paths are correct or incorrect.
- Raw TRX is in `/tmp/mdsms-project-defense-testresults/project-defense.trx` and is not committed/copied into the repository.
- Older saved TRX runs also show failures; the old docs claim of “171 passed, 0 failed” is not a reliable current result.

### Representative observed test cases

| Test / scenario | Outcome in this run | What it demonstrates |
|---|---|---|
| `SaleServiceTests.CreateAsync_WithCashPayment_SetsPaymentStatusPaid` | PASS | Full payment status calculation |
| `SaleServiceTests.CreateAsync_WithPartialPayment_SetsPaymentStatusPartial` | PASS | Partial balance calculation |
| `SaleServiceTests.CreateAsync_WithDiscount_CapsDiscountForPharmacist` | PASS | Role-based discount cap for covered case |
| `InventoryServiceTests.RecordDamage_ExceedingStock_IsRejectedAndLeavesNoPartialWrite` | PASS | Damage quantity guard/atomicity for covered case |
| `InventoryServiceTests.ProcessExpired_RunTwice_DoesNotDuplicateExpiryRecords` | PASS | Expiry sweep idempotency for covered case |
| `DatabaseIntegrationTests.FEFO_Should_Prioritize_Earliest_Expiry` | PASS | Tested FEFO ordering scenario |
| `PersistenceTests.Sale_With_Items_Should_Survive_Context_Recycle` | PASS | Isolated DB persistence across context recycle |
| `CosmeticCategoryIntegrationTests.BuiltInCosmeticCategory_IsReturnedByApiAfterFreshDbContext` | FAIL | Fixture fails while resolving missing `ICurrentUserService`; assertion not reached |
| `InventorySupplierTests.CreatePurchase_Medicine_SetsBatchSupplierFromPurchase_AndInventoryResolvesIt` | FAIL | Fixture fails while resolving missing `ICurrentUserService`; assertion not reached |

The exact named cases and overall counters were read from the generated TRX. Passing one case is not proof of full feature correctness.

## Build verification

- `npm run build` in `frontend/`: **succeeded**, Vite transformed 2,762 modules and built output. Warnings: settingsApi is both dynamic and static imported; main minified JS chunk is about 994 kB, above Vite’s 500 kB warning threshold.
- The .NET test invocation built the solution in Release successfully before tests; compiler/analyzer warnings included EF1002 interpolated raw SQL in SaleRepository/PurchaseRepository, CS1998 async method without await, nullable warnings in tests, and xUnit analyzer warnings.
- These are builds, not a security or performance audit.

## Existing test table image correction

`docs/3.6-testing.md` and `docs/test-case-table.svg/png` previously said 171 passed / no failures and that Docker was unavailable. That conflicts with the fresh run. The Markdown testing page and SVG/PNG were corrected to the actual 252/235/17 result; the image now labels representative cases as examples and identifies the DI setup failure count.

## Testing concepts

- Unit tests isolate one class with mocks/in-memory data; fast but may miss relational provider behavior.
- Integration tests exercise multiple layers/API or real PostgreSQL migrations and constraints.
- Testcontainers gives tests an isolated disposable PostgreSQL instance; do not configure it to a production database.
- A regression test should reproduce the exact missing `ICurrentUserService` fixture dependency before the fixture is fixed.
- Useful edge cases include concurrent sale against same batch, expired batch sale, negative/overpaid sale payments, arbitrary purchase status, branch-specific reports, migration rollback, and backup restore.

## Recommended next quality actions (not performed)

1. Register `ICurrentUserService` in the two failing test fixtures using a deterministic mock/current-user implementation.
2. Re-run the full suite and retain a CI artifact; do not update “passed” numbers until a fresh result exists.
3. Add frontend tests for auth refresh, route roles and critical POS calculations, plus end-to-end tests against a non-production environment.
4. Add concurrency, expiry boundary, payment bounds, and branch scope tests.
5. Fix analyzer warnings and configure CI to fail on unhandled warnings where appropriate.

## Corrected sample table/image

See [docs/3.6-testing.md](../3.6-testing.md), [SVG](../test-case-table.svg), and [PNG](../test-case-table.png). The sample image is a visual summary of observed test outcomes, not a coverage claim.

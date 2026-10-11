# Technical challenges and defensible explanations

This page separates observable repository changes from historical claims. Commit messages indicate that work was changed; they do not identify the presenter or prove the issue was deployed/fixed in production.

| Topic | Evidence and current status | How to explain it | Remaining risk / next check |
|---|---|---|---|
| Cosmetic support extended into batch/purchase/sale/inventory flows | Entities, services, migrations and tests include cosmetic batches; commits include migrations and integration coverage | “The data model was extended so cosmetic stock can be purchased and sold by batch, with optional expiry.” | Current two integration fixtures fail before assertions due to missing current-user DI |
| Supplier-specific batch details | PurchaseService sets supplier on batch; inventory mapping resolves supplier per batch; integration tests exist | “Supplier association belongs to the batch, because different lots can come from different suppliers.” | Current `InventorySupplierTests` fixture group cannot run its assertions until DI setup is repaired |
| Category and unit resolution for built-in/custom IDs | Category service uses built-in IDs and custom records; product FK constraints are not present | “Application code resolves the display category; these IDs are not relational foreign keys.” | Add relational catalog design/migration if strict referential integrity is required |
| Auth revocation for deactivated users | `OnTokenValidated` rechecks active status; tests cover old JWT rejection after deactivation | “The JWT signature alone is not enough; active state is checked against the server on requests.” | Adds database dependency/request overhead; inspect outage behavior and caching tradeoff |
| Cost visibility by role | Inventory redaction commit/test verifies server-side purchase cost suppression | “Sensitive costs are removed in backend response mapping, not just hidden by React.” | Recheck every endpoint and export/report path for cost leakage |
| Report responsiveness and business period | Recent commits fix report UI overlap and use backend business period; report service uses Ethiopian clock rules | “A backend period boundary avoids the frontend inventing a different date range.” | Dashboard branch filtering, unguarded empty cosmetic batch Min, and in-memory aggregation remain concerns |
| Deployment health route | Commit `6958d075` says health path fix; current source has `/api/health`; live Render host returned no-server 404 | “The route exists in source but the remote host did not route to a running service during my check.” | Hosting dashboard, build/deploy log, service state and DB attachment need operator verification |
| Testing documentation mismatch | Existing page said 171 pass/0 fail with Docker unavailable; fresh run 252/235/17 with Testcontainers | “I corrected the documentation to reflect a reproducible current run.” | Fix fixtures, rerun, store CI artifact and re-audit table image |
| Sale/purchase atomicity versus post-commit audit | Business rows commit before audit call | “The core transaction protects stock and sale rows; audit logging is a later side effect.” | Use an outbox or audit in same transaction where feasible; test retry/idempotency |
| Automated expiry | Daily job sweeps existing expired stock and records it | “A periodic worker complements the live stock service.” | Ensure sale query itself excludes past expiry and avoid multi-instance double work |

## How to discuss a failed test professionally

State the command, isolation boundary, counts, and failure cause. Explain that the failures occurred during fixture service resolution before test assertions. Do not call the suite green or imply those feature assertions passed. A clean fix requires registering the exact `ICurrentUserService` dependency and rerunning the whole suite; source code was not changed for this report.

## Debugging method to describe

1. Reproduce with a specific test or API request and record the exact output.
2. Trace the request from page/action through Axios, route, controller, service, repository, and DB.
3. Inspect DI and configuration before changing business code.
4. Identify whether the error is input, authorization, dependency resolution, provider/schema, or business logic.
5. Add/repair a regression test, run the narrow case, then run the relevant full suite.
6. Verify persisted result in a fresh context and check rollback/error path.
7. If deployment-related, compare source config with hosting dashboard/logs and probe a health endpoint.

Do not claim a historical challenge was personally fixed unless your own records confirm that.

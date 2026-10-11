# Findings and verification status

## Severity-ranked findings

| Severity | Finding | Evidence | Recommended next step |
|---|---|---|---|
| High | Current backend test run has 17 failures because two integration fixtures omit `ICurrentUserService` | TRX at `/tmp/mdsms-project-defense-testresults/project-defense.trx`; constructors and fixtures | Repair test fixture DI; rerun and publish actual result |
| High | Configured Render API health endpoint returns 404/no-server | Live probe 2026-10-10; header `x-render-routing: no-server` | Inspect hosting dashboard, deploy state/logs and DB attachment |
| High | Login frontend logs entire response, including token fields | `frontend/src/store/appStore.ts` | Remove/redact sensitive response logging |
| High | JWT signing service has a source fallback if configured secret absent | `Infrastructure/Services/JwtTokenService.cs`, Program.cs | Fail closed and validate non-placeholder high-entropy secret |
| Medium-high | Refresh/reset tokens stored raw; client token storage is localStorage; logout is client-only | AuthService/entities/appStore/AuthController | Hash tokens, add server revoke, redesign browser session boundary |
| Medium | Render CORS key does not match the bound section; frontend API URL falls back to localhost if build variable is absent | render.yaml, Program.cs, frontend/src/config.ts | Align CORS key, set VITE_API_BASE_URL at build, verify preflight and browser network target |
| Medium | FluentValidation types exist without registration/pipeline | `Validators/Validators.cs`, DI scan | Register validators and test boundary rejection |
| Medium | FEFO ordering may include already expired batches until daily sweep | SaleService and ExpiryCheckBackgroundService | Filter expired lots in sale query and test exact boundary |
| Medium | Report branch filters are inconsistent; cosmetic low-stock calculation can Min an empty filtered batch list; broad data is materialized | ReportService lines 168–208 | Guard empty batches, align branch filters, move aggregates into DB, and test no-batch/per-branch cases |
| Medium | Sale/purchase audit call after commit may fail after persistence | service order of operations | Same-transaction audit/outbox and idempotent client behavior |
| Medium | Sale payment bounds and purchase supplied status need review | SaleService/PurchaseService | Validate amount 0..total; derive or validate allowed status |
| Medium | Concurrent sales may race on the same stock lot | No general concurrency token found | Add optimistic/atomic stock decrement and race integration test |
| Low-medium | SQL analyzer warnings on interpolated raw SQL lock queries | Sale/Purchase repositories | Use parameters and rerun analyzer |
| Informational | Build succeeds with frontend chunk/import warnings | Vite output 2026-10-10 | Split chunks and avoid mixed static/dynamic import if useful |

## Final verification report

| Area | Status | Evidence | Remaining work |
|---|---|---|---|
| Repository inspection | VERIFIED | Git status/HEAD, file tree, manifests, source/config inspection | Personal role/context needs presenter input |
| Frontend analysis | VERIFIED | App/routes/store/api/pages; `npm run build` success | Browser end-to-end tests absent |
| Backend analysis | VERIFIED | Program, layers, services, controller routes, middleware | Runtime behavior depends on current hosting state |
| Architecture | VERIFIED | Project references and request path; Mermaid diagrams here | Actual dashboard topology not accessible |
| Database schema | VERIFIED from model/config | 24 AppDbContext DbSets, configs, five migrations and ER source | Live DB schema not queried |
| Business rules | PARTIALLY VERIFIED | Service code and backend test names/results | Several edge cases and validators need work |
| Authentication/authorization | PARTIALLY VERIFIED | JWT setup, BCrypt service, attributes, tests | Secrets/runtime configuration not audited; token risks remain |
| API documentation | VERIFIED from source | Controller route/attribute scan | Deployed API route behavior unavailable |
| Automated tests | VERIFIED execution; NOT ALL PASS | .NET run: 252 total, 235 pass, 17 fail, 0 skipped | Fix fixture setup and rerun |
| Frontend build | VERIFIED | `npm run build`, success with warnings | No frontend test suite |
| Production configuration | PARTIALLY VERIFIED | Dockerfile, Render and Vercel files | Hosting dashboard values/state not visible |
| Deployment | PARTIALLY VERIFIED / END-TO-END NOT VERIFIED | Vercel 200; Render API 404 no-server on 2026-10-10 | Restore/confirm API and verify DB workflow |
| Evaluator question bank | VERIFIED artifact | `14-evaluator-questions-and-answers.md`, 150 entries | Presenter should practice and personalize |
| Presentation preparation | VERIFIED artifacts | Slides, demo, glossary, study plan | Actual internship facts and contribution need personalization |

## Commands and actions performed

- Inspected Git baseline and source/manifests/configuration.
- Ran backend suite using isolated PostgreSQL Testcontainers; command and result are in [testing](09-testing-and-quality.md).
- Ran `npm run build` in `frontend/`; success with warnings.
- Probed public Vercel and Render URLs; result is in [deployment](10-deployment-and-persistence.md).
- Did not read the local SQLite/DB artifact, access a production database, alter hosting, run deployment, or edit application source.
- Corrected an inaccurate untracked testing document and its diagram image to match actual run evidence. Added documentation under `docs/project-defense/`.

## Questions requiring manual confirmation

1. What exact work did the presenter personally contribute?
2. What organization, internship dates, project sponsor and requirements were officially assigned?
3. Is the Render service intentionally paused, deleted, or awaiting deployment? What is the current deployed commit?
4. Is production PostgreSQL attached and backed up; has restore been tested?
5. Are production JWT/email/S3 secrets valid and managed outside source?
6. What branch behavior does the store require for pharmacist reads and reports?
7. Does a future API deployment enforce CORS using `Cors__AllowedOrigins`?
8. Is there a CI pipeline/hosting automation outside this repository?

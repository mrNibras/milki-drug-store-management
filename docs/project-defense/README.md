# Milki Drug Store Management System: project defense knowledge base

This folder turns the checked-out repository into a study and presentation guide. Read in order for a full walkthrough; use the revision sheet and question bank for rehearsal.

## Phase 1 reconnaissance — completed

**VERIFIED at repository HEAD `fbe48983` (branch `master`, matching `origin/master`):** the repository contains a React 19 + TypeScript + Vite frontend, a .NET 8 ASP.NET Core API, and a PostgreSQL/EF Core persistence layer. The backend is split into Domain, Application, Persistence, Infrastructure, API, and Tests projects. It is a layered modular monolith, with selective MediatR request handling; it is not a microservice system. The working tree had pre-existing untracked diagram/testing documentation, which was preserved. No application-source change was made for this knowledge base.

**Important initial findings:** the existing testing document claimed 171 passing tests and no failures; that claim is contradicted by a fresh isolated Testcontainers run, which recorded 252 total, 235 passed, 17 failed. The failures are test-fixture dependency registration failures. Vercel answered HTTP 200, but the configured Render API health URL returned 404 with `x-render-routing: no-server`; end-to-end production operation is therefore unverified and currently appears unavailable. `npm run build` succeeds with chunk warnings. These results are dated 10 October 2026 and are detailed in [testing and quality](09-testing-and-quality.md) and [deployment and persistence](10-deployment-and-persistence.md).

## Reading map

| File | Purpose |
|---|---|
| [00 Project overview](00-project-overview.md) | Problem, scope, terminology, safe introductions |
| [01 Repository map](01-repository-map.md) | Structure and key files to study |
| [02 Frontend deep dive](02-frontend-deep-dive.md) | Routes, state, API calls, auth, UI risks |
| [03 Backend deep dive](03-backend-deep-dive.md) | Startup, layers, DI, controllers, services, persistence |
| [04 Architecture](04-architecture.md) | Editable high-level, component, deployment, and sequence diagrams |
| [05 Database and ER diagram](05-database-and-er-diagram.md) | 24 DbSets, relations, integrity caveats, ER source/image |
| [06 Business rules](06-business-rules.md) | Purchase, sale, inventory, auth, reporting rules and evidence |
| [07 API reference](07-api-reference.md) | Route groups, HTTP methods, roles |
| [08 Security review](08-security-review.md) | Controls, limitations, prioritized remediation |
| [09 Testing and quality](09-testing-and-quality.md) | Actual commands/results and sample test cases |
| [10 Deployment and persistence](10-deployment-and-persistence.md) | Config versus live verification; safe persistence procedure |
| [11 Technical challenges](11-technical-challenges.md) | Evidence-supported issues and defense language |
| [12 Presentation outline](12-presentation-outline.md) | 17-slide internship presentation structure |
| [13 Live demo script](13-live-demo-script.md) | Safe demo and failure fallback |
| [14 Evaluator Q&A](14-evaluator-questions-and-answers.md) | 150 concise, repository-specific questions |
| [15 Technical glossary](15-technical-glossary.md) | Terms tied to the implementation |
| [16 One-page revision sheet](16-one-page-revision-sheet.md) | Last-minute review |
| [17 Seven-day study plan](17-seven-day-study-plan.md) | Guided learning and one-day emergency plan |
| [18 Findings and verification status](18-findings-and-verification-status.md) | Severity-ranked findings and final audit |

## Evidence labels

- **VERIFIED** means directly present in checked-out source/configuration, reproduced by a local command, or observed by the dated live probe.
- **PARTIALLY VERIFIED** means some evidence exists but important runtime or dashboard conditions remain unknown.
- **DOCUMENTED ONLY** means a README/config says it, but this check did not independently confirm it.
- **INFERRED** means a plausible interpretation, not a proven historical decision.
- **NOT IMPLEMENTED** means no implementation was found in the inspected repository.
- **NOT VERIFIED** means available access cannot establish the claim.

Use repository-relative file paths in this guide. Existing diagram files live in `docs/` and are linked from the relevant sections. Do not describe the live service as fully deployed or claim 100% passing tests.

## Important personal-contribution note

Git history proves that changes exist in this shared repository, but it does not prove which work the presenter personally authored. Adapt the introduction templates only after filling in your actual role and reviewing your own contribution records. Do not state that you individually built a feature unless you can support that statement.

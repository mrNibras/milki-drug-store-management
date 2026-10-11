# Presentation outline

A reasonable 12–15 minute plan is 12 content slides. If the assessment expects a longer report defense, use the optional detail slides. Keep personal contribution and internship organization facts to information you can verify independently.

| Slide | Title and points | Visual/evidence | Spoken focus and likely follow-up |
|---|---|---|---|
| 1 | Title, project name, presenter, dates | Title slide | State actual role; follow-up: what did you personally build? |
| 2 | Context and problem | Simple pharmacy workflow sketch | Explain batches, supplier, expiry and sales problem; avoid quantified benefits without evidence |
| 3 | Objectives and scope | Feature/module list | Separate implemented code from proposed features; ask: does it process payments? answer: no external gateway found |
| 4 | Users and requirements | Existing use case diagram `docs/functional-use-case-diagram.png` | Admin/staff role scope; note API is authority |
| 5 | Technology stack | Logos or stack table from package/csproj | Explain React/TS, .NET 8, EF Core, PostgreSQL, xUnit |
| 6 | Architecture | `docs/system-architecture.svg` plus editable Mermaid in 04 | Layered modular monolith, partial MediatR; ask why not microservices |
| 7 | Database model | `docs/er-diagram.svg` | 24 DbSets grouped into catalog, batch, transaction, identity, audit |
| 8 | Purchase/inventory workflow | Sequence/process diagram | Purchase creates batch and inventory transaction in DB transaction |
| 9 | Sale workflow | `docs/process-sale-sequence.svg` | FEFO, stock decrement, discount and payment rules; note expiry/concurrency caveats |
| 10 | UI demonstration | Screenshots from safe test environment | Explain click → API → service → persistence; never present screenshot as proof of live operation |
| 11 | Testing and quality | Corrected `docs/test-case-table.svg` | 252 total, 235 pass, 17 fixture DI failures; show honest result and next action |
| 12 | Deployment and limitations | Deployment diagram in 04 | Frontend HTTP 200; Render API 404 no-server on 2026-10-10; no end-to-end claim |
| 13 | Challenges and improvements (optional) | Ranked findings table | Show evidence-first debugging and remediation priorities |
| 14 | Future work (optional) | Priority roadmap | Validation, secrets/logging, expired stock exclusion, concurrency, CI and monitoring |
| 15 | Conclusion and questions | One-line recap | Summarize what is implemented and what remains unverified |

## Suggested verbal transitions

- Problem → requirements: “The key data issue is stock by product, branch and batch, not just an overall quantity.”
- Requirements → architecture: “These workflows are split between a React client and a server that owns authorization and business rules.”
- Architecture → database: “The model records headers separately from lines and stock lots.”
- Workflow → testing: “I validated individual calculations and database behavior, and the current run also exposed incomplete test dependency setup.”
- Testing → deployment: “A passing local scenario is not proof the live host is available; I probed both configured public URLs.”

## Facts to have ready

- Exact contribution; current test run count and root cause; 24 DbSets; .NET 8/React 19; actual API host probe; one complete sale or purchase path; at least two limitations. Keep the relevant files open locally during Q&A.

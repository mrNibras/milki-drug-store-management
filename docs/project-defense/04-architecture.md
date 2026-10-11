# System architecture

## Classification

**VERIFIED:** client-server web application; layered modular monolith backend; component-based React frontend; EF Core/Npgsql relational persistence; service/repository/UnitOfWork patterns; MediatR mediator for selected commands and queries. **Not a microservice system.** CQRS is partial/selective because some controllers directly call services and the repository does not consistently separate reads and writes. “Clean Architecture” is too strong because Application references Persistence.

## A. High-level architecture

~~~mermaid
flowchart LR
  A[Administrator] --> Browser
  P[Pharmacist / staff] --> Browser
  subgraph Client[User device]
    Browser[Web browser]
    React[React 19 + TypeScript + Vite]
    Zustand[React Router + Zustand + Axios]
    Browser --> React --> Zustand
  end
  subgraph Web[Configured web hosts]
    Vercel[Vercel frontend<br/>live probe: HTTP 200]
    Render[Render API<br/>live probe: 404 no-server]
  end
  subgraph Api[ASP.NET Core .NET 8 API]
    Controllers[Controllers + auth filters]
    App[Application services / selective MediatR]
    Persist[EF Core + repositories + UnitOfWork]
    Infra[Infrastructure services + hosted jobs]
    Controllers --> App --> Persist
    App --> Infra
  end
  DB[(PostgreSQL<br/>provider configured)]
  S3[(S3 storage provider<br/>configured option, live use unverified)]
  Zustand -- HTTPS JSON /api --> Controllers
  Vercel -. hosts frontend build .-> Browser
  Persist --> DB
  Infra -. optional file storage .-> S3
~~~

The diagram shows repository/configuration intent plus dated probes, not verified dashboard topology. Render returned no server; a working user-to-database production path cannot be claimed.

## B. Backend component diagram

~~~mermaid
flowchart TB
  HTTP[HTTP request + JSON DTO]
  Auth[JWT authentication + role authorization]
  C[API controller]
  M[MediatR command/query<br/>only selected routes]
  S[Application service / handler]
  U[UnitOfWork and repositories]
  EF[AppDbContext + EF Core mapping]
  PG[(PostgreSQL)]
  Event[Sale domain event list<br/>no dispatcher found]
  Audit[Audit log service]
  HTTP --> Auth --> C
  C --> M --> S
  C --> S
  S --> U --> EF --> PG
  S --> Audit
  S -. accumulates .-> Event
~~~

## C. Deployment architecture: declared versus observed

~~~mermaid
flowchart LR
  Repo[Git repository]
  VConfig[frontend/vercel.json<br/>SPA rewrite + headers]
  RConfig[render.yaml<br/>Docker API + DB declaration + disk]
  Vercel[Vercel URL<br/>HTTP 200 observed 2026-10-10]
  Render[Render API URL<br/>HTTP 404 x-render-routing: no-server<br/>observed 2026-10-10]
  Pg[(Render PostgreSQL<br/>declared in YAML; live connection unverified)]
  Disk[(Render /var/data disk<br/>declared; dashboard state unverified)]
  Repo --> VConfig --> Vercel
  Repo --> RConfig --> Render
  Render -. intended connection .-> Pg
  Render -. intended mount .-> Disk
  Vercel -. browser API calls intended .-> Render
~~~

Deployment files describe a target arrangement: Vercel frontend, Render Docker API, a Render PostgreSQL service, persistent disk, and optional S3. The HTTP 200 on the frontend only proves a web response. The API no-server response means deployment readiness/availability is not verified. No hosting dashboard access was available.

## D. Sale request sequence

~~~mermaid
sequenceDiagram
  actor Staff
  participant UI as POSPage / Zustand
  participant API as Axios + ASP.NET API
  participant Auth as JWT + role checks
  participant C as SalesController
  participant M as MediatR
  participant H as CreateSaleCommandHandler
  participant S as SaleService
  participant U as UnitOfWork / EF Core
  participant DB as PostgreSQL
  Staff->>UI: Select items, quantities, discount, payment
  UI->>API: POST /api/sales with bearer token
  API->>Auth: Validate signature, active user and role
  Auth-->>C: Principal with user, role and branch claims
  C->>M: Send CreateSaleCommand with request and claims
  M->>H: Dispatch command
  H->>S: CreateAsync(request, user, role, branch)
  S->>U: Begin transaction
  S->>DB: Read branch batches / calculate allocation
  S->>U: Add sale, lines, stock changes, inventory transactions
  U->>DB: SaveChanges
  S->>U: Commit
  S->>S: Write audit and map response
  S-->>C: Sale DTO (unless post-commit audit fails)
  C-->>API: HTTP response
  API-->>UI: JSON response
  UI->>UI: Update or reload displayed state
~~~

This sequence follows `SalesController` and `SaleService`; the actual UI refresh should be confirmed in current store action before a live demonstration. The service commits before audit log completion, so an audit error can yield an apparent failed request after the sale has persisted.

## Data flow and placement of checks

- Frontend: input and usability checks, token attachment, UI state. These do not establish data integrity.
- API auth: JWT signature/claims and active-user recheck; controller role attributes/policies.
- Application: quantity, stock, batch, discount and payment calculations; some manual validation.
- EF/database: constraints, FK relationships, uniqueness/indexes, decimal precision, transaction commits.
- Response: DTO mapping and selected cost redaction; client state update.

## Architecture trade-offs and answers

- A separated client/server allows independent UI and API work and uses a JSON boundary. It adds CORS, deployment, versioning, and availability coordination.
- A modular monolith is simpler to develop and transact than microservices for this current scope. It still needs explicit module boundaries to avoid coupling.
- React is appropriate for interactive inventory/POS pages, but the repository does not document the original decision; describe this as a technical rationale, not historical fact.
- ASP.NET Core provides DI, authentication middleware, EF integration and typed C# services. Node.js is a valid alternative; no evidence establishes it was rejected.
- EF Core maps relational entities and supports migrations/tracking/transactions; hand-written SQL can be more explicit/faster for some queries but raises mapping and maintenance cost.
- PostgreSQL fits relational references and transactions. SQLite is useful for local small tests but differs in concurrency/provider behavior; this repository configures Npgsql.
- REST is simple and resource-oriented for CRUD/report endpoints. GraphQL could help clients needing variable-shaped reads but adds schema/resolver complexity.
- MediatR can decouple some controllers from handlers, but overuse adds indirection; here it is selective.

## Facts to confirm before a defense

1. Active Vercel/Render project settings, current service deployment commit and logs.
2. Actual production DB attachment, persistence/backup plan and migration history.
3. Whether S3 is reachable and used for application data.
4. Actual per-user branch scope expected by the business.
5. Presenter’s personal contribution and internship/company context.

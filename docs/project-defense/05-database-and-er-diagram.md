# Database and ER diagram

## Actual model

`AppDbContext` exposes 24 DbSets. The PostgreSQL provider is configured in `DbProviderResolver`; entity configurations are applied from the Persistence assembly. Five migrations are present:

1. `20260910142614_InitialCreate`
2. `20260920203840_AddIndexesForUserBranchDamageExpired`
3. `20260923054841_AddCosmeticBatchFields`
4. `20260923103402_MakeProductIdsNullableForCosmetics`
5. `20260929201644_SupportCosmeticDamageAndExpiry`

### Entity/table responsibilities

| Group | Entities | Meaning |
|---|---|---|
| Identity and organization | Roles, Users, Branches, Settings | Role membership, branch ownership and per-branch configuration |
| Catalog | Categories, UnitTypes, Medicines, Cosmetics | Product information and display classification |
| Stock | MedicineBatches, CosmeticBatches, InventoryTransactions | Lot-level received/issued/damaged/expired quantities and movement history |
| Procurement | Suppliers, Purchases, PurchaseItems | Vendor, purchase header and medicine/cosmetic lines |
| Sales | Sales, SaleItems | Sale header and product/batch lines, prices, discounts, payment values |
| Control/history | DamageRecords, ExpiredRecords, Notifications, AuditLogs | Operational write-offs, alerts and recorded staff actions |
| Retention/auth | RefreshTokens, PasswordResets, AuditLogArchives, NotificationArchives | Account token/reset records and archive-shaped tables; the current retention job deletes old rows and no archive population code was found |

## Relationship summary

- Role 1 → many Users; Branch 1 → many Users, Sales, Purchases, medicine batches, damage/expiry records, notifications and audit records.
- Branch ↔ Settings is configured one-to-one through unique `Settings.BranchId`.
- User 1 → many Sales, RefreshTokens, PasswordResets, AuditLogs; user/transaction relationship has a nullable EF shadow FK, described below.
- Medicine 1 → many MedicineBatches and optional line references; Cosmetic 1 → many CosmeticBatches and line references.
- Supplier 1 → many Purchases and optional medicine/cosmetic batch references; Cosmetic also has an optional supplier link.
- Purchase 1 → many PurchaseItems; Sale 1 → many SaleItems.
- PurchaseItem/SaleItem allow nullable medicine/product and cosmetic references, so one table supports either product kind. Application code is responsible for making a line coherent.
- DamageRecord/ExpiredRecord can reference medicine or cosmetic and their batch; one-of-two consistency is primarily a service rule.
- CategoryId and UnitTypeId on Medicine/Cosmetic are not database foreign keys. Built-in negative IDs and custom positive IDs are resolved by catalog code.
- `ReferenceId` in InventoryTransaction and original archive identifiers are scalar identifiers rather than FKs in the model. Archive original IDs are uniquely indexed but do not enforce parent-row existence.

## Diagram sources and caveats

The current editable source is [ER diagram PlantUML](../er-diagram.puml); rendered assets are [SVG](../er-diagram.svg) and [PNG](../er-diagram.png). The separate [database schema graphic](../database-schema.svg) may simplify fields. These diagrams were checked against 24 AppDbContext DbSets and the current configuration; use the notes below when presenting.

**Model caveat:** `InventoryTransaction` has a nullable shadow `UserId` FK inferred/configured via `User.InventoryTransactions`, in addition to its CLR `CreatedBy` field. Service code generally sets `CreatedBy`. Do not conflate the creator column with the navigation FK. The ER PlantUML marks the shadow field with `UserId*` and a note.

## Integrity and database principles in context

- **Primary key:** identifies one row, such as `SaleId` or `BatchId`.
- **Foreign key:** prevents dangling relationships where configured, such as SaleItem → Sale.
- **Unique constraints/indexes:** prevent duplicate email, product codes/barcodes, sale numbers, purchase numbers and selected catalog names (see configurations).
- **Indexes:** support common lookups and filters, including branch/date, product/batch, notification and audit paths. Indexes speed reads but add write/storage cost.
- **Decimal precision:** monetary fields are configured with fixed decimal precision (commonly decimal(18,2)); see individual configuration classes.
- **Delete behavior:** CosmeticBatch explicitly cascades from Cosmetic; other behaviors follow fluent configuration/provider conventions and should be checked before deleting parent records. Product deletion is soft-delete in services.
- **Transactions / ACID:** purchase and sale multi-row operations wrap inventory and line updates in a DB transaction. External calls/audit after commit are not atomic with it.
- **Normalization:** the schema separates sale headers from lines, purchases from lines, and products from batches. This reduces repeated header/vendor/product data. Some duplicated product identification and optional polymorphic references are pragmatic but require application consistency; do not claim a formal 3NF proof without a complete functional-dependency analysis.
- **Loading:** EF navigation loads are generally explicit using Include or queries; no lazy-loading proxy registration was found.
- **Concurrency:** sequence generation uses PostgreSQL advisory transaction locks, but there is no general optimistic concurrency token on stock batches. Concurrent sales against the same batch need explicit concurrency protection/testing.

## Data durability and recovery

Successful sale/purchase saves go through EF Core and transaction commits. Tests named `Sale_With_Items_Should_Survive_Context_Recycle`, `Medicine_Should_Survive_Context_Recycle`, and related persistence tests passed in the fresh test run. This proves isolated test database persistence across contexts, not production database durability or backup quality. A settings backup/restore API and PostgreSQL client tools appear in source/config, but no successful live backup restore was verified. The daily retention job deletes old audit logs and read notifications; despite archive entities existing, no code that copies those rows into archive tables was found.

Do not inspect or use `backend/src/MilkiDrugStore.Api/MilkiDrugStoreDB2.db`; it is a repository-local DB artifact and is not the configured production provider evidence. No data was read or changed.

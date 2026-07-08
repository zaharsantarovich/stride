# Phase 0 Research: Task Planning Application

All Technical Context items are resolved. There are no remaining `NEEDS CLARIFICATION`
markers. Decisions below combine the project constitution, the explicit user-provided
technology choices, and prior repository research (see
`/memories/repo/datetime-concurrency-research.md`).

## Decision Log

### 1. Backend platform & language

- **Decision**: C# on .NET 10 with nullable reference types enabled; ASP.NET Core for the REST API.
- **Rationale**: Mandated by the constitution (Type-Safe Full-Stack, .NET 10 backend). ASP.NET Core is the standard REST host for .NET.
- **Alternatives considered**: Minimal APIs vs. MVC controllers — controllers chosen for clearer grouping of the many CRUD endpoints and easier per-controller authorization.

### 2. Solution layering

- **Decision**: Four backend projects under `src/` — `ZSLabs.Stride.Domain` (entities + enums, no business logic), `ZSLabs.Stride.Persistence` (EF Core `DbContext`, configurations, migrations), `ZSLabs.Stride.App` (services under `Services/`), `ZSLabs.Stride.Api` (controllers + DTO contracts). One matching test project per production project under `tests/`.
- **Rationale**: Required by repository conventions (`copilot-instructions.md`). Clean separation keeps entities logic-free and data access confined to the persistence layer (constitution Principle V).
- **Alternatives considered**: Single project — rejected for poor separation and test isolation.

### 3. Frontend stack

- **Decision**: React 19.x + TypeScript 6.x (strict), built with Vite 8.x. Tailwind CSS 3.x for styling (light theme, shared design tokens). dnd-kit for board drag-and-drop. State via React built-ins only (`useState`, `useReducer`, `useContext`).
- **Rationale**: Constitution mandates React + TypeScript strict and a light theme. User explicitly selected TypeScript v6, Vite v8, Tailwind v3.x, dnd-kit, and React-only state (no Redux/Zustand).
- **Version note**: Per repo convention, resolve the latest stable within each stated major at scaffold time — TypeScript `^6`, Vite `^8`, Tailwind `^3`. Pin the exact resolved versions in `package.json` when the frontend is created.
- **Alternatives considered**: Redux/Zustand — explicitly excluded by the user. react-beautiful-dnd — superseded/unmaintained; dnd-kit chosen for modern React support and touch/mobile compatibility (constitution Principle III).

### 4. Persistence & primary keys

- **Decision**: SQLite via EF Core 10 (SQLite provider) as the single data-access layer; versioned EF Core migrations. All entities use autoincrementing integer primary keys (`int Id`, identity column).
- **Rationale**: Constitution Principle V mandates SQLite through EF Core with migrations. User requested autoincrementing integer PKs for all entities.
- **Alternatives considered**: GUID keys — rejected per explicit user choice for integer identity keys.

### 5. Authentication & authorization

- **Decision**: Username + password sign-in. Passwords hashed and verified with ASP.NET Core Identity `PasswordHasher<TUser>` (PBKDF2). Cookie-based session authentication. Role gate: a single seeded `Admin` user restricted to user-account management; regular users restricted from user management. Sign-in errors are generic (do not reveal whether username or password was wrong — FR-002).
- **Rationale**: User explicitly chose `PasswordHasher<TUser>`. Cookies suit a first-party SPA + same-owner API and avoid token storage in JS (reduces XSS token theft risk). The admin/regular split is enforced by authorization policies per FR-003–FR-005.
- **Alternatives considered**: JWT bearer tokens — heavier and requires client-side token storage; unnecessary for a first-party small-scale app. Full ASP.NET Core Identity stack — heavier than needed; only the password hasher component is required, with a custom lightweight user store.

### 6. API hosting / origins & CORS

- **Decision**: The REST API is served on its own origin/port, never as an `/api` suffix on the frontend domain. Local development: frontend on `http://localhost:5173` (Vite), API on `https://localhost:7080`. CORS on the API allows the frontend origin with credentials enabled (for the auth cookie).
- **Rationale**: User explicitly requested distinct domains (no `/api` suffix) and distinct localhost ports for frontend vs. API in local dev.
- **Alternatives considered**: Reverse-proxy `/api` path on one domain — rejected per explicit user instruction.

### 7. Testing & coverage

- **Decision**: xUnit.v3 as the test framework and NSubstitute for mocking across all `.NET` test projects. Enforce ≥ 80% aggregate line coverage as a CI gate (constitution Principle II).
- **Rationale**: User explicitly chose xUnit.v3 + NSubstitute. Coverage floor is constitutionally mandated and non-negotiable.
- **Alternatives considered**: xUnit v2 / NUnit / Moq — superseded by explicit user choices.

### 8. UTC storage & local-time display

- **Decision**: The application always assigns UTC-kind values to `DateTime` fields (e.g. `DateTime.UtcNow`), so no write-side normalization is needed. A single global EF Core convention applies a **read-only** conversion that re-tags materialized values as `DateTimeKind.Utc` (SQLite stores dates as offset-less text and EF otherwise returns `DateTimeKind.Unspecified`). This ensures the API serializes ISO-8601 UTC strings with a trailing `Z`; the React client converts to the viewer's local time zone with `Intl.DateTimeFormat`.
- **Rationale**: FR-038/FR-039 and SC-007. Because writes are guaranteed UTC, the defensive write-side `ToUniversalTime()` is unnecessary; only the read side needs to restore `Kind = Utc` so serialized timestamps carry the `Z` suffix and the client parses them as UTC rather than local. Applied once as a global convention so every `DateTime`/`DateTime?` property (including new ones) is covered automatically.
- **Alternatives considered**: Two-way converter that also normalizes on write — rejected as redundant given the UTC-write guarantee. Forcing `Z` via a JSON converter at the API boundary, or having the client assume UTC — viable but the read-only EF convention keeps the contract correct with the least, centralized code. Server-side localization — rejected; the viewing user's time zone is a client concern and keeps stored values canonical.

### 9. Concurrency: last-write-wins

- **Decision**: Public-space edits use last-write-wins — the most recent save overwrites earlier changes with no conflict warning (FR-040). No optimistic concurrency token is surfaced to users; on any write, `UpdatedAt` is set to `DateTime.UtcNow` and the latest save simply persists.
- **Rationale**: The spec explicitly resolves conflicts as last-write-wins for a small-scale app. Prior research documented an optional `[ConcurrencyCheck]` retry pattern, but for LWW the simplest correct behavior is to let the last save win without a version gate.
- **Alternatives considered**: Optimistic concurrency with conflict prompts — rejected; contradicts the clarified last-write-wins requirement.

### 10. Deletion strategy

- **Decision**: Hard deletes with cascade — deleting a Space removes its Tasks, their Subtasks, and all Comments; deleting a Task removes its Subtasks and Comments; deleting a Subtask removes its Comments. Configured via EF Core relationships with `DeleteBehavior.Cascade`.
- **Rationale**: FR-036/FR-037 and SC-006.
- **Alternatives considered**: Soft delete — explicitly out of scope (spec mandates hard delete).

### 11. Board ordering & columns

- **Decision**: The board renders one always-visible column per task status including a dedicated Archived column (5 columns). Within a column, tasks are ordered automatically by priority (Critical → High → Medium → Low) then by creation date ascending; no manual within-column reordering. dnd-kit drag only changes a task's status (moves it between columns).
- **Rationale**: FR-020a and FR-026a.
- **Alternatives considered**: Manual ordering with a persisted sort index — rejected; spec disallows manual reordering.

### 12. Space visibility & authorship rules

- **Decision**: Private spaces are visible/editable only by their author. Public spaces are visible and full-CRUD to every regular user, except the Public flag, which only the author may change (FR-016a). Space keys are globally unique. Assignee of a task/subtask is limited to users with access to the space (any registered user for public, author only for private — FR-030a).
- **Rationale**: FR-015–FR-018, FR-030a.

## Outstanding Unknowns

None. All Technical Context fields are resolved.

# Quickstart & Validation Guide: Task Planning Application

This guide explains how to run Stride locally and validate that the feature works
end-to-end. It references the [data model](./data-model.md) and the
[API contract](./contracts/openapi.yaml) rather than duplicating implementation details.

## Prerequisites

- .NET 10 SDK
- Node.js LTS (with npm) for the Vite frontend
- EF Core CLI tools: `dotnet tool install --global dotnet-ef`

## Layout

- Backend solution under `src/` (`ZSLabs.Stride.Domain`, `.Persistence`, `.App`, `.Api`).
- Frontend under `src/ZSLabs.Stride.Web` (React + TypeScript + Vite + Tailwind).
- Tests under `tests/` (one xUnit.v3 project per production project).

## Local URLs & ports

- Frontend (Vite dev server): `http://localhost:5173`
- REST API: `https://localhost:7080`

The API runs on its own origin/port — it is **not** exposed as an `/api` suffix on the
frontend domain. The API enables CORS for the frontend origin with credentials so the
auth cookie flows.

## First-time setup

```powershell
# Backend: restore, apply migrations (creates SQLite DB and seeds the single admin user)
dotnet restore
dotnet ef database update --project src/ZSLabs.Stride.Persistence --startup-project src/ZSLabs.Stride.Api

# Frontend
cd src/ZSLabs.Stride.Web
npm install
```

## Run

```powershell
# Terminal 1 — API on https://localhost:7080
dotnet run --project src/ZSLabs.Stride.Api

# Terminal 2 — frontend on http://localhost:5173
cd src/ZSLabs.Stride.Web
npm run dev
```

## Run tests (≥ 80% line coverage)

```powershell
dotnet test --collect:"XPlat Code Coverage"
```

## End-to-end validation scenarios

Each scenario maps to a user story / success criterion in [spec.md](./spec.md).

1. **Admin sign-in & user management (US1, US2 — FR-001..FR-011, SC-008)**
   - Sign in as the seeded admin → only user-account management is available (no spaces/tasks).
   - Create a regular user; creating a duplicate username returns `409`.
   - Confirm the new regular user can sign in. Attempt to reach user management as a
     regular user → denied.
   - Sign in with a wrong password → generic error that does not reveal which field failed.

2. **Spaces & visibility (US3 — FR-012..FR-019, SC-004, SC-005, SC-010)**
   - As a brand-new regular user, confirm zero spaces on first sign-in.
   - Create a private and a public space (unique keys). Duplicate key → `409`.
   - From a second account, confirm the private space is hidden and the public space is
     visible and editable; confirm the non-author cannot change the public space's visibility.
   - Delete a space that has tasks/subtasks/comments → all descendants are gone (SC-006).

3. **Board & tasks (US4 — FR-020..FR-026a, SC-001, SC-002, SC-003)**
   - Open a space board: five always-visible columns (Backlog, Todo, In Progress, Done, Archived).
   - Create a task (defaults to Backlog) and confirm it lands in the correct column.
   - Create multiple tasks with different priorities in one column → ordered Critical → High →
     Medium → Low, then by creation date.
   - Drag a task to another column → status updates within ~1s, persists across reload.

4. **Subtasks (US5 — FR-027..FR-031)**
   - Add subtasks to a task; confirm they render as a list under the parent task.
   - Update a subtask's status (Todo/In Progress/Done) and other fields.
   - Delete a subtask → its comments are removed too.

5. **Comments (US6 — FR-032..FR-035, SC-009)**
   - Add ≥ 3 comments to a task and a subtask → sorted ascending by creation date.
   - Edit/delete own comment succeeds; editing/deleting another user's comment is denied.

6. **Dates & time zones (FR-038, FR-039, SC-007)**
   - Confirm stored timestamps are UTC and displayed values match the viewer's local time
     zone; verify across at least two different time zones.

7. **Concurrency (FR-040)**
   - Edit the same task in a public space from two sessions → the most recent save wins with
     no conflict warning (last-write-wins).

## Expected outcomes

All scenarios above pass, the .NET test suite is green with ≥ 80% aggregate line coverage,
and the UI is verified at both mobile and desktop widths against the light theme.

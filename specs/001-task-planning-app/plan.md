# Implementation Plan: Task Planning Application

**Branch**: `001-task-planning-app` | **Date**: 2026-07-08 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-task-planning-app/spec.md`

## Summary

A task planning application for personal and family usage. Regular users organize work into private/public **Spaces**; each space renders a single **board** view with always-visible status columns (Backlog, Todo, In Progress, Done, Archived). Users create **Tasks** (with priority-based auto-ordering within columns), break them into **Subtasks**, and add **Comments**; drag-and-drop moves a task between columns to change its status. A single seeded **admin** user manages regular user accounts only. Deletes are hard/cascade, all dates are stored in UTC and shown in the viewer's local time, and concurrent edits in shared public spaces use last-write-wins.

Technical approach: a typed full-stack web application — a React + TypeScript SPA (Vite build, Tailwind CSS light theme, dnd-kit for board drag-and-drop, React built-in state) talking over HTTP to an ASP.NET Core REST API on .NET 10. The API is layered into Domain (EF entities/enums), Persistence (EF Core + SQLite + migrations), App (services), and Api (controllers/DTOs). Passwords are hashed with ASP.NET Core Identity `PasswordHasher<TUser>`; all primary keys are autoincrementing integers.

## Technical Context

**Language/Version**: C# on .NET 10 (nullable reference types enabled) for the backend; TypeScript 6.x (strict mode) for the frontend.

**Primary Dependencies**:

- Backend: ASP.NET Core (REST API), Entity Framework Core 10 (SQLite provider), ASP.NET Core Identity `PasswordHasher<TUser>` for password hashing/verification, cookie-based authentication.
- Frontend: React 19.x, Vite 8.x (build/dev server), Tailwind CSS 3.x (light theme), dnd-kit (board drag-and-drop). State via React built-ins (`useState`/`useReducer`/`useContext`) — no external state library (no Redux/Zustand).
- Testing: xUnit.v3 + NSubstitute (mocking) for .NET unit tests.

**Storage**: SQLite as system of record, accessed exclusively through EF Core with versioned EF Core migrations. All entities use autoincrementing integer primary keys. All `DateTime` fields stored in UTC.

**Testing**: xUnit.v3 with NSubstitute for the .NET solution; unit tests authored alongside implementation, with a regression test accompanying each bug fix. Aggregate unit-test line coverage MUST be ≥ 80%.

**Target Platform**: Modern browsers only (latest stable Chrome, Edge, Firefox, Safari). Backend runs cross-platform (.NET 10). Local development: frontend and API run on `localhost` with distinct ports (frontend `http://localhost:5173`, API `https://localhost:7080`); the API is served on its own origin/port, never as an `/api` suffix on the frontend domain.

**Project Type**: Web application (React SPA frontend + ASP.NET Core REST API backend).

**Performance Goals**: Board status change on drag-and-drop reflected within 1 second and persisted (SC-003); create-space-to-empty-board under 1 minute (SC-001); create-task-into-column under 30 seconds (SC-002). Personal/family scale (small number of users).

**Constraints**: Responsive UI usable at mobile and desktop widths; light theme only, colors from shared design tokens; UTC storage with local-time display; hard/cascade deletes; last-write-wins on concurrent edits in public spaces.

**Scale/Scope**: Personal and family usage — a small number of users; 6 user stories, ~5 core entities, roughly a dozen board/space/user screens and flows.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Principle | Status | Notes |
|---|-----------|--------|-------|
| I | Type-Safe Full-Stack Architecture | PASS | React + TypeScript (strict); no plain-JS app source. Backend on .NET 10 with nullable reference types enabled. API request/response shapes typed on both sides (C# DTOs ↔ TS interfaces, kept in sync via contracts). |
| II | Test Discipline (NON-NEGOTIABLE) | PASS | xUnit.v3 + NSubstitute; unit tests authored alongside implementation; ≥ 80% aggregate line coverage; bug fixes require a failing regression test. |
| III | Responsive, Cross-Device Experience | PASS | Tailwind responsive utilities (fluid layouts, breakpoints, touch-friendly targets); board verified at mobile and desktop widths. |
| IV | Modern Browser Baseline | PASS | Targets current Chrome/Edge/Firefox/Safari only; no legacy polyfills or obsolete transpile targets (Vite/`esnext`). |
| V | Lightweight, Reliable Persistence | PASS | SQLite via EF Core as the single data-access layer; versioned EF Core migrations; parameterized queries only (no string-concatenated SQL). |
| VI | Light-Theme User Interface | PASS | Light theme everywhere; colors sourced from shared Tailwind design tokens; no dark/alternate theme introduced. |

**Result**: All gates PASS. No violations to justify; Complexity Tracking left empty.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── ZSLabs.Stride.Domain/            # EF entities, enums — no business logic
│   ├── Entities/                    # User, Space, Task, Subtask, Comment
│   └── Enums/                       # UserRole, SpaceVisibility, TaskStatus, TaskPriority, SubtaskStatus
├── ZSLabs.Stride.Persistence/       # EF Core data-access layer
│   ├── StrideDbContext.cs
│   ├── Configurations/              # IEntityTypeConfiguration<T> per entity
│   └── Migrations/                  # EF Core migrations (versioned)
├── ZSLabs.Stride.App/               # Application/business logic
│   └── Services/                    # AuthService, UserService, SpaceService, TaskService, SubtaskService, CommentService
├── ZSLabs.Stride.Api/               # ASP.NET Core REST API
│   ├── Controllers/
│   ├── Contracts/                   # Request/response DTOs (typed contract)
│   └── Program.cs
└── ZSLabs.Stride.Web/               # React + TypeScript SPA (Vite)
    ├── src/
    │   ├── components/              # Board, Column, TaskCard, SubtaskList, CommentList, etc.
    │   ├── pages/                   # SignIn, Spaces, SpaceBoard, AdminUsers
    │   ├── api/                     # typed API client + generated/mirrored contract types
    │   ├── hooks/                   # React state hooks (useAuth, useSpaces, ...)
    │   └── styles/                  # Tailwind entry + design tokens
    ├── index.html
    ├── vite.config.ts
    ├── tailwind.config.js
    └── package.json

tests/
├── ZSLabs.Stride.Domain.Tests/
├── ZSLabs.Stride.App.Tests/         # service unit tests (NSubstitute mocks)
├── ZSLabs.Stride.Persistence.Tests/ # EF Core config/migration behavior (SQLite in-memory/file)
└── ZSLabs.Stride.Api.Tests/         # controller unit tests
```

**Structure Decision**: Web application with a layered .NET backend and a React SPA frontend. Backend projects follow the mandated `ZSLabs.Stride.*` prefix and layering (Domain → Persistence → App → Api), with entities kept free of business logic. Source lives under `/src/`, unit tests under `/tests/` (one test project per production project). The frontend (`ZSLabs.Stride.Web`) is a standalone Vite app served on its own localhost port and deployed on its own origin, communicating with the API over typed HTTP contracts.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No constitutional violations. All gates pass; no complexity to justify.

## Phase 0 & 1 Artifacts

- [research.md](./research.md) \u2014 resolved technical decisions and dependency versions.
- [data-model.md](./data-model.md) \u2014 entities, fields, relationships, validation, enums.
- [contracts/](./contracts/) \u2014 REST API contract (OpenAPI) for auth, users, spaces, tasks, subtasks, comments.
- [quickstart.md](./quickstart.md) \u2014 setup and end-to-end validation guide.

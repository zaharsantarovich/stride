# Implementation Plan: Task Modal Editing

**Branch**: `002-add-edit-task-dialog` | **Date**: 2026-07-14 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/002-add-edit-task-dialog/spec.md`

## Summary

Replace the board's inline task creation/editing controls with a shared task modal opened from task cards and the board header. The board becomes a read-only planning summary for task title, priority, assignee username, and subtask titles while preserving drag-and-drop status movement. The modal supports creating/editing tasks and managing subtasks, discards unsaved changes on dismiss, uses full-screen mobile and centered desktop sizing, and keeps save errors visible without losing draft entries.

Technical approach: keep the current React + TypeScript SPA and ASP.NET Core REST API. Reuse existing task/subtask request shapes and `AssigneeId` fields, add a generic regular-user lookup endpoint for public-space assignee selectors, and keep backend validation authoritative: private spaces may assign only the current user, public spaces may assign any regular user, and task/subtask saves reject ineligible assignees. Do not add frontend unit tests; cover assignment rules with .NET unit tests and validate the UI through frontend build/lint plus mobile/desktop manual checks.

## Technical Context

**Language/Version**: C# on .NET 10 (nullable reference types enabled) for the backend; TypeScript 6.x in strict mode for the frontend.

**Primary Dependencies**:

- Backend: ASP.NET Core REST API, Entity Framework Core 10 with SQLite provider, cookie-based authentication/authorization.
- Frontend: React 19.x, Vite 8.x, Tailwind CSS 4.x light-theme tokens, dnd-kit for board drag-and-drop.
- Testing: xUnit.v3 + NSubstitute for .NET unit tests. No frontend unit test dependencies are added for this feature.

**Storage**: SQLite via EF Core. No schema change is required because tasks and subtasks already store nullable `AssigneeId`, spaces already store `IsPublic`, and users already store `Role`.

**Testing**: .NET unit tests for task/subtask assignee eligibility and rejection paths; existing .NET/API tests remain in `/tests`. Frontend validation uses `npm run build`, `npm run lint`, and responsive manual checks; do not add frontend unit tests.

**Target Platform**: Latest stable Chrome, Edge, Firefox, and Safari. Local development uses frontend `https://localhost:5173` and API `https://localhost:7080` on separate origins with cookie credentials.

**Project Type**: Web application: React SPA frontend + ASP.NET Core REST API backend.

**Performance Goals**: Users can create or edit a task and see the updated board summary in under 30 seconds; modal opens without blocking the board interaction path; existing status drag-and-drop remains reflected and persisted within 1 second.

**Constraints**: Responsive modal: full-screen below 768px; centered at 768px and wider with max 90vw, max 90vh, and max width 960px. Light theme only. Board task/subtask details are read-only except status movement. Save errors preserve modal draft state. Assignment rules: private space = current user only; public space = any regular user; admin users are not assignable.

**Scale/Scope**: Personal/family usage with a small number of regular users. Scope spans the board page, task card summaries, task modal UI, typed frontend API client, task/subtask backend validation, and one generic regular-user lookup endpoint.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Principle | Status | Notes |
|---|-----------|--------|-------|
| I | Type-Safe Full-Stack Architecture | PASS | Feature remains React + TypeScript strict and .NET 10. New/updated API request and response shapes are typed in C# DTOs and mirrored TypeScript interfaces. |
| II | Test Discipline (NON-NEGOTIABLE) | PASS | Automated unit tests are still required for backend assignment rules and rejection paths. Per user constraint, no frontend unit tests are added; frontend correctness is validated with build/lint and responsive checks. |
| III | Responsive, Cross-Device Experience | PASS | Modal dimensions are specified for mobile and desktop; quickstart requires verification below 768px and at desktop width. |
| IV | Modern Browser Baseline | PASS | Uses current React/Vite/browser capabilities; no legacy polyfills or obsolete targets introduced. |
| V | Lightweight, Reliable Persistence | PASS | No datastore change. Existing EF Core/SQLite entities and relationships support the feature; backend validation uses EF Core queries. |
| VI | Light-Theme User Interface | PASS | Modal and board changes use the existing light-theme tokens; no dark or alternate theme introduced. |

**Result**: All gates PASS. No constitutional violations to justify.

## Project Structure

### Documentation (this feature)

```text
specs/002-add-edit-task-dialog/
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
├── ZSLabs.Stride.Api/
│   ├── Contracts/                  # Task/Subtask DTOs plus regular user lookup response DTO if needed
│   └── Controllers/                # Tasks, Subtasks, and generic regular-user lookup endpoint
├── ZSLabs.Stride.App/
│   └── Services/                   # TaskService/SubtaskService assignee eligibility validation; user lookup service
├── ZSLabs.Stride.Domain/
│   ├── Entities/                   # Task, Subtask, Space, User (no business logic)
│   └── Enums/                      # UserRole, TaskStatus, TaskPriority, SubtaskStatus
├── ZSLabs.Stride.Persistence/       # EF Core DbContext/configuration; no new migration expected
└── ZSLabs.Stride.Web/
    └── src/
        ├── api/                    # typed task and assignee client methods/contracts
        ├── components/             # Board, TaskCard, TaskModal, modal subtask editor controls
        ├── hooks/                  # task state orchestration reused by modal
        └── pages/                  # SpaceBoard removes inline create form and opens modal

tests/
├── ZSLabs.Stride.App.Tests/         # assignment eligibility unit tests for task/subtask services
└── ZSLabs.Stride.Api.Tests/         # API response behavior for ineligible assignees/regular user lookup if controller logic changes
```

**Structure Decision**: Continue with the existing layered web application. UI changes remain under `src/ZSLabs.Stride.Web/src`; backend validation and optional query helpers live in `src/ZSLabs.Stride.App/Services`; public REST contracts live in `src/ZSLabs.Stride.Api`. No frontend test project or frontend unit test dependency is introduced.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | N/A | N/A |

## Phase 0 & 1 Artifacts

- [research.md](./research.md) - resolved technical decisions for modal structure, assignment eligibility, and validation strategy.
- [data-model.md](./data-model.md) - feature data model, validation rules, and state transitions.
- [contracts/openapi.yaml](./contracts/openapi.yaml) - REST contract changes for modal task/subtask saves and regular user lookup.
- [quickstart.md](./quickstart.md) - runnable validation scenarios for modal create/edit, assignment rules, read-only board display, and responsive behavior.

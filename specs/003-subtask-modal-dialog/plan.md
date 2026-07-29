# Implementation Plan: Dedicated Subtask Modal Dialog

**Branch**: `003-subtask-modal-dialog` | **Date**: 2026-07-29 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/003-subtask-modal-dialog/spec.md`

## Summary

Move subtask creation, viewing, editing, deletion, and discussion out of the task dialog into one dedicated modal with the task dialog's existing responsive geometry. The board page will own a single discriminated modal state so task and subtask dialogs replace one another rather than stack, while preserving whether a subtask was launched from the board or parent task for post-delete return behavior.

The existing .NET layers remain in place. `SubtaskService` will expose an authorized single-subtask query and load author/assignee/comment-author data; API contracts will add author usernames; existing create/update/delete and comment endpoints will be reused. Task-list and subtask comment data will be returned in deterministic creation order. No entity or database schema change is required. The React client will replace embedded subtask forms with a table, add URL-like subtask controls to both launch surfaces, and keep a saved subtask modal open while refreshing board summaries.

## Technical Context

**Language/Version**: C# on .NET 10 with nullable reference types enabled; TypeScript 6.0.x in strict mode with React 19.2.x.

**Primary Dependencies**: ASP.NET Core REST API, Entity Framework Core 10 with SQLite, React 19, React Router 7.18, Vite 8.1, Tailwind CSS 4.3, and existing React built-in state. No runtime or frontend test dependency is added.

**Storage**: Existing SQLite database accessed exclusively through EF Core. Existing `Task`, `Subtask`, `Comment`, and `User` tables and cascade relationships are sufficient; no migration is planned.

**Testing**: Existing xUnit.v3, NSubstitute, ASP.NET Core test host, and Coverlet projects for service/controller regression coverage. Run all .NET tests with aggregate line coverage at or above 80%. Frontend validation uses existing TypeScript build, ESLint, and browser checks at mobile and desktop widths; no frontend unit tests are added.

**Target Platform**: Latest stable Chrome, Edge, Firefox, and Safari; responsive mobile and desktop web layouts. Backend remains cross-platform .NET 10. Local frontend and API use their existing HTTPS origins.

**Project Type**: Typed full-stack web application with a React SPA and layered ASP.NET Core REST API.

**Performance Goals**: Existing subtask details open from either launch surface without an extra page navigation; successful create/update refreshes visible summaries while leaving the modal open; users can add a subtask and see it on both displays within 45 seconds; chronological ordering is correct for all tested data sets.

**Constraints**: Exactly one active modal; task and subtask modal outer geometry must match at each viewport; all content remains vertically reachable without horizontal scrolling; due dates remain date-only in the UI without timezone drift; subtask/comment author and timestamps are read-only; comment mutation is author-only; hard/cascade deletion and existing access/assignment rules remain unchanged; light-theme tokens only.

**Scale/Scope**: Personal/family scale; one board page, two launch surfaces, one new modal component, existing task/subtask/comment APIs, four domain entities, and focused changes across App, Api, Web, and existing .NET test projects.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

| # | Principle | Pre-Design | Post-Design | Notes |
|---|-----------|------------|-------------|-------|
| I | Type-Safe Full-Stack Architecture | PASS | PASS | C# DTOs and mirrored strict TypeScript interfaces describe every changed request/response; .NET 10 and React + TypeScript remain unchanged. |
| II | Test Discipline (NON-NEGOTIABLE) | PASS | PASS | Existing xUnit projects cover new service/controller behavior and regressions; all .NET tests run with the existing 80% aggregate line-coverage target. No frontend test framework is introduced; TypeScript, lint, and browser scenarios validate the UI. |
| III | Responsive, Cross-Device Experience | PASS | PASS | The subtask modal reuses the task modal's full-screen mobile and centered desktop geometry, touch-friendly controls, and scrollable content region; quickstart covers both widths. |
| IV | Modern Browser Baseline | PASS | PASS | Existing Vite target and semantic HTML rely only on current browser behavior; no legacy polyfill is added. |
| V | Lightweight, Reliable Persistence | PASS | PASS | All reads and writes stay in EF Core services over SQLite; no raw SQL or schema change is introduced. |
| VI | Light-Theme User Interface | PASS | PASS | New UI uses existing `stride-*` light-theme tokens and does not add alternate theme values. |

**Gate Result**: All constitutional gates pass before and after design. No clarification or exception remains.

## Project Structure

### Documentation (this feature)

```text
specs/003-subtask-modal-dialog/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── openapi.yaml
└── tasks.md                 # Existing Phase 2 artifact; updated by /speckit.tasks, not this command
```

### Source Code (repository root)

```text
src/
├── ZSLabs.Stride.Domain/
│   └── Entities/            # Existing Subtask/Comment/User relationships; no schema change
├── ZSLabs.Stride.Persistence/
│   └── Configurations/      # Existing cascade and relationship configuration; no migration
├── ZSLabs.Stride.App/
│   └── Services/            # Single-subtask query, graph loading, ordering, ownership enforcement
├── ZSLabs.Stride.Api/
│   ├── Contracts/           # AuthorUsername additions to Subtask and Comment responses
│   └── Controllers/         # GET subtask plus enriched subtask/comment mappings
└── ZSLabs.Stride.Web/
    └── src/
        ├── api/             # Typed get/create/update/delete subtask and comment calls
        ├── components/      # New SubtaskModal; TaskModal table; TaskCard links
        ├── hooks/           # Board refresh and focused subtask operations
        ├── pages/           # Single task/subtask modal-state coordinator
        └── utils/           # Shared date-only and local timestamp formatting

tests/
├── ZSLabs.Stride.App.Tests/
│   └── Services/            # Authorized graph loading, chronological order, comment ownership
└── ZSLabs.Stride.Api.Tests/
    └── Controllers/         # GET/DTO mappings and unauthorized/not-found responses
```

**Structure Decision**: Keep the existing layered `ZSLabs.Stride.*` solution. Domain entities remain data-only, persistence remains EF Core, application services own access and ordering rules, controllers own HTTP/DTO mapping, and `SpaceBoardPage` remains the frontend state owner. This is a focused extension of existing projects, not a new project or framework.

## Complexity Tracking

No constitutional violations or additional architectural complexity require justification.

## Phase 0 & 1 Artifacts

- [research.md](./research.md) - resolved technical and interaction decisions.
- [data-model.md](./data-model.md) - existing persisted entities plus client-only modal state and transitions.
- [contracts/openapi.yaml](./contracts/openapi.yaml) - feature-level REST contract for subtask detail and comment workflows.
- [quickstart.md](./quickstart.md) - automated, static, and browser validation scenarios.

---
description: "Task list for Task Planning Application implementation"
---

# Tasks: Task Planning Application

**Input**: Design documents from `/specs/001-task-planning-app/`

**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/openapi.yaml

**Tests**: Test tasks ARE included. The project constitution requires automated unit tests written alongside implementation (xUnit.v3 + NSubstitute), and aggregate unit-test line coverage MUST be ≥ 80%. Tests do not have to be written first, but each service/controller implementation and its automated tests must be completed as one implementation slice before moving to unrelated story work.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to (US1–US6)
- Backend projects use the `ZSLabs.Stride.` prefix; source under `src/`, tests under `tests/`

## Path Conventions

- **Backend**: `src/ZSLabs.Stride.Domain`, `src/ZSLabs.Stride.Persistence`, `src/ZSLabs.Stride.App`, `src/ZSLabs.Stride.Api`
- **Frontend**: `src/ZSLabs.Stride.Web`
- **Tests**: `tests/ZSLabs.Stride.Domain.Tests`, `tests/ZSLabs.Stride.App.Tests`, `tests/ZSLabs.Stride.Persistence.Tests`, `tests/ZSLabs.Stride.Api.Tests`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Solution, project scaffolding, and toolchain

- [X] T001 Create solution file `Stride.sln` at repo root and the `src/` and `tests/` folder layout per plan.md
- [X] T002 [P] Create `src/ZSLabs.Stride.Domain/ZSLabs.Stride.Domain.csproj` (.NET 10 class library, nullable reference types enabled)
- [X] T003 [P] Create `src/ZSLabs.Stride.Persistence/ZSLabs.Stride.Persistence.csproj` (.NET 10) referencing Domain, with latest-stable `Microsoft.EntityFrameworkCore.Sqlite` and `Microsoft.EntityFrameworkCore.Design` packages
- [X] T004 [P] Create `src/ZSLabs.Stride.App/ZSLabs.Stride.App.csproj` (.NET 10) referencing Domain and Persistence, with latest-stable `Microsoft.Extensions.Identity.Core` package (provides `PasswordHasher<TUser>`)
- [X] T005 [P] Create `src/ZSLabs.Stride.Api/ZSLabs.Stride.Api.csproj` (ASP.NET Core web project, .NET 10) referencing App
- [X] T006 [P] Create the four xUnit.v3 + NSubstitute test projects under `tests/` (`ZSLabs.Stride.Domain.Tests`, `ZSLabs.Stride.App.Tests`, `ZSLabs.Stride.Persistence.Tests`, `ZSLabs.Stride.Api.Tests`) with latest-stable `xunit.v3`, `NSubstitute`, and coverage collector packages, each referencing its production project; add all `src` and `tests` projects to the solution
- [X] T007 [P] Scaffold the frontend `src/ZSLabs.Stride.Web` as a Vite + React 19 + TypeScript 6 (strict) app; add latest-stable-within-major `vite@^8`, `tailwindcss@^4`, `@tailwindcss/vite@^4`, and `@dnd-kit/*`; pin resolved versions in `package.json`
- [X] T008 [P] Configure `.editorconfig`, backend analyzers, and coverage collection in test projects (≥ 80% aggregate line coverage); configure Tailwind v4 (register `@tailwindcss/vite` in `vite.config.ts`, CSS-first `@import "tailwindcss"` with light-theme `@theme` tokens) and TypeScript strict/ESLint in `src/ZSLabs.Stride.Web`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared domain model, persistence, auth framework, and app shells that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Domain (enums + entities)

- [X] T009 [P] Create enums `UserRole`, `TaskStatus`, `TaskPriority`, `SubtaskStatus` in `src/ZSLabs.Stride.Domain/Enums/`
- [X] T010 [P] Create `User` entity in `src/ZSLabs.Stride.Domain/Entities/User.cs` (data + constructor only, no business logic)
- [X] T011 [P] Create `Space` entity in `src/ZSLabs.Stride.Domain/Entities/Space.cs`
- [X] T012 [P] Create `Task` entity in `src/ZSLabs.Stride.Domain/Entities/Task.cs`
- [X] T013 [P] Create `Subtask` entity in `src/ZSLabs.Stride.Domain/Entities/Subtask.cs`
- [X] T014 [P] Create `Comment` entity in `src/ZSLabs.Stride.Domain/Entities/Comment.cs` (exactly one of TaskId/SubtaskId set)

### Persistence (DbContext, configurations, migration, seed)

- [X] T015 Create `StrideDbContext` in `src/ZSLabs.Stride.Persistence/StrideDbContext.cs` with `DbSet`s for all entities
- [X] T016 [P] Create `IEntityTypeConfiguration<T>` for each entity in `src/ZSLabs.Stride.Persistence/Configurations/` (unique `User.Username`, unique `Space.Key`, `Comment` one-owner check constraint, cascade delete Space→Task→Subtask→Comment and Task→Comment)
- [X] T017 Add global UTC read-side `DateTime` convention (re-tag materialized values as `DateTimeKind.Utc`) in `StrideDbContext.ConfigureConventions`
- [X] T018 Create the initial EF Core migration in `src/ZSLabs.Stride.Persistence/Migrations/` and wire `DbContext` registration (SQLite connection) in the Persistence layer
- [X] T019 [P] Add `Persistence.Tests` verifying cascade delete chain, unique constraints, and the UTC read convention in `tests/ZSLabs.Stride.Persistence.Tests/`

### Backend cross-cutting infrastructure

- [X] T020 Register services, DbContext, cookie authentication scheme, CORS (allow `https://localhost:5173` with credentials), and authorization policies `AdminOnly` / `RegularOnly` in `src/ZSLabs.Stride.Api/Program.cs`
- [X] T021 [P] Add password hashing wrapper over `PasswordHasher<User>` in `src/ZSLabs.Stride.App/Services/PasswordHashingService.cs` (+ unit test in `tests/ZSLabs.Stride.App.Tests/`)
- [X] T022 [P] Add global error-handling / problem-details middleware and generic error contract in `src/ZSLabs.Stride.Api/`
- [X] T023 Add admin user seeding (single seeded `Admin`) on startup in `src/ZSLabs.Stride.Persistence/` (idempotent) and invoke from `Program.cs`

### Frontend foundation

- [X] T024 [P] Create typed API client base (fetch wrapper with credentials) and mirrored contract types in `src/ZSLabs.Stride.Web/src/api/`
- [X] T025 [P] Create shared design tokens + Tailwind entry stylesheet in `src/ZSLabs.Stride.Web/src/styles/`
- [X] T026 Create app router shell, `useAuth` context skeleton, and role-based protected-route guard in `src/ZSLabs.Stride.Web/src/` (redirects unauthenticated users; separates admin vs. regular areas)
- [X] T027 [P] Add a local-time date formatting utility (UTC → viewer local via `Intl.DateTimeFormat`) in `src/ZSLabs.Stride.Web/src/utils/`

**Checkpoint**: Foundation ready — user story implementation can now begin

---

## Phase 3: User Story 1 - Sign in to the application (Priority: P1) 🎯 MVP

**Goal**: Admin and regular users sign in with username/password, land on the role-appropriate home, and can sign out; invalid credentials are rejected with a generic message.

**Independent Test**: Sign in with valid and invalid credentials for both an admin and a regular account; confirm each role lands on its correct home and that sign-out ends the session.

### Implementation and Tests for User Story 1

- [X] T028 [US1] Implement `AuthService` (validate credentials via password hasher, resolve current user) in `src/ZSLabs.Stride.App/Services/AuthService.cs`
- [X] T029 [P] [US1] `AuthService` unit tests (valid/invalid credentials, generic failure) in `tests/ZSLabs.Stride.App.Tests/AuthServiceTests.cs`
- [X] T030 [P] [US1] Add auth DTOs `LoginRequest` and `CurrentUser` in `src/ZSLabs.Stride.Api/Contracts/`
- [X] T031 [US1] Implement `AuthController` (`POST /auth/login`, `POST /auth/logout`, `GET /auth/me`) with cookie sign-in and generic 401 in `src/ZSLabs.Stride.Api/Controllers/AuthController.cs`
- [X] T032 [P] [US1] `AuthController` unit tests (login/logout/me, 401 generic) in `tests/ZSLabs.Stride.Api.Tests/AuthControllerTests.cs`
- [X] T033 [P] [US1] Implement `SignIn` page (username/password form, generic error display) in `src/ZSLabs.Stride.Web/src/pages/SignIn.tsx`
- [X] T034 [US1] Wire `useAuth` (login/logout/me), role-based home routing (admin → users, regular → spaces), and sign-out in `src/ZSLabs.Stride.Web/src/hooks/useAuth.ts` and router

**Checkpoint**: User Story 1 fully functional and independently testable

---

## Phase 4: User Story 2 - Admin manages regular user accounts (Priority: P1)

**Goal**: The seeded admin creates and updates regular users (unique username enforced, optional email); regular users are denied access to user management, and admin has no task/space access.

**Independent Test**: Sign in as admin, create a regular user, edit its details, verify the new user can sign in, and confirm a regular user cannot reach user management.

### Implementation and Tests for User Story 2

- [X] T035 [US2] Implement `UserService` (create/update regular users, enforce unique username, hash passwords) in `src/ZSLabs.Stride.App/Services/UserService.cs`
- [X] T036 [P] [US2] `UserService` unit tests (create, update, duplicate-username rejection) in `tests/ZSLabs.Stride.App.Tests/UserServiceTests.cs`
- [X] T037 [P] [US2] Add user DTOs `User`, `CreateUserRequest`, `UpdateUserRequest` in `src/ZSLabs.Stride.Api/Contracts/`
- [X] T038 [US2] Implement `UsersController` (`GET/POST /users`, `PUT /users/{id}`) guarded by `AdminOnly` policy in `src/ZSLabs.Stride.Api/Controllers/UsersController.cs`
- [X] T039 [P] [US2] `UsersController` unit tests (admin-only 403, 409 on duplicate) in `tests/ZSLabs.Stride.Api.Tests/UsersControllerTests.cs`
- [X] T040 [P] [US2] Implement `AdminUsers` page (list, create, edit users) in `src/ZSLabs.Stride.Web/src/pages/AdminUsers.tsx` with API client methods in `src/ZSLabs.Stride.Web/src/api/`

**Checkpoint**: User Stories 1 and 2 both work independently

---

## Phase 5: User Story 3 - Create and manage spaces (Priority: P1)

**Goal**: Regular users create spaces (unique key, name, public/private), see own + public spaces, update fields, delete spaces (cascade), with author-only control of the Public flag and private-space visibility restrictions.

**Independent Test**: Sign in as a regular user, create a private and a public space, edit them, verify visibility from another account, confirm only the author can toggle Public, and delete a space.

### Implementation and Tests for User Story 3

- [X] T041 [US3] Implement `SpaceService` (create with unique key, list visible, get, update with author-only Public flag rule, cascade delete) in `src/ZSLabs.Stride.App/Services/SpaceService.cs`
- [X] T042 [P] [US3] `SpaceService` unit tests (unique key, visibility filtering, author-only Public flag, cascade delete) in `tests/ZSLabs.Stride.App.Tests/SpaceServiceTests.cs`
- [X] T043 [P] [US3] Add space DTOs `Space`, `CreateSpaceRequest`, `UpdateSpaceRequest` in `src/ZSLabs.Stride.Api/Contracts/`
- [X] T044 [US3] Implement `SpacesController` (`GET/POST /spaces`, `GET/PUT/DELETE /spaces/{id}`) with visibility/authorship enforcement in `src/ZSLabs.Stride.Api/Controllers/SpacesController.cs`
- [X] T045 [P] [US3] `SpacesController` unit tests (403 on private no-access, 409 duplicate key, author-only visibility change) in `tests/ZSLabs.Stride.Api.Tests/SpacesControllerTests.cs`
- [X] T046 [P] [US3] Implement `Spaces` page (list with own/public, create, edit, delete, empty state) and `useSpaces` hook in `src/ZSLabs.Stride.Web/src/pages/Spaces.tsx` and `src/ZSLabs.Stride.Web/src/hooks/`

**Checkpoint**: All P1 stories (US1–US3) independently functional — core MVP complete

---

## Phase 6: User Story 4 - Organize tasks on a board (Priority: P2)

**Goal**: Within a space board, users create tasks (default Backlog), see them in five always-visible status columns ordered by priority then creation date, edit tasks, and drag a task between columns to change and persist its status.

**Independent Test**: Open a space board, create tasks with varied priorities/statuses, confirm correct column placement and ordering, and drag a task to a new column to change its status (persists across reload).

### Implementation and Tests for User Story 4

- [X] T047 [US4] Implement `TaskService` (create default Backlog, list ordered by priority then CreatedAt within status, update, change status, assignee access check, cascade delete, last-write-wins `UpdatedAt`) in `src/ZSLabs.Stride.App/Services/TaskService.cs`
- [X] T048 [P] [US4] `TaskService` unit tests (default Backlog, priority-then-date ordering, status change, assignee access validation, cascade delete) in `tests/ZSLabs.Stride.App.Tests/TaskServiceTests.cs`
- [X] T049 [P] [US4] Add task DTOs `Task`, `CreateTaskRequest`, `UpdateTaskRequest`, and `TaskStatus`/`TaskPriority` contract types in `src/ZSLabs.Stride.Api/Contracts/`
- [X] T050 [US4] Implement `TasksController` (`GET/POST /spaces/{spaceId}/tasks`, `PUT/DELETE /tasks/{id}`, `PATCH /tasks/{id}/status`) in `src/ZSLabs.Stride.Api/Controllers/TasksController.cs`
- [X] T051 [P] [US4] `TasksController` unit tests (create/list/update/status/delete, 403 no-access) in `tests/ZSLabs.Stride.Api.Tests/TasksControllerTests.cs`
- [X] T052 [P] [US4] Implement `Board`, `Column`, and `TaskCard` components (five always-visible columns) in `src/ZSLabs.Stride.Web/src/components/`
- [X] T053 [US4] Implement `SpaceBoard` page with dnd-kit drag-and-drop wiring status change to `PATCH /tasks/{id}/status`, plus task create/edit UI and `useTasks` hook in `src/ZSLabs.Stride.Web/src/pages/SpaceBoard.tsx`

**Checkpoint**: US4 works on top of US1–US3

---

## Phase 7: User Story 5 - Manage subtasks under a task (Priority: P2)

**Goal**: Users add subtasks (Todo/InProgress/Done) to a task, update and delete them, with assignee access rules; subtasks render as a list beneath the parent task.

**Independent Test**: Open a task, add multiple subtasks, update and delete them, and confirm they render as a list under the parent task on the board.

### Implementation and Tests for User Story 5

- [X] T054 [US5] Implement `SubtaskService` (create/update/delete under a task, assignee access check, cascade delete comments) in `src/ZSLabs.Stride.App/Services/SubtaskService.cs`
- [X] T055 [P] [US5] `SubtaskService` unit tests (create/update/delete, status values, assignee access validation, cascade delete of comments) in `tests/ZSLabs.Stride.App.Tests/SubtaskServiceTests.cs`
- [X] T056 [P] [US5] Add subtask DTOs `Subtask`, `CreateSubtaskRequest`, `UpdateSubtaskRequest`, and `SubtaskStatus` contract type in `src/ZSLabs.Stride.Api/Contracts/`
- [X] T057 [US5] Implement `SubtasksController` (`POST /tasks/{taskId}/subtasks`, `PUT/DELETE /subtasks/{id}`) in `src/ZSLabs.Stride.Api/Controllers/SubtasksController.cs`
- [X] T058 [P] [US5] `SubtasksController` unit tests (create/update/delete, 403 no-access) in `tests/ZSLabs.Stride.Api.Tests/SubtasksControllerTests.cs`
- [X] T059 [P] [US5] Implement `SubtaskList` component (list under parent task, add/edit/delete) in `src/ZSLabs.Stride.Web/src/components/SubtaskList.tsx`

**Checkpoint**: US5 works on top of US4

---

## Phase 8: User Story 6 - Comment on tasks and subtasks (Priority: P3)

**Goal**: Users add comments to tasks and subtasks (sorted ascending by creation date) and can edit/delete only their own comments.

**Independent Test**: Add comments to a task and a subtask, confirm ascending creation-date order, and verify a user can edit/delete only their own comments.

### Implementation and Tests for User Story 6

- [X] T060 [US6] Implement `CommentService` (add to task/subtask, author-only edit/delete, return sorted ascending by CreatedAt) in `src/ZSLabs.Stride.App/Services/CommentService.cs`
- [X] T061 [P] [US6] `CommentService` unit tests (add to task/subtask, author-only edit/delete, ascending order) in `tests/ZSLabs.Stride.App.Tests/CommentServiceTests.cs`
- [X] T062 [P] [US6] Add comment DTOs `Comment` and `CreateCommentRequest` in `src/ZSLabs.Stride.Api/Contracts/`
- [X] T063 [US6] Implement `CommentsController` (`POST /tasks/{taskId}/comments`, `POST /subtasks/{subtaskId}/comments`, `PUT/DELETE /comments/{id}`) in `src/ZSLabs.Stride.Api/Controllers/CommentsController.cs`
- [X] T064 [P] [US6] `CommentsController` unit tests (create, author-only 403 on edit/delete) in `tests/ZSLabs.Stride.Api.Tests/CommentsControllerTests.cs`
- [X] T065 [P] [US6] Implement `CommentList` component (ascending order, add, author-only edit/delete) in `src/ZSLabs.Stride.Web/src/components/CommentList.tsx`

**Checkpoint**: All user stories independently functional

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Quality, coverage, responsiveness, and final validation across all stories

- [X] T066 [P] Verify responsive board/space/user layouts at mobile and desktop widths (Tailwind breakpoints, touch targets) in `src/ZSLabs.Stride.Web/`
- [X] T067 [P] Confirm all displayed dates use the UTC→local utility across pages/components in `src/ZSLabs.Stride.Web/`
- [X] T068 Run all .NET unit tests and confirm ≥ 80% aggregate line coverage
- [X] T069 Remove unused code, using statements, NuGet packages, frontend dependencies, resources, and files across the solution
- [X] T070 Execute `quickstart.md` end-to-end validation (setup + full user journey)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories
- **User Stories (Phases 3–8)**: All depend on Foundational
  - US1 (sign in) is the auth foundation for every other story; build it first
  - US2 needs US1 (admin must sign in to reach user management); US3 needs US1 + US2 (a regular user is created by the admin, then signs in). So the P1 stories form a chain US1 → US2 → US3 for end-to-end testing, not a parallel set
  - US4 (P2) builds on US3 (needs spaces); US5 (P2) builds on US4 (needs tasks); US6 (P3) builds on US4/US5 (comments target tasks/subtasks)
- **Polish (Phase 9)**: Depends on all targeted user stories being complete

### User Story Dependencies

- **US1 (P1)**: Foundational only — the authentication foundation; no dependency on other stories
- **US2 (P1)**: Depends on US1 — the admin must sign in before reaching user management; also provisions the regular users US3 needs
- **US3 (P1)**: Depends on US1 + US2 — requires an authenticated regular user account (created by the admin in US2)
- **US4 (P2)**: Requires spaces from US3 (board lives in a space)
- **US5 (P2)**: Requires tasks from US4
- **US6 (P3)**: Requires tasks (US4) and subtasks (US5) as comment targets

> **Shared-file coordination**: `src/ZSLabs.Stride.Api/Program.cs` (DI + endpoint registration), the frontend router/`useAuth` wiring (T026/T034), and the typed API client (T024) are each edited by multiple stories. Parallel work on separate stories must serialize edits to these files to avoid conflicts.

### Within Each User Story

- Tests do not need to be written first, but each service/controller implementation must be completed together with its adjacent unit-test task
- Service (App) + service tests → DTOs → Controller (Api) + controller tests → Frontend
- DTOs marked [P] are independent of the service and can be authored in parallel

### Parallel Opportunities

- All Setup tasks marked [P] (project scaffolds) can run in parallel after T001
- Foundational entities (T010–T014) and enums (T009) can run in parallel; configurations (T016) follow the DbContext (T015)
- Within a single story, DTO tasks can run in parallel with service work, and adjacent unit-test tasks can run as soon as the implementation unit they verify is available
- Across stories, parallelism is limited: the P1 stories form a US1 → US2 → US3 chain, and stories share `Program.cs`, the frontend router, and the API client (see Shared-file coordination). True cross-story parallelism is mostly confined to writing isolated service/controller code ahead of wiring it up

---

## Parallel Example: User Story 3

```text
# Implementation first (DTOs parallel to service):
Task: "Add space DTOs in src/ZSLabs.Stride.Api/Contracts/"
Task: "Implement SpaceService in src/ZSLabs.Stride.App/Services/SpaceService.cs"

# Pair each implementation unit with its tests before moving to unrelated work:
Task: "SpaceService unit tests in tests/ZSLabs.Stride.App.Tests/SpaceServiceTests.cs"
Task: "Implement SpacesController in src/ZSLabs.Stride.Api/Controllers/SpacesController.cs"
Task: "SpacesController unit tests in tests/ZSLabs.Stride.Api.Tests/SpacesControllerTests.cs"
```

---

## Implementation Strategy

### MVP First

The product is unusable without authentication, user provisioning, and spaces, so the MVP is **all three P1 stories**:

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phases 3–5: US1 (sign in), US2 (admin users), US3 (spaces)
4. **STOP and VALIDATE**: admin can create a user, that user signs in and manages spaces
5. Deploy/demo

> If a leaner first slice is needed, US1 alone (Phase 3) is the smallest independently testable increment.

### Incremental Delivery

1. Setup + Foundational → foundation ready
2. Add US1 → US2 → US3 (P1 MVP) → validate → demo
3. Add US4 (board) → validate → demo
4. Add US5 (subtasks) → validate → demo
5. Add US6 (comments) → validate → demo
6. Polish (Phase 9)

### Parallel Team Strategy

After Foundational completes, cross-story parallelism is limited by the US1 → US2 → US3 auth chain and shared files (`Program.cs`, router, API client). A workable split: one developer drives the P1 chain (US1 → US2 → US3) while another prepares isolated US4/US5/US6 service and controller code (App/Api) that is wired up once its prerequisite story lands. Merges into the shared files should be serialized.

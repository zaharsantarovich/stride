---
description: "Task list for Task Modal Editing implementation"
---

# Tasks: Task Modal Editing

**Input**: Design documents from `/specs/002-add-edit-task-dialog/`

**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/openapi.yaml, quickstart.md

**Tests**: Backend unit tests ARE included because assignment eligibility and rejection paths are required by the feature and the constitution requires automated unit tests with aggregate unit-test line coverage >= 80%. Frontend unit tests MUST NOT be added for this feature; frontend validation uses lint, build, and manual checks in the VS Code built-in browser.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story. Per user instruction, tasks are intentionally sequential: no task is marked `[P]`, and no task should be executed in parallel.

## Format: `[ID] [Story] Description`

- **[Story]**: Which user story this task belongs to (US1-US4); setup, foundational, and polish tasks have no story label
- **Sequential only**: Do not add `[P]` markers and do not parallelize implementation or verification tasks
- Include exact file paths in every task description

## Path Conventions

- **Backend**: `src/ZSLabs.Stride.App`, `src/ZSLabs.Stride.Api`, `src/ZSLabs.Stride.Domain`, `src/ZSLabs.Stride.Persistence`
- **Frontend**: `src/ZSLabs.Stride.Web`
- **Tests**: `tests/ZSLabs.Stride.App.Tests`, `tests/ZSLabs.Stride.Api.Tests`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare the feature branch and identify the existing implementation points before changing behavior

- [x] T001 Review the feature design documents in `specs/002-add-edit-task-dialog/plan.md`, `specs/002-add-edit-task-dialog/spec.md`, `specs/002-add-edit-task-dialog/data-model.md`, `specs/002-add-edit-task-dialog/contracts/openapi.yaml`, and `specs/002-add-edit-task-dialog/quickstart.md`
- [x] T002 Inspect existing task, subtask, user, and board implementation points in `src/ZSLabs.Stride.App/Services/TaskService.cs`, `src/ZSLabs.Stride.App/Services/SubtaskService.cs`, `src/ZSLabs.Stride.App/Services/UserService.cs`, `src/ZSLabs.Stride.Api/Controllers/TasksController.cs`, `src/ZSLabs.Stride.Api/Controllers/SubtasksController.cs`, `src/ZSLabs.Stride.Api/Controllers/UsersController.cs`, `src/ZSLabs.Stride.Web/src/pages/SpaceBoard.tsx`, `src/ZSLabs.Stride.Web/src/components/Board.tsx`, `src/ZSLabs.Stride.Web/src/components/TaskCard.tsx`, `src/ZSLabs.Stride.Web/src/components/SubtaskList.tsx`, and `src/ZSLabs.Stride.Web/src/hooks/useTasks.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Add shared backend contracts, validation rules, and typed frontend API support needed by all modal stories

**CRITICAL**: No user story work can begin until this phase is complete

- [x] T003 Add `RegularUserLookup` response contract in `src/ZSLabs.Stride.Api/Contracts/RegularUserLookup.cs`
- [x] T004 Extend `IUserService` and `UserService` with regular-user lookup ordered by username in `src/ZSLabs.Stride.App/Services/IUserService.cs` and `src/ZSLabs.Stride.App/Services/UserService.cs`
- [x] T005 Add `UserService` unit tests for regular-user lookup excluding admin users and ordering by username in `tests/ZSLabs.Stride.App.Tests/UserServiceTests.cs`
- [x] T006 Add regular-user lookup action `GET /regular-users` for authenticated regular users in `src/ZSLabs.Stride.Api/Controllers/UsersController.cs`
- [x] T007 Add `UsersController` unit tests for `GET /regular-users` success, authenticated access, and admin exclusion behavior in `tests/ZSLabs.Stride.Api.Tests/UsersControllerTests.cs`
- [x] T008 Add backend assignee eligibility validation for task create/update in `src/ZSLabs.Stride.App/Services/TaskService.cs`
- [x] T009 Add backend assignee eligibility validation for subtask create/update in `src/ZSLabs.Stride.App/Services/SubtaskService.cs`
- [x] T010 Add task assignee eligibility unit tests covering null assignee, private-space current user, private-space other regular user rejection, public-space regular user, and admin rejection in `tests/ZSLabs.Stride.App.Tests/TaskServiceTests.cs`
- [x] T011 Add subtask assignee eligibility unit tests covering null assignee, private-space current user, private-space other regular user rejection, public-space regular user, and admin rejection in `tests/ZSLabs.Stride.App.Tests/SubtaskServiceTests.cs`
- [x] T012 Ensure task and subtask API save failures return modal-displayable error responses for ineligible assignees in `src/ZSLabs.Stride.Api/Controllers/TasksController.cs`, `src/ZSLabs.Stride.Api/Controllers/SubtasksController.cs`, and `src/ZSLabs.Stride.Api/Contracts/ErrorResponse.cs`
- [x] T013 Add typed frontend regular-user lookup and assignee-related task/subtask contract fields in `src/ZSLabs.Stride.Web/src/api/users.ts`, `src/ZSLabs.Stride.Web/src/api/tasks.ts`, and `src/ZSLabs.Stride.Web/src/api/contracts.ts`

**Checkpoint**: Backend eligibility, lookup contracts, and typed frontend API support are ready for modal UI work

---

## Phase 3: User Story 1 - Edit a Task from the Board (Priority: P1) MVP

**Goal**: A board user opens an existing task from the board, updates it in a modal dialog, and returns to the board with the updated read-only summary visible.

**Independent Test**: Open a task card, edit task fields and subtasks in the modal, save, and confirm the modal closes, the board summary updates, and no inline task/subtask editing controls appear on the board.

### Implementation and Tests for User Story 1

- [x] T014 [US1] Create shared task modal component with edit-mode draft state, save failure display, discard-on-dismiss behavior, and desktop/mobile layout classes in `src/ZSLabs.Stride.Web/src/components/TaskModal.tsx`
- [x] T015 [US1] Move editable subtask management from board-facing controls into modal draft rows in `src/ZSLabs.Stride.Web/src/components/TaskModal.tsx` and remove edit responsibilities from `src/ZSLabs.Stride.Web/src/components/SubtaskList.tsx`
- [x] T016 [US1] Update task state orchestration to save edited task fields and modal-managed subtasks without leaking unsaved draft state to the board in `src/ZSLabs.Stride.Web/src/hooks/useTasks.ts`
- [x] T017 [US1] Open the modal when a user selects an existing task card and close it after successful save or dismiss in `src/ZSLabs.Stride.Web/src/pages/SpaceBoard.tsx`
- [x] T018 [US1] Convert board task and subtask display to read-only interaction while preserving task-card selection and drag behavior in `src/ZSLabs.Stride.Web/src/components/Board.tsx`, `src/ZSLabs.Stride.Web/src/components/Column.tsx`, `src/ZSLabs.Stride.Web/src/components/TaskCard.tsx`, and `src/ZSLabs.Stride.Web/src/components/SubtaskList.tsx`

**Checkpoint**: User Story 1 is fully functional and independently testable

---

## Phase 4: User Story 2 - Create a Task from the Board Header (Priority: P2)

**Goal**: A board user clicks Create Task, enters task details in the same modal experience used for editing, saves, and sees the new task on the board.

**Independent Test**: Click Create Task, complete valid task details and subtasks in the modal, save, and confirm the new task appears without any old top-of-board create form.

### Implementation and Tests for User Story 2

- [x] T019 [US2] Add create-mode defaults, labels, and submit handling to the shared modal in `src/ZSLabs.Stride.Web/src/components/TaskModal.tsx`
- [x] T020 [US2] Replace the top-of-board inline task creation form with a Create Task button that opens the modal in `src/ZSLabs.Stride.Web/src/pages/SpaceBoard.tsx`
- [x] T021 [US2] Wire create-task API submission and board refresh/update behavior through `src/ZSLabs.Stride.Web/src/hooks/useTasks.ts` and `src/ZSLabs.Stride.Web/src/api/tasks.ts`

**Checkpoint**: User Stories 1 and 2 both work through the shared modal

---

## Phase 5: User Story 3 - Scan Task Summaries on the Board (Priority: P3)

**Goal**: A board user can scan task title, priority, assignee username, and all subtask titles directly on each read-only task card.

**Independent Test**: View tasks with priorities, assigned users, unassigned state, and subtasks; confirm the card shows the required summary information and does not allow inline editing.

### Implementation and Tests for User Story 3

- [x] T022 [US3] Ensure task list responses expose assignee username and subtask titles needed by the board summary in `src/ZSLabs.Stride.Api/Contracts/Task.cs`, `src/ZSLabs.Stride.Api/Contracts/Subtask.cs`, and `src/ZSLabs.Stride.Api/Controllers/TasksController.cs`
- [x] T023 [US3] Update frontend task contract types to include assignee username and subtask title summary data in `src/ZSLabs.Stride.Web/src/api/contracts.ts`
- [x] T024 [US3] Render title, priority, assigned username or clear unassigned state, and all subtask titles as compact read-only card content in `src/ZSLabs.Stride.Web/src/components/TaskCard.tsx`
- [x] T025 [US3] Preserve board/column scrolling for cards with many subtasks without adding inline controls in `src/ZSLabs.Stride.Web/src/components/Board.tsx`, `src/ZSLabs.Stride.Web/src/components/Column.tsx`, and `src/ZSLabs.Stride.Web/src/components/TaskCard.tsx`

**Checkpoint**: User Story 3 board summaries are readable and independently testable

---

## Phase 6: User Story 4 - Use the Modal Across Device Sizes (Priority: P4)

**Goal**: The task modal is usable on mobile and desktop, occupying the full screen below 768px and centered with constrained bounds at 768px and wider.

**Independent Test**: Open the modal below 768px and at desktop width, add enough subtask content to require vertical scrolling, and confirm all controls remain reachable without horizontal scrolling.

### Implementation and Tests for User Story 4

- [x] T026 [US4] Apply responsive modal sizing, centering, max 90vw, max 90vh, max width 960px, and scroll containment in `src/ZSLabs.Stride.Web/src/components/TaskModal.tsx`
- [x] T027 [US4] Ensure modal controls remain touch-usable and light-theme-token based across mobile and desktop widths in `src/ZSLabs.Stride.Web/src/components/TaskModal.tsx` and `src/ZSLabs.Stride.Web/src/styles/index.css`

**Checkpoint**: User Story 4 responsive modal behavior is independently testable

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Validate the complete feature, remove unused implementation leftovers, and run required browser checks

- [x] T028 Run all .NET tests and confirm aggregate unit-test line coverage remains >= 80% using `coverage.runsettings`
- [x] T029 Run frontend lint and production build validation in `src/ZSLabs.Stride.Web/package.json`
- [x] T030 Remove unused inline-editing code, unused using statements, unused frontend imports, unused NuGet packages, unused frontend dependencies, unused resources, and dead files from `src/ZSLabs.Stride.Web`, `src/ZSLabs.Stride.Api`, `src/ZSLabs.Stride.App`, `src/ZSLabs.Stride.Domain`, and `src/ZSLabs.Stride.Persistence`
- [x] T031 Start the API and frontend using the local development flow documented in `specs/002-add-edit-task-dialog/quickstart.md`
- [x] T032 In the VS Code built-in browser at `https://localhost:5173`, log in as regular user username `a` and password `a`, then verify editing an existing task through the modal updates the board summary while board task and subtask content remains read-only using `specs/002-add-edit-task-dialog/quickstart.md`
- [x] T033 In the VS Code built-in browser at `https://localhost:5173`, logged in as username `a` and password `a`, verify Create Task opens the shared modal, saves a new task, shows it on the board, and confirms the old top-of-board create form is absent using `specs/002-add-edit-task-dialog/quickstart.md`
- [x] T034 In the VS Code built-in browser at `https://localhost:5173`, logged in as username `a` and password `a`, verify public-space and private-space assignee selector behavior plus modal save failure preservation using `specs/002-add-edit-task-dialog/quickstart.md`
- [x] T035 In the VS Code built-in browser at `https://localhost:5173`, logged in as username `a` and password `a`, verify modal layout below 768px and at desktop width including vertical scrolling with many subtasks using `specs/002-add-edit-task-dialog/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; start immediately
- **Foundational (Phase 2)**: Depends on Setup; blocks all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational; MVP scope
- **User Story 2 (Phase 4)**: Depends on User Story 1 because it reuses the shared modal
- **User Story 3 (Phase 5)**: Depends on Foundational and should be completed after User Story 1 so the read-only card behavior remains aligned with modal opening
- **User Story 4 (Phase 6)**: Depends on User Story 1 because responsive behavior applies to the shared modal
- **Polish (Phase 7)**: Depends on all targeted user stories being complete

### User Story Dependencies

- **US1 (P1)**: Foundational only; delivers the MVP edit modal and read-only board behavior
- **US2 (P2)**: Depends on US1; create mode extends the shared modal
- **US3 (P3)**: Depends on Foundational and coordinates with US1; summary data and read-only card rendering must preserve task-card selection
- **US4 (P4)**: Depends on US1; responsive rules apply to the modal created for editing

### Within Each User Story

- Complete backend service validation and adjacent unit tests before relying on the frontend to filter assignees
- Complete typed API contracts before wiring modal controls to API calls
- Complete modal draft state before connecting board open/save/dismiss flows
- Validate each checkpoint before starting the next phase

### Parallel Opportunities

None. Per user instruction, do not parallelize tasks. Execute tasks sequentially in task ID order.

---

## Sequential Execution Examples

### User Story 1

```text
Task: "Create shared task modal component with edit-mode draft state in src/ZSLabs.Stride.Web/src/components/TaskModal.tsx"
Task: "Move editable subtask management into modal draft rows in src/ZSLabs.Stride.Web/src/components/TaskModal.tsx"
Task: "Update task state orchestration in src/ZSLabs.Stride.Web/src/hooks/useTasks.ts"
Task: "Open the modal from existing task cards in src/ZSLabs.Stride.Web/src/pages/SpaceBoard.tsx"
Task: "Convert board task and subtask display to read-only content in src/ZSLabs.Stride.Web/src/components/TaskCard.tsx"
```

### User Story 2

```text
Task: "Add create-mode defaults to src/ZSLabs.Stride.Web/src/components/TaskModal.tsx"
Task: "Replace the inline task creation form in src/ZSLabs.Stride.Web/src/pages/SpaceBoard.tsx"
Task: "Wire create-task submission through src/ZSLabs.Stride.Web/src/hooks/useTasks.ts"
```

### User Story 3

```text
Task: "Expose assignee username and subtask titles through src/ZSLabs.Stride.Api/Contracts/Task.cs"
Task: "Update frontend task contracts in src/ZSLabs.Stride.Web/src/api/contracts.ts"
Task: "Render compact read-only card summaries in src/ZSLabs.Stride.Web/src/components/TaskCard.tsx"
```

### User Story 4

```text
Task: "Apply responsive modal sizing in src/ZSLabs.Stride.Web/src/components/TaskModal.tsx"
Task: "Verify touch-usable light-theme controls in src/ZSLabs.Stride.Web/src/components/TaskModal.tsx"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational validation and lookup support
3. Complete Phase 3: User Story 1 edit modal and read-only board behavior
4. Stop and validate User Story 1 independently

### Incremental Delivery

1. Complete Setup + Foundational
2. Add User Story 1 and validate edit modal behavior
3. Add User Story 2 and validate create modal behavior
4. Add User Story 3 and validate board summaries
5. Add User Story 4 and validate responsive modal behavior
6. Complete Polish tasks, including VS Code built-in browser validation as regular user `a` / `a`

### Sequential Team Strategy

1. Execute tasks in task ID order from T001 through T035
2. Do not split tasks across parallel implementers unless this task plan is revised
3. Validate each checkpoint before moving to the next phase

---

## Notes

- No `[P]` markers are used because the user requested no task parallelization
- No frontend unit tests should be added for this feature
- Browser verification must use the VS Code built-in browser, `https://localhost:5173`, and the regular user credentials username `a`, password `a`
- Backend validation remains authoritative for assignee eligibility; frontend filtering is usability support only
- Stop at any checkpoint to validate the current story independently
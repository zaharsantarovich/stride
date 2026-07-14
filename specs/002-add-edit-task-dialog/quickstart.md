# Quickstart: Task Modal Editing

## Prerequisites

- .NET 10 SDK installed.
- Node.js/npm compatible with the existing Vite frontend.
- Repository root as the working directory.
- HTTPS dev certificates available for the existing API/frontend local setup.

## Setup

```powershell
dotnet restore Stride.sln
Push-Location src/ZSLabs.Stride.Web
npm install
Pop-Location
```

## Automated Validation

Run backend tests, including assignment eligibility coverage for tasks and subtasks:

```powershell
dotnet test Stride.sln --settings coverage.runsettings --collect:"XPlat Code Coverage" --results-directory .coverage-results
```

Expected outcome:

- Task creation/update allows null assignees.
- In a private space, task and subtask creation/update accepts the current user as assignee and rejects any other regular user.
- In a public space, task and subtask creation/update accepts any regular user as assignee.
- Task and subtask creation/update rejects admin users as assignees.
- Aggregate unit-test line coverage remains at least 80%.

Run frontend static validation without adding frontend unit tests:

```powershell
Push-Location src/ZSLabs.Stride.Web
npm run lint
npm run build
Pop-Location
```

Expected outcome:

- TypeScript compiles in strict mode.
- ESLint passes.
- No frontend unit test dependency or frontend unit test file is added for this feature.

## Manual End-to-End Validation

Start the API and frontend using the repository's existing local development flow, then sign in as a regular user.

### Edit Existing Task

1. Open a space board with at least one task.
2. Select a task card.
3. Confirm the task modal opens with the selected task details and subtasks.
4. Change title, priority, assignee, and at least one subtask value.
5. Save.

Expected outcome:

- The modal closes.
- The board summary updates with the task title, priority, assignee username, and subtask titles.
- The board itself still has no inline task or subtask editing controls.

### Create Task

1. Select the Create Task button in the board header.
2. Confirm the same modal experience opens in create mode.
3. Enter valid task details and save.

Expected outcome:

- The modal closes.
- The new task appears on the board.
- The old top-of-board create form is not present.

### Assignment Eligibility

1. Call the generic regular-user lookup endpoint and confirm it returns regular users only.
2. In a private space, open the create/edit modal and inspect task and subtask assignee controls.
3. In a public space, open the create/edit modal and inspect task and subtask assignee controls.
4. Attempt direct API saves with an ineligible assignee id for both a task and subtask.

Expected outcome:

- `GET /regular-users` returns all regular users ordered by username and does not return admin users.
- Private-space selectors offer only unassigned and the current user.
- Public-space selectors offer unassigned and regular users.
- Admin users are not offered.
- Direct API saves with ineligible assignees fail and return an error suitable for display in the modal.

### Dismiss and Failure Behavior

1. Open a task modal, change fields, then cancel or dismiss the modal.
2. Reopen the task.
3. Trigger a save failure, such as by submitting an ineligible assignee through stale modal state.

Expected outcome:

- Cancel/dismiss discards unsaved changes immediately.
- Save failure leaves the modal open and keeps unsaved entries available for correction or retry.

### Responsive Behavior

1. Open the modal below 768px viewport width.
2. Open the modal at 768px or wider, including a representative desktop width.
3. Add enough subtask content to require vertical scrolling.

Expected outcome:

- Below 768px, the modal occupies the full screen and all controls are touch-usable.
- At 768px and wider, the modal is centered and constrained to max 90vw, max 90vh, and max width 960px.
- Required controls remain reachable without horizontal scrolling.
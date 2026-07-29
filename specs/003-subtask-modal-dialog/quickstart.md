# Quickstart: Dedicated Subtask Modal Dialog

## Prerequisites

- .NET 10 SDK.
- Node.js/npm compatible with the existing Vite application.
- Repository root as the working directory.
- Existing ASP.NET Core HTTPS development certificate setup.
- At least two regular-user accounts and one public space for ownership checks.

## Setup

```powershell
dotnet restore Stride.sln
Push-Location src/ZSLabs.Stride.Web
npm install
Pop-Location
```

Apply existing migrations and start the API using the repository's normal development configuration:

```powershell
dotnet run --project src/ZSLabs.Stride.Api/ZSLabs.Stride.Api.csproj
```

In another terminal, start the frontend:

```powershell
Push-Location src/ZSLabs.Stride.Web
npm run dev
```

Open the HTTPS URL printed by Vite and sign in as a regular user.

## Automated Validation

Run all .NET tests because C# and API contracts change:

```powershell
dotnet test Stride.sln --settings coverage.runsettings --collect:"XPlat Code Coverage" --results-directory .coverage-results
```

Expected outcome:

- Single-subtask retrieval validates parent-space access and returns the complete graph.
- Subtask and comment responses include author usernames.
- Subtasks and comments are ordered only by creation time ascending, without an ID or other secondary sort key.
- Comment update/delete rejects non-authors and users who no longer have parent-space access.
- Subtask delete preserves existing hard/cascade behavior.
- Controller tests cover success, not-found, conflict, and forbidden responses for changed endpoints.
- All tests pass and aggregate unit-test line coverage remains at least 80%.

Run existing frontend static validation. This feature adds no frontend unit-test framework, packages, or test files.

```powershell
Push-Location src/ZSLabs.Stride.Web
npm run lint
npm run build
Pop-Location
```

Expected outcome:

- ESLint passes.
- TypeScript compiles in strict mode.
- Vite produces the production bundle.
- The frontend dependency list remains unchanged.

Validate plan artifacts:

```powershell
python -c "import pathlib, yaml; yaml.safe_load(pathlib.Path('specs/003-subtask-modal-dialog/contracts/openapi.yaml').read_text(encoding='utf-8')); print('OpenAPI YAML parsed successfully.')"
git diff --check
```

## API Contract Smoke Checks

Use the authenticated browser session or an API client carrying the `Stride.Auth` cookie. Request and response shapes are defined in [contracts/openapi.yaml](./contracts/openapi.yaml).

1. Call `GET /subtasks/{subtaskId}` for an accessible subtask.
2. Call it for a missing subtask and for a subtask in an inaccessible private space.
3. Create a subtask with `POST /tasks/{taskId}/subtasks`.
4. Add a comment with `POST /subtasks/{subtaskId}/comments`.
5. Edit and delete an owned comment, then attempt both operations as another user.
6. Delete a subtask and confirm a later GET returns not found.

Expected outcome:

- Successful subtask responses include `authorUsername`, optional `assigneeUsername`, and ordered comments containing `authorUsername`.
- Missing resources return `404`; inaccessible resources and unauthorized comment mutations return `403`.
- Create/update responses contain generated audit values required to update the still-open modal.
- Subtask deletion returns `204` and removes its comments.

## Browser Validation

Prepare a parent task containing at least five subtasks with varied statuses, assignees, and creation times. Add comments from both regular users.

### Open From Both Surfaces

1. On a task card, confirm each subtask title looks and behaves like a URL link.
2. Activate a subtask link with pointer input.
3. Close it, open the parent task dialog, and confirm its subtask section is a Title/Status/Assignee table with an empty state when applicable.
4. Activate the same subtask from the table.
5. Repeat both activations with keyboard navigation and Enter.

Expected outcome:

- Each link opens the exact selected subtask in the same dedicated modal.
- Link activation does not also open the task card or start dragging.
- The task and subtask dialogs replace one another; they never stack.
- Links have visible focus and a clear accessible name.

### View and Edit

1. Confirm the modal shows title, status, author username, assignee, due date, created at, updated at, description, and comments.
2. Confirm author and audit timestamps are read-only.
3. Confirm due date is a calendar date without a time field.
4. Edit every editable field and save.

Expected outcome:

- The modal remains open after save and shows returned persisted values.
- Board-card and parent-table summaries refresh without another manual reload.
- Reopening the subtask shows the saved values.
- A failed save leaves entered values available for correction and retry.

### Create and Comment Gating

1. Open an existing parent task and select Add Subtask.
2. Confirm the dedicated modal opens in create mode with the parent established.
3. Confirm the comments section is empty and comment submission is visible but disabled.
4. Enter valid subtask fields and save.
5. Without closing the modal, add a comment.

Expected outcome:

- The first save creates the subtask under the expected task and leaves the modal open in edit mode.
- Generated ID-dependent values, author, and timestamps appear after save.
- Comment submission becomes enabled only after successful creation.
- The new subtask appears in both chronological summary displays.

### Comment Ordering and Ownership

1. Read the comment list from top to bottom.
2. Confirm each row shows content, author username, created at, and updated at or `Not updated`.
3. Add a comment, edit it, and delete it as its author.
4. Sign in as the second regular user and open the same subtask.
5. Inspect the first user's comments and attempt direct API edit/delete requests.

Expected outcome:

- Fields appear first, followed by comments, followed by new-comment controls.
- Comments remain earliest-to-latest after every mutation.
- Only the author's comments expose edit/delete controls.
- Direct non-author requests return `403` and do not change data.

### Parent Navigation

1. Open a subtask directly from a board card and activate the labeled parent-task control.
2. Open a subtask from the task-dialog table and repeat.

Expected outcome:

- The correct refreshed parent task dialog replaces the subtask modal in both cases.
- No board search or second active modal is required.

### Delete and Launch-Surface Restoration

1. Open an existing subtask from the board, select Delete, and cancel confirmation.
2. Repeat and confirm deletion.
3. Open another subtask from the task-dialog table and confirm deletion.
4. Create a new subtask from the task dialog, then delete it while the modal remains open.

Expected outcome:

- Cancel leaves the subtask and comments unchanged.
- Board-origin deletion returns to a refreshed board.
- Task-origin and newly-created deletion return to the refreshed parent task dialog.
- Deleted subtask rows/links and child comments are gone.
- A delete failure keeps the modal open with a clear error.

### Ordering

1. Compare the parent task's card links and task-dialog table.
2. Change subtask status and assignee values, then refresh.
3. Add a new subtask and refresh again.

Expected outcome:

- Both surfaces use earliest-to-latest creation order only.
- Status/assignee changes do not regroup or reorder existing subtasks.
- The newly created subtask appears at its chronological position.

### Responsive Geometry

Validate at minimum at `390x844` and `1440x900`; also inspect current Chrome, Edge, Firefox, and Safari before approval.

1. Open a task modal and record its outer top, left, width, and height.
2. Replace it with a subtask modal at the same viewport and compare boundaries.
3. Populate enough comments to overflow vertically.
4. Navigate through every field and action using touch-sized controls at the mobile width.

Expected outcome:

- Task and subtask modal outer boundaries are identical at each viewport.
- Mobile uses the existing full-screen modal geometry.
- Desktop uses the existing centered `90vw`/`90vh`, maximum `960px` geometry.
- Modal content scrolls vertically while header actions remain usable.
- Every field, comment, and control is reachable with no horizontal scrolling or overlap.

## Completion Criteria

- All .NET tests, coverage checks, lint, and frontend build pass.
- OpenAPI YAML parses and `git diff --check` is clean.
- Browser scenarios pass at representative mobile and desktop widths.
- No unused code, imports, packages, resources, migrations, or frontend test files remain.

# Data Model: Task Modal Editing

## Task

Represents a work item shown on the board and edited through the modal.

**Existing fields used by this feature**:

- `Id`: integer identifier.
- `SpaceId`: containing space identifier.
- `Title`: required task title.
- `Description`: optional details edited in the modal.
- `Status`: board column/status; can still be changed by existing board movement behavior and edited in the modal if exposed.
- `Priority`: required priority displayed on the card.
- `AuthorId`: regular user who created the task.
- `AssigneeId`: nullable assignee constrained by the containing space's assignment rules.
- `DueDate`: optional UTC timestamp.
- `CreatedAt`, `UpdatedAt`: UTC timestamps.
- `Subtasks`: child subtasks shown as read-only titles on the board and managed in the modal.

**Validation rules**:

- `Title` must be non-empty after trimming.
- `Priority` must be one of `Low`, `Medium`, `High`, or `Critical`.
- `Status` must be one of `Backlog`, `Todo`, `InProgress`, `Done`, or `Archived` when supplied.
- `AssigneeId` may be null for an unassigned task.
- In a private space, `AssigneeId` must be null or the current user's id.
- In a public space, `AssigneeId` must be null or the id of a regular user.
- Admin users are never valid task assignees.

## Subtask

Represents a smaller work item belonging to a task. Subtasks appear as read-only titles on the board and are created, edited, or deleted inside the task modal.

**Existing fields used by this feature**:

- `Id`: integer identifier.
- `TaskId`: parent task identifier.
- `Title`: required subtask title.
- `Description`: optional details edited in the modal.
- `Status`: subtask status.
- `AuthorId`: regular user who created the subtask.
- `AssigneeId`: nullable assignee constrained by the parent task's containing space assignment rules.
- `DueDate`: optional UTC timestamp.
- `CreatedAt`, `UpdatedAt`: UTC timestamps.

**Validation rules**:

- `Title` must be non-empty after trimming.
- `Status` must be one of `Todo`, `InProgress`, or `Done` when supplied.
- `AssigneeId` may be null for an unassigned subtask.
- Subtask assignee eligibility is evaluated against the parent task's containing space.
- In a private space, `AssigneeId` must be null or the current user's id.
- In a public space, `AssigneeId` must be null or the id of a regular user.
- Admin users are never valid subtask assignees.

## Space

Represents the board container whose visibility controls assignment eligibility.

**Existing fields used by this feature**:

- `Id`: integer identifier.
- `Name`: displayed in the board header.
- `AuthorId`: owner of the private space.
- `IsPublic`: public/private visibility flag.

**Assignment impact**:

- Private spaces allow only the current user to be assigned to tasks and subtasks.
- Public spaces allow any regular user to be assigned to tasks and subtasks.

## Regular User Lookup

Generic regular-user projection used by the modal to render public-space task and subtask assignee selectors.

**Fields**:

- `Id`: regular user identifier.
- `Username`: display name shown in assignee controls and board summaries.

**Lookup rules**:

- Only users with role `Regular` are returned.
- The lookup is not space-scoped and does not apply private-space filtering.
- For private spaces, the modal offers only the current signed-in user.
- For public spaces, the modal offers the regular users returned by the generic lookup.

## Task Modal Draft

Client-side state only; not persisted as a domain entity.

**Fields**:

- Task fields being edited: title, description, status, priority, assignee id, due date.
- Subtask draft rows: title, description, status, assignee id, due date, local pending/delete state as needed.
- Save state: idle, saving, failed.
- Error message: latest save failure message.

**State transitions**:

- `closed -> open-create`: user selects Create Task; draft starts with defaults.
- `closed -> open-edit`: user selects a task card; draft initializes from the selected task and its subtasks.
- `open-* -> saving`: user submits valid draft.
- `saving -> closed`: API save succeeds; board refreshes or updates in memory with returned task data.
- `saving -> open-*`: API save fails; draft values remain available for correction/retry.
- `open-* -> closed`: user cancels/dismisses; draft is discarded immediately.

## Relationships

- A `Space` has many `Task` records.
- A `Task` belongs to one `Space` and has many `Subtask` records.
- A `Task` has zero or one assignee.
- A `Subtask` has zero or one assignee.
- A `Regular User Lookup` item is derived from `User` records with role `Regular`.
- Effective assignment eligibility is computed from the selected `Space`, current actor, and selected assignee id during task/subtask save validation.
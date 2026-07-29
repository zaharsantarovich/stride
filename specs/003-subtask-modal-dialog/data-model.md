# Data Model: Dedicated Subtask Modal Dialog

## Overview

This feature uses the existing persisted model. No entity property, relationship, configuration, or EF Core migration is required. Changes are limited to loading existing navigation data, exposing it through typed response contracts, and adding client-only modal state.

## Task

Parent work item and launch context for subtask creation and navigation.

**Existing persisted fields used**:

- `Id`: integer identifier used as `Subtask.TaskId`.
- `SpaceId`: containing space used for access and assignment validation.
- `Title`: displayed as parent context and in the parent task dialog.
- `AuthorId`: creator identifier.
- `CreatedAt`, `UpdatedAt`: UTC audit timestamps.
- `Subtasks`: child collection returned in creation order for task-card and task-dialog summaries.

**Relationships**:

- Belongs to one `Space`.
- Has zero or more `Subtask` records.
- Deleting a task follows existing hard/cascade behavior for subtasks and comments.

**Feature rules**:

- The parent must already exist before a subtask can be created.
- Parent-task navigation resolves `Subtask.TaskId` against refreshed board task data.
- The task dialog orders its subtask table by `CreatedAt` ascending without a secondary sort key.

## Subtask

Child work item displayed and edited in the dedicated modal.

**Existing persisted fields**:

- `Id`: integer identifier generated at first successful save.
- `TaskId`: required parent task identifier; immutable through this feature.
- `Title`: required editable title.
- `Description`: optional editable text.
- `Status`: editable `Todo`, `InProgress`, or `Done` value.
- `AuthorId`: creator identifier; read-only after creation.
- `AssigneeId`: optional editable regular-user identifier.
- `DueDate`: optional UTC value represented as a date-only control in the UI.
- `CreatedAt`: required UTC creation timestamp; read-only.
- `UpdatedAt`: optional UTC last-update timestamp; read-only.
- `Author`: existing user navigation loaded to expose author username.
- `Assignee`: existing optional user navigation loaded to expose assignee username.
- `Comments`: child collection loaded in creation order.

**Validation rules**:

- `Title` is required and must remain non-empty after trimming.
- `Status` must be one of `Todo`, `InProgress`, or `Done`.
- `AssigneeId` may be null.
- For a private space, assignee must be null or the owning eligible regular user.
- For a public space, assignee must be null or an eligible regular user.
- Admin users are never eligible assignees.
- `DueDate`, `Description`, `AssigneeId`, and `UpdatedAt` may be null.
- Existing space access rules govern load, create, update, and delete.

**Ordering rule**:

- All task-card and task-dialog projections order subtasks only by `CreatedAt` ascending.
- Equal timestamps remain tied; `Id` or another field is not used to reorder them.

**Delete rule**:

- Deletion is permanent and requires UI confirmation.
- Existing EF Core cascade behavior deletes all child comments.

## Comment

Time-stamped discussion entry belonging to exactly one task or subtask. This feature displays and mutates subtask comments.

**Existing persisted fields**:

- `Id`: integer identifier.
- `TaskId`: null for a subtask comment.
- `SubtaskId`: required for a subtask comment.
- `AuthorId`: required creator identifier.
- `Content`: required editable text for the author.
- `CreatedAt`: required UTC creation timestamp; read-only.
- `UpdatedAt`: optional UTC last-update timestamp; read-only.
- `Author`: existing user navigation loaded to expose author username.

**Validation rules**:

- The existing database constraint requires exactly one of `TaskId` and `SubtaskId`.
- `Content` uses existing request/domain validation and must not be blank.
- Only `AuthorId` may update or delete the comment.
- Space access is required to view, add, update, or delete a subtask comment.
- API ownership and current parent-space access are checked again at mutation time even if the UI previously showed controls.
- A null `UpdatedAt` is displayed as `Not updated`.

**Ordering rule**:

- Comments are returned and displayed only by `CreatedAt` ascending.
- Equal timestamps remain tied; no secondary sort key is applied.

**Delete rule**:

- Author deletion is permanent.
- Parent subtask deletion cascades to all comments.

## User

Authenticated person represented in the modal by username.

**Existing fields used**:

- `Id`: compared with `Subtask.AuthorId`, `Subtask.AssigneeId`, and `Comment.AuthorId`.
- `Username`: displayed for subtask authors, assignees, and comment authors.
- `Role`: determines assignment eligibility; only regular users can be assigned.

**Feature rules**:

- Author usernames are response projection fields and are not duplicated in persisted subtasks/comments.
- Comment edit/delete controls are rendered only when the current user's ID equals `Comment.AuthorId`.

## API Projections

### Subtask Response

Existing subtask fields plus:

- `authorUsername`: required username loaded from `Subtask.Author`.
- `assigneeUsername`: optional username loaded from `Subtask.Assignee`.
- `comments`: chronologically ordered `Comment Response` collection.

### Comment Response

Existing comment fields plus:

- `authorUsername`: required username loaded from `Comment.Author`.

No request contract accepts author IDs, author usernames, created timestamps, or updated timestamps from the client.

## Client-Only Models

### Board Modal State

A discriminated union held by `SpaceBoardPage`; never persisted.

**Variants**:

- `closed`: no active modal.
- `task-create`: new parent task draft.
- `task-edit`: existing task ID/data.
- `subtask-create`: parent task ID, launch surface `task`, no subtask ID.
- `subtask-edit`: subtask ID, parent task ID, launch surface `board` or `task`.

Only one variant exists at a time, preventing modal stacking.

### Subtask Modal Draft

**Fields**:

- Mode: create or edit.
- Persisted ID: absent until first successful create.
- Parent task ID: fixed for the modal lifetime.
- Editable values: title, status, assignee ID, date-only due-date input, description.
- Read-only values: author username, created-at display, updated-at display.
- Ordered comments: response comments plus successful local mutations.
- New-comment content.
- Per-operation state: loading, saving subtask, deleting subtask, creating/editing/deleting comment.
- Recoverable error message.
- Launch surface: board or task.

## State Transitions

### Open and Load

- `closed -> subtask-edit/loading`: activate a subtask link.
- `task-edit -> subtask-edit/loading`: activate a subtask table link; the task modal is replaced.
- `subtask-edit/loading -> subtask-edit/ready`: authorized GET succeeds.
- `subtask-edit/loading -> recoverable-error`: request fails; show a clear message and a route back to the board.

### Create

- `task-edit -> subtask-create/ready`: activate Add Subtask with parent ID established.
- `subtask-create/ready -> subtask-create/saving`: submit valid fields.
- `subtask-create/saving -> subtask-edit/ready`: create succeeds; set generated ID/read-only fields, enable comments, refresh summaries, keep the modal open.
- `subtask-create/saving -> subtask-create/error`: create fails; preserve editable values and keep comment submission disabled.

### Update

- `subtask-edit/ready -> subtask-edit/saving`: submit changed valid fields.
- `subtask-edit/saving -> subtask-edit/ready`: update succeeds; replace draft/read-only values from response, refresh summaries, keep modal open.
- `subtask-edit/saving -> subtask-edit/error`: update fails; preserve edits for retry.

### Comments

- `subtask-create/*`: comment list remains empty and new-comment submission remains disabled until create succeeds.
- `subtask-edit/ready -> comment-mutating`: author adds, edits, or deletes an eligible comment.
- `comment-mutating -> subtask-edit/ready`: mutation succeeds; update and re-order comments.
- `comment-mutating -> subtask-edit/error`: mutation fails; retain recoverable content and show the API error.

### Parent Navigation and Delete

- `subtask-* -> task-edit`: select parent-task control; refresh tasks and replace the subtask modal with the matching parent task dialog.
- `subtask-edit -> delete-confirmation`: select Delete.
- `delete-confirmation -> subtask-edit`: cancel; no data changes.
- `delete-confirmation -> closed`: confirm board-launched deletion, delete succeeds, refresh board, then clear modal.
- `delete-confirmation -> task-edit`: confirm task-launched or newly-created deletion, delete succeeds, refresh tasks, then open the parent task dialog.
- `delete-confirmation -> subtask-edit/error`: delete fails; keep the subtask modal open.

### Dismiss

- `task-* | subtask-* -> closed`: existing dismissal behavior discards unsaved client state without confirmation.

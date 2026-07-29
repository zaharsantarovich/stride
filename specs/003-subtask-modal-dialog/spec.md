# Feature Specification: Dedicated Subtask Modal Dialog

**Feature Branch**: `003-subtask-modal-dialog`

**Created**: 2026-07-29

**Status**: Draft

**Input**: User description: "Move subtask creation, viewing, and editing into a separate modal dialog. Open it from URL-like subtask titles on task cards and in a task-dialog table, order subtasks and comments by creation date ascending, support comment ownership controls, provide parent-task navigation, and match the task dialog's size and position."

## Clarifications

### Session 2026-07-29

- Q: Where should users delete an existing subtask after embedded task-dialog editing is removed? → A: Delete from the subtask modal, with confirmation.
- Q: What should happen after a subtask is successfully created or updated? → A: Keep the subtask dialog open.
- Q: How should comment controls behave before a new subtask has been saved for the first time? → A: Show comments but disable adding until saved.
- Q: What should replace the subtask dialog after a confirmed deletion succeeds? → A: Return to the original launch surface.
- Q: What information should each comment display in the subtask dialog? → A: Content, author username, created at, and updated at.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Open and Manage a Subtask (Priority: P1)

A board user opens a subtask in a dedicated modal dialog, reviews all of its details, updates editable fields, and saves the changes without editing the subtask inside its parent task dialog.

**Why this priority**: A dedicated place to view and edit a subtask is the central outcome of this feature and the destination for every other subtask interaction.

**Independent Test**: Can be tested by opening an existing subtask, confirming every required field is shown with the correct editability, changing editable fields, saving, and confirming the updated values appear when it is reopened.

**Acceptance Scenarios**:

1. **Given** an existing subtask, **When** the user opens it, **Then** a dedicated subtask modal displays its title, status, author username, assignee, due date, creation time, update time, description, and comments.
2. **Given** an existing subtask is open, **When** the user changes valid editable fields and saves, **Then** the dialog remains open with the saved values and the changes are reflected anywhere that subtask is summarized.
3. **Given** an existing subtask is open, **When** the user views its author, creation time, or update time, **Then** those values cannot be edited.
4. **Given** a subtask has a due date, **When** it is viewed or edited, **Then** the due date is presented as a calendar date without a time component.
5. **Given** an existing subtask is open, **When** the user chooses Delete and confirms, **Then** the subtask and its comments are permanently deleted, the subtask is removed from its parent task's displays, and the refreshed board or parent task dialog from which the subtask was opened replaces the subtask dialog.

---

### User Story 2 - Reach a Subtask from Task Context (Priority: P1)

A board user recognizes subtask titles as links and opens the same dedicated subtask modal from either a task card or the parent task dialog.

**Why this priority**: The dedicated dialog has value only when users can discover and reach it quickly from both existing subtask surfaces.

**Independent Test**: Can be tested by opening the same subtask first from its board card and then from its task-dialog row, confirming both links open the same subtask details in the dedicated dialog.

**Acceptance Scenarios**:

1. **Given** a task card displays subtasks, **When** the user selects a URL-like subtask title, **Then** the dedicated modal opens for that exact subtask without opening the task dialog instead.
2. **Given** the task dialog displays subtasks, **When** the user selects a URL-like subtask title in the table, **Then** the same dedicated modal opens for that exact subtask.
3. **Given** a user navigates using a keyboard, **When** focus reaches a subtask title link and the user activates it, **Then** the corresponding subtask modal opens.

---

### User Story 3 - Add a Subtask from its Parent Task (Priority: P2)

A board user opens a task, chooses to add a subtask, and completes the new subtask in the dedicated modal dialog with the parent task already established.

**Why this priority**: Subtasks must still be created from a clear parent context after embedded subtask editing is removed.

**Independent Test**: Can be tested by opening an existing task, selecting Add Subtask, entering valid details in the dedicated dialog, saving, and confirming the new row and board link appear under the parent task.

**Acceptance Scenarios**:

1. **Given** an existing task dialog is open, **When** the user selects Add Subtask, **Then** the dedicated subtask modal opens in creation mode for that task.
2. **Given** the new-subtask modal contains valid values, **When** the user saves, **Then** the subtask is created under the expected parent, the dialog remains open in existing-subtask mode with generated values populated, and the subtask appears in the parent's ordered displays.
3. **Given** the task dialog is open, **When** the user reviews available actions, **Then** Add Subtask is available there and no other board-level subtask creation control is required.
4. **Given** a new subtask has not been saved, **When** the creation modal is open, **Then** it shows an empty comments section with comment submission disabled; after the first successful save, comment submission becomes available.

---

### User Story 4 - Review and Discuss a Subtask (Priority: P2)

A board user reads a subtask's conversation in chronological order, adds a comment, and manages only comments they authored.

**Why this priority**: Keeping discussion beside the subtask details provides the context needed to understand and update the work item.

**Independent Test**: Can be tested with comments from multiple users by confirming ascending chronological display, adding a comment, editing and deleting an owned comment, and confirming another user's comment has no usable edit or delete action.

**Acceptance Scenarios**:

1. **Given** a subtask has comments created at different times, **When** its modal opens, **Then** each comment displays its content, author username, creation time, and update time, and comments are ordered from earliest to latest creation time.
2. **Given** the subtask modal is open, **When** the user adds a valid comment, **Then** it appears in its chronological position in the comment list.
3. **Given** a comment was authored by the current user, **When** the user edits or deletes it, **Then** the requested change is applied.
4. **Given** a comment was authored by another user, **When** the current user views it or attempts to change it, **Then** edit and delete are unavailable and the change is rejected.
5. **Given** the modal is open, **When** the user reads from top to bottom, **Then** subtask fields appear first, followed by the comment list, followed by controls for adding a comment.

---

### User Story 5 - Return to the Parent Task (Priority: P3)

A board user viewing a subtask can clearly identify and return to its parent task without closing the workflow and searching the board.

**Why this priority**: Subtasks derive their context from a parent task, and direct return navigation prevents users from losing their place.

**Independent Test**: Can be tested by opening a subtask from either launch surface, selecting the parent-task navigation control, and confirming the correct parent task dialog opens.

**Acceptance Scenarios**:

1. **Given** a subtask modal is open, **When** the user selects the clearly labeled parent-task navigation control, **Then** the subtask modal is replaced by the dialog for that subtask's parent task.
2. **Given** a subtask was opened directly from a board card, **When** the user navigates to its parent, **Then** the correct task dialog opens without requiring a board search.

---

### User Story 6 - Use Consistent Dialog Geometry (Priority: P3)

A board user moves between task and subtask dialogs without a distracting change in modal size or position on either desktop or mobile.

**Why this priority**: Matching geometry makes parent-child navigation feel coherent and retains the task dialog's established cross-device usability.

**Independent Test**: Can be tested by opening task and subtask dialogs at representative mobile and desktop widths and comparing their outer size and position.

**Acceptance Scenarios**:

1. **Given** a task and its subtask are opened at the same viewport size, **When** their modal boundaries are compared, **Then** their outer size and position are identical.
2. **Given** a user opens the subtask modal on a mobile viewport, **When** its content exceeds the available height, **Then** all fields, comments, and controls remain reachable without horizontal scrolling.

### Edge Cases

- A task has no subtasks; its task dialog shows an empty table state while retaining the Add Subtask action.
- A task has many subtasks with different statuses or assignees; both displays remain ordered solely from earliest to latest creation time, without grouping or reordering by another field.
- Two subtasks have the same recorded creation time; their relative order remains stable and no secondary user-visible sort is applied.
- A subtask has no assignee, due date, description, comments, or recorded update time; the dialog presents clear empty states without treating optional values as errors.
- A user attempts to add a comment before the new subtask is saved; no comment is accepted or persisted.
- A comment has never been edited; its Updated at value uses a clear not-yet-updated state.
- A subtask or its parent task is deleted or becomes inaccessible before a link is activated; the user receives a clear message and remains able to return to the board.
- A user starts deleting a subtask but cancels the confirmation; the subtask and its comments remain unchanged.
- A comment is added while older comments already exist; the complete list remains ordered by creation time rather than simply placing the new item according to another property.
- The current user loses permission or comment ownership between opening the dialog and saving an edit or deletion; the action is rejected and the comment remains unchanged.
- A user attempts to leave a create or edit dialog with unsaved changes; existing modal dismissal behavior applies and unsaved changes are discarded.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide one dedicated modal dialog for creating, viewing, and editing subtasks.
- **FR-002**: The dedicated subtask modal MUST display Title, Status, Author username, Assignee, Due date, Created at, Updated at, Description, and the subtask's comments.
- **FR-003**: The subtask modal MUST allow Title, Status, Assignee, Due date, and Description to be entered or changed subject to existing validation and permissions.
- **FR-004**: The subtask modal MUST display Author username, Created at, and Updated at as read-only values.
- **FR-005**: The subtask modal MUST present Due date as a date component only, without a time-of-day component.
- **FR-006**: The subtask modal MUST lay out content in this order: subtask fields, comment list, then new-comment controls.
- **FR-007**: Each task card MUST display each of its subtask titles with the recognizable appearance and interaction behavior of a URL link.
- **FR-008**: Selecting a subtask title on a task card MUST open the dedicated modal for the selected subtask.
- **FR-009**: The task dialog MUST replace embedded subtask editing controls with a subtask table containing Title, Status, and Assignee columns.
- **FR-010**: Each subtask title in the task-dialog table MUST have the recognizable appearance and interaction behavior of a URL link.
- **FR-011**: Selecting a subtask title in the task-dialog table MUST open the same dedicated modal used from the board card for the selected subtask.
- **FR-012**: Subtask links MUST be operable by pointer, keyboard, and touch input and MUST expose a clear accessible name.
- **FR-013**: The board task card and task-dialog table MUST order subtasks solely by creation time in ascending order, from earliest to latest.
- **FR-014**: The task dialog MUST contain the action for adding a new subtask.
- **FR-015**: Selecting Add Subtask MUST open the dedicated modal in creation mode with the parent task established.
- **FR-016**: After a subtask is created or updated successfully, the subtask modal MUST remain open with the saved values; after creation it MUST transition to existing-subtask mode and populate generated read-only values, while all visible summaries reflect the saved values and required ordering.
- **FR-017**: The subtask modal MUST display comments solely by creation time in ascending order, from earliest to latest.
- **FR-017a**: Each displayed comment MUST show its content, author username, Created at, and Updated at values; author and timestamps MUST be read-only, and an absent Updated at value MUST use a clear not-yet-updated state.
- **FR-018**: The subtask modal MUST allow an eligible user to add a comment with existing comment validation applied.
- **FR-018a**: Before a new subtask's first successful save, the modal MUST show the empty comments section with comment submission disabled; after that save, it MUST enable comment submission in the still-open dialog.
- **FR-019**: The system MUST allow only a comment's author to edit or delete that comment.
- **FR-020**: Edit and delete controls MUST NOT be offered for comments authored by another user, and direct unauthorized attempts MUST be rejected.
- **FR-021**: The subtask modal MUST provide a clearly labeled, user-friendly control that opens the corresponding parent task dialog.
- **FR-022**: Opening the parent task from a subtask MUST replace the subtask dialog rather than stack two active modal dialogs.
- **FR-023**: At any given viewport size, the subtask dialog's outer size and position MUST be identical to the task dialog's outer size and position.
- **FR-024**: The subtask modal MUST keep all required content and controls reachable when its content exceeds the available vertical space.
- **FR-025**: For an existing subtask, the subtask modal MUST provide a Delete action that requires explicit confirmation before permanently deleting the subtask and its comments; after successful deletion, the refreshed launch surface MUST replace the subtask dialog, using the board for a board-launched subtask and the parent task dialog for a task-dialog-launched or newly created subtask.
- **FR-026**: The subtask modal MUST clearly communicate load and save failures while keeping recoverable unsaved entries available for correction or retry.
- **FR-027**: The system MUST preserve existing subtask field validation, status choices, assignment eligibility, hard-delete behavior, and access permissions except where this specification explicitly changes presentation or navigation.

### Key Entities *(include if feature involves data)*

- **Task**: The parent work item that owns zero or more subtasks and provides the context and launch action for creating them.
- **Subtask**: A child work item with a title, status, author, optional assignee, optional due date, creation time, optional update time, optional description, and ordered comments.
- **Comment**: A time-stamped note belonging to a subtask, with content and an author who exclusively owns its edit and delete permissions.
- **User**: An authenticated person represented by username who may author a subtask or comment and may be eligible for assignment according to the parent task's space.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: At least 95% of users in a usability test can open the intended subtask from either a task card or task dialog on their first attempt.
- **SC-002**: At least 95% of users can add a subtask from the parent task dialog and see it in both ordered subtask displays within 45 seconds.
- **SC-003**: In tests containing at least five subtasks with deliberately varied creation times, statuses, and assignees, 100% of board and task-dialog displays order them from earliest to latest creation time.
- **SC-004**: In tests containing comments from at least two users, 100% of comments appear from earliest to latest creation time, and unauthorized comment edits and deletions succeed 0% of the time.
- **SC-005**: At representative mobile and desktop viewport sizes, the task and subtask dialogs have identical outer boundaries, and 100% of required subtask content is reachable without horizontal scrolling.
- **SC-006**: At least 90% of users can return from a subtask dialog to the correct parent task dialog on their first attempt without searching the board.
- **SC-007**: At least 90% of users correctly distinguish editable subtask fields from read-only authorship and timestamp fields during usability review.

## Assumptions

- Existing task and subtask access permissions determine who may open, create, edit, or delete a subtask.
- Existing assignment rules remain unchanged: private spaces restrict assignment to the current eligible user, while public spaces permit eligible regular users; admin users are not assignable.
- Existing subtask statuses, field validation, hard-delete behavior, and timestamp display conventions remain unchanged.
- Opening a subtask from the task dialog transitions from the task dialog to the subtask dialog rather than stacking dialogs; the parent-task control provides the return path.
- The Add Subtask action applies to an already-created parent task. A task being created must be saved before subtasks can be added.
- Existing modal dismissal behavior applies: dismissing a modal discards unsaved changes without confirmation.
- An absent Updated at value is shown as a clear not-yet-updated state.
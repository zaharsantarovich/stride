# Feature Specification: Task Planning Application

**Feature Branch**: `001-task-planning-app`

**Created**: 2026-07-05

**Status**: Draft

**Input**: User description: "Build a task planning application for personal and family usage. Tasks grouped by spaces (private/public), each space has a single board with status columns, tasks with subtasks, comments, an admin-managed user model, hard deletes with cascade, and UTC storage with local-time display."

## Clarifications

### Session 2026-07-07

- Q: Is the board a distinct persisted entity, or just a view of a space's tasks? → A: Not a separate entity; the board is the page displayed when a user navigates to a space, rendering that space's tasks in status columns.

### Session 2026-07-05

- Q: How should tasks be ordered within each status column? → A: Automatic by priority (Critical → Low), then creation date; no manual within-column reordering.
- Q: Who can be set as the Assignee of a task or subtask? → A: Only users with access to the space — any registered user for public spaces, only the author for private spaces.
- Q: How should "Archived" tasks appear on the board? → A: As a dedicated, always-visible "Archived" column alongside the other status columns.
- Q: How are conflicting simultaneous edits to a shared public space resolved? → A: Last-write-wins — the most recent save overwrites earlier ones, with no conflict warning.
- Q: What status should a newly created task default to? → A: Backlog (users may change it during or after creation).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Sign in to the application (Priority: P1)

Both the single admin user and regular users open the application and sign in with a username and password so they can access the features permitted to their role.

**Why this priority**: Nothing in the product is usable without authentication. It is the entry point for every other capability and gates access by role (admin vs. regular user).

**Independent Test**: Can be fully tested by attempting to sign in with valid and invalid credentials for both an admin and a regular account, and confirming that a signed-in user lands on the appropriate home experience for their role.

**Acceptance Scenarios**:

1. **Given** a regular user with valid credentials, **When** they submit their username and password, **Then** they are signed in and see their spaces (empty for a brand-new user).
2. **Given** the admin user with valid credentials, **When** they submit their username and password, **Then** they are signed in and see only user-account management (no tasks or spaces).
3. **Given** any user, **When** they submit an incorrect username or password, **Then** sign-in is rejected with a clear, non-revealing error message.
4. **Given** a signed-in user, **When** they sign out, **Then** their session ends and protected areas are no longer accessible without signing in again.

---

### User Story 2 - Admin manages regular user accounts (Priority: P1)

The single admin user creates and updates regular user accounts so that family members and other people can be granted access to the application.

**Why this priority**: Regular users cannot exist or sign in until the admin creates them. This capability is a prerequisite for onboarding anyone other than the admin.

**Independent Test**: Can be fully tested by signing in as the admin, creating a new regular user, editing that user's details, and verifying the new user can subsequently sign in.

**Acceptance Scenarios**:

1. **Given** the signed-in admin, **When** they create a regular user with a username and password, **Then** the account is saved and that user can sign in.
2. **Given** the signed-in admin, **When** they update an existing regular user's username, password, or email, **Then** the changes are persisted.
3. **Given** the signed-in admin, **When** they attempt to create a user with a username that already exists, **Then** the action is rejected with a clear error.
4. **Given** a signed-in regular user, **When** they attempt to reach user-account management, **Then** access is denied.
5. **Given** the signed-in admin, **When** they view the application, **Then** no task, space, or board functionality is available to them.

---

### User Story 3 - Create and manage spaces (Priority: P1)

A regular user creates spaces to organize their work, marking each space as private (visible only to them) or public (visible to and editable by all users), and can update or delete spaces.

**Why this priority**: Spaces are the top-level container for all planning work. A board and its tasks cannot exist without a space, so this is foundational to the core value of the product.

**Independent Test**: Can be fully tested by signing in as a regular user, creating a private and a public space, editing them, verifying visibility rules from another user's perspective, and deleting a space.

**Acceptance Scenarios**:

1. **Given** a signed-in regular user, **When** they create a space with a unique key, name, and public/private flag, **Then** the space is saved with its author and creation date, and a single empty board is available for it.
2. **Given** a signed-in regular user, **When** they attempt to create a space using a key that already exists, **Then** the action is rejected with a clear error.
3. **Given** a private space, **When** a user who is not its author views the list of spaces, **Then** that private space is not visible to them.
4. **Given** a public space authored by another user, **When** a signed-in user views spaces, **Then** the public space is visible and they may create, read, update, and delete its contents (all fields except the public/private status).
5. **Given** a public space authored by another user, **When** a non-author user attempts to change its public/private status, **Then** the action is not permitted; only the space's author may change it.
6. **Given** a space with a board, tasks, subtasks, and comments, **When** the space is deleted, **Then** the space and all of its child entities are permanently removed.
7. **Given** a brand-new regular user, **When** they first sign in, **Then** no spaces exist for them by default.

---

### User Story 4 - Organize tasks on a board (Priority: P2)

Within a space's board, a user creates tasks, sees them arranged into columns by status, and drags a task from one column to another to change its status.

**Why this priority**: This is the core planning experience and the primary reason to use the product, but it depends on spaces (US3) already existing.

**Independent Test**: Can be fully tested by opening a space's board, creating tasks with different priorities and statuses, confirming they appear in the correct status columns, and dragging a task to a new column to change its status.

**Acceptance Scenarios**:

1. **Given** a space's board, **When** a user creates a task with a title and priority, **Then** the task is saved with its author, creation date, and default/selected status and appears in the matching status column.
2. **Given** a board with tasks, **When** the board is displayed, **Then** tasks are grouped into columns corresponding to the task status values (Backlog, Todo, In Progress, Done, Archived).
3. **Given** a task in one column, **When** the user drags it to another column, **Then** the task's status updates to match the destination column and the change persists.
4. **Given** an existing task, **When** the user updates its title, description, assignee, priority, or due date, **Then** the changes are saved.
5. **Given** a task with subtasks and comments, **When** the task is deleted, **Then** the task and all of its child entities are permanently removed.

---

### User Story 5 - Manage subtasks under a task (Priority: P2)

A user breaks a task down into subtasks, each with its own status, and manages those subtasks, which appear as a list beneath their parent task on the board.

**Why this priority**: Subtasks add valuable granularity to planning but are not required for the board to deliver value; they build on the task capability (US4).

**Independent Test**: Can be fully tested by opening a task, adding multiple subtasks, updating and deleting them, and confirming they render as a list beneath the parent task on the board.

**Acceptance Scenarios**:

1. **Given** an existing task, **When** the user adds a subtask with a title, **Then** the subtask is saved with its author, creation date, and status, and appears in the list under its parent task.
2. **Given** a subtask, **When** the user updates its title, description, status, assignee, or due date, **Then** the changes are saved.
3. **Given** a subtask, **When** the subtask's status is changed, **Then** it may be one of the subtask status values (Todo, In Progress, Done).
4. **Given** a subtask with comments, **When** the subtask is deleted, **Then** the subtask and all of its child entities are permanently removed.

---

### User Story 6 - Comment on tasks and subtasks (Priority: P3)

A user adds comments to tasks and subtasks to discuss the work, and can edit or delete their own comments.

**Why this priority**: Comments support collaboration and context but are supplementary to the primary planning workflow.

**Independent Test**: Can be fully tested by adding comments to a task and a subtask, confirming they appear ordered by creation date, and verifying a user can edit and delete only their own comments.

**Acceptance Scenarios**:

1. **Given** a task or subtask, **When** a user adds a comment with content, **Then** the comment is saved with its author and creation date.
2. **Given** a task or subtask with multiple comments, **When** the comments are displayed, **Then** they are sorted by creation date in ascending order.
3. **Given** a comment authored by the current user, **When** they edit or delete it, **Then** the change is applied.
4. **Given** a comment authored by another user, **When** the current user attempts to edit or delete it, **Then** the action is not permitted.

---

### Edge Cases

- What happens when a user enters a space key that duplicates an existing space's key? Creation is rejected with a clear error, since keys are unique.
- What happens to a public space's tasks when a different user edits or deletes them? The change is permitted and applies to all viewers, because public spaces allow CRUD by any user.
- What happens when a non-author tries to change a public space's public/private status? The action is blocked; only the space's author may change the Public flag.
- What happens when two users edit the same task at the same time in a public space? The most recent save wins and overwrites the earlier one; no conflict warning is shown (last-write-wins).
- How does the system handle deleting a space, task, or subtask that has many descendants? All descendant entities (board contents, tasks, subtasks, comments) are permanently removed together (cascade hard delete).
- What happens when a task with a non-empty column is dragged to a status column? The task's status changes to the destination column; existing tasks in that column remain.
- How are dates shown to users in different time zones? Stored in UTC, displayed converted to each user's local time zone.
- What happens when a regular user tries to reach admin-only user management, or the admin tries to reach task/space features? Access is denied for the disallowed area.
- What happens when a brand-new user signs in with no spaces? They see an empty state prompting them to create their first space.

## Requirements *(mandatory)*

### Functional Requirements

#### Authentication & Roles

- **FR-001**: The system MUST allow the admin user and regular users to sign in using a username and password.
- **FR-002**: The system MUST reject sign-in attempts with invalid credentials and MUST NOT reveal which of the username or password was incorrect.
- **FR-003**: The system MUST support exactly one special admin user whose sole capability is managing regular user accounts.
- **FR-004**: The system MUST prevent the admin user from accessing any task, subtask, space, board, or comment functionality.
- **FR-005**: The system MUST prevent regular users from creating or managing any user accounts (including their own).
- **FR-006**: The system MUST allow a signed-in user to sign out, ending their session.

#### User Account Management (Admin)

- **FR-007**: The admin MUST be able to create regular user accounts.
- **FR-008**: The admin MUST be able to update existing regular user accounts.
- **FR-009**: The system MUST NOT provide any capability to delete or deactivate regular user accounts.
- **FR-010**: The system MUST require a username and password for every user account and MUST allow an optional email.
- **FR-011**: The system MUST enforce unique usernames across all accounts.

#### Spaces

- **FR-012**: Regular users MUST be able to create spaces, specifying whether each space is public or private.
- **FR-013**: The system MUST require each space to have a Key, Name, Author, Public flag, and Creation Date.
- **FR-014**: The system MUST require the space Key to be entered by the user and MUST enforce that each Key is unique.
- **FR-015**: The system MUST make public spaces visible to all regular users and private spaces visible only to their author.
- **FR-016**: The system MUST allow any regular user to perform create, read, update, and delete operations on public spaces and their contents, except for changing the space's Public flag (see FR-016a).
- **FR-016a**: The system MUST restrict changing a space's Public flag (its public/private status) to the space's author only; other users MUST NOT be able to change it, even for a public space.
- **FR-017**: The system MUST restrict create, read, update, and delete operations on a private space and its contents to the space's author.
- **FR-018**: The system MUST NOT create any spaces by default for a new user.
- **FR-019**: The system MUST present each space's tasks on a single board view — the page displayed when a user navigates to that space; the board is not a separately stored entity.

#### Boards & Tasks

- **FR-020**: The system MUST organize a board's tasks into columns based on task status.
- **FR-020a**: The system MUST render one always-visible column per task status, including a dedicated "Archived" column, so all five status columns (Backlog, Todo, In Progress, Done, Archived) are shown on the board.
- **FR-021**: The system MUST support the task status values Backlog, Todo, In Progress, Done, and Archived.
- **FR-022**: The system MUST support the task priority values Low, Medium, High, and Critical.
- **FR-023**: Users MUST be able to create, update, and delete tasks.
- **FR-024**: The system MUST require each task to have a Title, Author, Status, Priority, and Creation Date.
- **FR-024a**: The system MUST default a newly created task's status to Backlog, while allowing the user to select a different status during or after creation.
- **FR-025**: The system MUST allow each task to optionally have a Description, Assignee, and Due Date.
- **FR-026**: Users MUST be able to drag and drop a task between columns to change its status, and the change MUST persist.
- **FR-026a**: Within each status column, the system MUST order tasks automatically by priority (Critical, High, Medium, Low) and then by creation date; manual reordering of tasks within a column is not supported.

#### Subtasks

- **FR-027**: Users MUST be able to create, update, and delete subtasks belonging to a task.
- **FR-028**: The system MUST support the subtask status values Todo, In Progress, and Done.
- **FR-029**: The system MUST require each subtask to have a Title, Author, Status, and Creation Date.
- **FR-030**: The system MUST allow each subtask to optionally have a Description, Assignee, and Due Date.
- **FR-030a**: The system MUST restrict the Assignee of a task or subtask to users who have access to the containing space — any registered user for a public space, and only the author for a private space.
- **FR-031**: The system MUST display subtasks as a list under their parent task on the board.

#### Comments

- **FR-032**: Users MUST be able to add comments to tasks and subtasks.
- **FR-033**: The system MUST require each comment to have an Author, Content, and Creation Date.
- **FR-034**: Users MUST be able to edit and delete their own comments and MUST NOT be able to edit or delete comments authored by others.
- **FR-035**: The system MUST sort comments by Creation Date in ascending order.

#### Deletion & Data Handling

- **FR-036**: The system MUST implement deletion of spaces, tasks, subtasks, and comments as a hard delete (removed from the database).
- **FR-037**: Deleting an entity MUST also delete all of its child entities (cascade).
- **FR-038**: The system MUST store all date fields in UTC.
- **FR-039**: The system MUST display all date fields converted to the viewing user's local time zone.
- **FR-040**: When multiple users edit the same entity in a shared public space concurrently, the system MUST apply last-write-wins — the most recent save overwrites earlier changes without a conflict warning.

### Key Entities *(include if feature involves data)*

- **User**: A person who can sign in. Has a username and password (required), an email (optional), a Creation Date, and an optional Update Date. Is either the single admin (manages users only) or a regular user (manages spaces, tasks, subtasks, and comments).
- **Space**: A top-level container that groups planning work. Has a user-entered unique Key, a Name, an Author (the creating user), a Public flag (public vs. private visibility), a Creation Date, and an optional Update Date. Its tasks are shown on a board view (the page displayed when navigating to the space); the board is not a separate entity.
- **Task**: A unit of work on a board. Required: Title, Author, Status, Priority, Creation Date. Optional: Description, Assignee, Due Date, Update Date. May own many Subtasks and Comments.
- **Subtask**: A smaller unit of work belonging to a task. Required: Title, Author, Status, Creation Date. Optional: Description, Assignee, Due Date, Update Date. May own many Comments.
- **Comment**: A note attached to a task or a subtask. Required: Author, Content, Creation Date. Optional: Update Date. Owned by exactly one task or subtask.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A signed-in user can create a new space and see its empty board in under 1 minute.
- **SC-002**: A user can create a task and place it in the correct status column in under 30 seconds.
- **SC-003**: Dragging a task to a new column reflects the updated status immediately (within 1 second) and the change survives a page reload.
- **SC-004**: 100% of private spaces are hidden from users other than their author, verified across accounts.
- **SC-005**: 100% of public spaces are visible and editable by every signed-in regular user.
- **SC-006**: Deleting a space, task, or subtask removes it and 100% of its descendant entities, with none remaining afterward.
- **SC-007**: All displayed dates match the viewing user's local time zone, while stored values remain in UTC, verified across at least two different time zones.
- **SC-008**: The admin user has zero access paths to task, subtask, space, board, or comment features, and regular users have zero access paths to user-account management.
- **SC-009**: Comments always appear in ascending creation-date order, verified with at least three comments.
- **SC-010**: A brand-new regular user sees zero spaces on first sign-in.

## Assumptions

- The single admin account is provisioned during initial system setup (seeded), since regular users cannot create accounts and the admin cannot be created through the in-app user management by another admin.
- Passwords are stored using a secure one-way hashing mechanism; plain-text password storage is out of scope and not permitted.
- Space Keys are unique globally across all spaces (not per-user), because public spaces are shared across all users and keys identify spaces unambiguously.
- The Public/private flag of a space may be changed only by the space's author; all other regular users may update every other field of a public space but not its public/private status.
- A newly created task defaults to Backlog status; the user may select a different status during or after creation.
- "Author" for spaces, tasks, subtasks, and comments refers to the user who created the entity and is recorded automatically at creation time.
- Reading (viewing) a private space's contents is restricted to its author, consistent with the visibility rule; only public-space contents are readable by others.
- The application targets personal and family-scale usage (a small number of users), so large-scale concurrency and enterprise-grade access control tiers are out of scope for this feature.

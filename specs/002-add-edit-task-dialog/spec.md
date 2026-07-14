# Feature Specification: Task Modal Editing

**Feature Branch**: `002-add-edit-task-dialog`

**Created**: 2026-07-14

**Status**: Draft

**Input**: User description: "Tasks are currently edited inline on the board and added via a form at the top of the board. Add a feature that allows users to edit tasks through a modal dialog that opens when they click a task. Tasks should also be added through the same modal dialog, which opens when the user clicks the 'Create Task' button at the top of the board. For each task, the board should display the title, priority, assignee's username, and the titles of its subtasks. The modal dialog should take up the entire screen on mobile devices and most of the screen on desktop devices, where it should be centered. Tasks and subtasks should be read-only on the board. The current form for adding tasks should be removed from the board."

## Clarifications

### Session 2026-07-14

- Q: Should the task modal support creating/editing subtasks, or only display them? → A: The task modal supports full subtask management using existing subtask validation and behavior.
- Q: What should happen when a user dismisses a modal with unsaved changes? → A: Dismissing the modal always discards unsaved changes immediately.
- Q: How should modal saves handle concurrent task updates? → A: Last save wins; the modal save overwrites the latest task state using existing save behavior.
- Q: What responsive threshold and desktop dimensions should the task modal use? → A: Full-screen below 768px; centered at 768px and wider with max 90vw, max 90vh, and max width 960px.
- Q: How should task cards display many subtasks? → A: Show all subtask titles on the card in a compact read-only list; the board or column scrolls as needed.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Edit a Task from the Board (Priority: P1)

A board user opens an existing task from the board, reviews its full details in a modal dialog, updates the task, and returns to the board with the updated summary visible.

**Why this priority**: Editing existing work is the core change in behavior and removes inline editing from the board.

**Independent Test**: Can be tested by opening a task from the board, changing its editable details in the modal, saving, and confirming the board summary updates while the board itself remains read-only.

**Acceptance Scenarios**:

1. **Given** a board contains at least one task, **When** the user clicks a task, **Then** a modal dialog opens with that task's editable details.
2. **Given** the task modal is open for an existing task, **When** the user changes task details and saves, **Then** the modal closes and the board displays the updated task summary.
3. **Given** the task modal is open for an existing task, **When** the user cancels or dismisses the modal, **Then** no unsaved task changes appear on the board.
4. **Given** the board is visible, **When** the user attempts to modify task or subtask text directly on the board, **Then** the board provides no inline editing controls.

---

### User Story 2 - Create a Task from the Board Header (Priority: P2)

A board user clicks the Create Task button at the top of the board, enters task details in the same modal experience used for editing, saves the task, and sees the new task appear on the board.

**Why this priority**: Creating tasks must remain available after the top-of-board form is removed, and using the same modal keeps task entry consistent.

**Independent Test**: Can be tested by clicking Create Task, completing the modal, saving, and confirming the new task appears on the board without using a top-of-board form.

**Acceptance Scenarios**:

1. **Given** the board is visible, **When** the user clicks Create Task, **Then** an empty task modal opens for creating a new task.
2. **Given** the create-task modal is open with valid task details, **When** the user saves, **Then** the modal closes and the new task appears on the board.
3. **Given** the board is visible, **When** the user looks above the board columns, **Then** no task creation form is displayed there.

---

### User Story 3 - Scan Task Summaries on the Board (Priority: P3)

A board user scans each task card to understand the work item without opening it, seeing the task title, priority, assigned user's username, and subtask titles.

**Why this priority**: The board remains useful as a high-level planning view even after detailed editing moves into the modal.

**Independent Test**: Can be tested by viewing tasks with priorities, assignees, and subtasks, then confirming each task card presents those summary details as read-only information.

**Acceptance Scenarios**:

1. **Given** a task has a title, priority, assignee, and subtasks, **When** the board displays the task, **Then** the task card shows the title, priority, assignee's username, and each subtask title.
2. **Given** a task has no subtasks, **When** the board displays the task, **Then** the task card still shows the task title, priority, and assignee without implying editable subtask fields.
3. **Given** the user views a subtask title on the board, **When** the user clicks or focuses that subtask title, **Then** the board does not allow direct subtask editing from that location.

---

### User Story 4 - Use the Modal Across Device Sizes (Priority: P4)

A board user can comfortably create and edit tasks on both mobile and desktop devices because the modal uses the available screen space appropriately.

**Why this priority**: The modal becomes the main task editing surface, so it must be usable on small touch screens and larger desktop screens.

**Independent Test**: Can be tested by opening the task modal below 768px viewport width and at 768px viewport width and wider, then confirming that it fills the smaller screen and uses the centered desktop bounds.

**Acceptance Scenarios**:

1. **Given** the user opens the task modal below 768px viewport width, **When** the modal appears, **Then** it occupies the full screen and supports touch-friendly interaction.
2. **Given** the user opens the task modal at 768px viewport width or wider, **When** the modal appears, **Then** it is centered and constrained to max 90vw, max 90vh, and max width 960px.
3. **Given** the task modal contains more content than can fit vertically, **When** the user scrolls, **Then** modal content remains accessible without losing the ability to save or cancel.

### Edge Cases

- Task changes are saved while another user has recently updated the same task; existing last-save-wins behavior applies, so a successful modal save overwrites the latest task state.
- A task has many subtasks and the board card has limited vertical space; the card still shows all subtask titles in a compact read-only list, and the board or column scrolls as needed.
- A task has no assigned user.
- The user opens the create-task modal, enters partial information, and cancels.
- The user opens a task modal and then changes the viewport size between mobile and desktop widths.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The board MUST open a task editing modal when a user selects an existing task card.
- **FR-002**: The board MUST open a task creation modal when a user selects the Create Task button at the top of the board.
- **FR-003**: The task creation and task editing flows MUST use the same modal dialog experience, with mode-appropriate labels and initial values.
- **FR-004**: The board MUST NOT display the current inline task creation form.
- **FR-005**: The board MUST NOT allow direct inline editing of tasks or subtasks.
- **FR-006**: Each task card on the board MUST display the task title, task priority, assignee username when assigned, and all subtask titles in a compact read-only list.
- **FR-007**: The task modal MUST allow users to save valid changes to an existing task and return to the board with the updated task summary visible.
- **FR-008**: The task modal MUST allow users to create a valid new task and return to the board with the new task visible.
- **FR-009**: The task modal MUST allow users to cancel or dismiss without saving changes; cancelling or dismissing MUST immediately discard unsaved changes without confirmation.
- **FR-010**: Below 768px viewport width, the task modal MUST occupy the entire screen.
- **FR-011**: At 768px viewport width and wider, the task modal MUST be centered and constrained to max 90vw, max 90vh, and max width 960px while leaving visible page context around it.
- **FR-012**: The task modal MUST keep all required controls usable when its content exceeds the available vertical space.
- **FR-013**: The board MUST continue to support existing non-editing board interactions, including status movement, without requiring inline task or subtask editing controls.
- **FR-014**: The task modal MUST clearly communicate save failures and keep the user's unsaved entries available for correction or retry.
- **FR-015**: The task modal MUST support full subtask management using existing subtask validation and behavior.

### Key Entities *(include if feature involves data)*

- **Task**: A work item shown on the board. Key visible attributes for this feature include title, priority, assignee, and subtasks.
- **Subtask**: A smaller work item belonging to a task. Its title appears as read-only summary information on the board, and it can be managed through the task modal.
- **Assignee**: The user assigned to a task. The board displays this user's username when a task is assigned.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 95% of users can open an existing task from the board, update it, and see the updated summary on the board in under 30 seconds.
- **SC-002**: 95% of users can create a new task from the board header and see it on the board in under 30 seconds.
- **SC-003**: In usability review, at least 90% of participants correctly identify that task and subtask details on the board are read-only.
- **SC-004**: Below 768px and at 768px viewport width and wider, 100% of required modal controls are visible or reachable without horizontal scrolling, with desktop modal bounds no larger than max 90vw, max 90vh, and max width 960px.
- **SC-005**: 100% of task cards with complete task data show title, priority, assignee username, and all subtask titles in the board summary.

## Assumptions

- Existing task permissions and space access rules continue to determine who can create or edit tasks.
- Existing task fields, validation rules, priorities, assignees, subtasks, and status behavior remain in scope; this feature changes where task creation and editing happen, not the underlying task model.
- Unassigned tasks may exist; in that case, the board should present a clear non-editable unassigned state instead of a username.
- Existing board movement behavior is treated as a status change interaction, not inline task editing.

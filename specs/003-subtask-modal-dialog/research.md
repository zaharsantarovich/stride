# Research: Dedicated Subtask Modal Dialog

## Decision 1: Keep One Board-Owned Modal State

**Decision**: Replace the current task-only modal union in `SpaceBoardPage` with one discriminated state that can represent task create/edit or subtask create/edit. A subtask state carries `parentTaskId` and a launch surface of `board` or `task`.

**Rationale**: The board page already owns modal visibility and loaded task data. Replacing this state guarantees that task and subtask dialogs cannot be active together, preserves the origin needed after deletion, and avoids adding global state.

**Alternatives considered**:

- Stack task and subtask dialogs: rejected because it conflicts with the required replacement behavior and complicates focus management.
- Route every modal through a new URL: rejected because the current board uses local modal state and the feature does not require deep links or browser-history semantics.
- Add a modal context/store: rejected because one page owns all launch surfaces and React built-in state is sufficient.

## Decision 2: Load an Existing Subtask Through a Dedicated GET Endpoint

**Decision**: Add authorized `GET /subtasks/{subtaskId}` support to `ISubtaskService`, `SubtaskService`, and `SubtasksController`. Return a full subtask graph containing author username, assignee username, and ordered comments with comment author usernames.

**Rationale**: Board summaries can become stale and do not currently contain author usernames. A focused endpoint gives the modal an authoritative load path, consistent access checks, and a clear not-found response while reusing the existing subtask response shape.

**Alternatives considered**:

- Open directly from the task-list summary: rejected because required author display would be missing and stale details could be edited.
- Add a separate comments GET request: rejected because comments are part of the subtask modal aggregate and are already included in subtask responses.
- Add a parent-task GET endpoint: rejected because the board already has its task list and `taskId`; refreshing that list supplies the parent dialog and summaries.

## Decision 3: Enrich Existing Response Contracts, Not Entities

**Decision**: Add `AuthorUsername` to the API and TypeScript `Subtask` and `Comment` response contracts. Update EF Core graph queries to include `Subtask.Author` and `Comment.Author`, and reload created/updated comments before mapping when necessary.

**Rationale**: Username is presentation data available through existing relationships. The domain already stores `AuthorId` and exposes `Author`; duplicating usernames in persisted entities would create stale data and require an unnecessary migration.

**Alternatives considered**:

- Resolve usernames client-side from the regular-user lookup: rejected because authors may not be present in a mutable lookup and every response should be self-contained.
- Persist author usernames on subtasks/comments: rejected because it duplicates user data and violates normalized ownership.
- Return only author IDs: rejected because it does not satisfy the modal display contract.

## Decision 4: Preserve Existing Persistence Model

**Decision**: Use the current `Task`, `Subtask`, `Comment`, and `User` entities and relationship configurations without a migration.

**Rationale**: Required fields, optional timestamps, task/subtask ownership, author/assignee relationships, the exactly-one-parent comment constraint, and subtask-to-comment cascade deletion already exist. This feature changes queries, contracts, and presentation only.

**Alternatives considered**:

- Add launch-surface or modal fields to entities: rejected because they are transient UI state.
- Soft-delete subtasks/comments: rejected because the specification preserves existing hard/cascade deletion.

## Decision 5: Make Chronological Ordering Authoritative

**Decision**: Return subtasks and comments in ascending `CreatedAt` order from EF Core queries and controller mappings. Do not apply `Id` or any other secondary sort criterion. Client updates preserve or reapply the same creation-time ordering.

**Rationale**: Server-authoritative ordering keeps both launch surfaces consistent and follows the requirement that creation time is the sole ordering criterion. Equal creation times remain tied rather than being reordered by another field.

**Alternatives considered**:

- Sort only on the client: rejected because multiple consumers could disagree and refreshed API data would remain unspecified.
- Sort by ID, status, or assignee after creation time: rejected because the specification requires chronological ordering only.

## Decision 6: Reuse Task Modal Geometry Exactly

**Decision**: Extract or directly share the task modal shell classes for backdrop, mobile full-screen dimensions, desktop `90vw`/`90vh` bounds, `960px` maximum width, and the scrollable content region. Render fields, comments, and new-comment controls in one vertical flow.

**Rationale**: Shared geometry prevents style drift and makes equality testable at identical viewport sizes. The existing shell already supports full-screen mobile and constrained desktop layouts.

**Alternatives considered**:

- Approximate the task modal in a separate style block: rejected because small class differences would violate identical boundaries.
- Use a nested comments modal: rejected because comments must remain in the subtask flow and nested modals would hurt mobile reachability.

## Decision 7: Use Semantic, URL-Like Subtask Controls

**Decision**: Render subtask titles as semantic links with an actual board-local `href`, prevent full navigation when JavaScript handles the modal transition, stop task-card click/drag propagation, and provide an accessible name with subtask and parent-task context where available.

**Rationale**: Native anchors provide keyboard activation, focus semantics, touch behavior, and recognizable URL-like presentation. A meaningful fallback URL is more robust than `href="#"`.

**Alternatives considered**:

- Link-styled buttons: rejected because the requested interaction is explicitly URL-like.
- Clickable text or `div` elements: rejected because they require recreated keyboard and accessibility behavior.
- Anchors with `role="button"`: rejected because changing the native role would undermine link semantics.

## Decision 8: Keep Create and Update in the Same Modal Instance

**Decision**: The create modal begins with no subtask ID and an empty comments list. Comment input is visible but disabled. A successful create response supplies the ID, author/timestamps, and mode transition; the same open modal then enables comment creation. Updates also keep the modal open and refresh board/task summaries.

**Rationale**: The existing create endpoint returns a complete subtask. Updating modal state from that response satisfies the required in-place transition without closing or refetching.

**Alternatives considered**:

- Close and reopen after creation: rejected because it violates the required still-open workflow.
- Permit draft comments before save: rejected because no persisted subtask parent exists and the specification forbids acceptance.
- Hide comments in create mode: rejected because the specification requires the empty section and disabled submission to be visible.

## Decision 9: Enforce Comment Ownership in API and UI

**Decision**: Keep `CommentService` as the authorization boundary for edit/delete, retain its author-ID check, and resolve the comment's task/subtask parent to revalidate current space access at mutation time. The modal renders edit/delete controls only when `comment.authorId` equals the current user ID; a later authorization failure remains visible without discarding draft text.

**Rationale**: UI filtering satisfies discoverability requirements, while service enforcement protects direct API calls and handles ownership or space-access changes after load.

**Alternatives considered**:

- UI-only enforcement: rejected because callers can bypass the browser.
- Disabled controls for other users: rejected because controls must not be offered.
- Controller-only authorization: rejected because service-level checks already protect all controller callers and tests.

## Decision 10: Preserve Date-Only Due Dates and Local Timestamp Display

**Decision**: Move the existing `toDateInput`/`fromDateInput` behavior into a shared utility used by task and subtask modals. Due dates use `<input type="date">`; created/updated timestamps use the existing local date-time formatter, and null updates display a clear `Not updated` state.

**Rationale**: Reusing one conversion path avoids task/subtask drift and protects calendar dates from timezone shifts while retaining UTC API storage. Audit timestamps remain local-time display values.

**Alternatives considered**:

- Display due dates with the timestamp formatter: rejected because it exposes a time component and can shift the calendar date.
- Add a new date library: rejected because native parsing and the existing utility are sufficient.
- Change database due dates to text/date columns: rejected because no schema change is required.

## Decision 11: Refresh Summaries Without Closing the Modal

**Decision**: After subtask create/update/comment mutation, update the modal from the returned response and refresh board tasks. Parent navigation and task-origin deletion replace the subtask state with an edit-task state backed by refreshed task data; board-origin deletion clears modal state after refresh.

**Rationale**: This keeps every summary current while preserving the required active modal. Refreshing before replacement avoids showing a deleted or stale row in the restored task dialog.

**Alternatives considered**:

- Optimistically patch every nested task object: rejected because it duplicates ordering and graph-mapping rules.
- Close after every save: rejected by the clarified workflow.
- Restore cached stale parent data after deletion: rejected because the launch surface must be refreshed.

## Decision 12: Use Existing Test Tooling Only

**Decision**: Add service/controller tests in the existing xUnit projects for the GET graph, username mappings, ordering, authorization, and error responses. Run all .NET tests and the existing 80% aggregate line-coverage target. Validate frontend changes with TypeScript build, ESLint, and browser scenarios; add no frontend unit-test packages or files.

**Rationale**: This meets the requested dependency constraint, exercises the new server behavior at its ownership boundaries, and uses the repository's established validation flow.

**Alternatives considered**:

- Add Vitest and React Testing Library: rejected because frontend unit tests are explicitly out of scope for this feature.
- Rely only on manual API checks: rejected because changed service and controller behavior requires automated regression coverage.

## Resolved Clarifications

No `NEEDS CLARIFICATION` items remain. The feature uses existing access, assignment, validation, hard-delete, dismissal, HTTPS, and light-theme conventions.

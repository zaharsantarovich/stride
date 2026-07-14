# Research: Task Modal Editing

## Decision: Reuse Existing Task/Subtask Persistence and API Shapes

**Rationale**: `Task` and `Subtask` already include nullable `AssigneeId`, status, description, due date, author, and timestamp fields. The modal changes where users edit these values, not the underlying domain model. Keeping the existing create/update request shapes avoids a migration and keeps the frontend API client small.

**Alternatives considered**: Add separate modal-specific draft entities or endpoints. Rejected because draft state is local UI state and persistence already supports last-save-wins saves.

## Decision: Backend Remains Authoritative for Assignment Eligibility

**Rationale**: Task and subtask assignment rules are authorization-adjacent and must not rely on the UI alone. Saves must reject ineligible `assigneeId` values even if a client sends a crafted request. The existing service layer already has the right ownership context: task/subtask, containing space, current actor, assignee user, and user role.

**Alternatives considered**: Frontend-only filtering. Rejected because it would not protect direct API calls or stale modal state after space visibility changes.

## Decision: Add a Generic Regular Users Contract

**Rationale**: The existing `/users` endpoint is admin-only and should remain focused on account management. Regular board users still need a typed way to populate public-space assignee selectors. A generic regular-user lookup endpoint returns all regular users without coupling the lookup to a specific space. The modal derives private-space assignee options from the current signed-in user and uses the regular-user list for public spaces. Backend task/subtask save validation remains authoritative for both private and public spaces.

**Alternatives considered**: Make `/users` accessible to regular users. Rejected because it broadens an admin management endpoint and couples account administration to task assignment.

## Decision: No Frontend Unit Tests for This Feature

**Rationale**: The user explicitly requested no frontend unit tests. The feature still satisfies automated test discipline by adding .NET unit tests for backend assignment eligibility and rejection paths, while frontend behavior is validated with TypeScript build, linting, and manual responsive checks documented in quickstart.

**Alternatives considered**: Add Vitest/React Testing Library. Rejected because it conflicts with the user constraint and would add new frontend test dependencies outside this feature's request.

## Decision: Modal Owns Draft State and Discards on Dismiss

**Rationale**: The spec requires cancel/dismiss to discard unsaved changes immediately and save failures to preserve entries. Local modal state can initialize from an existing task or empty defaults, submit through existing task/subtask mutations, and reset only on successful save or dismiss.

**Alternatives considered**: Bind modal inputs directly to board task state. Rejected because direct binding would leak unsaved changes onto the board and undermine the read-only board requirement.
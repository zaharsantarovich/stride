<!--
SYNC IMPACT REPORT
==================
Version change: 2.0.2 -> 3.0.0
Bump rationale: Governance change — removed the former mandatory threshold while keeping
automated tests alongside implementation and regression tests for bug fixes. Principle II
and workflow gates were redefined (MAJOR).

Modified principles:
- II. Test Discipline (NON-NEGOTIABLE) — removed former threshold mandate

Added sections: none

Removed sections: none

Templates requiring updates:
- OK .specify/templates/plan-template.md (Constitution Check gate is generic; aligned)
- OK .specify/templates/spec-template.md (no principle-specific edits required)
- OK .specify/templates/tasks-template.md (no coverage-specific edits required)

Follow-up TODOs: none
-->

# Stride Constitution

## Core Principles

### I. Type-Safe Full-Stack Architecture

The system MUST be built as a typed full-stack application. The frontend MUST use React
with TypeScript; plain JavaScript source files are NOT permitted in application code. The
backend MUST be built on .NET 10. TypeScript MUST run in `strict` mode and the .NET build
MUST treat nullable reference types as enabled. Public contracts between frontend and
backend (API request/response shapes) MUST be explicitly typed on both sides.

**Rationale**: A uniformly typed stack catches integration errors at compile time, reduces
runtime defects, and keeps the contract between client and server verifiable rather than
assumed.

### II. Test Discipline (NON-NEGOTIABLE)

Every feature MUST ship with automated unit tests, written alongside implementation rather
than deferred. Bug fixes MUST include a regression test that fails before the fix. Writing
tests before code is permitted but not required.

**Rationale**: Writing tests alongside code keeps quality current, and regression tests make
bug fixes verifiable instead of assumed.

### III. Responsive, Cross-Device Experience

The web UI MUST be fully usable on both desktop and mobile devices. Layouts MUST be
responsive (fluid grids/flex, relative units, breakpoints) rather than fixed-width.
Interactive targets MUST remain accessible to touch input on small viewports. A feature is
NOT complete until it has been verified at both a representative mobile width and a desktop
width.

**Rationale**: Users reach the product from phones and desktops interchangeably; designing
for both from the start avoids costly retrofits and broken mobile experiences.

### IV. Modern Browser Baseline

The product targets only the latest stable versions of popular browsers (current Chrome,
Edge, Firefox, and Safari). Legacy-browser polyfills and transpilation targets for obsolete
engines MUST NOT be added. Code MAY use modern, broadly-shipped web platform features
without legacy fallbacks. Any feature that is unsupported in a current targeted browser MUST
NOT be used without a documented, working alternative.

**Rationale**: Restricting support to current browsers keeps the bundle lean, removes
maintenance overhead from legacy shims, and lets the team rely on the modern platform.

### V. Lightweight, Reliable Persistence

SQLite MUST be the system of record, and it MUST be accessed through Entity Framework Core
(EF Core) as the single data-access layer — ad-hoc queries scattered across the codebase are
prohibited. All schema changes MUST be applied through EF Core migrations, which MUST be
versioned and repeatable. Queries MUST rely on EF Core's parameterization; where raw SQL is
unavoidable it MUST be parameterized — string-concatenated SQL is prohibited.

**Rationale**: A single embedded database keeps deployment simple, while routing all access
through EF Core provides a typed, disciplined data-access layer whose migrations and
automatic parameterization protect data integrity and guard against SQL injection.

### VI. Light-Theme User Interface

The user interface MUST use a light theme everywhere; every screen, component, and state MUST
be designed and rendered against the light theme. A dark theme or alternate color scheme MUST
NOT be introduced without a constitutional amendment. UI colors MUST be sourced from the
shared light-theme design tokens rather than hard-coded per component, so the theme stays
consistent across the product.

**Rationale**: Committing to a single light theme keeps the visual language consistent,
removes the cost of maintaining and testing multiple themes, and lets contributors build UI
without ambiguity about which palette to use.

## Technology Stack & Constraints

The following stack is mandatory unless this constitution is amended:

- **Frontend**: React + TypeScript (TypeScript `strict` mode).
- **Backend**: .NET 10 (nullable reference types enabled).
- **Database**: SQLite, accessed via Entity Framework Core (EF Core) with EF Core migrations.
- **UI theme**: light theme across the entire application, driven by shared design tokens.
- **Browser support**: latest stable Chrome, Edge, Firefox, and Safari only.
- **Devices**: responsive UI supporting desktop and mobile form factors.

Introducing an additional runtime, datastore, or major framework is a constitutional change
and MUST follow the amendment procedure in Governance.

## Development Workflow & Quality Gates

- **Type checks**: CI MUST run TypeScript type checking and the .NET build with warnings
  surfaced; type errors block merge.
- **Review**: Every change MUST be reviewed before merge, and the reviewer MUST confirm
  compliance with the principles above (types, tests, responsive behavior, browser
  baseline, safe data access via EF Core, light-theme UI).
- **Responsive verification**: UI changes MUST be checked at mobile and desktop widths
  before approval.

## Governance

This constitution supersedes other development practices where they conflict. Amendments
MUST be proposed in writing, justified against the principles above, reviewed, and recorded
by updating this document and bumping its version.

Versioning policy (semantic):
- **MAJOR**: Backward-incompatible governance changes or principle removals/redefinitions.
- **MINOR**: A new principle/section or materially expanded mandatory guidance.
- **PATCH**: Clarifications, wording, or non-semantic refinements.

Compliance is verified at review time: pull requests MUST demonstrate adherence to all
principles, and any deviation MUST be explicitly justified and approved. Unjustified
complexity or violations MUST be rejected.

**Version**: 3.0.0 | **Ratified**: 2026-06-30 | **Last Amended**: 2026-07-08

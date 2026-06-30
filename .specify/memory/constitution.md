<!--
SYNC IMPACT REPORT
==================
Version change: (template/unversioned) → 1.0.0
Bump rationale: Initial ratification — first concrete constitution replacing the
unfilled template scaffold (MAJOR baseline establishment).

Modified principles:
- [PRINCIPLE_1_NAME] → I. Type-Safe Full-Stack Architecture
- [PRINCIPLE_2_NAME] → II. Test-First Discipline (NON-NEGOTIABLE)
- [PRINCIPLE_3_NAME] → III. Responsive, Cross-Device Experience
- [PRINCIPLE_4_NAME] → IV. Modern Browser Baseline
- [PRINCIPLE_5_NAME] → V. Lightweight, Reliable Persistence

Added sections:
- Technology Stack & Constraints (was [SECTION_2_NAME])
- Development Workflow & Quality Gates (was [SECTION_3_NAME])

Removed sections: none

Templates requiring updates:
- ✅ .specify/templates/plan-template.md (Constitution Check gate is generic; aligned)
- ✅ .specify/templates/spec-template.md (no principle-specific edits required)
- ✅ .specify/templates/tasks-template.md (test-task guidance aligns with Principle II)

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

### II. Test-First Discipline (NON-NEGOTIABLE)

Every feature MUST ship with automated unit tests. Aggregate unit-test line coverage MUST
be at least 80% and MUST be enforced as a build/CI gate — merges that drop coverage below
80% MUST fail. Tests MUST be written alongside or before implementation, never deferred to
a later cleanup pass. Bug fixes MUST include a regression test that fails before the fix.

**Rationale**: Coverage enforced as a gate (not a guideline) is the only mechanism that
keeps quality from eroding over time; an 80% floor balances confidence against the cost of
testing trivial code.

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

SQLite MUST be the system of record. Database access MUST go through a single, well-defined
data-access layer rather than ad-hoc queries scattered across the codebase. All schema
changes MUST be applied through versioned, repeatable migrations. User input that reaches
the database MUST use parameterized queries — string-concatenated SQL is prohibited.

**Rationale**: A single embedded database keeps deployment simple, while a disciplined
access layer, migrations, and parameterized queries protect data integrity and guard
against SQL injection.

## Technology Stack & Constraints

The following stack is mandatory unless this constitution is amended:

- **Frontend**: React + TypeScript (TypeScript `strict` mode).
- **Backend**: .NET 10 (nullable reference types enabled).
- **Database**: SQLite, accessed via a dedicated data-access layer with versioned migrations.
- **Browser support**: latest stable Chrome, Edge, Firefox, and Safari only.
- **Devices**: responsive UI supporting desktop and mobile form factors.

Introducing an additional runtime, datastore, or major framework is a constitutional change
and MUST follow the amendment procedure in Governance.

## Development Workflow & Quality Gates

- **Coverage gate**: CI MUST measure unit-test coverage and fail any change that brings
  aggregate line coverage below 80%.
- **Type checks**: CI MUST run TypeScript type checking and the .NET build with warnings
  surfaced; type errors block merge.
- **Review**: Every change MUST be reviewed before merge, and the reviewer MUST confirm
  compliance with the principles above (types, tests/coverage, responsive behavior, browser
  baseline, safe data access).
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

**Version**: 1.0.0 | **Ratified**: 2026-06-30 | **Last Amended**: 2026-06-30

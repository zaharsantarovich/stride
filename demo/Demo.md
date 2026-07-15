# Spec Driven Development with Spec Kit

## Why SDD Over Planning Mode? (With or Without Saving Plans to the Repository)

Evolution: Prompt Engineering -> Vibe Coding -> **Context Engineering (including plan mode and SDD) - we are here** -> Nearest future: Loop Engineering / Dark Factory / something else

Benefits according to the authors of SDD frameworks:
- Hierarchical documents: requirements → architecture → implementation details
- Structured documents with a consistent format
- Specifications can serve as a source of truth for the project
- Changes made at one level can automatically propagate to lower levels
- Better identification of edge cases and conflicting or missing requirements

## Comparison of SDD Frameworks

|                |Spec Kit       |OpenSpec                         |BMAD           |
|----------------|---------------|---------------------------------|---------------|
|Stars at GitHub |121k           |61k                              |51k            |
|Complexity      |Medium to high |Low to medium                    |Medium to high |
|Flexibility     |Moderate       |High                             |Lower          |
|Greenfield usage|Strong fit     |Good fit                         |Strong fit     |
|Brownfield usage|Good fit       |Strong fit                       |Good fit       |
|Supported by    |GitHub         |Community, backed by Y Combinator|Community      |
|License         |MIT            |MIT                              |MIT            |

## Demo Project

Task tracking web application:  
https://github.com/zaharsantarovich/stride

- Public access
- Base entities: users, spaces to organize tasks, boards, tasks, subtasks, comments
- Technical stack:
  - Backend: .NET 10, REST API, SQLite, Entity Framework Core
  - Frontend: React, TypeScript, Tailwind CSS, dnd-kit, Vite


## Initialize project with Spec Kit

### Step 1: Install Prerequisites

- Python
- Python uv package manager
- pip install PyYAML (optional?)

### Step 2: Install Specify CLI

~~~
uv tool install specify-cli --from git+https://github.com/github/spec-kit.git@v0.12.15
~~~

### Step 3: Initialize Repository

~~~
specify init my-project --integration copilot
~~~

Adds Spec Kit custom agents, prompts, templates, configuration files, etc.

### Step 4: Create Constitution

Initial version:
~~~
/speckit.constitution

Web UI should support both desktop and mobile devices.
Support of only latest versions of popular browsers is required.
Use .NET 10 for backend.
Use SQLite for database.
Use React and TypeScript for frontend.
Unit test coverage should be at least 80%.
~~~

- Q: should I add tech stack details to the constitution?



## Build With Spec Kit

### Flow
1. Create Spec.
1. Clarify Spec (optional).
1. Create a Technical Implementation Plan.
1. Create Tasks.
1. Analyze All Documents Before Implementation (optional).
1. Implement Tasks.

### Step 1: Create Spec

<details>
<summary>MVP</summary>

~~~
/speckit.specify

Build a task planning application for personal and family usage.

Tasks should be grouped by spaces.
Users should be able to create a new space and specify if it is private or public.
Public spaces should be visible to all users, private spaces are visible only to author.
Any user should be able to perform CRUD operations on public spaces.
Required space fields: Key, Name, Author, Public, CreationDate.
Each space should have a unique key that is entered by the user.
Only the author of a space should be able to change its public/private status.
For a new user, no spaces should be created by default.
Each space should have a single board containing tasks.

Boards should organize tasks into columns based on their status.
Users should be able to drag and drop tasks between columns to change their status.
Tasks may have multiple subtasks.
Users should be able to create, update, and delete tasks and subtasks.
Task required fields: Title, Author, Status, Priority, CreationDate.
Task optional fields: Description, Assignee, DueDate.
Subtask required fields: Title, Author, Status, CreationDate.
Subtask optional fields: Description, Assignee, DueDate.
Task status values: Backlog, Todo, In Progress, Done, Archived.
Subtask status values: Todo, In Progress, Done.
Task priority values: Low, Medium, High, Critical.
Subtasks should be displayed on the board as a list under their parent task.

Users should be able to add comments to tasks and subtasks.
Users should be able to edit and delete their own comments.
Comment required fields: Author, Content, CreationDate.
Comments should be sorted by CreationDate in ascending order.

Only a special single admin user should be able to create and update regular user accounts.
For simplicity, it is not possible to delete or deactivate regular user accounts.
Regular users should not be able to create and manage accounts for themselves.
The admin user will manage only users, not tasks or spaces.
Task functionality should not be available to the admin user.
The admin user and regular users should be able to log in using a username and password.
Required user fields: Username and Password.
Optional user fields: Email.

Delete operation for spaces, tasks, subtasks, and comments should be implemented as a hard delete, meaning that the entity is deleted from the database.
Delete operation for entity also deletes all its child entities.
All date fields should be stored in UTC format in the database but displayed in the user's local time zone in the UI.
~~~
</details>

<details>
<summary>Feature example</summary>

~~~
/speckit.specify

Tasks are currently edited inline on the board and added via a form at the top of the board.
Add a feature that allows users to edit tasks through a modal dialog that opens when they click a task.
Tasks should also be added through the same modal dialog, which opens when the user clicks the “Create Task” button at the top of the board.
For each task, the board should display the title, priority, assignee’s username, and the titles of its subtasks.
The modal dialog should take up the entire screen on mobile devices and most of the screen on desktop devices, where it should be centered.
Tasks and subtasks should be read-only on the board.
The current form for adding tasks should be removed from the board.
~~~
</details>

Generated files: spec.md and requirements.md.  
Specs shouldn't contain any technical details!

### Step 2: Clarify Spec (optional)

~~~
/speckit.clarify
~~~

Adds a Clarifications section to the spec.md.  

Modes:
- Automated / structured
- Manual

### Step 3: Create a technical implementation plan

<details>
<summary>MVP</summary>

~~~
/speckit.plan

Do not use the same domain for the REST API as for the frontend with an /api suffix.
For local development, use localhost and different ports for the frontend and REST API.
Use the latest stable version of TypeScript that is v6 as of today.
Use React state, don't add external state management libraries like Redux or Zustand.
Use Tailwind CSS (the latest stable version is v4.x.x as of today) for styling the frontend.
Use dnd-kit for drag and drop functionality at space board.
Use Vite (the latest stable version is v8.x.x as of today) for frontend build tool.

Use xunit.v3 for .NET unit testing and NSubstitute for mocking.

Use autoincrementing integer primary keys for all entities in the database.
Use ASP.NET Core Identity PasswordHasher<TUser> for password hashing and verification.
~~~
</details>

<details>
<summary>Feature example</summary>

~~~
/speckit.plan

In a private space, only the current user can be assigned to tasks and subtasks.
In a public space, any regular user can be assigned to tasks and subtasks.
Don't add unit tests for the frontend.
~~~
</details>

Generated files: plan.md, research.md, quickstart.md, data-model.md, contracts (openapi.yaml, for example)

### Step 4: Create tasks

~~~
/speckit.tasks

Add a task or tasks to verify the planned changes in the built-in browser in VS Code.
Use the regular user with the username "a" and password "a" to log in.
This user has already been added to the database.
Do not parallelize tasks.
~~~

Generated files: tasks.md.

### Step 5: Analyze All Documents Before Implementation (optional)

~~~
/speckit.analyze
~~~

Run it using several models for a cross-check.

### Step 6: Implement Tasks

~~~
/speckit.implement
~~~


## Personal findings

- :thumbsup: Spec Kit is great at collecting and clarifying requirements, identifying edge cases and inconsistencies in documentation.
- :neutral_face: Tech stack and architecture selection process
  - Your choice is not requested explicitly, but the alternatives are provided in research.md.
  - Solution: conducted separate in-depth research on the tech stack and compared the outputs from GPT and Perplexity. Requested several options, including the pros and cons of each, and asked for a recommended option with the reasoning behind the decision.
	- Alternative solution: build a custom deep-research agent in Copilot. Keep an eye on token consumption, as it may be high.
- :point_right: Native Spec Kit extension for the bug-fixing workflow:  
  https://github.com/github/spec-kit/tree/main/extensions/bug
- :neutral_face: The Flow Forward approach (immutable specs) is better supported than the Living Spec approach (specs are always kept up to date).  
  https://github.com/github/spec-kit/blob/main/docs/concepts/spec-persistence.md
- :neutral_face: Spec Kit is not explicitly helping with UI/UX.
  - Possible solution: create sketches in Figma, Paint, etc., and attach them to your request.

**Not specific to Spec Kit:**
- :thumbsup: Ask Copilot to validate the changes:
  - For web applications, Copilot can verify the changes in the built-in browser in VS Code.
  - Copilot can test HTTP APIs as well.
- :thumbsdown: High cognitive load when reviewing documents and generated source code.
- :point_right: Don't try to implement multiple features in one run, implement them one by one.
- :neutral_face: The content of generated documents may vary even across different runs of the same model due to the nondeterministic nature of LLMs

<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan
<!-- SPECKIT END -->

All .NET project names and namespaces should have prefix "ZSLabs.Stride.".
Regular source code should be added to /src/ folder. Unit tests should be added to /tests/ folder.
Add services to a separate project named "ZSLabs.Stride.Services".
Add persistence-related code to a separate project named "ZSLabs.Stride.Persistence".
Add models to a separate project named "ZSLabs.Stride.Domain".
Models should not contain any business logic, only data and constructors.
If C# or csproj files were added or updated as a part of the change, run all unit tests.
When change is complete, ensure that there is no unused code, using statements, NuGet packages, resources, or files.
Always use the latest stable version of NuGet packages.

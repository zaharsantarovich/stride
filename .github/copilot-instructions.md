<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan
<!-- SPECKIT END -->

All .NET project names and namespaces should have prefix "ZSLabs.Stride.".  
Regular source code should be added to /src/ folder. Unit tests should be added to /tests/ folder.  
Project structure:
- ZSLabs.Stride.Web - frontend
- ZSLabs.Stride.Api - REST API
- ZSLabs.Stride.App - includes services under "Services" subfolder
- ZSLabs.Stride.Domain - includes EF entities under "Entities" subfolder, enums under "Enums" subfolder, etc.
- ZSLabs.Stride.Persistence - includes DBContext, migrations, configurations, etc.

EF entities should not contain any business logic, only data and constructors.  
If C# or csproj files were added or updated as a part of the change, run all unit tests.  
When change is complete, ensure that there is no unused code, using statements, NuGet packages, resources, or files.  
For each added NuGet package and frontend dependency check the latest stable version and use it.

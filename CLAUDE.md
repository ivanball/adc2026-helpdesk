# Helpdesk

Modular monolith on .NET 10. Modules live under src/Modules/<Name> with four projects:
Domain, Application, Infrastructure, Presentation. Composition root is Helpdesk.Host.

## Commands
- Build: dotnet build Helpdesk.slnx
- Test: dotnet test Helpdesk.slnx (API tests need the SQL container: docker compose up -d)
- Run: dotnet run --project src/Helpdesk.Host

## Layer rules (enforced by tests in Helpdesk.ArchitectureTests)
- Domain references only Helpdesk.SharedKernel. No NuGet packages in Domain.
- Application references Domain. Infrastructure and Presentation reference Application.
- Only Host references Infrastructure. Modules never reference other modules' internals.

## Conventions
- Errors: return Result / Result<T> from SharedKernel. Never throw for business rule
  failures. Exceptions are for bugs and infrastructure faults only.
- Entities: private constructors, static factory methods returning Result<T>.
- Handlers: one command or query per file, handler beside it, internal visibility.
- Nullable enabled, warnings as errors, file-scoped namespaces.
- Never: add a NuGet package without asking; touch the database schema without a migration.

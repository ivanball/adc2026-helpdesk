Create the solution skeleton described in CLAUDE.md:
- Helpdesk.slnx at the root; projects per the layout in CLAUDE.md.
- SharedKernel, Domain, Application: classlib. Infrastructure: classlib with
  Microsoft.EntityFrameworkCore.SqlServer. Presentation: classlib with a
  Microsoft.AspNetCore.App framework reference. Host: web project.
- Test projects use xUnit; ArchitectureTests also gets NetArchTest.Rules and references
  SharedKernel plus all four Tickets projects.
- Wire ONLY the project references allowed by the layer rules in CLAUDE.md.
- Each classlib gets one placeholder file ModuleInfo.cs in its root namespace:
  public static class ModuleInfo { public const string Name = "Tickets"; } (Name is
  "SharedKernel" in Helpdesk.SharedKernel). Each test project gets one PlaceholderTests.cs
  with a single xUnit fact asserting ModuleInfo.Name: Domain.Tests against
  Helpdesk.Tickets.Domain, ArchitectureTests against Helpdesk.SharedKernel. Delete each
  template's Class1.cs / UnitTest1.cs.
- Host: minimal API with a /health endpoint returning "OK".
- Everything must build with zero warnings (Directory.Build.props already sets
  TreatWarningsAsErrors).

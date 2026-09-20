namespace Helpdesk.ApiTests;

/// <summary>
/// One factory for the whole assembly. The factory drops and re-creates the test database,
/// so two test classes must not each own a copy of it and reset it under one another.
/// </summary>
[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<HelpdeskApiFactory>
{
    public const string Name = "Api";
}

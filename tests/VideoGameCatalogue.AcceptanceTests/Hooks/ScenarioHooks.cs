using Reqnroll;
using Reqnroll.BoDi;
using VideoGameCatalogue.AcceptanceTests.Drivers;
using VideoGameCatalogue.AcceptanceTests.Support;

namespace VideoGameCatalogue.AcceptanceTests.Hooks;

[Binding]
public class ScenarioHooks
{
    private static CustomWebApplicationFactory? _factory;

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        _factory = new CustomWebApplicationFactory();
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        _factory?.Dispose();
    }

    [BeforeScenario]
    public async Task BeforeScenario(IObjectContainer container)
    {
        if (_factory is null)
        {
            _factory = new CustomWebApplicationFactory();
        }

        await _factory.ResetDatabaseAsync();

        var client = _factory.CreateClient();
        var driver = new VideoGameApiDriver(client);

        container.RegisterInstanceAs(driver);
    }
}

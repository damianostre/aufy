namespace TestWebApi.IntegrationTests.Tests;

[SetUpFixture]
public class TestsSetup
{
    [OneTimeSetUp]
    public static void OneTimeSetup()
    {
    }

    [OneTimeTearDown]
    public static void OneTimeTearDown()
    {
        MyWebApplicationFactory.DisposeInstance().Wait();
    }
}
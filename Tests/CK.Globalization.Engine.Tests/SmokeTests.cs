using CK.Testing;
using NUnit.Framework;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.Globalization.Engine.Tests;

[TestFixture]
public class SmokeTests
{
    [Test]
    public async Task engine_runs_on_an_empty_configuration_Async()
    {
        var config = TestHelper.CreateDefaultEngineConfiguration();
        var result = await config.RunSuccessfullyAsync();
        var map = result.LoadMap();
        await using var services = map.CreateAutomaticServices();
        Assert.Pass();
    }
}

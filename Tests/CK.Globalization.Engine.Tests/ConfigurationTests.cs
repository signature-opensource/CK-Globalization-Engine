using CK.Core;
using CK.Setup;
using CK.Testing;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.Globalization.Engine.Tests;

[TestFixture]
public class ConfigurationTests
{
    [SetUp]
    [TearDown]
    public void ClearCache()
        => typeof( NormalizedCultureInfo )
            .GetMethod( "ClearCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static )!
            .Invoke( null, null );

    [Test]
    public void active_cultures_are_parsed_from_the_comma_separated_list()
    {
        var binCfg = new GlobalizationBinPathAspectConfiguration { ActiveCultures = "fr, de" };
        var actives = binCfg.GetActiveCultures();
        actives.Select( c => c.Name ).ShouldBe( new[] { "fr", "de" }, ignoreOrder: true );
    }

    [Test]
    public async Task aspect_resolves_and_runs_with_globalization_aspect_configured_Async()
    {
        var config = TestHelper.CreateDefaultEngineConfiguration();
        config.AddAspect( new GlobalizationAspectConfiguration() );
        config.FirstBinPath.AddAspect( new GlobalizationBinPathAspectConfiguration { ActiveCultures = "fr" } );

        var result = await config.RunSuccessfullyAsync();   // proves CK.Setup.GlobalizationAspect loads + runs
        var map = result.LoadMap();
        await using var services = map.CreateAutomaticServices();
        Assert.Pass();
    }
}

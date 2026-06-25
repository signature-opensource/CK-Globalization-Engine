using CK.Core;
using CK.Setup;
using CK.Testing;
using NUnit.Framework;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.Globalization.Engine.Tests;

[TestFixture]
public class MergeTests
{
    [SetUp]
    [TearDown]
    public void ClearCache()
        => typeof( NormalizedCultureInfo )
            .GetMethod( "ClearCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static )!
            .Invoke( null, null );

    [Test]
    public async Task package_locales_are_merged_and_registered_Async()
    {
        var config = TestHelper.CreateDefaultEngineConfiguration();
        config.AddAspect( new GlobalizationAspectConfiguration() );
        config.FirstBinPath.AddAspect( new GlobalizationBinPathAspectConfiguration { ActiveCultures = "fr" } );
        config.FirstBinPath.Types.Add( typeof( GlobalizationTranslationInstaller ) );
        config.FirstBinPath.Types.Add( typeof( SampleLocalePackage ) );

        var result = await config.RunSuccessfullyAsync();
        var map = result.LoadMap();
        await using var services = map.CreateAutomaticServices();

        var s = new TranslationService();
        var fr = NormalizedCultureInfo.EnsureNormalizedCultureInfo( "fr" );

        var greeting = await s.TranslateAsync( new CodeString( fr, $"Hello {2}", "Engine.Sample.Greeting" ) );
        greeting.Text.ShouldBe( "Bonjour 2" );
        // Perfect (not Good): the requested culture "fr" is the EXACT culture carrying the merged format.
        // Good would require a parent/sibling fallback (e.g. requesting "fr-FR" against a format on "fr").
        greeting.TranslationQuality.ShouldBe( MCString.Quality.Perfect );

        var farewell = await s.TranslateAsync( new CodeString( fr, $"Goodbye", "Engine.Sample.Farewell" ) );
        farewell.Text.ShouldBe( "Au revoir" );
    }
}

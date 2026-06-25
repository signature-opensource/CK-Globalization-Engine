using CK.Core;
using CK.Setup;
using CK.Testing;
using NUnit.Framework;
using Shouldly;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.Globalization.Engine.Tests;

[TestFixture]
public class IsolationTests
{
    [SetUp]
    [TearDown]
    public void Reset()
    {
        typeof( NormalizedCultureInfo )
            .GetMethod( "ClearCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static )!
            .Invoke( null, null );
        NormalizedCultureInfo.ClearCachedTranslations();   // new public hook
    }

    [Test]
    public async Task empty_locales_produce_an_empty_installer_no_error_Async()
    {
        var config = TestHelper.CreateDefaultEngineConfiguration();
        config.AddAspect( new GlobalizationAspectConfiguration() );
        config.FirstBinPath.AddAspect( new GlobalizationBinPathAspectConfiguration { ActiveCultures = "fr" } );
        config.FirstBinPath.Types.Add( typeof( GlobalizationTranslationInstaller ) );   // NO SampleLocalePackage

        var result = await config.RunSuccessfullyAsync();
        var map = result.LoadMap();
        await using var services = map.CreateAutomaticServices();
        map.StObjs.Obtain<GlobalizationTranslationInstaller>().ShouldNotBeNull();        // carrier still present, no-op body
    }

    [Test]
    public async Task clear_cached_translations_drops_stale_fr_format_Async()
    {
        NormalizedCultureInfo.EnsureNormalizedCultureInfo( "fr" )
            .SetCachedTranslations( new System.Collections.Generic.Dictionary<string, string> { { "K", "V {0}" } } );

        NormalizedCultureInfo.ClearCachedTranslations();

        // After reset, "fr" carries no cached translations: a fresh CodeString falls back to code-default.
        var s = new TranslationService();
        var c = new CodeString( NormalizedCultureInfo.EnsureNormalizedCultureInfo( "fr" ), $"english", "K" );
        var t = await s.TranslateAsync( c );
        t.Text.ShouldBe( "english" );
        t.TranslationQuality.ShouldBe( MCString.Quality.Awful );
    }
}

using CK.Core;
using CK.Setup;
using CK.Testing;
using NUnit.Framework;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.Globalization.Engine.Tests;

[TestFixture]
public class SniffTests
{
    [SetUp]
    [TearDown]
    public void Reset()
    {
        typeof( NormalizedCultureInfo )
            .GetMethod( "ClearCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static )!
            .Invoke( null, null );
        NormalizedCultureInfo.ClearCachedTranslations();
    }

    [Test]
    public async Task no_config_sniffs_cultures_from_locale_filenames_Async()
    {
        var config = TestHelper.CreateDefaultEngineConfiguration();
        config.AddAspect( new GlobalizationAspectConfiguration() );
        // NO GlobalizationBinPathAspectConfiguration => sniff mode (binCfg is null).
        config.FirstBinPath.Types.Add( typeof( GlobalizationTranslationInstaller ) );
        config.FirstBinPath.Types.Add( typeof( SniffLocalePackage ) );

        var result = await config.RunSuccessfullyAsync();
        var map = result.LoadMap();
        await using var services = map.CreateAutomaticServices();

        var s = new TranslationService();
        var de = NormalizedCultureInfo.EnsureNormalizedCultureInfo( "de" );
        var greeting = await s.TranslateAsync( new CodeString( de, $"Hello", "Engine.Sniff.Greeting" ) );
        greeting.Text.ShouldBe( "Hallo" );
        greeting.TranslationQuality.ShouldBe( MCString.Quality.Perfect );
    }

    [Test]
    public async Task empty_active_cultures_line_also_sniffs_Async()
    {
        var config = TestHelper.CreateDefaultEngineConfiguration();
        config.AddAspect( new GlobalizationAspectConfiguration() );
        // BinPath aspect present but ActiveCulturesLine empty => still sniff mode.
        config.FirstBinPath.AddAspect( new GlobalizationBinPathAspectConfiguration { ActiveCultures = "" } );
        config.FirstBinPath.Types.Add( typeof( GlobalizationTranslationInstaller ) );
        config.FirstBinPath.Types.Add( typeof( SniffLocalePackageEs ) );

        var result = await config.RunSuccessfullyAsync();
        var map = result.LoadMap();
        await using var services = map.CreateAutomaticServices();

        var s = new TranslationService();
        var es = NormalizedCultureInfo.EnsureNormalizedCultureInfo( "es" );
        var greeting = await s.TranslateAsync( new CodeString( es, $"Hello", "Engine.SniffEs.Greeting" ) );
        greeting.Text.ShouldBe( "Hola" );
    }

    [Test]
    public async Task explicit_active_cultures_restrict_unlisted_locale_files_Async()
    {
        var config = TestHelper.CreateDefaultEngineConfiguration();
        config.AddAspect( new GlobalizationAspectConfiguration() );
        // Explicit "fr" => strict allowlist. The fixture only ships de.jsonc, so de must NOT compile;
        // the merge handler logs "Ignoring translation file for 'de'…" and drops it.
        config.FirstBinPath.AddAspect( new GlobalizationBinPathAspectConfiguration { ActiveCultures = "fr" } );
        config.FirstBinPath.Types.Add( typeof( GlobalizationTranslationInstaller ) );
        config.FirstBinPath.Types.Add( typeof( SniffLocalePackage ) );

        var result = await config.RunSuccessfullyAsync();
        var map = result.LoadMap();
        await using var services = map.CreateAutomaticServices();

        var s = new TranslationService();
        var de = NormalizedCultureInfo.EnsureNormalizedCultureInfo( "de" );
        var greeting = await s.TranslateAsync( new CodeString( de, $"Hello", "Engine.Sniff.Greeting" ) );
        // de was ignored by the explicit allowlist, so it falls back to the English literal.
        greeting.Text.ShouldBe( "Hello" );
    }

    [Test]
    public async Task bogus_locale_filename_is_skipped_without_crashing_the_build_Async()
    {
        var config = TestHelper.CreateDefaultEngineConfiguration();
        config.AddAspect( new GlobalizationAspectConfiguration() );
        // No ActiveCultures => sniff. BogusLocalePackage ships a real it.jsonc AND a junk xx-bogus.jsonc
        // (stem passes IsValidCultureName but throws CultureNotFoundException on EnsureNormalizedCultureInfo).
        config.FirstBinPath.Types.Add( typeof( GlobalizationTranslationInstaller ) );
        config.FirstBinPath.Types.Add( typeof( BogusLocalePackage ) );

        // (a) The junk locale file does NOT crash the build (RunSuccessfully = no monitor.Error).
        var result = await config.RunSuccessfullyAsync();
        var map = result.LoadMap();
        await using var services = map.CreateAutomaticServices();

        // (b) The real culture is still sniffed and compiled alongside the junk file.
        var s = new TranslationService();
        var it = NormalizedCultureInfo.EnsureNormalizedCultureInfo( "it" );
        var greeting = await s.TranslateAsync( new CodeString( it, $"Hello", "Engine.Bogus.Greeting" ) );
        greeting.Text.ShouldBe( "Ciao" );
    }
}

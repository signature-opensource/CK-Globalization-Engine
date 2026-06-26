using CK.Core;
using CK.Setup;
using CK.Testing;
using NUnit.Framework;
using Shouldly;
using System;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.Globalization.Engine.Tests;

[TestFixture]
public class LocaleAttrTests
{
    [SetUp]
    [TearDown]
    public void ClearCache()
        => typeof( NormalizedCultureInfo )
            .GetMethod( "ClearCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static )!
            .Invoke( null, null );

    [Test]
    public async Task package_decorated_with_LocalePackage_is_discovered_without_manual_registration_Async()
    {
        var config = TestHelper.CreateDefaultEngineConfiguration();
        config.AddAspect( new GlobalizationAspectConfiguration() );
        config.FirstBinPath.AddAspect( new GlobalizationBinPathAspectConfiguration { ActiveCultures = "fr" } );
        config.FirstBinPath.Types.Add( typeof( GlobalizationTranslationInstaller ) );
        // Scan the fixture assembly the way a real app scans a referenced package assembly. We do NOT
        // Types.Add( typeof( LocaleAttrPackage ) ): discovery must come solely from its [LocalePackage] attribute.
        // Point the bin path at THIS test's output folder, where the referenced fixture assembly is copied.
        config.FirstBinPath.Path = AppContext.BaseDirectory;
        config.FirstBinPath.Assemblies.Add( "CK.Globalization.Engine.Tests.LocaleAttrFixture" );

        var result = await config.RunSuccessfullyAsync();
        var map = result.LoadMap();
        await using var services = map.CreateAutomaticServices();

        var s = new TranslationService();
        var fr = NormalizedCultureInfo.EnsureNormalizedCultureInfo( "fr" );

        var greeting = await s.TranslateAsync( new CodeString( fr, "Hello", "Engine.LocaleAttr.Greeting" ) );
        greeting.Text.ShouldBe( "Bonjour" );
        greeting.TranslationQuality.ShouldBe( MCString.Quality.Perfect );
    }

    [Test]
    public async Task non_locale_package_resource_types_are_ignored_Async()
    {
        var config = TestHelper.CreateDefaultEngineConfiguration();
        config.AddAspect( new GlobalizationAspectConfiguration() );
        config.FirstBinPath.AddAspect( new GlobalizationBinPathAspectConfiguration { ActiveCultures = "fr" } );
        config.FirstBinPath.Types.Add( typeof( GlobalizationTranslationInstaller ) );
        // PlainResourceGroupPackage is [EmbeddedResourceType] (NOT [LocalePackage]) and ships Res/locales/fr.jsonc.
        // Even though it is registered and is an IResourceGroup, the engine must NOT merge its locales:
        // selection is on [LocalePackage] only. This guards against regressing to the broad IResourceGroup scan.
        config.FirstBinPath.Types.Add( typeof( PlainResourceGroupPackage ) );

        var result = await config.RunSuccessfullyAsync();
        var map = result.LoadMap();
        await using var services = map.CreateAutomaticServices();

        var s = new TranslationService();
        var fr = NormalizedCultureInfo.EnsureNormalizedCultureInfo( "fr" );

        var greeting = await s.TranslateAsync( new CodeString( fr, "Plain", "Engine.Plain.Greeting" ) );
        // Not a [LocalePackage] => its fr.jsonc is ignored => falls back to the English code text.
        greeting.Text.ShouldBe( "Plain" );
    }
}

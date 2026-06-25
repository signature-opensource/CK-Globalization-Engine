using CK.Core;
using CK.Setup;
using CK.Testing;
using NUnit.Framework;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.Globalization.Engine.Tests;

[TestFixture]
public class ValidationTests
{
    [SetUp]
    [TearDown]
    public void ClearCache()
        => typeof( NormalizedCultureInfo )
            .GetMethod( "ClearCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static )!
            .Invoke( null, null );

    [Test]
    public async Task malformed_positional_format_fails_the_build_Async()
    {
        var config = TestHelper.CreateDefaultEngineConfiguration();
        config.AddAspect( new GlobalizationAspectConfiguration() );
        config.FirstBinPath.AddAspect( new GlobalizationBinPathAspectConfiguration { ActiveCultures = "fr" } );
        config.FirstBinPath.Types.Add( typeof( GlobalizationTranslationInstaller ) );
        config.FirstBinPath.Types.Add( typeof( BadLocalePackage ) );

        var result = await config.RunAsync();
        result.Status.ShouldBe( RunStatus.Failed );
    }

    [Test]
    public async Task orphan_key_in_culture_file_fails_the_build_Async()
    {
        // The merge already enforces this: a key in fr.jsonc that doesn't exist in default.jsonc
        // causes monitor.Error in ResourceContainerGlobalizationExtension.ReadSpecificSet, which
        // propagates as RunStatus.Failed. This test documents and protects that existing behavior.
        var config = TestHelper.CreateDefaultEngineConfiguration();
        config.AddAspect( new GlobalizationAspectConfiguration() );
        config.FirstBinPath.AddAspect( new GlobalizationBinPathAspectConfiguration { ActiveCultures = "fr" } );
        config.FirstBinPath.Types.Add( typeof( GlobalizationTranslationInstaller ) );
        config.FirstBinPath.Types.Add( typeof( OrphanKeyLocalePackage ) );

        var result = await config.RunAsync();
        result.Status.ShouldBe( RunStatus.Failed );
    }
}

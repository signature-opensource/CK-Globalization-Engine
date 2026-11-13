using CK.Core;
using CK.Setup;
using NUnit.Framework;
using Shouldly;
using static CK.Testing.MonitorTestHelper;

namespace CK.Globalization.Engine.Tests;

[TestFixture]
public class GlobalizationAppBuilderContributorTests
{
    static AppBuilderContext CreateContext( string binPathJson )
    {
        var engine = new EngineConfiguration();
        var section = ImmutableConfigurationSection.CreateFromJson( "ckonf:App:BinPaths", "Sample", binPathJson );
        return new AppBuilderContext( engine, [new AppBinPath( "Sample", TestHelper.TestProjectFolder, TestHelper.TestProjectFolder, section, engine.FirstBinPath )] );
    }

    [TestCase( """{ "Globalization": { "ActiveCultures": "fr, fr-CA" } }""" )]
    [TestCase( """{ "Globalization": { "ActiveCultures": [ "fr", "fr-CA" ] } }""" )]
    public void the_Globalization_of_a_BinPath_configures_the_aspect( string json )
    {
        var context = CreateContext( json );
        new GlobalizationAppBuilderContributor().Contribute( TestHelper.Monitor, context ).ShouldBeTrue();
        context.EngineConfiguration.FindAspect<GlobalizationAspectConfiguration>().ShouldNotBeNull();
        context.EngineConfiguration.FirstBinPath.FindAspect<GlobalizationBinPathAspectConfiguration>().ShouldNotBeNull()
               .ActiveCultures.ShouldBe( "fr, fr-CA" );
        context.BinPaths[0].CompileKeys.ShouldBe( ["Globalization"] );
    }

    [Test]
    public void without_Globalization_there_is_no_aspect_and_an_unknown_key_is_an_error()
    {
        var context = CreateContext( """{ "Logging": {} }""" );
        new GlobalizationAppBuilderContributor().Contribute( TestHelper.Monitor, context ).ShouldBeTrue();
        context.EngineConfiguration.FindAspect<GlobalizationAspectConfiguration>().ShouldBeNull();

        new GlobalizationAppBuilderContributor().Contribute( TestHelper.Monitor, CreateContext( """{ "Globalization": { "Cultures": "fr" } }""" ) ).ShouldBeFalse();
    }
}

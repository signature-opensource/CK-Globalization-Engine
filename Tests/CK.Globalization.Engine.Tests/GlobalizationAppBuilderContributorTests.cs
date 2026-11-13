using CK.Core;
using CK.Setup;
using NUnit.Framework;
using Shouldly;
using static CK.Testing.MonitorTestHelper;

namespace CK.Globalization.Engine.Tests;

[TestFixture]
public class GlobalizationAppBuilderContributorTests
{
    static AppBuilderContext CreateContext( string appJson )
    {
        var engine = new EngineConfiguration();
        var section = ImmutableConfigurationSection.CreateFromJson( "ckonf:Apps", "Sample", appJson );
        return new AppBuilderContext( engine, [new AppConfiguration( "Sample", TestHelper.TestProjectFolder, TestHelper.TestProjectFolder, section, engine.FirstBinPath )] );
    }

    [TestCase( """{ "Aspects": { "Globalization": { "ActiveCultures": "fr, fr-CA" } } }""" )]
    [TestCase( """{ "Aspects": { "Globalization": { "ActiveCultures": [ "fr", "fr-CA" ] } } }""" )]
    public void the_Globalization_aspect_of_an_App_configures_the_aspect( string json )
    {
        var context = CreateContext( json );
        new GlobalizationAppBuilderContributor().Contribute( TestHelper.Monitor, context ).ShouldBeTrue();
        context.EngineConfiguration.FindAspect<GlobalizationAspectConfiguration>().ShouldNotBeNull();
        context.EngineConfiguration.FirstBinPath.FindAspect<GlobalizationBinPathAspectConfiguration>().ShouldNotBeNull()
               .ActiveCultures.ShouldBe( "fr, fr-CA" );
        // The aspect is handled.
        context.Apps[0].Aspects.UnhandledAspects.ShouldBeEmpty();
    }

    [Test]
    public void without_Globalization_there_is_no_aspect_and_an_unknown_key_is_an_error()
    {
        var context = CreateContext( """{ "Logging": {} }""" );
        new GlobalizationAppBuilderContributor().Contribute( TestHelper.Monitor, context ).ShouldBeTrue();
        context.EngineConfiguration.FindAspect<GlobalizationAspectConfiguration>().ShouldBeNull();

        new GlobalizationAppBuilderContributor().Contribute( TestHelper.Monitor, CreateContext( """{ "Aspects": { "Globalization": { "Cultures": "fr" } } }""" ) ).ShouldBeFalse();
    }
}

using CK.Core;
using CK.Setup;
using NUnit.Framework;
using Shouldly;
using static CK.Testing.MonitorTestHelper;

namespace CK.Globalization.Engine.Tests;

[TestFixture]
public class GlobalizationAppAspectTests
{
    static AppBuilderContext CreateContext()
    {
        var engine = new EngineConfiguration();
        var section = ImmutableConfigurationSection.CreateFromJson( "ckonf:Apps", "Sample", "{}" );
        return new AppBuilderContext( engine, [new AppConfiguration( "Sample", TestHelper.TestProjectFolder, TestHelper.TestProjectFolder, section, engine.FirstBinPath )] );
    }

    [TestCase( """{ "ActiveCultures": "fr, fr-CA" }""" )]
    [TestCase( """{ "ActiveCultures": [ "fr", "fr-CA" ] }""" )]
    public void the_Globalization_aspect_of_an_App_configures_the_engine( string json )
    {
        var context = CreateContext();
        var aspect = new GlobalizationAppAspect( ImmutableConfigurationSection.CreateFromJson( "Aspects", "Globalization", json ) );
        aspect.Apply( TestHelper.Monitor, context, context.Apps[0] ).ShouldBeTrue();
        context.EngineConfiguration.FindAspect<GlobalizationAspectConfiguration>().ShouldNotBeNull();
        context.EngineConfiguration.FirstBinPath.FindAspect<GlobalizationBinPathAspectConfiguration>().ShouldNotBeNull()
               .ActiveCultures.ShouldBe( "fr, fr-CA" );
    }

    [Test]
    public void an_unknown_key_throws()
    {
        Util.Invokable( () => new GlobalizationAppAspect( ImmutableConfigurationSection.CreateFromJson( "Aspects", "Globalization", """{ "Cultures": "fr" }""" ) ) )
            .ShouldThrow<CKException>().Message.ShouldContain( "Unknown \"Cultures\"" );
    }
}

using CK.Core;
using CK.Setup;
using NUnit.Framework;
using System.Linq;
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

    static UserMessageCollector Collector() => new UserMessageCollector( new CurrentCultureInfo( new TranslationService(), NormalizedCultureInfo.CodeDefault ) );

    static GlobalizationAppAspectConfiguration? Read( string json, UserMessageCollector collector )
    {
        ConfigurationModelReader.TryRead<GlobalizationAppAspectConfiguration>( ImmutableConfigurationSection.CreateFromJson( "Aspects", "Globalization", json ),
                                                                               "Aspects:Globalization",
                                                                               collector,
                                                                               out var configuration );
        return configuration;
    }

    [TestCase( """{ "ActiveCultures": "fr, fr-CA" }""" )]
    [TestCase( """{ "ActiveCultures": [ "fr", "fr-CA" ] }""" )]
    public void the_Globalization_aspect_of_an_App_configures_the_engine( string json )
    {
        var context = CreateContext();
        var aspect = new GlobalizationAppAspect( Read( json, Collector() ).ShouldNotBeNull() );
        aspect.Apply( TestHelper.Monitor, context, context.Apps[0] ).ShouldBeTrue();
        context.EngineConfiguration.FindAspect<GlobalizationAspectConfiguration>().ShouldNotBeNull();
        context.EngineConfiguration.FirstBinPath.FindAspect<GlobalizationBinPathAspectConfiguration>().ShouldNotBeNull()
               .ActiveCultures.ShouldBe( "fr, fr-CA" );
    }

    [Test]
    public void an_unknown_key_is_an_error()
    {
        var collector = Collector();
        Read( """{ "Cultures": "fr" }""", collector ).ShouldBeNull();
        collector.UserMessages.Single( m => m.Level == UserMessageLevel.Error ).Text
                 .ShouldBe( "Unknown key 'Aspects:Globalization:Cultures': the keys of 'Aspects:Globalization' are ActiveCultures." );
    }

    [Test]
    public void the_description_of_the_configuration()
    {
        var d = GlobalizationAppAspectConfiguration.ModelDescription;
        d.Find( "ActiveCultures" ).ShouldNotBeNull().Kind.ShouldBe( ConfigurationPropertyKind.List );
        ConfigurationModelSchema.ToJsonSchema( d )["properties"]!["ActiveCultures"].ShouldNotBeNull();
    }
}

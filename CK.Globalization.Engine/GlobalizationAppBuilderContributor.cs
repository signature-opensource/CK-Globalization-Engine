using CK.Core;
using System.Linq;

[assembly: CK.Setup.AppBuilderContributor( typeof( CK.Setup.GlobalizationAppBuilderContributor ) )]

namespace CK.Setup;

/// <summary>
/// Configures the Globalization aspect from the Apps of the builder ("ckonf.json"): the "Globalization" aspect of an App
/// (<c>"Aspects": { "Globalization": { "ActiveCultures": "fr, en" } }</c>, or an array of culture names) is its
/// <see cref="GlobalizationBinPathAspectConfiguration"/>, and its presence gives the <see cref="GlobalizationAspectConfiguration"/>.
/// Like every aspect, it is compile time: it is not projected in the host's "appsettings.json".
/// </summary>
public sealed class GlobalizationAppBuilderContributor : IAppBuilderContributor
{
    /// <inheritdoc />
    public bool Contribute( IActivityMonitor monitor, AppBuilderContext context )
    {
        bool success = true;
        foreach( var a in context.Apps )
        {
            var section = a.Aspects.TryGetAspect( "Globalization" );
            if( section == null ) continue;
            foreach( var k in section.GetChildren() )
            {
                if( !k.Key.Equals( "ActiveCultures", System.StringComparison.OrdinalIgnoreCase ) )
                {
                    monitor.Error( $"Unknown \"{k.Path}\": the key of \"Globalization\" is ActiveCultures." );
                    success = false;
                }
            }
            var cultures = section["ActiveCultures"]
                           ?? string.Join( ", ", section.TryGetSection( "ActiveCultures" )?.GetChildren().Select( c => c.Value ) ?? [] );
            context.EngineConfiguration.EnsureAspect<GlobalizationAspectConfiguration>();
            a.BinPathConfiguration.EnsureAspect<GlobalizationBinPathAspectConfiguration>().ActiveCultures = cultures;
        }
        return success;
    }
}

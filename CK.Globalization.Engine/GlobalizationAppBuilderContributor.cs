using CK.Core;
using System.Linq;

[assembly: CK.Setup.AppBuilderContributor( typeof( CK.Setup.GlobalizationAppBuilderContributor ) )]

namespace CK.Setup;

/// <summary>
/// Configures the Globalization aspect from the BinPaths of the App ("ckonf.json"): the "Globalization" section of a
/// BinPath (<c>"Globalization": { "ActiveCultures": "fr, en" }</c>, or an array of culture names) is its
/// <see cref="GlobalizationBinPathAspectConfiguration"/>, and its presence gives the <see cref="GlobalizationAspectConfiguration"/>.
/// It is compile time: it is not projected in the host's "appsettings.json".
/// </summary>
public sealed class GlobalizationAppBuilderContributor : IAppBuilderContributor
{
    /// <inheritdoc />
    public bool Contribute( IActivityMonitor monitor, AppBuilderContext context )
    {
        bool success = true;
        foreach( var b in context.BinPaths )
        {
            var section = b.Configuration.TryGetSection( "Globalization" );
            if( section == null ) continue;
            b.AddCompileKey( "Globalization" );
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
            b.BinPathConfiguration.EnsureAspect<GlobalizationBinPathAspectConfiguration>().ActiveCultures = cultures;
        }
        return success;
    }
}

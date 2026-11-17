using CK.Core;
using CK.Setup;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace CK.Globalization;

/// <summary>
/// The "Globalization" App aspect: the back-end translations of an App (the Res/locales sets of the packages that carry a
/// <see cref="LocalePackageAttribute"/> are merged for its active cultures). Optional: an App declares it in its
/// <c>"Aspects"</c> of the builder's "ckonf.json":
/// <code>
/// "Aspects": { "Globalization": { "ActiveCultures": "fr, en" } }   // Or an array of culture names.
/// </code>
/// It configures the <see cref="GlobalizationAspectConfiguration"/> of the engine and the
/// <see cref="GlobalizationBinPathAspectConfiguration"/> of the App. Like every aspect, it is compile time: it is not
/// projected in the host's "appsettings.json". An invalid configuration throws.
/// </summary>
[AppAspect( "Globalization", TypeActivation.Optional )]
public sealed class GlobalizationAppAspect : IAppAspect
{
    /// <summary>
    /// Initializes a new <see cref="GlobalizationAppAspect"/> from its configuration.
    /// </summary>
    /// <param name="configuration">The "Globalization" section. Null for a bare <c>true</c> (no active culture).</param>
    public GlobalizationAppAspect( IConfiguration? configuration )
    {
        ActiveCultures = "";
        if( configuration != null )
        {
            foreach( var k in configuration.GetChildren() )
            {
                if( !k.Key.Equals( "ActiveCultures", StringComparison.OrdinalIgnoreCase ) )
                {
                    Throw.CKException( $"Unknown \"{k.Key}\": the key of \"Globalization\" is ActiveCultures." );
                }
            }
            var s = configuration.GetSection( "ActiveCultures" );
            ActiveCultures = s.Value ?? string.Join( ", ", s.GetChildren().Select( c => c.Value ) );
        }
    }

    /// <summary>
    /// Gets the comma separated active cultures.
    /// </summary>
    public string ActiveCultures { get; }

    /// <inheritdoc />
    public bool Apply( IActivityMonitor monitor, AppBuilderContext context, AppConfiguration app )
    {
        context.EngineConfiguration.EnsureAspect<GlobalizationAspectConfiguration>();
        app.BinPathConfiguration.EnsureAspect<GlobalizationBinPathAspectConfiguration>().ActiveCultures = ActiveCultures;
        return true;
    }
}

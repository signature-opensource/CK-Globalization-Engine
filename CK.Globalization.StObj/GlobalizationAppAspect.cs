using CK.Core;
using CK.Setup;

namespace CK.Globalization;

/// <summary>
/// The "Globalization" App aspect: the back-end translations of an App (the Res/locales sets of the packages that carry a
/// <see cref="LocalePackageAttribute"/> are merged for its active cultures). Optional: an App declares it in its
/// <c>"Aspects"</c> of the builder's "ckonf.json"; its configuration is <see cref="GlobalizationAppAspectConfiguration"/>:
/// <code>
/// "Aspects": { "Globalization": { "ActiveCultures": "fr, en" } }   // Or an array of culture names.
/// </code>
/// It configures the <see cref="GlobalizationAspectConfiguration"/> of the engine and the
/// <see cref="GlobalizationBinPathAspectConfiguration"/> of the App. Like every aspect, it is compile time: it is not
/// projected in the host's "appsettings.json".
/// </summary>
[AppAspect( "Globalization", TypeActivation.Optional )]
[Configuration<GlobalizationAppAspectConfiguration>]
public sealed class GlobalizationAppAspect : IAppAspect
{
    /// <summary>
    /// Initializes a new <see cref="GlobalizationAppAspect"/> from its configuration.
    /// </summary>
    /// <param name="configuration">Its configuration, read and validated by the App builder.</param>
    public GlobalizationAppAspect( GlobalizationAppAspectConfiguration configuration )
    {
        Throw.CheckNotNullArgument( configuration );
        ActiveCultures = string.Join( ", ", configuration.ActiveCultures );
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

using CK.Core;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CK.Setup;

/// <summary>
/// Per-BinPath <c>&lt;Globalization&gt;</c> configuration.
/// Holds the active culture set that the build-time aspect will compile into the generated registrar.
/// </summary>
public sealed class GlobalizationBinPathAspectConfiguration : BinPathAspectConfiguration
{
    /// <summary>
    /// Gets or sets the comma-separated BCP47 culture list to compile into the registrar.
    /// </summary>
    public string ActiveCultures { get; set; } = "";

    /// <summary>
    /// Parses <see cref="ActiveCultures"/> and returns the deduplicated set as
    /// <see cref="NormalizedCultureInfo"/> instances.
    /// </summary>
    public IReadOnlyList<NormalizedCultureInfo> GetActiveCultures()
    {
        var names = ActiveCultures
            .Split( ',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries )
            .Distinct( System.StringComparer.OrdinalIgnoreCase );
        return names.Select( NormalizedCultureInfo.EnsureNormalizedCultureInfo ).ToArray();
    }

    /// <inheritdoc/>
    public override void InitializeFrom( XElement e )
    {
        ActiveCultures = (string?)e.Attribute( "ActiveCultures" ) ?? "";
    }

    /// <inheritdoc/>
    protected override void WriteXml( XElement e )
    {
        e.SetAttributeValue( "ActiveCultures", ActiveCultures );
    }
}

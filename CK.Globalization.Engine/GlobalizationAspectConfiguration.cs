using System.Xml.Linq;

namespace CK.Setup;

/// <summary>
/// Engine aspect configuration for backend translation composition.
/// The <see cref="AspectType"/> string points CKSetup at <see cref="GlobalizationAspect"/>
/// in the same assembly (<c>CK.Globalization.Engine</c>).
/// </summary>
public sealed class GlobalizationAspectConfiguration : EngineAspectConfiguration
{
    /// <summary>Initializes a new default configuration.</summary>
    public GlobalizationAspectConfiguration() { }

    /// <summary>Initializes a configuration from a deserialized <see cref="XElement"/>.</summary>
    /// <param name="e">The XML element; no aspect-level attributes today.</param>
    public GlobalizationAspectConfiguration( XElement e ) { }

    /// <inheritdoc/>
    public override string AspectType => "CK.Setup.GlobalizationAspect, CK.Globalization.Engine";

    /// <inheritdoc/>
    public override XElement SerializeXml( XElement e ) => e;

    /// <inheritdoc/>
    public override BinPathAspectConfiguration CreateBinPathConfiguration()
        => new GlobalizationBinPathAspectConfiguration();
}

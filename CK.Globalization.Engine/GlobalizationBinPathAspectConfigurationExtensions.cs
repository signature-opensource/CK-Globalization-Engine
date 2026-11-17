using CK.Core;
using System.Collections.Generic;
using System.Linq;

namespace CK.Setup;

/// <summary>
/// Extends <see cref="GlobalizationBinPathAspectConfiguration"/> (the configuration is in the model, CK.Globalization.StObj,
/// the parsing of the cultures is the engine's).
/// </summary>
public static class GlobalizationBinPathAspectConfigurationExtensions
{
    /// <summary>
    /// Parses <see cref="GlobalizationBinPathAspectConfiguration.ActiveCultures"/> and returns the deduplicated set as
    /// <see cref="NormalizedCultureInfo"/> instances.
    /// </summary>
    /// <param name="configuration">This configuration.</param>
    /// <returns>The active cultures.</returns>
    public static IReadOnlyList<NormalizedCultureInfo> GetActiveCultures( this GlobalizationBinPathAspectConfiguration configuration )
    {
        var names = configuration.ActiveCultures
            .Split( ',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries )
            .Distinct( System.StringComparer.OrdinalIgnoreCase );
        return names.Select( NormalizedCultureInfo.EnsureNormalizedCultureInfo ).ToArray();
    }
}

using CK.Core;
using System.Collections.Generic;

namespace CK.Globalization;

/// <summary>
/// The configuration of the "Globalization" App aspect (<see cref="GlobalizationAppAspect"/>):
/// <code>
/// "Aspects": { "Globalization": { "ActiveCultures": "fr, en" } }   // Or an array of culture names.
/// </code>
/// </summary>
[ConfigurationModel]
public sealed class GlobalizationAppAspectConfiguration
{
    /// <summary>
    /// Gets the active cultures of the App: the cultures whose translations are merged (a comma separated string or an
    /// array of culture names). None by default.
    /// </summary>
    public IReadOnlyList<string> ActiveCultures { get; init; } = [];
}

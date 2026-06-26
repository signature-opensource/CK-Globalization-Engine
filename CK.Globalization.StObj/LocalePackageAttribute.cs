using CK.Core;
using CK.Setup;
using System;
using System.Runtime.CompilerServices;

namespace CK.Globalization;

/// <summary>
/// Decorates a backend resource package (a type implementing <see cref="IResourceGroup"/> or
/// <see cref="IResourcePackage"/>) that contributes a <c>Res/locales/*.jsonc</c> set to the build-time
/// translation merge performed by the CK.Globalization.Engine aspect.
/// <para>
/// This attribute is the discovery hook: exactly like <c>[TypeScriptPackage]</c> does for ts-locales, decorating
/// a type with it pulls the type into the StObj type set so the engine scans its <c>Res/locales</c> folder.
/// No manual registration (<c>Types.Add</c> / <c>[RegisterCKType]</c>) is required — drop a <c>Res/locales</c>
/// file, decorate the package, and the translations are merged.
/// </para>
/// <para>
/// It lives in this thin StObj-aware companion package (not in the runtime <c>CK.Globalization</c>) because it
/// derives from <see cref="ContextBoundDelegationAttribute"/>, which is defined in <c>CK.StObj.Model</c>: the
/// runtime <c>CK.Globalization</c> deliberately stays free of any <c>CK.StObj.*</c> dependency to avoid a
/// repository cycle (<c>CK.StObj.Runtime</c> references <c>CK.Globalization</c>).
/// </para>
/// </summary>
[AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = false )]
public sealed class LocalePackageAttribute : ContextBoundDelegationAttribute, IEmbeddedResourceTypeAttribute
{
    /// <summary>
    /// Initializes a new <see cref="LocalePackageAttribute"/>.
    /// </summary>
    /// <param name="callerFilePath">Automatically set by the Roslyn compiler and used to compute the associated <c>Res/</c> folder.</param>
    public LocalePackageAttribute( [CallerFilePath] string? callerFilePath = null )
        : base( "CK.Globalization.Engine.LocalePackageAttributeImpl, CK.Globalization.Engine" )
    {
        CallerFilePath = callerFilePath;
    }

    /// <inheritdoc />
    public string? CallerFilePath { get; }
}

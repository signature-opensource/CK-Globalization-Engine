using CK.Core;
using CK.Setup;
using System;
using System.Reflection;

namespace CK.Globalization.Engine;

/// <summary>
/// Engine-side implementation bound to <c>CK.Globalization.LocalePackageAttribute</c> through the
/// <see cref="ContextBoundDelegationAttribute"/> mechanism (resolved by assembly-qualified name, so the engine
/// keeps no compile-time reference to the attribute package).
/// <para>
/// Its sole purpose is to be a context-bound attribute implementation: its mere presence on a type makes the
/// StObj type collector register that type into the type set. The actual <c>Res/locales</c> merge is then done
/// by <see cref="GlobalizationAspect"/>, which scans the type set for <see cref="IResourceGroup"/> /
/// <see cref="IResourcePackage"/> types. No merge work happens here.
/// </para>
/// </summary>
public sealed class LocalePackageAttributeImpl : IAttributeContextBoundInitializer
{
    /// <summary>
    /// Instantiated by the StObj engine for each type decorated with <c>[LocalePackage]</c>. The base
    /// <see cref="Attribute"/> parameter is used (rather than the concrete attribute type) so this engine
    /// assembly needs no reference to the attribute's package.
    /// </summary>
    /// <param name="monitor">Injected monitor.</param>
    /// <param name="attr">The decorating attribute instance.</param>
    /// <param name="type">The decorated type.</param>
    public LocalePackageAttributeImpl( IActivityMonitor monitor, Attribute attr, Type type )
    {
        if( !typeof( IResourceGroup ).IsAssignableFrom( type ) )
        {
            monitor.Error( $"[LocalePackage] can only decorate an IResourceGroup or IResourcePackage: '{type:N}' implements neither." );
        }
    }

    void IAttributeContextBoundInitializer.Initialize( IActivityMonitor monitor, ITypeAttributesCache owner, MemberInfo m, Action<Type> alsoRegister )
    {
        // Nothing to initialize: being a context-bound attribute is what pulls the decorated type into the type set.
    }
}

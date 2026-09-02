# CK.Globalization.StObj

One attribute, `[LocalePackage]`. It is the discovery hook that pulls a backend resource package into
the build-time translation merge - and the reason it lives in its own package is worth reading before
you go looking for it in `CK.Globalization`.

## The attribute

```csharp
[AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = false )]
public sealed class LocalePackageAttribute : ContextBoundDelegationAttribute, IEmbeddedResourceTypeAttribute
```

Decorate a type that implements `IResourceGroup` or `IResourcePackage` and contributes a
`Res/locales/*.jsonc` set, and that is all:

> This attribute is the discovery hook: exactly like `[TypeScriptPackage]` does for ts-locales,
> decorating a type with it pulls the type into the StObj type set so the engine scans its
> `Res/locales` folder. No manual registration (`Types.Add` / `[RegisterCKType]`) is required — drop a
> `Res/locales` file, decorate the package, and the translations are merged.

It satisfies the `IEmbeddedResourceTypeAttribute` contract the usual way, by capturing its own
`[CallerFilePath]` - which is what maps the decorated type to the `Res/` folder sitting beside its
source.

## What you actually write

Three pieces, and no registration anywhere. A class carrying the attribute:

```csharp
[LocalePackage]
public class LocaleAttrPackage : IResourceGroup
{
}
```

the translations beside it, one file per culture plus `default`:

```
LocaleAttrPackage.cs
Res/locales/default.jsonc     { "Engine.LocaleAttr.Greeting": "Hello" }
Res/locales/fr.jsonc          { "Engine.LocaleAttr.Greeting": "Bonjour" }
```

and that is the whole declaration. Once the assembly is in the build's scan, asking for that resource
name in `fr` returns the merged translation:

```csharp
var fr = NormalizedCultureInfo.EnsureNormalizedCultureInfo( "fr" );
var greeting = await s.TranslateAsync( new CodeString( fr, "Hello", "Engine.LocaleAttr.Greeting" ) );
// greeting.Text is "Bonjour", greeting.TranslationQuality is MCString.Quality.Perfect
```

Note what is *absent*: no `Types.Add`, no `[RegisterCKType]`, no entry in any configuration naming this
class. The attribute is the registration.

Taken from
[`LocaleAttrPackage`](../Tests/CK.Globalization.Engine.Tests.LocaleAttrFixture/LocaleAttrPackage.cs) and
the assertions of `LocaleAttrTests` - a fixture that exists precisely to prove the discovery works
through the attribute alone, and that lives in its own assembly *"exactly as a real app scans a
referenced package assembly"*.

## Why this package exists at all

A single attribute in a package of its own looks like over-splitting until you read the constraint it
resolves. From its own comment:

> It lives in this thin StObj-aware companion package (not in the runtime `CK.Globalization`) because it
> derives from `ContextBoundDelegationAttribute`, which is defined in `CK.StObj.Model`: the runtime
> `CK.Globalization` deliberately stays free of any `CK.StObj.*` dependency to avoid a repository cycle
> (`CK.StObj.Runtime` references `CK.Globalization`).

So the split is not stylistic: putting this attribute in the runtime package would make
`CK.Globalization` depend on `CK.StObj.Model`, while `CK.StObj.Runtime` already depends on
`CK.Globalization`. The cycle is broken by moving the one type that needs both into a third package.
That package does ship: any assembly decorating a type with `[LocalePackage]` references it. What it
carries is only the attribute - the behaviour stays in the engine.

## Runtime cost

None beyond the attribute itself. The base constructor names its engine counterpart by string -
`"CK.Globalization.Engine.LocalePackageAttributeImpl, CK.Globalization.Engine"` - so the behaviour lives
in the engine assembly. What ships here is a marker; what acts on it stays on the build side. That string is also the reason the engine needs no reference back: its implementation
takes a plain `Attribute`, precisely *"so this engine assembly needs no reference to the attribute's
package"*.

## Requires.

- `CK.StObj.Model` for `ContextBoundDelegationAttribute`, and `CK.EmbeddedResources.Abstractions` for
  `IEmbeddedResourceTypeAttribute`.

# CK.Globalization.Engine

The build-time aspect that merges the `Res/locales/*.jsonc` of every package marked with
`[LocalePackage]` into one set of per-culture translations, and generates the code that registers them
during DI configuration. This assembly is only referenced by a build project - what it emits is what
ships.

## What it produces

`GlobalizationAspect` is an `IStObjEngineAspect` and an `ICSCodeGeneratorWithFinalization`, and all the
work happens in the finalization pass: it merges the per-package `Res/locales/*.jsonc` and generates the
`ApplyTranslations` override of `GlobalizationTranslationInstaller`, which registers the merged
per-culture formats via `SetCachedTranslations`. That generated body is invoked from
`GlobalizationTranslationInstaller.RegisterStartupServices`, at the top of the DI-configuration phase -
after every `StObjInitialize`, before the service provider is built.

The merged formats are baked into the generated code as literal `(resName, format)` tuples, so **the
aspect's output reads no translation file at runtime** - the
[`LocalesResourceHandler`](https://github.com/signature-opensource/CK-ResourceSpace/blob/stable/CK.ResourceSpace.Globalization/README.md)
is built with `installer: null` and `ResSpace.Install` is never called. Note that the runtime package does ship a
`GlobalizationFileHelper` that loads `.json`/`.jsonc` from a folder; that is a separate, opt-in path, not
what this aspect produces.

## Selection is by attribute, not by interface

The one design decision worth knowing, because it looks wrong at first glance. The aspect could pick up
resource packages by testing for `IResourceGroup`; it deliberately does not, and keys off
`[LocalePackage]` instead. The comment explains what that avoids:

> A real app's type set contains framework resource types (e.g. CK.TypeScript / CK.TS.Angular packages
> implement `IResourceGroup` via `ITypeScriptPackage`) that have no backend resource container;
> registering those would log errors and abort the build. `[LocalePackage]` is the explicit, unambiguous
> selector — it mirrors how the TypeScript aspect keys off `[TypeScriptPackage]`. The impl validates
> IResourceGroup at construction.

That last sentence carries the enforcement, and it happens far earlier than this aspect:
`LocalePackageAttributeImpl`'s constructor - run by the type collector, for every decorated type - calls
`monitor.Error` when the type implements neither `IResourceGroup` nor `IResourcePackage`. Decorating the
wrong type **fails the build**, during type collection, before the merge is even reached.

A different case is a type that *is* a resource group but for which `RegisterPackage` returns null -
typically because it carries no backend resource container. `RegisterPackage` has logged the error, and the
aspect fails: it registers all the decorated types first, so that all the errors are reported, then stops
before building the resource space.

## Order of operations

The aspect builds its `ResSpaceData` (see
[CK.ResourceSpace](https://github.com/signature-opensource/CK-ResourceSpace/blob/stable/CK.ResourceSpace/README.md)
for the build steps of a resource space) **before** deciding the culture set, and says why:

> Built BEFORE the culture set is decided: the sniff (step 3) needs the resource containers, and
> `ResSpaceDataBuilder.Build` is culture-free.

That ordering is what allows the active cultures to be *sniffed* rather than configured. When no
`<Globalization>` element is given, or its `ActiveCultures` is blank, the aspect walks the packages'
`locales` folders and takes each `*.jsonc` filename stem. Four gates then apply: `default` is skipped,
`NormalizedCultureInfo.IsValidCultureName` must accept the stem, resolving it must not throw - a
`CultureNotFoundException` is caught, warned about and skipped, which is what a stem like `xx-bogus`
hits despite passing the syntax check - and a default culture is skipped. What survives is deduplicated.
The cultures are whatever the packages actually ship.

The `ResSpace` itself is built later, after the culture set exists, because it consumes it.

## Configuration

The configuration is in the model, [CK.Globalization.StObj](../CK.Globalization.StObj/README.md): the "Globalization"
App aspect (`GlobalizationAppAspect`), declared in the `"Aspects"` of an App of the builder's `ckonf.json`:

```jsonc
"Aspects": { "Globalization": { "ActiveCultures": "fr, fr-CA" } }   // Or an array of culture names.
```

It ensures the two engine configurations: `GlobalizationAspectConfiguration` (it carries nothing: it points the engine
at the aspect through its `AspectType` string) and the App's `GlobalizationBinPathAspectConfiguration` (its
`ActiveCultures`). Like every aspect, it is compile time: it is not projected in the host's `appsettings.json`.

`ActiveCultures` is a comma-separated BCP47 list.
`GlobalizationBinPathAspectConfigurationExtensions.GetActiveCultures` (this engine) splits it, trims, drops empties,
deduplicates case-insensitively and resolves each name through `NormalizedCultureInfo`. An App without the aspect has
no per-BinPath configuration: the aspect handles it.

## Requires.

- Its model, `CK.Globalization.StObj` (the configuration), `CK.StObj.Engine` and `CK.Engine.Configuration` for the aspect contracts, and
  `CK.ResourceSpace.Globalization` for the merge itself.

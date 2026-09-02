Build-time aspect that composes backend translations. An engine assembly, referenced by a build project.

It collects the Res/locales/*.jsonc of every package decorated with [LocalePackage], merges them per
culture, and generates the code that registers the result at the top of the DI-configuration phase - so
its own output reads no translation file at runtime.

Packages are selected by that attribute rather than by the resource-package interface, because a real
application's type set contains framework resource types that carry no backend resources. Active cultures
are configured per BinPath as a comma-separated BCP47 list, or inferred from the locale filenames the
packages actually ship.

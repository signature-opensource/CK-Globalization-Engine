A build-time aspect that merges the translations of every package decorated with `[LocalePackage]` and generates the
code that registers them per culture at startup.

It is an engine assembly, referenced by a build project, that merges the `Res/locales/*.jsonc` files of the packages.
Main types: `GlobalizationAspect`, and `GlobalizationBinPathAspectConfiguration` whose `ActiveCultures` (a BCP47
list) restricts the cultures, otherwise inferred from the locale files the packages ship.

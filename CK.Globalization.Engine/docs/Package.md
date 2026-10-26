Build-time aspect (an engine assembly, referenced by a build project) that merges the `Res/locales/*.jsonc` of every
package decorated with `[LocalePackage]` and generates the code that registers the per-culture translations at startup.
Main types: `GlobalizationAspect`, and `GlobalizationBinPathAspectConfiguration` whose `ActiveCultures` (a BCP47
list) restricts the cultures, otherwise inferred from the locale files the packages ship.

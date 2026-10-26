The `[LocalePackage]` attribute (`LocalePackageAttribute`): the discovery hook for backend translations.
Decorate a resource package that ships a `Res/locales/*.jsonc` set and the build-time engine (CK.Globalization.Engine)
finds it and merges its translations, with no manual type registration.

The [LocalePackage] attribute: the discovery hook for backend translations.

Decorate a resource package that ships a Res/locales/*.jsonc set and the build-time engine finds it and
merges its translations. No manual type registration is needed.

It lives in this thin StObj-aware companion rather than in the runtime CK.Globalization package to break
a repository cycle: the attribute derives from ContextBoundDelegationAttribute, defined in CK.StObj.Model,
while CK.StObj.Runtime already depends on CK.Globalization. What ships is a marker; the behaviour lives
in an engine assembly, bound to it by name alone.

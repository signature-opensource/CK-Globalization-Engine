using CK.Core;

namespace CK.Globalization.Engine.Tests;

/// <summary>
/// Second test-only sniff fixture, contributing Res/locales/es.jsonc for culture 'es' (named in no config).
/// Distinct culture + key namespace from SniffLocalePackage so the two sniff tests produce distinct maps
/// (distinct generated code), keeping them cache-isolated.
/// </summary>
[EmbeddedResourceType]
public class SniffLocalePackageEs : IResourceGroup
{
}

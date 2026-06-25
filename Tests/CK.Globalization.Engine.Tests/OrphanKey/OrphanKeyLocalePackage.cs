using CK.Core;

namespace CK.Globalization.Engine.Tests;

/// <summary>
/// Test-only resource package contributing an orphan key in Res/locales/fr.jsonc
/// (a key that does not exist in default.jsonc). Used by ValidationTests to assert that
/// the existing merge logic already catches orphan keys and fails the build.
/// </summary>
[EmbeddedResourceType]
public class OrphanKeyLocalePackage : IResourceGroup
{
}

using CK.Core;
using CK.Globalization;

namespace CK.Globalization.Engine.Tests;

/// <summary>
/// Test-only resource package contributing a malformed positional composite format
/// in Res/locales/fr.jsonc. Used by ValidationTests to assert that EmitInstaller
/// emits a build-time error when a format string is invalid.
/// </summary>
[LocalePackage]
public class BadLocalePackage : IResourceGroup
{
}

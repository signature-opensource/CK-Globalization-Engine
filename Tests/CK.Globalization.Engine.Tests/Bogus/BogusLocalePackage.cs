using CK.Core;

namespace CK.Globalization.Engine.Tests;

/// <summary>
/// Test-only resource package contributing a real locale (it.jsonc) alongside a junk locale file
/// (xx-bogus.jsonc, whose stem passes IsValidCultureName but throws CultureNotFoundException on
/// EnsureNormalizedCultureInfo). Used by SniffTests to prove the sniff skips the bogus file (Warn) without
/// crashing the build, while the real culture still compiles. Its source lives in its own Bogus/ subfolder
/// so [EmbeddedResourceType] resolves Bogus/Res/ via CallerFilePath.
/// </summary>
[EmbeddedResourceType]
public class BogusLocalePackage : IResourceGroup
{
}

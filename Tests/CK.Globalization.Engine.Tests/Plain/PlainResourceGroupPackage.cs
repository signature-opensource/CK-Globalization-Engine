using CK.Core;

namespace CK.Globalization.Engine.Tests;

/// <summary>
/// Negative fixture: a resource group that ships Res/locales but is decorated with the plain
/// <see cref="EmbeddedResourceTypeAttribute"/> — NOT <c>[LocalePackage]</c>. It exists to lock in that the
/// engine selects locale packages by the <c>[LocalePackage]</c> attribute only: even when this type is
/// registered (Types.Add) and is an IResourceGroup carrying a Res/locales/fr.jsonc, its translations must NOT
/// be merged. This is the regression guard for the broad-IResourceGroup-scan bug.
/// Its source lives in its own Plain/ subfolder so [EmbeddedResourceType] resolves Plain/Res/ via CallerFilePath.
/// </summary>
[EmbeddedResourceType]
public class PlainResourceGroupPackage : IResourceGroup
{
}

using CK.Core;
using CK.Globalization;

namespace CK.Globalization.Engine.Tests;

/// <summary>
/// Test-only resource package contributing Res/locales/de.jsonc for a culture (de) named in NO
/// &lt;Globalization&gt; config. Used by SniffTests to prove culture-name sniffing compiles it with zero config.
/// Its source lives in its own Sniff/ subfolder so [LocalePackage] resolves Sniff/Res/ via CallerFilePath.
/// </summary>
[LocalePackage]
public class SniffLocalePackage : IResourceGroup
{
}

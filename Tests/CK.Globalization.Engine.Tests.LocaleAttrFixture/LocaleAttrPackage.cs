using CK.Core;
using CK.Globalization;

namespace CK.Globalization.Engine.Tests.LocaleAttrFixture;

/// <summary>
/// Stands in for a real backend package that ships a <c>Res/locales</c> set. It lives in its OWN assembly so a
/// test can put that assembly in <c>FirstBinPath.Assemblies</c> (exactly as a real app scans a referenced
/// package assembly) and assert the merge happens through the <see cref="LocalePackageAttribute"/> discovery
/// hook alone — with NO <c>config.FirstBinPath.Types.Add(...)</c> for this type.
/// </summary>
[LocalePackage]
public class LocaleAttrPackage : IResourceGroup
{
}

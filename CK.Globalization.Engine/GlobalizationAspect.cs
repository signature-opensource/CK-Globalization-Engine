using CK.CodeGen;
using CK.Core;
using CK.EmbeddedResources;
using CK.Setup;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CK.Setup;

/// <summary>
/// Build-time aspect that merges per-package <c>Res/locales/*.jsonc</c> and generates the
/// <see cref="CK.Globalization.GlobalizationTranslationInstaller"/> body so that, at map load,
/// the merged per-culture formats are registered via <c>SetCachedTranslations</c>.
/// </summary>
public sealed class GlobalizationAspect : IStObjEngineAspect, ICSCodeGeneratorWithFinalization
{
    readonly GlobalizationAspectConfiguration _config;

    /// <summary>
    /// Instantiated by the CKSetup engine with the resolved configuration.
    /// </summary>
    public GlobalizationAspect( GlobalizationAspectConfiguration config ) => _config = config;

    bool IStObjEngineAspect.Configure( IActivityMonitor monitor, IStObjEngineConfigureContext context ) => true;
    bool IStObjEngineAspect.OnSkippedRun( IActivityMonitor monitor ) => true;
    bool IStObjEngineAspect.RunPreCode( IActivityMonitor monitor, IStObjEngineRunContext context ) => true;
    bool IStObjEngineAspect.RunPostCode( IActivityMonitor monitor, IStObjEnginePostCodeRunContext context ) => true;
    bool IStObjEngineAspect.Terminate( IActivityMonitor monitor, IStObjEngineTerminateContext context ) => true;

    CSCodeGenerationResult ICSCodeGenerator.Implement( IActivityMonitor monitor, ICSCodeGenerationContext codeGenContext )
        => CSCodeGenerationResult.Success;

    bool ICSCodeGeneratorWithFinalization.FinalImplement( IActivityMonitor monitor, ICSCodeGenerationContext c )
    {
        var group = c.CurrentRun.ConfigurationGroup;

        // 1) Find our per-BinPath configuration (may be null when no <Globalization> element is given).
        var binCfg = group.SimilarConfigurations
                          .Select( cf => cf.FindAspect<GlobalizationBinPathAspectConfiguration>() )
                          .FirstOrDefault( a => a != null );

        // 2) Build a ResSpace over every resource-bearing package in this BinPath's type set.
        //    Built BEFORE the culture set is decided: the sniff (step 3) needs the resource containers,
        //    and ResSpaceDataBuilder.Build is culture-free.
        var cfg = new ResSpaceConfiguration( group.TypeCache, group.TypeSet.Contains );
        var pkgIfc = group.TypeCache.Get( typeof( IResourcePackage ) );
        var grpIfc = group.TypeCache.Get( typeof( IResourceGroup ) );
        foreach( var t in group.TypeSet )
        {
            if( !t.Interfaces.Contains( pkgIfc ) && !t.Interfaces.Contains( grpIfc ) ) continue;
            if( cfg.RegisterPackage( monitor, t, defaultTargetPath: default ) == null )
            {
                monitor.Trace( $"Skipped non-resource type '{t.Type.FullName}' for locales merge." );
            }
        }
        var collector = cfg.Build( monitor );
        if( collector == null ) return false;
        var spaceData = new ResSpaceDataBuilder( collector ).Build( monitor );
        if( spaceData == null ) return false;

        // 3) Decide the active culture set: explicit config restricts (strict allowlist; the merge handler
        //    warns about + drops files for unlisted cultures); otherwise sniff from contributed locale files.
        IReadOnlyList<NormalizedCultureInfo> actives =
            binCfg != null && !string.IsNullOrWhiteSpace( binCfg.ActiveCultures )
                ? binCfg.GetActiveCultures()
                : SniffActiveCultures( monitor, spaceData );
        var activeSet = new ActiveCultureSet( actives );

        // 4) Run the "locales" merge. installer:null + never calling ResSpace.Install => no files are written.
        //    Building the ResSpace fires the handler's Initialize, which computes FinalTranslations.
        var handler = new LocalesResourceHandler( installer: null,
                                                  spaceData.CoreData.SpaceDataCache,
                                                  "locales",
                                                  activeSet,
                                                  LocalesResourceHandler.InstallOption.Full );
        var spaceBuilder = new ResSpaceBuilder( spaceData );
        if( !spaceBuilder.RegisterHandler( monitor, handler ) ) return false;
        var resSpace = spaceBuilder.Build( monitor );
        if( resSpace == null ) return false;

        // 5) Project FinalTranslations into (culture, (resName,format)[]) for each NON-default active culture.
        //    The default culture (en/Invariant) is skipped: SetCachedTranslations rejects it.
        var final = handler.FinalTranslations;
        var data = new List<(string culture, (string resName, string format)[] pairs)>();
        if( final != null )
        {
            foreach( var ac in final.Culture.ActiveCultures.AllActiveCultures )
            {
                if( ac.Culture.IsDefault ) continue;
                var set = final.FindTranslationSetOrParent( ac );
                var pairs = set.RootPropagatedTranslations
                               .Select( kv => (kv.Key, kv.Value.Text) )
                               .ToArray();
                data.Add( (ac.Culture.Name, pairs) );
            }
        }

        return EmitInstaller( monitor, c, data );  // always emit, even when data is empty; returns false if any format invalid
    }

    /// <summary>
    /// Zero-config culture discovery: scans every package's <c>Res/locales/</c> folder for
    /// <c>&lt;culture&gt;.jsonc</c> files and returns the deduplicated culture set. Mirrors the discovery
    /// half of <c>ResourceContainerGlobalizationExtension.ReadTranslations</c>.
    /// </summary>
    static IReadOnlyList<NormalizedCultureInfo> SniffActiveCultures( IActivityMonitor monitor, ResSpaceData spaceData )
    {
        var found = new HashSet<NormalizedCultureInfo>();
        // Scan ALL packages (including the synthetic <App> Before slot, which carries app-level locale
        // overrides). Packages without a Res/locales/ folder simply yield nothing via TryGetFolder.
        foreach( var pkg in spaceData.CoreData.Packages )
        {
            ScanLocalesFolder( monitor, pkg.Resources.Resources, found );
            ScanLocalesFolder( monitor, pkg.AfterResources.Resources, found );
        }
        if( found.Count > 0 )
        {
            monitor.Info( $"Globalization: sniffed active cultures from package locales: {string.Join( ", ", found.Select( ci => ci.Name ) )}." );
        }
        else
        {
            monitor.Info( "Globalization: no package locale cultures found; only the default culture is active." );
        }
        return found.ToArray();

        static void ScanLocalesFolder( IActivityMonitor monitor, IResourceContainer container, HashSet<NormalizedCultureInfo> found )
        {
            if( !container.TryGetFolder( "locales", out var folder ) ) return;
            foreach( var loc in folder.AllResources )
            {
                if( !loc.ResourceName.EndsWith( ".jsonc" ) ) continue;
                var stem = System.IO.Path.GetFileNameWithoutExtension( loc.FullResourceName );
                // default.jsonc is the canonical English key registry, not a culture: skip silently.
                if( stem == "default" ) continue;
                if( !NormalizedCultureInfo.IsValidCultureName( stem ) ) continue;
                NormalizedCultureInfo ci;
                try
                {
                    // Ensure (creating) is required: the sniff is the first place a filename stem becomes a
                    // culture, so a non-creating lookup would find nothing. Guard the throw so a bogus locale
                    // filename warns + is skipped instead of crashing the build.
                    ci = NormalizedCultureInfo.EnsureNormalizedCultureInfo( stem );
                }
                catch( System.Globalization.CultureNotFoundException )
                {
                    monitor.Warn( $"Globalization: ignoring locale file '{stem}.jsonc' — '{stem}' is not a recognized culture." );
                    continue;
                }
                if( ci.IsDefault ) continue;
                found.Add( ci );
            }
        }
    }

    static bool EmitInstaller( IActivityMonitor monitor,
                               ICSCodeGenerationContext c,
                               IEnumerable<(string culture, (string resName, string format)[] pairs)> data )
    {
        // Always emit the override (even when data is empty): the body simply overrides the virtual
        // no-op base ApplyTranslations. The carrier's StObjInitialize then invokes the generated body.
        bool success = true;
        ITypeScope ck = c.Assembly.Code.Global
                         .FindOrCreateAutoImplementedClass( monitor, typeof( CK.Globalization.GlobalizationTranslationInstaller ) );
        IFunctionScope fn = ck.CreateFunction( "protected override void ApplyTranslations()" );
        foreach( var (culture, pairs) in data )
        {
            fn.Append( "CK.Core.NormalizedCultureInfo.EnsureNormalizedCultureInfo(" )
              .AppendSourceString( culture )
              .Append( ").SetCachedTranslations( new (string,string)[]{" )
              .NewLine();

            foreach( var (resName, format) in pairs )
            {
                if( !PositionalCompositeFormat.TryParse( format, out _, out var error ) )
                {
                    monitor.Error( $"Invalid positional composite format for '{resName}' in culture '{culture}': {error} (value: \"{format}\")." );
                    success = false;
                    continue;  // don't emit the malformed entry
                }
                fn.Append( "(" )
                  .AppendSourceString( resName )
                  .Append( "," )
                  .AppendSourceString( format )
                  .Append( ")," )
                  .NewLine();
            }
            fn.Append( "} );" ).NewLine();
        }
        return success;
    }
}

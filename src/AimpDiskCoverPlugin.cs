using AIMP.DiskCover.Infrastructure;
using AIMP.DiskCover.Interfaces;
using AIMP.SDK;

namespace AIMP.DiskCover
{
    [AimpPlugin(PluginName, "Evgeniy Bogdan, Roman Nikitin", Version)]
// ReSharper disable ClassNeverInstantiated.Global
    public class AimpDiskCoverPlugin : AimpPlugin
// ReSharper restore ClassNeverInstantiated.Global
    {
        public const string PluginName = "AIMP Disc Cover";
        public const string Version = "2.0.0";

        private IDiskCoverPlugin _plugin;

        public override void Initialize()
        {
            UnityConfig.Initialize(Player);
            _plugin = DependencyResolver.Current.ResolveService<IDiskCoverPlugin>();
            _plugin.Initialize();
        }

        public override void Dispose()
        {
           _plugin.Dispose();
        }
    }
}
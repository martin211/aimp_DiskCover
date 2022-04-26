using AIMP.DiskCover.Infrastructure;
using AIMP.DiskCover.Interfaces;

namespace AIMP.DiskCover
{
    [AimpPlugin(PluginName, "Evgeniy Bogdan, Roman Nikitin", AdditionalInfo.Version)]
    public class AimpDiskCoverPlugin : AimpPlugin
    {
        public const string PluginName = $"AIMP Disc Cover v.{AdditionalInfo.Version}";

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
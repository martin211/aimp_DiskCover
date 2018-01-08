using System;
using AIMP.DiskCover.Core;
using AIMP.SDK;
using AIMP.SDK.Logger;

namespace AIMP.DiskCover
{
    [AimpPlugin(PluginName, "Evgeniy Bogdan, Roman Nikitin", Version)]
// ReSharper disable ClassNeverInstantiated.Global
    public class AimpDiskCoverPlugin : AimpPlugin
// ReSharper restore ClassNeverInstantiated.Global
    {
        public const String PluginName = "AIMP Disc Cover";
        public const String Version = "2.0.0";

        private IDiskCoverPlugin _plugin;

        public override void Initialize()
        {
            UnityConfig.Iitialize(Player);
            _plugin = DependencyResolver.Current.ResolveService<IDiskCoverPlugin>();
            _plugin.Initialize();
        }

        public override void Dispose()
        {
           _plugin.Dispose();
        }
    }

    internal class InternalLoggerManager : ILogger
    {
        public void Write(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void Write(Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception.ToString());
        }

        public void Close()
        {
        }
    }
}
using System.Linq;
using AIMP.DiskCover.Infrastructure.Logger;
using AIMP.DiskCover.Interfaces;
using AIMP.DiskCover.Settings;
using AIMP.SDK.Options;
using AIMP.SDK.Player;
using Unity;
using Unity.Lifetime;
using Unity.RegistrationByConvention;

namespace AIMP.DiskCover.Infrastructure
{
    internal static class UnityConfig
    {
        private const string _matchingInternalTypePattern = "AIMP.DiskCover";

        public static void Initialize(IAimpPlayer player)
        {
            var container = new UnityContainer();

            container.RegisterTypes(
                AllClasses.FromLoadedAssemblies()
                    .Where(assembly => !string.IsNullOrWhiteSpace(assembly.Namespace) &&
                                       assembly.Namespace.Contains(_matchingInternalTypePattern)),
                WithMappings.FromMatchingInterface,
                WithName.Default,
                WithLifetime.PerResolve);

            container.RegisterType<ConfigProvider>(new ContainerControlledLifetimeManager());
            container.RegisterType<IPluginSettings, ConfigProvider>();
            container.RegisterType<IConfigProvider, ConfigProvider>();

            container.RegisterType<PluginEvents>(new ContainerControlledLifetimeManager());
            container.RegisterType<IPluginEvents, PluginEvents>();
            container.RegisterType<IPluginEventsExecutor, PluginEvents>();

            container.RegisterInstance(player, new ContainerControlledLifetimeManager());
            container.RegisterType<ILogger, FileLoggerManager>(new ContainerControlledLifetimeManager());
            container.RegisterType<ICoverFinderManager, CoverFinderManager>(new ContainerControlledLifetimeManager());

            RegisterAimpExtensions(container);

            DependencyResolver.Current = new UnityDependencyResolver(container);
        }

        private static void RegisterAimpExtensions(IUnityContainer container)
        {
            container.RegisterType<IAimpOptionsDialogFrame, OptionsFrame>(new ContainerControlledLifetimeManager());
        }
    }
}
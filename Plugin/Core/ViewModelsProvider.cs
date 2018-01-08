using AIMP.DiskCover.Settings;

namespace AIMP.DiskCover.Core
{
    public class ViewModelsProvider : IViewModelsProvider
    {
        private readonly IPluginEventsExecutor _eventsExecutor;
        private readonly IPluginSettings _settings;

        public ViewModelsProvider(IPluginEventsExecutor eventsExecutor, IPluginSettings settings)
        {
            _eventsExecutor = eventsExecutor;
            _settings = settings;
        }

        public SettingsViewModel GetSettingsViewModel()
        {
            return new SettingsViewModel(_eventsExecutor, _settings);
        }
    }
}
using AIMP.DiskCover.Interfaces;
using AIMP.DiskCover.Settings;

namespace AIMP.DiskCover.Infrastructure
{
    public class ViewModelsProvider : IViewModelsProvider
    {
        private readonly IEventAggregator _aggregator;
        private readonly IPluginSettings _settings;

        public ViewModelsProvider(IPluginSettings settings, IEventAggregator aggregator)
        {
            _settings = settings;
            _aggregator = aggregator;
        }

        public SettingsViewModel GetSettingsViewModel()
        {
            return new SettingsViewModel(_settings, _aggregator);
        }
    }
}
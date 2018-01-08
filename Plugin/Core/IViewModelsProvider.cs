using AIMP.DiskCover.Settings;

namespace AIMP.DiskCover.Core
{
    public interface IViewModelsProvider
    {
        SettingsViewModel GetSettingsViewModel();
    }
}
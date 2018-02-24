using AIMP.DiskCover.Settings;

namespace AIMP.DiskCover.Interfaces
{
    public interface IViewModelsProvider
    {
        SettingsViewModel GetSettingsViewModel();
    }
}
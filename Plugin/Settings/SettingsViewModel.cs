using System.Collections.ObjectModel;
using System.Linq;

namespace AIMP.DiskCover.Settings
{
    public class SettingsViewModel
    {
        public ObservableCollection<FindRule> AppliedRules = new ObservableCollection<FindRule>(Config.Instance.Rules.Where(r => r.Enabled));
        public ObservableCollection<FindRule> AvailableRules = new ObservableCollection<FindRule>(Config.Instance.Rules.Where(r => !r.Enabled));


        #region Localization

        public string GeneralTab => Localization.DiskCover.Options.General;

        public string DisplayIconInTaskbar => Localization.DiskCover.Options.DisplayIconInTaskbar;

        public string EnableResizeModeHotkeys => Localization.DiskCover.Options.EnableResizeModeHotkeys;

        public string SearchRules => Localization.DiskCover.Options.SearchRules;

        public string AvailableRules => Localization.DiskCover.Options.AvailableRules;

        public string AppliedRules => Localization.DiskCover.Options.AppliedRules;

        public string Help => Localization.DiskCover.Options.Help;

        public string ShiftDescription => Localization.DiskCover.Options.ShiftDescription;

        public string AltDescription => Localization.DiskCover.Options.AltDescription;

        public string CtrlDescription => Localization.DiskCover.Options.CtrlDescription;

        public string CancelButton => Localization.DiskCover.Options.Cancel;

        public string SaveButton => Localization.DiskCover.Options.Save;

        #endregion
    }
}
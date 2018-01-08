
using AIMP.DiskCover.Core;
using AIMP.SDK.Player;

namespace AIMP.DiskCover
{
    internal class Localization
    {
		private class LocalizationManager
		{
			private static LocalizationManager _instance;
			private readonly IAimpPlayer _player;

			public static LocalizationManager Instance => _instance ?? (_instance = new LocalizationManager());

			public LocalizationManager()
			{
				_player = DependencyResolver.Current.ResolveService<IAimpPlayer>();
			}

			public string GetLocalizedString(string term, string defaultValue = "")
			{
				var val = _player.MUIManager.GetValue(term);
				return !string.IsNullOrWhiteSpace(val) 
                    ? val 
                    : (!string.IsNullOrEmpty(defaultValue) ? defaultValue : term);
			}
		}

		private const string DiskCoverKey = "DiskCover";
		private const string DiskCoverOptionsKey = "DiskCover.Options";
		public class DiskCover
		{
			

			/// <summary>
			/// Disk Cover
			/// </summary>
			public static string Title => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverKey}\\Title");
			

			/// <summary>
			/// Cover Art
			/// </summary>
			public static string MenuName => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverKey}\\MenuName");
			public class Options
			{
				

				/// <summary>
				/// Cancel
				/// </summary>
				public static string Cancel => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\Cancel");
				

				/// <summary>
				/// Save
				/// </summary>
				public static string Save => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\Save");
				

				/// <summary>
				/// Display an icon in taskbar
				/// </summary>
				public static string DisplayIconInTaskbar => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\DisplayIconInTaskbar");
				

				/// <summary>
				/// Enable resize mode hotkeys
				/// </summary>
				public static string EnableResizeModeHotkeys => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\EnableResizeModeHotkeys");
				

				/// <summary>
				/// General
				/// </summary>
				public static string General => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\General");
				

				/// <summary>
				/// Search rules
				/// </summary>
				public static string SearchRules => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\SearchRules");
				

				/// <summary>
				/// Help
				/// </summary>
				public static string Help => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\Help");
				

				/// <summary>
				/// Settings
				/// </summary>
				public static string Title => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\Title");
				

				/// <summary>
				/// Available rules:
				/// </summary>
				public static string AvailableRules => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\AvailableRules");
				

				/// <summary>
				/// Applied rules:
				/// </summary>
				public static string AppliedRules => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\AppliedRules");
				

				/// <summary>
				/// The window takes square proportions and keeps them.
				/// </summary>
				public static string ShiftDescription => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\ShiftDescription");
				

				/// <summary>
				/// The window takes proportions of the currently displayed picture.
				/// </summary>
				public static string AltDescription => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\AltDescription");
				

				/// <summary>
				/// The window keeps its current proportions.
				/// </summary>
				public static string CtrlDescription => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\CtrlDescription");
				

				/// <summary>
				/// Search for file cover.*
				/// </summary>
				public static string CoverFile => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\CoverFile");
				

				/// <summary>
				/// Load from tags
				/// </summary>
				public static string FromTags => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\FromTags");
				

				/// <summary>
				/// Search for &lt;album name&gt;.*
				/// </summary>
				public static string AlbumFile => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\AlbumFile");
				

				/// <summary>
				/// Search in last.fm
				/// </summary>
				public static string LastFM => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\LastFM");
			}
		}
    }
}

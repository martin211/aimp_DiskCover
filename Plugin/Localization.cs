
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using AIMP.DiskCover.Core;
using AIMP.SDK.Player;


namespace AIMP.DiskCover
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class MarkupLocalizationAttribute : Attribute
    {
        public MarkupLocalizationAttribute(string text)
        {
            Text = text;
        }

        public string Text { get; set; }
    }

    public class LocalizerExtension : MarkupExtension
    {
        private static readonly String _localizedDataFullName = typeof(Localization).FullName;

        public LocalizerExtension()
        {
        }

        public LocalizerExtension(String key)
        {
            Key = key;
        }

        [ConstructorArgument("key")] 
        public String Key { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            String typeName = _localizedDataFullName;

            String[] parts = Key.Split('.');

            // If resource name is composite (is not on the top of hierarchy)
            if (parts.Length > 1)
            {
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    typeName += "+" + parts[i];
                }
            }

            var rootObjectProvider = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
            var isInDesign =rootObjectProvider == null;

            Type containingClassType = Type.GetType(typeName);

            if (containingClassType == null)
            {
                throw new LocalizerException("Containing class is not found: " + typeName);
            }

            // Search for the requested static property.
            PropertyInfo property = containingClassType.GetProperty(parts.Last());

            if (property == null)
            {
                throw new LocalizerException("Property is not found: " + parts.Last());
            }
            // Return its value.
            if (isInDesign)
            {
                var attr = property.GetCustomAttribute<MarkupLocalizationAttribute>();
                return attr != null ? attr.Text : "Value not found";
            }

            return property.GetValue(null, null);
        }
    }

    /// <summary>
    /// An exception that is raised when some error happens 
    /// in <see cref="LocalizerExtension"/>.
    /// </summary>
    public class LocalizerException : ApplicationException
    {
        public LocalizerException(String message)
            :base(message)
        {
            
        }
    }

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
		private const string DiskCoverAlbumArtCatalogKey = "DiskCover.AlbumArtCatalog";
		public class DiskCover
		{
			

			/// <summary>
			/// Disk Cover
			/// </summary>
			[MarkupLocalization("Disk Cover")]
			public static string Title => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverKey}\\Title");
			

			/// <summary>
			/// Cover Art
			/// </summary>
			[MarkupLocalization("Cover Art")]
			public static string MenuName => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverKey}\\MenuName");
			public class Options
			{
				

				/// <summary>
				/// Display an icon in taskbar
				/// </summary>
				[MarkupLocalization("Display an icon in taskbar")]
				public static string DisplayIconInTaskbar => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\DisplayIconInTaskbar");
				

				/// <summary>
				/// Enable resize mode hotkeys
				/// </summary>
				[MarkupLocalization("Enable resize mode hotkeys")]
				public static string EnableResizeModeHotkeys => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\EnableResizeModeHotkeys");
				

				/// <summary>
				/// General
				/// </summary>
				[MarkupLocalization("General")]
				public static string General => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\General");
				

				/// <summary>
				/// Search rules
				/// </summary>
				[MarkupLocalization("Search rules")]
				public static string SearchRules => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\SearchRules");
				

				/// <summary>
				/// Help
				/// </summary>
				[MarkupLocalization("Help")]
				public static string Help => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\Help");
				

				/// <summary>
				/// Settings
				/// </summary>
				[MarkupLocalization("Settings")]
				public static string Title => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\Title");
				

				/// <summary>
				/// Available rules:
				/// </summary>
				[MarkupLocalization("Available rules:")]
				public static string AvailableRules => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\AvailableRules");
				

				/// <summary>
				/// Applied rules:
				/// </summary>
				[MarkupLocalization("Applied rules:")]
				public static string AppliedRules => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\AppliedRules");
				

				/// <summary>
				/// The window takes square proportions and keeps them.
				/// </summary>
				[MarkupLocalization("The window takes square proportions and keeps them.")]
				public static string ShiftDescription => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\ShiftDescription");
				

				/// <summary>
				/// The window takes proportions of the currently displayed picture.
				/// </summary>
				[MarkupLocalization("The window takes proportions of the currently displayed picture.")]
				public static string AltDescription => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\AltDescription");
				

				/// <summary>
				/// The window keeps its current proportions.
				/// </summary>
				[MarkupLocalization("The window keeps its current proportions.")]
				public static string CtrlDescription => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\CtrlDescription");
				

				/// <summary>
				/// Search for file cover.*
				/// </summary>
				[MarkupLocalization("Search for file cover.*")]
				public static string CoverFile => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\CoverFile");
				

				/// <summary>
				/// Load from tags
				/// </summary>
				[MarkupLocalization("Load from tags")]
				public static string FromTags => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\FromTags");
				

				/// <summary>
				/// Search for &lt;album name&gt;.*
				/// </summary>
				[MarkupLocalization("Search for &lt;album name&gt;.*")]
				public static string AlbumFile => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\AlbumFile");
				

				/// <summary>
				/// Search in last.fm
				/// </summary>
				[MarkupLocalization("Search in last.fm")]
				public static string LastFM => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\LastFM");
				

				/// <summary>
				/// From AIMP library
				/// </summary>
				[MarkupLocalization("From AIMP library")]
				public static string Aimp => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\Aimp");
				

				/// <summary>
				/// Search rules
				/// </summary>
				[MarkupLocalization("Search rules")]
				public static string Rules => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\Rules");
				

				/// <summary>
				/// Enable debug mode
				/// </summary>
				[MarkupLocalization("Enable debug mode")]
				public static string DebugMode => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverOptionsKey}\\DebugMode");
			}
			public class AlbumArtCatalog
			{
				

				/// <summary>
				/// Last.fm
				/// </summary>
				[MarkupLocalization("Last.fm")]
				public static string Name => LocalizationManager.Instance.GetLocalizedString($"{DiskCoverAlbumArtCatalogKey}\\Name");
			}
		}
    }
}

using System.Collections.Generic;

namespace AIMP.DiskCover.Settings
{
    public interface IPluginSettings
    {
        bool IsEnabled { get; set; }

        double Height { get; set; }

        double Width { get; set; }

        double Left { get; set; }

        double Top { get; set; }

        bool ShowInTaskbar { get; set; }

        bool EnableHotKeys { get; set; }

        bool SaveImageAtTags { get; set; }

        /// <summary>
        /// Gets a collection of cover finding rules.
        /// </summary>
        IEnumerable<FindRule> Rules { get; }

        IEnumerable<FindRule> AppliedRules { get; set; }

        bool DebugMode { get; set; }
    }
}
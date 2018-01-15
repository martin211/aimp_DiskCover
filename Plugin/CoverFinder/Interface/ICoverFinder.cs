using System.Drawing;
using AIMP.SDK.FileManager;
using AIMP.SDK.Player;

namespace AIMP.DiskCover
{
    public interface ICoverFinder
    {
        /// <summary>
        /// Gets or sets finder name.
        /// </summary>
        string Name { get; }

        CoverRuleType RuleType { get; }

        Bitmap GetBitmap(TrackInfo track);

        Bitmap GetBitmap(TrackInfo track, FindRule currentRule);
    }
}

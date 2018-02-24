using System.Drawing;
using System.Threading.Tasks;
using AIMP.DiskCover.CoverFinder;
using AIMP.DiskCover.Infrastructure;

namespace AIMP.DiskCover.Interfaces
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

        Task<Bitmap> GetBitmapAsync(TrackInfo track);

        Task<Bitmap> GetBitmapAsync(TrackInfo track, FindRule currentRule);
    }
}

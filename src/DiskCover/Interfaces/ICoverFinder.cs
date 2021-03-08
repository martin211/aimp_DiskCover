using System.Drawing;
using System.Threading.Tasks;
using AIMP.DiskCover.Infrastructure;
using AIMP.SDK.Player;

namespace AIMP.DiskCover.Interfaces
{
    public interface ICoverFinder
    {
        /// <summary>
        /// Gets or sets finder name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the type of the rule.
        /// </summary>
        CoverRuleType RuleType { get; }

        /// <summary>
        /// Gets the bitmap.
        /// </summary>
        /// <param name="player">The player.</param>
        Bitmap GetBitmap(IAimpPlayer player);

        /// <summary>
        /// Gets the bitmap.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="currentRule">The current rule.</param>
        Bitmap GetBitmap(IAimpPlayer player, FindRule currentRule);

        /// <summary>
        /// Gets the bitmap asynchronous.
        /// </summary>
        /// <param name="player">The player.</param>
        Task<Bitmap> GetBitmapAsync(IAimpPlayer player);

        /// <summary>
        /// Gets the bitmap asynchronous.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="currentRule">The current rule.</param>
        Task<Bitmap> GetBitmapAsync(IAimpPlayer player, FindRule currentRule);
    }
}

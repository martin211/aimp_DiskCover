using System;
using System.Drawing;
using AIMP.SDK.Services.Player;

namespace AIMP.DiskCover
{
    public interface ICoverFinder
    {
        /// <summary>
        /// Gets or sets finder name.
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Returns a cover art image for the specified track.
        /// </summary>
        /// <param name="trackInfo">
        /// An object contating data of currently playing track.
        /// </param>
        /// <param name="concreteRule">
        /// A rule that should be used to search for cover art image.
        /// </param>
        /// <returns>Result of cover art image search.</returns>
       // Bitmap GetBitmap(TrackInfo trackInfo, FindRule concreteRule);

        Bitmap GetBitmap(IAimpPlayer player, FindRule concreteRule);
    }
}

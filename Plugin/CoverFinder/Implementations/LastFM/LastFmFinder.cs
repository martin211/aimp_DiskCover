using System;
using System.ComponentModel.Composition;
using System.Windows;
using LastFmLib.General;
using System.Drawing;
using AIMP.SDK.Services.Player;

namespace AIMP.DiskCover.LastFM
{
    using LastFmLib.API20.Types;

    [Export(typeof(ICoverFinder))]
    public class LastFmFinder : ICoverFinder
    {
        public const string ModuleName = "lastfmfinder";

        private const string ApiKey = "f5610848fef2dc0abd449e6268acb1d2";
        private const string SecretKey = "a5008c1485f6639a9c4edfc7cc773e03";

        public String Name
        {
            get { return ModuleName; }
        }

        /// <summary>
        /// DO NOT DELETE. Used in composition process.
        /// </summary>
        public LastFmFinder()
        {
            LastFmLib.API20.Settings20.AuthData = new AuthData(new MD5Hash(ApiKey), new MD5Hash(SecretKey));
        }

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
        public Bitmap GetBitmap(TrackInfo trackInfo, FindRule concreteRule)
        {
            Bitmap result = null;

            try
            {
                if (!trackInfo.IsStream)
                {
                    var n = new LastFmLib.API20.Album.AlbumGetInfo(trackInfo.Artist, trackInfo.Album);
                    n.Start();
                    if (n.Succeeded)
                    {
                        if (n.Result.HasAnyImage)
                        {
                            result = n.Result.DownloadImage(LastFmLib.API20.modEnums.ImageSize.Original);
                            if (result == null)
                            {
                                if (n.Result.ImageLarge != null)
                                    result = n.Result.DownloadImage(LastFmLib.API20.modEnums.ImageSize.ExtraLarge);
                                else if (n.Result.ImageLarge != null)
                                    result = n.Result.DownloadImage(LastFmLib.API20.modEnums.ImageSize.Large);
                                else if (n.Result.ImageMedium != null)
                                    result = n.Result.DownloadImage(LastFmLib.API20.modEnums.ImageSize.Medium);
                                else if (n.Result.ImageSmall != null)
                                    result = n.Result.DownloadImage(LastFmLib.API20.modEnums.ImageSize.Small);
                            }
                        }
                    }
                }
                else
                {
                    // get bitmap for radio
                    String[] s = String.IsNullOrEmpty(trackInfo.Artist)
                        ? trackInfo.Title.Split('-')
                        : new string[] { trackInfo.Artist, trackInfo.Title };

                    if (s.Length > 1)
                    {
                        var n = new LastFmLib.API20.Tracks.TrackGetInfo(s[0].Trim(), s[1].Trim());
                        n.Start();
                        if (n.Succeeded)
                        {
                            var info = (TrackInformation)n.Result;
                            if (info.Album != null && info.Album.HasAnyImage)
                            {
                                result = info.Album.DownloadImage(LastFmLib.API20.modEnums.ImageSize.Original);
                                if (result == null)
                                {
                                    if (info.Album.ImageLarge != null)
                                        result = info.Album.DownloadImage(LastFmLib.API20.modEnums.ImageSize.ExtraLarge);
                                    else if (info.Album.ImageLarge != null)
                                        result = info.Album.DownloadImage(LastFmLib.API20.modEnums.ImageSize.Large);
                                    else if (n.Result.ImageMedium != null)
                                        result = info.Album.DownloadImage(LastFmLib.API20.modEnums.ImageSize.Medium);
                                    else if (n.Result.ImageSmall != null)
                                        result = info.Album.DownloadImage(LastFmLib.API20.modEnums.ImageSize.Small);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(ex.ToString(), "LastFm Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            return result;
        }

        public Bitmap GetBitmap(IAimpPlayer player, FindRule concreteRule)
        {
            return GetBitmap(new TrackInfo(player), concreteRule);
        }
    }
}

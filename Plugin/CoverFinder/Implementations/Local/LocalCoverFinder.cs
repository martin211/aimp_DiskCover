using System;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.ComponentModel.Composition;
using AIMP.SDK.Interfaces;

namespace AIMP.DiskCover
{

    [Export(typeof(ICoverFinder))]
    public class LocalCoverFinder : ICoverFinder
    {
        public const string ModuleName = "localfinder";

        public string Name
        {
            get { return ModuleName; }
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


            var fileDir = Path.GetDirectoryName(trackInfo.FileName);

            if (String.IsNullOrEmpty(fileDir))
            {
                return null;
            }

            var dir = new DirectoryInfo(fileDir);

            String searchPattern;

            if (concreteRule.Rule == CoverRuleType.AlbumFile)
            {
                searchPattern = trackInfo.Album + ".*";
            }
            else if (concreteRule.Rule == CoverRuleType.CoverFile)
            {
                searchPattern = "cover.*";
            }
            else
            {
                throw new NotSupportedException("The " + concreteRule.Rule + " rule cannot be handled by this method.");
            }

            // In case the pattern is not correct for the file system, do not proceed further.
            if (Core.Algorithms.ContainsInvalidFileNameChars(searchPattern))
            {
                return null;
            }

            var covers = dir.GetFiles(searchPattern, SearchOption.TopDirectoryOnly);

            if (covers.Length > 0)
            {
                foreach (var cover in covers)
                {
                    Debug.WriteLine(String.Format("Loading image from HDD : {0}", cover.Name));

                    var ms = new MemoryStream(File.ReadAllBytes(cover.FullName));
                    try
                    {
                        result = (Bitmap)Image.FromStream(ms);
                        break;

                    }
                    catch (ArgumentException)
                    {
                        // Means that this is not an image or this format is not supported.
                        ms.Dispose();
                    }
                }
            }

            return result;
        }

        public Bitmap GetBitmap(IAimpPlayer player, FindRule concreteRule)
        {
            return GetBitmap(new TrackInfo(player), concreteRule);
        }
    }
}

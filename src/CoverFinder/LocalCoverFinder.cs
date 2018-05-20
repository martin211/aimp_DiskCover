using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using AIMP.DiskCover.Infrastructure;
using AIMP.DiskCover.Interfaces;
using AIMP.SDK.Player;

namespace AIMP.DiskCover.CoverFinder
{
    [Export(typeof(ICoverFinder))]
    public class LocalCoverFinder : ICoverFinder
    {
        public const string ModuleName = "localfinder";

        public string Name => ModuleName;

        public CoverRuleType RuleType => CoverRuleType.CoverFile;

        public Bitmap GetBitmap(IAimpPlayer player)
        {
            return null;
        }

        public Bitmap GetBitmap(IAimpPlayer player, FindRule concreteRule)
        {
            var trackInfo = new TrackInfo(player.CurrentFileInfo);
            Bitmap result = null;

            var fileDir = Path.GetDirectoryName(trackInfo.FileName);

            if (string.IsNullOrEmpty(fileDir))
            {
                return null;
            }

            var dir = new DirectoryInfo(fileDir);
            string searchPattern;

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
            if (Algorithms.ContainsInvalidFileNameChars(searchPattern))
            {
                return null;
            }

            var covers = dir.GetFiles(searchPattern, SearchOption.TopDirectoryOnly);

            if (covers.Length > 0)
            {
                foreach (var cover in covers)
                {
                    Debug.WriteLine($"Loading image from HDD : {cover.Name}");

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

        /// <inheritdoc />
        public Task<Bitmap> GetBitmapAsync(IAimpPlayer player)
        {
            return Task.FromResult(GetBitmap(player));
        }

        /// <inheritdoc />
        public Task<Bitmap> GetBitmapAsync(IAimpPlayer player, FindRule currentRule)
        {
            return Task.FromResult(GetBitmap(player, currentRule));
        }
    }
}

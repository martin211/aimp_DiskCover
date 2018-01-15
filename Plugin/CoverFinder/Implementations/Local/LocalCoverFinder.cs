using System;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.ComponentModel.Composition;

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

        public CoverRuleType RuleType => CoverRuleType.CoverFile;

        public Bitmap GetBitmap(TrackInfo track)
        {
            throw new NotImplementedException();
        }

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
    }
}

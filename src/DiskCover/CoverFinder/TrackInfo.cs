using AIMP.SDK.FileManager.Objects;

namespace AIMP.DiskCover.CoverFinder
{
    /// <summary>
    /// Container for data of various data of 
    /// a musical track which might be required by
    /// cover finder plugins.
    /// </summary>
    public class TrackInfo
    {
        public TrackInfo()
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="TrackInfo"/> class.
        /// </summary>
        public TrackInfo(IAimpFileInfo trackInfo)
        {
            Artist = trackInfo.Artist;
            Album = trackInfo.Album;
            Title = trackInfo.Title;
            FileName = trackInfo.FileName;
        }

        public bool IsStream => FileName.StartsWith("http") || FileName.StartsWith("https") || FileName.StartsWith("ftp") || FileName.Contains("://");

        public string Artist { get; set; }

        public string Album { get; set; }
        
        public string Title { get; set; }
        
        public string FileName { get; set; }
    }
}

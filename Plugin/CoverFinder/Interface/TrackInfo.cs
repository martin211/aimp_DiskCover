using System;
using System.Diagnostics.Contracts;
using AIMP.SDK;
using AIMP.SDK.Interfaces;


namespace AIMP.DiskCover
{
    /// <summary>
    /// Container for data of various data of 
    /// a musical track which might be required by
    /// cover finder plugins.
    /// </summary>
    public class TrackInfo
    {
        private IAimpPlayer _player;

        /// <summary>
        /// Creates an instance of <see cref="TrackInfo"/> class.
        /// </summary>
        /// <param name="player">An instance of AIMP player to load data from.</param>
        public TrackInfo(IAimpPlayer player)
        {
            Contract.Requires(player != null);
            _player = player;

            var trackInfo = player.CurrentFileInfo;          
            //StreamType = player.CurrentPlayingInfo.StreamType;

            IsStream = trackInfo.FileName.StartsWith("http") || trackInfo.FileName.StartsWith("https") || trackInfo.FileName.StartsWith("ftp");

            if (trackInfo != null)
            {
                Artist = trackInfo.Artist;
                Album = trackInfo.Album;
                Title = trackInfo.Title;
                FileName = trackInfo.FileName;
            }
        }

        public bool IsStream { get; private set; }

        public String Artist { get; private set; }

        public String Album { get; private set; }
        
        public String Title { get; private set; }
        
        public String FileName { get; private set; }
    }
}

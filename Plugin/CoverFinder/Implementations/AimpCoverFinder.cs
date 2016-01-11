namespace AIMP.DiskCover.CoverFinder.Implementations
{
    using System;
    using System.ComponentModel.Composition;
    using System.Drawing;
    using System.Threading;

    using AIMP.SDK.AlbumArtManager;
    using AIMP.SDK.Player;

    [Export(typeof(ICoverFinder))]
    public class AimpCoverFinder : ICoverFinder
    {
        public const string ModuleName = "AIMPCOVERART";

                            //         _player.AlbumArtManager.Completed += (sender, args) =>
                            //    {
                            //        result = args.CoverImage;
                            //    };

                            //

        private AutoResetEvent _resetEvent;

        private Bitmap _result;

        private Object _lock;

        private Bitmap Result
        {
            get
            {
                lock (_lock)
                {
                    return _result;
                }
            }

            set
            {
                lock (_lock)
                {
                    _result = value;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public AimpCoverFinder()
        {
            _resetEvent = new AutoResetEvent(false);
            _lock = new object();
        }

        #region Implementation of ICoverFinder

        /// <summary>
        /// Gets or sets finder name.
        /// </summary>
        public string Name
        {
            get
            {
                return ModuleName;
            }
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
            throw new System.NotImplementedException();
        }

        public Bitmap GetBitmap(IAimpPlayer player, FindRule concreteRule)
        {
            player.AlbumArtManager.Completed += (sender, args) =>
                {
                    Result = args.CoverImage;
                    _resetEvent.Set();
                };

            player.AlbumArtManager.GetImage(player.CurrentFileInfo, AimpFingCovertArtType.None, null);
            _resetEvent.WaitOne(new TimeSpan(0, 0, 0, 20));
           
            return Result;
        }

        #endregion
    }
}
using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using AIMP.DiskCover.Infrastructure;
using AIMP.DiskCover.Interfaces;
using AIMP.SDK.AlbumArtManager;
using AIMP.SDK.Logger;
using AIMP.SDK.Player;

namespace AIMP.DiskCover.CoverFinder
{
    [Export(typeof(ICoverFinder))]
    public class AimpCoverFinder : ICoverFinder
    {
        public const string ModuleName = "AIMPCOVERART";

        private AutoResetEvent _resetEvent;

        private ILogger Logger => DependencyResolver.Current.ResolveService<ILogger>();

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

        public CoverRuleType RuleType => CoverRuleType.AIMP;
        public Bitmap GetBitmap(TrackInfo track)
        {
            throw new NotImplementedException();
        }

        public Bitmap GetBitmap(TrackInfo track, FindRule currentRule)
        {
            throw new NotImplementedException();
        }

        public Task<Bitmap> GetBitmapAsync(TrackInfo track)
        {
            throw new NotImplementedException();
        }

        public Task<Bitmap> GetBitmapAsync(TrackInfo track, FindRule currentRule)
        {
            throw new NotImplementedException();
        }

        public Bitmap GetBitmap(IAimpPlayer player)
        {
            player.AlbumArtManager.Completed += (sender, args) =>
                {
                    Result = args.CoverImage;
                    _resetEvent.Set();
                };

            Logger.Write($"Request [{ModuleName}-{nameof(GetBitmap)}]: artist {player.CurrentFileInfo.Artist} track {player.CurrentFileInfo.Title}");
            player.AlbumArtManager.GetImage(player.CurrentFileInfo, AimpFindCovertArtType.None, null);
            _resetEvent.WaitOne(new TimeSpan(0, 0, 0, 20));
           
            return Result;
        }

        public Bitmap GetBitmap(IAimpPlayer player, FindRule currentRule)
        {
            return GetBitmap(player);
        }
    }
}
using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using AIMP.DiskCover.Infrastructure;
using AIMP.DiskCover.Interfaces;
using AIMP.SDK;
using AIMP.SDK.AlbumArt;

namespace AIMP.DiskCover.CoverFinder
{
    [Export(typeof(ICoverFinder))]
    public class AimpCoverFinder : ICoverFinder
    {
        public const string ModuleName = "AIMPCOVERART";

        private AutoResetEvent _resetEvent;

        private Bitmap _result;

        private object _lock;

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
        public string Name => ModuleName;

        public CoverRuleType RuleType => CoverRuleType.AIMP;

        public Bitmap GetBitmap(IAimpPlayer player)
        {
            player.ServiceAlbumArt.Completed += (sender, args) =>
                {
                    Result = args.CoverImage;
                    _resetEvent.Set();
                };

            player.ServiceAlbumArt.Get2(player.ServicePlayer.CurrentFileInfo, AimpFindCovertArtType.None, null);
            _resetEvent.WaitOne(new TimeSpan(0, 0, 0, 20));
           
            return Result;
        }

        public Bitmap GetBitmap(IAimpPlayer player, FindRule currentRule)
        {
            return GetBitmap(player);
        }

        public Task<Bitmap> GetBitmapAsync(IAimpPlayer player)
        {
            return Task.FromResult(GetBitmap(player));
        }

        public Task<Bitmap> GetBitmapAsync(IAimpPlayer player, FindRule currentRule)
        {
            throw new NotImplementedException();
        }
    }
}
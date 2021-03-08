using System;
using System.Drawing;
using AIMP.DiskCover.Interfaces;
using AIMP.SDK;
using AIMP.SDK.AlbumArtManager;
using AIMP.SDK.FileManager;
using AIMP.SDK.Player;

namespace AIMP.DiskCover.Infrastructure.AlbumArtCatalog
{
    public class AimpExtensionAlbumArtCatalog : IAimpExtensionAlbumArtCatalog
    {
        private readonly ICoverFinderManager _coverFinderManager;
        private readonly IAimpPlayer _player;

        public AimpExtensionAlbumArtCatalog(IAimpPlayer player, ICoverFinderManager coverFinderManager)
        {
            _coverFinderManager = coverFinderManager;
            _player = player;
        }

        AimpActionResult<string> IAimpExtensionAlbumArtCatalog.GetName()
        {
            return new AimpActionResult<string>(ActionResultType.OK, Localization.DiskCover.AlbumArtCatalog.Name);
        }

        public AimpActionResult<Bitmap> Show(string fileUrl, string artist, string album)
        {
            Bitmap img = null;
            var result = _player.ServiceThreadPool.Execute(new AimpTask(() =>
            {
                img = _coverFinderManager.FindCoverImage(fileUrl, artist, album);
                return new AimpActionResult(img != null ? ActionResultType.OK : ActionResultType.Fail);
            }));

            _player.ServiceThreadPool.WaitFor(result.Result);
            return new AimpActionResult<Bitmap>(result.ResultType, img);
        }

        AimpActionResult<Bitmap> IAimpExtensionAlbumArtCatalog.GetIcon()
        {
            return new AimpActionResult<Bitmap>(ActionResultType.OK, Properties.Resources.diskcover);
        }

        public AimpActionResult<Bitmap> Show(IAimpFileInfo fileInfo)
        {
            Bitmap img = null;
            var result = _player.ServiceThreadPool.Execute(new AimpTask(() =>
            {
                img = _coverFinderManager.FindCoverImage(fileInfo);
                return new AimpActionResult(img != null ? ActionResultType.OK : ActionResultType.Fail);
            }));

            _player.ServiceThreadPool.WaitFor(result.Result);
            return new AimpActionResult<Bitmap>(result.ResultType, img);
        }
    }
}
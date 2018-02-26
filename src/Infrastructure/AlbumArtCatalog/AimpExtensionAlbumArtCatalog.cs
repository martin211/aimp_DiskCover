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

        public AimpActionResult Show(IAimpFileInfo fileInfo, out Bitmap image)
        {
            UIntPtr taskId = UIntPtr.Zero;
            Bitmap img = null;
            var result = _player.ServiceThreadPool.Execute(new AimpTask(() =>
            {
                img = _coverFinderManager.FindCoverImage(fileInfo);
                return AimpActionResult.Ok;
            }), out taskId);

            _player.ServiceThreadPool.WaitFor(taskId);
            image = img;
            return result;
        }

        public Bitmap GetIcon()
        {
            return Properties.Resources.diskcover;
        }

        public string GetName()
        {
            return Localization.DiskCover.AlbumArtCatalog.Name;
        }

        public AimpActionResult Show(string fileUrl, string artist, string album, out Bitmap image)
        {
            UIntPtr taskId = UIntPtr.Zero;
            Bitmap img = null;
            var result = _player.ServiceThreadPool.Execute(new AimpTask(() =>
            {
                img = _coverFinderManager.FindCoverImage(fileUrl, artist, album);
                return AimpActionResult.Ok;
            }), out taskId);

            _player.ServiceThreadPool.WaitFor(taskId);
            image = img;
            return result;
        }
    }
}
using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows;
using AIMP.DiskCover.Infrastructure;
using AIMP.DiskCover.Interfaces;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;

namespace AIMP.DiskCover.CoverFinder
{
    [Export(typeof(ICoverFinder))]
    public class LastFmFinder : ICoverFinder
    {
        public const string ModuleName = "lastfmfinder";

        private const string ApiKey = "f5610848fef2dc0abd449e6268acb1d2";
        private const string SecretKey = "a5008c1485f6639a9c4edfc7cc773e03";

        // TODO: Use DI
        private ILogger Logger => DependencyResolver.Current.ResolveService<ILogger>();

        private LastfmClient _client;

        public string Name => ModuleName;

        public CoverRuleType RuleType => CoverRuleType.LastFM;

        public Bitmap GetBitmap(TrackInfo trackInfo)
        {
            return GetBitmap(trackInfo, null);
        }

        public Bitmap GetBitmap(TrackInfo trackInfo, FindRule currentRule)
        {
            return GetBitmapAsync(trackInfo, currentRule).Result;
        }

        public Task<Bitmap> GetBitmapAsync(TrackInfo trackInfo)
        {
            return GetBitmapAsync(trackInfo, null);
        }

        public Task<Bitmap> GetBitmapAsync(TrackInfo trackInfo, FindRule currentRule)
        {
            try
            {
                return GetTrackCoverAsync(trackInfo, currentRule);
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(ex.ToString(), "LastFm Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            return null;
        }

        public LastFmFinder()
        {
            _client = new LastfmClient(ApiKey, SecretKey);
        }

        private Bitmap DownloadImage(Uri uri)
        {
            Bitmap bitmap;
            if (uri == null)
            {
                bitmap = null;
            }
            else
            {
                Stream imageStream = GetImageStream(uri);
                if (imageStream == null)
                {
                    bitmap = null;
                }
                else
                {
                    GC.Collect();
                    bitmap = new Bitmap(imageStream);
                }
            }

            return bitmap;
        }

        private Stream GetImageStream(Uri url)
        {
            Stream stream;
            if ((object)url == null)
            {
                stream = null;
            }
            else
            {
                ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                       | SecurityProtocolType.Tls11
                                                       | SecurityProtocolType.Tls12
                                                       | SecurityProtocolType.Ssl3;

                HttpClient client = new HttpClient();
                var t = client.GetAsync(url);
                t.Wait();
                var response = t.Result;

                try
                {
                    response.EnsureSuccessStatusCode();
                    var d = response.Content.ReadAsStreamAsync();
                    d.Wait();
                    stream = d.Result;
                }
                catch (Exception e)
                {
                    Logger.Write(e);
                    return null;
                }
            }

            return stream;
        }

        public bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private async Task<LastTrack> GetTrackInfo(string artist, string track)
        {
            Logger.Write($"Request [{ModuleName}-{nameof(GetTrackInfo)}]: artist {artist} track {track}");
            return await GetData(() => _client.Track.GetInfoAsync(track, artist, string.Empty));
        }

        private async Task<LastAlbum> GetAlbumInfo(string artist, string album)
        {
            Logger.Write($"Request [{ModuleName}-{nameof(GetAlbumInfo)}]: artist {artist} album {album}");
            return await GetData(() => _client.Album.GetInfoAsync(artist, album, true));
        }

        private async Task<LastArtist> GetArtistInfo(string artist)
        {
            Logger.Write($"Request [{ModuleName}-{nameof(GetArtistInfo)}]: artist {artist}");
            return await GetData(() => _client.Artist.GetInfoAsync(artist, "en", true));
        }

        private async Task<TData> GetData<TData>(Func<Task<LastResponse<TData>>> action)
        {
            LastResponse<TData> data = await action();
            if (data.Status == LastResponseStatus.Successful)
            {
                var content = data.Content;
                Logger.Write($"Response [{ModuleName}]: {content}");
                return content;
            }

            return default;
        }

        private async Task<Bitmap> GetTrackCoverAsync(TrackInfo trackInfo, FindRule currentRule)
        {
            var album = trackInfo.Album;
            var artist = trackInfo.Artist;
            var title = trackInfo.Title;

            if (trackInfo.IsStream && string.IsNullOrEmpty(trackInfo.Artist) && string.IsNullOrEmpty(trackInfo.Title))
            {
                var s = trackInfo.Title.Split('-');
                artist = s[0];
                title = s[1];
            }

            if (!string.IsNullOrWhiteSpace(album))
            {
                var albumInfo = await GetAlbumInfo(artist, album);
                if (albumInfo.Images != null && albumInfo.Images.Any())
                {
                    return DownloadImage(albumInfo.Images.Largest);
                }
            }

            var ti = await GetTrackInfo(artist, title);
            if (!string.IsNullOrWhiteSpace(ti.AlbumName))
            {
                var albumInfo = await GetAlbumInfo(artist, ti.AlbumName);
                if (albumInfo.Images != null && albumInfo.Images.Any())
                {
                    return DownloadImage(albumInfo.Images.Largest);
                }

                if (ti.Images != null && ti.Images.Any())
                {
                    return DownloadImage(ti.Images.Largest);
                }
            }

            var artistInfo = await GetArtistInfo(artist);
            if (artistInfo.MainImage != null)
            {
                return DownloadImage(artistInfo.MainImage.Largest);
            }

            return null;
        }
    }
}

using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;

namespace AIMP.DiskCover.LastFM
{
    [Export(typeof(ICoverFinder))]
    public class LastFmFinder : ICoverFinder
    {
        public const string ModuleName = "lastfmfinder";

        private const string ApiKey = "f5610848fef2dc0abd449e6268acb1d2";
        private const string SecretKey = "a5008c1485f6639a9c4edfc7cc773e03";

        private LastfmClient _client;

        public String Name
        {
            get { return ModuleName; }
        }

        public CoverRuleType RuleType => CoverRuleType.LastFM;

        public Bitmap GetBitmap(TrackInfo trackInfo)
        {
            return GetBitmap(trackInfo, null);
        }

        public Bitmap GetBitmap(TrackInfo trackInfo, FindRule currentRule)
        {
            try
            {
                // get bitmap for radio
                String[] s = String.IsNullOrEmpty(trackInfo.Artist)
                    ? trackInfo.Title.Split('-')
                    : new[] { trackInfo.Artist.Trim(), trackInfo.Title.Trim() };

                if (s.Length > 1)
                {
                    var track = GetTrackInfo(s[0], s[1]);
                    track.Wait();
                    if (!string.IsNullOrWhiteSpace(track.Result?.AlbumName))
                    {
                        var album = GetAlbumInfo(s[0], track.Result.AlbumName);
                        album.Wait();
                        if (album.Result?.Images != null)
                        {
                            return DownloadImage(album.Result.Images.Largest);
                        }

                        if (track.Result.Images != null)
                        {
                            return DownloadImage(track.Result.Images.Largest);
                        }
                    }

                    var artist = GetArtistInfo(s[0]);
                    artist.Wait();
                    if (artist.Result?.MainImage != null)
                    {
                        return DownloadImage(artist.Result.MainImage.Largest);
                    }
                }
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

        private static Stream GetImageStream(Uri url)
        {
            Stream stream;
            if ((object)url == null)
            {
                stream = (Stream)null;
            }
            else
            {
                WebRequest webRequest = WebRequest.CreateDefault(url);
                webRequest.Method = "GET";
                WebResponse response;
                try
                {
                    response = webRequest.GetResponse();
                }
                catch (Exception ex)
                {
                    return null;
                }

                stream = response.GetResponseStream();
            }
            return stream;
        }

        private async Task<LastTrack> GetTrackInfo(string artist, string track)
        {
            return await GetData(() => _client.Track.GetInfoAsync(track, artist, string.Empty));
        }

        private async Task<LastAlbum> GetAlbumInfo(string artist, string album)
        {
            return await GetData(() => _client.Album.GetInfoAsync(artist, album, true));
        }

        private async Task<LastArtist> GetArtistInfo(string artist)
        {
            return await GetData(() => _client.Artist.GetInfoAsync(artist, "en", true));
        }

        private async Task<TData> GetData<TData>(Func<Task<LastResponse<TData>>> action)
        {
            LastResponse<TData> data = await action();
            if (data.Status == LastResponseStatus.Successful)
            {
                return data.Content;
            }

            return default(TData);
        }
    }
}

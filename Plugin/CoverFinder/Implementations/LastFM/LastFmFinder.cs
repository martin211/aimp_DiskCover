using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;

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
            Bitmap result = null;

            try
            {
                if (!trackInfo.IsStream)
                {
                    var response = _client.Album.GetInfoAsync(trackInfo.Artist, trackInfo.Album);
                    response.Wait();
                    if (response.IsCompleted && response.Result.Status == LastResponseStatus.Successful)
                    {
                        if (response.Result.Content != null && response.Result.Content.Images.Any())
                        {
                            result = DownloadImage(response.Result.Content.Images.Largest);
                        }
                    }
                }
                else
                {
                    // get bitmap for radio
                    String[] s = String.IsNullOrEmpty(trackInfo.Artist)
                        ? trackInfo.Title.Split('-')
                        : new[] { trackInfo.Artist, trackInfo.Title };

                    if (s.Length > 1)
                    {
                        var n = _client.Track.GetInfoAsync(s[0].Trim(), s[1].Trim(), string.Empty);
                        n.Wait();
                        if (n.IsCompleted)
                        {
                            var info = n.Result.Content;
                            if (info.Images.Any())
                            {
                                result = DownloadImage(info.Images.Largest);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(ex.ToString(), "LastFm Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            return result;
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
    }
}

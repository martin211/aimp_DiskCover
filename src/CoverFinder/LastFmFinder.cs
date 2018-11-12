﻿using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using AIMP.DiskCover.Infrastructure;
using AIMP.DiskCover.Interfaces;
using AIMP.SDK.Logger;
using AIMP.SDK.Player;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using Newtonsoft.Json;

namespace AIMP.DiskCover.CoverFinder
{
    [Export(typeof(ICoverFinder))]
    public class LastFmFinder : ICoverFinder
    {
        public const string ModuleName = "lastfmfinder";

        private const string ApiKey = "f5610848fef2dc0abd449e6268acb1d2";
        private const string SecretKey = "a5008c1485f6639a9c4edfc7cc773e03";

        /// <summary>
        /// Initializes a new instance of the <see cref="LastFmFinder"/> class.
        /// </summary>
        public LastFmFinder()
        {
            _client = new LastfmClient(ApiKey, SecretKey);
        }

        // TODO: Use DI
        private ILogger Logger => DependencyResolver.Current.ResolveService<ILogger>();

        private readonly LastfmClient _client;

        /// <inheritdoc />
        public string Name => ModuleName;

        /// <inheritdoc />
        public CoverRuleType RuleType => CoverRuleType.LastFM;

        /// <inheritdoc />
        public Bitmap GetBitmap(IAimpPlayer player)
        {
            return GetBitmap(player, null);
        }

        /// <inheritdoc />
        public Bitmap GetBitmap(IAimpPlayer player, FindRule currentRule)
        {
            return GetBitmapAsync(player, currentRule).Result;
        }

        /// <inheritdoc />
        public Task<Bitmap> GetBitmapAsync(IAimpPlayer player)
        {
            return GetBitmapAsync(player, null);
        }

        /// <inheritdoc />
        public Task<Bitmap> GetBitmapAsync(IAimpPlayer player, FindRule currentRule)
        {
            try
            {
                return GetTrackCoverAsync(player, currentRule);
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
#if DEBUG
                MessageBox.Show(ex.ToString(), "LastFm Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }

            return null;
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
                stream = null;
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
                catch (Exception)
                {
                    return null;
                }

                stream = response.GetResponseStream();
            }
            return stream;
        }

        private async Task<LastTrack> GetTrackInfo(string artist, string track)
        {
            Logger.Write($"Request [{ModuleName}-{nameof(GetTrackInfo)}]: artist {artist} track: {track}");
            return await GetData(() => _client.Track.GetInfoAsync(track, artist, string.Empty));
        }

        private async Task<LastAlbum> GetAlbumInfo(string artist, string album)
        {
            Logger.Write($"Request [{ModuleName}-{nameof(GetAlbumInfo)}]: artist {artist} album: {album}");
            return await GetData(() => _client.Album.GetInfoAsync(artist, album, true));
        }

        private async Task<LastArtist> GetArtistInfo(string artist)
        {
            Logger.Write($"Request [{ModuleName}-{nameof(GetArtistInfo)}]: artist: {artist}");
            return await GetData(() => _client.Artist.GetInfoAsync(artist, "en", true));
        }

        private async Task<TData> GetData<TData>(Func<Task<LastResponse<TData>>> action)
        {
            LastResponse<TData> data = await action();
            if (data.Status == LastResponseStatus.Successful)
            {
                var content = data.Content;
                Logger.Write($"Response [{ModuleName}]: {JsonConvert.SerializeObject(content)}");
                return content;
            }

            return default(TData);
        }

        private async Task<Bitmap> GetTrackCoverAsync(IAimpPlayer player, FindRule currentRule)
        {
            var trackInfo = new TrackInfo(player.CurrentFileInfo);

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

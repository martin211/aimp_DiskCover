using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using AIMP.DiskCover.CoverFinder;
using AIMP.DiskCover.Interfaces;
using AIMP.SDK.FileManager;
using AIMP.SDK.Player;

namespace AIMP.DiskCover.Infrastructure
{
    public delegate void EndRequestHandler(UIntPtr aimpTaskId, Bitmap foundImage);

    public class CoverFinderManager : ICoverFinderManager
    {
        /// <summary>
        /// Unique identifier of this request. It is used to distinguish
        /// subsequent requests one from another to prevent results from older request
        /// being sent after newer one.
        /// </summary>
        private Guid _currentRequestId;

        private readonly IAimpPlayer _aimpPlayer;
        private readonly ILogger _logger;
        private readonly IPluginSettings _config;

        /// <summary>
        /// Creates an instance of <see cref="CoverFinderManager"/> class.
        /// </summary>
        public CoverFinderManager(IAimpPlayer aimpPlayer, ILogger logger, IPluginSettings settings)
        {
            _config = settings;

            var aggregateCatalog = new AggregateCatalog();
            var assemblyCatalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            aggregateCatalog.Catalogs.Add(assemblyCatalog);

            var container = new CompositionContainer(aggregateCatalog);
            container.ComposeParts();

            // Load all available cover finders.
            CoverModules = container
                .GetExportedValues<ICoverFinder>()
                .ToList();

            _aimpPlayer = aimpPlayer;
            _logger = logger;
        }

        public Bitmap FindCoverImage(string fileUrl, string artist, string album)
        {
            Guid initialRequestId = _currentRequestId = Guid.NewGuid();
            return LoadImageWorkItem(initialRequestId, new TrackInfo
            {
                Album = album,
                Artist = artist,
                FileName = fileUrl
            });
        }

        private List<ICoverFinder> CoverModules { get; set; }

        /// <summary>
        /// An event that is raised prior to starting loading a fresh cover image.
        /// </summary>
        public event EventHandler BeginRequest;

        /// <summary>
        /// An event that is raised when cover finder finishes its work.
        /// </summary>
        public event EndRequestHandler EndRequest;

        /// <summary>
        /// Starts loading a new cover image.
        /// The result will come in an event arguments.
        /// </summary>
        public void FindCoverImageAsync(UIntPtr taskId)
        {
            Guid initialRequestId = _currentRequestId = Guid.NewGuid();
            OnBeginRequest(this, null);
            var coverArt = LoadImageWorkItem(initialRequestId, new TrackInfo(_aimpPlayer.CurrentFileInfo));
            if (coverArt != null)
            {
                if (initialRequestId == _currentRequestId)
                {
                    OnEndRequest(taskId, coverArt);
                }
            }
            else
            {
                // TIMEOUT
                OnEndRequest(taskId, null);
            }
        }

        public Bitmap FindCoverImage(IAimpFileInfo trackInfo)
        {
            Guid initialRequestId = _currentRequestId = Guid.NewGuid();
            return LoadImageWorkItem(initialRequestId, new TrackInfo(trackInfo));
        }

        /// <summary>
        /// Being in a separate thread, this method uses find rules one by one 
        /// and tries to load the cover image.
        /// </summary>
        private Bitmap LoadImageWorkItem(object initialRequestId, TrackInfo trackInfo)
        {
            Bitmap result = null;

            // Make a pre-check that this request is really needed.
            if ((Guid)initialRequestId != _currentRequestId)
            {
                return null;
            }

            try
            {
                _logger.Write($"Album: {trackInfo.Album}\tArtist: {trackInfo.Artist}\tTrack: {trackInfo.FileName}\tFile:{trackInfo.FileName}");

                var enabledRules = _config.AppliedRules.ToList();

                for (var i = 0; i < enabledRules.Count; i++)
                {
                    FindRule rule;
                    string moduleName;

                    // If it is a local audio file - use ordinary set of rules.
                    if (!trackInfo.IsStream)
                    {
                        rule = enabledRules.Skip(i).FirstOrDefault();
                        Contract.Assume(rule != null);
                        moduleName = rule.Module;
                    }
                    //If it is a CDA or a radio station - only LastFm can help us.
                    else
                    {
                        // If Last.Fm is disabled - stop iterating, we can't get the cover image.
                        if (enabledRules.All(r => r.Module != LastFmFinder.ModuleName))
                        {
                            break;
                        }

                        i = enabledRules.Count; // Prevent further iterations.

                        rule = null;
                        moduleName = LastFmFinder.ModuleName;
                    }

                    ICoverFinder finder = CoverModules.FirstOrDefault(c => c.Name == moduleName);
                    if (finder == null)
                    {
                        _logger.Write($"Finder plugin {moduleName} has not been found.");
                        break;
                    }

                    result = finder.GetBitmap(trackInfo, rule);

                    if (result != null)
                    {
                        _logger.Write($"Module: {moduleName}\tCover art has been found");
                        break;
                    }

                    _logger.Write($"Module: {moduleName}\tCover art has not been found");
                }
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
            }

            return result;
        }

        private void OnBeginRequest(object sender, EventArgs e)
        {
            EventHandler temp = Interlocked.CompareExchange(ref BeginRequest, null, null);
            temp?.Invoke(sender, e);
        }

        private void OnEndRequest(UIntPtr aimpTaskId, Bitmap coverArt)
        {
            var temp = Interlocked.CompareExchange(ref EndRequest, null, null);
            temp?.Invoke(aimpTaskId, coverArt);
        }
    }
}
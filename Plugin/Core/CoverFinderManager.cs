using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Drawing;
using System.Threading;
using AIMP.DiskCover.LastFM;
using AIMP.DiskCover.Settings;
using AIMP.SDK.Logger;
using AIMP.SDK.Player;

namespace AIMP.DiskCover
{
    //using AIMP.DiskCover.LastFM;

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

        private List<ICoverFinder> CoverModules { get; set; }

        /// <summary>
        /// An event that is raised prior to starting loading a fresh cover image.
        /// </summary>
        public event EventHandler BeginRequest;

        /// <summary>
        /// An event that is raised when cover finder finishes its work.
        /// </summary>
        public event EventHandler<FinderEvent> EndRequest;

        /// <summary>
        /// Starts loading a new cover image.
        /// The result will come in an event arguments.
        /// </summary>
        public void StartLoadingBitmap()
        {
            Guid initialRequestId;

            initialRequestId = _currentRequestId = Guid.NewGuid();
            OnBeginRequest(this, null);

            var coverArt = LoadImageWorkItem(initialRequestId);
            if (coverArt != null)
            {
                if (initialRequestId == _currentRequestId)
                {
                    OnEndRequest(this, new FinderEvent(coverArt));
                }
            }
            else
            {
                // TIMEOUT
                OnEndRequest(this, new FinderEvent(null));
            }
        }

        /// <summary>
        /// Being in a separate thread, this method uses find rules one by one 
        /// and tries to load the cover image.
        /// </summary>
        /// <param name="initialRequestId">An identifier of current search session.</param>
        private Bitmap LoadImageWorkItem(Object initialRequestId)
        {
            Bitmap result = null;

            // Make a pre-check that this request is really needed.
            if ((Guid)initialRequestId != _currentRequestId)
            {
                return null;
            }

            try
            {
                _logger.Write(
                    $"Album: {_aimpPlayer.CurrentFileInfo.Album};\tArtist: {_aimpPlayer.CurrentFileInfo.Artist};\tTrack: {_aimpPlayer.CurrentFileInfo.FileName}");

                // This object prevents race conditions when the search 
                // is in progress and at the same time AIMP is changing a track.
                var trackInfo = new TrackInfo(_aimpPlayer);

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
                        throw new ApplicationException("Finder plugin " + moduleName + " has not been found.");
                    }

                    result = finder.GetBitmap(_aimpPlayer, rule);

                    if (result != null)
                    {
                        _logger.Write($"Module: {moduleName}\tCover art has been found");
                        break;
                    }
                    {
                        _logger.Write($"Module: {moduleName}\tCover art has not been found");
                    }
                }

                _logger.Write("------------------------------------------------------");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
            }

            return result;
        }

        private void OnBeginRequest(object sender, EventArgs e)
        {
            EventHandler temp = Interlocked.CompareExchange(ref BeginRequest, null, null);
            temp?.Invoke(sender, e);
        }

        private void OnEndRequest(object sender, FinderEvent e)
        {
            EventHandler<FinderEvent> temp = Interlocked.CompareExchange(ref EndRequest, null, null);
            temp?.Invoke(sender, e);
        }
    }
}
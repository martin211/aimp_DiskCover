using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using AIMP.DiskCover.Resources;
using System.Drawing;
using System.Threading;
using AIMP.SDK.Logger;
using AIMP.SDK.Player;

namespace AIMP.DiskCover
{
    using AIMP.DiskCover.LastFM;

    //using AIMP.DiskCover.LastFM;

    public class CoverFinderManager
    {
        /// <summary>
        /// Unique identifier of this request. It is used to distinguish
        /// subsequent requests one from another to prevent results from older request
        /// being sent after newer one.
        /// </summary>
        private Guid _currentRequestId;

        /// <summary>
        /// The synchronisation object.
        /// </summary>
        private readonly Object _syncRoot = new Object();

        private readonly IAimpPlayer _aimpPlayer;

        private readonly ILogger _logger;

        /// <summary>
        /// Creates an instance of <see cref="CoverFinderManager"/> class.
        /// </summary>
        /// <param name="aimpPlayer">AIMP player object.</param>
        public CoverFinderManager(IAimpPlayer aimpPlayer, ILogger logger)
        {
            Contract.Requires(aimpPlayer != null);

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

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_aimpPlayer != null);
            Contract.Invariant(CoverModules != null);
            Contract.Invariant(CoverModules.Count > 0, "No cover image loaders are found.");
        }

        /// <summary>
        /// Starts loading a new cover image.
        /// The result will come in an event arguments.
        /// </summary>
        public async Task StartLoadingBitmap()
        {
            Guid initialRequestId;

            initialRequestId = _currentRequestId = Guid.NewGuid();
            OnBeginRequest(this, null);

            var token = new CancellationTokenSource();
            var task = Task.Run(() => LoadImageWorkItem(initialRequestId, token.Token), token.Token);
            if (await Task.WhenAny(task, Task.Delay(10000, token.Token)) == task)
            {
                if (initialRequestId == _currentRequestId)
                {
                    OnEndRequest(this, new FinderEvent(task.Result));
                }
            }
            else
            {
                // TIMEOUT
                token.Cancel();
                OnEndRequest(this, new FinderEvent(null));
            }
        }

        /// <summary>
        /// Being in a separate thread, this method uses find rules one by one 
        /// and tries to load the cover image.
        /// </summary>
        /// <param name="initialRequestId">An identifier of current search session.</param>
        private Bitmap LoadImageWorkItem(Object initialRequestId, CancellationToken token)
        {
            Bitmap result = null;

            // Make a pre-check that this request is really needed.
            if ((Guid)initialRequestId != _currentRequestId)
            {
                return null;
            }

            try
            {
                _logger.Write(string.Format("Album: {0};\tArtist: {1};\tTrack: {2}", _aimpPlayer.CurrentFileInfo.Album, _aimpPlayer.CurrentFileInfo.Artist, _aimpPlayer.CurrentFileInfo.FileName));

                // This object prevents race conditions when the search 
                // is in progress and at the same time AIMP is changing a track.
                var trackInfo = new TrackInfo(_aimpPlayer);

                var enabledRules = Config.Instance.Rules.Where(r => r.Enabled).ToArray();

                for (Int32 i = 0; i < enabledRules.Length; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        return null;
                    }

                    FindRule rule;
                    String moduleName;

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
                        if (!enabledRules.Any(r => r.Module == LastFmFinder.ModuleName))
                        {
                            break;
                        }

                        i = enabledRules.Length; // Prevent further iterations.

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
                        _logger.Write(string.Format("Module: {0}\tCover art has been found", moduleName));
                        break;
                    }
                    {
                        _logger.Write(string.Format("Module: {0}\tCover art has not been found", moduleName));
                    }
                }

                _logger.Write("------------------------------------------------------");
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(
                    LocalizedData.ErrorOnCoverImageSearch + ex.Message,
                    LocalizedData.PluginName,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
#endif
            }

            return result;
        }

        private void OnBeginRequest(object sender, EventArgs e)
        {
            EventHandler temp = Interlocked.CompareExchange(ref BeginRequest, null, null);
            if (temp != null)
                temp(sender, e);
        }

        private void OnEndRequest(object sender, FinderEvent e)
        {
            EventHandler<FinderEvent> temp = Interlocked.CompareExchange(ref EndRequest, null, null);
            if (temp != null)
                temp(sender, e);
        }
    }
}
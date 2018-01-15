using System;
using System.Drawing;
using AIMP.DiskCover.Settings;
using AIMP.SDK;
using AIMP.SDK.AlbumArtManager;
using AIMP.SDK.Logger;
using AIMP.SDK.MenuManager;
using AIMP.SDK.Options;
using AIMP.SDK.Player;
using AIMP.SDK.Threading;

namespace AIMP.DiskCover.Core
{
    public class AimpTask : IAimpTask
    {
        private readonly Func<AimpActionResult> _action;

        public AimpTask(Func<AimpActionResult> action)
        {
            _action = action;
        }

        public AimpActionResult Execute(IAimpTaskOwner owner)
        {
            return _action();
        }
    }

    public class DiskCoverPlugin : IDiskCoverPlugin, IDisposable
    {
        private readonly IAimpPlayer _player;
        private readonly ILogger _logger;
        private readonly IPluginSettings _settings;
        private readonly IAimpOptionsDialogFrame _dialogFrame;
        private readonly IPluginEventsExecutor _pluginEventsExecutor;
        private readonly IPluginEvents _pluginEvents;
        private readonly IAimpExtensionAlbumArtCatalog _aimpExtensionAlbumArtCatalog;
        private IAimpMenuItem _menuItem;
        private bool _isChecked;
        private readonly ICoverFinderManager _coverFinderManager;
        private CoverWindow _coverWindow;
        private bool _isInitialized;
        private bool _isShowen;

        public DiskCoverPlugin(
            IAimpPlayer player,
            IPluginSettings settings,
            ILogger logger,
            IPluginEventsExecutor pluginEventsExecutor,
            IAimpExtensionAlbumArtCatalog aimpExtensionAlbumArtCatalog,
            IPluginEvents pluginEvents,
            ICoverFinderManager coverFinderManager,
            IAimpOptionsDialogFrame dialogFrame)
        {
            _player = player;
            _logger = logger;
            _settings = settings;
            _dialogFrame = dialogFrame;
            _pluginEventsExecutor = pluginEventsExecutor;
            _pluginEvents = pluginEvents;
            _coverFinderManager = coverFinderManager;
            _aimpExtensionAlbumArtCatalog = aimpExtensionAlbumArtCatalog;
        }

        public void Initialize()
        {
            _logger.Write("DiskCover: Initialize plugin");

            _player.Core.RegisterExtension(_dialogFrame);
            _player.Core.RegisterExtension(_aimpExtensionAlbumArtCatalog);

            InitMenu();

            _player.Core.CoreMessage += CoreOnCoreMessage;
            _player.StateChanged += PlayerOnStateChanged;
            _player.TrackChanged += PlayerOnTrackChanged;
            _pluginEvents.ConfigUpdated += (sender, args) => { };

            InitCoverWindow();
        }

        private void InitMenu()
        {
            _player.MenuManager.CreateMenuItem(out _menuItem);
            _menuItem.Checked = _settings.IsEnabled;
            _menuItem.OnExecute += AimpMenu_Click;
            _menuItem.Style = AimpMenuItemStyle.CheckBox;
            _menuItem.Name = Localization.DiskCover.MenuName;
            _isChecked = _settings.IsEnabled;
            _player.MenuManager.Add(ParentMenuType.AIMP_MENUID_COMMON_UTILITIES, _menuItem);
        }

        private void InitCoverWindow()
        {
            _coverFinderManager.BeginRequest += OnBeginFindCoverRequest;
            _coverFinderManager.EndRequest += OnEndFindCoverRequest;
            // TODO: Extract to provider and use DI
            _coverWindow = new CoverWindow(_settings, _pluginEventsExecutor)
            {
                MinHeight = 200,
                MinWidth = 200,
                Width = _settings.Width,
                Height = _settings.Height,
                Left = _settings.Left,
                Top = _settings.Top
            };

            // Make Win32 window owner of this WPF window.
            new System.Windows.Interop.WindowInteropHelper(_coverWindow) { Owner = _player.Win32Manager.GetAimpHandle() };

            if (_settings.IsEnabled)
            {
                ShowCoverForm();
            }
        }

        private void AimpMenu_Click(object sender, EventArgs e)
        {
            _isChecked = !_isChecked;
            _menuItem.Checked = _isChecked;
            if (_isChecked)
            {
                ShowCoverForm();
            }
            else
            {
                _coverWindow.Hide();
            }

            _settings.IsEnabled = _isChecked;
        }

        private void ShowCoverForm()
        {
            _isShowen = true;
            _coverWindow.Show();
            _coverWindow.Activate();

            RequestFreshCoverImage();
            _coverWindow.Display();
        }

        private void OnBeginFindCoverRequest(object s, EventArgs e)
        {
            _coverWindow.Dispatcher.Invoke(new Action(_coverWindow.BeginLoadImage));
        }

        private void OnEndFindCoverRequest(UIntPtr aimpTaskId, Bitmap coverArt)
        {
            // if player is not playing, no need to apply search results.
            if (_player.State == AimpPlayerState.Playing)
            {
                UpdateImage(coverArt);
            }

            _player.ServiceThreadPool.Cancel(aimpTaskId, AimpServiceThreadPoolType.None);
        }

        private void UpdateImage(Bitmap newImage)
        {
            // ReSharper disable CoVariantArrayConversion
            _coverWindow.Dispatcher.Invoke(new Action<Bitmap>(_coverWindow.ChangeCoverImage), new[] { newImage });
            // ReSharper restore CoVariantArrayConversion
        }

        private void PlayerOnTrackChanged(object sender, EventArgs eventArgs)
        {
           // RequestFreshCoverImage();
        }

        private void PlayerOnStateChanged(object sender, StateChangedEventArgs stateChangedEventArgs)
        {
            if (stateChangedEventArgs.PlayerState == AimpPlayerState.Playing && _isShowen)
            {
                RequestFreshCoverImage();
            }
            else if (stateChangedEventArgs.PlayerState == AimpPlayerState.Stopped && _isShowen)
            {
                _coverWindow.ChangeCoverImage(null);
            }
        }

        private void CoreOnCoreMessage(AimpMessages.AimpCoreMessageType param1, int param2)
        {
            if (param1 == AimpMessages.AimpCoreMessageType.AIMP_MSG_EVENT_STREAM_START || param1 == AimpMessages.AimpCoreMessageType.AIMP_MSG_EVENT_STREAM_START_SUBTRACK)
            {
                RequestFreshCoverImage();
            }
        }

        private void RequestFreshCoverImage()
        {
            // This happens if cover window is currently not enabled.
            if (!_isShowen)
            {
                return;
            }

            if (_player.State == AimpPlayerState.Playing)
            {
                UIntPtr taskId = UIntPtr.Zero;
                _player.ServiceThreadPool.Execute(new AimpTask(() =>
                {
                    _coverFinderManager.FindCoverImageAsync(taskId);
                    return AimpActionResult.Ok;
                }), out taskId);
            }
            else
            {
                UpdateImage(null);
            }
        }

        public void Dispose()
        {
            _coverWindow?.Close();
            //_player.MenuManager.Delete(_menuItem);
            _logger.Close();
        }
    }
}
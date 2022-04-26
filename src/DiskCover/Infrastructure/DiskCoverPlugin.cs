using System;
using System.Drawing;
using System.Runtime.InteropServices;
using AIMP.DiskCover.Infrastructure.Events;
using AIMP.DiskCover.Interfaces;
using AIMP.SDK;
using AIMP.SDK.AlbumArt.Extensions;
using AIMP.SDK.MenuManager;
using AIMP.SDK.MenuManager.Objects;
using AIMP.SDK.MessageDispatcher;
using AIMP.SDK.Options;
using AIMP.SDK.Threading;

namespace AIMP.DiskCover.Infrastructure
{
    public class AimpTask : IAimpTask
    {
        private readonly Func<AimpActionResult> _action;

        public AimpTask(Func<AimpActionResult> action)
        {
            _action = action;
        }

        public void Execute(IAimpTaskOwner owner)
        {
            _action();
        }
    }

    public class DiskCoverPlugin : IDiskCoverPlugin, IDisposable
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int _controlfp(int newControl, int mask);

        private readonly IAimpPlayer _player;
        private readonly ILogger _logger;
        private readonly IPluginSettings _settings;
        private readonly IAimpOptionsDialogFrame _dialogFrame;
        private readonly IAimpExtensionAlbumArtCatalog _aimpExtensionAlbumArtCatalog;
        private readonly IEventAggregator _eventAggregator;
        private IAimpMenuItem _menuItem;
        private bool _isChecked;
        private readonly ICoverFinderManager _coverFinderManager;
        private CoverWindow _coverWindow;
        private bool _isShown;
        private AimpMessageHook _aimpMessageHook;

        public DiskCoverPlugin(
            IAimpPlayer player,
            IPluginSettings settings,
            ILogger logger,
            IAimpExtensionAlbumArtCatalog aimpExtensionAlbumArtCatalog,
            ICoverFinderManager coverFinderManager,
            IAimpOptionsDialogFrame dialogFrame,
            IEventAggregator eventAggregator
            )
        {
            _player = player;
            _logger = logger;
            _settings = settings;
            _dialogFrame = dialogFrame;
            _coverFinderManager = coverFinderManager;
            _aimpExtensionAlbumArtCatalog = aimpExtensionAlbumArtCatalog;
            _eventAggregator = eventAggregator;
        }

        public void Initialize()
        {
            _logger.Write("DiskCover: Initialize plugin");

            _controlfp(0x9001F, 0xfffff);

            var res = _player.Core.RegisterExtension(_dialogFrame);
            if (res.ResultType != ActionResultType.OK)
            {
                _logger.Write($"Unable register IAimpOptionsDialogFrame: {res}");
            }

            res = _player.Core.RegisterExtension(_aimpExtensionAlbumArtCatalog);
            if (res.ResultType != ActionResultType.OK)
            {
                _logger.Write($"Unable register IAimpExtensionAlbumArtCatalog: {res}");
            }

            InitMenu();

            _aimpMessageHook = new AimpMessageHook();
            _aimpMessageHook.OnCoreMessage += CoreOnCoreMessage;
            _player.ServiceMessageDispatcher.Hook(_aimpMessageHook);

            InitCoverWindow();
        }

        private void InitMenu()
        {
            var res = _player.Core.CreateObject<IAimpMenuItem>();

            if (res.ResultType == ActionResultType.OK)
            {
                _menuItem = res.Result as IAimpMenuItem;
                _menuItem.Checked = _settings.IsEnabled;
                _menuItem.OnExecute += AimpMenu_Click;
                _menuItem.Style = MenuItemStyle.CheckBox;
                _menuItem.Name = Localization.DiskCover.MenuName;
                _menuItem.Id = Guid.NewGuid().ToString();
                _isChecked = _settings.IsEnabled;
                _player.ServiceMenuManager.Add(ParentMenuType.CommonUtilities, _menuItem);
            }
            else
            {
                _logger.Write($"Unable create and register menu item: {res}");
            }
        }

        private void InitCoverWindow()
        {
            _coverFinderManager.BeginRequest += OnBeginFindCoverRequest;
            _coverFinderManager.EndRequest += OnEndFindCoverRequest;
            // TODO: Extract to provider and use DI
            _coverWindow = new CoverWindow(_settings, _player, _eventAggregator)
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
            _isShown = true;
            _coverWindow.Show();
            _coverWindow.Activate();

            RequestFreshCoverImage(null);
            _coverWindow.Display();
        }

        private void OnBeginFindCoverRequest(object s, EventArgs e)
        {
            _coverWindow.Dispatcher.Invoke(_coverWindow.BeginLoadImage);
        }

        private void OnEndFindCoverRequest(UIntPtr aimpTaskId, Bitmap coverArt)
        {
            // if player is not playing, no need to apply search results.
            if (_player.ServicePlayer.State == AimpPlayerState.Playing)
            {
                UpdateImage(coverArt);
            }

            _player.ServiceThreadPool.Cancel(aimpTaskId, AimpServiceThreadPoolType.None);
        }

        private void UpdateImage(Bitmap newImage)
        {
            _coverWindow.Dispatcher.Invoke(new Action<Bitmap>(_coverWindow.ChangeCoverImage), newImage);
        }
        
        private AimpActionResult CoreOnCoreMessage(AimpCoreMessageType message, int param1, IntPtr intPtr)
        {
            if (message == AimpCoreMessageType.EventStreamStart ||
                message == AimpCoreMessageType.EventStreamStartSubtrack)
            {
                RequestFreshCoverImage(message);
            }

            if (message == AimpCoreMessageType.CmdStop && _isShown)
            {
                _coverWindow.ChangeCoverImage(null);
            }

            if (message == AimpCoreMessageType.CmdPlay && _isShown)
            {
                RequestFreshCoverImage(message);
            }

            _eventAggregator.Raise(new CoreMessageEventArgs(message, param1, intPtr));

            return new AimpActionResult(ActionResultType.OK);
        }

        private void RequestFreshCoverImage(AimpCoreMessageType? message)
        {
            // This happens if cover window is currently not enabled.
            if (!_isShown)
            {
                return;
            }

            if (_player.ServicePlayer.State == AimpPlayerState.Playing || (message.HasValue && message == AimpCoreMessageType.CmdPlay))
            {
                UIntPtr taskId = UIntPtr.Zero;
                var id = taskId;
                var res = _player.ServiceThreadPool.Execute(new AimpTask(() =>
                {
                    _coverFinderManager.FindCoverImageAsync(id);
                    return new AimpActionResult(ActionResultType.OK);
                }));

                id = res.Result;
            }
            else
            {
                UpdateImage(null);
            }
        }

        public void Dispose()
        {
            _coverWindow?.Close();
            _player.ServiceMenuManager.Delete(_menuItem);
            _logger.Close();
        }
    }
}
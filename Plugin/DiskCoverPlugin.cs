using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AIMP.DiskCover.Core;
using AIMP.DiskCover.Resources;
using DiskCover.Settings;
using AIMP.SDK;
using AIMP.SDK.MenuManager;

namespace AIMP.DiskCover
{
    using System.IO;

    using AIMP.DiskCover.Settings;
    using AIMP.SDK.Logger;
    using AIMP.SDK.Player;

    using IWin32Window = System.Windows.Forms.IWin32Window;

    [AimpPlugin(PluginName, "Evgeniy Bogdan, Roman Nikitin", Version)]
// ReSharper disable ClassNeverInstantiated.Global
    public class DiskCoverPlugin : AimpPlugin
// ReSharper restore ClassNeverInstantiated.Global
    {
        public const String PluginName = "AIMP Disc Cover";
        public const String Version = "2.0.0";

        /// <summary>
        /// Indicates that plugin has been loaded and thus should be disposed.
        /// </summary>
        private Boolean _isInitialized;
        
        private CoverFinderManager _coverFinderManager;
        private CoverWindow _coverWindow;

        private bool _isShowen;
        private bool _isChecked;

        /// <summary>
        /// Main menu item for the image cover window.
        /// </summary>
        private IAimpMenuItem _menuItem;

        private ILogger _logger;
        public ILogger LoggerManager => _logger ?? (_logger = new InternalLoggerManager());

        private IntPtr AimpHandle
        {
            get
            {
                return Player.Win32Manager.GetAimpHandle();
            }
        }
       
        public override void Initialize()
        {
            LoggerManager.Write("DiskCover: Initialize plugin");

            DependencyResolver.Current.Register(new List<object>
            {
                Player
            });

            #region Add an item to AIMP's main menu and subscribe to its checking.

            Config.ConfigFolderPath = Path.Combine(Player.Core.GetPath(AimpMessages.AimpCorePathType.AIMP_CORE_PATH_PROFILE), "DiskCover");

            #region Initialize plugin language.

            // Try to get current language from the first 2 letters 
            // of the name of a language file used by AIMP.
            String langFileName = Player.MUIManager.GetName();
            if (langFileName != null && langFileName.Length >= 2)
            {
                var lang = langFileName.Substring(0, 2);

                try
                {
                    System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(lang);
                }
                catch (System.Globalization.CultureNotFoundException)
                {
                    // This exception can happen if AIMP uses some non-standard 
                    // language or the name of a language file is not standard.
                }
            }

            #endregion

            Player.MenuManager.CreateMenuItem(out _menuItem);
            _menuItem.Checked = Config.Instance.IsEnabled;
            _menuItem.OnExecute += AimpMenu_Click;
            _menuItem.Style = AimpMenuItemStyle.CheckBox;
            _menuItem.Name = LocalizedData.AIMPMenuItemName;
            _isChecked = Config.Instance.IsEnabled;

            Player.MenuManager.Add(ParentMenuType.AIMP_MENUID_COMMON_UTILITIES, _menuItem);

            #endregion

            #region Subscribe to AIMP player events.

            Player.Core.CoreMessage += CoreOnCoreMessage;
            Player.StateChanged += PlayerOnStateChanged;
            Player.TrackChanged += PlayerOnTrackChanged;

            #endregion



            _coverFinderManager = new CoverFinderManager(Player, LoggerManager);
            _coverFinderManager.BeginRequest += OnBeginFindCoverRequest;
            _coverFinderManager.EndRequest += OnEndFindCoverRequest;

            _coverWindow = new CoverWindow
            {
                MinHeight = 200,
                MinWidth = 200,
                Width = Config.Instance.Width,
                Height = Config.Instance.Height,
                Left = Config.Instance.Left,
                Top = Config.Instance.Top
            };

            // Make Win32 window owner of this WPF window.
            new System.Windows.Interop.WindowInteropHelper(_coverWindow) { Owner = AimpHandle };

            if (AimpHandle == IntPtr.Zero)
            {
                MessageBox.Show(LocalizedData.CannotFindAIMP);
            }


            if (Config.Instance.IsEnabled)
            {
                ShowCoverForm();
            }

            var options = new OptionsFrame(Player);
            Player.Core.RegisterExtension(options);

            _isInitialized = true;
        }

        private void PlayerOnTrackChanged(object sender, EventArgs eventArgs)
        {
            RequestFreshCoverImage();
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

        public override void Dispose()
        {
            if (_coverWindow != null)
            {
                _coverWindow.Close();
            }

            if (_isInitialized)
            {
                Config.Instance.StoreChanges();
            }

            Player.MenuManager.Delete(_menuItem);

            LoggerManager.Close();
        }

        /// <summary>
        /// This method is called when user requests settings dialog to open.
        /// </summary>
        /// <param name="parentWnd">A handler of a parent window.</param>
        public void ShowSettingDialog(IWin32Window parentWnd)
        {
            var wnd = new SettingsWindow();

            // Make Win32 window owner of this WPF window.
            //new System.Windows.Interop.WindowInteropHelper(wnd) { Owner = AimpHandle };

            if (wnd.ShowDialog() ?? false)
            {
                Config.Instance.StoreChanges();
            }

            wnd.Close();
        }

        /// <summary>
        /// Exchanges current image with the passed one.
        /// </summary>
        /// <param name="newImage">A new image.</param>
        private void UpdateImage(Bitmap newImage)
        {
            // ReSharper disable CoVariantArrayConversion
            _coverWindow.Dispatcher.Invoke(new Action<Bitmap>(_coverWindow.ChangeCoverImage), new[] { newImage });
            // ReSharper restore CoVariantArrayConversion
        }

        /// <summary>
        /// A handler for clicking on "Show cover" menu option event.
        /// </summary>
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

            Config.Instance.IsEnabled = _isChecked;
        }


        private void ShowCoverForm()
        {
            _isShowen = true;
            _coverWindow.Show();
            _coverWindow.Activate();

            RequestFreshCoverImage();
            //this.Player.
            _coverWindow.Display();
        }

        /// <summary>
        /// Requests new cover image from cover finder plugins.
        /// </summary>
        private void RequestFreshCoverImage()
        {
            // This happens if cover window is currently not enabled.
            if (!_isShowen)
            {
                return;
            }

            if (Player.State == AimpPlayerState.Playing)
            {
                _coverFinderManager.StartLoadingBitmap();
            }
            else
            {
                UpdateImage(null);
            }
        }

        #region CoverFinder events
        
        private void OnBeginFindCoverRequest(object s, EventArgs e)
        {
            _coverWindow.Dispatcher.Invoke(new Action(_coverWindow.BeginLoadImage));
        }

        private void OnEndFindCoverRequest(object s, FinderEvent e)
        {
            // if player is not playing, no need to apply search results.
            if (Player.State == AimpPlayerState.Playing)
            {
                UpdateImage(e.CoverBitmap);
            }
        }

        #endregion
    }

    internal class InternalLoggerManager : ILogger
    {
        #region Implementation of ILogger

        public void Write(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void Write(Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception.ToString());
        }

        public void Close()
        {
        }

        #endregion
    }
}
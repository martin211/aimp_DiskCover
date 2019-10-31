using System;
using AIMP.DiskCover.Interfaces;
using AIMP.SDK.Options;
using AIMP.SDK.Player;

namespace AIMP.DiskCover.Settings
{
    public class OptionsFrame : IAimpOptionsDialogFrame
    {
        private OptionsForm _settingsWindow;
        private readonly IAimpPlayer _player;
        private readonly IPluginEvents _pluginEvents;
        private readonly IViewModelsProvider _modelsProvider;
        private readonly IPluginEventsExecutor _pluginEventsExecutor;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public OptionsFrame(
            IAimpPlayer player,
            IPluginEvents pluginEvents,
            IViewModelsProvider modelsProvider,
            IPluginEventsExecutor pluginEventsExecutor)
        {
            _player = player;
            _pluginEvents = pluginEvents;
            _modelsProvider = modelsProvider;
            _pluginEventsExecutor = pluginEventsExecutor;
        }

        public string GetName()
        {
            return Localization.DiskCover.Title;
        }

        public IntPtr CreateFrame(IntPtr parentWindow)
        {
            _settingsWindow = new OptionsForm(parentWindow, _modelsProvider.GetSettingsViewModel());
            _pluginEvents.ConfigUpdated += (sender, args) => 
            {
                _player.ServiceOptionsDialog.FrameModified(this);
            };

            _settingsWindow.Show();
            return _settingsWindow.Handle;
        }

        public void DestroyFrame()
        {
            _settingsWindow.Close();
            _settingsWindow = null;
        }

        public void Notification(OptionsDialogFrameNotificationType id)
        {
            if (id == OptionsDialogFrameNotificationType.AIMP_SERVICE_OPTIONSDIALOG_NOTIFICATION_CAN_SAVE)
            {
                _pluginEventsExecutor.OnSaveConfig();
            }
        }
    }
}
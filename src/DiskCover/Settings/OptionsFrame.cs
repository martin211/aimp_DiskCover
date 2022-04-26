using System;
using AIMP.DiskCover.Infrastructure;
using AIMP.DiskCover.Infrastructure.Events;
using AIMP.DiskCover.Interfaces;
using AIMP.SDK;
using AIMP.SDK.Options;
using AIMP.SDK.Player;

namespace AIMP.DiskCover.Settings
{
    public class OptionsFrame : IAimpOptionsDialogFrame
    {
        private OptionsForm _settingsWindow;
        private readonly IAimpPlayer _player;
        private readonly IEventAggregator _aggregator;
        private readonly IViewModelsProvider _modelsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public OptionsFrame(
            IAimpPlayer player,
            IViewModelsProvider modelsProvider,
            IEventAggregator aggregator)
        {
            _player = player;
            _modelsProvider = modelsProvider;
            _aggregator = aggregator;
        }

        public string GetName()
        {
            return Localization.DiskCover.Title;
        }

        public IntPtr CreateFrame(IntPtr parentWindow)
        {
            _settingsWindow = new OptionsForm(parentWindow, _modelsProvider.GetSettingsViewModel());
            _aggregator.Register<ConfigUpdatedEventArgs>(e =>
            {
                _player.ServiceOptionsDialog.FrameModified(this);
            });

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
            if (id == OptionsDialogFrameNotificationType.CanSave)
            {
                _aggregator.Raise(new SaveConfigEventArgs());
            }
        }
    }
}
namespace AIMP.DiskCover.Settings
{
    using System;

    using AIMP.DiskCover.Resources;
    using AIMP.SDK.Player;

    public class OptionsFrame : AIMP.SDK.Options.IAimpOptionsDialogFrame
    {
        private OptionsForm _settingsWindow;

        private IAimpPlayer _player;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public OptionsFrame(IAimpPlayer player)
        {
            _player = player;
        }

        #region Implementation of IAimpOptionsDialogFrame

        public string GetName()
        {
            return LocalizedData.AIMPMenuItemName;
        }

        public IntPtr CreateFrame(IntPtr parentWindow)
        {
            _settingsWindow = new OptionsForm(parentWindow);
            _settingsWindow.OnSaved += (sender, args) =>
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

        public void Notification(int id)
        {
            
        }

        #endregion
    }
}
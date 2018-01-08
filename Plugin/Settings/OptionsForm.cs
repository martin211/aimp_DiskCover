using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AIMP.DiskCover.Core;

namespace AIMP.DiskCover.Settings
{
    public partial class OptionsForm : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        public OptionsForm(IntPtr parentWindow, SettingsViewModel settingsViewModel)
        {
            InitializeComponent();
            SetParent(Handle, parentWindow);
            WindowState = FormWindowState.Maximized;
            var settings = new SettingsWindow();
            settings.DataContext = settingsViewModel;
            elementHost1.Child = settings;
        }
    }
}

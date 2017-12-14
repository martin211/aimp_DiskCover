using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIMP.DiskCover.Settings
{
    using System.Runtime.InteropServices;

    using AIMP.SDK.Player;

    using global::DiskCover.Settings;

    public partial class OptionsForm : Form
    {
        public event EventHandler OnSaved;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        public OptionsForm(IntPtr parentWindow)
        {
            InitializeComponent();
            SetParent(Handle, parentWindow);
            WindowState = FormWindowState.Maximized;
            var settings = new SettingsWindow();
            elementHost1.Child = settings;
        }

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    var result = settings.ShowDialog();
        //    if (result.HasValue && result.Value)
        //    {
        //        var tmp = System.Threading.Interlocked.CompareExchange(ref OnSaved, null, null);
        //        if (tmp != null)
        //        {
        //            tmp(this, EventArgs.Empty);
        //        }
        //    }
        //}
    }
}

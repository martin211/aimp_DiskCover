using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Xml.Serialization;

namespace AIMP.DiskCover.Settings.ConfigObjects
{
    /// <summary>
    /// Represents an object that is intended to be serialized as XML 
    /// to a file.
    /// </summary>
    [XmlRoot("Config")]
    public class ConfigObjectV1
    {
        /// <summary>
        /// Contains code contracts invariants. Do not call explicitly!
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(Version == 1);
            Contract.Invariant(Height > 0);
            Contract.Invariant(Width > 0);
            Contract.Invariant(Rules != null);
        }
                
        public ConfigObjectV1()
        {
            // Enable all rules.
            Rules = Enum.GetValues(typeof(CoverRuleType))
                .Cast<CoverRuleType>()
                .ToArray();

            Width = 500;
            Height = 500;
            Left = 10;
            Top = 10;

            ShowInTaskbar = false;
            EnableHotKeys = true;
        }

        /// <summary>
        /// Gets or sets a version of the current config.
        /// </summary>
        public Int32 Version 
        { 
            get 
            {
                return 1; 
            }
            // Version cannot be set explicitly. The setter exists only
            // to circumvent XML serialization mechanism.
// ReSharper disable ValueParameterNotUsed
// ReSharper disable UnusedMember.Global
            set 
// ReSharper restore UnusedMember.Global
// ReSharper restore ValueParameterNotUsed
            { }
        }

        /// <summary>
        /// Gets or sets a value which indicates that plugin 
        /// is enabled and should show its window.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets height of plugin's window.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Gets or sets width of plugin's window.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Gets or sets an offset of plugin's window from the left side of the screen.
        /// </summary>
        public double Left { get; set; }

        /// <summary>
        /// Gets or sets an offset of plugin's window from the top of the screen.
        /// </summary>
        public double Top { get; set; }

        /// <summary>
        /// Gets or sets a value which indicates that 
        /// the plugin should show its separate element in window's taskbar.
        /// </summary>
        public bool ShowInTaskbar { get; set; }

        /// <summary>
        /// Gets or sets a value which indicates that 
        /// special Hot Keys are being handled by the player.
        /// </summary>
        public bool EnableHotKeys { get; set; }

        /// <summary>
        /// A collection of enabled cover finding rules.
        /// </summary>
        public CoverRuleType[] Rules { get; set; }
    }
}

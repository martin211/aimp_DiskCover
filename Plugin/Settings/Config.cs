namespace AIMP.DiskCover
{
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;
    using System;
    using System.Linq;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;

    using Settings.ConfigObjects;
    using Resources;
    using System.Windows;

    /// <summary>
    /// A singleton that is responsible for configuration logic of the plugin.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Contains code contracts invariants. Do not call explicitly!
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(Height > 0);
            Contract.Invariant(Width > 0);
            Contract.Invariant(Contract.ForAll(Rules, fr => fr != null));
        }

        /// <summary>
        /// Just an object to be used in <see langword="lock"/> constructions.
        /// </summary>
        private static readonly Object _syncRoot = new Object();
        
        /// <summary>
        /// A path to the folder which contains the settings file.
        /// </summary>
        private static String _configFolderPath;

        /// <summary>
        /// A read-only collection of all available cover finding rules.
        /// </summary>
        private static readonly ReadOnlyCollection<FindRule> _rules = new ReadOnlyCollection<FindRule>(FindRule.GetAvailableRules());

        /// <summary>
        /// This private constructor doesn't allow to instanciate this method.
        /// </summary>
        private Config()
        {
            #region Load or create default config storage.

            ConfigObjectV1 co = null;
            
            String configFile = Path.Combine(_configFolderPath, "config.xml");

            if (!File.Exists(configFile))
            {
                co = new ConfigObjectV1();
                StoreChanges();
            }

            if (File.Exists(configFile))
            {
                using (var stream = new FileStream(configFile, FileMode.Open, FileAccess.Read))
                using (var reader = new System.Xml.XmlTextReader(stream))
                {
                    // Find the version of config file.
                    String configVersion = null;
                    while (reader.Read())
                    {
                        if (reader.Name.Equals("Version", StringComparison.Ordinal))
                        {
                            reader.Read(); // Force reader to go to node's value.

                            configVersion = reader.Value;

                            break;
                        }
                    }

                    if (configVersion != null)                    
                    {
                        Int32 version;
                        
                        if (Int32.TryParse(configVersion, out version))
                        {
                            if (version > 1)
                            {
                                MessageBox.Show(
                                    LocalizedData.ConfigIsNewerThanSupported,
                                    LocalizedData.PluginName,
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Exclamation);
                            }
                            // Correct version, read config's content.
                            else if (version == 1)
                            {
                                stream.Position = 0;

                                var serializer = new XmlSerializer(typeof(ConfigObjectV1));
                               
                                try
                                {
                                    co = (ConfigObjectV1)serializer.Deserialize(stream);
                                }
                                catch (InvalidOperationException ex)
                                {
                                    MessageBox.Show(
                                        LocalizedData.FailedToDeserializePlugin + ex.Message,
                                        LocalizedData.PluginName,
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Exclamation);
                                }
                            }
                            else // This is really odd situation, negative or zero version...
                            {
                                throw new ApplicationException("Unknown version of configuration file: " + version);
                            }
                        }                        
                    }
                }
            }

            co = co ?? new ConfigObjectV1();

            #endregion

            #region Transfer data from the storage into this object.

            IsEnabled = co.IsEnabled;
            Height = co.Height;
            Width = co.Width;
            Left = co.Left;
            Top = co.Top;
            ShowInTaskbar = co.ShowInTaskbar;
            EnableHotKeys = co.EnableHotKeys;

            foreach (var rule in _rules)
            {
                rule.Enabled = co.Rules.Contains(rule.Rule);
            }

            #endregion
        }
        
        #region Public properties

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

        public bool SaveImageAtTags { get; set; }

        /// <summary>
        /// Gets a collection of cover finding rules.
        /// </summary>
        [Pure]
        public IEnumerable<FindRule> Rules
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<FindRule>>() != null);

                return _rules;
            }
        }

        #endregion
        
        private static volatile Config _instance;
        /// <summary>
        /// Gets instance of current configuration object.
        /// Creates it if it hasn't been requested yet.
        /// </summary>
        public static Config Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new Config();
                        }
                    }
                }
                return _instance;
            }
        }

        public static String ConfigFolderPath
        {
            get
            {
                return _configFolderPath;
            }

            set
            {
                _configFolderPath = value;
            }
        }
                                
        /// <summary>
        /// Saves changes made to this object to configuration file
        /// and raises the <see cref="Saved"/> event;
        /// </summary>
        public void StoreChanges()
        {
            #region Create a storage object with a copy of this object's data.

            var configObject = new ConfigObjectV1
            {
                IsEnabled = IsEnabled,
                Height = Height,
                Width = Width,
                Left = Left,
                Top = Top,
                ShowInTaskbar = ShowInTaskbar,
                EnableHotKeys = EnableHotKeys,
                Rules = _rules
                    .Where(r => r.Enabled)
                    .Select(r => r.Rule)
                    .ToArray()
            };

            #endregion

            #region Serialize storage object to an XML file.

            if (!Directory.Exists(_configFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(_configFolderPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        LocalizedData.FailedToCreateConfigFolder + ex.Message,
                        LocalizedData.PluginName,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    throw;
                }
            }

            String configFile = Path.Combine(_configFolderPath, "config.xml");

            var serializer = new XmlSerializer(typeof(ConfigObjectV1));
            using (var ms = new MemoryStream())
            {
                serializer.Serialize(ms, configObject);
                ms.Flush();
                byte[] buf = ms.GetBuffer();
                if (ms.Length > 0)
                {
                    using (var fs = new FileStream(configFile, FileMode.Create))
                    {
                        fs.Write(buf, 0, (int) ms.Length);
                    }
                }
            }

            #endregion

            OnSaved(this, EventArgs.Empty);
        }

        /// <summary>
        /// An event which indicates that config has been stored to disk.
        /// </summary>
        public event EventHandler Saved;

        /// <summary>
        /// Raises <see cref="Saved"/> event.
        /// </summary>
        private void OnSaved(object sender, EventArgs e)
        {
            var temp = System.Threading.Interlocked.CompareExchange(ref Saved, null, null);

            if (temp != null)
            {
                temp(sender, e);
            }
        }
    }
}
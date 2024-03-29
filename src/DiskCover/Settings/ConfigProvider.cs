using System.Linq;
using AIMP.SDK.Player;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using AIMP.DiskCover.Infrastructure;
using AIMP.DiskCover.Infrastructure.Events;
using AIMP.DiskCover.Interfaces;
using AIMP.SDK;

namespace AIMP.DiskCover
{
    /// <summary>
    /// A singleton that is responsible for configuration logic of the plugin.
    /// </summary>
    public class ConfigProvider : IPluginSettings, IConfigProvider
    {
        private const string SectionName = "DiskCover.Settings";
        private readonly string SettingEnabled = $"{SectionName}\\Enabled";
        private readonly string SettingHeight = $"{SectionName}\\Height";
        private readonly string SettingWidth = $"{SectionName}\\Width";
        private readonly string SettingLeft = $"{SectionName}\\Left";
        private readonly string SettingTop = $"{SectionName}\\Top";
        private readonly string SettingShowInTaskbar = $"{SectionName}\\ShowInTaskbar";
        private readonly string SettingEnableHotKeys = $"{SectionName}\\EnableHotKeys";
        private readonly string SettingRules = $"{SectionName}\\Rules";
        private readonly string SettingDebug = $"{SectionName}\\DebugMode";

        private readonly IAimpPlayer _player;
        private readonly IEventAggregator _aggregator;

        /// <summary>
        /// A read-only collection of all available cover finding rules.
        /// </summary>
        private static readonly ReadOnlyCollection<FindRule> _rules = new ReadOnlyCollection<FindRule>(FindRule.GetAvailableRules());

        public ConfigProvider(IAimpPlayer player, IEventAggregator aggregator)
        {
            _player = player;
            _aggregator = aggregator;
            _aggregator.Register<SaveConfigEventArgs>(StoreChanges);
            LoadSettings();
        }

        private void LoadSettings()
        {
            Height = _player.ServiceConfig.GetValueAsFloat(SettingHeight).Result;
            Width = _player.ServiceConfig.GetValueAsFloat(SettingWidth).Result;

            var emptyConfig = Height == 0 && Width == 0;
            if (emptyConfig)
            {
                Height = 200;
                Width =  200;
                IsEnabled = true;
                DebugMode = false;

                foreach (var rule in _rules)
                {
                    rule.Enabled = true;
                }

                AppliedRules = _rules;
            }
            else
            {
                IsEnabled = _player.ServiceConfig.GetValueAsInt32(SettingEnabled).Result == 1;
                ShowInTaskbar = _player.ServiceConfig.GetValueAsInt32(SettingShowInTaskbar).Result == 1;
                EnableHotKeys = _player.ServiceConfig.GetValueAsInt32(SettingEnableHotKeys).Result == 1;
                DebugMode = _player.ServiceConfig.GetValueAsInt32(SettingDebug).Result == 1;

                var rules = _player.ServiceConfig
                    .GetValueAsString(SettingRules).Result
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c =>
                    {
                        Enum.TryParse(c, out CoverRuleType type);
                        return type;
                    }).ToList();

                var appliedRules = new List<FindRule>(_rules.Count);
                foreach (var rule in _rules)
                {
                    rule.Enabled = rules.Any(c => rule.Rule == c);
                }

                foreach (var rule in rules)
                {
                    appliedRules.Add(_rules.Single(c => c.Rule == rule));
                }

                AppliedRules = appliedRules;
            }

            Left = _player.ServiceConfig.GetValueAsFloat(SettingLeft).Result;
            Top = _player.ServiceConfig.GetValueAsFloat(SettingTop).Result;
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

        public bool SaveImageAtTags { get; set; }

        /// <summary>
        /// Gets a collection of cover finding rules.
        /// </summary>
        [Pure]
        public IEnumerable<FindRule> Rules => _rules;

        public IEnumerable<FindRule> AppliedRules { get; set; }

        public bool DebugMode { get; set; }

        public void StoreChanges()
        {
            _player.ServiceConfig.SetValueAsInt32(SettingEnabled, IsEnabled ? 1 : 0);
            _player.ServiceConfig.SetValueAsFloat(SettingHeight, (float)Height);
            _player.ServiceConfig.SetValueAsFloat(SettingWidth, (float)Width);
            _player.ServiceConfig.SetValueAsFloat(SettingLeft, (float)Left);
            _player.ServiceConfig.SetValueAsFloat(SettingTop, (float)Top);
            _player.ServiceConfig.SetValueAsInt32(SettingShowInTaskbar, ShowInTaskbar ? 1 : 0);
            _player.ServiceConfig.SetValueAsInt32(SettingEnableHotKeys, EnableHotKeys ? 1 : 0);
            _player.ServiceConfig.SetValueAsInt32(SettingDebug, DebugMode ? 1 : 0);
            _player.ServiceConfig.SetValueAsString(SettingRules, string.Join(";", AppliedRules.Select(c => c.Rule.ToString())));
            _player.ServiceConfig.FlushCache();
        }

        private void StoreChanges(SaveConfigEventArgs e)
        {
            StoreChanges();
        }
    }
}
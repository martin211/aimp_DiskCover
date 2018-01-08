using System.Linq;
using AIMP.DiskCover.Settings;
using AIMP.SDK.Player;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using AIMP.DiskCover.Core;


namespace AIMP.DiskCover
{
    /// <summary>
    /// A singleton that is responsible for configuration logic of the plugin.
    /// </summary>
    public class ConfigProvider : IPluginSettings, IConfigProvider
    {
        private const string SectionName = "DiskCover.Settings";
        private const string SettingEnabled = "Enabled";
        private const string SettingHeight = "Height";
        private const string SettingWidth = "Width";
        private const string SettingLeft = "Left";
        private const string SettingTop = "Top";
        private const string SettingShowInTaskbar = "ShowInTaskbar";
        private const string SettingEnableHotKeys = "EnableHotKeys";
        private const string SettingRules = "Rules";

        private readonly IAimpPlayer _player;
        private readonly IPluginEvents _pluginEvents;

        /// <summary>
        /// A read-only collection of all available cover finding rules.
        /// </summary>
        private static readonly ReadOnlyCollection<FindRule> _rules = new ReadOnlyCollection<FindRule>(FindRule.GetAvailableRules());

        public ConfigProvider(IAimpPlayer player, IPluginEvents pluginEvents)
        {
            _player = player;
            _pluginEvents = pluginEvents;
            _pluginEvents.SaveConfig += (sender, args) => { StoreChanges(); };
            LoadSettings();
        }

        private void LoadSettings()
        {
            IsEnabled = _player.ServiceConfig.GetValueAsInt32($"{SectionName}\\{SettingEnabled}") == 1;
            Height = _player.ServiceConfig.GetValueAsFloat($"{SectionName}\\{SettingHeight}");
            Width = _player.ServiceConfig.GetValueAsFloat($"{SectionName}\\{SettingWidth}");
            Left = _player.ServiceConfig.GetValueAsFloat($"{SectionName}\\{SettingLeft}");
            Top = _player.ServiceConfig.GetValueAsFloat($"{SectionName}\\{SettingTop}");
            ShowInTaskbar = _player.ServiceConfig.GetValueAsInt32($"{SectionName}\\{SettingShowInTaskbar}") == 1;
            EnableHotKeys = _player.ServiceConfig.GetValueAsInt32($"{SectionName}\\{SettingEnableHotKeys}") == 1;

            var rules = _player.ServiceConfig
                .GetValueAsString($"{SectionName}\\{SettingRules}")
                .Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
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
                return _rules;
            }
        }

        public IEnumerable<FindRule> AppliedRules { get; set; }

        public void StoreChanges()
        {
            _player.ServiceConfig.SetValueAsInt32($"{SectionName}\\{SettingEnabled}", IsEnabled ? 1 : 0);
            _player.ServiceConfig.SetValueAsFloat($"{SectionName}\\{SettingHeight}", (float)Height);
            _player.ServiceConfig.SetValueAsFloat($"{SectionName}\\{SettingWidth}", (float)Width);
            _player.ServiceConfig.SetValueAsFloat($"{SectionName}\\{SettingLeft}", (float)Left);
            _player.ServiceConfig.SetValueAsFloat($"{SectionName}\\{SettingTop}", (float)Top);
            _player.ServiceConfig.SetValueAsInt32($"{SectionName}\\{SettingShowInTaskbar}", ShowInTaskbar ? 1 : 0);
            _player.ServiceConfig.SetValueAsInt32($"{SectionName}\\{SettingEnableHotKeys}", EnableHotKeys ? 1 : 0);
            _player.ServiceConfig.SetValueAsString($"{SectionName}\\{SettingRules}", string.Join(";", AppliedRules.Select(c => c.Rule.ToString())));
            _player.ServiceConfig.FlushCache();
        }
    }
}
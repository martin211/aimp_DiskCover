using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using AIMP.DiskCover.Annotations;
using AIMP.DiskCover.Infrastructure;
using AIMP.DiskCover.Infrastructure.Events;
using AIMP.DiskCover.Interfaces;

namespace AIMP.DiskCover.Settings
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private bool _isEnabled;
        private bool _enableHotKeys;
        private double _height;
        private double _width;
        private double _left;
        private double _top;
        private CoverRuleType[] _rules;
        private readonly IEventAggregator _aggregator;
        private IPluginSettings _pluginSettings;

        private ObservableCollection<FindRule> _appliedRules;
        private ObservableCollection<FindRule> _availableRules;

        public SettingsViewModel(IPluginSettings settings, IEventAggregator aggregator)
        {
            _pluginSettings = settings;
            _aggregator = aggregator;
            AppliedRules = new ObservableCollection<FindRule>(settings.AppliedRules);
            AvailableRules = new ObservableCollection<FindRule>(settings.Rules.Where(r => !r.Enabled));
        }

        public ObservableCollection<FindRule> AppliedRules
        {
            get => _appliedRules;
            set
            {
                _appliedRules = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FindRule> AvailableRules
        {
            get => _availableRules;
            set
            {
                _availableRules = value;
                OnPropertyChanged();
            }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        public double Height
        {
            get => _height;
            set
            {
                _height = value;
                OnPropertyChanged();
            }
        }

        public double Width
        {
            get => _width;
            set
            {
                _width = value;
                OnPropertyChanged();
            }
        }

        public double Left
        {
            get => _left;
            set
            {
                _left = value;
                OnPropertyChanged();
            }
        }

        public double Top
        {
            get => _pluginSettings.Top;
            set
            {
                _pluginSettings.Top = value;
                OnPropertyChanged();
            }
        }

        public bool EnableHotKeys
        {
            get => _pluginSettings.EnableHotKeys;
            set
            {
                _pluginSettings.EnableHotKeys = value;
                OnPropertyChanged();
            }
        }

        public bool ShowInTaskbar
        {
            get => _pluginSettings.ShowInTaskbar;
            set
            {
                _pluginSettings.ShowInTaskbar = value;
                OnPropertyChanged();
            }
        }

        public bool IsDebugMode
        {
            get => _pluginSettings.DebugMode;
            set
            {
                _pluginSettings.DebugMode = value;
                OnPropertyChanged();
            }
        }

        public Visibility IsDebug
        {
            get
            {
                return Visibility.Visible;
            }
        }

        public FindRule SelectedAvailableRule { get; set; }

        public FindRule SelectedAppliedRule { get; set; }

        public CoverRuleType[] Rules
        {
            get => _rules;
            set
            {
                _rules = value;
                OnPropertyChanged();
            }
        }

        public string Version
        {
            get
            {
                var asm = Assembly.GetExecutingAssembly();
                var ver = FileVersionInfo.GetVersionInfo(asm.Location);
                return ver.FileVersion;
            }
        }

        public string InformationalVersion
        {
            get
            {
                var asm = Assembly.GetExecutingAssembly();
                var ver = FileVersionInfo.GetVersionInfo(asm.Location);
                return ver.ProductVersion;
            }
        }

        private ICommand _addRuleCommand;
        public ICommand AddRuleCommand
        {
            get { return _addRuleCommand ?? (_addRuleCommand = new RelayCommand(
                             c => AddRule(),
                             c => CanAddRule)); }
        }

        private ICommand _deleteRuleCommand;
        public ICommand DeleteRuleCommand
        {
            get { return _deleteRuleCommand ?? (_deleteRuleCommand = new RelayCommand(
                             c => DeleteRule(),
                             c =>  CanRemoveRule)); }
        }

        private ICommand _incrementRuleCommand;
        public ICommand IncrementRuleCommand
        {
            get { return _incrementRuleCommand ?? (_incrementRuleCommand = new RelayCommand(
                             c => IncrementRule(),
                             c => CanUpPosition)); }
        }

        private ICommand _decrementRuleCommand;
        public ICommand DecrementRuleCommand
        {
            get
            {
                return _decrementRuleCommand ?? (_decrementRuleCommand = new RelayCommand(
                           c => DecrimentRulePosition(),
                           c => CanDownPosition));
            }
        }

        private ICommand _copyToClipboard;

        public ICommand CopyToClipboard
        {
            get
            {
                return _copyToClipboard ?? (_copyToClipboard = new RelayCommand(c => CopyInformationToClipboard()));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            // TODO find best way how to update it
            _pluginSettings.AppliedRules = new List<FindRule>(AppliedRules);
            _aggregator.Raise(new ConfigUpdatedEventArgs());
        }

        private void AddRule()
        {
            if (SelectedAvailableRule == null)
            {
                return;
            }

            SelectedAvailableRule.Enabled = true;
            AppliedRules.Add(SelectedAvailableRule);
            AvailableRules.Remove(SelectedAvailableRule);
            OnPropertyChanged();
        }

        private void DeleteRule()
        {
            if (SelectedAppliedRule == null)
            {
                return;
            }

            SelectedAppliedRule.Enabled = false;
            AvailableRules.Add(SelectedAppliedRule);
            AppliedRules.Remove(SelectedAppliedRule);
            OnPropertyChanged();
        }

        private void IncrementRule()
        {
            var currentItemIndex = AppliedRules.IndexOf(SelectedAppliedRule);
            if (currentItemIndex > 0)
            {
                AppliedRules.Insert(currentItemIndex-1, SelectedAppliedRule);
                AppliedRules.RemoveAt(currentItemIndex+1);
                SelectedAppliedRule = AppliedRules.ElementAt(currentItemIndex - 1);
                OnPropertyChanged(nameof(AppliedRules));
            }
        }

        private void DecrimentRulePosition()
        {
            var currentItemIndex = AppliedRules.IndexOf(SelectedAppliedRule);
            if (currentItemIndex != AppliedRules.Count - 1)
            {
                var copySelectedItem = new FindRule
                {
                    Enabled = SelectedAppliedRule.Enabled,
                    Module = SelectedAppliedRule.Module,
                    Rule = SelectedAppliedRule.Rule
                };
                AppliedRules.Insert(currentItemIndex + 2, copySelectedItem);
                AppliedRules.RemoveAt(currentItemIndex);
                SelectedAppliedRule = AppliedRules.ElementAt(currentItemIndex + 1);
                OnPropertyChanged(nameof(AppliedRules));
            }
        }

        private void CopyInformationToClipboard()
        {
            Clipboard.SetText(InformationalVersion);
        }

        private bool CanAddRule => AvailableRules.Any() && SelectedAvailableRule != null;

        private bool CanRemoveRule => AppliedRules.Any() && SelectedAppliedRule != null;

        private bool CanUpPosition => AppliedRules.Any() && SelectedAppliedRule != null && AppliedRules.IndexOf(SelectedAppliedRule) > 0;

        private bool CanDownPosition => AppliedRules.Any() && SelectedAppliedRule != null && AppliedRules.IndexOf(SelectedAppliedRule) != AppliedRules.Count - 1;
    }
}
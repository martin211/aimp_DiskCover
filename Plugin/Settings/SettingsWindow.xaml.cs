using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AIMP.DiskCover;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace DiskCover.Settings
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public ObservableCollection<FindRule> AppliedRules = new ObservableCollection<FindRule>(Config.Instance.Rules.Where(r => r.Enabled));
        public ObservableCollection<FindRule> AvailableRules = new ObservableCollection<FindRule>(Config.Instance.Rules.Where(r => !r.Enabled));

        public SettingsWindow()
        {
            try
            {
                InitializeComponent();

                chShowInTaskbar.IsChecked = Config.Instance.ShowInTaskbar;
                chEnableHotkeys.IsChecked = Config.Instance.EnableHotKeys;

                lbApplied.ItemsSource = AppliedRules;
                lbAvailable.ItemsSource = AvailableRules;
            }
            catch (Exception ex)
            {
                
                
            }
        }
        
        private void bSave_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

            Config.Instance.ShowInTaskbar = chShowInTaskbar.IsChecked ?? false;
            Config.Instance.EnableHotKeys = chEnableHotkeys.IsChecked ?? false;

            // User's rules choise is already saved by button handlers.
            
            Config.Instance.StoreChanges();
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (lbAvailable.SelectedItem == null) 
                return;

            FindRule rule = (FindRule)lbAvailable.SelectedItem;
            rule.Enabled = true;

            AvailableRules.Remove(rule);
            AppliedRules.Add(rule);
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (lbApplied.SelectedItem == null) 
                return;

            FindRule rule = (FindRule)lbApplied.SelectedItem;
            rule.Enabled = false;

            AppliedRules.Remove(rule);
            AvailableRules.Add(rule);
        }
    }

    [ValueConversion(typeof(Object), typeof(Boolean))]
    public class ObjectToBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AIMP.DiskCover.Settings
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : UserControl
    {
        //public ObservableCollection<FindRule> AppliedRules = new ObservableCollection<FindRule>(Config.Instance.Rules.Where(r => r.Enabled));
        //public ObservableCollection<FindRule> AvailableRules = new ObservableCollection<FindRule>(Config.Instance.Rules.Where(r => !r.Enabled));

        public SettingsWindow()
        {
            InitializeComponent();

            //try
            //{
                

            //    chShowInTaskbar.IsChecked = Config.Instance.ShowInTaskbar;
            //    chEnableHotkeys.IsChecked = Config.Instance.EnableHotKeys;

            //    lbApplied.ItemsSource = AppliedRules;
            //    lbAvailable.ItemsSource = AvailableRules;

            //    DataContext = new SettingsViewModel();
            //}
            //catch (Exception ex)
            //{
                
                
            //}
        }
    }

    [ValueConversion(typeof(Object), typeof(Boolean))]
    public class ObjectToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

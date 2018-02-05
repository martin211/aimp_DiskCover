using System.Runtime.InteropServices;
using AIMP.DiskCover.Infrastructure;
using AIMP.DiskCover.Interfaces;
using AIMP.DiskCover.Settings;

namespace AIMP.DiskCover
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;
    using System.Diagnostics;

    using Resources;

    /// <summary>
    /// Interaction logic for CoverWindow.xaml
    /// </summary>
    public partial class CoverWindow
    {
        private Bitmap _coverImage;
        private IPluginSettings _settings;
        private IPluginEventsExecutor _pluginEventsExecutor;

        [DllImport("msvcr120.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int _fpreset();

        /// <summary>
        /// The WPF image element that shows AIMP's bitmap.
        /// </summary>
        public System.Windows.Controls.Image ImageElement
        {
            get
            {
                return coverImage;
            }
        }

        public CoverWindow(IPluginSettings settings, IPluginEventsExecutor pluginEventsExecutor)
        {
            _settings = settings;
            _pluginEventsExecutor = pluginEventsExecutor;
            Init();
        }

        private void Init()
        {
            try
            {
                _fpreset();
                InitializeComponent();

                ShowInTaskbar = _settings.ShowInTaskbar;

                // Set maximum allowed sizes of the window.
                MaxWidth = System.Windows.SystemParameters.WorkArea.Width;
                MaxHeight = System.Windows.SystemParameters.WorkArea.Height;

                if (MaxWidth <= MaxHeight)
                {
                    MessageBox.Show(
                        LocalizedData.UnusualProportionsMessage,
                        LocalizedData.PluginName,
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                }
            }
            catch (Exception ex)
            {
            }
        }

        void ConfigChangedHandler(object sender, EventArgs e)
        {
            ShowInTaskbar = _settings.ShowInTaskbar;
        }

        public void ChangeCoverImage(Bitmap image)
        {
            if (image == null)
            {
                image = LocalizedData.NoCoverImage;
            }

            _coverImage = image;

            var ms = new MemoryStream();
            _coverImage.Save(ms, ImageFormat.Png);
            ms.Position = 0;
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            coverImage.Source = bi;
            ResizeImage();
            EndLoadImage();
        }

        public void Show(IntPtr parent)
        {
            
        }

        private void ResizeImage()
        {
            if (_coverImage != null)
            {
                Single w;
                Single h;

                Algorithms.ProportionalImageResize((float)ActualWidth, (float)ActualHeight, _coverImage.Width, _coverImage.Height, out w, out h);

                coverImage.Width = w;
                coverImage.Height = h;
            }
        }

        /// <summary>
        /// Sets 'Normal' state to the window and shows it.
        /// </summary>
        public void Display()
        {
            WindowState = WindowState.Normal;
            Show();
        }

        public void BeginLoadImage()
        {
            if (loading.Visibility != Visibility.Hidden)
            {
                return;
            }

            loading.Visibility = Visibility.Visible;

            var w = new Binding 
                        { 
                            Mode = BindingMode.OneWay,
                            ElementName = "dockPanel",
                            Path = new PropertyPath("ActualWidth")
                        };                
            loading.SetBinding(WidthProperty, w);

            var h = new Binding
                        {
                            Mode = BindingMode.OneWay,
                            ElementName = "dockPanel",
                            Path = new PropertyPath("ActualHeight")
                        };
            loading.SetBinding(HeightProperty, h);
        }

        public void EndLoadImage()
        {
            loading.Visibility = System.Windows.Visibility.Hidden;
            
            loading.Height = 0;
            loading.Width = 0;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _settings.Width = Width;
            _settings.Height = Height;
            _settings.Left = Left;
            _settings.Top = Top;

            ResizeImage();
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _settings.Width = Width;
            _settings.Height = Height;
            _settings.Left = Left;
            _settings.Top = Top;

            _pluginEventsExecutor.OnSaveConfig();
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // TODO: Add event handler implementation here.
        }

        private void Window_SourceInitialized(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
        }

        /// <summary>
        /// Indicates that user requested window to resize holding the square proportions.
        /// </summary>
        private bool _squareResizeIsRequired;

        /// <summary>
        /// Indicates that user requested window to resize holding current cover image's proportions.
        /// </summary>
        private bool _proportialResizeIsRequired;

        /// <summary>
        /// Indicates that user requested window to resize holding current cover image's proportions.
        /// </summary>
        private bool _currentProportionsResizeIsRequired;

        /// <summary>
        /// Proportions that window had when user first requested the proportional resize.
        /// </summary>
        private Double? _currentProportions;
        
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (_settings.EnableHotKeys)
            {
                if ((e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt) && e.IsDown)
                {
                    _proportialResizeIsRequired = true;
                }
                else if ((e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) && e.IsDown)
                {
                    // Only initial ratio will be stored.
                    if (_currentProportionsResizeIsRequired == false)
                    {
                        _currentProportions = this.ActualWidth / this.ActualHeight;
                    }

                    _currentProportionsResizeIsRequired = true;
                }
                else if ((e.Key == Key.LeftShift || e.Key == Key.RightShift) && e.IsDown)
                {
                    _squareResizeIsRequired = true;
                }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (_settings.EnableHotKeys)
            {
                if (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt)
                {
                    _proportialResizeIsRequired = false;
                }
                if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                {
                    _currentProportionsResizeIsRequired = false;
                    _currentProportions = null;
                }
                if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                {
                    _squareResizeIsRequired = false;
                }
            }
        }

        private void ResizeGripDragDelta(object aSender, System.Windows.Controls.Primitives.DragDeltaEventArgs aEventArgs)
        {
            double lWidth = ActualWidth + aEventArgs.HorizontalChange;
            double lHeight = ActualHeight + aEventArgs.VerticalChange;

            TrySetSize(lWidth, lHeight);
        }

        private void ResizeGripMouseDown(object aSender, MouseButtonEventArgs aEventArgs)
        {
            if (aEventArgs.LeftButton == MouseButtonState.Pressed &&
                  aEventArgs.RightButton == MouseButtonState.Released &&
                  aEventArgs.MiddleButton == MouseButtonState.Released)
            {
                Mouse.Capture(ResizeGrip);
            }
        }

        private void ResizeGripMouseUp(object aSender, MouseButtonEventArgs aEventArgs)
        {
            if (Mouse.Captured == ResizeGrip)
            {
                Mouse.Capture(null);
            }
        }

        /// <summary>
        /// Resizes the window according to currently pressed buttons.
        /// </summary>
        private void TrySetSize(double newWidth, double newHeight)
        {
            if (_proportialResizeIsRequired)
            {
                RecalculateToComplyWithTheRatio((Double)_coverImage.Width / _coverImage.Height, ref newWidth, ref newHeight);
            }
            else if (_currentProportionsResizeIsRequired)
            {
                Debug.Assert(_currentProportions.HasValue, "Current window's proportional resize is requested, but current proportiones were not saved.");

                RecalculateToComplyWithTheRatio(_currentProportions.Value, ref newWidth, ref newHeight);
            }
            else if (_squareResizeIsRequired)
            {
                RecalculateToComplyWithTheRatio(1, ref newWidth, ref newHeight);
            }
            else
            {
                if (newWidth < MinWidth)
                {
                    newWidth = MinWidth;
                }
                if (newHeight < MinHeight)
                {
                    newHeight = MinHeight;
                }
            }

            this.Width = newWidth;
            this.Height = newHeight;
        }

        /// <summary>
        /// Using the biggest passed dimension, this function tries to adjust the other one 
        /// that way, that result will have the specified ratio.
        /// </summary>
        /// <param name="ratio">Ratio that result must comply with.</param>
        /// <param name="width">Original width.</param>
        /// <param name="height">Original height.</param>
        private void RecalculateToComplyWithTheRatio(Double ratio, ref Double width, ref Double height)
        {
            // Always resize to biggest dimension.
            if (width >= height)
            {
                // Compress by width
                height = width / ratio;
                if (height < 1) height = 1;
            }
            else
            {
                // Compress by height
                width = height * ratio;
                if (width < 1) width = 1;
            }

            // If one of dimensions became less than  allowed, consider it as equal
            // to the min. allowed size and make the other dimension eqaul to ratio * min. size

            // If height is greater than width.
            if (ratio < 1)
            {
                if (width < MinWidth)
                {
                    width = MinWidth;
                    height = width / ratio;
                    if (height < 1) height = 1;
                }
            }
            else
            {
                if (height <= MinHeight)
                {
                    height = MinHeight;
                    width = height * ratio;
                    if (width < 1) width = 1;
                }
            }

            // If one of dimensions became greater than allowed, consider it as equal
            // to the max. allowed size and make the other dimension eqaul to ratio * max. size

            // If height is greater than width.
            if (ratio < 1)
            {
                if (height > MaxHeight)
                {
                    height = MaxHeight;
                    width = height * ratio;
                    if (width < 1) width = 1;
                }                
            }
            else if (ratio > 1)
            {
                if (width > MaxWidth)
                {
                    width = MaxWidth;
                    height = width / ratio;
                    if (height < 1) height = 1;
                }
            }
            else
            {
                if (MaxWidth <= MaxHeight)
                {
                    // This case is not supported (unusual type of screen proportions).
                    return;
                }

                // In case of ratio 1 we could exceed only height, as width 
                // is for sure greater than height. 
                if (height > MaxHeight)
                {
                    width = height = MaxHeight;
                }                
            }
        }
    }
}

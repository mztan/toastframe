using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Em.UI.Xaml.Controls
{
    /// <summary>
    /// Represents the status bar within a ToastFrame control.
    /// </summary>
    public class ToastFrameStatusBar : Control
    {
        /// <summary>
        /// Gets or sets whether the status bar is currently displayed on the screen.
        /// </summary>
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(ToastFrameStatusBar), new PropertyMetadata(null, IsOpenChanged));

        /// <summary>
        /// Gets or sets whether the progress bar shows a repeating pattern.
        /// </summary>
        public bool IsIndeterminate
        {
            get { return (bool)GetValue(IsIndeterminateProperty); }
            set { SetValue(IsIndeterminateProperty, value); }
        }

        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register("IsIndeterminate", typeof(bool), typeof(ToastFrameStatusBar), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value representing progress in the range 0 to 1.
        /// </summary>
        public double ProgressValue
        {
            get { return (double)GetValue(ProgressValueProperty); }
            set { SetValue(ProgressValueProperty, value); }
        }

        public static readonly DependencyProperty ProgressValueProperty =
            DependencyProperty.Register("ProgressValue", typeof(double), typeof(ToastFrameStatusBar), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the fill of the progress bar.
        /// </summary>
        public SolidColorBrush ProgressBarFill
        {
            get { return (SolidColorBrush)GetValue(ProgressBarFillProperty); }
            set { SetValue(ProgressBarFillProperty, value); }
        }

        public static readonly DependencyProperty ProgressBarFillProperty =
            DependencyProperty.Register("ProgressBarFill", typeof(SolidColorBrush), typeof(ToastFrameStatusBar), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the status bar text.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ToastFrameStatusBar), new PropertyMetadata(null));

        private static void IsOpenChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as ToastFrameStatusBar;
            if (control == null) return;

            VisualStateManager.GoToState(control, control.IsOpen ? "StatusBarVisible" : "StatusBarHidden", true);
        }

        /// <summary>
        /// Represents the status bar built into a ToastFrame.
        /// </summary>
        public ToastFrameStatusBar()
        {
            DefaultStyleKey = typeof (ToastFrameStatusBar);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            VisualStateManager.GoToState(this, "StatusBarHidden", false);
        }
    }
}

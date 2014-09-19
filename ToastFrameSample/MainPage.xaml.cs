using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Em.UI.Xaml.Controls;

namespace ToastFrameSample
{
    public sealed partial class MainPage
    {
        private ToastFrame _frame;

        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _frame = (ToastFrame)Frame;
        }

        private void Short_OnClick(object sender, RoutedEventArgs e)
        {
            _frame.ShowToast("This is a short actionable toast. You can tap to activate, or swipe to dismiss.");
        }

        private void Info_OnClick(object sender, RoutedEventArgs e)
        {
            _frame.ShowInfoToast("This is an informational toast. It is not actionable.");
        }

        private void Long_OnClick(object sender, RoutedEventArgs e)
        {
            _frame.ShowToast("This is a very long toast. It spans up to 3 lines. Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat");
        }

        private void WithTitle_OnClick(object sender, RoutedEventArgs e)
        {
            _frame.ShowToast("This is a toast with title text in bold", "Title Text");
        }

        private void LongWithTitle_OnClick(object sender, RoutedEventArgs e)
        {
            _frame.ShowToast("This is a long toast with title text. The title text spans 2 lines, and the body text spans 3 lines. ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud", "Title text Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor");
        }

        private void Hide_OnClick(object sender, RoutedEventArgs e)
        {
            _frame.HideToast();
        }

        private void Clear_OnClick(object sender, RoutedEventArgs e)
        {
            _frame.ClearAllToasts();
        }

        private void SetStatusBar_OnClick(object sender, RoutedEventArgs e)
        {
            _frame.StatusBar.Text = StatusBarText.Text;
            _frame.StatusBar.IsOpen = true;
        }

        private void HideStatusBar_OnClick(object sender, RoutedEventArgs e)
        {
            _frame.StatusBar.IsOpen = false;
        }
    }
}

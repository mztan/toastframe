using System;
using Windows.ApplicationModel.Activation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Em.UI.Xaml.Controls;

namespace ToastFrameSample
{
    public sealed partial class App
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            // Recommended that you hide the status bar
            StatusBar.GetForCurrentView().HideAsync();

            var rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                // ToastFrame is constructed and used here
                rootFrame = new ToastFrame();
                rootFrame.CacheSize = 1;
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                if (!rootFrame.Navigate(typeof(MainPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            Window.Current.Activate();
        }
    }
}
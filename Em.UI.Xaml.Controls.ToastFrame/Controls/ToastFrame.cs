using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Em.UI.Xaml.Controls
{
    /// <summary>
    /// Adds toast-like notifications and an easier to use status bar to the built-in Frame control.
    /// It is recommended that the app hides the built-in StatusBar when using this control.
    /// </summary>
    public class ToastFrame : Frame
    {
        // Swipe must have a certain amount of speed in order to qualify as a swipe
        private const double SwipeSpeedThreshold = 1.0;

        // Alternatively, you can swipe a certain distance (at any speed) and it will also count as a swipe.
        private const double SwipeDistanceThreshold = 90;

        // Must swipe a minimum distance in order to qualify as a swipe, regardless of speed.
        private const double MinSwipeDistanceThreshold = 20;
        
        // Default timeout duration for a toast
        private static readonly TimeSpan ToastTimeout = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Gets or sets a brush that provides the background of the toast notification.
        /// </summary>
        public SolidColorBrush ToastBackground
        {
            get { return (SolidColorBrush)GetValue(ToastBackgroundProperty); }
            set { SetValue(ToastBackgroundProperty, value); }
        }

        public static readonly DependencyProperty ToastBackgroundProperty =
            DependencyProperty.Register("ToastBackground", typeof(SolidColorBrush), typeof(ToastFrame), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets a brush that provides the foreground of the toast notification.
        /// </summary>
        public SolidColorBrush ToastForeground
        {
            get { return (SolidColorBrush)GetValue(ToastForegroundProperty); }
            set { SetValue(ToastForegroundProperty, value); }
        }

        public static readonly DependencyProperty ToastForegroundProperty =
            DependencyProperty.Register("ToastForeground", typeof(SolidColorBrush), typeof(ToastFrame), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the style for the inner ToastFrameStatusBar.
        /// </summary>
        public Style StatusBarStyle
        {
            get { return (Style)GetValue(StatusBarStyleProperty); }
            set { SetValue(StatusBarStyleProperty, value); }
        }

        public static readonly DependencyProperty StatusBarStyleProperty =
            DependencyProperty.Register("StatusBarStyle", typeof(Style), typeof(ToastFrame), new PropertyMetadata(null));

        /// <summary>
        /// Provides properties for interacting with the status bar and progress indicator.
        /// </summary>
        public ToastFrameStatusBar StatusBar { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a toast is currently visible.
        /// </summary>
        public bool IsToastVisible { get { return _currentToast != null; } }

        private readonly Queue<ToastInfo> _toastQueue;
        private readonly DispatcherTimer _toastTimer;
        private ToastInfo _currentToast;
        private bool _currentToastHandled;

        private TranslateTransform _toastTranslate;
        private TextBlock _toastTitle;
        private TextBlock _toastText;
        private UIElement _normalToast;
        private UIElement _infoToast;
        private TextBlock _infoToastText;

        /// <summary>
        /// Initializes a new instance of the ToastFrame class.
        /// </summary>
        public ToastFrame()
        {
            DefaultStyleKey = typeof(ToastFrame);

            _toastQueue = new Queue<ToastInfo>();
            _toastTimer = new DispatcherTimer { Interval = ToastTimeout };
            _toastTimer.Tick += ToastTimer_Tick;
        }

        /// <summary>
        /// Displays an informational toast that is not actionable and only displays one line of text.
        /// </summary>
        /// <param name="text">The text to display.</param>
        public void ShowInfoToast(string text)
        {
            EnsureThreadAccess();

            ShowToast(new ToastInfo(text, true));
        }

        /// <summary>
        /// Displays a toast on the screen.
        /// </summary>
        /// <param name="text">The text to display.</param>
        public void ShowToast(string text)
        {
            ShowToast(text, null, null);
        }

        /// <summary>
        /// Displays a toast on the screen.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="title">The title text to display.</param>
        public void ShowToast(string text, string title)
        {
            ShowToast(text, title, null);
        }

        /// <summary>
        /// Displays a toast on the screen. Calls the specified callback when the toast has been handled.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="callback">The callback to call.</param>
        /// <param name="state">Optional state.</param>
        public void ShowToast(string text, EventHandler<ToastHandledEventArgs> callback, object state = null)
        {
            ShowToast(text, null, callback, state);
        }

        /// <summary>
        /// Displays a toast on the screen. Calls the specified callback when the toast has been handled.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="title">The title text to display.</param>
        /// <param name="callback">The callback to call.</param>
        /// <param name="state">Optional state.</param>
        public void ShowToast(string text, string title, EventHandler<ToastHandledEventArgs> callback, object state = null)
        {
            EnsureThreadAccess();

            ShowToast(new ToastInfo(text, title, callback, state));
        }

        /// <summary>
        /// Hides the currently visible toast.
        /// </summary>
        public void HideToast()
        {
            EnsureThreadAccess();

            // Do nothing if there is no toast currently visible
            if (IsToastVisible)
            {
                ProcessToastQueue();
            }
        }

        /// <summary>
        /// Hides the currently visible toast and clears all toasts in the queue.
        /// </summary>
        public void ClearAllToasts()
        {
            EnsureThreadAccess();

            // Clear the queue and then call ProcessToastQueue, to ensure that the currently visible toast
            // gets hidden and cleared correctly.
            _toastQueue.Clear();
            if (IsToastVisible)
            {
                ProcessToastQueue();
            }
        }

        private void EnsureThreadAccess()
        {
            if (!Dispatcher.HasThreadAccess)
                throw new InvalidOperationException("not on UI thread");
        }

        private void ShowToast(ToastInfo toastInfo)
        {
            _toastQueue.Enqueue(toastInfo);
            if (!IsToastVisible)
            {
                ProcessToastQueue();
            }
        }

        // Showing a toast must result in exactly one of the following three outcomes:
        //  1. The toast is activated by the user tapping on the toast.
        //  2. The toast is dismissed by the user via swiping away the toast.
        //  3. The toast times out due to user inaction.
        private void OnToastActivated()
        {
            // If this toast has already been handled (i.e. it has already been activated, dismissed, or timed out), do nothing
            if (_currentToastHandled) return;
            _currentToastHandled = true;

            // Call the callback if necessary, passing along the result and the user-specified state
            if (_currentToast.Callback != null)
            {
                _currentToast.Callback(this, new ToastHandledEventArgs(ToastResult.Activated, _currentToast.State));
            }

            // Move ahead to the next toast in the queue
            ProcessToastQueue();
        }

        private void OnToastDismissed()
        {
            if (_currentToastHandled) return;
            _currentToastHandled = true;

            if (_currentToast.Callback != null)
            {
                _currentToast.Callback(this, new ToastHandledEventArgs(ToastResult.Dismissed, _currentToast.State));
            }
            ProcessToastQueue();
        }

        private void OnToastTimedOut()
        {
            if (_currentToastHandled) return;
            _currentToastHandled = true;

            if (_currentToast.Callback != null)
            {
                _currentToast.Callback(this, new ToastHandledEventArgs(ToastResult.TimedOut, _currentToast.State));
            }
            ProcessToastQueue();
        }

        private async void ProcessToastQueue()
        {
            // For simplicity's sake, we stop the timer on tick, and re-start it only when needed
            _toastTimer.Stop();

            // Ensure we can always reset to a safe state, should something go wrong
            if (_toastQueue.Count == 0)
            {
                _currentToast = null;
                _currentToastHandled = false;
                VisualStateManager.GoToState(this, "ToastHidden", true);
                return;
            }

            // If a toast is already visible, we need to do the hide animation for that toast before showing 
            // the show animation for the next toast.
            if (IsToastVisible)
            {
                VisualStateManager.GoToState(this, "ToastHidden", true);
                await Task.Delay(250); // This delay is obviously dependent on the duration of the visual state transition storyboard
            }

            // Get the next toast from the queue
            var toastInfo = _toastQueue.Dequeue();

            // Informational toasts and normal toasts have different UI
            if (_infoToast != null)
            {
                _infoToast.Visibility = toastInfo.IsInformational ? Visibility.Visible : Visibility.Collapsed;
            }
            if (_normalToast != null)
            {
                _normalToast.Visibility = toastInfo.IsInformational ? Visibility.Collapsed : Visibility.Visible;
            }

            if (toastInfo.IsInformational)
            {
                if (_infoToastText != null)
                {
                    _infoToastText.Text = toastInfo.Text ?? string.Empty; // TextBlock.Text property can not be set to null!
                }
            }
            else
            {
                if (_toastTitle != null)
                {
                    _toastTitle.Text = toastInfo.Title ?? string.Empty;
                    _toastTitle.Visibility = string.IsNullOrEmpty(toastInfo.Title)
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                }
                if (_toastText != null)
                {
                    _toastText.Text = toastInfo.Text ?? string.Empty;
                }
            }

            // Finally, transition to the visible state
            VisualStateManager.GoToState(this, "ToastVisible", true);

            // Set instance state and kick off timer
            _currentToast = toastInfo;
            _currentToastHandled = false;
            _toastTimer.Start();
        }

        void ToastTimer_Tick(object sender, object e)
        {
            OnToastTimedOut();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var outerToast = (UIElement)GetTemplateChild("OuterToast");
            if (outerToast != null)
            {
                _toastTranslate = outerToast.RenderTransform as TranslateTransform;
                if (_toastTranslate != null)
                {
                    outerToast.ManipulationStarted += _outerToast_ManipulationStarted;
                    outerToast.ManipulationDelta += _outerToast_ManipulationDelta;
                    outerToast.ManipulationCompleted += _outerToast_ManipulationCompleted;
                }

                outerToast.Tapped += _outerToast_Tapped;
            }

            var storyboard = (Storyboard)GetTemplateChild("DraggingToHiddenStoryboard");
            if (storyboard != null)
            {
                storyboard.Completed += DraggingToHiddenStoryboard_Completed;
            }

            _toastTitle = (TextBlock)GetTemplateChild("ToastTitle");
            _toastText = (TextBlock)GetTemplateChild("ToastText");

            _normalToast = (UIElement) GetTemplateChild("NormalToast");
            _infoToast = (UIElement) GetTemplateChild("InfoToast");
            _infoToastText = (TextBlock) GetTemplateChild("InfoToastText");

            StatusBar = (ToastFrameStatusBar) GetTemplateChild("StatusBar");

            // Ensure that we are initialized to the correct state
            VisualStateManager.GoToState(this, "ToastHidden", false);
        }

        void _outerToast_Tapped(object sender, TappedRoutedEventArgs e)
        {
            OnToastActivated();
        }

        void _outerToast_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            _toastTranslate.X = 0;
            VisualStateManager.GoToState(this, "ToastDragging", false);
        }

        void _outerToast_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.Cumulative.Translation.X >= 0)
            {
                _toastTranslate.X = e.Cumulative.Translation.X;
            }
        }

        void _outerToast_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var delta = e.Velocities.Linear.X;
            var distance = e.Cumulative.Translation.X;

            // There are 2 ways to dismiss a toast:
            //  1. Swiping quickly. Note that this has 2 conditions, one is the speed threshold, and the other
            //     is a minimum distance threshold. This reduces the chance that random touch-screen jitter will
            //     cause a swipe to be registered.
            //  2. Swiping a certain distance. Note that this can be done at any speed.
            if ((delta > SwipeSpeedThreshold && distance > MinSwipeDistanceThreshold) ||
                distance > SwipeDistanceThreshold)
            {
                VisualStateManager.GoToState(this, "ToastHidden", true);
            }
            else
            {
                _toastTranslate.X = 0;
                VisualStateManager.GoToState(this, "ToastVisible", false);
            }
        }

        void DraggingToHiddenStoryboard_Completed(object sender, object e)
        {
            // Reset the translation back to 0 (this is not done for us automatically in the storyboard)
            _toastTranslate.X = 0;
            
            // The completion of this storyboard indicates a toast has been dismissed by the user
            OnToastDismissed();
        }

        public class ToastInfo
        {
            /// <summary>
            /// Title text that is displayed in bold. This is optional.
            /// </summary>
            public string Title { get; set; }
            
            /// <summary>
            /// Content text that is displayed under the title text.
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// Callback that gets called after a toast has been handled.
            /// </summary>
            public EventHandler<ToastHandledEventArgs> Callback { get; set; }

            /// <summary>
            /// Optional state object to pass to the callback.
            /// </summary>
            public object State { get; set; }

            /// <summary>
            /// Indicates whether this toast is an informational toast.
            /// Informational toasts are not actionable, and they do not take up as much space on the screen.
            /// They basically look like StatusBar text.
            /// </summary>
            public bool IsInformational { get; set; }

            internal ToastInfo(string text)
            {
                Text = text;
            }

            internal ToastInfo(string text, bool isInformational)
            {
                Text = text;
                IsInformational = isInformational;
            }

            internal ToastInfo(string text, string title)
            {
                Text = text;
                Title = title;
            }

            internal ToastInfo(string text, string title, EventHandler<ToastHandledEventArgs> callback, object state)
            {
                Text = text;
                Title = title;
                Callback = callback;
                State = state;
            }
        }
    }
}

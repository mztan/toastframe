namespace Em.UI.Xaml
{
    /// <summary>
    /// Specifies the result of displaying a toast on the screen.
    /// </summary>
    public enum ToastResult
    {
        /// <summary>
        /// The toast was activated by the user (via tap).
        /// </summary>
        Activated,

        /// <summary>
        /// The toast was dismissed by the user (via swipe).
        /// </summary>
        Dismissed,

        /// <summary>
        /// The toast expired via timeout (user ignored the toast).
        /// </summary>
        TimedOut
    }
}

using System;

namespace Em.UI.Xaml
{
    /// <summary>
    /// Provides data for the ToastFrame.ShowToast callback.
    /// </summary>
    public class ToastHandledEventArgs : EventArgs
    {
        /// <summary>
        /// The result of displaying the toast.
        /// </summary>
        public ToastResult Result { get; set; }

        /// <summary>
        /// Optional state object associated with this toast instance.
        /// </summary>
        public object State { get; set; }

        internal ToastHandledEventArgs(ToastResult result, object state)
        {
            Result = result;
            State = state;
        }
    }
}

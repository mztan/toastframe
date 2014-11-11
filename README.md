toastframe
==========

Frame control for Windows Phone 8.1 that encapsulates toast-like popups. It also includes status bar like functionality to replace the built-in StatusBar. 

Why?
--------
Because there is currently no simple way to display toasts within a Windows Phone app. In Windows Store apps, toast notifications are displayed even when the app is in the foreground; this is not the case for Windows Phone (as of WP8.1).

Why include status bar (progress bar and text) functionality?
--------
Because it is currently not possible to display UI on top of the system status bar. Without including this functionality, using ToastFrame side-by-side with the built-in StatusBar's ProgressIndicator would be possible but troublesome.

Features
--------
1. Two types of toasts: actionable and informational.
  * Actionable toasts can be tapped to be activated, or swiped to be dismissed. (They can also be hidden via timeout.)
  * Informational toasts can not be activated or dismissed, and they only occupy one line of text.
2. Actionable toasts can optionally contain title text which is displayed in bold.
3. Toasts are queued automatically. Its behavior is similar to Windows Phone's built-in toast notification popups.
4. Status bar contains a progress bar and one line of text, exactly the same as the OS's built-in status bar.
5. However, the status bar's progress bar's fill can be changed!!


Please take a look at the sample app to get a feel for what the toast notifications look like.
Comments and pull requests are welcome.

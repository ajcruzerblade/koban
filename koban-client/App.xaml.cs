using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Koban
{
    using ServiceHelpers;
    using System.Diagnostics;
    using Views;
    using Windows.Data.Xml.Dom;
    using Windows.UI.Notifications;
    sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
            }
#endif

            AppShell shell = Window.Current.Content as AppShell;

            if (shell == null)
            {
                SettingsHelper.Instance.SettingsChanged += (target, args) =>
                {
                    EmotionServiceHelper.ApiKey = SettingsHelper.Instance.EmotionApiKey;
                    FaceServiceHelper.ApiKey = SettingsHelper.Instance.FaceApiKey;
                    FaceServiceHelper.ApiKeyRegion = SettingsHelper.Instance.FaceApiKeyRegion;
                    VisionServiceHelper.ApiKey = SettingsHelper.Instance.VisionApiKey;
                    VisionServiceHelper.ApiKeyRegion = SettingsHelper.Instance.VisionApiKeyRegion;
                    BingSearchHelper.SearchApiKey = SettingsHelper.Instance.BingSearchApiKey;
                    BingSearchHelper.AutoSuggestionApiKey = SettingsHelper.Instance.BingAutoSuggestionApiKey;
                    TextAnalyticsHelper.ApiKey = SettingsHelper.Instance.TextAnalyticsKey;
                    TextAnalyticsHelper.ApiKeyRegion = SettingsHelper.Instance.TextAnalyticsApiKeyRegion;
                    TextAnalyticsHelper.ApiKey = SettingsHelper.Instance.TextAnalyticsKey;
                    ImageAnalyzer.PeopleGroupsUserDataFilter = SettingsHelper.Instance.WorkspaceKey;
                    FaceListManager.FaceListsUserDataFilter = SettingsHelper.Instance.WorkspaceKey;
                    CoreUtil.MinDetectableFaceCoveragePercentage = SettingsHelper.Instance.MinDetectableFaceCoveragePercentage;
                };

                FaceServiceHelper.Throttled = () => ShowThrottlingToast("Face");
                EmotionServiceHelper.Throttled = () => ShowThrottlingToast("Emotion");
                VisionServiceHelper.Throttled = () => ShowThrottlingToast("Vision");
                ErrorTrackingHelper.TrackException = (ex, msg) => LogException(ex, msg);
                ErrorTrackingHelper.GenericApiCallExceptionHandler = Util.GenericApiCallExceptionHandler;

                SettingsHelper.Instance.Initialize();

                shell = new AppShell();

                shell.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                shell.AppFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                }

                var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
                var titleBar = appView.TitleBar;
                titleBar.BackgroundColor = Windows.UI.Colors.Black;
                titleBar.ForegroundColor = Windows.UI.Colors.White;
                titleBar.ButtonBackgroundColor = Windows.UI.Colors.Black;
                titleBar.ButtonForegroundColor = Windows.UI.Colors.White;
            }

            Window.Current.Content = shell;

            if (shell.AppFrame.Content == null)
            {
                shell.AppFrame.Navigate(typeof(SettingsPage), e.Arguments, new Windows.UI.Xaml.Media.Animation.SuppressNavigationTransitionInfo());
            }

            Window.Current.Activate();
        }

        private static void ShowThrottlingToast(string api)
        {
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText02;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode("Koban"));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode("The " + api + " API is throttling your requests."));

            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        private static void LogException(Exception ex, string message)
        {
            Debug.WriteLine("Error detected! Exception: \"{0}\", More info: \"{1}\".", ex.Message, message);
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            deferral.Complete();
        }
    }
}

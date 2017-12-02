using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Koban.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            this.DataContext = SettingsHelper.Instance;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.cameraSourceComboBox.ItemsSource = await Util.GetAvailableCameraNamesAsync();
            this.cameraSourceComboBox.SelectedItem = SettingsHelper.Instance.CameraName;
            base.OnNavigatedFrom(e);
        }

        private void OnGenerateNewKeyClicked(object sender, RoutedEventArgs e)
        {
            SettingsHelper.Instance.WorkspaceKey = Guid.NewGuid().ToString();
        }

        private async void OnResetSettingsClick(object sender, RoutedEventArgs e)
        {
            await Util.ConfirmActionAndExecute("This will reset all the settings and erase your changes. Confirm?",
                async () =>
                {
                    await Task.Run(() => SettingsHelper.Instance.RestoreAllSettings());
                    await new MessageDialog("Settings restored. Please restart the application to load the default settings.").ShowAsync();
                });
        }

        private void OnCameraSourceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cameraSourceComboBox.SelectedItem != null)
            {
                SettingsHelper.Instance.CameraName = this.cameraSourceComboBox.SelectedItem.ToString();
            }
        }

        private void ResetMallKioskSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            SettingsHelper.Instance.RestoreMallKioskSettingsToDefaultFile();
        }
    }
}

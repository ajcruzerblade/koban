﻿using Koban.Models;
using Newtonsoft.Json.Linq;
using ServiceHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Koban.Views
{
    [KioskExperience(Title = "Koban Explorer", ImagePath = "ms-appx:/Assets/VisionAPI.jpg")]
    public sealed partial class KobanExplorer : Page
    {
        public KobanExplorer()
        {
            this.InitializeComponent();

            this.cameraControl.ImageCaptured += CameraControl_ImageCaptured;
            this.cameraControl.CameraRestarted += CameraControl_CameraRestarted;

            this.favoritePhotosGridView.ItemsSource = new string[] 
                {
                    "https://previews.123rf.com/images/ferreira669/ferreira6691003/ferreira669100300010/6679431-man-holding-gun-wearing-sunglasses-Stock-Photo.jpg",
                    "https://static4.depositphotos.com/1012193/385/i/950/depositphotos_3855432-stock-photo-man-with-gun.jpg",
                    "https://cdn.xl.thumbs.canstockphoto.com/business-man-holding-gun-shoot-stock-image_csp17753563.jpg",
                };
        }

        private async void CameraControl_CameraRestarted(object sender, EventArgs e)
        {
            await Task.Delay(500);

            this.imageFromCameraWithFaces.Visibility = Visibility.Collapsed;
            this.resultsDetails.Visibility = Visibility.Collapsed;
        }

        private void DisplayProcessingUI()
        {
            this.tagsGridView.ItemsSource = new[] { new { Name = "Analyzing..." } };
            this.descriptionGridView.ItemsSource = new[] { new { Description = "Analyzing..." } };
            this.celebritiesTextBlock.Text = "Analyzing...";
            this.colorInfoListView.ItemsSource = new[] { new { Description = "Analyzing..." } };

            this.ocrToggle.IsEnabled = false;
        }

        private async void UpdateResults(ImageAnalyzer img)
        {
            ProcessedData pd = new ProcessedData();
            pd.Location = "Area 1";

            if (img.AnalysisResult.Tags == null || !img.AnalysisResult.Tags.Any())
            {
                this.tagsGridView.ItemsSource = new[] { new { Name = "No tags" } };
            }
            else
            {
                this.tagsGridView.ItemsSource = img.AnalysisResult.Tags.Select(t => new { Confidence = string.Format("({0}%)", Math.Round(t.Confidence * 100)), Name = t.Name });
            }

            if (img.AnalysisResult.Description == null || !img.AnalysisResult.Description.Captions.Any(d => d.Confidence >= 0.2))
            {
                this.descriptionGridView.ItemsSource = new[] { new { Description = "Not sure what that is" } };
                pd.Report = "Not sure what that is";
            }
            else
            {
                this.descriptionGridView.ItemsSource = img.AnalysisResult.Description.Captions.Select(d => new { Confidence = string.Format("({0}%)", Math.Round(d.Confidence * 100)), Description = d.Text });
                pd.Report = img.AnalysisResult.Description.Captions[0].Text;

            }

            var celebNames = this.GetCelebrityNames(img);
            if (celebNames == null || !celebNames.Any())
            {
                this.celebritiesTextBlock.Text = "None";
            }
            else
            {
                this.celebritiesTextBlock.Text = string.Join(", ", celebNames.OrderBy(name => name));
            }

            if (img.AnalysisResult.Color == null)
            {
                this.colorInfoListView.ItemsSource = new[] { new { Description = "Not available" } };
            }
            else
            { 
                this.colorInfoListView.ItemsSource = new[]
                {
                    new { Description = "Dominant background color:", Colors = new string[] { img.AnalysisResult.Color.DominantColorBackground } },
                    new { Description = "Dominant foreground color:", Colors = new string[] { img.AnalysisResult.Color.DominantColorForeground } },
                    new { Description = "Dominant colors:", Colors = img.AnalysisResult.Color.DominantColors },
                    new { Description = "Accent color:", Colors = new string[] { "#" + img.AnalysisResult.Color.AccentColor } }
                };
            }

            this.ocrToggle.IsEnabled = true;

            if (img.ImageUrl != null)
            {
                // Download - Start
                HttpClient client = new HttpClient();
                byte[] buffer = await client.GetByteArrayAsync(img.ImageUrl);

                pd.Img = buffer;
                // Download - End
            }
            else if (img.Data != null)
            {
                pd.Img = img.Data;
            }

            HttpClient httpClient = new HttpClient();
            MultipartFormDataContent form = new MultipartFormDataContent();

            form.Add(new ByteArrayContent(pd.Img, 0, pd.Img.Length), "img", "image.jpg");
            form.Add(new StringContent(pd.Location), "location");
            form.Add(new StringContent(pd.Report), "report");
            HttpResponseMessage response = await httpClient.PostAsync("http://192.168.43.46:8000/api/crime/uploadCrime", form);
            //HttpResponseMessage response = await httpClient.PostAsync("https://requestb.in/100adnv1", form);

            response.EnsureSuccessStatusCode();
            httpClient.Dispose();
            string sd = response.Content.ReadAsStringAsync().Result;
        }

        private IEnumerable<String> GetCelebrityNames(ImageAnalyzer analyzer)
        {
            if (analyzer.AnalysisResult?.Categories != null)
            {
                foreach (var category in analyzer.AnalysisResult.Categories.Where(c => c.Detail != null))
                {
                    dynamic detail = JObject.Parse(category.Detail.ToString());
                    if (detail.celebrities != null)
                    {
                        foreach (var celebrity in detail.celebrities)
                        {

                            yield return celebrity.name.ToString();
                        }
                    }
                }
            }
        }

        private async void CameraControl_ImageCaptured(object sender, ImageAnalyzer e)
        {
            this.UpdateActivePhoto(e);

            this.imageFromCameraWithFaces.DataContext = e;
            this.imageFromCameraWithFaces.Visibility = Visibility.Visible;

            await this.cameraControl.StopStreamAsync();
        }

        private void UpdateActivePhoto(ImageAnalyzer img)
        {
            this.landingMessage.Visibility = Visibility.Collapsed;
            this.resultsDetails.Visibility = Visibility.Visible;

            if (img.AnalysisResult != null)
            {
                this.UpdateResults(img);
            }
            else
            {
                this.DisplayProcessingUI();
                img.ComputerVisionAnalysisCompleted += (s, args) =>
                {
                    this.UpdateResults(img);
                };
            }
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            await this.cameraControl.StopStreamAsync();
            base.OnNavigatingFrom(e);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (string.IsNullOrEmpty(SettingsHelper.Instance.VisionApiKey))
            {
                await new MessageDialog("Missing Computer Vision API Key. Please enter a key in the Settings page.", "Missing API Key").ShowAsync();
            }

            base.OnNavigatedTo(e);
        }

        private async void OnImageSearchCompleted(object sender, IEnumerable<ImageAnalyzer> args)
        {
            this.favoritePhotosGridView.SelectedItem = null;

            this.imageSearchFlyout.Hide();
            ImageAnalyzer image = args.First();
            image.ShowDialogOnFaceApiErrors = true;

            this.imageWithFacesControl.Visibility = Visibility.Visible;
            this.webCamHostGrid.Visibility = Visibility.Collapsed;
            await this.cameraControl.StopStreamAsync();

            this.UpdateActivePhoto(image);

            this.imageWithFacesControl.DataContext = image;
        }

        private void OnImageSearchCanceled(object sender, EventArgs e)
        {
            this.imageSearchFlyout.Hide();
        }

        private async void OnWebCamButtonClicked(object sender, RoutedEventArgs e)
        {
            await StartWebCameraAsync();
        }

        private async Task StartWebCameraAsync()
        {
            this.favoritePhotosGridView.SelectedItem = null;
            this.landingMessage.Visibility = Visibility.Collapsed;
            this.webCamHostGrid.Visibility = Visibility.Visible;
            this.imageWithFacesControl.Visibility = Visibility.Collapsed;
            this.resultsDetails.Visibility = Visibility.Collapsed;

            await this.cameraControl.StartStreamAsync();
            await Task.Delay(250);
            this.imageFromCameraWithFaces.Visibility = Visibility.Collapsed;

            UpdateWebCamHostGridSize();
        }

        private void OnPageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateWebCamHostGridSize();
        }

        private void UpdateWebCamHostGridSize()
        {
            this.webCamHostGrid.Height = this.webCamHostGrid.ActualWidth / (this.cameraControl.CameraAspectRatio != 0 ? this.cameraControl.CameraAspectRatio : 1.777777777777);
        }

        private async void OnFavoriteSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.favoriteImagePickerFlyout.Hide();

            if (!string.IsNullOrEmpty((string)this.favoritePhotosGridView.SelectedValue))
            {
                this.landingMessage.Visibility = Visibility.Collapsed;

                ImageAnalyzer image = new ImageAnalyzer((string)this.favoritePhotosGridView.SelectedValue);
                image.ShowDialogOnFaceApiErrors = true;

                this.imageWithFacesControl.Visibility = Visibility.Visible;
                this.webCamHostGrid.Visibility = Visibility.Collapsed;
                await this.cameraControl.StopStreamAsync();

                this.UpdateActivePhoto(image);

                this.imageWithFacesControl.DataContext = image;
            }
        }

        private void OnOCRToggled(object sender, RoutedEventArgs e)
        {
            var currentImageDisplay = this.imageWithFacesControl.Visibility == Visibility.Visible ? this.imageWithFacesControl : this.imageFromCameraWithFaces;
            if (currentImageDisplay.DataContext != null)
            {
                var img = currentImageDisplay.DataContext;
                currentImageDisplay.DataContext = null;
                currentImageDisplay.DataContext = img;
            }
        }
    }
}

using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Face.Contract;
using ServiceHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.FaceAnalysis;
using Windows.Media.MediaProperties;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Koban.Controls
{
    public enum AutoCaptureState
    {
        WaitingForFaces,
        WaitingForStillFaces,
        ShowingCountdownForCapture,
        ShowingCapturedPhoto
    }

    public interface IRealTimeDataProvider
    {
        EmotionScores GetLastEmotionForFace(BitmapBounds faceBox);
        Face GetLastFaceAttributesForFace(BitmapBounds faceBox);
        IdentifiedPerson GetLastIdentifiedPersonForFace(BitmapBounds faceBox);
        SimilarPersistedFace GetLastSimilarPersistedFaceForFace(BitmapBounds faceBox);
    }

    public sealed partial class CameraControl : UserControl
    {
        public event EventHandler<ImageAnalyzer> ImageCaptured;
        public event EventHandler<AutoCaptureState> AutoCaptureStateChanged;
        public event EventHandler CameraRestarted;
        public event EventHandler CameraAspectRatioChanged;

        public static readonly DependencyProperty ShowDialogOnApiErrorsProperty =
            DependencyProperty.Register(
            "ShowDialogOnApiErrors",
            typeof(bool),
            typeof(CameraControl),
            new PropertyMetadata(true)
            );

        public bool ShowDialogOnApiErrors
        {
            get { return (bool)GetValue(ShowDialogOnApiErrorsProperty); }
            set { SetValue(ShowDialogOnApiErrorsProperty, (bool)value); }
        }

        public bool FilterOutSmallFaces
        {
            get;
            set;
        }

        private bool enableAutoCaptureMode;
        public bool EnableAutoCaptureMode
        {
            get
            {
                return enableAutoCaptureMode;
            }
            set
            {
                this.enableAutoCaptureMode = value;
                this.commandBar.Visibility = this.enableAutoCaptureMode ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public double CameraAspectRatio { get; set; }
        public int CameraResolutionWidth { get; private set; }
        public int CameraResolutionHeight { get; private set; }

        public int NumFacesOnLastFrame { get; set; }

        public CameraStreamState CameraStreamState { get { return captureManager != null ? captureManager.CameraStreamState : CameraStreamState.NotStreaming; } }

        private MediaCapture captureManager;
        private VideoEncodingProperties videoProperties;
        private FaceTracker faceTracker;
        private ThreadPoolTimer frameProcessingTimer;
        private SemaphoreSlim frameProcessingSemaphore = new SemaphoreSlim(1);
        private AutoCaptureState autoCaptureState;
        private IEnumerable<DetectedFace> detectedFacesFromPreviousFrame;
        private DateTime timeSinceWaitingForStill;
        private DateTime lastTimeWhenAFaceWasDetected;

        private IRealTimeDataProvider realTimeDataProvider;

        public CameraControl()
        {
            this.InitializeComponent();
        }

        #region Camera stream processing

        public async Task StartStreamAsync(bool isForRealTimeProcessing = false)
        {
            try
            {
                if (captureManager == null ||
                    captureManager.CameraStreamState == CameraStreamState.Shutdown ||
                    captureManager.CameraStreamState == CameraStreamState.NotStreaming)
                {
                    if (captureManager != null)
                    {
                        captureManager.Dispose();
                    }

                    captureManager = new MediaCapture();

                    MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings();
                    var allCameras = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                    var selectedCamera = allCameras.FirstOrDefault(c => c.Name == SettingsHelper.Instance.CameraName);
                    if (selectedCamera != null)
                    {
                        settings.VideoDeviceId = selectedCamera.Id;
                    }

                    await captureManager.InitializeAsync(settings);
                    await SetVideoEncodingToHighestResolution(isForRealTimeProcessing);

                    this.webCamCaptureElement.Source = captureManager;
                }

                if (captureManager.CameraStreamState == CameraStreamState.NotStreaming)
                {
                    if (this.faceTracker == null)
                    {
                        this.faceTracker = await FaceTracker.CreateAsync();
                    }

                    this.videoProperties = this.captureManager.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;

                    await captureManager.StartPreviewAsync();

                    if (this.frameProcessingTimer != null)
                    {
                        this.frameProcessingTimer.Cancel();
                        frameProcessingSemaphore.Release();
                    }
                    TimeSpan timerInterval = TimeSpan.FromMilliseconds(66); 
                    this.frameProcessingTimer = ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(ProcessCurrentVideoFrame), timerInterval);

                    this.cameraControlSymbol.Symbol = Symbol.Camera;
                    this.webCamCaptureElement.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                await Util.GenericApiCallExceptionHandler(ex, "Error starting the camera.");
            }
        }

        private async Task SetVideoEncodingToHighestResolution(bool isForRealTimeProcessing = false)
        {
            VideoEncodingProperties highestVideoEncodingSetting;

            var availableResolutions = this.captureManager.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview).Cast<VideoEncodingProperties>().OrderByDescending(v => v.Width * v.Height * (v.FrameRate.Numerator / v.FrameRate.Denominator));

            if (isForRealTimeProcessing)
            {
                uint maxHeightForRealTime = 720;
                highestVideoEncodingSetting = availableResolutions.FirstOrDefault(v => v.Height <= maxHeightForRealTime);
                if (highestVideoEncodingSetting == null)
                {
                    highestVideoEncodingSetting = availableResolutions.LastOrDefault();
                }
            }
            else
            {
                highestVideoEncodingSetting = availableResolutions.FirstOrDefault();
            }

            if (highestVideoEncodingSetting != null)
            {
                this.CameraAspectRatio = (double)highestVideoEncodingSetting.Width / (double)highestVideoEncodingSetting.Height;
                this.CameraResolutionHeight = (int)highestVideoEncodingSetting.Height;
                this.CameraResolutionWidth = (int)highestVideoEncodingSetting.Width;

                await this.captureManager.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, highestVideoEncodingSetting);

                if (this.CameraAspectRatioChanged != null)
                {
                    this.CameraAspectRatioChanged(this, EventArgs.Empty);
                }
            }
        }

        private async void ProcessCurrentVideoFrame(ThreadPoolTimer timer)
        {
            if (captureManager.CameraStreamState != Windows.Media.Devices.CameraStreamState.Streaming
                || !frameProcessingSemaphore.Wait(0))
            {
                return;
            }

            try
            {
                IEnumerable<DetectedFace> faces = null;

                const BitmapPixelFormat InputPixelFormat = BitmapPixelFormat.Nv12;
                using (VideoFrame previewFrame = new VideoFrame(InputPixelFormat, (int)this.videoProperties.Width, (int)this.videoProperties.Height))
                {
                    await this.captureManager.GetPreviewFrameAsync(previewFrame);

                    if (FaceDetector.IsBitmapPixelFormatSupported(previewFrame.SoftwareBitmap.BitmapPixelFormat))
                    {
                        faces = await this.faceTracker.ProcessNextFrameAsync(previewFrame);

                        if (this.FilterOutSmallFaces)
                        {
                            faces = faces.Where(f => CoreUtil.IsFaceBigEnoughForDetection((int)f.FaceBox.Height, (int)this.videoProperties.Height));
                        }

                        this.NumFacesOnLastFrame = faces.Count();

                        if (this.EnableAutoCaptureMode)
                        {
                            this.UpdateAutoCaptureState(faces);
                        }

                        var previewFrameSize = new Windows.Foundation.Size(previewFrame.SoftwareBitmap.PixelWidth, previewFrame.SoftwareBitmap.PixelHeight);
                        var ignored = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            this.ShowFaceTrackingVisualization(previewFrameSize, faces);
                        });
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                frameProcessingSemaphore.Release();
            }
        }

        private void ShowFaceTrackingVisualization(Windows.Foundation.Size framePixelSize, IEnumerable<DetectedFace> detectedFaces)
        {
            this.FaceTrackingVisualizationCanvas.Children.Clear();

            double actualWidth = this.FaceTrackingVisualizationCanvas.ActualWidth;
            double actualHeight = this.FaceTrackingVisualizationCanvas.ActualHeight;

            if (captureManager.CameraStreamState == Windows.Media.Devices.CameraStreamState.Streaming &&
                detectedFaces != null && actualWidth != 0 && actualHeight != 0)
            {
                double widthScale = framePixelSize.Width / actualWidth;
                double heightScale = framePixelSize.Height / actualHeight;

                foreach (DetectedFace face in detectedFaces)
                {
                    RealTimeFaceIdentificationBorder faceBorder = new RealTimeFaceIdentificationBorder();
                    this.FaceTrackingVisualizationCanvas.Children.Add(faceBorder);

                    faceBorder.ShowFaceRectangle((uint)(face.FaceBox.X / widthScale), (uint)(face.FaceBox.Y / heightScale), (uint)(face.FaceBox.Width / widthScale), (uint)(face.FaceBox.Height / heightScale));

                    if (this.realTimeDataProvider != null)
                    {
                        EmotionScores lastEmotion = this.realTimeDataProvider.GetLastEmotionForFace(face.FaceBox);
                        if (lastEmotion != null)
                        {
                            faceBorder.ShowRealTimeEmotionData(lastEmotion);
                        }

                        Face detectedFace = this.realTimeDataProvider.GetLastFaceAttributesForFace(face.FaceBox);
                        IdentifiedPerson identifiedPerson = this.realTimeDataProvider.GetLastIdentifiedPersonForFace(face.FaceBox);
                        SimilarPersistedFace similarPersistedFace = this.realTimeDataProvider.GetLastSimilarPersistedFaceForFace(face.FaceBox);

                        string uniqueId = null;
                        if (similarPersistedFace != null)
                        {
                            uniqueId = similarPersistedFace.PersistedFaceId.ToString("N").Substring(0, 4);
                        }

                        if (detectedFace != null && detectedFace.FaceAttributes != null)
                        {
                            if (identifiedPerson != null && identifiedPerson.Person != null)
                            {
                                faceBorder.ShowIdentificationData(detectedFace.FaceAttributes.Age, detectedFace.FaceAttributes.Gender, (uint)Math.Round(identifiedPerson.Confidence * 100), identifiedPerson.Person.Name, uniqueId: uniqueId);
                            }
                            else
                            {
                                faceBorder.ShowIdentificationData(detectedFace.FaceAttributes.Age, detectedFace.FaceAttributes.Gender, 0, null, uniqueId: uniqueId);
                            }
                        }
                        else if (identifiedPerson != null && identifiedPerson.Person != null)
                        {
                            faceBorder.ShowIdentificationData(0, null, (uint)Math.Round(identifiedPerson.Confidence * 100), identifiedPerson.Person.Name, uniqueId: uniqueId);
                        }
                        else if (uniqueId != null)
                        {
                            faceBorder.ShowIdentificationData(0, null, 0, null, uniqueId: uniqueId);
                        }
                    }

                    if (SettingsHelper.Instance.ShowDebugInfo)
                    {
                        this.FaceTrackingVisualizationCanvas.Children.Add(new TextBlock
                        {
                            Text = string.Format("Coverage: {0:0}%", 100 * ((double)face.FaceBox.Height / this.videoProperties.Height)),
                            Margin = new Thickness((uint)(face.FaceBox.X / widthScale), (uint)(face.FaceBox.Y / heightScale), 0, 0)
                        });
                    }
                }
            }
        }

        private async void UpdateAutoCaptureState(IEnumerable<DetectedFace> detectedFaces)
        {
            const int IntervalBeforeCheckingForStill = 500;
            const int IntervalWithoutFacesBeforeRevertingToWaitingForFaces = 3;

            if (!detectedFaces.Any())
            {
                if (this.autoCaptureState == AutoCaptureState.WaitingForStillFaces &&
                    (DateTime.Now - this.lastTimeWhenAFaceWasDetected).TotalSeconds > IntervalWithoutFacesBeforeRevertingToWaitingForFaces)
                {
                    this.autoCaptureState = AutoCaptureState.WaitingForFaces;
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        this.OnAutoCaptureStateChanged(this.autoCaptureState);
                    });
                }

                return;
            }

            this.lastTimeWhenAFaceWasDetected = DateTime.Now;

            switch (this.autoCaptureState)
            {
                case AutoCaptureState.WaitingForFaces:
                    this.detectedFacesFromPreviousFrame = detectedFaces;
                    this.timeSinceWaitingForStill = DateTime.Now;
                    this.autoCaptureState = AutoCaptureState.WaitingForStillFaces;

                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        this.OnAutoCaptureStateChanged(this.autoCaptureState);
                    });

                    break;

                case AutoCaptureState.WaitingForStillFaces:
                    if ((DateTime.Now - this.timeSinceWaitingForStill).TotalMilliseconds >= IntervalBeforeCheckingForStill)
                    {
                        if (this.AreFacesStill(this.detectedFacesFromPreviousFrame, detectedFaces))
                        {
                            this.autoCaptureState = AutoCaptureState.ShowingCountdownForCapture;
                            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                this.OnAutoCaptureStateChanged(this.autoCaptureState);
                            });
                        }
                        else
                        {
                            this.timeSinceWaitingForStill = DateTime.Now;
                            this.detectedFacesFromPreviousFrame = detectedFaces;
                        }
                    }
                    break;

                case AutoCaptureState.ShowingCountdownForCapture:
                    break;

                case AutoCaptureState.ShowingCapturedPhoto:
                    break;

                default:
                    break;
            }
        }

        public async Task<ImageAnalyzer> TakeAutoCapturePhoto()
        {
            var image = await CaptureFrameAsync();
            this.autoCaptureState = AutoCaptureState.ShowingCapturedPhoto;
            this.OnAutoCaptureStateChanged(this.autoCaptureState);
            return image;
        }

        public void RestartAutoCaptureCycle()
        {
            this.autoCaptureState = AutoCaptureState.WaitingForFaces;
            this.OnAutoCaptureStateChanged(this.autoCaptureState);
        }

        private bool AreFacesStill(IEnumerable<DetectedFace> detectedFacesFromPreviousFrame, IEnumerable<DetectedFace> detectedFacesFromCurrentFrame)
        {
            int horizontalMovementThreshold = (int)(videoProperties.Width * 0.02);
            int verticalMovementThreshold = (int)(videoProperties.Height * 0.02);

            int numStillFaces = 0;
            int totalFacesInPreviousFrame = detectedFacesFromPreviousFrame.Count();

            foreach (DetectedFace faceInPreviousFrame in detectedFacesFromPreviousFrame)
            {
                if (numStillFaces > 0 && numStillFaces >= totalFacesInPreviousFrame / 2)
                {
                    break;
                }

                if (detectedFacesFromCurrentFrame.Any(f => Math.Abs((int)faceInPreviousFrame.FaceBox.X - (int)f.FaceBox.X) <= horizontalMovementThreshold &&
                                                           Math.Abs((int)faceInPreviousFrame.FaceBox.Y - (int)f.FaceBox.Y) <= verticalMovementThreshold))
                {
                    numStillFaces++;
                }
            }

            if (numStillFaces > 0 && numStillFaces >= totalFacesInPreviousFrame / 2)
            {
                return true;
            }

            return false;
        }

        public async Task StopStreamAsync()
        {
            try
            {
                if (this.frameProcessingTimer != null)
                {
                    this.frameProcessingTimer.Cancel();
                }

                if (captureManager != null && captureManager.CameraStreamState != Windows.Media.Devices.CameraStreamState.Shutdown)
                {
                    this.FaceTrackingVisualizationCanvas.Children.Clear();
                    await this.captureManager.StopPreviewAsync();

                    this.FaceTrackingVisualizationCanvas.Children.Clear();
                    this.webCamCaptureElement.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
            }
        }

        public async Task<ImageAnalyzer> CaptureFrameAsync()
        {
            try
            {
                if (!(await this.frameProcessingSemaphore.WaitAsync(250)))
                {
                    return null;
                }

                var videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, CameraResolutionWidth, CameraResolutionHeight);
                using (var currentFrame = await captureManager.GetPreviewFrameAsync(videoFrame))
                {
                    using (SoftwareBitmap previewFrame = currentFrame.SoftwareBitmap)
                    {
                        ImageAnalyzer imageWithFace = new ImageAnalyzer(await Util.GetPixelBytesFromSoftwareBitmapAsync(previewFrame));

                        imageWithFace.ShowDialogOnFaceApiErrors = this.ShowDialogOnApiErrors;
                        imageWithFace.FilterOutSmallFaces = this.FilterOutSmallFaces;
                        imageWithFace.UpdateDecodedImageSize(this.CameraResolutionHeight, this.CameraResolutionWidth);

                        return imageWithFace;
                    }
                }
            }
            catch (Exception ex)
            {
                if (this.ShowDialogOnApiErrors)
                {
                    await Util.GenericApiCallExceptionHandler(ex, "Error capturing photo.");
                }
            }
            finally
            {
                this.frameProcessingSemaphore.Release();
            }

            return null;
        }

        private void OnImageCaptured(ImageAnalyzer imageWithFace)
        {
            if (this.ImageCaptured != null)
            {
                this.ImageCaptured(this, imageWithFace);
            }
        }

        private void OnAutoCaptureStateChanged(AutoCaptureState state)
        {
            if (this.AutoCaptureStateChanged != null)
            {
                this.AutoCaptureStateChanged(this, state);
            }
        }

        #endregion

        public void HideCameraControls()
        {
            this.commandBar.Visibility = Visibility.Collapsed;
        }

        public void SetRealTimeDataProvider(IRealTimeDataProvider provider)
        {
            this.realTimeDataProvider = provider;
        }

        private async void CameraControlButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.cameraControlSymbol.Symbol == Symbol.Camera)
            {
                var img = await CaptureFrameAsync();
                if (img != null)
                {
                    this.cameraControlSymbol.Symbol = Symbol.Refresh;
                    this.OnImageCaptured(img);
                }
            }
            else
            {
                this.cameraControlSymbol.Symbol = Symbol.Camera;

                await StartStreamAsync();

                this.CameraRestarted?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}

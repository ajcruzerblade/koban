using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Koban.Controls
{
    public sealed partial class OCRBorder : UserControl
    {
        public OCRBorder()
        {
            this.InitializeComponent();
        }

        public void SetData(double width, double height, string text)
        {
            this.borderRectangle.Width = width;
            this.borderRectangle.Height = height;
            this.captionText.Text = text;
        }

        private void OnCaptionSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.captionCanvas.Margin = new Thickness(this.borderRectangle.Margin.Left - (this.captionCanvas.ActualWidth - this.borderRectangle.ActualWidth) / 2,
                                                      -this.captionCanvas.ActualHeight, 0, 0);

        }
    }
}
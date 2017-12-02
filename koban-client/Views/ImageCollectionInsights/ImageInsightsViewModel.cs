using Windows.UI.Xaml.Media;

namespace Koban.Views.ImageCollectionInsights
{
    public class ImageInsightsViewModel
    {
        public ImageInsights Insights { get; set; }
        public ImageSource ImageSource { get; set; }

        public ImageInsightsViewModel(ImageInsights insights, ImageSource imageSource)
        {
            this.Insights = insights;
            this.ImageSource = imageSource;
        }
    }
}

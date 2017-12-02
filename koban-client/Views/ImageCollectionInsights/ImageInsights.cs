namespace Koban.Views.ImageCollectionInsights
{
    public class ImageInsights
    {
        public string ImageId { get; set; }
        public FaceInsights[] FaceInsights { get; set; }
        public VisionInsights VisionInsights { get; set; }
    }
}

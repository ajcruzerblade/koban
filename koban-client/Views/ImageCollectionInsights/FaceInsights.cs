using Microsoft.ProjectOxford.Face.Contract;
using System;

namespace Koban.Views.ImageCollectionInsights
{
    public class FaceInsights
    {
        public Guid UniqueFaceId { get; set; }
        public FaceRectangle FaceRectangle { get; set; }
        public string TopEmotion { get; set; }
        public string Gender { get; set; }
        public double Age { get; set; }
    }
}

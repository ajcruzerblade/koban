using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Vision;
using ServiceHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Koban.Views.ImageCollectionInsights
{
    public class ImageProcessor
    {
        private static VisualFeature[] DefaultVisualFeatureTypes = new VisualFeature[] { VisualFeature.Tags, VisualFeature.Description };

        public static async Task<ImageInsights> ProcessImageAsync(Func<Task<Stream>> imageStream, string imageId)
        {
            ImageAnalyzer analyzer = new ImageAnalyzer(imageStream);
            analyzer.ShowDialogOnFaceApiErrors = true;

            await Task.WhenAll(analyzer.AnalyzeImageAsync(detectCelebrities: false, visualFeatures: DefaultVisualFeatureTypes), analyzer.DetectFacesAsync(detectFaceAttributes: true), analyzer.DetectEmotionAsync());

            await analyzer.FindSimilarPersistedFacesAsync();

            ImageInsights result = new ImageInsights { ImageId = imageId };

            result.VisionInsights = new VisionInsights
            {
                Caption = analyzer.AnalysisResult.Description?.Captions[0].Text,
                Tags = analyzer.AnalysisResult.Tags != null ? analyzer.AnalysisResult.Tags.Select(t => t.Name).ToArray() : new string[0]
            };

            List<FaceInsights> faceInsightsList = new List<FaceInsights>();
            foreach (var face in analyzer.DetectedFaces)
            {
                FaceInsights faceInsights = new FaceInsights
                {
                    FaceRectangle = face.FaceRectangle,
                    Age = face.FaceAttributes.Age,
                    Gender = face.FaceAttributes.Gender
                };

                SimilarFaceMatch similarFaceMatch = analyzer.SimilarFaceMatches.FirstOrDefault(s => s.Face.FaceId == face.FaceId);
                if (similarFaceMatch != null)
                {
                    faceInsights.UniqueFaceId = similarFaceMatch.SimilarPersistedFace.PersistedFaceId;
                }

                Emotion faceEmotion = CoreUtil.FindFaceClosestToRegion(analyzer.DetectedEmotion, face.FaceRectangle);
                if (faceEmotion != null)
                {
                    faceInsights.TopEmotion = faceEmotion.Scores.ToRankedList().First().Key;
                }

                faceInsightsList.Add(faceInsights);
            }

            result.FaceInsights = faceInsightsList.ToArray();

            return result;
        }
    }
}

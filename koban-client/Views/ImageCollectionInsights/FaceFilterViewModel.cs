using System;
using System.ComponentModel;
using Windows.UI.Xaml.Media;

namespace Koban.Views.ImageCollectionInsights
{
    public class FaceFilterViewModel : INotifyPropertyChanged
    {
        public bool IsChecked { get; set; }
        public Guid FaceId { get; set; }
        public ImageSource ImageSource { get; set; }

        private int count;
        public int Count
        {
            get { return this.count; }
            set
            {
                this.count = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public FaceFilterViewModel(Guid faceId, ImageSource croppedFace)
        {
            this.FaceId = faceId;
            this.ImageSource = croppedFace;
        }
    }
}

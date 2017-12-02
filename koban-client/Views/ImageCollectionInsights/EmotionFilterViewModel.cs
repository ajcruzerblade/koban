using System.ComponentModel;

namespace Koban.Views.ImageCollectionInsights
{
    public class EmotionFilterViewModel : INotifyPropertyChanged
    {
        public bool IsChecked { get; set; }
        public string Emotion { get; set; }

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

        public EmotionFilterViewModel(string emotion)
        {
            this.Emotion = emotion;
        }
    }
}

using System.ComponentModel;

namespace Koban.Views.ImageCollectionInsights
{
    public class TagFilterViewModel : INotifyPropertyChanged
    {
        public bool IsChecked { get; set; }
        public string Tag { get; set; }

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

        public TagFilterViewModel(string tag)
        {
            this.Tag = tag;
        }
    }
}

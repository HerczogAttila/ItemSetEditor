using System.ComponentModel;

namespace ItemSetEditor
{
    public class Map : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; set; }
        public int ID { get; set; }
        public bool? IsChecked { get; set; }

        public Map()
        {
            Name = "";
            ID = 0;
        }

        public void OnChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

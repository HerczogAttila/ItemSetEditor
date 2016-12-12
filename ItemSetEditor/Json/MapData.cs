using Newtonsoft.Json;
using System.ComponentModel;

namespace ItemSetEditor
{
    public class MapData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public DDImage Image { get; set; }
        public string MapName { get; set; }
        public int MapId { get; set; }

        [JsonIgnore]
        public bool? IsChecked { get; set; }

        public MapData()
        {
            Image = new DDImage();
            MapName = "";
            MapId = 0;
            IsChecked = false;
        }

        public void OnChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

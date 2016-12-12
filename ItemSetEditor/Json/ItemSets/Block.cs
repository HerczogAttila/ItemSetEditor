using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ItemSetEditor
{
    public class Block : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Item> Items { get; private set; }
        [JsonProperty("type")]
        public string BlockType { get; set; }

        public Block()
        {
            Items = new ObservableCollection<Item>();
            BlockType = "";
        }

        public void OnChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
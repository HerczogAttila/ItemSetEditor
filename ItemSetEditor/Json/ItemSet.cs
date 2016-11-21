using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ItemSetEditor
{
    public class ItemSet : INotifyPropertyChanged
    {
        [JsonIgnore]
        public static Collection<Map> MapIds { get; set; } = new Collection<Map>();

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Block> blocks { get; set; }
        public string mode { get; set; }
        public string type { get; set; }
        public string uid { get; set; }
        public ObservableCollection<int> associatedMaps { get; private set; }
        public int sortrank { get; set; }
        public ObservableCollection<int> associatedChampions { get; private set; }
        public bool priority { get; set; }
        public bool isGlobalForMaps { get; set; }
        public bool isGlobalForChampions { get; set; }
        public string title { get; set; }
        public string map { get; set; }

        public ItemSet()
        {
            blocks = new ObservableCollection<Block>();
            mode = "any";
            type = "custom";
            uid = CodeGenerator.GenerateUID();
            associatedMaps = new ObservableCollection<int>();
            sortrank = 0;
            associatedChampions = new ObservableCollection<int>();
            priority = false;
            isGlobalForMaps = true;
            isGlobalForChampions = false;
            title = "";
            map = "any";
        }

        public void OnChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
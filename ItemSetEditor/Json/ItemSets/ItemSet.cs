using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ItemSetEditor
{
    public class ItemSet : INotifyPropertyChanged
    {
        [JsonIgnore]
        public static ObservableCollection<MapData> MapIds { get; set; } = new ObservableCollection<MapData>();

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonIgnore]
        public ObservableCollection<ChampionData> Champions { get; private set; }

        public ObservableCollection<Block> blocks { get; set; }
        public ObservableCollection<int> associatedMaps { get; private set; }
        public ObservableCollection<int> associatedChampions { get; private set; }
        public string mode { get; set; }
        public string type { get; set; }
        public string uid { get; set; }
        public string title { get; set; }
        public string map { get; set; }
        public int sortrank { get; set; }
        public bool priority { get; set; }
        public bool isGlobalForMaps { get; set; }
        public bool isGlobalForChampions { get; set; }

        public ItemSet()
        {
            Champions = new ObservableCollection<ChampionData>();

            blocks = new ObservableCollection<Block>();
            associatedMaps = new ObservableCollection<int>();
            associatedChampions = new ObservableCollection<int>();
            mode = "any";
            type = "custom";
            uid = CodeGenerator.GenerateUID();
            title = "";
            map = "any";
            sortrank = 0;
            priority = false;
            isGlobalForMaps = true;
            isGlobalForChampions = false;
        }

        public void OnChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void Deserialized()
        {
            ChampionData champion;
            foreach(int i in associatedChampions)
            {
                champion = MainWindow.Champions.data.Values.FirstOrDefault(s => s.key == i);
                if (champion != null)
                    Champions.Add(champion);
            }
        }
    }
}
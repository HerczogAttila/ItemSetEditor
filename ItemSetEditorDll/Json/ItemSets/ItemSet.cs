using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ItemSetEditor
{
    public class ItemSet : INotifyPropertyChanged
    {
        [JsonIgnore]
        public static ObservableCollection<MapData> MapIds { get; private set; } = new ObservableCollection<MapData>();

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonIgnore]
        public ObservableCollection<ChampionData> Champions { get; private set; }

        public ObservableCollection<Block> Blocks { get; private set; }
        public ObservableCollection<int> AssociatedMaps { get; private set; }
        public ObservableCollection<int> AssociatedChampions { get; private set; }
        public string Mode { get; set; }
        [JsonProperty("type")]
        public string ItemType { get; set; }
        [JsonProperty("uid")]
        public string Id { get; set; }
        public string Title { get; set; }
        public string Map { get; set; }
        public int SortRank { get; set; }
        public bool Priority { get; set; }
        public bool IsGlobalForMaps { get; set; }
        public bool IsGlobalForChampions { get; set; }

        public ItemSet()
        {
            Champions = new ObservableCollection<ChampionData>();

            Blocks = new ObservableCollection<Block>();
            AssociatedMaps = new ObservableCollection<int>();
            AssociatedChampions = new ObservableCollection<int>();
            Mode = "any";
            ItemType = "custom";
            Id = "";
            Title = "";
            Map = "any";
            SortRank = 0;
            Priority = false;
            IsGlobalForMaps = true;
            IsGlobalForChampions = false;
        }

        public void OnChanged(string name)
        {
#if DEBUG
            Log.Info("ItemSet OnChanged: " + name);
#endif

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void Deserialized(Dictionary<string, ChampionData> data)
        {
            if (data == null)
            {
#if DEBUG
                Log.Warning("ItemSet deserialized failed. Data is null.");
#endif

                return;
            }

#if DEBUG
            Log.Info("ItemSet deserialize: " + Title);
#endif

            ChampionData champion;
            foreach(int i in AssociatedChampions)
            {
                champion = data.Values.FirstOrDefault(s => s.Key == i);
                if (champion != null)
                {
#if DEBUG
                    Log.Info("Add associated champion to item: " + champion.Name);
#endif

                    Champions.Add(champion);
                }
#if DEBUG
                else
                    Log.Warning("Missing champion id: " + i);
#endif
            }
        }
    }
}
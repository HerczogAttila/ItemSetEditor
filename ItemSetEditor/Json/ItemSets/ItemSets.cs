using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace ItemSetEditor
{
    public class ItemSets
    {
        [JsonProperty("itemSets")]
        public ObservableCollection<ItemSet> Sets { get; private set; }
        public long Timestamp { get; set; }

        public ItemSets()
        {
            Sets = new ObservableCollection<ItemSet>();
            Timestamp = 0;
        }
    }
}

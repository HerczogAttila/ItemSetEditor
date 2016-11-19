using System.Collections.ObjectModel;

namespace ItemSetEditor
{
    public class ItemSets
    {
        public ObservableCollection<ItemSet> itemSets { get; private set; }
        public long timeStamp { get; set; }

        public ItemSets()
        {
            itemSets = new ObservableCollection<ItemSet>();
            timeStamp = 0;
        }
    }
}

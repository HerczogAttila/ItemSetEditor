using System.Collections.ObjectModel;

namespace ItemSetEditor
{
    public class ItemSets
    {
        public Collection<ItemSet> itemSets { get; private set; }
        public long timeStamp { get; set; }

        public ItemSets()
        {
            itemSets = new Collection<ItemSet>();
            timeStamp = 0;
        }
    }
}

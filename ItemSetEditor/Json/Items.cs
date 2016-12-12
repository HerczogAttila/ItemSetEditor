using System.Collections.Generic;
using System.Linq;

namespace ItemSetEditor
{
    public class Items
    {
        public Dictionary<string, ItemData> Data { get; private set; }

        public Items()
        {
            Data = new Dictionary<string, ItemData>();
        }

        public void Deserialized()
        {
            foreach (var v in Data.Where(s => !s.Value.Gold.Purchasable).ToArray())
                Data.Remove(v.Key);

            foreach (int v in MainWindow.Config.IgnoredItemIds)
                Data.Remove(v + "");

            foreach (KeyValuePair<string, ItemData> s in Data)
                s.Value.Id = s.Key;
        }
    }
}

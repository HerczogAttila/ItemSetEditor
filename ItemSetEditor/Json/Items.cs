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

        public void Deserialized(Config config)
        {
            if (config == null)
                return;

            foreach (var v in Data.Where(s => !s.Value.Gold.Purchasable).ToArray())
                Data.Remove(v.Key);

            foreach (int v in config.IgnoredItemIds)
                Data.Remove(v + "");

            foreach (KeyValuePair<string, ItemData> s in Data)
                s.Value.Id = s.Key;
        }
    }
}

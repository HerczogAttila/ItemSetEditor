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
            {
#if DEBUG
                Log.Warning("Items deserialized failed. config is null.");
#endif

                return;
            }

#if DEBUG
            Log.Info("Items deserialize.");
#endif

            foreach (var v in Data.Where(s => !s.Value.Gold.Purchasable).ToArray())
            {
#if DEBUG
                Log.Info("Removed item: " + v.Value.Name);
#endif

                Data.Remove(v.Key);
            }

            foreach (var v in config.IgnoredItemIds)
            {
#if DEBUG
                Log.Info("Removed item id: " + v);
#endif

                Data.Remove(v + "");
            }

            foreach (var s in Data)
                s.Value.Id = s.Key;
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace ItemSetEditor
{
    public class ItemDto
    {
        public Dictionary<string, ItemData> data { get; private set; }

        public ItemDto()
        {
            data = new Dictionary<string, ItemData>();
        }

        public void Deserialized()
        {
            foreach (var v in data.Where(s => !s.Value.gold.purchasable).ToArray())
                data.Remove(v.Key);

            foreach (int v in MainWindow.Config.IgnoredItemIds)
                data.Remove(v + "");

            foreach (KeyValuePair<string, ItemData> s in data)
                s.Value.id = s.Key;
        }
    }
}

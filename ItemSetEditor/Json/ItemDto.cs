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
            var keys = data.Where(s => !s.Value.gold.purchasable).Select(s => s.Key).ToArray();
            foreach (string s in keys)
                data.Remove(s);

            foreach (KeyValuePair<string, ItemData> s in data)
                s.Value.id = s.Key;
        }
    }
}

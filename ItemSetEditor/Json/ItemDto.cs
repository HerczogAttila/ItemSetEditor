using System.Collections.Generic;

namespace ItemSetEditor
{
    public class ItemDto
    {
        public Dictionary<string, ItemData> data { get; private set; }

        public ItemDto()
        {
            data = new Dictionary<string, ItemData>();
        }
    }
}

using System.Collections.ObjectModel;

namespace ItemSetEditor
{
    public class Block
    {
        public string type { get; set; }
        public Collection<Item> items { get; private set; }

        public Block()
        {
            type = "";
            items = new Collection<Item>();
        }
    }
}
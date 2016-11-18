using System.Collections.ObjectModel;

namespace ItemSetEditor
{
    public class ItemSet
    {
        public Collection<Block> blocks { get; set; }
        public string mode { get; set; }
        public string type { get; set; }
        public string uid { get; set; }
        public Collection<int> associatedMaps { get; private set; }
        public int sortrank { get; set; }
        public Collection<int> associatedChampions { get; private set; }
        public bool priority { get; set; }
        public bool isGlobalForMaps { get; set; }
        public bool isGlobalForChampions { get; set; }
        public string title { get; set; }
        public string map { get; set; }

        public ItemSet()
        {
            blocks = new Collection<Block>();
            mode = "any";
            type = "custom";
            uid = "";
            associatedMaps = new Collection<int>();
            sortrank = 0;
            associatedChampions = new Collection<int>();
            priority = false;
            isGlobalForMaps = false;
            isGlobalForChampions = false;
            title = "";
            map = "any";
        }
    }
}
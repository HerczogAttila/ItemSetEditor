using System.Collections.ObjectModel;

namespace ItemSetEditor
{
    public class ChampionData
    {
        public Collection<string> tags { get; private set; }
        public DDImage image { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public int key { get; set; }

        public ChampionData()
        {
            tags = new Collection<string>();
            image = new DDImage();
            name = "";
            title = "";
            key = 0;
        }
    }
}

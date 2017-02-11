using System.Collections.ObjectModel;

namespace ItemSetEditor
{
    public class ChampionData
    {
        public Collection<string> Tags { get; private set; }
        public DDImage Image { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int Key { get; set; }

        public ChampionData()
        {
            Tags = new Collection<string>();
            Image = new DDImage();
            Name = "";
            Title = "";
            Key = 0;
        }
    }
}

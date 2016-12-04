using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ItemSetEditor
{
    public class ItemData
    {
        public Dictionary<string, bool> maps { get; private set; }
        public Collection<string> Tags { get; private set; }
        public DDImage image { get; set; }
        public Gold gold { get; set; }
        public string requiredChampion { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string id { get; set; }
        public bool hideFromAll { get; set; }

        public ItemData()
        {
            maps = new Dictionary<string, bool>();
            Tags = new Collection<string>();
            image = new DDImage();
            gold = new Gold();
            requiredChampion = "";
            name = "";
            description = "";
            id = "0";
        }

        public void Deserialized()
        {
            foreach (var v in maps.Where(s => s.Value == false).ToArray())
                maps.Remove(v.Key);

            description = name + "(" + id + ")" + "\r\n" + description;
            description = description.Replace("<br>", "\r\n");
            int start = description.IndexOf("<"), end;
            while (start != -1)
            {
                end = description.IndexOf(">", start);
                description = description.Remove(start, end - start + 1);

                start = description.IndexOf("<");
            }
        }
    }
}

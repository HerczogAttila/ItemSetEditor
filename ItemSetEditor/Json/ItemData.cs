using System.Collections.ObjectModel;

namespace ItemSetEditor
{
    public class ItemData
    {
        public Collection<string> Tags { get; private set; }
        public DDImage image { get; set; }
        public Gold gold { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string id { get; set; }

        public ItemData()
        {
            Tags = new Collection<string>();
            image = new DDImage();
            name = "";
            description = "";
            gold = new Gold();
            id = "0";
        }

        public void Deserialized()
        {
            description = name + "\r\n" + description;
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

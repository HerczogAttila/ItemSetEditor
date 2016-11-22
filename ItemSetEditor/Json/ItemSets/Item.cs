using Newtonsoft.Json;

namespace ItemSetEditor
{
    public class Item
    {
        public int id { get; set; }
        public int count { get; set; }

        [JsonIgnore]
        public DDImage image
        {
            get
            {
                ItemData item;
                if (MainWindow.Items.data.TryGetValue(id + "", out item))
                    return item.image;

                return new DDImage() { sprite = "item0.png", h = 45, w = 45, x = 45, y = 5 * 45 };
            }
        }

        public Item()
        {
            id = 0;
            count = 1;
        }
    }
}
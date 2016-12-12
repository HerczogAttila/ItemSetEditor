using Newtonsoft.Json;

namespace ItemSetEditor
{
    public class Item
    {
        public int Id { get; set; }
        public int Count { get; set; }

        [JsonIgnore]
        public DDImage Image
        {
            get
            {
                ItemData item;
                if (MainWindow.Items.Data.TryGetValue(Id + "", out item))
                    return item.Image;

                return new DDImage() { Sprite = "item0.png", Height = 45, Width = 45, Left = 45, Top = 5 * 45 };
            }
        }

        public Item()
        {
            Id = 0;
            Count = 1;
        }
    }
}
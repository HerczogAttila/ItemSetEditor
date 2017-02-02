using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ItemSetEditor
{
    public class ItemData
    {
        public Dictionary<string, bool> Maps { get; private set; }
        public Collection<string> Tags { get; private set; }
        public DDImage Image { get; set; }
        public Gold Gold { get; set; }
        public string RequiredChampion { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
        public bool HideFromAll { get; set; }

        public ItemData()
        {
            Maps = new Dictionary<string, bool>();
            Tags = new Collection<string>();
            Image = new DDImage();
            Gold = new Gold();
            RequiredChampion = "";
            Name = "";
            Description = "";
            Id = "0";
        }

        public void Deserialized()
        {
#if DEBUG
            Log.Info("ItemData deserialize.");
#endif

            foreach (var v in Maps.Where(s => s.Value == false).ToArray())
            {
#if DEBUG
                Log.Info("Remove map from item: " + v.Key);
#endif

                Maps.Remove(v.Key);
            }

#if DEBUG
            Log.Info("Create item description: " + Name);
#endif

            Description = Name + "(" + Id + ")" + "\r\n" + Description;
            Description = Description.Replace("<br>", "\r\n");
            int start = Description.IndexOf("<", StringComparison.OrdinalIgnoreCase), end;
            while (start != -1)
            {
                end = Description.IndexOf(">", start, StringComparison.OrdinalIgnoreCase);
                Description = Description.Remove(start, end - start + 1);

                start = Description.IndexOf("<", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}

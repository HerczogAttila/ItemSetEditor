using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSetEditor
{
    public class ItemData
    {
        public Collection<string> Tags { get; private set; }
        public DDImage image { get; set; }
        public string Name { get; set; }

        public ItemData()
        {
            Tags = new Collection<string>();
            image = new DDImage();
            Name = "";
        }
    }
}

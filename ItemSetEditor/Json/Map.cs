using System.Collections.Generic;

namespace ItemSetEditor
{
    public class Map
    {
        public Dictionary<int, MapData> data { get; private set; }

        public Map()
        {
            data = new Dictionary<int, MapData>();
        }
    }
}

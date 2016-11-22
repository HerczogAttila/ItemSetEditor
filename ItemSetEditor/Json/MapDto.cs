using System.Collections.Generic;

namespace ItemSetEditor
{
    public class MapDto
    {
        public Dictionary<int, MapData> data { get; private set; }

        public MapDto()
        {
            data = new Dictionary<int, MapData>();
        }
    }
}

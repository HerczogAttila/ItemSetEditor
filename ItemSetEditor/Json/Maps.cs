using System.Collections.Generic;

namespace ItemSetEditor
{
    public class Maps
    {
        public Dictionary<int, MapData> Data { get; private set; }

        public Maps()
        {
            Data = new Dictionary<int, MapData>();
        }
    }
}

using System.Collections.Generic;

namespace ItemSetEditor
{
    public class Champions
    {
        public Dictionary<string, ChampionData> Data { get; private set; }

        public Champions()
        {
            Data = new Dictionary<string, ChampionData>();
        }
    }
}

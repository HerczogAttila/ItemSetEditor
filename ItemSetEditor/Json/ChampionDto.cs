using System.Collections.Generic;

namespace ItemSetEditor
{
    public class ChampionDto
    {
        public Dictionary<string, ChampionData> data { get; private set; }

        public ChampionDto()
        {
            data = new Dictionary<string, ChampionData>();
        }
    }
}

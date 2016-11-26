using Newtonsoft.Json;

namespace ItemSetEditor
{
    public class Gold
    {
        public bool purchasable { get; set; }
        [JsonProperty("base")]
        public int baseValue { get; set; }
        public int total { get; set; }
        public int sell { get; set; }

        public Gold()
        {
            purchasable = false;
        }
    }
}

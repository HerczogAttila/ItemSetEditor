using Newtonsoft.Json;

namespace ItemSetEditor
{
    public class Gold
    {
        public bool Purchasable { get; set; }
        [JsonProperty("base")]
        public int BaseValue { get; set; }
        public int Total { get; set; }
        public int Sell { get; set; }

        public Gold()
        {
            Purchasable = false;
            BaseValue = 0;
            Total = 0;
            Sell = 0;
        }
    }
}

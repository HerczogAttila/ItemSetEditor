using System.Collections.ObjectModel;

namespace ItemSetEditor
{
    public class Config
    {
        public Collection<int> IgnoredMapIds { get; private set; }
        public Collection<int> IgnoredItemIds { get; private set; }
        public string Version { get; set; }
        public string Language { get; set; }

        public Config()
        {
            IgnoredMapIds = new Collection<int>();
            IgnoredItemIds = new Collection<int>();
            Version = "";
            Language = "";
        }
    }
}

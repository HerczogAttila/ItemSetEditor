using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;

namespace ItemSetEditor
{
    public class Config
    {
        private static string SavePath = "ItemSetEditor\\Config.json";

        public string LinkVersions { get; set; } = "https://ddragon.leagueoflegends.com/api/versions.json";
        public string LinkMaps => "http://ddragon.leagueoflegends.com/cdn/" + Version + "/data/" + Language + "/map.json";
        public string LinkChampions => "http://ddragon.leagueoflegends.com/cdn/" + Version + "/data/" + Language + "/champion.json";
        public string LinkItems => "http://ddragon.leagueoflegends.com/cdn/" + Version + "/data/" + Language + "/item.json";
        public string PathMaps => "ItemSetEditor\\maps_" + Version + "_" + Language + ".json";
        public string PathChampions => "ItemSetEditor\\champions_" + Version + "_" + Language + ".json";
        public string PathItems => "ItemSetEditor\\items_" + Version + "_" + Language + ".json";

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

        public static bool Exists() { return File.Exists(SavePath); }
        public static Config Load()
        {
#if DEBUG
            Log.Info("Load config.");
#endif

            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(SavePath));
        }
        public void Save()
        {
#if DEBUG
            Log.Info("Save config.");
#endif

            File.WriteAllText(SavePath, JsonConvert.SerializeObject(this));
        }
    }
}

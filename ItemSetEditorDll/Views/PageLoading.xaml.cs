using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ItemSetEditor
{
    /// <summary>
    /// Interaction logic for PageLoading.xaml
    /// </summary>
    public partial class PageLoading : Page, INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string ProgressText { get; set; }

        private WebClient webClient;
        private Thread loading;
        private DataEditor data;

        public PageLoading()
        {
            InitializeComponent();
            webClient = new WebClient();
            data = new DataEditor();
            Item.Data = data;
            ProgressText = "Loading...";

            DataContext = this;
        }

        private async Task DownloadImage(DDImage image, bool update = false)
        {
            if (File.Exists(image.Path) && !update)
                return;

#if DEBUG
            Log.Info("Download image: " + image.Link + " to: " + image.Path);
#endif

            try
            {
                await webClient.DownloadFileTaskAsync(image.Link, image.Path);
            }
            catch (Exception e)
            {
#if DEBUG
                Log.Error("Failed to download image: " + image.Link + ". \r\n" + e.Message);
#endif
            }
        }
        private async Task<string> LatestVersion()
        {
#if DEBUG
            Log.Info("Download versions.");
#endif

            string versionsData = "";
            try
            {
                versionsData = await webClient.DownloadStringTaskAsync(data.Config.LinkVersions);
            }
            catch (Exception e)
            {
#if DEBUG
                Log.Error("Failed to download versions: " + data.Config.LinkVersions + ". \r\n" + e.Message);
#endif
            }

#if DEBUG
            if (string.IsNullOrEmpty(versionsData))
                Log.Warning("Download versions is failed!");
#endif

            var versions = JsonConvert.DeserializeObject<Collection<string>>(versionsData);
            if (versions != null)
            {
                if (versions.Count > 0)
                    return versions.ElementAt(0);
#if DEBUG
                else
                    Log.Warning("Versions list is empty!");
#endif
            }
#if DEBUG
            else
                Log.Warning("Deserialize versions is failed!");
#endif

            return "7.2.1";
        }
        private async Task ReadConfig()
        {
#if DEBUG
            Log.Info("Read config.");
#endif

            if (Config.Exists())
            {
                data.Config = Config.Load();
                var latest = await LatestVersion();
                if (data.Config.Version != latest)
                {
#if DEBUG
                    Log.Info("New version detected.");
#endif

                    data.Config.Version = await LatestVersion();
                    data.Config.Save();
                }
            }
            else
            {
#if DEBUG
                Log.Warning("Not found config file! Create with default data.");
#endif

                data.Config = new Config() { Version = await LatestVersion(), Language = "en_US" };

                data.Config.IgnoredMapIds.Add(1);
                data.Config.IgnoredMapIds.Add(8);
                data.Config.IgnoredMapIds.Add(9);
                data.Config.IgnoredItemIds.Add(3631);
                data.Config.IgnoredItemIds.Add(3634);
                data.Config.IgnoredItemIds.Add(3635);
                data.Config.IgnoredItemIds.Add(3636);
                data.Config.IgnoredItemIds.Add(3640);
                data.Config.IgnoredItemIds.Add(3641);
                data.Config.IgnoredItemIds.Add(3642);
                data.Config.IgnoredItemIds.Add(3643);
                data.Config.IgnoredItemIds.Add(3647);
                data.Config.IgnoredItemIds.Add(3680);
                data.Config.IgnoredItemIds.Add(3681);
                data.Config.IgnoredItemIds.Add(3682);
                data.Config.IgnoredItemIds.Add(3683);
                data.Config.Save();
            }

            DDImage.BaseLink = "http://ddragon.leagueoflegends.com/cdn/" + data.Config.Version + "/img/sprite/";
        }
        private async Task ReadAndDownloadMaps()
        {
#if DEBUG
            Log.Info("Read maps.");
#endif

            bool isDownloaded = false;
            if (!File.Exists(data.Config.PathMaps))
            {
                ProgressText = "Download maps...";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProgressText"));

                try
                {
                    await webClient.DownloadFileTaskAsync(data.Config.LinkMaps, data.Config.PathMaps);
                }
                catch (Exception e)
                {
#if DEBUG
                    Log.Error("Failed to download maps: " + data.Config.LinkMaps + ". \r\n" + e.Message);
#endif
                }

                isDownloaded = true;
            }

            var maps = JsonConvert.DeserializeObject<Maps>(File.ReadAllText(data.Config.PathMaps));
            foreach (var v in maps.Data.Values)
            {
                if (!data.Config.IgnoredMapIds.Contains(v.MapId))
                {
#if DEBUG
                    Log.Info("Add map: " + v.MapName + ". Id: " + v.MapId);
#endif

                    ItemSet.MapIds.Add(v);
                }
#if DEBUG
                else
                    Log.Info("Ignored map: " + v.MapName + ". Id: " + v.MapId);
#endif
            }

            MapData i;
            foreach (var v in maps.Data.Values.Select(s => s.Image.Sprite).Distinct())
            {
                i = maps.Data.Values.FirstOrDefault(s => s.Image.Sprite.Equals(v));
                if (i != null)
                    await DownloadImage(i.Image, isDownloaded);
            }

            data.Maps = maps;
        }
        private async Task ReadAndDownloadItems()
        {
#if DEBUG
            Log.Info("Read items.");
#endif

            bool isDownloaded = false;
            if (!File.Exists(data.Config.PathItems))
            {
                ProgressText = "Download items...";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProgressText"));

                try
                {
                    await webClient.DownloadFileTaskAsync(data.Config.LinkItems, data.Config.PathItems);
                }
                catch (Exception e)
                {
#if DEBUG
                    Log.Error("Failed to download items: " + data.Config.LinkItems + ". \r\n" + e.Message);
#endif
                }

                isDownloaded = true;
            }

#if DEBUG
            Log.Info("Initialize item tags.");
#endif

            data.ItemTags.Clear();
            data.ItemTags.Add(new SortTag() { IsChecked = true });

            var items = JsonConvert.DeserializeObject<Items>(File.ReadAllText(data.Config.PathItems));
            items.Deserialized(data.Config);
            foreach (var v in items.Data.Values)
            {
                v.Deserialized();

                foreach (var t in v.Tags)
                    if (data.ItemTags.FirstOrDefault(s => s.Tag.Equals(t)) == null)
                        data.ItemTags.Add(new SortTag() { Tag = t });
            }

            ItemData i;
            foreach (var v in items.Data.Values.Select(s => s.Image.Sprite).Distinct())
            {
                i = items.Data.Values.FirstOrDefault(s => s.Image.Sprite.Equals(v));
                if (i != null)
                    await DownloadImage(i.Image, isDownloaded);
            }

            data.Items = items;
        }
        private async Task ReadAndDownloadChampions()
        {
#if DEBUG
            Log.Info("Read champions.");
#endif

            bool isDownloaded = false;
            if (!File.Exists(data.Config.PathChampions))
            {
                ProgressText = "Download champions...";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProgressText"));

                try
                {
                    await webClient.DownloadFileTaskAsync(data.Config.LinkChampions, data.Config.PathChampions);
                }
                catch (Exception e)
                {
#if DEBUG
                    Log.Error("Failed to download champions: " + data.Config.LinkChampions + ". \r\n" + e.Message);
#endif
                }

                isDownloaded = true;
            }

#if DEBUG
            Log.Info("Initialize champion tags.");
#endif

            data.ChampionTags.Add(new SortTag() { IsChecked = true });

            var champions = JsonConvert.DeserializeObject<Champions>(File.ReadAllText(data.Config.PathChampions));
            foreach (var v in champions.Data.Values)
            {
                foreach (var t in v.Tags)
                    if (data.ChampionTags.FirstOrDefault(s => s.Tag.Equals(t)) == null)
                        data.ChampionTags.Add(new SortTag() { Tag = t });
            }

            ChampionData i;
            foreach (var v in champions.Data.Values.Select(s => s.Image.Sprite).Distinct())
            {
                i = champions.Data.Values.FirstOrDefault(s => s.Image.Sprite.Equals(v));
                if (i != null)
                    await DownloadImage(i.Image, isDownloaded);
            }

            data.Champions = champions;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (loading != null)
                loading.Abort();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                if (webClient != null)
                    webClient.Dispose();
            }
            // free native resources
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void StartLoading(Window win)
        {
#if DEBUG
            Log.Info("Start loading.");
#endif

            loading = new Thread(new ThreadStart(async () =>
            {
                await ReadConfig();
                await ReadAndDownloadMaps();
                await ReadAndDownloadItems();
                await ReadAndDownloadChampions();

                data.SortChampions();

                Dispatcher.Invoke(() =>
                {
                    data.UndoAll();
                    win.Content = new PageEditor(data);
                });
            }));

            loading.Start();
        }
    }
}

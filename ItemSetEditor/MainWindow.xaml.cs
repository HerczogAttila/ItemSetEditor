using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Net;
using System.Collections.ObjectModel;

namespace ItemSetEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static ItemDto Items { get; set; }
        public static ChampionDto Champions { get; set; }

        private static string PathItemSets = "Config\\ItemSets.json";
        private static string PathConfig = "ItemSetEditor\\Config.json";
        private static string LinkVersions = "https://ddragon.leagueoflegends.com/api/versions.json";

        private string LinkMaps => "http://ddragon.leagueoflegends.com/cdn/" + Config.Version + "/data/" + Config.Language + "/map.json";
        private string LinkChampions => "http://ddragon.leagueoflegends.com/cdn/" + Config.Version + "/data/" + Config.Language + "/champion.json";
        private string LinkItems => "http://ddragon.leagueoflegends.com/cdn/" + Config.Version + "/data/" + Config.Language + "/item.json";
        private string LinkSprites => "http://ddragon.leagueoflegends.com/cdn/" + Config.Version + "/img/sprite/";
        private string PathMaps => "ItemSetEditor\\maps_" + Config.Version + "_" + Config.Language + ".json";
        private string PathChampions => "ItemSetEditor\\champions_" + Config.Version + "_" + Config.Language + ".json";
        private string PathItems => "ItemSetEditor\\items_" + Config.Version + "_" + Config.Language + ".json";

        public event PropertyChangedEventHandler PropertyChanged;

        public MapDto Maps { get; set; }
        public ItemSets ItemSets { get; set; }
        public ItemSet Selected { get; set; }
        public bool IsChanged { get; set; }

        public static Config Config;

        private WebClient WebClient = new WebClient();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void DownloadImage(DDImage image)
        {
            if (File.Exists(image.Path))
                return;

            WebClient.DownloadFile(image.Link, image.Path);
        }

        private async Task<string> Download(string link)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(link);
                var response = client.GetAsync(link).Result;
                if (response.IsSuccessStatusCode) return await response.Content.ReadAsStringAsync();
            }

            return string.Empty;
        }

        private async Task DownloadAndSave(string link, string path)
        {
            var data = await Download(link);
            File.WriteAllText(path, data);
        }

        private async Task<string> LatestVersion()
        {
            var data = await Download(LinkVersions);
            var versions = JsonConvert.DeserializeObject<Collection<string>>(data);
            if (versions != null)
                if (versions.Count > 0)
                    return versions.ElementAt(0);

            return "6.22.1";
        }

        private void SelectItemSet(ItemSet itemSet)
        {
            Selected = itemSet;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Selected"));

            if (Selected == null)
            {
                PanelSelected.Visibility = Visibility.Collapsed;
                return;
            }
            else
                PanelSelected.Visibility = Visibility.Visible;

            foreach(MapData m in ItemSet.MapIds)
            {
                m.IsChecked = Selected.associatedMaps.Contains(m.MapId);
                m.OnChanged("IsChecked");
            }
        }

        private void itemSetChanged(bool enabled)
        {
            IsChanged = enabled;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsChanged"));
        }

        private void ReadItemSets()
        {
            if (File.Exists(PathItemSets))
            {
                ItemSets = JsonConvert.DeserializeObject<ItemSets>(File.ReadAllText(PathItemSets));
                foreach (ItemSet i in ItemSets.itemSets)
                    i.Deserialized();
            }
            else
                ItemSets = new ItemSets();
        }

        private async Task ReadConfig()
        {
            if (File.Exists(PathConfig))
                Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(PathConfig));
            else
            {
                Config = new Config() { Version = await LatestVersion(), Language = "en_US" };
                
                Config.IgnoredMapIds.Add(1);
                Config.IgnoredMapIds.Add(8);
                Config.IgnoredMapIds.Add(9);
                SaveConfig();
            }
        }

        private async Task ReadAndDownloadMaps()
        {
            bool isDownloaded = false;
            if (!File.Exists(PathMaps))
            {
                await DownloadAndSave(LinkMaps, PathMaps);
                isDownloaded = true;
            }

            Maps = JsonConvert.DeserializeObject<MapDto>(File.ReadAllText(PathMaps));
            foreach(MapData s in Maps.data.Values)
            {
                if(!Config.IgnoredMapIds.Contains(s.MapId))
                    ItemSet.MapIds.Add(s);

                if (isDownloaded)
                    DownloadImage(s.image);
            }
        }

        private async Task ReadAndDownloadItems()
        {
            bool isDownloaded = false;
            if (!File.Exists(PathItems))
            {
                await DownloadAndSave(LinkItems, PathItems);
                isDownloaded = true;
            }

            Items = JsonConvert.DeserializeObject<ItemDto>(File.ReadAllText(PathItems));
            foreach (ItemData s in Items.data.Values)
            {
                if (isDownloaded)
                    DownloadImage(s.image);
            }
        }

        private async Task ReadAndDownloadChampions()
        {
            bool isDownloaded = false;
            if (!File.Exists(PathChampions))
            {
                await DownloadAndSave(LinkChampions, PathChampions);
                isDownloaded = true;
            }

            Champions = JsonConvert.DeserializeObject<ChampionDto>(File.ReadAllText(PathChampions));
            foreach (ChampionData s in Champions.data.Values)
            {
                if (isDownloaded)
                    DownloadImage(s.image);
            }
        }

        private void SaveConfig()
        {
            File.WriteAllText(PathConfig, JsonConvert.SerializeObject(Config));
        }

        private void ItemSet_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            bool prev = IsChanged;
            SelectItemSet((sender as TextBlock).Tag as ItemSet);
            itemSetChanged(prev);
        }

        private void NewItemSet_Click(object sender, RoutedEventArgs e)
        {
            var newSet = new ItemSet() { title = "Item set " + ItemSets.itemSets.Count };
            newSet.blocks.Add(new Block() { type = "Item block 1" });
            while (ItemSets.itemSets.FirstOrDefault(s => s.uid == newSet.uid) != null) {
                newSet.uid = CodeGenerator.GenerateUID();
            }
            ItemSets.itemSets.Add(newSet);
            SelectItemSet(newSet);
            itemSetChanged(true);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(PathItemSets, JsonConvert.SerializeObject(ItemSets));
            itemSetChanged(false);
        }

        private void UndoAll_Click(object sender, RoutedEventArgs e)
        {
            ReadItemSets();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ItemSets"));

            if (ItemSets.itemSets.Count > 0)
                SelectItemSet(ItemSets.itemSets.ElementAt(ItemSets.itemSets.Count - 1));
            else
                SelectItemSet(null);

            itemSetChanged(false);
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            ItemSets.itemSets.Remove(Selected);
            if (ItemSets.itemSets.Count > 0)
                SelectItemSet(ItemSets.itemSets.ElementAt(ItemSets.itemSets.Count - 1));
            else
                SelectItemSet(null);

            itemSetChanged(true);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (string s in new string[] { "Config", "ItemSetEditor" })
                if (!Directory.Exists(s))
                    Directory.CreateDirectory(s);

            await ReadConfig();
            await ReadAndDownloadMaps();
            await ReadAndDownloadItems();
            await ReadAndDownloadChampions();

            DataContext = this;

            UndoAll_Click(this, new RoutedEventArgs());
        }

        private void Title_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                Selected.title = (sender as TextBox).Text;
                Selected.OnChanged("title");
                itemSetChanged(true);
            }
        }

        private void Map_Checked(object sender, RoutedEventArgs e)
        {
            var id = int.Parse((sender as CheckBox).Tag.ToString());
            if (!Selected.associatedMaps.Contains(id))
            {
                if (Selected.associatedMaps.Count == 0)
                {
                    Selected.isGlobalForMaps = false;
                    Selected.OnChanged("isGlobalForMaps");
                }

                Selected.associatedMaps.Add(id);
                itemSetChanged(true);
            }
        }

        private void Map_Unchecked(object sender, RoutedEventArgs e)
        {
            var id = int.Parse((sender as CheckBox).Tag.ToString());
            Selected.associatedMaps.Remove(id);
            itemSetChanged(true);

            if (Selected.associatedMaps.Count == 0)
            {
                Selected.isGlobalForMaps = true;
                Selected.OnChanged("isGlobalForMaps");
            }
        }

        private void BlockTitle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var txt = (sender as TextBox);
                var itemBlock = txt.Tag as Block;
                itemBlock.type = txt.Text;
                itemBlock.OnChanged("type");
                itemSetChanged(true);
            }
        }

        private void NewItemBlock_Click(object sender, RoutedEventArgs e)
        {
            Selected.blocks.Add(new Block() { type = "Item block " + (Selected.blocks.Count() + 1) });
            itemSetChanged(true);
        }

        private void DeleteItemBlock_Click(object sender, RoutedEventArgs e)
        {
            var itemBlock = (sender as Control).Tag as Block;
            if (itemBlock != null)
            {
                Selected.blocks.Remove(itemBlock);
                itemSetChanged(true);
            }
        }

        private void ChampionRemove_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var champion = (sender as Image).Tag as ChampionData;
            Selected.associatedChampions.Remove(champion.key);
            Selected.Champions.Remove(champion);

            if (Selected.associatedChampions.Count == 0)
            {
                Selected.isGlobalForChampions = true;
                Selected.OnChanged("isGlobalForChampions");
            }

            itemSetChanged(true);
        }

        private void ChampionAdd_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var champion = (sender as Image).Tag as ChampionData;
            if (!Selected.associatedChampions.Contains(champion.key))
            {
                if (Selected.associatedChampions.Count == 0)
                {
                    Selected.isGlobalForChampions = false;
                    Selected.OnChanged("isGlobalForChampions");
                }

                Selected.associatedChampions.Add(champion.key);
                Selected.Champions.Add(champion);
                itemSetChanged(true);
            }
        }
    }
}

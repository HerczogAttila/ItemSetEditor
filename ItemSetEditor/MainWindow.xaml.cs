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
using System.Collections.Generic;
using System.Threading;

namespace ItemSetEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static ItemDto Items { get; set; }
        public static ChampionDto Champions { get; set; }
        public static Config Config { get; set; }

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

        public IEnumerable<ItemData> SortedItems { get; set; }
        public IEnumerable<ChampionData> SortedChampions { get; set; }
        public Collection<SortTag> ItemTags { get; set; }
        public Collection<SortTag> ChampionTags { get; set; }
        public Visibility VisLoading { get; set; }
        public Visibility VisEditor { get; set; }
        public Visibility VisSelected { get; set; }
        public MapDto Maps { get; set; }
        public ItemSets ItemSets { get; set; }
        public ItemSet Selected { get; set; }
        public string SortItemName { get; set; }
        public string SortChampionName { get; set; }
        public string ProgressText { get; set; }
        public bool IsChanged { get; set; }

        private WebClient WebClient = new WebClient();
        private ItemData Dragged;
        private Thread thread;

        public MainWindow()
        {
            InitializeComponent();
            ChampionTags = new Collection<SortTag>();
            ItemTags = new Collection<SortTag>();
            VisLoading = Visibility.Visible;
            VisEditor = Visibility.Collapsed;
            VisSelected = Visibility.Collapsed;
            ProgressText = "Loading...";

            DataContext = this;
        }

        private async Task DownloadImage(DDImage image, bool update = false)
        {
            if (File.Exists(image.Path) && !update)
                return;

            await WebClient.DownloadFileTaskAsync(image.Link, image.Path);
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

            return "6.23.1";
        }

        private void SortItems()
        {
            var sorted = Items.data.Values.Where(s => !s.hideFromAll);
            foreach (var v in Selected.associatedMaps)
                sorted = sorted.Where(s => s.maps.ContainsKey(v + ""));

            ChampionData cd;
            foreach (var v in Selected.associatedChampions)
            {
                cd = Champions.data.Values.FirstOrDefault(s => s.key.Equals(v));
                if (cd != null)
                    sorted = sorted.Where(s => s.requiredChampion.Equals("") || s.requiredChampion.Equals(cd.name));
            }

            SortItemName = SortItemName.ToLower();
            if (!string.IsNullOrEmpty(SortItemName))
                sorted = sorted.Where(s => s.name.ToLower().IndexOf(SortItemName) > -1);

            var tag = ItemTags.FirstOrDefault(s => s.IsChecked == true);
            if (tag != null)
                if (!string.IsNullOrEmpty(tag.Tag))
                    sorted = sorted.Where(s => s.Tags.Contains(tag.Tag));

            SortedItems = sorted;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SortedItems"));
        }

        private void SortChampions()
        {
            SortedChampions = Champions.data.Values;

            SortChampionName = SortChampionName.ToLower();
            if (!string.IsNullOrEmpty(SortChampionName))
                SortedChampions = SortedChampions.Where(s => s.name.ToLower().IndexOf(SortChampionName) > -1);

            var tag = ChampionTags.FirstOrDefault(s => s.IsChecked == true);
            if (tag != null)
                if (!string.IsNullOrEmpty(tag.Tag))
                    SortedChampions = SortedChampions.Where(s => s.tags.Contains(tag.Tag));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SortedChampions"));
        }

        private void SelectItemSet(ItemSet itemSet)
        {
            Selected = itemSet;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Selected"));

            if (Selected == null)
            {
                VisSelected = Visibility.Collapsed;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VisSelected"));
                return;
            }
            else
            {
                VisSelected = Visibility.Visible;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VisSelected"));
            }

            foreach(MapData m in ItemSet.MapIds)
            {
                m.IsChecked = Selected.associatedMaps.Contains(m.MapId);
                m.OnChanged("IsChecked");
            }

            SortItems();
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
            {
                Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(PathConfig));
                Config.Version = await LatestVersion();
                SaveConfig();
            }
            else
            {
                Config = new Config() { Version = await LatestVersion(), Language = "en_US" };

                Config.IgnoredMapIds.Add(1);
                Config.IgnoredMapIds.Add(8);
                Config.IgnoredMapIds.Add(9);
                Config.IgnoredItemIds.Add(3631);
                Config.IgnoredItemIds.Add(3634);
                Config.IgnoredItemIds.Add(3635);
                Config.IgnoredItemIds.Add(3636);
                Config.IgnoredItemIds.Add(3640);
                Config.IgnoredItemIds.Add(3641);
                Config.IgnoredItemIds.Add(3642);
                Config.IgnoredItemIds.Add(3643);
                Config.IgnoredItemIds.Add(3647);
                Config.IgnoredItemIds.Add(3680);
                Config.IgnoredItemIds.Add(3681);
                Config.IgnoredItemIds.Add(3682);
                Config.IgnoredItemIds.Add(3683);
                SaveConfig();
            }
        }

        private async Task ReadAndDownloadMaps()
        {
            bool isDownloaded = false;
            if (!File.Exists(PathMaps))
            {
                ProgressText = "Download maps...";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProgressText"));

                await DownloadAndSave(LinkMaps, PathMaps);
                isDownloaded = true;
            }

            Maps = JsonConvert.DeserializeObject<MapDto>(File.ReadAllText(PathMaps));
            foreach (var v in Maps.data.Values)
            {
                if(!Config.IgnoredMapIds.Contains(v.MapId))
                    ItemSet.MapIds.Add(v);
            }

            MapData i;
            foreach (var v in Maps.data.Values.Select(s => s.image.sprite).Distinct())
            {
                i = Maps.data.Values.FirstOrDefault(s => s.image.sprite.Equals(v));
                if (i != null)
                    await DownloadImage(i.image, isDownloaded);
            }
        }

        private async Task ReadAndDownloadItems()
        {
            bool isDownloaded = false;
            if (!File.Exists(PathItems))
            {
                ProgressText = "Download items...";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProgressText"));

                await DownloadAndSave(LinkItems, PathItems);
                isDownloaded = true;
            }

            SortItemName = "";

            ItemTags.Clear();
            ItemTags.Add(new SortTag() { IsChecked = true });

            Items = JsonConvert.DeserializeObject<ItemDto>(File.ReadAllText(PathItems));
            Items.Deserialized();
            foreach (var v in Items.data.Values)
            {
                v.Deserialized();

                foreach (var t in v.Tags)
                    if (ItemTags.FirstOrDefault(s => s.Tag.Equals(t)) == null)
                        ItemTags.Add(new SortTag() { Tag = t });
            }

            ItemData i;
            foreach (var v in Items.data.Values.Select(s => s.image.sprite).Distinct())
            {
                i = Items.data.Values.FirstOrDefault(s => s.image.sprite.Equals(v));
                if (i != null)
                    await DownloadImage(i.image, isDownloaded);
            }
        }

        private async Task ReadAndDownloadChampions()
        {
            bool isDownloaded = false;
            if (!File.Exists(PathChampions))
            {
                ProgressText = "Download champions...";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProgressText"));

                await DownloadAndSave(LinkChampions, PathChampions);
                isDownloaded = true;
            }

            SortChampionName = "";
            ChampionTags.Clear();
            ChampionTags.Add(new SortTag() { IsChecked = true });

            Champions = JsonConvert.DeserializeObject<ChampionDto>(File.ReadAllText(PathChampions));
            foreach (var v in Champions.data.Values)
            {
                foreach (var t in v.tags)
                    if (ChampionTags.FirstOrDefault(s => s.Tag.Equals(t)) == null)
                        ChampionTags.Add(new SortTag() { Tag = t });
            }

            ChampionData i;
            foreach (var v in Champions.data.Values.Select(s => s.image.sprite).Distinct())
            {
                i = Champions.data.Values.FirstOrDefault(s => s.image.sprite.Equals(v));
                if (i != null)
                    await DownloadImage(i.image, isDownloaded);
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            thread = new Thread(new ThreadStart(async ()=>
            {
                foreach (string s in new string[] { "Config", "ItemSetEditor" })
                    if (!Directory.Exists(s))
                        Directory.CreateDirectory(s);

                await ReadConfig();
                await ReadAndDownloadMaps();
                await ReadAndDownloadItems();
                await ReadAndDownloadChampions();

                SortChampions();

                UndoAll_Click(this, new RoutedEventArgs());

                VisLoading = Visibility.Collapsed;
                VisEditor = Visibility.Visible;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VisLoading"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VisEditor"));
            }));

            thread.Start();
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
                SortItems();
            }
        }

        private void Map_Unchecked(object sender, RoutedEventArgs e)
        {
            var id = int.Parse((sender as CheckBox).Tag.ToString());
            Selected.associatedMaps.Remove(id);
            itemSetChanged(true);
            SortItems();

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
            SortItems();
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
                SortItems();
            }
        }

        #region drag

        private void ItemDrag_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed && Dragged == null)
            {
                Mouse.AddMouseMoveHandler(this, Item_MouseMove);
                Mouse.AddMouseUpHandler(this, Item_MouseRelease);

                var p = e.GetPosition(this);
                drag.Margin = new Thickness(p.X - 24, p.Y - 24, 0, 0);

                drag.Source = (sender as Image).Source;
                Mouse.OverrideCursor = Cursors.No;
                drag.Visibility = Visibility.Visible;

                Dragged = (sender as Image).Tag as ItemData;
            }
        }

        private void Item_MouseMove(object sender, MouseEventArgs e)
        {
            var p = e.GetPosition(this);
            drag.Margin = new Thickness(p.X - 24, p.Y - 24, 0, 0);

            if (e.LeftButton == MouseButtonState.Released)
                Item_MouseRelease(sender, e);
        }

        private void Item_MouseRelease(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
            drag.Visibility = Visibility.Collapsed;

            Mouse.RemoveMouseMoveHandler(this, Item_MouseMove);
            Mouse.RemoveMouseUpHandler(this, Item_MouseRelease);

            Dragged = null;
        }

        #endregion

        private void ItemBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Dragged == null)
                return;

            var block = (sender as StackPanel).Tag as Block;
            block.items.Add(new Item() { id = int.Parse(Dragged.id), count = 1 });

            Dragged = null;

            itemSetChanged(true);
        }

        private void SortItemName_TextChanged(object sender, TextChangedEventArgs e)
        {
            SortItems();
        }

        private void SelectItemTag_Checked(object sender, RoutedEventArgs e)
        {
            SortItems();
        }

        private void SortChampionName_TextChanged(object sender, TextChangedEventArgs e)
        {
            SortChampions();
        }

        private void SelectChampionTag_Checked(object sender, RoutedEventArgs e)
        {
            SortChampions();
        }

        private void BlockItemDrag_MouseMove(object sender, MouseEventArgs e)
        {
            if (Dragged != null)
                Mouse.OverrideCursor = Cursors.None;

            if (e.LeftButton == MouseButtonState.Pressed && Dragged == null)
            {
                Mouse.AddMouseMoveHandler(this, Item_MouseMove);
                Mouse.AddMouseUpHandler(this, Item_MouseRelease);

                var p = e.GetPosition(this);
                drag.Margin = new Thickness(p.X - 24, p.Y - 24, 0, 0);

                drag.Source = (sender as Image).Source;
                Mouse.OverrideCursor = Cursors.No;
                drag.Visibility = Visibility.Visible;

                var item = (sender as Image).Tag as Item;
                foreach (var v in Selected.blocks)
                    v.items.Remove(item);

                Dragged = Items.data.Values.FirstOrDefault(s => s.id.Equals(item.id + ""));
            }
        }

        private void InsertItem(object sender, MouseButtonEventArgs e)
        {
            if (Dragged == null)
                return;

            var item = (sender as Image).Tag as Item;
            var block = Selected.blocks.FirstOrDefault(s => s.items.Contains(item));
            if (block == null)
                return;

            var index = block.items.IndexOf(item); 

            block.items.Insert(index, new Item() { id = int.Parse(Dragged.id), count = 1 });

            Dragged = null;

            itemSetChanged(true);
        }

        private void BlockItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Dragged != null)
                Mouse.OverrideCursor = Cursors.No;
        }

        private void ItemBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Dragged != null)
                Mouse.OverrideCursor = Cursors.No;
        }

        private void ItemBlock_MouseMove(object sender, MouseEventArgs e)
        {
            if (Dragged != null)
                Mouse.OverrideCursor = Cursors.None;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (thread != null)
                thread.Abort();
        }
    }
}
//item tree
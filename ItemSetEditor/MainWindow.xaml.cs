using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;
using System;
using System.Net;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;

namespace ItemSetEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, IDisposable
    {
        public static Items Items { get; set; }
        public static Champions Champions { get; set; }
        public static Config Config { get; set; }

        private static string PathItemSets = "Config\\ItemSets.json";
        private static string PathConfig = "ItemSetEditor\\Config.json";
        private static string LinkVersions = "https://ddragon.leagueoflegends.com/api/versions.json";

        private static string LinkMaps => "http://ddragon.leagueoflegends.com/cdn/" + Config.Version + "/data/" + Config.Language + "/map.json";
        private static string LinkChampions => "http://ddragon.leagueoflegends.com/cdn/" + Config.Version + "/data/" + Config.Language + "/champion.json";
        private static string LinkItems => "http://ddragon.leagueoflegends.com/cdn/" + Config.Version + "/data/" + Config.Language + "/item.json";
        private static string PathMaps => "ItemSetEditor\\maps_" + Config.Version + "_" + Config.Language + ".json";
        private static string PathChampions => "ItemSetEditor\\champions_" + Config.Version + "_" + Config.Language + ".json";
        private static string PathItems => "ItemSetEditor\\items_" + Config.Version + "_" + Config.Language + ".json";

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<ItemData> SortedItems { get; private set; }
        public IEnumerable<ChampionData> SortedChampions { get; private set; }
        public Collection<SortTag> ItemTags { get; private set; }
        public Collection<SortTag> ChampionTags { get; private set; }
        public Visibility VisLoading { get; set; }
        public Visibility VisEditor { get; set; }
        public Visibility VisSelected { get; set; }
        public Maps Maps { get; set; }
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

        private async Task<string> LatestVersion()
        {
            var data = await WebClient.DownloadStringTaskAsync(LinkVersions);
            var versions = JsonConvert.DeserializeObject<Collection<string>>(data);
            if (versions != null)
                if (versions.Count > 0)
                    return versions.ElementAt(0);

            return "6.23.1";
        }

        private void SortItems()
        {
            var sorted = Items.Data.Values.Where(s => !s.HideFromAll);
            foreach (var v in Selected.AssociatedMaps)
                sorted = sorted.Where(s => s.Maps.ContainsKey(v + ""));

            ChampionData cd;
            foreach (var v in Selected.AssociatedChampions)
            {
                cd = Champions.Data.Values.FirstOrDefault(s => s.Key.Equals(v));
                if (cd != null)
                    sorted = sorted.Where(s => string.IsNullOrEmpty(s.RequiredChampion) || s.RequiredChampion.Equals(cd.Name));
            }

            if (!string.IsNullOrEmpty(SortItemName))
                sorted = sorted.Where(s => s.Name.IndexOf(SortItemName, StringComparison.OrdinalIgnoreCase) > -1);

            var tag = ItemTags.FirstOrDefault(s => s.IsChecked == true);
            if (tag != null)
                if (!string.IsNullOrEmpty(tag.Tag))
                    sorted = sorted.Where(s => s.Tags.Contains(tag.Tag));

            SortedItems = sorted;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SortedItems"));
        }

        private void SortChampions()
        {
            SortedChampions = Champions.Data.Values;

            if (!string.IsNullOrEmpty(SortChampionName))
                SortedChampions = SortedChampions.Where(s => s.Name.IndexOf(SortChampionName, StringComparison.OrdinalIgnoreCase) > -1);

            var tag = ChampionTags.FirstOrDefault(s => s.IsChecked == true);
            if (tag != null)
                if (!string.IsNullOrEmpty(tag.Tag))
                    SortedChampions = SortedChampions.Where(s => s.Tags.Contains(tag.Tag));

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
                m.IsChecked = Selected.AssociatedMaps.Contains(m.MapId);
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
                foreach (ItemSet i in ItemSets.Sets)
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

                await WebClient.DownloadFileTaskAsync(LinkMaps, PathMaps);
                isDownloaded = true;
            }

            Maps = JsonConvert.DeserializeObject<Maps>(File.ReadAllText(PathMaps));
            foreach (var v in Maps.Data.Values)
            {
                if(!Config.IgnoredMapIds.Contains(v.MapId))
                    ItemSet.MapIds.Add(v);
            }

            MapData i;
            foreach (var v in Maps.Data.Values.Select(s => s.Image.Sprite).Distinct())
            {
                i = Maps.Data.Values.FirstOrDefault(s => s.Image.Sprite.Equals(v));
                if (i != null)
                    await DownloadImage(i.Image, isDownloaded);
            }
        }

        private async Task ReadAndDownloadItems()
        {
            bool isDownloaded = false;
            if (!File.Exists(PathItems))
            {
                ProgressText = "Download items...";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProgressText"));

                await WebClient.DownloadFileTaskAsync(LinkItems, PathItems);
                isDownloaded = true;
            }

            SortItemName = "";

            ItemTags.Clear();
            ItemTags.Add(new SortTag() { IsChecked = true });

            Items = JsonConvert.DeserializeObject<Items>(File.ReadAllText(PathItems));
            Items.Deserialized();
            foreach (var v in Items.Data.Values)
            {
                v.Deserialized();

                foreach (var t in v.Tags)
                    if (ItemTags.FirstOrDefault(s => s.Tag.Equals(t)) == null)
                        ItemTags.Add(new SortTag() { Tag = t });
            }

            ItemData i;
            foreach (var v in Items.Data.Values.Select(s => s.Image.Sprite).Distinct())
            {
                i = Items.Data.Values.FirstOrDefault(s => s.Image.Sprite.Equals(v));
                if (i != null)
                    await DownloadImage(i.Image, isDownloaded);
            }
        }

        private async Task ReadAndDownloadChampions()
        {
            bool isDownloaded = false;
            if (!File.Exists(PathChampions))
            {
                ProgressText = "Download champions...";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProgressText"));

                await WebClient.DownloadFileTaskAsync(LinkChampions, PathChampions);
                isDownloaded = true;
            }

            SortChampionName = "";
            ChampionTags.Clear();
            ChampionTags.Add(new SortTag() { IsChecked = true });

            Champions = JsonConvert.DeserializeObject<Champions>(File.ReadAllText(PathChampions));
            foreach (var v in Champions.Data.Values)
            {
                foreach (var t in v.Tags)
                    if (ChampionTags.FirstOrDefault(s => s.Tag.Equals(t)) == null)
                        ChampionTags.Add(new SortTag() { Tag = t });
            }

            ChampionData i;
            foreach (var v in Champions.Data.Values.Select(s => s.Image.Sprite).Distinct())
            {
                i = Champions.Data.Values.FirstOrDefault(s => s.Image.Sprite.Equals(v));
                if (i != null)
                    await DownloadImage(i.Image, isDownloaded);
            }
        }

        private static void SaveConfig()
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
            var newSet = new ItemSet() { Title = "Item set " + ItemSets.Sets.Count };
            newSet.Blocks.Add(new Block() { BlockType = "Item block 1" });
            while (ItemSets.Sets.FirstOrDefault(s => s.Id == newSet.Id) != null) {
                newSet.Id = CodeGenerator.Generate();
            }
            ItemSets.Sets.Add(newSet);
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

            if (ItemSets.Sets.Count > 0)
                SelectItemSet(ItemSets.Sets.ElementAt(ItemSets.Sets.Count - 1));
            else
                SelectItemSet(null);

            itemSetChanged(false);
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            ItemSets.Sets.Remove(Selected);
            if (ItemSets.Sets.Count > 0)
                SelectItemSet(ItemSets.Sets.ElementAt(ItemSets.Sets.Count - 1));
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
                Selected.Title = (sender as TextBox).Text;
                Selected.OnChanged("title");
                itemSetChanged(true);
            }
        }

        private void Map_Checked(object sender, RoutedEventArgs e)
        {
            var id = int.Parse((sender as CheckBox).Tag.ToString(), CultureInfo.GetCultureInfo("en-US").NumberFormat);
            if (!Selected.AssociatedMaps.Contains(id))
            {
                if (Selected.AssociatedMaps.Count == 0)
                {
                    Selected.IsGlobalForMaps = false;
                    Selected.OnChanged("isGlobalForMaps");
                }

                Selected.AssociatedMaps.Add(id);
                itemSetChanged(true);
                SortItems();
            }
        }

        private void Map_Unchecked(object sender, RoutedEventArgs e)
        {
            var id = int.Parse((sender as CheckBox).Tag.ToString(), CultureInfo.GetCultureInfo("en-US").NumberFormat);
            Selected.AssociatedMaps.Remove(id);
            itemSetChanged(true);
            SortItems();

            if (Selected.AssociatedMaps.Count == 0)
            {
                Selected.IsGlobalForMaps = true;
                Selected.OnChanged("isGlobalForMaps");
            }
        }

        private void BlockTitle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var txt = (sender as TextBox);
                var itemBlock = txt.Tag as Block;
                itemBlock.BlockType = txt.Text;
                itemBlock.OnChanged("type");
                itemSetChanged(true);
            }
        }

        private void NewItemBlock_Click(object sender, RoutedEventArgs e)
        {
            Selected.Blocks.Add(new Block() { BlockType = "Item block " + (Selected.Blocks.Count() + 1) });
            itemSetChanged(true);
        }

        private void DeleteItemBlock_Click(object sender, RoutedEventArgs e)
        {
            var itemBlock = (sender as Control).Tag as Block;
            if (itemBlock != null)
            {
                Selected.Blocks.Remove(itemBlock);
                itemSetChanged(true);
            }
        }

        private void ChampionRemove_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var champion = (sender as Image).Tag as ChampionData;
            Selected.AssociatedChampions.Remove(champion.Key);
            Selected.Champions.Remove(champion);

            if (Selected.AssociatedChampions.Count == 0)
            {
                Selected.IsGlobalForChampions = true;
                Selected.OnChanged("isGlobalForChampions");
            }

            itemSetChanged(true);
            SortItems();
        }

        private void ChampionAdd_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var champion = (sender as Image).Tag as ChampionData;
            if (!Selected.AssociatedChampions.Contains(champion.Key))
            {
                if (Selected.AssociatedChampions.Count == 0)
                {
                    Selected.IsGlobalForChampions = false;
                    Selected.OnChanged("isGlobalForChampions");
                }

                Selected.AssociatedChampions.Add(champion.Key);
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

                var img = sender as Image;
                drag.Source = img.Source;
                Mouse.OverrideCursor = Cursors.No;
                drag.Visibility = Visibility.Visible;

                Dragged = img.Tag as ItemData;
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
            block.Items.Add(new Item() { Id = int.Parse(Dragged.Id, CultureInfo.GetCultureInfo("en-US").NumberFormat), Count = 1 });

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

                var img = sender as Image;
                drag.Source = img.Source;
                Mouse.OverrideCursor = Cursors.No;
                drag.Visibility = Visibility.Visible;

                var item = img.Tag as Item;
                foreach (var v in Selected.Blocks)
                    v.Items.Remove(item);

                Dragged = Items.Data.Values.FirstOrDefault(s => s.Id.Equals(item.Id + ""));
            }
        }

        private void InsertItem(object sender, MouseButtonEventArgs e)
        {
            if (Dragged == null)
                return;

            var item = (sender as Image).Tag as Item;
            var block = Selected.Blocks.FirstOrDefault(s => s.Items.Contains(item));
            if (block == null)
                return;

            var index = block.Items.IndexOf(item); 

            block.Items.Insert(index, new Item() { Id = int.Parse(Dragged.Id, CultureInfo.GetCultureInfo("en-US").NumberFormat), Count = 1 });

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

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                if (WebClient != null)
                    WebClient.Dispose();
            }
            // free native resources
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
//item tree
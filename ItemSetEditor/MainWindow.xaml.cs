using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Input;

namespace ItemSetEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static string path = "Config\\ItemSets.json";

        public event PropertyChangedEventHandler PropertyChanged;

        public ItemSets ItemSets { get; set; }
        public ItemSet Selected { get; set; }
        public bool IsChanged { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            ReadItemSets();
            if (ItemSets.itemSets.Count > 0)
                SelectItemSet(ItemSets.itemSets.ElementAt(0));

            DataContext = this;
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

            CheckMap11.IsChecked = Selected.associatedMaps.Contains(11);
            CheckMap8.IsChecked = Selected.associatedMaps.Contains(8);
            CheckMap10.IsChecked = Selected.associatedMaps.Contains(10);
            CheckMap12.IsChecked = Selected.associatedMaps.Contains(12);
        }

        private void itemSetChanged(bool enabled)
        {
            IsChanged = enabled;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsChanged"));
        }

        private void ReadItemSets()
        {
            ItemSets = JsonConvert.DeserializeObject<ItemSets>(File.ReadAllText(path));
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
            while(ItemSets.itemSets.FirstOrDefault(s => s.uid == newSet.uid) != null) {
                newSet.uid = CodeGenerator.GenerateUID();
            }
            ItemSets.itemSets.Add(newSet);
            SelectItemSet(newSet);
            itemSetChanged(true);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(ItemSets));
            itemSetChanged(false);
        }

        private void UndoAll_Click(object sender, RoutedEventArgs e)
        {
            ReadItemSets();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ItemSets"));
            itemSetChanged(false);

            if (ItemSets.itemSets.Count > 0)
                SelectItemSet(ItemSets.itemSets.ElementAt(ItemSets.itemSets.Count - 1));
            else
                SelectItemSet(null);
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

        private void Title_TextChanged(object sender, TextChangedEventArgs e)
        {
            itemSetChanged(true);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            itemSetChanged(false);
        }

        private void Title_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                Selected.title = (sender as TextBox).Text;
                Selected.OnChanged("title");
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
            }
        }

        private void Map_Unchecked(object sender, RoutedEventArgs e)
        {
            var id = int.Parse((sender as CheckBox).Tag.ToString());
            Selected.associatedMaps.Remove(id);

            if (Selected.associatedMaps.Count == 0)
            {
                Selected.isGlobalForMaps = true;
                Selected.OnChanged("isGlobalForMaps");
            }
        }
    }
}

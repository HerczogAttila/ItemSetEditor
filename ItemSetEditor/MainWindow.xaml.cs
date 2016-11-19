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

        public ItemSet Selected { get; set; }
        public ItemSets ItemSets { get; set; }
        public bool IsChanged { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            ReadItemSets();
            if (ItemSets.itemSets.Count > 0)
                Selected = ItemSets.itemSets.ElementAt(0);

            DataContext = this;
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

        private void TextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            bool prev = IsChanged;
            Selected = (sender as TextBlock).Tag as ItemSet;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Selected"));
            itemSetChanged(prev);
        }

        private void NewItemSet_Click(object sender, RoutedEventArgs e)
        {
            var newSet = new ItemSet() { title = "Item set " + ItemSets.itemSets.Count };
            while(ItemSets.itemSets.FirstOrDefault(s => s.uid == newSet.uid) != null) {
                newSet.uid = CodeGenerator.GenerateUID();
            }
            ItemSets.itemSets.Add(newSet);
            Selected = newSet;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Selected"));
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
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            ItemSets.itemSets.Remove(Selected);
            if (ItemSets.itemSets.Count > 0)
                Selected = ItemSets.itemSets.ElementAt(ItemSets.itemSets.Count - 1);
            else
                Selected = null;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Selected"));
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
                sets.Focus();
                (sender as Control).Focus();
            }
        }
    }
}
//11:   Summoner's Rift
//8:    The Crystal Scar
//10:   Twisted Treeline
//12:   Howling Abyss
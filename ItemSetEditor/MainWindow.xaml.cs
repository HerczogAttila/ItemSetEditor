using Newtonsoft.Json;
using System.IO;
using System.Windows;

namespace ItemSetEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = JsonConvert.DeserializeObject<ItemSets>(File.ReadAllText("ItemSets.json"));
        }
    }
}

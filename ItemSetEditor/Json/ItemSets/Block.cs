using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ItemSetEditor
{
    public class Block : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string type { get; set; }
        public ObservableCollection<Item> items { get; private set; }

        public Block()
        {
            type = "";
            items = new ObservableCollection<Item>();
        }

        public void OnChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
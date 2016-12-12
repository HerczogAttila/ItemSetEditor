using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ItemSetEditor
{
    /// <summary>
    /// Interaction logic for PageEditor.xaml
    /// </summary>
    public partial class PageEditor : Page
    {
        private DataEditor data;
        private ItemData dragged;

        public PageEditor(DataEditor data)
        {
            InitializeComponent();
            this.data = data;
            DataContext = data;
        }

        private void ItemSet_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            bool prev = data.IsChanged;
            data.SelectItemSet((sender as TextBlock).Tag as ItemSet);
            data.ItemSetChanged(prev);
        }
        private void NewItemSet_Click(object sender, RoutedEventArgs e) { data.AddNewItemSet(); }
        private void Save_Click(object sender, RoutedEventArgs e) { data.SaveItemSets(); }
        private void UndoAll_Click(object sender, RoutedEventArgs e) { data.UndoAll(); }
        private void DeleteSelected_Click(object sender, RoutedEventArgs e) { data.DeleteSelectedItemSet(); }
        private void Title_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                data.Selected.Title = (sender as TextBox).Text;
                data.Selected.OnChanged("title");
                data.ItemSetChanged(true);
            }
        }
        private void Map_Checked(object sender, RoutedEventArgs e)
        {
            var id = int.Parse((sender as CheckBox).Tag.ToString(), CultureInfo.GetCultureInfo("en-US").NumberFormat);
            data.AddMap(id);
        }
        private void Map_Unchecked(object sender, RoutedEventArgs e)
        {
            var id = int.Parse((sender as CheckBox).Tag.ToString(), CultureInfo.GetCultureInfo("en-US").NumberFormat);
            data.RemoveMap(id);
        }
        private void BlockTitle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var txt = (sender as TextBox);
                var itemBlock = txt.Tag as Block;
                itemBlock.BlockType = txt.Text;
                itemBlock.OnChanged("type");
                data.ItemSetChanged(true);
            }
        }
        private void NewItemBlock_Click(object sender, RoutedEventArgs e) { data.AddNewItemBlock(); }
        private void DeleteItemBlock_Click(object sender, RoutedEventArgs e) { data.DeleteItemBlock((sender as Control).Tag as Block); }
        private void ChampionRemove_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { data.RemoveChampion((sender as Image).Tag as ChampionData); }
        private void ChampionAdd_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { data.AddChampion((sender as Image).Tag as ChampionData); }

        #region drag & drop

        private void ItemDrag_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && dragged == null)
            {
                Mouse.AddMouseMoveHandler(this, Item_MouseMove);
                Mouse.AddMouseUpHandler(this, Item_MouseRelease);

                var p = e.GetPosition(this);
                drag.Margin = new Thickness(p.X - 24, p.Y - 24, 0, 0);

                var img = sender as Image;
                drag.Source = img.Source;
                Mouse.OverrideCursor = Cursors.No;
                drag.Visibility = Visibility.Visible;

                dragged = img.Tag as ItemData;
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

            dragged = null;
        }
        private void ItemBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dragged == null)
                return;

            var block = (sender as StackPanel).Tag as Block;
            block.Items.Add(new Item() { Id = int.Parse(dragged.Id, CultureInfo.GetCultureInfo("en-US").NumberFormat), Count = 1 });

            dragged = null;

            data.ItemSetChanged(true);
        }
        private void BlockItemDrag_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragged != null)
                Mouse.OverrideCursor = Cursors.None;

            if (e.LeftButton == MouseButtonState.Pressed && dragged == null)
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
                foreach (var v in data.Selected.Blocks)
                    v.Items.Remove(item);

                dragged = data.Items.Data.Values.FirstOrDefault(s => s.Id.Equals(item.Id + ""));
            }
        }
        private void InsertItem(object sender, MouseButtonEventArgs e)
        {
            if (dragged == null)
                return;

            var item = (sender as Image).Tag as Item;
            var block = data.Selected.Blocks.FirstOrDefault(s => s.Items.Contains(item));
            if (block == null)
                return;

            var index = block.Items.IndexOf(item);

            block.Items.Insert(index, new Item() { Id = int.Parse(dragged.Id, CultureInfo.GetCultureInfo("en-US").NumberFormat), Count = 1 });

            dragged = null;

            data.ItemSetChanged(true);
        }
        private void BlockItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (dragged != null)
                Mouse.OverrideCursor = Cursors.No;
        }
        private void ItemBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            if (dragged != null)
                Mouse.OverrideCursor = Cursors.No;
        }
        private void ItemBlock_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragged != null)
                Mouse.OverrideCursor = Cursors.None;
        }

        #endregion

        private void SortItemName_TextChanged(object sender, TextChangedEventArgs e)
        {
            data.SortItems();
        }
        private void SelectItemTag_Checked(object sender, RoutedEventArgs e)
        {
            data.SortItems();
        }
        private void SortChampionName_TextChanged(object sender, TextChangedEventArgs e)
        {
            data.SortChampions();
        }
        private void SelectChampionTag_Checked(object sender, RoutedEventArgs e)
        {
            data.SortChampions();
        }
    }
}
//item tree
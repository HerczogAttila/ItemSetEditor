﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace ItemSetEditor
{
    public class DataEditor : INotifyPropertyChanged
    {
        private static string SavePath = "Config\\ItemSets.json";

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<ItemData> SortedItems { get; private set; }
        public IEnumerable<ChampionData> SortedChampions { get; private set; }
        public Collection<SortTag> ItemTags { get; private set; }
        public Collection<SortTag> ChampionTags { get; private set; }
        public Config Config { get; set; }
        public Items Items { get; set; }
        public Champions Champions { get; set; }
        public Maps Maps { get; set; }
        public Visibility VisSelected { get; set; }
        public ItemSets ItemSets { get; set; }
        public ItemSet Selected { get; set; }
        public string SortItemName { get; set; }
        public string SortChampionName { get; set; }
        public bool IsChanged { get; set; }

        public DataEditor()
        {
            ChampionTags = new Collection<SortTag>();
            ItemTags = new Collection<SortTag>();
            Config = new Config();
            Items = new Items();
            Champions = new Champions();
            Maps = new Maps();
            VisSelected = Visibility.Collapsed;
            ItemSets = new ItemSets();
            SortItemName = "";
            SortChampionName = "";
            IsChanged = false;
        }

        public void SortItems()
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
        public void SortChampions()
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
        public void SelectItemSet(ItemSet itemSet)
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

            foreach (MapData m in ItemSet.MapIds)
            {
                m.IsChecked = Selected.AssociatedMaps.Contains(m.MapId);
                m.OnChanged("IsChecked");
            }

            SortItems();
        }
        public void ItemSetChanged(bool enabled)
        {
            IsChanged = enabled;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsChanged"));
        }
        public void AddNewItemSet()
        {
            var newSet = new ItemSet() { Title = "Item set " + ItemSets.Sets.Count };
            newSet.Blocks.Add(new Block() { BlockType = "Item block 1" });
            while (ItemSets.Sets.FirstOrDefault(s => s.Id == newSet.Id) != null)
            {
                newSet.Id = CodeGenerator.Generate();
            }
            ItemSets.Sets.Add(newSet);
            SelectItemSet(newSet);
            ItemSetChanged(true);
        }
        public void ReadItemSets()
        {
            if (File.Exists(SavePath))
            {
                ItemSets = JsonConvert.DeserializeObject<ItemSets>(File.ReadAllText(SavePath));
                foreach (ItemSet i in ItemSets.Sets)
                    i.Deserialized(Champions.Data);
            }
            else
                ItemSets = new ItemSets();
        }
        public void SaveItemSets()
        {
            File.WriteAllText(SavePath, JsonConvert.SerializeObject(ItemSets));
            ItemSetChanged(false);
        }
        public void UndoAll()
        {
            ReadItemSets();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ItemSets"));

            if (ItemSets.Sets.Count > 0)
                SelectItemSet(ItemSets.Sets.ElementAt(ItemSets.Sets.Count - 1));
            else
                SelectItemSet(null);

            ItemSetChanged(false);
        }
        public void DeleteSelectedItemSet()
        {
            ItemSets.Sets.Remove(Selected);
            if (ItemSets.Sets.Count > 0)
                SelectItemSet(ItemSets.Sets.ElementAt(ItemSets.Sets.Count - 1));
            else
                SelectItemSet(null);

            ItemSetChanged(true);
        }
        public void AddMap(int id)
        {
            if (!Selected.AssociatedMaps.Contains(id))
            {
                if (Selected.AssociatedMaps.Count == 0)
                {
                    Selected.IsGlobalForMaps = false;
                    Selected.OnChanged("isGlobalForMaps");
                }

                Selected.AssociatedMaps.Add(id);
                ItemSetChanged(true);
                SortItems();
            }
        }
        public void RemoveMap(int id)
        {
            Selected.AssociatedMaps.Remove(id);
            ItemSetChanged(true);
            SortItems();

            if (Selected.AssociatedMaps.Count == 0)
            {
                Selected.IsGlobalForMaps = true;
                Selected.OnChanged("isGlobalForMaps");
            }
        }
        public void AddChampion(ChampionData champion)
        {
            if (champion == null)
                return;

            if (!Selected.AssociatedChampions.Contains(champion.Key))
            {
                if (Selected.AssociatedChampions.Count == 0)
                {
                    Selected.IsGlobalForChampions = false;
                    Selected.OnChanged("isGlobalForChampions");
                }

                Selected.AssociatedChampions.Add(champion.Key);
                Selected.Champions.Add(champion);
                ItemSetChanged(true);
                SortItems();
            }
        }
        public void RemoveChampion(ChampionData champion)
        {
            if (champion == null)
                return;

            Selected.AssociatedChampions.Remove(champion.Key);
            Selected.Champions.Remove(champion);

            if (Selected.AssociatedChampions.Count == 0)
            {
                Selected.IsGlobalForChampions = true;
                Selected.OnChanged("isGlobalForChampions");
            }

            ItemSetChanged(true);
            SortItems();
        }
        public void AddNewItemBlock()
        {
            Selected.Blocks.Add(new Block() { BlockType = "Item block " + (Selected.Blocks.Count() + 1) });
            ItemSetChanged(true);
        }
        public void DeleteItemBlock(Block itemBlock)
        {
            if (itemBlock != null)
            {
                Selected.Blocks.Remove(itemBlock);
                ItemSetChanged(true);
            }
        }

        public void OnChanged(string name) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
    }
}

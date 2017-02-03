﻿using System.IO;
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

            var dirs = new string[] { "Config", "ItemSetEditor" };

#if DEBUG
            dirs = new string[] { @"ItemSetEditor\log", "Config", };
#endif

            foreach (var s in dirs)
                if (!Directory.Exists(s))
                {
#if DEBUG
                    Log.Info("Create directory: " + s);
#endif

                    Directory.CreateDirectory(s);
                }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            Log.Info("Change page: PageLoading");
#endif

            var page = new PageLoading();
            Content = page;
            page.StartLoading(this);
        }
        private void Window_Closed(object sender, System.EventArgs e)
        {
#if DEBUG
            Log.Info("Exit");
            Log.Close();
#endif
        }
    }
}

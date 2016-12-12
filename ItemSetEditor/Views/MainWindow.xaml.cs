﻿using System.Windows;

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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var page = new PageLoading();
            Content = page;
            page.StartLoading(this);
        }
    }
}

﻿using ModAnalyzer.ViewModels;
using System.Windows.Controls;
using System.Windows;
using ModAnalyzer.Domain.Services;

namespace ModAnalyzer.Views {
    /// <summary>
    /// Interaction logic for ClassifyArchivesView.xaml
    /// </summary>
    public partial class ClassifyArchivesView : UserControl {
        public ClassifyArchivesView() {
            InitializeComponent();
        }

        private void UserControl_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Copy;
        }

        private void UserControl_Drop(object sender, DragEventArgs e) {
            string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
            ((ClassifyArchivesViewModel)DataContext).AnalyzeArchives(fileNames);
        }

        private void PackIcon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            ((ClassifyArchivesViewModel)DataContext).Back();
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            string selectedGameName = (e.AddedItems[0] as ComboBoxItem).Content as string;
            GameService.SetGame(selectedGameName);
        }
    }
}

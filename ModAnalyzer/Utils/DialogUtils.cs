using System;
using System.Windows.Forms;

namespace ModAnalyzer.Utils {
    public static class DialogUtils {
        public static string BrowseFolder(string prompt) {
            using (var folderDialog = new FolderBrowserDialog()) {
                folderDialog.Description = prompt;
                DialogResult result = folderDialog.ShowDialog();
                bool noSelection = string.IsNullOrWhiteSpace(folderDialog.SelectedPath);
                if (result == DialogResult.OK && !noSelection) {
                    return folderDialog.SelectedPath;
                }
            }
            return null;
        }

        public static void ShowMessage(string title, string message) {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void ShowError(string title, string message) {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
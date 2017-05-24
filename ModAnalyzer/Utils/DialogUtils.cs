using ModAnalyzer.UIFixes;
using System;
using System.Windows.Forms;

namespace ModAnalyzer.Utils {
    public static class DialogUtils {
        public static string BrowseFolder(string prompt) {
            var folderDialog = new FolderSelectDialog();
            folderDialog.Title = prompt;
            if (folderDialog.ShowDialog(IntPtr.Zero)) {
                bool noSelection = string.IsNullOrWhiteSpace(folderDialog.FileName);
                return noSelection ? null : folderDialog.FileName;
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
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace HaneStory
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer statusTimer;

        private string openedFilename;
        private string openedContent;

        private string findQuery;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!handleIfModified())
            {
                e.Cancel = true;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (Keyboard.Modifiers)
            {
                case ModifierKeys.Control:
                    switch (e.Key)
                    {
                        case Key.F:
                            e.Handled = true;
                            find(true, false);
                            break;
                        case Key.I:
                            e.Handled = true;
                            showInfo();
                            break;
                        case Key.O:
                            e.Handled = true;
                            openFile();
                            break;
                        case Key.S:
                            e.Handled = true;
                            saveFile();
                            break;
                    }
                    break;
                case ModifierKeys.None:
                    switch (e.Key)
                    {
                        case Key.F3:
                            e.Handled = true;
                            find(false, false);
                            break;
                    }
                    break;
                case ModifierKeys.Shift:
                    switch (e.Key)
                    {
                        case Key.F3:
                            e.Handled = true;
                            find(false, true);
                            break;
                    }
                    break;
            }
        }

        private void find(bool reset, bool backward)
        {
            if (reset)
            {
                findQuery = null;
            }

            if (String.IsNullOrEmpty(findQuery))
            {
                FindDialog findDialog = new FindDialog();

                findDialog.Owner = this;

                if (findDialog.ShowDialog() ?? false)
                {
                    findQuery = findDialog.Query;
                }
            }

            if (!String.IsNullOrEmpty(findQuery))
            {
                int index;

                if (backward)
                {
                    if (mainTextBox.SelectionStart == 0)
                    {
                        index = -1;
                    }
                    else
                    {
                        index = mainTextBox.Text.LastIndexOf(findQuery, mainTextBox.SelectionStart - 1, StringComparison.CurrentCultureIgnoreCase);
                    }
                }
                else
                {
                    index = mainTextBox.Text.IndexOf(findQuery, mainTextBox.SelectionStart + mainTextBox.SelectionLength, StringComparison.CurrentCultureIgnoreCase);
                }

                if (index == -1)
                {
                    showStatus(String.Format(Properties.Resources.findFailStatus, findQuery));
                }
                else
                {
                    mainTextBox.Select(index, findQuery.Length);
                }
            }
        }

        private int getWordCount()
        {
            int count = 0;

            int i = 0;

            while (i < mainTextBox.Text.Length)
            {
                while (i < mainTextBox.Text.Length && Char.IsWhiteSpace(mainTextBox.Text[i]))
                {
                    i++;
                }

                count++;

                while (i < mainTextBox.Text.Length && !Char.IsWhiteSpace(mainTextBox.Text[i]))
                {
                    i++;
                }
            }

            return count;
        }

        private bool handleIfModified()
        {
            if (mainTextBox.Text.Length == 0 && openedContent == null)
            {
                return true;
            }

            if (mainTextBox.Text != openedContent)
            {
                MessageBoxResult result = MessageBox.Show(this, Properties.Resources.saveChangesText, string.Empty, MessageBoxButton.YesNoCancel);

                switch (result)
                {
                    case MessageBoxResult.Cancel:
                        return false;
                    case MessageBoxResult.No:
                        return true;
                    case MessageBoxResult.Yes:
                        return saveFile();
                    default:
                        return false;
                }
            }

            return true;
        }

        private bool openFile()
        {
            if (!handleIfModified())
            {
                return false;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog(this) ?? false)
            {
                try
                {
                    string content = File.ReadAllText(openFileDialog.FileName);

                    openedFilename = openFileDialog.FileName;
                    openedContent = content;

                    mainTextBox.IsUndoEnabled = false;
                    mainTextBox.Text = content;
                    mainTextBox.IsUndoEnabled = true;

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show(this, e.Message);
                }
            }

            return false;
        }

        private bool saveFile()
        {
            string saveFilename = openedFilename;

            if (saveFilename == null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();

                if (saveFileDialog.ShowDialog(this) ?? false)
                {
                    saveFilename = saveFileDialog.FileName;
                }
            }

            if (saveFilename != null)
            {
                try
                {
                    File.WriteAllText(saveFilename, mainTextBox.Text);

                    openedFilename = saveFilename;
                    openedContent = mainTextBox.Text;

                    showStatus(String.Format(Properties.Resources.saveStatusSuccess, openedFilename));

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show(this, e.Message);
                }
            }

            return false;
        }

        private void showInfo()
        {
            string filename = openedFilename ?? Properties.Resources.unsavedFilename;
            int wordCount = getWordCount();
            string time = DateTime.Now.ToShortTimeString();

            showStatus(String.Format(Properties.Resources.infoStatus, filename, wordCount, time));
        }

        private void showStatus(string status)
        {
            statusTextBlock.Text = status;
            statusTextBlock.Visibility = Visibility.Visible;

            if (statusTimer == null)
            {
                statusTimer = new DispatcherTimer();
                statusTimer.Interval = TimeSpan.FromSeconds(2.0);
                statusTimer.Tick += (sender, e) =>
                {
                    statusTextBlock.Visibility = Visibility.Hidden;
                    statusTimer.Stop();
                };
            }

            statusTimer.Stop();
            statusTimer.Start();
        }
    }
}

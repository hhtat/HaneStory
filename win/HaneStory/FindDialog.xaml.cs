using System.Windows;
using System.Windows.Input;

namespace HaneStory
{
    public partial class FindDialog : Window
    {
        public string Query
        {
            get
            {
                return findTextBox.Text;
            }
        }

        public FindDialog()
        {
            InitializeComponent();
        }

        private void findButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}

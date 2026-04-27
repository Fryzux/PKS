using System.Windows;
using System.Windows.Controls;
using WpfHttpApp.ViewModels;

namespace WpfHttpApp
{
    public partial class MainWindow : Window
    {
        private MainViewModel VM => (MainViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void PresetGet_Click(object sender, RoutedEventArgs e)
        {
            VM.SelectedMethod = "GET";
            VM.ClientUrl = "https://jsonplaceholder.typicode.com/posts";
        }

        private void PresetLocalStatus_Click(object sender, RoutedEventArgs e)
        {
            VM.SelectedMethod = "GET";
            VM.ClientUrl = $"http://localhost:{VM.Port}/status";
        }

        private void PresetLocalPost_Click(object sender, RoutedEventArgs e)
        {
            VM.SelectedMethod = "POST";
            VM.ClientUrl = $"http://localhost:{VM.Port}/messages";
            VM.RequestBody = "{\n  \"message\": \"Hello from the client!\"\n}";
        }
    }
}
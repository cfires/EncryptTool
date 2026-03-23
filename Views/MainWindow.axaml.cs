using Avalonia.Controls;

namespace EncryptTool.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ContentArea.Content = new AesView();
        }

        private void GoAes(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
         => ContentArea.Content = new AesView();

        private void GoSm4(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => ContentArea.Content = new Sm4View();

        private void GoDes(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => ContentArea.Content = new DesView();

        private void GoBase64(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => ContentArea.Content = new Base64View();
    }
}
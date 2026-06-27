using Avalonia.Controls;
using EncryptTool.Services;
using System;
using System.Threading.Tasks;

namespace EncryptTool.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ContentArea.Content = new AesView();
            Opened += MainWindowOpened;
        }

        private async void MainWindowOpened(object? sender, EventArgs e)
        {
            Opened -= MainWindowOpened;
            await CheckForUpdatesAsync();
        }

        private async Task CheckForUpdatesAsync()
        {
            UpdateCheckResult result = await UpdateService.CheckForUpdateAsync();
            if (!result.HasUpdate || result.Manifest is null)
                return;

            var prompt = new UpdatePromptWindow(result);
            await prompt.ShowDialog(this);
        }

        private void GoAes(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
         => ContentArea.Content = new AesView();

        private void GoSm4(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => ContentArea.Content = new Sm4View();

        private void GoSm2(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => ContentArea.Content = new Sm2View();

        private void GoBase64(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            => ContentArea.Content = new Base64View();
    }
}

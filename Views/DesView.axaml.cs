using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EncryptTool.Services;

namespace EncryptTool;

public partial class DesView : UserControl
{
    public DesView()
    {
        InitializeComponent();
    }
    private void Encrypt(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        TxtOutput.Text = DesHelper.Encrypt(TxtInput.Text!, TxtKey.Text!);
    }

    private void Decrypt(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        TxtOutput.Text = DesHelper.Decrypt(TxtInput.Text!, TxtKey.Text!);
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EncryptTool.Services;

namespace EncryptTool;

public partial class Base64View : UserControl
{
    public Base64View()
    {
        InitializeComponent();
    }

    private void Encode(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        TxtOutput.Text = Base64Helper.Encode(TxtInput.Text!);
    }

    private void Decode(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        TxtOutput.Text = Base64Helper.Decode(TxtOutput.Text!);
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EncryptTool.Services;
using System;

namespace EncryptTool;

public partial class Base64View : UserControl
{
    public Base64View()
    {
        InitializeComponent();
    }

    private void Encode(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            TxtOutput.Text = Base64Helper.Encode(TxtInput.Text ?? string.Empty);
        }
        catch (Exception ex)
        {
            TxtOutput.Text = $"编码失败：{ex.Message}";
        }
    }

    private void Decode(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            TxtOutput.Text = Base64Helper.Decode(TxtInput.Text ?? string.Empty);
        }
        catch (Exception ex)
        {
            TxtOutput.Text = $"解码失败：{ex.Message}";
        }
    }
}

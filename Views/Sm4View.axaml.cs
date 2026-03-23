using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EncryptTool.Services;
using EncryptTool.ViewModels;
using System;

namespace EncryptTool;

public partial class Sm4View : UserControl
{
    public Sm4View()
    {
        InitializeComponent();
        DataContext = new Sm4ViewModel();
    }
}
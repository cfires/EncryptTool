using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using EncryptTool.Services;
using EncryptTool.ViewModels;
using System;

namespace EncryptTool;

public partial class AesView : UserControl
{
    public AesView()
    {
        InitializeComponent();
        DataContext = new AesViewModel();
    }
}
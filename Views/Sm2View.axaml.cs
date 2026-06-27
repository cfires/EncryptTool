using Avalonia.Controls;
using EncryptTool.ViewModels;

namespace EncryptTool;

public partial class Sm2View : UserControl
{
    public Sm2View()
    {
        InitializeComponent();
        DataContext = new Sm2ViewModel();
    }
}

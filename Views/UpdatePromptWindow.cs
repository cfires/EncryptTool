using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using EncryptTool.Services;
using System;

namespace EncryptTool.Views
{
    public sealed class UpdatePromptWindow : Window
    {
        private readonly UpdateCheckResult _result;

        public UpdatePromptWindow(UpdateCheckResult result)
        {
            _result = result.Manifest is null
                ? throw new ArgumentException("缺少更新清单", nameof(result))
                : result;

            Title = "发现新版本";
            Width = 460;
            Height = 330;
            MinWidth = 440;
            MinHeight = 310;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            CanResize = false;

            Content = BuildContent(result);
        }

        private Control BuildContent(UpdateCheckResult result)
        {
            string packageText = result.MatchedPackage is null
                ? $"当前系统：{result.CurrentRid}，未找到完全匹配的安装包，将打开发布页。"
                : $"当前系统：{result.CurrentRid}，安装包：{result.MatchedPackage.FileName ?? result.MatchedPackage.Rid}";

            var title = new TextBlock
            {
                Text = $"发现新版本 {result.LatestVersion}",
                FontSize = 20,
                FontWeight = FontWeight.Bold,
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            };

            var version = new TextBlock
            {
                Text = $"当前版本：{result.CurrentVersion}",
                FontSize = 14,
                Margin = new Avalonia.Thickness(0, 0, 0, 6)
            };

            var package = new TextBlock
            {
                Text = packageText,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(0, 0, 0, 12)
            };

            var notes = new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(result.Manifest!.ReleaseNotes)
                    ? "建议更新到最新版本，以获得修复和改进。"
                    : result.Manifest.ReleaseNotes,
                TextWrapping = TextWrapping.Wrap
            };

            var updateButton = new Button
            {
                Content = result.MatchedPackage is null ? "打开发布页" : "下载安装包",
                Width = 110,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            updateButton.Click += (_, _) =>
            {
                UpdateService.OpenDownloadPage(_result);
                Close();
            };

            var laterButton = new Button
            {
                Content = result.Manifest.ForceUpdate ? "关闭" : "稍后再说",
                Width = 100,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            laterButton.Click += (_, _) => Close();

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 10,
                Children =
                {
                    laterButton,
                    updateButton
                }
            };

            var grid = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto,Auto,Auto,*,Auto"),
                Margin = new Avalonia.Thickness(22),
                Children =
                {
                    title,
                    version,
                    package,
                    notes,
                    buttons
                }
            };

            for (int i = 0; i < grid.Children.Count; i++)
            {
                Grid.SetRow(grid.Children[i], i);
            }

            return grid;
        }
    }
}

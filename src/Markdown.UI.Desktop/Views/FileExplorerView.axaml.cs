using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

using Markdown.UI.Desktop.Models;
using Markdown.UI.Desktop.ViewModels;

namespace Markdown.UI.Desktop.Views;

/// <summary>
/// File explorer view displaying the workspace file tree.
/// Handles folder expansion and file opening.
/// </summary>
internal partial class FileExplorerView : UserControl
{
    private FileExplorerViewModel? _viewModel;

    public FileExplorerView()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;

        // Handle double-click on tree items
        FileTree.DoubleTapped += OnFileTreeDoubleTapped;

        // Handle tree item expansion
        FileTree.AddHandler(TreeViewItem.ExpandedEvent, OnTreeItemExpanded, RoutingStrategies.Bubble);

        // Handle single click on tree items to toggle expansion
        FileTree.AddHandler(PointerPressedEvent, OnTreeItemPointerPressed, RoutingStrategies.Tunnel);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        _viewModel = DataContext as FileExplorerViewModel;
    }

    private void OnTreeItemPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Find the TreeViewItem that was clicked
        var source = e.Source as Control;
        var treeViewItem = source?.FindAncestorOfType<TreeViewItem>();

        if (treeViewItem?.DataContext is FileTreeNode node && node.IsFolder)
        {
            // Don't toggle if clicking on the expander button itself (let it handle that)
            if (source is ToggleButton)
            {
                return;
            }

            // Toggle expansion on single click for folders
            treeViewItem.IsExpanded = !treeViewItem.IsExpanded;
            e.Handled = true;
        }
    }

    private void OnFileTreeDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        if (FileTree.SelectedItem is FileTreeNode node && !node.IsFolder)
        {
            // Open the file
            _viewModel.OpenFileCommand.Execute(node);
        }
    }

    private void OnTreeItemExpanded(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        if (e.Source is TreeViewItem treeViewItem &&
            treeViewItem.DataContext is FileTreeNode node &&
            node.IsFolder)
        {
            // Load folder contents when expanded
            _viewModel.ExpandFolderCommand.Execute(node);
        }
    }
}

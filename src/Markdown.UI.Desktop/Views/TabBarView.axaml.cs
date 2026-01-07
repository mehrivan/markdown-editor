using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

using Markdown.UI.Desktop.ViewModels;

namespace Markdown.UI.Desktop.Views;

/// <summary>
/// Tab bar view displaying open document tabs.
/// Handles tab selection on click and active tab styling.
/// </summary>
internal partial class TabBarView : UserControl
{
    private MainWindowViewModel? _viewModel;

    public TabBarView()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;

        // Handle tab selection via pointer pressed
        AddHandler(PointerPressedEvent, OnPointerPressed, handledEventsToo: true);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _viewModel = DataContext as MainWindowViewModel;

        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            UpdateActiveTabStyling();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.ActiveTab))
        {
            UpdateActiveTabStyling();
        }
    }

    private void UpdateActiveTabStyling()
    {
        if (_viewModel is null)
        {
            return;
        }

        // Find all tab borders and update their active class
        foreach (var container in TabsItemsControl.GetLogicalChildren())
        {
            if (container is ContentPresenter presenter)
            {
                var border = presenter.GetVisualDescendants().OfType<Border>().FirstOrDefault();
                if (border is not null)
                {
                    var tabVm = border.DataContext as TabViewModel;
                    if (tabVm == _viewModel.ActiveTab)
                    {
                        if (!border.Classes.Contains("active"))
                        {
                            border.Classes.Add("active");
                        }
                    }
                    else
                    {
                        border.Classes.Remove("active");
                    }
                }
            }
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Find the clicked tab
        var source = e.Source as Control;
        var tabBorder = source?.FindAncestorOfType<Border>();

        if (tabBorder?.DataContext is TabViewModel clickedTab && _viewModel is not null)
        {
            // Set the clicked tab as active
            _viewModel.ActiveTab = clickedTab;
        }
    }
}

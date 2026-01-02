namespace Markdown.UI.Desktop.ViewModels;

/// <summary>
/// Base class for all ViewModels in the application.
/// Inherits from ObservableObject to provide INotifyPropertyChanged implementation.
/// </summary>
internal abstract partial class ViewModelBase : ObservableObject
{
    /// <summary>
    /// Indicates whether the ViewModel is currently busy performing an operation.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    /// <summary>
    /// Indicates whether the ViewModel is not busy (inverse of IsBusy).
    /// Useful for enabling/disabling UI elements.
    /// </summary>
    public bool IsNotBusy => !IsBusy;

    /// <summary>
    /// Sets the busy state to true.
    /// </summary>
    protected void SetBusy() => IsBusy = true;

    /// <summary>
    /// Clears the busy state (sets to false).
    /// </summary>
    protected void ClearBusy() => IsBusy = false;
}

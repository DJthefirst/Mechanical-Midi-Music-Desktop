using AvaloniaGUI.Data;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaGUI.ViewModels;

public partial class ComponentViewModel : ViewModelBase
{
    [ObservableProperty]
    private PageComponentNames _pageName;

    protected ComponentViewModel(PageComponentNames pageName)
    {
        _pageName = pageName;
    }
}
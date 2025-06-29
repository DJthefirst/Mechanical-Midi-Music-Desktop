using AvaloniaGUI.Data;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaGUI.ViewModels;

public partial class PageViewModel : ViewModelBase
{
    [ObservableProperty]
    private ApplicationPageNames _pageName;

    protected PageViewModel(ApplicationPageNames pageName)
    {
        _pageName = pageName;
    }
}
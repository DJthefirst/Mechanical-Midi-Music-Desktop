using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaGUI.ViewModels;

namespace AvaloniaGUI.Views;

public partial class DistributorListView : UserControl
{
	public DistributorListView()
	{
		InitializeComponent();

		//MutedSelector.SelectionChanged += (s, e) =>
		//{
		//	(DataContext as DistributorListViewModel)?.UpdateMutedDistributors();
		//};
	}
}
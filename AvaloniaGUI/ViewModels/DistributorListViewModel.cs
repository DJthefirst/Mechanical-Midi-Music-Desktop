using AvaloniaGUI.Data;
using AvaloniaGUI.Factories;
using AvaloniaGUI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MMM_Core;
using MMM_Device;
using MMM_Server;
using System;
using System.Collections.ObjectModel;

namespace AvaloniaGUI.ViewModels;

public partial class DistributorListViewModel : ComponentViewModel
{
	public SessionContext Context => SessionContext.Instance;

	public DistributorListViewModel() : base(PageComponentNames.DeviceList)
	{

		Context.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == nameof(Context.SelectedDevice))
			{
				SelectedDevice = Context.SelectedDevice;
				DistributorList.Clear();
				if (Context.SelectedDevice is null) return;
				
				var distributors = Context.SelectedDevice.Distributors;
				foreach (var distributor in distributors)
				{
					DistributorList.Add(distributor);
				}
			}
		};
	}

	public ObservableCollection<Distributor> DistributorList { get; } = new();

	[ObservableProperty]
	private Device? _selectedDevice;

	[ObservableProperty]
	private Distributor? _selectedDistributor;

	partial void OnSelectedDistributorChanged(Distributor? value)
	{
		Context.SelectedDistributor = value;
	}
}


public partial class DesignDistributorListViewModel : DistributorListViewModel
{
	public DesignDistributorListViewModel()
		: base()
	{
		byte[] Distributor0 = [0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x01, 0x7F, 0x00, 0x01, 0x00, 0x01, 0x7F, 0x01, 0x00];
		byte[] Distributor1 = [0x00, 0x01, 0x00, 0x00, 0x04, 0x00, 0x00, 0x7F, 0x7F, 0x00, 0x01, 0x00, 0x01, 0x7F, 0x01, 0x00];

		SelectedDevice = new Device();
		for (int i = 0; i < 5; i++)
		{
			var distributor0 = new Distributor(Distributor0);
			var distributor1 = new Distributor(Distributor0);
			SelectedDevice.Distributors.Add(distributor0);
			SelectedDevice.Distributors.Add(distributor1);
			DistributorList.Add(distributor0);
			DistributorList.Add(distributor1);
		}
		SelectedDistributor = DistributorList[0];
	}
}
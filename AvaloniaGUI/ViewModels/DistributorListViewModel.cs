using Avalonia.Metadata;
using Avalonia.Threading;
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

	private DeviceEntry? _lastSelectedDeviceEntry;
	private event EventHandler? _distributorsChangedHandler;

	public DistributorListViewModel() : base(PageComponentNames.DeviceList)
	{
		// Initialize SelectedDevice and DistributorList if a device is already selected
		DeviceManager.Instance.OnDeviceChanged += (s, e) =>
		{
			Dispatcher.UIThread.Post(() =>
			{
				updateDistributors();
			});
		};

		Context.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == nameof(Context.SelectedDevice))
			{
				updateDistributors();

				Console.WriteLine("Log1");

				// Unsubscribe from previous device's distributors changed event
				if (_lastSelectedDeviceEntry?.Device.Distributors is { } oldDistributors)
				{
					oldDistributors.CollectionChanged -= OnDistributorsChangedHandler;
					Console.WriteLine("Log2");
				}

				// Subscribe to new device's distributors changed event
				_lastSelectedDeviceEntry = Context.SelectedDevice;
				if (_lastSelectedDeviceEntry?.Device.Distributors is { } newDistributors)
				{
					newDistributors.CollectionChanged += OnDistributorsChangedHandler;
					Console.WriteLine("Log3");
				}
			}
		};
	}


	private void OnDistributorsChangedHandler(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	{
		updateDistributors();
		Console.WriteLine("Log4");
	}
	private void updateDistributors()
	{
		SelectedDevice = Context.SelectedDevice;

		DistributorList.Clear();
		if (Context.SelectedDevice is null) return;

		var distributors = Context.SelectedDevice?.Device.Distributors;
		if (distributors != null)
		{
			foreach (var distributor in distributors)
			{
				DistributorList.Add(distributor);
			}
		}

		if (Context.SelectedDevice?.Device.NumDistributors != DistributorList.Count) return;
		if (Context.SelectedDistributor?.Index is int idx && idx >= 0 && idx < DistributorList.Count)
		{
			SelectedDistributor = DistributorList[idx];
			Context.SelectedDistributor = SelectedDistributor;
		}
		else if (DistributorList.Count > 0)
		{
			SelectedDistributor = DistributorList[DistributorList.Count - 1];
			Context.SelectedDistributor = SelectedDistributor;
		}
		else
		{
			//SelectedDistributor = null;
		}
	}

	public ObservableCollection<Distributor> DistributorList { get; } = new();

	[ObservableProperty]
	private DeviceEntry? _selectedDevice;

	[ObservableProperty]
	private Distributor? _selectedDistributor;

	partial void OnSelectedDistributorChanged(Distributor? value)
	{
		if(value != null)
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

		SelectedDevice = new DeviceEntry();
		for (int i = 0; i < 5; i++)
		{
			var distributor0 = new Distributor(Distributor0);
			var distributor1 = new Distributor(Distributor0);
			SelectedDevice?.Device.Distributors.Add(distributor0);
			SelectedDevice?.Device.Distributors.Add(distributor1);
			DistributorList.Add(distributor0);
			DistributorList.Add(distributor1);
		}
		SelectedDistributor = DistributorList[0];

	}
}
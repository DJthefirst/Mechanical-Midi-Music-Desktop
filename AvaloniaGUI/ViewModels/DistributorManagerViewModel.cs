using AvaloniaGUI.Data;
using AvaloniaGUI.Factories;
using AvaloniaGUI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MMM_Core;
using MMM_Device;
using System;
using System.Xml.Linq;

namespace AvaloniaGUI.ViewModels;

public partial class DistributorManagerViewModel : ComponentViewModel
{
	public SessionContext Context => SessionContext.Instance;

	public DistributorManagerViewModel() : base(PageComponentNames.DistributorManager)
	{
		Context.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == nameof(Context.SelectedDistributor))
			{
				SelectedDistributor = Context.SelectedDistributor;
				Channels = (SelectedDistributor?.Channels ?? 0);
				Instruments = (SelectedDistributor?.Instruments ?? 0);
				Distribution = SelectedDistributor?.DistributionMethod ?? DistributionMethod.RoundRobin;
				Damper = SelectedDistributor?.DamperPedal ?? false;
				Polyphonic = SelectedDistributor?.Polyphonic ?? false;
				NoteOverwrite = SelectedDistributor?.NoteOverwrite ?? false;
				MinNote = SelectedDistributor?.MinNote ?? 0;
				MaxNote = SelectedDistributor?.MaxNote ?? 127;
				NumPolyphonicNotes = SelectedDistributor?.NumPolyphonicNotes ?? 1;
			}
		};

	}

	[ObservableProperty]
	private DistributionMethod[] _distributionMethods = (DistributionMethod[])Enum.GetValues(typeof(DistributionMethod));

	[ObservableProperty]
	private Distributor? _selectedDistributor;

	[ObservableProperty]
	private int _channels = 0;

	[ObservableProperty]
	private int _instruments = 0;

	[ObservableProperty]
	private DistributionMethod _distribution = DistributionMethod.RoundRobin;

	[ObservableProperty]
	private bool _damper = false;

	[ObservableProperty]
	private bool _polyphonic = false;

	[ObservableProperty]
	private bool _noteOverwrite = false;

	[ObservableProperty]
	private int _minNote = 0;

	[ObservableProperty]
	private int _maxNote = 127;

	[ObservableProperty]
	private int _numPolyphonicNotes = 1;

	[RelayCommand]
	public void Update()
	{
		Device? device = Context.SelectedDevice?.Device;
		IConnection? connection = device != null ? DeviceManager.Instance.GetConnection(device) : null;
		if (device == null || connection == null || SelectedDistributor == null) return;

		Distributor distributor = new Distributor();
		distributor.Index = SelectedDistributor.Index;
		distributor.Channels = Channels;
		distributor.Instruments = Instruments;
		distributor.DistributionMethod = Distribution;
		distributor.DamperPedal = Damper;
		distributor.Polyphonic = Polyphonic;
		distributor.NoteOverwrite = NoteOverwrite;
		distributor.MinNote = MinNote;
		distributor.MaxNote = MaxNote;
		distributor.NumPolyphonicNotes = NumPolyphonicNotes;

		MMM_Msg msg = MMM_Msg.GenerateSysEx(
			device.SYSEX_DEV_ID,
			SysEx.SetDistributorConstruct,
			distributor.ToSerial());
		DeviceManager.Instance.Update(connection, msg.ToMidiEvent());

		msg = MMM_Msg.GenerateSysEx(device.SYSEX_DEV_ID, SysEx.GetDeviceConstruct, []);
		DeviceManager.Instance.Update(connection, msg.ToMidiEvent());
	}

	[RelayCommand]
	public void Add()
	{
		Device? device = Context.SelectedDevice?.Device;
		IConnection? connection = Context.SelectedDevice?.Connection;
		if (device == null || connection == null) return;

		Distributor distributor = new Distributor();
		distributor.Index = device.Distributors.Count;
		distributor.Channels = Channels;
		distributor.Instruments = Instruments;
		distributor.DistributionMethod = Distribution;
		distributor.DamperPedal = Damper;
		distributor.Polyphonic = Polyphonic;
		distributor.NoteOverwrite = NoteOverwrite;
		distributor.MinNote = MinNote;
		distributor.MaxNote = MaxNote;
		distributor.NumPolyphonicNotes = NumPolyphonicNotes;

		MMM_Msg msg = MMM_Msg.GenerateSysEx(
			device.SYSEX_DEV_ID,
			SysEx.AddDistributor,
			distributor.ToSerial()
		);
		DeviceManager.Instance.Update(connection, msg.ToMidiEvent());

		msg = MMM_Msg.GenerateSysEx(device.SYSEX_DEV_ID, SysEx.GetDeviceConstruct, []);
		DeviceManager.Instance.Update(connection, msg.ToMidiEvent());
	}

	[RelayCommand]
	public void Remove()
	{
		Device? device = Context.SelectedDevice?.Device;
		IConnection? connection = Context.SelectedDevice?.Connection;
		if (device == null || connection == null || SelectedDistributor == null) return;

		// Fix: Convert int? to byte safely, defaulting to 0 if null
		int index = SelectedDistributor.Index ?? 0;

		MMM_Msg msg = MMM_Msg.GenerateSysEx(
		device.SYSEX_DEV_ID,
		SysEx.RemoveDistributor,
		[(byte)((index >> 7) & 0x7F), (byte)((index >> 0) & 0x7F)]
		);
		DeviceManager.Instance.Update(connection, msg.ToMidiEvent());

		device.Distributors.Clear();
		msg = MMM_Msg.GenerateSysEx(device.SYSEX_DEV_ID, SysEx.GetDeviceConstruct, []);
		DeviceManager.Instance.Update(connection, msg.ToMidiEvent());
	}

	[RelayCommand]
	public void Clear()
	{
		Device? device = Context.SelectedDevice?.Device;
		IConnection? connection = Context.SelectedDevice?.Connection;
		if (device == null || connection == null) return;
		MMM_Msg msg = MMM_Msg.GenerateSysEx(device.SYSEX_DEV_ID, SysEx.RemoveAllDistributors, []);
		DeviceManager.Instance.Update(connection, msg.ToMidiEvent());

		device.Distributors.Clear();
		msg = MMM_Msg.GenerateSysEx(device.SYSEX_DEV_ID, SysEx.GetDeviceConstruct, []);
		DeviceManager.Instance.Update(connection, msg.ToMidiEvent());
	}
}



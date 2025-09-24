using AvaloniaGUI.Data;
using AvaloniaGUI.Factories;
using AvaloniaGUI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MMM_Core;
using MMM_Device;
using System;
using System.Xml.Linq;
using static SkiaSharp.HarfBuzz.SKShaper;

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
				StrChannels = NumberToListedItems(SelectedDistributor?.Channels ?? 0);
				StrInstruments = NumberToListedItems(SelectedDistributor?.Instruments ?? 0);
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

	[NotifyPropertyChangedFor(nameof(Channels))]
	[ObservableProperty]
	private string _strChannels = "";
	private int Channels => UpdateChannels();

	[NotifyPropertyChangedFor(nameof(Instruments))]
	[ObservableProperty]
	private string _strInstruments = "";
	private int Instruments => UpdateInstruments();

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


	public int UpdateChannels()
	{
		int result = Math.Clamp(ListedItemsToNumber(StrChannels), 0, 0xFFFF);
		StrChannels = NumberToListedItems(result);
		Console.WriteLine($"Parsed Channels: {StrChannels} -> {result}");
		return result;
	}
	public int UpdateInstruments()
	{
		int result = ListedItemsToNumber(StrInstruments); //TODO Fix result size uint32
		StrInstruments = NumberToListedItems(result);
		Console.WriteLine($"Parsed Instruments: {StrInstruments} -> {result}");
		return result;
	}

	public static int ListedItemsToNumber(object? input)
	{
		if (input is int x)
			return x;
		if (input is not string s || string.IsNullOrWhiteSpace(s))
			return 0;

		// Handle hex, binary, or bitwise expressions
		if (System.Text.RegularExpressions.Regex.IsMatch(s, @"0x|0b|[|&]", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
		{
			try
			{
				if (!System.Text.RegularExpressions.Regex.IsMatch(s, @"^[\d\s|&xob]+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
					return 0;
				s = System.Text.RegularExpressions.Regex.Replace(s, @"0b([01]+)", m => Convert.ToInt32(m.Groups[1].Value, 2).ToString());
				var eval = new System.Data.DataTable().Compute(s, "");
				return Convert.ToInt32(eval);
			}
			catch { return 0; }
		}

		int result = 0;
		var rangeRegex = new System.Text.RegularExpressions.Regex(@"(\d+)\s*-\s*(\d+)", System.Text.RegularExpressions.RegexOptions.Compiled);
		var numRegex = new System.Text.RegularExpressions.Regex(@"(?<=^|[^-\s\d]|\d\s)\s*(\d+)\s*(?=[^-\s\d]|\s\d|$)", System.Text.RegularExpressions.RegexOptions.Compiled);

		foreach (System.Text.RegularExpressions.Match m in rangeRegex.Matches(s))
		{
			if (int.TryParse(m.Groups[1].Value, out int start) &&
				int.TryParse(m.Groups[2].Value, out int end) &&
				start > 0 && end > 0 && start <= end && end <= 32)
			{
				for (int i = start; i <= end; i++)
					result |= 1 << (i - 1);
			}
		}

		foreach (System.Text.RegularExpressions.Match m in numRegex.Matches(s))
		{
			if (int.TryParse(m.Groups[1].Value, out int n) && n > 0 && n <= 32)
				result |= 1 << (n - 1);
		}

		return result;
	}

	/// <summary>
	/// Converts a bitmask integer into a human-readable string listing the set bits as ranges or single values.
	/// For example, 0b1011 (11) becomes "1-2,4".
	/// </summary>
	/// <param name="num">The bitmask integer to convert.</param>
	/// <returns>A string representing the set bits as a comma-separated list and ranges.</returns>
	public static string NumberToListedItems(int num)
	{
		if (num == 0) return string.Empty; // No bits set, return empty string.
		var result = new System.Text.StringBuilder();
		int i = 1; // Bit position (1-based)
		while (num != 0)
		{
			// Skip unset bits
			if ((num & 1) == 0)
			{
				num >>= 1;
				i++;
				continue;
			}
			int start = i; // Start of a run of set bits
						   // Find the end of the run of consecutive set bits
			while (((num >> 1) & 1) == 1)
			{
				num >>= 1;
				i++;
			}
			int end = i; // End of the run
			if (result.Length > 0) result.Append(','); // Add comma if not the first range
													   // Append either a single value or a range
			result.Append(start == end ? $"{start}" : $"{start}-{end}");
			num >>= 1;
			i++;
		}
		return result.ToString();
	}

}



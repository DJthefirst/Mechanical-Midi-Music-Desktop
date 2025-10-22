using AvaloniaGUI.Data;
using AvaloniaGUI.Factories;
using AvaloniaGUI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MMM_Core;
using MMM_Device;
using System;

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
				Distribution = SelectedDistributor?.Method ?? DistributionMethod.RoundRobin;
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
		if (Context.SelectedDevice is not DeviceEntry deviceEntry || SelectedDistributor == null) return;

		Distributor distributor = new Distributor();
		distributor.Index = SelectedDistributor.Index;
		distributor.Channels = Channels;
		distributor.Instruments = Instruments;
		distributor.Method = Distribution;
		distributor.Muted = SelectedDistributor.Muted;
		distributor.DamperPedal = Damper;
		distributor.Polyphonic = Polyphonic;
		distributor.NoteOverwrite = NoteOverwrite;
		distributor.MinNote = MinNote;
		distributor.MaxNote = MaxNote;
		distributor.NumPolyphonicNotes = NumPolyphonicNotes;

		MMM_Msg msg = MMM_Msg.GenerateSysEx(
			deviceEntry.Id,
			SysEx.SetDistributorConstruct,
			distributor.ToSerial());
		deviceEntry.Connection.SendEvent(msg.ToMidiEvent());

		msg = MMM_Msg.GenerateSysEx(deviceEntry.Id, SysEx.GetNumOfDistributors, []);
		deviceEntry.Connection.SendEvent(msg.ToMidiEvent());
	}

	[RelayCommand]
	public void Add()
	{
		if (Context.SelectedDevice is not DeviceEntry deviceEntry) return;

		Distributor distributor = new Distributor();
		distributor.Index = deviceEntry.Device.Distributors.Count;
		distributor.Channels = Channels;
		distributor.Instruments = Instruments;
		distributor.Method = Distribution;
		distributor.Muted = false;
		distributor.DamperPedal = Damper;
		distributor.Polyphonic = Polyphonic;
		distributor.NoteOverwrite = NoteOverwrite;
		distributor.MinNote = MinNote;
		distributor.MaxNote = MaxNote;
		distributor.NumPolyphonicNotes = NumPolyphonicNotes;

		deviceEntry.Device.NumDistributors += 1;
		Context.SelectedDistributor = distributor;

		MMM_Msg msg = MMM_Msg.GenerateSysEx(
			deviceEntry.Id,
			SysEx.AddDistributor,
			distributor.ToSerial());
		deviceEntry.Connection.SendEvent(msg.ToMidiEvent());

		msg = MMM_Msg.GenerateSysEx(deviceEntry.Id, SysEx.GetNumOfDistributors, []);
		deviceEntry.Connection.SendEvent(msg.ToMidiEvent());
	}

	[RelayCommand]
	public void Remove()
	{
		if (Context.SelectedDevice is not DeviceEntry deviceEntry || SelectedDistributor == null) return;

		// Fix: Convert int? to byte safely, defaulting to 0 if null
		int index = SelectedDistributor.Index ?? 0;

		MMM_Msg msg = MMM_Msg.GenerateSysEx(
			deviceEntry.Id,
			SysEx.RemoveDistributor,
			[(byte)((index >> 7) & 0x7F), (byte)((index >> 0) & 0x7F)]);
		deviceEntry.Connection.SendEvent(msg.ToMidiEvent());

		msg = MMM_Msg.GenerateSysEx(deviceEntry.Id, SysEx.GetNumOfDistributors, []);
		deviceEntry.Connection.SendEvent(msg.ToMidiEvent());
	}

	[RelayCommand]
	public void Clear()
	{
		if (Context.SelectedDevice is not DeviceEntry deviceEntry || SelectedDistributor == null) return;

		MMM_Msg msg = MMM_Msg.GenerateSysEx(deviceEntry.Id, SysEx.RemoveAllDistributors, []);
		deviceEntry.Connection.SendEvent(msg.ToMidiEvent());

		msg = MMM_Msg.GenerateSysEx(deviceEntry.Id, SysEx.GetNumOfDistributors, []);
		deviceEntry.Connection.SendEvent(msg.ToMidiEvent());
	}

	public int UpdateChannels()
	{
		int result = Math.Clamp(ListedItemsToNumber(StrChannels, 16), 0, 0xFFFF);
		StrChannels = NumberToListedItems(result);
		return result;
	}
	public int UpdateInstruments()
	{
		int result = ListedItemsToNumber(StrInstruments, 32); //TODO Fix result size uint32
		StrInstruments = NumberToListedItems(result);
		return result;
	}

	/// <summary>
	/// Converts a user input (string or int) representing a set of items (channels/instruments)
	/// into a bitmask integer. Supports ranges (e.g. "1-3"), individual numbers (e.g. "5"),
	/// and bitwise/hex/binary expressions (e.g. "0xF", "0b1011", "1|2|4").
	/// </summary>
	/// <param name="input">The input to parse (string or int).</param>
	/// <returns>An integer bitmask representing the selected items.</returns>
	public static int ListedItemsToNumber(object? input, int maxItems)
	{
		// Build mask for allowed bits
		uint mask = (maxItems == 32) ? 0xFFFFFFFFu : ((1u << maxItems) - 1u);

		// If input is already an int, apply mask and return
		if (input is int x)
			return (int)((uint)x & mask);

		// If input is not a string or is empty/whitespace, return 0
		if (input is not string s || string.IsNullOrWhiteSpace(s))
			return 0;

		// Handle hex, binary, or bitwise expressions (e.g. "0xF", "0b1011", "1|2|4")
		if (System.Text.RegularExpressions.Regex.IsMatch(s, @"0x|0b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
		{
			try
			{
				// Only allow valid characters for safety
				if (!System.Text.RegularExpressions.Regex.IsMatch(s, @"^[\d\s|&xobXa-fA-F]+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
					return 0;
				// Convert binary literals (e.g. "0b1011") to decimal
				s = System.Text.RegularExpressions.Regex.Replace(s, @"0b([01]+)", m => Convert.ToInt32(m.Groups[1].Value, 2).ToString());
				// Convert hex literals (e.g. "0xFF") to decimal
				s = System.Text.RegularExpressions.Regex.Replace(s, @"0x([0-9A-Fa-f]+)", m => Convert.ToInt32(m.Groups[1].Value, 16).ToString());
				// Evaluate the expression using DataTable.Compute
				var eval = new System.Data.DataTable().Compute(s, "");
				long val = Convert.ToInt64(eval);
				return (int)((uint)val & mask);
			}
			catch { return 0; }
		}

		int result = 0;
		// Regex to match ranges like "1-3"
		var rangeRegex = new System.Text.RegularExpressions.Regex(@"(\d+)\s*-\s*(\d+)", System.Text.RegularExpressions.RegexOptions.Compiled);
		// Regex to match individual numbers not part of a range
		var numRegex = new System.Text.RegularExpressions.Regex(@"(?<=^|[^-\s\d]|\d\s)\s*(\d+)\s*(?=[^-\s\d]|\s\d|$)", System.Text.RegularExpressions.RegexOptions.Compiled);

		// Parse and set bits for all ranges found (clamped to allowed range)
		foreach (System.Text.RegularExpressions.Match m in rangeRegex.Matches(s))
		{
			if (int.TryParse(m.Groups[1].Value, out int start) &&
				int.TryParse(m.Groups[2].Value, out int end) &&
				start > 0 && end > 0 && start <= end)
			{
				// Clamp range to [1, maxItems]
				int st = Math.Max(1, start);
				int en = Math.Min(maxItems, end);
				if (st <= en)
				{
					for (int i = st; i <= en; i++)
						result |= 1 << (i - 1);
				}
			}
		}

		// Parse and set bits for all individual numbers found (clamped to allowed range)
		foreach (System.Text.RegularExpressions.Match m in numRegex.Matches(s))
		{
			if (int.TryParse(m.Groups[1].Value, out int n) && n > 0 && n <= maxItems)
				result |= 1 << (n - 1);
		}

		// Finally, ensure any stray bits above maxItems are cleared
		result = (int)((uint)result & mask);

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

		// Use unsigned arithmetic to avoid sign-extension on the high bit (bit 31 -> instrument 32).
		// If the signed int has the high bit set, casting to uint produces the correct bit pattern
		uint unum = unchecked((uint)num);

		var result = new System.Text.StringBuilder();
		int i = 1; // Bit position (1-based)
		while (unum != 0)
		{
			// Skip unset bits
			if ((unum & 1u) == 0u)
			{
				unum >>= 1;
				i++;
				continue;
			}
			int start = i; // Start of a run of set bits
						   // Find the end of the run of consecutive set bits
			while (((unum >> 1) & 1u) == 1u)
			{
				unum >>= 1;
				i++;
			}
			int end = i; // End of the run
			if (result.Length > 0) result.Append(','); // Add comma if not the first range
													   // Append either a single value or a range
			result.Append(start == end ? $"{start}" : $"{start}-{end}");
			unum >>= 1;
			i++;
		}
		return result.ToString();
	}

}



using CommunityToolkit.Mvvm.ComponentModel;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MMM_Device;

//Algorythmic Methods to Distribute Notes Amoungst Instruments.
public enum DistributionMethod
{
    StraightThrough = 0,    //Each Channel goes to the Instrument with a matching ID ex. Ch 10 -> Instrument 10
    RoundRobin,             //Distributes Notes in a circular manner.
    RoundRobinBalance,      //Distributes Notes in a circular manner (Balances Notes across Instruments).
    Ascending,              //Plays Note on lowest available Instrument (Balances Notes across Instruments).
    Descending,             //Plays Note on highest available Instrument (Balances Notes across Instruments).
    Stack                   //TODO Play Notes Polyphonicaly on lowest available Instrument until full.
};

/* Routes Midi Notes to various instrument groups via configurable algorithms. */
public partial class Distributor : ObservableObject
{
	public const int NUM_CFG_BYTES = 24;

	const int BOOL_MUTED = 0x0001;
	const int BOOL_POLYPHONIC = 0x0002;
	const int BOOL_NOTEOVERWRITE = 0x0004;
	const int BOOL_DAMPERPEDAL = 0x0008;
	const int BOOL_VIBRATO = 0x0010;

	[ObservableProperty]
	private int? _index = 0;
	[ObservableProperty]
	private int _currentChannel = 0;
	[ObservableProperty]
	private int _currentInstrument = 0;
	[ObservableProperty]
	private int _channels = 0;
	[ObservableProperty]
	private int _instruments = 0;
	[ObservableProperty]
	private bool _muted = false;
	[ObservableProperty]
	private bool _polyphonic = true;
	[ObservableProperty]
	private bool _noteOverwrite = false;
	[ObservableProperty]
	private bool _damperPedal = false;
	[ObservableProperty]
	private bool _vibrato = false;
	[ObservableProperty]
	private int _minNote = 0;
	[ObservableProperty]
	private int _maxNote = 127;
	[ObservableProperty]
	private int _numPolyphonicNotes = 1;
	[ObservableProperty]
	private DistributionMethod _method = DistributionMethod.RoundRobinBalance;

	public Distributor()
	{
		Console.WriteLine("Added a Distributor");
	}

	public Distributor(byte[] data)
	{
		Console.WriteLine("Added a Distributor");
		this.SetDistributor(data);
	}

	public byte[] ToSerial()
	{
		byte[] distributorObj = new byte[NUM_CFG_BYTES];

		int distributorBoolValue = 0;
		if (Muted) distributorBoolValue |= BOOL_MUTED;
		if (Polyphonic) distributorBoolValue |= BOOL_POLYPHONIC;
		if (NoteOverwrite) distributorBoolValue |= BOOL_NOTEOVERWRITE;
		if (DamperPedal) distributorBoolValue |= BOOL_DAMPERPEDAL;
		if (Vibrato) distributorBoolValue |= BOOL_VIBRATO;

		distributorObj[0] = (byte)(((Index ?? 0) >> 7) & 0x7F);
		distributorObj[1] = (byte)(((Index ?? 0) >> 0) & 0x7F);
		distributorObj[2] = (byte)((Channels >> 14) & 0x03);
		distributorObj[3] = (byte)((Channels >> 7) & 0x7F);
		distributorObj[4] = (byte)((Channels >> 0) & 0x7F);
		distributorObj[5] = (byte)((Instruments >> 28) & 0x0F);
		distributorObj[6] = (byte)((Instruments >> 21) & 0x7F);
		distributorObj[7] = (byte)((Instruments >> 14) & 0x7F);
		distributorObj[8] = (byte)((Instruments >> 7) & 0x7F);
		distributorObj[9] = (byte)((Instruments >> 0) & 0x7F);
		distributorObj[10] = (byte)(Method);
		distributorObj[11] = (byte)MinNote;
		distributorObj[12] = (byte)MaxNote;
		distributorObj[13] = (byte)NumPolyphonicNotes;
		distributorObj[14] = (byte)((distributorBoolValue >> 7) & 0x7F);
		distributorObj[15] = (byte)((distributorBoolValue >> 0) & 0x7F);
		
		// Bytes 16-23: Reserved
		for (int i = 16; i < NUM_CFG_BYTES; i++)
		{
			distributorObj[i] = 0;
		}

		return distributorObj;
	}

	public void SetDistributor(byte[] data)
	{
		int index =
			  (data[0] << 7)
			| (data[1] << 0);
		int channels =
			  (data[2] << 14)
			| (data[3] << 7)
			| (data[4] << 0);
		int instruments =
			  (data[5] << 28)
			| (data[6] << 21)
			| (data[7] << 14)
			| (data[8] << 7)
			| (data[9] << 0);
		DistributionMethod distributionMethod = (DistributionMethod)(data[10]);
		int boolValue = (data[14] << 7) | (data[15] << 0);

		Index = index;
		Channels = channels;
		Instruments = instruments;
		Method = distributionMethod;
		MinNote = data[11];
		MaxNote = data[12];
		NumPolyphonicNotes = data[13];
		Muted = (boolValue & BOOL_MUTED) != 0;
		Polyphonic = (boolValue & BOOL_POLYPHONIC) != 0;
		NoteOverwrite = (boolValue & BOOL_NOTEOVERWRITE) != 0;
		DamperPedal = (boolValue & BOOL_DAMPERPEDAL) != 0;
		Vibrato = (boolValue & BOOL_VIBRATO) != 0;
	}
}
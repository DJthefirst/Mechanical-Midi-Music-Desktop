using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MMM_Server_Grpc;

using MMM_Core;
using MMM_Core.MidiManagers;
using Melanchall.DryWetMidi.Multimedia;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace MMM_Server_Grpc.Services;


public class MMM
{
	//Singleton pattern to ensure only one instance of DeviceManager exists
	private static MMM instance = new MMM();

	public IMidiPlaylist playlist;
	public IMidiPlayer player;

	private MidiOutputSystem midiOutputSystem = new MidiOutputSystem();
	public MidiSerialManager serialManager = new MidiSerialManager();
	public MidiPortInManager midiPortInManager = new MidiPortInManager();
	public MidiPortOutManager midiPortOutManager = new MidiPortOutManager();
	MidiCore midiCore = new MidiCore();

	public static void Reset() => instance = new MMM();

	static MMM() { }
	private MMM()
	{
		playlist = new MidiPlaylist();
		player = new MidiPlayer(playlist);

		//Connect MIDI Inputs
		midiCore.Connect((IInputDevice)player);
		midiCore.Connect((IInputDevice)serialManager);
		midiCore.Connect((IInputDevice)midiPortInManager);

		//Connect MIDI Outputs
		//midiCore.Connect((IOutputDevice)midiOutputSystem);
		midiCore.Connect((IOutputDevice)serialManager);
		midiCore.Connect((IOutputDevice)midiPortOutManager);
	}
	public static MMM Instance => instance;
}

public class CoreService : SystemCMDs.SystemCMDsBase
{
	public override Task<StringMsg> Help(Cmd request, ServerCallContext context)
	{
		return Task.FromResult(new StringMsg
		{
			Message = ("\n------------------------------ HELP ------------------------------\n" +
			"For more information on a specific command, type HELP command-name")
		});
	}

	public override Task<NullMsg> Reset(Cmd request, ServerCallContext context)
	{
		MMM.Reset();
		return Task.FromResult(new NullMsg());
	}

	public override Task<NullMsg> Quit(Cmd request, ServerCallContext context)
	{ 
		//TODO: Implement quit logic here
		return Task.FromResult(new NullMsg());
	}

	public override Task<Struct> LoadConfiguration(IdCmd request, ServerCallContext context)
	{
		//TODO: Implement load configuration logic here
		return Task.FromResult(new Struct());
	}
}

// PlayerCMDs service
public class PlayerService : PlayerCMDs.PlayerCMDsBase
{
	public override Task<Struct> Request(Cmd request, ServerCallContext context)
	{
		return Task.FromResult(new Struct
		{
			Struct_ = Google.Protobuf.ByteString.CopyFrom(MMM.Instance.player.ToByteArray())
		});
	}

	public override Task<ModeMsg> Status(ModeCmd request, ServerCallContext context)
	{

		if (request.IsSetter)
		{

		}
		return Task.FromResult(new ModeMsg
		{
			Mode = (uint)MMM.Instance.player.GetStatus()
		});
	}

	public override Task<PositionMsg> Position(PositionCmd request, ServerCallContext context)
	{
		double position = request.IsSetter ?
			MMM.Instance.player.SetPositionMs(request.Position) :
			MMM.Instance.player.GetPositionMs();

		return Task.FromResult(new PositionMsg
		{
			Position = position
		});
	}

	public override Task<PositionMsg> Navigate(PositionCmd request, ServerCallContext context)
	{
		double position = request.IsSetter ?
			MMM.Instance.player.NavigateMs(request.Position) :
			MMM.Instance.player.GetPositionMs();

		return Task.FromResult(new PositionMsg
		{
			Position = position
		});
	}

	public override Task<ModeMsg> Repeat(ModeCmd request, ServerCallContext context)
	{
		if (!request.IsSetter)
			MMM.Instance.player.Repeat((uint)request.Mode);
		return Task.FromResult(new ModeMsg());
	}

	public override Task<BoolMsg> Loop(BoolCmd request, ServerCallContext context)
	{
		MMM.Instance.player.Loop(request.Value);
		return Task.FromResult(new BoolMsg
		{
			Value = request.Value
		});
	}

	public override Task<BoolMsg> AutoPlay(BoolCmd request, ServerCallContext context)
	{
		MMM.Instance.player.AutoPlay(request.Value);
		return Task.FromResult(new BoolMsg
		{
			Value = request.Value
		});
	}
}

// PlaylistCMDs service
public class PlaylistService : PlaylistCMDs.PlaylistCMDsBase
{
	public override Task<PlaylistStruct> Request(Cmd request, ServerCallContext context)
	{
		return Task.FromResult(new PlaylistStruct { 
			CurIndex = MMM.Instance.playlist.GetCurSong() is { } curSong ? (uint)curSong.Index : 0,
			List = { MMM.Instance.playlist.Songs.Select(s => new SongStruct { Index = s.Index, Name = s.Name, Path = s.Path.FullName }) }
		});
	}

	public override Task<PlaylistStruct> Clear(Cmd request, ServerCallContext context)
	{
		// Implement clear logic here
		return Task.FromResult(new PlaylistStruct());
	}

	public override Task<PlaylistStruct> Swap(SwapCmd request, ServerCallContext context)
	{
		// Implement swap logic here
		return Task.FromResult(new PlaylistStruct());
	}

	public override Task<PlaylistStruct> Add(Struct request, ServerCallContext context)
	{
		// Implement add logic here
		return Task.FromResult(new PlaylistStruct());
	}

	public override Task<PlaylistStruct> Remove(IdCmd request, ServerCallContext context)
	{
		// Implement remove logic here
		return Task.FromResult(new PlaylistStruct());
	}

	public override Task<ModeMsg> Index(ModeCmd request, ServerCallContext context)
	{
		// Implement index logic here
		return Task.FromResult(new ModeMsg());
	}

	public override Task<ModeMsg> Navigate(ModeCmd request, ServerCallContext context)
	{
		// Implement navigate logic here
		return Task.FromResult(new ModeMsg());
	}
}

// ConnectionCMDs service
public class ConnectionService : ConnectionCMDs.ConnectionCMDsBase
{
	public override Task<ConnectionListStruct> Request(ConnectionCmd request, ServerCallContext context)
	{
		// Implement connection request logic here
		return Task.FromResult(new ConnectionListStruct());
	}

	public override Task<ConnectionListStruct> Available(ConnectionCmd request, ServerCallContext context)
	{
		// Implement available logic here
		return Task.FromResult(new ConnectionListStruct());
	}

	public override Task<ConnectionListStruct> List(ConnectionCmd request, ServerCallContext context)
	{
		// Implement list logic here
		return Task.FromResult(new ConnectionListStruct());
	}

	public override Task<ConnectionListStruct> Connect(ConnectionStructCmd request, ServerCallContext context)
	{
		// Implement connect logic here
		return Task.FromResult(new ConnectionListStruct());
	}

	public override Task<ConnectionListStruct> Disconnect(ConnectionIdCmd request, ServerCallContext context)
	{
		// Implement disconnect logic here
		return Task.FromResult(new ConnectionListStruct());
	}

	public override Task<ConnectionListStruct> Clear(ConnectionCmd request, ServerCallContext context)
	{
		// Implement clear logic here
		return Task.FromResult(new ConnectionListStruct());
	}
}

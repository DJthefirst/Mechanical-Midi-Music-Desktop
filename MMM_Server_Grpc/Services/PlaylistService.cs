using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MMM_Server;

using MMM_Core;
using MMM_Core.MidiManagers;
using Melanchall.DryWetMidi.Multimedia;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace MMM_Server.Grpc_Services;

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
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MMM_Server;

using MMM_Core;
using MMM_Core.MidiManagers;
using Melanchall.DryWetMidi.Multimedia;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace MMM_Server.Grpc_Services;

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
			switch (request.Mode)
			{
				case 0:
					Console.WriteLine("Stop");
					MMM.Instance.player.Stop();
					break;
				case 1:
				case 2:
					Console.WriteLine("Play");
					MMM.Instance.player.Play();
					break;
				case 3:
					Console.WriteLine("Pause");
					MMM.Instance.player.Pause();
					break;
				default:
					break;
			}
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
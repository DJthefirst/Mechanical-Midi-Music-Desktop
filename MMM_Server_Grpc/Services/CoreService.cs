using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MMM_Server;

namespace MMM_Server.Grpc_Services;

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
		MMM.Instance.player.Stop();
		return Task.FromResult(new NullMsg());
	}

	public override Task<Struct> LoadConfiguration(IdCmd request, ServerCallContext context)
	{
		//TODO: Implement load configuration logic here
		return Task.FromResult(new Struct());
	}
}
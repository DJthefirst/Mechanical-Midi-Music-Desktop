using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MMM_Server;

using MMM_Core;
using MMM_Core.MidiManagers;
using Melanchall.DryWetMidi.Multimedia;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace MMM_Server.Grpc_Services;

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

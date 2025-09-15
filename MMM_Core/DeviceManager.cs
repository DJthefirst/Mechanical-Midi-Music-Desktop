using MMM_Core.MidiManagers;
using MMM_Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMM_Core
{
	public class DeviceManager
	{
		private static readonly Lazy<DeviceManager> _instance = new(() => new DeviceManager());
		public static DeviceManager Instance => _instance.Value;


		private DeviceManager() { }

		public Dictionary<(IConnection, int), Device> Devices { get; private set; } = new Dictionary<(IConnection, int), Device>();

		public event EventHandler<Dictionary<(IConnection, int), Device>>? OnListUpdated;

		public event EventHandler<Device>? DeviceUpdated;
		internal void AddDevice(IConnection connection, Device device)
		{
			device.DeviceUpdated += DeviceUpdated;
			Devices[(connection, device.SYSEX_DEV_ID)] = device;
			OnListUpdated?.Invoke(this, GetDevicesForConnection(connection));
		}

		private void Device_DeviceUpdated(object? sender, Device e)
		{
			throw new NotImplementedException();
		}

		public void RemoveDevice(IConnection connection, Device device)
		{
			if (Devices.Remove((connection, device.SYSEX_DEV_ID)))
			{
				OnListUpdated?.Invoke(this, GetDevicesForConnection(connection));
			}
		}

		internal void CloseConnection(IConnection connection)
		{
			string connstr = Device.GetConnectionString(connection);
			var devicesToRemove = Devices.Values.Where(d => d.ConnectionString == connstr).ToList();
			foreach (var device in devicesToRemove)
			{
				Devices.Remove((connection,device.SYSEX_DEV_ID));
			}
			if (devicesToRemove.Count > 0)
			{
				OnListUpdated?.Invoke(this, Devices);
			}
		}


		private Dictionary<(IConnection, int), Device> GetDevicesForConnection(IConnection connection)
		{
			return Devices
			.Where(kvp => kvp.Key.Item1 == connection)
			.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}
	}
}

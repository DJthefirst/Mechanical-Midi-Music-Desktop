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
		//Singleton pattern to ensure only one instance of DeviceManager exists
		private static readonly DeviceManager instance = new DeviceManager();

		public Dictionary<(IConnection, int), Device> Devices { get; private set; } = new Dictionary<(IConnection, int), Device>();

		public event EventHandler<Dictionary<(IConnection, int), Device>>? OnListUpdated;

		static DeviceManager() { }
		private DeviceManager() { }

		internal void AddDevice(IConnection connection, Device device)
		{
			Devices[(connection, device.SYSEX_DEV_ID)] = device;
			OnListUpdated?.Invoke(this, GetDevicesForConnection(connection));
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

		public static DeviceManager Instance => instance;
	}
}

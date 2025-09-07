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

		public Dictionary<int, Device> Devices { get; private set; } = new Dictionary<int, Device>();

		public event EventHandler<Dictionary<int, Device>>? OnListUpdated;

		static DeviceManager(){}
		private DeviceManager(){}

		internal void AddDevice(Device device)
		{
			if (Devices.ContainsKey(device.SYSEX_DEV_ID))
			{
				Devices[device.SYSEX_DEV_ID] = device;
			}
			else Devices.Add(device.SYSEX_DEV_ID, device);
			OnListUpdated?.Invoke(this, Devices);
		}

		internal void RemoveDevice(Device device)
		{
			Devices.Remove(device.SYSEX_DEV_ID);
			OnListUpdated?.Invoke(this, Devices);
		}

		internal void CloseConnection(object connection)
		{
			string connstr = Device.GetConnectionString(connection);
			var devicesToRemove = Devices.Values.Where(d => d.ConnectionString == connstr).ToList();
			foreach (var device in devicesToRemove)
			{
				Devices.Remove(device.SYSEX_DEV_ID);
			}
			if (devicesToRemove.Count > 0)
			{
				OnListUpdated?.Invoke(this, Devices);
			}
		}

		public static DeviceManager Instance => instance;
	}
}

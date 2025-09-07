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

		internal Dictionary<int, Device> devices = new Dictionary<int, Device>();

		static DeviceManager(){}
		private DeviceManager(){}

		internal void AddDevice(Device device)
		{
			if (devices.ContainsKey(device.SYSEX_DEV_ID))
			{
				devices[device.SYSEX_DEV_ID] = device;
			}
			else devices.Add(device.SYSEX_DEV_ID, device);
		}

		internal void RemoveDevice(Device device)
		{
			devices.Remove(device.SYSEX_DEV_ID);
		}

		public static DeviceManager Instance => instance;
	}
}

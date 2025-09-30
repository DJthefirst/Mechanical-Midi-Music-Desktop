using MMM_Core.MidiManagers;
using MMM_Device;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static MMM_Core.DeviceEntry;

namespace MMM_Core;

public struct DeviceEntry
{
	public IConnection Connection { get; }
	public int Id { get; set; }
	//	public int Id => Device.SYSEX_DEV_ID;
	public Device Device { get; }

	public DeviceEntry(IConnection connection, int deviceId, Device device)
	{
		Connection = connection;
		Id = deviceId;
		Device = device;
	}
}
public class DeviceCollection : IEnumerable<DeviceEntry>
{
	// Inherit from Dictionary for storage
	private readonly Dictionary<IConnection, Dictionary<int, Device>> _devices = new();
	public DeviceCollection() { }
	public DeviceCollection(Dictionary<IConnection, Dictionary<int, Device>> dictionary)
	{
		foreach (var kvp in dictionary)
		{
			_devices[kvp.Key] = new Dictionary<int, Device>(kvp.Value);
		}
	}

	public Dictionary<int, Device> this[IConnection key]
	{
		get => _devices[key];
		set => _devices[key] = value;
	}

	public ICollection<IConnection> Keys => _devices.Keys;
	public ICollection<Dictionary<int, Device>> Values => _devices.Values;
	public int Count => _devices.Count;

	public bool ContainsKey(IConnection key) => _devices.ContainsKey(key);
	public bool Remove(IConnection key) => _devices.Remove(key);
	public void Clear() => _devices.Clear();
	public bool TryGetValue(IConnection key, out Dictionary<int, Device> value)
	{
		if (_devices.TryGetValue(key, out var found))
		{
			value = found;
			return true;
		}
		value = new Dictionary<int, Device>();
		return false;
	}
	public void Add(IConnection key, Dictionary<int, Device> value) => _devices.Add(key, value);

	public IEnumerator<DeviceEntry> GetEnumerator()
	{
		foreach (var connKvp in _devices)
		{
			foreach (var devKvp in connKvp.Value)
			{
				yield return new DeviceEntry(connKvp.Key, devKvp.Key, devKvp.Value);
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class DeviceManager
{
	private static readonly Lazy<DeviceManager> _instance = new(() => new DeviceManager());
	public static DeviceManager Instance => _instance.Value;

	private DeviceManager() { }

	public DeviceCollection Devices { get; private set; } = new();

	public event EventHandler<DeviceCollection>? OnDeviceListChanged;
	public event EventHandler<DeviceEntry>? OnDeviceChanged;

	internal void HandleDeviceConstruct(IConnection connection, byte[] construct)
	{
		Device temp = new Device(construct);
		temp.ConnectionString = Device.GetConnectionString(connection);

		// Check that the connection exists and the device does not exist yet
		if (!Devices.ContainsKey(connection) || !Devices[connection].ContainsKey(temp.Id))
		{
			AddDevice(connection, temp);
			return;
		}
		Devices[connection][temp.Id].SetDeviceConstruct(construct);
	}
	internal void AddDevice(IConnection connection, Device device)
	{
		if (!Devices.ContainsKey(connection))
			Devices[connection] = new Dictionary<int, Device>();

		Devices[connection].Add(device.Id, device);

		device.PropertyChanged += (sender, args) =>
		{
			OnDeviceChanged?.Invoke(this, new DeviceEntry(connection, device.Id, device));
		};

		OnDeviceChanged?.Invoke(this, new DeviceEntry(connection, device.Id, device));
	}

	public void RemoveDevice(IConnection connection, Device device)
	{
		if (Devices.TryGetValue(connection, out var devDict) && devDict.Remove(device.Id))
		{
			if (devDict.Count == 0)
				Devices.Remove(connection);
			OnDeviceListChanged?.Invoke(this, Devices);
		}
	}
	internal void CloseConnection(IConnection connection)
	{
		if (Devices.Remove(connection))
		{
			OnDeviceListChanged?.Invoke(this, Devices);
		}
	}
}



using MMM_Core.MidiManagers;
using MMM_Device;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

	public event EventHandler<DeviceCollection>? OnListUpdated;
	public event EventHandler<DeviceEntry>? DeviceUpdated;

	internal void AddDevice(IConnection connection, Device device)
	{
		if (!Devices.ContainsKey(connection))
			Devices[connection] = new Dictionary<int, Device>();

		device.DeviceUpdated += UpdateDevice;
		connection.Updated += (sender, args) =>
		{
			DeviceUpdated?.Invoke(connection, new DeviceEntry(connection, device.SYSEX_DEV_ID, device));
		};
		Devices[connection][device.SYSEX_DEV_ID] = device;
		OnListUpdated?.Invoke(this, GetDevicesForConnection(connection));
	}

	private void UpdateDevice(object? sender, Device e)
	{
		if (sender is not Device device)
			return;

		// Find the connection and device id for the updated device
		if (Devices.Any(entry => entry.Device == device))
		{
			OnListUpdated?.Invoke(this, Devices);
		}
	}

	public void Update(IConnection connection, Melanchall.DryWetMidi.Core.MidiEvent midiEvent)
	{
		connection.SendEvent(midiEvent);
		OnListUpdated?.Invoke(this, GetDevicesForConnection(connection));
	}

	public void RemoveDevice(IConnection connection, Device device)
	{
		if (Devices.TryGetValue(connection, out var devDict) && devDict.Remove(device.SYSEX_DEV_ID))
		{
			if (devDict.Count == 0)
				Devices.Remove(connection);
			OnListUpdated?.Invoke(this, GetDevicesForConnection(connection));
		}
	}

	internal void CloseConnection(IConnection connection)
	{
		if (Devices.Remove(connection))
		{
			OnListUpdated?.Invoke(this, Devices);
		}
	}

	private DeviceCollection GetDevicesForConnection(IConnection connection)
	{
		var result = new Dictionary<IConnection, Dictionary<int, Device>>();
		if (Devices.TryGetValue(connection, out var devDict))
		{
			result[connection] = new Dictionary<int, Device>(devDict);
		}
		return new DeviceCollection(result);
	}

	public IConnection? GetConnection(Device device)
	{
		return Devices.FirstOrDefault(entry => entry.Device == device).Connection;
	}
}



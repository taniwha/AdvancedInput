/*
This file is part of Advanced Input.

Advanced Input is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Advanced Input is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with Advanced Input.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;

namespace AdvancedInput.InputLibWrapper {

[StructLayout(LayoutKind.Sequential)]
struct WrappedDevice
{
	public IntPtr next;
	public IntPtr path;
	public IntPtr name;
	public int fd;
	public int max_button;
	public IntPtr button_map;
	public int num_buttons;
	//private int pad0;
	public IntPtr buttons;
	public int max_abs_axis;
	//private int pad1;
	public IntPtr abs_axis_map;
	public int max_rel_axis;
	//private int pad1;
	public IntPtr rel_axis_map;
	public int num_abs_axes;
	public int num_rel_axes;
	public int num_axes;
	//private int pad2;
	public IntPtr axes;
	public int event_count;
}

[StructLayout(LayoutKind.Sequential)]
public struct Button
{
	public int num, evnum;
	public int state;
}

[StructLayout(LayoutKind.Sequential)]
public struct Axis
{
	public int num, evnum;
	public int value;
	public int min, max;
}

static class ilw
{
	//[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void Callback (IntPtr dev);

	[DllImport ("libinputlib.so", CallingConvention = CallingConvention.Cdecl)]
	public static extern int inputlib_init (Callback addDev, Callback remDev);

	[DllImport ("libinputlib.so", CallingConvention = CallingConvention.Cdecl)]
	public static extern void inputlib_close ();

	[DllImport ("libinputlib.so", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool inputlib_check_input ();
}

public class InputLibLoader
{
	const int RTLD_NOW = 2; // for dlopen's flags
	const int RTLD_GLOBAL = 8;
	[DllImport("libdl.so", CallingConvention = CallingConvention.Cdecl)]
	static extern IntPtr dlopen(string filename, int flags);
	[DllImport("libdl.so", CallingConvention = CallingConvention.Cdecl)]
	static extern int dlclose(IntPtr handle);
	static IntPtr handle;

	public static void openlib ()
	{
		string dll = Assembly.GetExecutingAssembly().Location;
		string dir = System.IO.Path.GetDirectoryName(dll);
		string libPath = dir + "/" + "libinputlib.so";
		handle = dlopen(libPath, RTLD_NOW|RTLD_GLOBAL);
		Debug.LogFormat ("[InputLib] openlib: {0} {1}", libPath, handle);
	}

	public static void closelib ()
	{
		dlclose (handle);
	}
}

public class InputLib
{
	static bool initialized;

	public static List<Device> devices { get; private set; }
	public delegate void Callback (Device dev);
	public static Callback DeviceAdded = (Device dev) => { };
	public static Callback DeviceRemoved = (Device dev) => { };

	static void addDevice (IntPtr devPtr)
	{
		try {
			Device dev = new Device (devPtr);
			devices.Add (dev);
			DeviceAdded (dev);
		} catch (Exception e) {
			Debug.LogFormat ("Exception handling addDevice\n{0}", e);
			throw;
		}
	}

	static void removeDevice (IntPtr devPtr)
	{
		try {
			string path = Device.DevicePath (devPtr);
			for (int i = devices.Count; i-- > 0; ) {
				Device dev = devices[i];
				if (dev.path == path) {
					devices.RemoveAt (i);
					DeviceRemoved (dev);
					dev.close_device ();
				}
			}
		} catch (Exception e) {
			Debug.LogFormat ("Exception handling removeDevice\n{0}", e);
			throw;
		}
	}

	static ilw.Callback addDev;
	static ilw.Callback remDev;

	public static bool Init ()
	{
		devices = new List<Device> ();
		addDev = addDevice;
		remDev = removeDevice;
		int res = ilw.inputlib_init (addDev, remDev);
		if (res < 0) {
			//FIXME should throw an error?
			return false;
		}
		initialized = true;
		return true;
	}

	public static void Close ()
	{
		if (!initialized) {
			//FIXME should throw an error?
			return;
		}
		ilw.inputlib_close ();
	}

	public static bool CheckInput ()
	{
		if (!initialized) {
			//FIXME should throw an error?
			return false;
		}
		return ilw.inputlib_check_input ();
	}
}

public class Device {
	public string path		{ get; private set; }
	public string name		{ get; private set; }
	public int num_buttons	{ get; private set; }
	public Button[] buttons { get; private set; }
	public int num_axes		{ get; private set; }
	public Axis[] axes		{ get; private set; }
	public int event_count	{ get; private set; }

	private IntPtr self;

	private GCHandle buttons_handle, axes_handle;

	internal static string DevicePath (IntPtr devptr)
	{
		unsafe {
			WrappedDevice *dev = (WrappedDevice *) devptr.ToPointer ();
			return Marshal.PtrToStringAnsi(dev->path);
		}
	}

	internal Device (IntPtr devptr)
	{
		self = devptr;
		unsafe {
			WrappedDevice *dev = (WrappedDevice *) devptr.ToPointer ();

			path = Marshal.PtrToStringAnsi(dev->path);
			name = Marshal.PtrToStringAnsi(dev->name);
			num_buttons = dev->num_buttons;
			buttons = new Button[dev->num_buttons];
			num_axes = dev->num_axes;
			axes = new Axis[dev->num_axes];

			int bsize = Marshal.SizeOf (typeof(Button));
			if (dev->num_buttons > 0) {
				for (int i = 0; i < dev->num_buttons; i++) {
					IntPtr bptr = new IntPtr(dev->buttons.ToInt64() + i * bsize);
					buttons[i] = (Button) Marshal.PtrToStructure (bptr, typeof(Button));
				}
				buttons_handle = GCHandle.Alloc(buttons, GCHandleType.Pinned);
				dev->buttons = buttons_handle.AddrOfPinnedObject();
			}

			int asize = Marshal.SizeOf (typeof(Axis));
			if (dev->num_axes > 0) {
				for (int i = 0; i < dev->num_axes; i++) {
					IntPtr aptr = new IntPtr (dev->axes.ToInt64() + i * asize);
					axes[i] = (Axis) Marshal.PtrToStructure (aptr, typeof(Axis));
				}
				axes_handle = GCHandle.Alloc(axes, GCHandleType.Pinned);
				dev->axes = axes_handle.AddrOfPinnedObject();
			}
		}
	}

	internal void close_device ()
	{
		unsafe {
			WrappedDevice *dev = (WrappedDevice *) self.ToPointer ();
			dev->buttons = IntPtr.Zero;
			dev->axes = IntPtr.Zero;
			buttons_handle.Free ();
			axes_handle.Free ();
		}
	}
}

}

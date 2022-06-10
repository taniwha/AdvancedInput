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
	public IntPtr prev;
	public IntPtr path;
	public IntPtr name;
	public IntPtr phys;
	public IntPtr uniq;
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

	public IntPtr data;
	public IntPtr axis_event;
	public IntPtr button_event;
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
	public delegate void DevCallback (IntPtr dev);
	public delegate void InpCallback (IntPtr inp, IntPtr data);

	[DllImport ("libinputlib.so", CallingConvention = CallingConvention.Cdecl)]
	public static extern int inputlib_init (DevCallback addDev, DevCallback remDev);

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

	private static List<int> freeDeviceIds;
	public static List<Device> devices { get; private set; }
	public delegate void Callback (Device dev);
	public static Callback DeviceAdded = (Device dev) => { };
	public static Callback DeviceRemoved = (Device dev) => { };

	public delegate void AxisEventDelegate (in Axis axis, Device device);
	public delegate void ButtonEventDelegate (in Button axis, Device device);
	public static AxisEventDelegate AxisEvent;
	public static ButtonEventDelegate ButtonEvent;

	static void axis_event (IntPtr axisPtr, IntPtr data)
	{
		try {
			var devId = (int) data.ToInt64 ();
			Axis axis;
			unsafe { axis = *(Axis *) axisPtr.ToPointer (); }
			var dev = devices[devId];
			if (dev.AxisEvent != null) {
				dev.AxisEvent (in axis, dev);
			}
			if (AxisEvent != null) {
				AxisEvent (in axis, dev);
			}
			//Debug.Log ($"[InputLib] axis event {devId} {dev.name} {axis.num} {axis.value}");
		} catch (Exception e) {
			Debug.Log ($"[InputLib] Exception handling AxisEvent\n{e}");
			throw;
		}
	}

	static void button_event (IntPtr buttonPtr, IntPtr data)
	{
		try {
			var devId = (int) data.ToInt64 ();
			Button button;
			unsafe { button = *(Button *) buttonPtr.ToPointer (); }
			var dev = devices[devId];
			if (dev.ButtonEvent != null) {
				dev.ButtonEvent (in button, dev);
			}
			if (ButtonEvent != null) {
				ButtonEvent (in button, dev);
			}
			//Debug.Log ($"[InputLib] button event {devId} {dev.name} {button.num} {button.state}");
		} catch (Exception e) {
			Debug.Log ($"[InputLib] Exception handling ButtonEvent\n{e}");
			throw;
		}
	}

	static void AddDevice (IntPtr devPtr)
	{
		try {
			Device dev = new Device (devPtr);
			int devId = devices.Count;
			int freeCount = freeDeviceIds.Count;
			if (freeCount > 0) {
				devId = freeDeviceIds[freeCount - 1];
				freeDeviceIds.RemoveAt (freeCount - 1);
				devices[devId] = dev;
			} else {
				devices.Add (dev);
			}
			dev.id = devId;
			dev.axis_event = axis_event;
			dev.button_event = button_event;
			DeviceAdded (dev);
			Debug.Log ($"[InputLib] Added device {dev.id} {dev.name}");
		} catch (Exception e) {
			Debug.Log ($"[InputLib] Exception handling AddDevice\n{e}");
			throw;
		}
	}

	static void RemoveDevice (IntPtr devPtr)
	{
		try {
			Debug.Log ($"[InputLib] Removing device ptr {devPtr}");
			int devId = Device.DeviceData (devPtr);
			Debug.Log ($"[InputLib] Removing device {devId}");
			Device dev = devices[devId];
			devices[devId] = null;
			freeDeviceIds.Add (devId);
			DeviceRemoved (dev);
			dev.close_device ();
		} catch (Exception e) {
			Debug.Log ($"Exception handling RemoveDevice\n{e}");
			throw;
		}
	}

	public static bool Init ()
	{
		freeDeviceIds = new List<int> ();
		devices = new List<Device> ();
		int res = ilw.inputlib_init (AddDevice, RemoveDevice);
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
	public string phys		{ get; private set; }
	public string uniq		{ get; private set; }
	public int num_buttons	{ get; private set; }
	public Button[] buttons { get; private set; }
	public int num_axes		{ get; private set; }
	public Axis[] axes		{ get; private set; }
	public int event_count	{ get; private set; }

	private IntPtr self;

	public delegate void AxisEventDelegate (in Axis axis, Device device);
	public delegate void ButtonEventDelegate (in Button axis, Device device);
	public AxisEventDelegate AxisEvent;
	public ButtonEventDelegate ButtonEvent;

	public int id
	{
		get {
			return DeviceData (self);
		}
		internal set {
			SetDeviceData (self, value);
		}
	}

	internal ilw.InpCallback axis_event
	{
		get {
			unsafe {
				WrappedDevice *dev = (WrappedDevice *) self.ToPointer ();
				return Marshal.GetDelegateForFunctionPointer<ilw.InpCallback> (dev->axis_event);
			}
		}
		set {
			unsafe {
				WrappedDevice *dev = (WrappedDevice *) self.ToPointer ();
				dev->axis_event = Marshal.GetFunctionPointerForDelegate<ilw.InpCallback> (value);
			}
		}
	}

	internal ilw.InpCallback button_event
	{
		get {
			unsafe {
				WrappedDevice *dev = (WrappedDevice *) self.ToPointer ();
				return Marshal.GetDelegateForFunctionPointer<ilw.InpCallback> (dev->button_event);
			}
		}
		set {
			unsafe {
				WrappedDevice *dev = (WrappedDevice *) self.ToPointer ();
				dev->button_event = Marshal.GetFunctionPointerForDelegate<ilw.InpCallback> (value);
			}
		}
	}

	private GCHandle buttons_handle, axes_handle;

	internal static string DevicePath (IntPtr devptr)
	{
		unsafe {
			WrappedDevice *dev = (WrappedDevice *) devptr.ToPointer ();
			return Marshal.PtrToStringAnsi(dev->path);
		}
	}

	internal static int DeviceData (IntPtr devptr)
	{
		unsafe {
			WrappedDevice *dev = (WrappedDevice *) devptr.ToPointer ();
			return (int) dev->data.ToInt64 ();
		}
	}

	internal static void SetDeviceData (IntPtr devptr, int data)
	{
		unsafe {
			WrappedDevice *dev = (WrappedDevice *) devptr.ToPointer ();
			dev->data = new IntPtr (data);
		}
	}

	internal Device (IntPtr devptr)
	{
		self = devptr;
		unsafe {
			WrappedDevice *dev = (WrappedDevice *) devptr.ToPointer ();

			path = Marshal.PtrToStringAnsi(dev->path);
			name = Marshal.PtrToStringAnsi(dev->name);
			phys = Marshal.PtrToStringAnsi(dev->phys);
			uniq = Marshal.PtrToStringAnsi(dev->uniq);
			num_buttons = dev->num_buttons;
			buttons = new Button[dev->num_buttons];
			num_axes = dev->num_axes;
			axes = new Axis[dev->num_axes];

			int bsize = Marshal.SizeOf (typeof(Button));
			if (dev->num_buttons > 0) {
				for (int i = 0; i < dev->num_buttons; i++) {
					IntPtr bptr = new IntPtr(dev->buttons.ToInt64 () + i * bsize);
					buttons[i] = (Button) Marshal.PtrToStructure (bptr, typeof(Button));
				}
				buttons_handle = GCHandle.Alloc(buttons, GCHandleType.Pinned);
				dev->buttons = buttons_handle.AddrOfPinnedObject();
			}

			int asize = Marshal.SizeOf (typeof(Axis));
			if (dev->num_axes > 0) {
				for (int i = 0; i < dev->num_axes; i++) {
					IntPtr aptr = new IntPtr (dev->axes.ToInt64 () + i * asize);
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

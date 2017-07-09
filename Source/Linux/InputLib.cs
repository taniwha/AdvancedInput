using System;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace InputLib {

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
	public int max_axis;
	//private int pad1;
	public IntPtr axis_map;
	public int num_axes;
	//private int pad2;
	public IntPtr axes;
	public int event_count;
}

[StructLayout(LayoutKind.Sequential)]
struct Button
{
	public int num, evnum;
	public int state;
}

[StructLayout(LayoutKind.Sequential)]
struct Axis
{
	public int num, evnum;
	public int value;
	public int min, max;
}

static class InputLibWrapper
{
	[DllImport ("libinputlib.so", CallingConvention = CallingConvention.Cdecl)]
	public static extern int check_device (string path);

	[DllImport ("libinputlib.so", CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr scan_devices ();

	[DllImport ("libinputlib.so", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool check_device_input ();
}

class Device {
	public string path		{ get; private set; }
	public string name		{ get; private set; }
	public int num_buttons	{ get; private set; }
	public Button[] buttons { get; private set; }
	public int num_axes		{ get; private set; }
	public Axis[] axes		{ get; private set; }
	public int event_count	{ get; private set; }

	public static List<Device> devices { get; private set; }

	private IntPtr self, next;
	private int bsize, asize;
	//private GCHandle bhandle, ahandle;

	Device (IntPtr devptr)
	{
		self = devptr;
		WrappedDevice wdev = (WrappedDevice) Marshal.PtrToStructure (devptr, typeof (WrappedDevice));
		path = Marshal.PtrToStringAnsi(wdev.path);
		name = Marshal.PtrToStringAnsi(wdev.name);
		num_buttons = wdev.num_buttons;
		buttons = new Button[wdev.num_buttons];
		num_axes = wdev.num_axes;
		axes = new Axis[wdev.num_axes];
		next = wdev.next;
		bsize = Marshal.SizeOf (typeof(Button));
		asize = Marshal.SizeOf (typeof(Axis));
		for (int i = 0; i < wdev.num_buttons; i++) {
			IntPtr bptr = new IntPtr(wdev.buttons.ToInt64() + i * bsize);
			buttons[i] = (Button) Marshal.PtrToStructure (bptr, typeof(Button));
		}
		for (int i = 0; i < wdev.num_axes; i++) {
			IntPtr aptr = new IntPtr (wdev.axes.ToInt64() + i * asize);
			axes[i] = (Axis) Marshal.PtrToStructure (aptr, typeof(Axis));
		}
	}

	const int RTLD_NOW = 2; // for dlopen's flags
	const int RTLD_GLOBAL = 8;
	[DllImport("libdl.so", CallingConvention = CallingConvention.Cdecl)]
	static extern IntPtr dlopen(string filename, int flags);
	[DllImport("libdl.so", CallingConvention = CallingConvention.Cdecl)]
	static extern int dlclose(IntPtr handle);
	static IntPtr dlhandle;
	   
	public static void open ()
	{
		string dll = Assembly.GetExecutingAssembly().Location;
		string dir = System.IO.Path.GetDirectoryName(dll);
		string libPath = dir + "/" + "libinputlib.so";
		dlhandle = dlopen(libPath, RTLD_NOW|RTLD_GLOBAL);
	}

	public static void close ()
	{
		dlclose (dlhandle);
	}


	public static void Scan ()
	{
		devices = new List<Device> ();

		IntPtr devptr = InputLibWrapper.scan_devices();
		while (devptr != IntPtr.Zero) {
			Device dev = new Device (devptr);
			devices.Add (dev);
			devptr = dev.next;
		}
	}

	public static bool CheckInput ()
	{
		if (InputLibWrapper.check_device_input ()) {
			for (int i = devices.Count; i-- > 0; ) {
				Device dev = devices[i];
				IntPtr wbuttons, waxes;
				WrappedDevice wdev = (WrappedDevice) Marshal.PtrToStructure (dev.self, typeof (WrappedDevice));
				dev.event_count = wdev.event_count;
				wbuttons = wdev.buttons;
				waxes = wdev.axes;
				for (int j = 0; j < dev.num_buttons; j++) {
					IntPtr bptr = new IntPtr(wbuttons.ToInt64() + j * dev.bsize);
					dev.buttons[j] = (Button) Marshal.PtrToStructure (bptr, typeof(Button));
				}
				for (int j = 0; j < dev.num_axes; j++) {
					IntPtr aptr = new IntPtr (waxes.ToInt64() + j * dev.asize);
					dev.axes[j] = (Axis) Marshal.PtrToStructure (aptr, typeof(Axis));
				}
			}
			return true;
		}
		return false;
	}
}

}

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AdvancedInput.InputLib {

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

static class InputLibWrapper
{
	[DllImport ("libinputlib.so", CallingConvention = CallingConvention.Cdecl)]
	public static extern int check_device (string path);

	[DllImport ("libinputlib.so", CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr scan_devices ();

	[DllImport ("libinputlib.so", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool check_device_input ();
}

public class Device {
	public string path		{ get; private set; }
	public string name		{ get; private set; }
	public int num_buttons	{ get; private set; }
	public Button[] buttons { get; private set; }
	public int num_axes		{ get; private set; }
	public Axis[] axes		{ get; private set; }
	public int event_count	{ get; private set; }

	public static List<Device> devices { get; private set; }

	private IntPtr self, next;

	Device (IntPtr devptr)
	{
		self = devptr;
		unsafe {
			WrappedDevice *dev = (WrappedDevice *) devptr.ToPointer ();

			next = dev->next;

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
				var handle = GCHandle.Alloc(buttons, GCHandleType.Pinned);
				dev->buttons = handle.AddrOfPinnedObject();
			}

			int asize = Marshal.SizeOf (typeof(Axis));
			if (dev->num_axes > 0) {
				for (int i = 0; i < dev->num_axes; i++) {
					IntPtr aptr = new IntPtr (dev->axes.ToInt64() + i * asize);
					axes[i] = (Axis) Marshal.PtrToStructure (aptr, typeof(Axis));
				}
				var handle = GCHandle.Alloc(axes, GCHandleType.Pinned);
				dev->axes = handle.AddrOfPinnedObject();
			}
		}
	}

	const int RTLD_NOW = 2; // for dlopen's flags
	const int RTLD_GLOBAL = 8;
	[DllImport("libdl.so", CallingConvention = CallingConvention.Cdecl)]
	static extern IntPtr dlopen(string filename, int flags);
	[DllImport("libdl.so", CallingConvention = CallingConvention.Cdecl)]
	static extern int dlclose(IntPtr handle);
	static IntPtr handle;
	   
	public static void open ()
	{
		string dll = Assembly.GetExecutingAssembly().Location;
		string dir = System.IO.Path.GetDirectoryName(dll);
		string libPath = dir + "/" + "libinputlib.so";
		handle = dlopen(libPath, RTLD_NOW|RTLD_GLOBAL);
	}

	public static void close ()
	{
		dlclose (handle);
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
				unsafe {
					var wdev = (WrappedDevice *) dev.self.ToPointer ();
					dev.event_count = wdev->event_count;
				}
			}
			return true;
		}
		return false;
	}
}

}

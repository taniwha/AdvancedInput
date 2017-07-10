/*
This file is part of Advanced Input.

Advanced Input is free software: you can redistribute it and/or
modify it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Advanced Input is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Advanced Input.  If not, see
<http://www.gnu.org/licenses/>.
*/
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

using KSP.IO;

namespace AdvancedInput {

	public delegate void AxisBindingDelegate (float value, bool updated);
	public delegate void ButtonBindingDelegate (int state, bool edge);

	[KSPAddon (KSPAddon.Startup.Flight, false)]
	public class AI_FlightControl : MonoBehaviour
	{
		FlightCtrlState ctrlState;

		public static bool overrideMainThrottle;
		public static bool overrideWheelThrottle;

		public float prevMainThrottle;
		public float prevWheelThrottle;
		public bool mainThrottleLock;
		public bool wheelThrottleLock;

		List<Device> devices;

		public static AI_FlightControl instance { get; private set; }

		void Awake ()
		{
			instance = this;
			GameEvents.onVesselChange.Add (OnVesselChange);
			ctrlState = new FlightCtrlState ();
			devices = new List<Device> ();
			prevMainThrottle = -2;
			prevMainThrottle = -2;
		}

		void OnDestroy ()
		{
			instance = null;
			GameEvents.onVesselChange.Remove (OnVesselChange);
		}

		void Start ()
		{
			foreach (var dev in InputLib.Device.devices) {
				devices.Add (new Device (dev));
			}
		}

		Vessel currentVessel;

		void OnVesselChange(Vessel vessel)
		{
			if (currentVessel != null) {
				currentVessel.OnFlyByWire -= OnFlyByWire;
			}
			currentVessel = vessel;
			if (vessel != null) {
				vessel.OnFlyByWire += OnFlyByWire;
			}

			/*FIXME use others?
			 * vessel.OnPreAutopilotUpdate;
			 * vessel.OnAutopilotUpdate;
			 * vessel.OnPostAutopilotUpdate;
			 * vessel.OnFlyByWire;
			 */
		}

		void OnFlyByWire (FlightCtrlState state)
		{
			InputLib.Device.CheckInput ();
			for (int i = devices.Count; i-- > 0; ) {
				devices[i].CheckInput ();
			}
			if (!ctrlState.isIdle) {
				state.roll = ctrlState.roll;
				state.pitch = ctrlState.pitch;
				state.yaw = ctrlState.yaw;
				state.X = ctrlState.X;
				state.Y = ctrlState.Y;
				state.Z = ctrlState.Z;
				state.wheelSteer = ctrlState.wheelSteer;
			}
			state.pitchTrim += ctrlState.pitchTrim;
			state.yawTrim += ctrlState.yawTrim;
			state.rollTrim += ctrlState.rollTrim;
			state.wheelSteerTrim += ctrlState.wheelSteerTrim;
			state.wheelThrottleTrim += ctrlState.wheelThrottleTrim;

			/* it seems these have no affect
			state.killRot ^= ctrlState.killRot;
			state.gearUp ^= ctrlState.gearUp;
			state.gearDown ^= ctrlState.gearDown;
			state.headlight ^= ctrlState.headlight;
			*/

			if (prevMainThrottle != state.mainThrottle) {
				prevMainThrottle = state.mainThrottle;
				if (!overrideMainThrottle) {
					ctrlState.mainThrottle = state.mainThrottle;
				}
			}
			if (prevWheelThrottle != state.wheelThrottle) {
				prevWheelThrottle = state.wheelThrottle;
				if (!overrideWheelThrottle) {
					ctrlState.wheelThrottle = state.wheelThrottle;
				}
			}

			state.mainThrottle = ctrlState.mainThrottle;
			state.wheelThrottle = ctrlState.wheelThrottle;
		}

		public void AxisBinding_MainThrottle (float value, bool updated)
		{
			if (updated && !mainThrottleLock) {
				ctrlState.mainThrottle = value;
			}
		}

		public void AxisBinding_WheelThrottle (float value, bool updated)
		{
			if (updated && !wheelThrottleLock) {
				ctrlState.wheelThrottle = value;
			}
		}

		public void AxisBinding_Pitch (float value, bool updated)
		{
			ctrlState.pitch = value;
		}

		public void AxisBinding_Yaw (float value, bool updated)
		{
			ctrlState.yaw = value;
		}

		public void AxisBinding_Roll (float value, bool updated)
		{
			ctrlState.roll = value;
		}

		public void AxisBinding_X (float value, bool updated)
		{
			ctrlState.X = value;
		}

		public void AxisBinding_Y (float value, bool updated)
		{
			ctrlState.Y = value;
		}

		public void AxisBinding_Z (float value, bool updated)
		{
			ctrlState.Z = value;
		}

		public void AxisBinding_WheelSteer (float value, bool updated)
		{
			ctrlState.wheelSteer = value;
		}

		public void ButtonBinding_MainThrottleLock (int state, bool edge)
		{
			mainThrottleLock = state != 0;
		}

		public void ButtonBinding_ToggleMainThrottleLock (int state, bool edge)
		{
			if (state > 0 && edge) {
				mainThrottleLock = !mainThrottleLock;
			}
		}

		public void ButtonBinding_WheelThrottleLock (int state, bool edge)
		{
			wheelThrottleLock = state != 0;
		}

		public void ButtonBinding_ToggleWheelThrottleLock (int state, bool edge)
		{
			if (state > 0 && edge) {
				wheelThrottleLock = !wheelThrottleLock;
			}
		}

		const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
		static object FindBinding<T> (string name)
		{
			Type type = typeof (AI_FlightControl);
			var method = type.GetMethod (name, bindingFlags);
			if (method == null) {
				return null;
			}
			return Delegate.CreateDelegate(typeof(T), instance, method);
		}

		public static AxisBindingDelegate GetAxisBinding (string name)
		{
			object binding = FindBinding<AxisBindingDelegate> ("AxisBinding_" + name);
			return (AxisBindingDelegate) binding;
		}

		public static ButtonBindingDelegate GetButtonBinding (string name)
		{
			object binding = FindBinding<ButtonBindingDelegate> ("ButtonBinding_" + name);
			return (ButtonBindingDelegate) binding;
		}





		void DumpLine (string name, float value)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (name);
			GUILayout.FlexibleSpace ();
			GUILayout.Label (value.ToString ("G3"));
			GUILayout.EndHorizontal ();
		}

		void DumpLine (string name, int value)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (name);
			GUILayout.FlexibleSpace ();
			GUILayout.Label (value.ToString ());
			GUILayout.EndHorizontal ();
		}

		void DumpState ()
		{
			GUILayout.BeginVertical (GUILayout.Width (180));
			DumpLine ("mThrot", ctrlState.mainThrottle);
			DumpLine ("roll", ctrlState.roll);
			DumpLine ("yaw", ctrlState.yaw);
			DumpLine ("pitch", ctrlState.pitch);
			DumpLine ("wheelSteer", ctrlState.wheelSteer);
			DumpLine ("wheelThrottle", ctrlState.wheelThrottle);
			DumpLine ("X", ctrlState.X);
			DumpLine ("Y", ctrlState.Y);
			DumpLine ("Z", ctrlState.Z);
			GUILayout.EndVertical ();
		}

		void DumpAxes (Device dev)
		{
			GUILayout.BeginVertical (GUILayout.Width (180));

			for (int i = 0; i < dev.num_axes; i++) {
				DumpLine (dev.AxisName (i), dev.AxisValue (i));
			}

			GUILayout.EndVertical ();
		}

		void DumpButtons (Device dev)
		{
			GUILayout.BeginVertical (GUILayout.Width (180));

			for (int i = 0; i < dev.num_buttons; i++) {
				if (i > 0 && (i % 20) == 0) {
					GUILayout.EndVertical ();
					GUILayout.BeginVertical (GUILayout.Width (180));
				}
				DumpLine (dev.ButtonName (i), dev.ButtonState (i));
			}

			GUILayout.EndVertical ();
		}

		int devidx;
		static Rect windowpos;
		void WindowGUI (int windowID)
		{
			if (devices.Count < 1) {
				return;
			}
			Device dev = devices[devidx];
			if (GUILayout.Button (dev.name)) {
				if (++devidx >= devices.Count) {
					devidx = 0;
				}
			}
			GUILayout.BeginHorizontal ();
			DumpState ();
			DumpAxes (dev);
			DumpButtons (dev);
			GUILayout.EndHorizontal ();
			GUI.DragWindow (new Rect (0, 0, 10000, 20));
		}

		void OnGUI ()
		{
			GUI.skin = HighLogic.Skin;
			if (windowpos.x == 0) {
				windowpos = new Rect (Screen.width / 2 - 250,
					Screen.height / 2 - 30, 0, 0);
			}
			string name = "Advanced Input";
			string ver = AdvancedInputVersionReport.GetVersion ();
			windowpos = GUILayout.Window (GetInstanceID (),
				windowpos, WindowGUI,
				name + " " + ver,
				GUILayout.Width (500));
		}
	}
}

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
		public FlightCtrlState ctrlState;

		public static bool overrideMainThrottle;
		public static bool overrideWheelThrottle;
		public static bool blockApplicationFocusLock;

		public float prevMainThrottle;
		public float prevWheelThrottle;
		public bool mainThrottleLock;
		public bool wheelThrottleLock;

		static Dictionary <string, ConstructorInfo> axisBindings;
		static Dictionary <string, ConstructorInfo> buttonBindings;

		List<Device> devices;

		public static AI_FlightControl instance { get; private set; }

		public static void UpdateTrim (ref float trim, float value)
		{
			trim = Mathf.Clamp (trim + 0.1f * value * Time.deltaTime, -1f, 1f);
		}

		static void DiscoverAxisBindings ()
		{
			axisBindings = new Dictionary <string, ConstructorInfo> ();
			var modules = AssemblyLoader.GetModulesImplementingInterface<IAxisBinding> (new Type[] {typeof (AI_FlightControl), typeof (ConfigNode)});
			var parms = new object [] {null, null};
			foreach (var mod in modules) {
				// create a dummy module in order to get its name
				var ab = (IAxisBinding) mod.Invoke (parms);
				Debug.LogFormat ("[AI_FlightControl] DiscoverAxisBindings: {0}", ab.name);
				axisBindings[ab.name] = mod;
			}
		}

		static void DiscoverButtonBindings ()
		{
			buttonBindings = new Dictionary <string, ConstructorInfo> ();
			var modules = AssemblyLoader.GetModulesImplementingInterface<IButtonBinding> (new Type[] {typeof (AI_FlightControl), typeof (ConfigNode)});
			var parms = new object [] {null, null};
			foreach (var mod in modules) {
				// create a dummy module in order to get its name
				var bb = (IButtonBinding) mod.Invoke (parms);
				Debug.LogFormat ("[AI_FlightControl] DiscoverButtonBindings: {0}", bb.name);
				buttonBindings[bb.name] = mod;
			}
		}

		void Awake ()
		{
			if (axisBindings == null) {
				DiscoverAxisBindings ();
				DiscoverButtonBindings ();
			}
			instance = this;
			GameEvents.onVesselChange.Add (OnVesselChange);
			GameEvents.onInputLocksModified.Add (OnInputLocksModified);
			ctrlState = new FlightCtrlState ();
			devices = new List<Device> ();
			prevMainThrottle = -2;
			prevMainThrottle = -2;
		}

		void OnDestroy ()
		{
			instance = null;
			GameEvents.onVesselChange.Remove (OnVesselChange);
			GameEvents.onInputLocksModified.Remove (OnInputLocksModified);
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

		const string appLock = "flightDriver_ApplicationFocus";
		void OnInputLocksModified (GameEvents.FromToAction<ControlTypes, ControlTypes> fromto)
		{
			if (blockApplicationFocusLock) {
				if (InputLockManager.lockStack.ContainsKey (appLock)) {
					InputLockManager.RemoveControlLock (appLock);
				}
			}
			for (int i = devices.Count; i-- > 0; ) {
				devices[i].UpdateInputLock (InputLockManager.lockMask);
			}
		}

		void OnFlyByWire (FlightCtrlState state)
		{
			InputLib.Device.CheckInput ();
			for (int i = devices.Count; i-- > 0; ) {
				devices[i].CheckInput ();
			}
			if (!ctrlState.isIdle) {
				state.roll = ctrlState.roll + state.rollTrim;
				state.pitch = ctrlState.pitch + state.pitchTrim;
				state.yaw = ctrlState.yaw + state.yawTrim;
				state.wheelSteer = ctrlState.wheelSteer + state.wheelSteerTrim;
			}

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

		public static IAxisBinding GetAxisBinding (ConfigNode node)
		{
			string name = node.GetValue ("binding");
			ConstructorInfo module;
			IAxisBinding binding = null;
			if (name != null && axisBindings.TryGetValue (name, out module)) {
				var parms = new object [] {instance, node};
				binding = (IAxisBinding) module.Invoke (parms);
			}
			return binding;
		}

		public static IButtonBinding GetButtonBinding (ConfigNode node)
		{
			string name = node.GetValue ("binding");
			ConstructorInfo module;
			IButtonBinding binding = null;
			if (name != null && buttonBindings.TryGetValue (name, out module)) {
				var parms = new object [] {instance, node};
				binding = (IButtonBinding) module.Invoke (parms);
			}
			return binding;
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

		void DumpState (FlightCtrlState state)
		{
			GUILayout.BeginVertical (GUILayout.Width (180));
			DumpLine ("mThrot", state.mainThrottle);
			DumpLine ("roll", state.roll);
			DumpLine ("yaw", state.yaw);
			DumpLine ("pitch", state.pitch);
			DumpLine ("wheelSteer", state.wheelSteer);
			DumpLine ("wheelThrottle", state.wheelThrottle);
			DumpLine ("X", state.X);
			DumpLine ("Y", state.Y);
			DumpLine ("Z", state.Z);
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
			DumpState (FlightInputHandler.state);
			DumpState (ctrlState);
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

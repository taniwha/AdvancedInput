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
		float ctrlPitch, ctrlYaw, ctrlRoll;

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
			var modules = AssemblyLoader.GetModulesImplementingInterface<IAxisBinding> (new Type[] {typeof (ConfigNode)});
			var parms = new object [] {null};
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
			var modules = AssemblyLoader.GetModulesImplementingInterface<IButtonBinding> (new Type[] {typeof (ConfigNode)});
			var parms = new object [] {null};
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
				currentVessel.OnPreAutopilotUpdate -= ControlUpdate;
				currentVessel.OnPostAutopilotUpdate -= OverrideSAS;
			}
			currentVessel = vessel;
			if (vessel != null) {
				vessel.OnPreAutopilotUpdate += ControlUpdate;
				currentVessel.OnPostAutopilotUpdate += OverrideSAS;
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

		float ClampedAdd (float a, float b)
		{
			return Mathf.Clamp (a + b, -1f, 1f);
		}

		void ControlUpdate (FlightCtrlState state)
		{
			InputLib.Device.CheckInput ();
			for (int i = devices.Count; i-- > 0; ) {
				devices[i].CheckInput ();
			}

			ctrlRoll = ctrlState.roll;
			state.roll = ClampedAdd (state.roll, ctrlRoll);
			ctrlPitch = ctrlState.pitch;
			state.pitch = ClampedAdd (state.pitch, ctrlPitch);
			ctrlYaw = ctrlState.yaw;
			state.yaw = ClampedAdd (state.yaw, ctrlYaw);
			state.wheelSteer = ClampedAdd (state.wheelSteer,
										   ctrlState.wheelSteer);

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

		bool Different (float a, float b)
		{
			return Mathf.Abs (a - b) > 0.01;
		}

		bool OverridingSAS (float user, float sas)
		{
			// user and sas are in opposite directions
			if (user * sas < 0) {
				return true;
			}
			// user is larger than sas
			if (Math.Abs (user) > Math.Abs (sas)) {
				return true;
			}
			return false;
		}

		void OverrideSAS (FlightCtrlState state)
		{
			if (!currentVessel.ActionGroups[KSPActionGroup.SAS]) {
				// SAS is off, so no need to do anything
				return;
			}
			if (Different (ctrlPitch, state.pitch)
				&& OverridingSAS (ctrlPitch, state.pitch)) {
				state.pitch = ctrlPitch;
			}
			if (Different (ctrlYaw, state.yaw)
				&& OverridingSAS (ctrlYaw, state.yaw)) {
				state.yaw = ctrlYaw;
			}
			if (Different (ctrlRoll, state.roll)
				&& OverridingSAS (ctrlRoll, state.roll)) {
				state.roll = ctrlRoll;
			}
		}

		public static IAxisBinding GetAxisBinding (ConfigNode node)
		{
			string name = node.GetValue ("binding");
			ConstructorInfo module;
			IAxisBinding binding = null;
			if (name != null && axisBindings.TryGetValue (name, out module)) {
				var parms = new object [] {node};
				binding = (IAxisBinding) module.Invoke (parms);
			}
			if (binding == null) {
				Debug.LogError (String.Format ("[AdvancedInput] axis binding '{0}' not found", name));
			}
			return binding;
		}

		public static IButtonBinding GetButtonBinding (ConfigNode node)
		{
			string name = node.GetValue ("binding");
			ConstructorInfo module;
			IButtonBinding binding = null;
			if (name != null && buttonBindings.TryGetValue (name, out module)) {
				var parms = new object [] {node};
				binding = (IButtonBinding) module.Invoke (parms);
			}
			if (binding == null) {
				Debug.LogError (String.Format ("[AdvancedInput] button binding '{0}' not found", name));
			}
			return binding;
		}
/*
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
				DumpLine (dev.AxisName (i), dev.AxisValue (i, false));
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
			if (currentVessel != null) {
				DumpState (currentVessel.ctrlState);
			}
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
*/
	}
}

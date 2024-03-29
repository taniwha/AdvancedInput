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
using UnityEngine;

namespace AdvancedInput {
	using InputLibWrapper;

	public delegate void AxisBindingDelegate (float value, bool updated);
	public delegate void ButtonBindingDelegate (int state, bool edge);

	[KSPAddon (KSPAddon.Startup.Flight, false)]
	public class AI_FlightControl : MonoBehaviour
	{
		public FlightCtrlState ctrlState;
		public KerbalCtrlState kerbalState;

		public static bool overrideMainThrottle;
		public static bool overrideWheelThrottle;
		public static bool blockApplicationFocusLock;

		public float prevMainThrottle;
		public float prevWheelThrottle;
		public bool mainThrottleLock;
		public bool wheelThrottleLock;

		// so bindings can register without worrying about deregistering
		public EventData<Vessel> onVesselChange;

		static Dictionary <string, ConstructorInfo> axisBindings;
		static Dictionary <string, ConstructorInfo> buttonBindings;

		public List<Device> devices { get; private set; }

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
			onVesselChange = new EventData<Vessel>("onVesselChange");
			GameEvents.onVesselChange.Add (OnVesselChange);
			GameEvents.onInputLocksModified.Add (OnInputLocksModified);
			ctrlState = new FlightCtrlState ();
			kerbalState = new KerbalCtrlState ();
			devices = new List<Device> ();
			prevMainThrottle = -2;
			prevMainThrottle = -2;
		}

		void PostMessage (string fmt, params string[] strs)
		{
			string message = String.Format (fmt, strs);
			ScreenMessages.PostScreenMessage (message, 3.0f, ScreenMessageStyle.UPPER_CENTER);
		}

		void GainedDevice (Device dev)
		{
			PostMessage ("{0} {1}.", "Gained connection to", dev.name);
		}

		void LostDevice (Device dev)
		{
			PostMessage ("<color=orange>{0} {1}.</color>", "Lost connection to", dev.name);
		}

		void DeviceAdded (InputLibWrapper.Device rawdev)
		{
			Device dev = new Device (rawdev);
			devices.Add (dev);
			GainedDevice (dev);
		}

		void DeviceRemoved (InputLibWrapper.Device rawdev)
		{
			for (int i = devices.Count; i-- > 0; ) {
				if (devices[i].rawDevice == rawdev) {
					LostDevice (devices[i]);
					devices.RemoveAt (i);
					break;
				}
			}
		}

		void OnDestroy ()
		{
			instance = null;
			GameEvents.onVesselChange.Remove (OnVesselChange);
			GameEvents.onInputLocksModified.Remove (OnInputLocksModified);
			InputLib.DeviceAdded -= DeviceAdded;
			InputLib.DeviceRemoved -= DeviceRemoved;
			FlightInputHandler.OnRawAxisInput -= ControlUpdate;
		}

		void Start ()
		{
			Debug.LogFormat ("[AI_FlightControl] Start {0}", GetInstanceID ());
			InputLib.DeviceAdded += DeviceAdded;
			InputLib.DeviceRemoved += DeviceRemoved;
			foreach (var rawdev in InputLib.devices) {
				if (rawdev != null) {
					DeviceAdded (rawdev);
				}
			}
			FlightInputHandler.OnRawAxisInput = ControlUpdate + FlightInputHandler.OnRawAxisInput;
		}

		void OnVesselChange(Vessel vessel)
		{
			Debug.LogFormat ("[AI_FlightControl] OnVesselChange {0}", GetInstanceID ());
			onVesselChange.Fire (vessel);
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

		public void CheckInput ()
		{
			while (InputLib.CheckInput ()) {
				for (int i = devices.Count; i-- > 0; ) {
					devices[i].CheckInput ();
				}
			}
		}

		void ControlUpdate (FlightCtrlState state)
		{
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

			CheckInput ();

			float ctrlRoll = ctrlState.roll;
			state.roll = ClampedAdd (state.roll, ctrlRoll);
			float ctrlPitch = ctrlState.pitch;
			state.pitch = ClampedAdd (state.pitch, ctrlPitch);
			float ctrlYaw = ctrlState.yaw;
			state.yaw = ClampedAdd (state.yaw, ctrlYaw);
			state.wheelSteer = ClampedAdd (state.wheelSteer,
										   ctrlState.wheelSteer);

			state.mainThrottle = ctrlState.mainThrottle;
			state.wheelThrottle = ctrlState.wheelThrottle;
		}

		bool Different (float a, float b)
		{
			return Mathf.Abs (a - b) > 0.01;
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
	}
}

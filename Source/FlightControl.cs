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
		static FlightCtrlState ctrlState;

		public static bool overrideMainThrottle;
		public static bool overrideWheelThrottle;

		static bool updateMainThrottle;
		static bool updateWheelThrottle;

		void Awake ()
		{
			GameEvents.onVesselChange.Add (OnVesselChange);
			ctrlState = new FlightCtrlState ();
		}

		void OnDestroy ()
		{
			GameEvents.onVesselChange.Remove (OnVesselChange);
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

			if (overrideMainThrottle || updateMainThrottle) {
				state.mainThrottle = ctrlState.mainThrottle;
				updateMainThrottle = false;
			}
			if (overrideWheelThrottle || updateWheelThrottle) {
				state.wheelThrottle = ctrlState.wheelThrottle;
				updateWheelThrottle = false;
			}
		}

		public static void AxisBinding_MainThrottle (float value, bool updated)
		{
			updateMainThrottle = updated;
			ctrlState.mainThrottle = value;
		}

		public static void AxisBinding_WheelThrottle (float value, bool updated)
		{
			updateWheelThrottle = updated;
			ctrlState.wheelThrottle = value;
		}

		public static void AxisBinding_Pitch (float value, bool updated)
		{
			ctrlState.pitch = value;
		}

		public static void AxisBinding_Yaw (float value, bool updated)
		{
			ctrlState.yaw = value;
		}

		public static void AxisBinding_Roll (float value, bool updated)
		{
			ctrlState.roll = value;
		}

		public static void AxisBinding_X (float value, bool updated)
		{
			ctrlState.X = value;
		}

		public static void AxisBinding_Y (float value, bool updated)
		{
			ctrlState.Y = value;
		}

		public static void AxisBinding_Z (float value, bool updated)
		{
			ctrlState.Z = value;
		}

		public static void AxisBinding_WheelSteer (float value, bool updated)
		{
			ctrlState.wheelSteer = value;
		}

		const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static;
		static object FindBinding<T> (string name)
		{
			Type type = typeof (AI_FlightControl);
			var method = type.GetMethod (name, bindingFlags);
			if (method == null) {
				return null;
			}
			return Delegate.CreateDelegate(typeof(T), method);
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
	}
}

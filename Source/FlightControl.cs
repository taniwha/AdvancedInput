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
using System.Collections.Generic;
using UnityEngine;

using KSP.IO;

namespace AdvancedInput {

	[KSPAddon (KSPAddon.Startup.Flight, false)]
	public class AI_FlightControl : MonoBehaviour
	{
		FlightCtrlState ctrlState;

		public static bool overrideMainThrottle;
		public static bool overrideWheelThrottle;

		bool updateMainThrottle;
		bool updateWheelThrottle;

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

		void UpdateMainThrottle (float value)
		{
			updateMainThrottle = ctrlState.mainThrottle != value;
			ctrlState.mainThrottle = value;
		}

		void UpdateWheelThrottle (float value)
		{
			updateWheelThrottle = ctrlState.wheelThrottle != value;
			ctrlState.wheelThrottle = value;
		}

		void UpdatePitch (float value)
		{
			ctrlState.pitch = value;
		}

		void UpdateYaw (float value)
		{
			ctrlState.yaw = value;
		}

		void UpdateRoll (float value)
		{
			ctrlState.roll = value;
		}

		void UpdateX (float value)
		{
			ctrlState.X = value;
		}

		void UpdateY (float value)
		{
			ctrlState.Y = value;
		}

		void UpdateZ (float value)
		{
			ctrlState.Z = value;
		}

		void UpdateWheelSteer (float value)
		{
			ctrlState.wheelSteer = value;
		}
	}
}

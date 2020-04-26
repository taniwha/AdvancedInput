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
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

using KSP.IO;
using Harmony;

namespace AdvancedInput {
	[HarmonyPatch(typeof(VesselAutopilot.VesselSAS))]
	[HarmonyPatch("CheckPitchInput")]
	class VesselAutopilot_VesselSAS_CheckPitchInput
	{
		static bool Prefix (VesselAutopilot.VesselSAS __instance, Vessel ___vessel, ref bool __result)
		{
			if (FlightGlobals.ActiveVessel != ___vessel) {
				__result = false;
				// this is what VesselSAS does anyway, so no point in calling it
				return false;
			}
			if (Mathf.Abs (AI_FlightControl.instance.ctrlState.pitch) > __instance.controlDetectionThreshold) {
				__result = true;
				// short circuit the logic
				return false;
			}
			// allow VesselSAS to check keyboard and other inputs
			return true;
		}
	}

	[HarmonyPatch(typeof(VesselAutopilot.VesselSAS))]
	[HarmonyPatch("CheckYawInput")]
	class VesselAutopilot_VesselSAS_CheckYawInput
	{
		static bool Prefix (VesselAutopilot.VesselSAS __instance, Vessel ___vessel, ref bool __result)
		{
			if (FlightGlobals.ActiveVessel != ___vessel) {
				__result = false;
				// this is what VesselSAS does anyway, so no point in calling it
				return false;
			}
			if (Mathf.Abs (AI_FlightControl.instance.ctrlState.yaw) > __instance.controlDetectionThreshold) {
				__result = true;
				// short circuit the logic
				return false;
			}
			// allow VesselSAS to check keyboard and other inputs
			return true;
		}
	}

	[HarmonyPatch(typeof(VesselAutopilot.VesselSAS))]
	[HarmonyPatch("CheckRollInput")]
	class VesselAutopilot_VesselSAS_CheckRollInput
	{
		static bool Prefix (VesselAutopilot.VesselSAS __instance, Vessel ___vessel, ref bool __result)
		{
			if (FlightGlobals.ActiveVessel != ___vessel) {
				__result = false;
				// this is what VesselSAS does anyway, so no point in calling it
				return false;
			}
			if (Mathf.Abs (AI_FlightControl.instance.ctrlState.roll) > __instance.controlDetectionThreshold) {
				__result = true;
				// short circuit the logic
				return false;
			}
			// allow VesselSAS to check keyboard and other inputs
			return true;
		}
	}
}

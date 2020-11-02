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
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Linq;
using UnityEngine;

using KSP.IO;
using Harmony;

namespace AdvancedInput {
	[HarmonyPatch(typeof(KerbalEVA))]
	[HarmonyPatch("HandleMovementInput")]
	class KerbalEVA_HandleMovementInput
	{
		static void Postfix (KerbalEVA __instance, ref Vector3 ___tgtFwd, ref Vector3 ___tgtUp, ref Vector3 ___tgtRpos, ref Vector3 ___ladderTgtRPos, ref Vector3 ___packTgtRPos, ref Vector3 ___cmdRot, ref Vector3 ___parachuteInput, ref bool ___manualAxisControl)
		{
			Vessel vessel = __instance.vessel;
			if (vessel.state != Vessel.State.ACTIVE || vessel.packed) {
				return;
			}
			// Get here only if the kerbal is the active vessel, in which
			// case AI_FlightControl vessel control hook is not called, so
			// need to check input ourselves
			AI_FlightControl.instance.CheckInput ();

			Transform transform = __instance.transform;
			KerbalCtrlState ctrl = AI_FlightControl.instance.kerbalState;

			if (ctrl.translation != Vector3.zero) {
				var trans = new Vector2 (ctrl.translation.x, ctrl.translation.z).normalized;
				if (__instance.CharacterFrameMode) {
					___tgtRpos = transform.forward * trans.y;
					___tgtRpos += transform.right * trans.x;
				} else {
					___tgtRpos = __instance.fFwd * trans.y;
					___tgtRpos += __instance.fRgt * trans.x;
				}

				___ladderTgtRPos = transform.up * ctrl.translation.z;
				___ladderTgtRPos += transform.right * ctrl.translation.x;

				___packTgtRPos = transform.right * ctrl.translation.x;
				___packTgtRPos += transform.up * ctrl.translation.y;
				___packTgtRPos += transform.forward * ctrl.translation.z;
			}

			if (ctrl.rotation != Vector3.zero) {
				___cmdRot = -transform.right * ctrl.rotation.x;
				___cmdRot += transform.up * ctrl.rotation.y;
				___cmdRot -= transform.forward * ctrl.rotation.z;
				___manualAxisControl = true;
			}
			if (ctrl.angularBrake > 0) {
				Vector3 w = __instance.part.Rigidbody.angularVelocity;
				float W = Vector3.Dot (w, w);

				if (W > 1) {
					w /= Mathf.Sqrt (W);
				}
				___cmdRot += -w * ctrl.angularBrake;
			}

			if (ctrl.parachute != Vector2.zero) {
				___parachuteInput.x = ctrl.parachute.x;
				___parachuteInput.y = ctrl.parachute.y;
			}

			if (vessel.LandedOrSplashed) {
				___manualAxisControl = false;
			}

			if (!___manualAxisControl
				&& (GameSettings.EVA_ROTATE_ON_MOVE || vessel.LandedOrSplashed)
				&& ctrl.translation != Vector3.zero) {
				___manualAxisControl = false;
				if (__instance.CharacterFrameMode) {
					___tgtFwd = __instance.fFwd;
					___tgtUp = __instance.fUp;
				} else {
					___tgtFwd = ___tgtRpos;
					___tgtUp = __instance.fUp;
				}
			}
		}
	}
}

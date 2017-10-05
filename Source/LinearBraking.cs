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
using System.IO;
using System.Collections.Generic;
using UnityEngine;

using ModuleWheels;

namespace AdvancedInput {

	public class AI_LinearBraking : VesselModule
	{
		List<ModuleWheelBrakes> leftBrakes;
		List<ModuleWheelBrakes> rightBrakes;
		List<ModuleWheelBrakes> centerBrakes;

		float sideEpsilon = 0.05f;	// 5cm slop should be good enough

		// center braking is the average of left and right braking, but only
		// if both are non-zero FIXME is this a good idea, or should it always
		// be the average and any central wheels have their brakes disabled?
		public float leftBraking;
		public float rightBraking;

		public override Activation GetActivation ()
		{
			return Activation.FlightScene | Activation.LoadedVessels;
		}

		void AllocateBrake (Part part, ModuleWheelBrakes brake)
		{
			Transform reference = vessel.ReferenceTransform;

			Vector3 offset = part.transform.position - reference.position;
			float side = Vector3.Dot(offset, reference.right);
			if (side > sideEpsilon) {
				rightBrakes.Add (brake);
			} else if (side < -sideEpsilon) {
				leftBrakes.Add (brake);
			} else {
				centerBrakes.Add (brake);
			}
		}

		public override void OnLoadVessel ()
		{
			leftBrakes = new List<ModuleWheelBrakes> ();
			rightBrakes = new List<ModuleWheelBrakes> ();
			centerBrakes = new List<ModuleWheelBrakes> ();

			for (int i = vessel.parts.Count; i-- > 0; ) {
				Part p = vessel.parts[i];
				var brakes = p.FindModulesImplementing<ModuleWheelBrakes> ();
				for (int j = brakes.Count; j-- > 0; ) {
					AllocateBrake (p, brakes[j]);
				}
			}
		}

		public override void OnUnloadVessel ()
		{
			// clear out the lists to ensure any attept to access unloaded
			// parts fails early
			leftBrakes = null;
			rightBrakes = null;
			centerBrakes = null;
		}

		void UpdateBrakeInput (ModuleWheelBrakes brake, float brakeInput)
		{
			brake.brakeInput = brakeInput;
			if (brake.statusLight != null) {
				bool status = brakeInput * brake.brakeTweakable > 0;
				if (brake.statusLight.IsOn != status) {
					brake.statusLight.SetStatus (status);
				}
			}
		}

		void FixedUpdate ()
		{
			if (vessel.ActionGroups[KSPActionGroup.Brakes]) {
				// brake "lock" is applied, so let it override
				return;
			}

			float centerBraking = 0;
			if (leftBraking > 0 && rightBraking > 0) {
				centerBraking = (leftBraking + rightBraking) / 2;
			}

			for (int i = leftBrakes.Count; i-- > 0; ) {
				UpdateBrakeInput (leftBrakes[i], leftBraking);
			}
			for (int i = centerBrakes.Count; i-- > 0; ) {
				UpdateBrakeInput (centerBrakes[i], centerBraking);
			}
			for (int i = rightBrakes.Count; i-- > 0; ) {
				UpdateBrakeInput (rightBrakes[i], rightBraking);
			}
		}

		public void UpdateBraking (float braking)
		{
			leftBraking = rightBraking = braking;
		}

		public void UpdateLeftBraking (float braking)
		{
			leftBraking = braking;
		}

		public void UpdateRightBraking (float braking)
		{
			rightBraking = braking;
		}

		public void UpdateBraking (float leftBraking, float rightBraking)
		{
			this.leftBraking = leftBraking;
			this.rightBraking = rightBraking;
		}
	}
}

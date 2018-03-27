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

using KSP.IO;

namespace AdvancedInput.AxisBindings {

	public class AI_AB_LinearBrake: IAxisBinding
	{
		public string name { get { return "LinearBrake"; } }
		public ControlTypes lockMask { get { return ControlTypes.GROUP_BRAKES; } }
		public bool locked { get; set; }

		public enum BrakeSide
		{
			left,
			center,
			right,
		}

		delegate void UpdateBraking(float braking);
		UpdateBraking updateBraking;

		AI_LinearBraking linearBrakingModule;
		BrakeSide side;

		public void Update (float value, bool updated)
		{
			if (updated) {
				updateBraking (value);
				//Debug.LogFormat ("[AI_AB_LinearBrake] {0} {1} {2} {3}", side,
				//				 value, linearBrakingModule.leftBraking,
				//				 linearBrakingModule.rightBraking);
			}
		}

		void UpdateBrakingDelegate (AI_LinearBraking lb)
		{
			linearBrakingModule = lb;
			switch (side) {
				case BrakeSide.left:
					updateBraking = linearBrakingModule.UpdateLeftBraking;
					break;
				case BrakeSide.center:
					updateBraking = linearBrakingModule.UpdateBraking;
					break;
				case BrakeSide.right:
					updateBraking = linearBrakingModule.UpdateRightBraking;
					break;
			}
			//Debug.LogFormat ("[AI_AB_LinearBrake] {0} {1}", side, updateBraking);
		}

		void onVesselChange (Vessel vessel)
		{
			//Debug.LogFormat ("[AI_AB_LinearBrake] {0} {1}", side, vessel);
			for (int i = vessel.vesselModules.Count; i-- > 0; ) {
				VesselModule vm = vessel.vesselModules[i];
				if (vm is AI_LinearBraking) {
					UpdateBrakingDelegate (vm as AI_LinearBraking);
					break;
				}
			}
		}

		BrakeSide ParseSide (string name)
		{
			if (name == null) {
				name = String.Empty;
			} else {
				name = name.ToLower ();
			}
			return AI_Utils.ToEnum<BrakeSide> (name, BrakeSide.center);
		}

		public AI_AB_LinearBrake (ConfigNode node)
		{
			if (node != null) {
				side = ParseSide (node.GetValue ("side"));
				//Debug.LogFormat ("[AI_AB_LinearBrake] {0} {1}", side, node);
				AI_FlightControl.instance.onVesselChange.Add (onVesselChange);
				if (FlightGlobals.ActiveVessel != null) {
					onVesselChange(FlightGlobals.ActiveVessel);
				}
			}
		}
	}
}

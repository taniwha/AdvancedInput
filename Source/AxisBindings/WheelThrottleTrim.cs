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

namespace AdvancedInput.AxisBindings {

	public class AI_AB_WheelThrottleTrim: IAxisBinding
	{
		public string name { get { return "WheelThrottleTrim"; } }
		public ControlTypes lockMask { get { return ControlTypes.WHEEL_THROTTLE; } }
		public bool locked { get; set; }

		public void Update (float value, bool updated)
		{
			AI_FlightControl.UpdateTrim (ref FlightInputHandler.state.wheelThrottleTrim, value);
		}

		public AI_AB_WheelThrottleTrim (AI_FlightControl fc, ConfigNode node)
		{
		}
	}
}

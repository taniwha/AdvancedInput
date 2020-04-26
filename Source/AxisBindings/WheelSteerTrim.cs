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

namespace AdvancedInput.AxisBindings {

	public class AI_AB_WheelSteerTrim: IAxisBinding
	{
		public string name { get { return "WheelSteerTrim"; } }
		public ControlTypes lockMask { get { return ControlTypes.WHEEL_STEER; } }
		public bool locked { get; set; }

		public void Update (float value, bool updated)
		{
			AI_FlightControl.UpdateTrim (ref FlightInputHandler.state.wheelSteerTrim, value);
		}

		public string GetParameters ()
		{
			return "";
		}

		public AI_AB_WheelSteerTrim (ConfigNode node)
		{
		}
	}
}

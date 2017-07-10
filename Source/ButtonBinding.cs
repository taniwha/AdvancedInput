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

namespace AdvancedInput {

	public class ButtonBinding
	{
		public int index { get; private set; }
		public Device device { get; private set; }
		public ButtonBindingDelegate binding { get; private set; }
		public int prevState { get; private set; }

		public ButtonBinding (Device dev, ConfigNode node)
		{
			int ind;
			if (int.TryParse (node.GetValue ("index"), out ind)) {
				index = ind;
			}

			binding = AI_FlightControl.GetButtonBinding (node.GetValue ("binding"));

			device = dev;

			prevState = device.ButtonState (index);
		}

		public void Update ()
		{
			int state = device.ButtonState (index);
			bool edge = state != prevState;
			prevState = state;
			if (binding != null) {
				binding (state, edge);
			}
		}
	}
}

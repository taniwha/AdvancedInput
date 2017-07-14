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

	public interface IAxisBinding
	{
		string name { get; }
		ControlTypes lockMask { get; }
		bool locked { get; set; }
		void Update (float value, bool updated);
	}

	public class AxisBinding
	{
		public int index { get; private set; }
		public Device device { get; private set; }
		public IAxisBinding binding { get; private set; }
		public float prevValue { get; private set; }

		public AxisBinding (Device dev, ConfigNode node)
		{
			int ind;
			if (int.TryParse (node.GetValue ("index"), out ind)) {
				index = ind;
			}

			binding = AI_FlightControl.GetAxisBinding (node);

			device = dev;

			prevValue = device.AxisValue (index);
		}

		public void Update ()
		{
			float value = device.AxisValue (index);
			bool updated = value != prevValue;
			prevValue = value;
			if (binding != null && !binding.locked) {
				binding.Update (value, updated);
			}
		}

		public void UpdateInputLock (ulong mask)
		{
			if (binding != null) {
				binding.locked = ((ulong)binding.lockMask & mask) != 0;
			}
		}
	}
}

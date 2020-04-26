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
		string GetParameters ();
	}

	public class AxisBinding
	{
		public int index { get; private set; }
		public BindingSet bindingSet { get; private set; }
		public IAxisBinding binding { get; private set; }
		public float prevValue { get; private set; }
		public bool invert { get; private set; }
		public float curve { get; private set; }

		public AxisBinding (BindingSet bs, ConfigNode node)
		{
			int ind;
			if (int.TryParse (node.GetValue ("index"), out ind)) {
				index = ind;
			}

			binding = AI_FlightControl.GetAxisBinding (node);

			bool b;
			if (bool.TryParse (node.GetValue ("invert"), out b)) {
				invert = b;
			}

			float c;
			if (float.TryParse (node.GetValue ("curve"), out c)) {
				curve = c;
			}

			bindingSet = bs;

			prevValue = bindingSet.AxisValue (index, invert);
		}

		public float Value ()
		{
			float value = bindingSet.AxisValue (index, invert);

			if (curve > 0 && curve != 1) {
				if (value > 0) {
					value = Mathf.Pow (value, curve);
				} else if (value < 0) {
					value = -Mathf.Pow (-value, curve);
				}
			}

			return value;
		}

		public void Update ()
		{
			if (binding == null) {
				return;
			}
			float value = Value ();
			bool updated = value != prevValue;
			prevValue = value;

			if (!binding.locked) {
				binding.Update (value, updated);
			}
		}

		public void UpdateInputLock (ulong mask)
		{
			if (binding != null) {
				binding.locked = ((ulong)binding.lockMask & mask) != 0;
			}
			Update ();
		}
	}
}

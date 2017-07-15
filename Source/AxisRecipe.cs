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

	public class AxisRecipe
	{
		public enum RecipeType {
			Balanced,
			Unbalanced,
		};

		public int axis { get; private set; }
		public int deadzone { get; private set; }
		public bool balanced { get; private set; }
		public bool inverted { get; private set; }

		public AxisRecipe ()
		{
			balanced = true;
			inverted = false;
		}

		public AxisRecipe (ConfigNode node)
		{
			bool b;
			int i;

			if (int.TryParse (node.GetValue ("index"), out i)) {
				axis = i;
			}
			if (int.TryParse (node.GetValue ("deadzone"), out i)) {
				deadzone = i;
			}
			if (bool.TryParse (node.GetValue ("balanced"), out b)) {
				balanced = b;
			}
			if (bool.TryParse (node.GetValue ("inverted"), out b)) {
				inverted = b;
			}
		}

		public float Process (ref InputLib.Axis axis, bool invert)
		{
			float dz = deadzone;
			float value = axis.value;
			float min = axis.min;
			float max = axis.max;
			float range = max - min - dz;

			invert ^= inverted;

			if (balanced) {
				float mid = (min + max) / 2;
				range = (range - dz) / 2;

				if (invert) {
					value = (mid - value);
				} else {
					value = (value - mid);
				}
			} else {
				if (invert) {
					value = (max - value);
				} else {
					value = (value - min);
				}
			}
			if (value > dz) {
				value -= dz;
			} else if (value < -dz) {
				value += dz;
			} else {
				value = 0;
			}
			return value / range;
		}
	}
}

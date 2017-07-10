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
			if (bool.TryParse (node.GetValue ("balanced"), out b)) {
				balanced = b;
			}
			if (bool.TryParse (node.GetValue ("inverted"), out b)) {
				inverted = b;
			}
		}

		public float Process (ref InputLib.Axis axis)
		{
			float value = axis.value;
			float min = axis.min;
			float max = axis.max;

			if (balanced) {
				float mid = (min + max) / 2;
				float range = (max - min) / 2;

				if (inverted) {
					value = (mid - value) / range;
				} else {
					value = (value - mid) / range;
				}
			} else {
				if (inverted) {
					value = (max - value) / (max - min);
				} else {
					value = (value - min) / (max - min);
				}
			}
			return value;
		}
	}
}

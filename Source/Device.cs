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

	public class Device
	{
		public List<AxisBinding> axisBindings { get; private set; }
		public List<ButtonBinding> buttonBindings { get; private set; }

		AxisRecipe[] axisRecipes;

		InputLib.Device rawDevice;

		public Device (InputLib.Device dev)
		{
			rawDevice = dev;
			axisRecipes = new AxisRecipe[dev.axes.Length];
			for (int i = 0; i < axisRecipes.Length; i++) {
				axisRecipes[i] = new AxisRecipe ();
			}
		}

		public int ButtonState (int index)
		{
			return rawDevice.buttons[index].state;
		}

		public float AxisValue (int index)
		{
			AxisRecipe recipe = axisRecipes[index];
			return recipe.Process (ref rawDevice.axes[index]);
		}
	}
}

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

	public class BindingSet
	{
		public List<AxisBinding> axisBindings { get; private set; }
		public List<ButtonBinding> buttonBindings { get; private set; }

		AxisRecipe[] axisRecipes;
		static AxisRecipe defaultRecipe;

		InputLibWrapper.Device rawDevice;

		public string name { get; private set; }

		void ParseConfig (ConfigNode node)
		{
			name = "";
			if (node.HasValue ("name")) {
				name = node.GetValue ("name");
			}

			foreach (ConfigNode n in node.nodes) {
				switch (n.name) {
					case "AxisRecipe":
						var ar = new AxisRecipe (n);
						axisRecipes[ar.axis] = ar;
						break;
					case "AxisBinding":
						var ab = new AxisBinding (this, n);
						axisBindings.Add (ab);
						break;
					case "ButtonBinding":
						var bb = new ButtonBinding (this, n);
						buttonBindings.Add (bb);
						break;
				}
			}
		}

		void init (Device dev)
		{
			axisBindings = new List<AxisBinding> ();
			buttonBindings = new List<ButtonBinding> ();
			axisRecipes = new AxisRecipe[dev.num_axes];

			if (defaultRecipe == null) {
				defaultRecipe = new AxisRecipe ();
			}
			for (int i = 0; i < axisRecipes.Length; i++) {
				axisRecipes[i] = defaultRecipe;
			}

			rawDevice = dev.rawDevice;
		}

		public BindingSet (Device dev, ConfigNode node)
		{
			init (dev);
			ParseConfig (node);
		}

		public BindingSet (Device dev)
		{
			init (dev);
		}

		public int ButtonState (int index)
		{
			return rawDevice.buttons[index].state;
		}

		public float AxisValue (int index, bool invert)
		{
			AxisRecipe recipe = axisRecipes[index];
			return recipe.Process (ref rawDevice.axes[index], invert);
		}

		public void CheckInput ()
		{
			for (int i = axisBindings.Count; i-- > 0; ) {
				axisBindings[i].Update ();
			}
			for (int i = buttonBindings.Count; i-- > 0; ) {
				buttonBindings[i].Update ();
			}
		}

		public void UpdateInputLock (ulong mask)
		{
			for (int i = axisBindings.Count; i-- > 0; ) {
				axisBindings[i].UpdateInputLock (mask);
			}
			for (int i = buttonBindings.Count; i-- > 0; ) {
				buttonBindings[i].UpdateInputLock (mask);
			}
		}
	}
}

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

		public int num_axes { get { return rawDevice.num_axes; } }
		public int num_buttons { get { return rawDevice.num_buttons; } }
		public string name { get { return rawDevice.name; } }
		public string shortName
		{
			get {
				if (devNames != null) {
					return devNames.shortName;
				}
				return name;
			}
		}

		DeviceNamesContainer devNames;

		void ParseConfig (ConfigNode node)
		{
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

		public Device (InputLib.Device dev)
		{
			axisBindings = new List<AxisBinding> ();
			buttonBindings = new List<ButtonBinding> ();
			axisRecipes = new AxisRecipe[dev.axes.Length];
			for (int i = 0; i < axisRecipes.Length; i++) {
				axisRecipes[i] = new AxisRecipe ();
			}

			rawDevice = dev;

			AI_Database.DeviceNames.TryGetValue (dev.name, out devNames);
			ConfigNode node;
			if (AI_Database.DeviceConfigs.TryGetValue (dev.name, out node)) {
				ParseConfig (node);
			}
		}

		public int ButtonState (int index)
		{
			return rawDevice.buttons[index].state;
		}

		public int RawAxis (int index)
		{
			return rawDevice.axes[index].value;
		}

		public float AxisValue (int index, bool invert)
		{
			AxisRecipe recipe = axisRecipes[index];
			return recipe.Process (ref rawDevice.axes[index], invert);
		}

		public string AxisName (int index)
		{
			if (devNames != null) {
				return devNames.AxisName (index);
			} else {
				return index.ToString ();
			}
		}

		public string ButtonName (int index)
		{
			if (devNames != null) {
				return devNames.ButtonName (index);
			} else {
				return index.ToString ();
			}
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

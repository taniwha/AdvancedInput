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

	public class DeviceNamesContainer
	{
		public string name { get; private set; }
		public string shortName { get; private set; }
		string []axes;
		string []buttons;

		public DeviceNamesContainer (ConfigNode node)
		{
			name = node.GetValue ("name");
			if (node.HasValue ("shortName")) {
				shortName = node.GetValue("shortName");
			} else {
				shortName = name;
			}
			axes = node.GetValues ("axis");
			buttons = node.GetValues ("button");
			Debug.Log ($"{name}");
			Debug.Log ($"{axes}");
			Debug.Log ($"{buttons}");
		}

		public string AxisName (int ind)
		{
			if (ind < 0 || ind >= axes.Length) {
				return ind.ToString ();
			}
			return axes[ind];
		}

		public string ButtonName (int ind)
		{
			if (ind < 0 || ind >= buttons.Length) {
				return ind.ToString ();
			}
			return buttons[ind];
		}
	}
}

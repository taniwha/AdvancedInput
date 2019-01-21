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

	public class Device
	{
		public InputLibWrapper.Device rawDevice { get; private set; }

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

		public Dictionary<string, BindingSet> bindingSets { get; private set; }
		public List<BindingSet> activeBindingSets { get; private set; }
		public BindingSet defaultBindings { get; private set; }

		DeviceNamesContainer devNames;

		void ParseConfig (ConfigNode node)
		{
			defaultBindings = new BindingSet (this, node);
			foreach (ConfigNode n in node.nodes) {
				switch (n.name) {
					case "BindingSet":
						var bs = new BindingSet (this, n);
						bindingSets[bs.name] = bs;
						break;
				}
			}
		}

		public Device (InputLibWrapper.Device dev)
		{
			rawDevice = dev;
			bindingSets = new Dictionary<string, BindingSet> ();
			activeBindingSets = new List<BindingSet> ();
			defaultBindings = new BindingSet (this);

			AI_Database.DeviceNames.TryGetValue (dev.name, out devNames);
			ConfigNode node;
			if (AI_Database.DeviceConfigs.TryGetValue (dev.name, out node)) {
				ParseConfig (node);
			}
		}

		public int RawAxis (int index)
		{
			return rawDevice.axes[index].value;
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
			for (int i = activeBindingSets.Count; i-- > 0; ) {
				activeBindingSets[i].CheckInput ();
			}
			defaultBindings.CheckInput ();
		}

		public void UpdateInputLock (ulong mask)
		{
			var sets = bindingSets.Values;
			foreach (var bs in sets) {
				bs.UpdateInputLock (mask);
			}
			defaultBindings.UpdateInputLock (mask);
		}
	}
}

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

using KSP.IO;

namespace AdvancedInput {

	[KSPAddon (KSPAddon.Startup.Instantly, true)]
	public class AI_Database : MonoBehaviour
	{
		public static string DataPath { get; private set; }
		public static Dictionary<string, DeviceNamesContainer> DeviceNames { get; private set; }
		public static Dictionary<string, ConfigNode> DeviceConfigs { get; private set; }

		public static void LoadConfigFiles ()
		{
			var di = new DirectoryInfo (DataPath);
			var files = di.GetFiles ("*.cfg");
			foreach (var f in files) {
				string filePath = DataPath + "/" + f.Name;
				ConfigNode node = ConfigNode.Load(filePath);
				foreach (ConfigNode n in node.nodes) {
					switch (n.name) {
						case "DeviceNames":
							var devNames = new DeviceNamesContainer (n);
							DeviceNames[devNames.name] = devNames;
							break;
						case "DeviceConfig":
							DeviceConfigs[n.GetValue("name")] = n;
							break;
					}
				}
			}
		}

		void Awake ()
		{
			DeviceNames = new Dictionary<string, DeviceNamesContainer> ();
			DeviceConfigs = new Dictionary<string, ConfigNode> ();
			GameObject.DontDestroyOnLoad(this);
			DataPath = AssemblyLoader.loadedAssemblies.GetPathByType (typeof (AI_Database));
			LoadConfigFiles ();
		}
	}
}

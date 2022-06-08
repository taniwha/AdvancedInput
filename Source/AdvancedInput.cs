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
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

using KSP.IO;
using Harmony;

namespace AdvancedInput {
	using InputLibWrapper;

	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class AdvancedInput : MonoBehaviour
	{
		public static AdvancedInput instance;

		void Awake ()
		{
			HarmonyInstance harmony = HarmonyInstance.Create ("AdvancedInput");
			harmony.PatchAll (Assembly.GetExecutingAssembly ());


			instance = this;
			GameObject.DontDestroyOnLoad(this);
			InputLibLoader.openlib ();

			InputLib.Init ();
		}

		void OnDestroy ()
		{
			InputLib.Close ();
			InputLibLoader.closelib ();

			instance = null;
		}
	}
}

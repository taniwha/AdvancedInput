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
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

using KSP.IO;

namespace AdvancedInput.ButtonBindings {

	public class AI_BB_MainThrottleLock: IButtonBinding
	{
		public string name { get { return "MainThrottleLock"; } }
		public ControlTypes lockMask { get { return ControlTypes.None; } }
		public bool locked { get; set; }

		ButtonMode mode;

		public void Update (int state, bool edge)
		{
			switch (mode) {
				case ButtonMode.hold:
					AI_FlightControl.instance.mainThrottleLock = state != 0;
					break;
				case ButtonMode.toggle:
					if (state > 0 && edge) {
						AI_FlightControl.instance.mainThrottleLock ^= true;
					}
					break;
				case ButtonMode.off:
					if (state > 0 && edge) {
						AI_FlightControl.instance.mainThrottleLock = false;
					}
					break;
				case ButtonMode.on:
					if (state > 0 && edge) {
						AI_FlightControl.instance.mainThrottleLock = true;
					}
					break;
			}
		}

		public AI_BB_MainThrottleLock (ConfigNode node)
		{
			if (node != null) {
				mode = ButtonMode_methods.Parse (node.GetValue ("mode"));
			}
		}
	}
}

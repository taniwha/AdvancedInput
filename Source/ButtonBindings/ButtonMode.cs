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

	public enum ButtonMode
	{
		hold,
		toggle,
		edgetoggle,
		off,
		on,
		trigger,
	}

	public static class ButtonMode_methods
	{
		public static ButtonMode Parse (string name)
		{
			if (name == null) {
				name = String.Empty;
			} else {
				name = name.ToLower ();
			}
			return AI_Utils.ToEnum<ButtonMode> (name, ButtonMode.hold);
		}
	}
}

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
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

using KSP.IO;

namespace AdvancedInput.ButtonBindings {

	public class AI_BB_ActionGroup: IButtonBinding
	{
		public virtual string name { get { return "ActionGroup"; } }
		public virtual ControlTypes lockMask { get { return ControlTypes.CUSTOM_ACTION_GROUPS; } }
		public bool locked { get; set; }

		public ActionGroupList actionGroups
		{
			get {
				return FlightGlobals.ActiveVessel.ActionGroups;
			}
		}
		public KSPActionGroup group;
		public ButtonMode mode;

		public virtual void Hold (bool state)
		{
			actionGroups.SetGroup (group, state);
		}

		public virtual void Toggle ()
		{
			actionGroups.ToggleGroup (group);
		}

		public virtual void Off ()
		{
			actionGroups.SetGroup (group, false);
		}

		public virtual void On ()
		{
			actionGroups.SetGroup (group, true);
		}

		public virtual void Trigger ()
		{
			actionGroups.SetGroup (group, true);
			//FIXME too fast? may not matter as trigger is really for staging
			//which provides its own implementation
			actionGroups.SetGroup (group, false);
		}

		public void Update (int state, bool edge)
		{
			switch (mode) {
				case ButtonMode.hold:
					if (edge) {
						Hold (state != 0);
					}
					break;
				case ButtonMode.toggle:
					if (state > 0 && edge) {
						Toggle ();
					}
					break;
				case ButtonMode.edgetoggle:
					if (edge) {
						Toggle ();
					}
					break;
				case ButtonMode.off:
					if (state > 0 && edge) {
						Off ();
					}
					break;
				case ButtonMode.on:
					if (state > 0 && edge) {
						On ();
					}
					break;
				case ButtonMode.trigger:
					if (state > 0 && edge) {
						Trigger ();
					}
					break;
			}
		}

		public AI_BB_ActionGroup (ConfigNode node)
		{
			if (node != null) {
				mode = ButtonMode_methods.Parse (node.GetValue ("mode"));
			}
		}
	}
}

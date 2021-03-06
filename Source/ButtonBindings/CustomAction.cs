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

namespace AdvancedInput.ButtonBindings {

	public class AI_BB_CustomAction: AI_BB_ActionGroup
	{
		public override string name { get { return "CustomAction"; } }
		public override ControlTypes lockMask { get { return ControlTypes.CUSTOM_ACTION_GROUPS; } }

		public AI_BB_CustomAction (ConfigNode node) : base (node)
		{
			if (node != null) {
				int id = (int) KSPActionGroup.None;

				int.TryParse (node.GetValue ("id"), out id);
				group = (KSPActionGroup) id;
			}
		}
	}
}

/*
This file is part of Extraplanetary Launchpads.

Extraplanetary Launchpads is free software: you can redistribute it and/or
modify it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Extraplanetary Launchpads is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Extraplanetary Launchpads.  If not, see
<http://www.gnu.org/licenses/>.
*/
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using KSP.UI.Screens;

using KodeUI;

namespace AdvancedInput {
	using InputLibWrapper;

	public class AIDeviceInfo
	{
		public string name { get { return device.name; } }
		public InputLibWrapper.Device device { get; private set; }

		public AIDeviceInfo (InputLibWrapper.Device device)
		{
			this.device = device;
		}

		public class List : List<AIDeviceInfo>, UIKit.IListObject
		{
			ToggleGroup group;
			public UnityAction<AIDeviceInfo> onSelected { get; set; }
			public Layout Content { get; set; }
			public RectTransform RectTransform
			{
				get { return Content.rectTransform; }
			}

			public void Create (int index)
			{
				Content
					.Add<AIDeviceInfoView> ()
						.Group (group)
						.OnSelected (onSelected)
						.Device (this[index])
						.Finish ()
					;
			}

			public void Update (GameObject obj, int index)
			{
				var view = obj.GetComponent<AIDeviceInfoView> ();
				view.Device (this[index]);
			}

			public List (ToggleGroup group)
			{
				this.group = group;
			}

			public void Select (int index)
			{
				if (index >= 0 && index < Count) {
					group.SetAllTogglesOff (false);
					var child = Content.rectTransform.GetChild (index);
					var view = child.GetComponent<AIDeviceInfoView> ();
					view.Select ();
				}
			}
		}
	}
}

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
using UnityEngine.Events;
using UnityEngine.UI;
using KSP.UI.Screens;

using KodeUI;

namespace AdvancedInput {
	using InputLibWrapper;

	public class AIButtonInfo
	{
		DeviceNamesContainer deviceNames;
		int button_id;

		public string name
		{
			get {
				if (deviceNames == null) {
					return button_id.ToString ();
				}
				return deviceNames.ButtonName (button_id);
			}
		}

		public AIButtonInfo (int button_id, DeviceNamesContainer deviceNames)
		{
			this.deviceNames = deviceNames;
			this.button_id = button_id;
		}

		public class List : List<AIButtonInfo>, UIKit.IListObject
		{
			ToggleGroup group;
			public UnityAction<AIButtonInfo> onSelected { get; set; }
			public Layout Content { get; set; }
			public RectTransform RectTransform
			{
				get { return Content.rectTransform; }
			}

			public void Create (int index)
			{
				Content
					.Add<AIButtonInfoView> ()
						.Group (group)
						.OnSelected (onSelected)
						.Button (this[index])
						.Finish ()
					;
			}

			public void Update (GameObject obj, int index)
			{
				var view = obj.GetComponent<AIButtonInfoView> ();
				view.Button (this[index]);
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
					var view = child.GetComponent<AIButtonInfoView> ();
					view.Select ();
				}
			}

			public void ButtonEvent (in Button button, InputLibWrapper.Device dev)
			{
				var child = Content.rectTransform.GetChild (button.num);
				var view = child.GetComponent<AIButtonInfoView> ();
				view.SetState (button.state != 0);
			}
		}
	}
}

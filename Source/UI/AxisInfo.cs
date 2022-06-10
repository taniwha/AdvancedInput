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

	public class AIAxisInfo
	{
		DeviceNamesContainer deviceNames;
		int axis_id;
		public int minValue { get; private set; }
		public int maxValue { get; private set; }
		public int value { get; private set; }

		public string name
		{
			get {
				if (deviceNames == null) {
					return axis_id.ToString ();
				}
				return deviceNames.AxisName (axis_id);
			}
		}

		public AIAxisInfo (in Axis axis, DeviceNamesContainer deviceNames)
		{
			this.deviceNames = deviceNames;
			this.axis_id = axis.num;
			minValue = axis.min;
			maxValue = axis.max;
			value = axis.value;
		}

		public class List : List<AIAxisInfo>, UIKit.IListObject
		{
			ToggleGroup group;
			public UnityAction<AIAxisInfo> onSelected { get; set; }
			public Layout Content { get; set; }
			public RectTransform RectTransform
			{
				get { return Content.rectTransform; }
			}

			public void Create (int index)
			{
				Content
					.Add<AIAxisInfoView> ()
						.Group (group)
						.OnSelected (onSelected)
						.Axis (this[index])
						.Finish ()
					;
			}

			public void Update (GameObject obj, int index)
			{
				var view = obj.GetComponent<AIAxisInfoView> ();
				view.Axis (this[index]);
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
					var view = child.GetComponent<AIAxisInfoView> ();
					view.Select ();
				}
			}

			public void AxisEvent (in Axis axis, InputLibWrapper.Device dev)
			{
				var child = Content.rectTransform.GetChild (axis.num);
				var view = child.GetComponent<AIAxisInfoView> ();
				this[axis.num].value = axis.value;
				view.SetValue (axis.value);
			}
		}
	}
}

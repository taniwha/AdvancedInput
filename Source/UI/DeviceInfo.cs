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

	public class AIDeviceInfo
	{
		public string name { get { return device.name; } }
		public string phys { get { return device.phys; } }
		public string uniq { get { return device.uniq; } }
		public InputLibWrapper.Device device { get; private set; }
		public bool active { get { return Time.time - eventTime < 1; } }

		int[] axis_values;
		float eventTime;

		public AIDeviceInfo (InputLibWrapper.Device device)
		{
			this.device = device;
			device.AxisEvent += AxisEvent;
			device.ButtonEvent += ButtonEvent;
			eventTime = Time.time;
			axis_values = new int[device.num_axes];
			for (int i = 0; i < device.num_axes; i++) {
				axis_values[i] = device.axes[i].value;
			}
		}

		void AxisEvent (in Axis axis, InputLibWrapper.Device dev)
		{
			if (Math.Abs (axis.value - axis_values[axis.num]) > Math.Abs (axis.max - axis.min) / 16) {
				axis_values[axis.num] = axis.value;
				eventTime = Time.time;
			}
		}

		void ButtonEvent (in Button button, InputLibWrapper.Device dev)
		{
			eventTime = Time.time;
		}

		void Unhook ()
		{
			device.AxisEvent -= AxisEvent;
			device.ButtonEvent -= ButtonEvent;
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

			public new void Clear ()
			{
				for (int i = 0; i < Count; i++) {
					this[i].Unhook ();
				}
				base.Clear ();
			}
		}
	}
}

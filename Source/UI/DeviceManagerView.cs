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
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using KodeUI;

namespace AdvancedInput {
	using InputLibWrapper;

	public class AIDeviceManagerView : Layout, TabController.ITabItem
	{
		List<InputLibWrapper.Device> devices
		{
			get {
				return InputLib.devices;
			}
		}

		ScrollView devicesView;
		ScrollView axisView;
		ScrollView buttonView;

		UIText shortName;

		AIAxisInfo.List axisInfos;
		AIButtonInfo.List buttonInfos;
		AIDeviceInfo.List deviceInfos;

		InputLibWrapper.Device selectedDevice;

		public override void CreateUI ()
		{
			base.CreateUI ();

			UIScrollbar devicesScrollbar;
			UIScrollbar axisScrollbar;
			UIScrollbar buttonScrollbar;

			this.Vertical ()
				.ControlChildSize (true, true)
				.ChildForceExpand (false, false)

				.Add<LayoutPanel> ()
					.Horizontal ()
					.Padding (8)
					.Anchor (AnchorPresets.HorStretchTop)
					.FlexibleLayout (true,true)
					.PreferredHeight (300)

					.Add<ScrollView> (out devicesView)
						.Horizontal (false)
						.Vertical (true)
						.Horizontal ()
						.ControlChildSize (true, true)
						.ChildForceExpand (false, true)
						.MinSize (400, -1)
						.Add<UIScrollbar> (out devicesScrollbar, "Scrollbar")
							.Direction (Scrollbar.Direction.BottomToTop)
							.PreferredWidth (15)
							.Finish()
						.Finish ()
					.Add<VerticalLayout> ()
						.Add<UIText> (out shortName)
							.Size (15)
							.Alignment (TextAlignmentOptions.Left)
							.Finish ()
						.Add<HorizontalLayout> ()
							.Add<ScrollView> (out axisView)
								.Horizontal (false)
								.Vertical (true)
								.Horizontal ()
								.ControlChildSize (true, true)
								.ChildForceExpand (false, true)
								.Add<UIScrollbar> (out axisScrollbar, "Scrollbar")
									.Direction (Scrollbar.Direction.BottomToTop)
									.PreferredWidth (15)
									.Finish()
								.Finish ()
							.Add<ScrollView> (out buttonView)
								.Horizontal (false)
								.Vertical (true)
								.Horizontal ()
								.ControlChildSize (true, true)
								.ChildForceExpand (false, true)
								.Add<UIScrollbar> (out buttonScrollbar, "Scrollbar")
									.Direction (Scrollbar.Direction.BottomToTop)
									.PreferredWidth (15)
									.Finish()
								.Finish ()
							.Finish ()
						.Finish ()

					.Finish ()

				.Finish ();

			devicesView.VerticalScrollbar = devicesScrollbar;
			devicesView.Viewport.FlexibleLayout (true, true);
			ToggleGroup devicesGroup;
			devicesView.Content
				.Vertical ()
				.ControlChildSize (true, true)
				.ChildForceExpand (false, false)
				.Anchor (AnchorPresets.HorStretchTop)
				.PreferredSizeFitter (true, false)
				.WidthDelta (0)
				.ToggleGroup (out devicesGroup)
				.Finish ();
			deviceInfos = new AIDeviceInfo.List (devicesGroup);
			deviceInfos.Content = devicesView.Content;
			deviceInfos.onSelected = OnDeviceSelected;

			axisView.VerticalScrollbar = axisScrollbar;
			axisView.Viewport.FlexibleLayout (true, true);
			ToggleGroup axisGroup;
			axisView.Content
				.Vertical ()
				.ControlChildSize (true, true)
				.ChildForceExpand (false, false)
				.Anchor (AnchorPresets.HorStretchTop)
				.PreferredSizeFitter (true, false)
				.WidthDelta (0)
				.ToggleGroup (out axisGroup)
				.Finish ();
			axisInfos = new AIAxisInfo.List (axisGroup);
			axisInfos.Content = axisView.Content;

			buttonView.VerticalScrollbar = buttonScrollbar;
			buttonView.Viewport.FlexibleLayout (true, true);
			ToggleGroup buttonGroup;
			buttonView.Content
				.Vertical ()
				.ControlChildSize (true, true)
				.ChildForceExpand (false, false)
				.Anchor (AnchorPresets.HorStretchTop)
				.PreferredSizeFitter (true, false)
				.WidthDelta (0)
				.ToggleGroup (out buttonGroup)
				.Finish ();
			buttonInfos = new AIButtonInfo.List (buttonGroup);
			buttonInfos.Content = buttonView.Content;

			RebuildDevices ();
		}

		void OnDeviceSelected (AIDeviceInfo dev)
		{
			if (selectedDevice != null) {
				selectedDevice.AxisEvent -= axisInfos.AxisEvent;
				selectedDevice.ButtonEvent -= buttonInfos.ButtonEvent;
			}
			selectedDevice = dev.device;
			selectedDevice.AxisEvent += axisInfos.AxisEvent;
			selectedDevice.ButtonEvent += buttonInfos.ButtonEvent;
			RebuildInputs ();
		}

		void RebuildDevices ()
		{
			deviceInfos.Clear ();
			for (int i = 0; i < devices.Count; i++) {
				var dev = devices[i];
				if (dev == null) {
					continue;
				}
				deviceInfos.Add (new AIDeviceInfo (dev));
			}
			deviceInfos.Sort ((a, b) => a.name.CompareTo (b.name));
			UIKit.UpdateListContent (deviceInfos);
			deviceInfos.Select (0);
		}

		void RebuildInputs ()
		{
			DeviceNamesContainer devNames;
			AI_Database.DeviceNames.TryGetValue (selectedDevice.name, out devNames);
			if (devNames != null) {
				shortName.Text (devNames.shortName);
			} else {
				shortName.Text (selectedDevice.name);
			}
			axisInfos.Clear ();
			for (int i = 0; i < selectedDevice.num_axes; i++) {
				axisInfos.Add (new AIAxisInfo (selectedDevice.axes[i], devNames));
			}
			UIKit.UpdateListContent (axisInfos);
			buttonInfos.Clear ();
			for (int i = 0; i < selectedDevice.num_buttons; i++) {
				buttonInfos.Add (new AIButtonInfo (i, devNames));
			}
			UIKit.UpdateListContent (buttonInfos);
		}

		void DeviceAdded (InputLibWrapper.Device dev)
		{
			deviceInfos.Add (new AIDeviceInfo (dev));
			deviceInfos.Sort ((a, b) => a.name.CompareTo (b.name));
			UIKit.UpdateListContent (deviceInfos);
		}

		void DeviceRemoved (InputLibWrapper.Device dev)
		{
			for (int i = 0; i < deviceInfos.Count; i++) {
				if (deviceInfos[i].device == dev) {
					deviceInfos.RemoveAt (i);
					UIKit.UpdateListContent (deviceInfos);
				}
			}
		}

		void Update ()
		{
			while (InputLib.CheckInput ()) {
			}
		}

		protected override void OnEnable ()
		{
			base.OnEnable ();
			if (deviceInfos != null) {
				RebuildDevices ();
			}
			InputLib.DeviceAdded += DeviceAdded;
			InputLib.DeviceRemoved += DeviceRemoved;
		}

		protected override void OnDisable ()
		{
			base.OnDisable ();
			InputLib.DeviceAdded -= DeviceAdded;
			InputLib.DeviceRemoved -= DeviceRemoved;
		}

		public string TabName { get { return "Devices"; } }
		public bool TabEnabled { get { return true; } }
		public void SetTabVisible(bool visible)
		{
			SetActive (visible);
		}
	}
}

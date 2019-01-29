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

namespace AdvancedInput {

	[KSPAddon (KSPAddon.Startup.Flight, false)]
	public class AI_ConfigWindow : MonoBehaviour
	{
		public struct AxisState
		{
			public float value;
			public float time;
		}

		List<Device> devices
		{
			get {
				return AI_FlightControl.instance.devices;
			}
		}
		Device currentDevice, newDevice;
		AxisState []axisStates;

		enum AB {
			Axis,
			Button,
		}
		AB axisButtons;

		int currentInput = -1, newInput = -1;

		int mouseButtons;

		static GUILayoutOption width400 = GUILayout.Width (400);
		static GUILayoutOption expandWidth = GUILayout.ExpandWidth (true);
		//static GUILayoutOption noExpandWidth = GUILayout.ExpandWidth (false);

		const string devShortNameField = "devShortName.AdvancedInput";
		static TextField devShortName = new TextField (devShortNameField);
		static GUIStyle devNameStyle;

		static GUILayoutOption toggleWidth = GUILayout.Width (100);

		static ScrollView devScroll = new ScrollView (150, 300);
		static ScrollView inputScroll = new ScrollView (150, 250);

#region Basic Window Controls
		static AI_ConfigWindow instance;
		static bool hide_ui = false;
		static bool gui_enabled = false;
		static Rect windowpos;
		static Rect windowdrag = new Rect (0, 0, 10000, 20);

		void onHideUI ()
		{
			hide_ui = true;
			UpdateGUIState ();
		}

		void onShowUI ()
		{
			hide_ui = false;
			UpdateGUIState ();
		}

		void Awake ()
		{
			instance = this;
			AI_AppButton.Toggle += ToggleGUI;
			GameEvents.onHideUI.Add (onHideUI);
			GameEvents.onShowUI.Add (onShowUI);
		}

		void Start ()
		{
			UpdateGUIState ();
		}

		void OnDestroy ()
		{
			AI_AppButton.Toggle -= ToggleGUI;
			GameEvents.onHideUI.Remove (onHideUI);
			GameEvents.onShowUI.Remove (onShowUI);
		}

		public static void ToggleGUI ()
		{
			gui_enabled = !gui_enabled;
			if (instance != null) {
				instance.UpdateGUIState ();
			}
		}

		public static void HideGUI ()
		{
			gui_enabled = false;
			if (instance != null) {
				instance.UpdateGUIState ();
			}
		}

		public static void ShowGUI ()
		{
			gui_enabled = true;
			if (instance != null) {
				instance.UpdateGUIState ();
			}
		}

		void UpdateGUIState ()
		{
			enabled = !hide_ui && gui_enabled;
		}

		void OnGUI ()
		{
			GUI.skin = HighLogic.Skin;
			if (windowpos.x == 0) {
				windowpos = new Rect (Screen.width / 2 - 250,
					Screen.height / 2 - 30, 0, 0);
			}
			windowpos = GUILayout.Window (GetInstanceID (),
				windowpos, WindowGUI,
				"Configuration",
				GUILayout.Width (500));
		}
#endregion

		bool DeviceLine (Device dev, bool mouseOver)
		{
			bool selected = false;

			if (devNameStyle == null) {
				devNameStyle = new GUIStyle (GUI.skin.label);
				devNameStyle.wordWrap = false;
			}

			GUILayout.BeginHorizontal ();
			GUILayout.Label (dev.shortName, devNameStyle);
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();

			var e = Event.current;
			if (e.type == EventType.Repaint) {
				var rect = GUILayoutUtility.GetLastRect ();
				if (mouseOver && rect.Contains (e.mousePosition)) {
					if (mouseButtons == 1) {
						devShortName.text = dev.shortName;
						selected = true;
					}
				}
			}

			return selected;
		}

		Device ListDevices (bool mouseOver)
		{
			Device dev = null;
			for (int i = 0; i < devices.Count; i++) {
				if (DeviceLine (devices[i], mouseOver)) {
					dev = devices[i];
				}
			}
			return dev;
		}

		Device SelectDevice ()
		{
			Device dev = null;

			GUILayout.BeginVertical ();

			if (devices.Count > 0) {
				devScroll.Begin ();
				dev = ListDevices (devScroll.mouseOver);
				devScroll.End ();
			}
			GUILayout.EndVertical ();
			return dev;
		}

		void ResetAxisStates (Device dev)
		{
			axisStates = new AxisState[dev.num_axes];
			for (int i = dev.num_axes; i-- > 0; ) {
				axisStates[i].value = dev.RawAxis (i);
				axisStates[i].time = 0;
			}
		}

		void RenameDevice (Device dev)
		{
			GUILayout.BeginHorizontal (expandWidth);
			if (dev != null) {
				GUILayout.Label ("Short Name: ");

				if (devShortName.HandleInput ()) {
					if (dev.shortName != devShortName.text) {
						dev.shortName = devShortName.text;
					}
				}
			}

			GUILayout.EndHorizontal ();
		}

		void ToggleEnum<T> (ref T curState, T state, string label)
		{
			bool on = false;

			on = curState.Equals (state);
			if (GUILayout.Toggle (on, label, toggleWidth)) {
				curState = state;
			}
		}

		void AxisButtons ()
		{
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			ToggleEnum<AB> (ref axisButtons, AB.Axis, "Axes");
			ToggleEnum<AB> (ref axisButtons, AB.Button, "Buttons");
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();
		}

		bool InputLine (bool state, string name, bool mouseOver)
		{
			bool selected = false;

			GUILayout.BeginHorizontal ();
			GUILayout.Toggle (state, "");
			GUILayout.Label (name, devNameStyle);
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();

			var e = Event.current;
			if (e.type == EventType.Repaint) {
				var rect = GUILayoutUtility.GetLastRect ();
				if (mouseOver && rect.Contains (e.mousePosition)) {
					if (mouseButtons == 1) {
						selected = true;
					}
				}
			}

			return selected;
		}

		int SelectAxis (Device dev, bool mouseOver)
		{
			int axis = -1;
			for (int i = 0; i < dev.num_axes; i++) {
				float val = dev.RawAxis (i);
				bool state = false;
				float time = Time.time;
				if (val != axisStates[i].value) {
					axisStates[i].value = val;
					axisStates[i].time = time;
					state = true;
				} else if (time - axisStates[i].time < 1) {
					state = true;
				}
				if (InputLine (state, dev.AxisName (i), mouseOver)) {
					axis = i;
				}
			}
			return axis;
		}

		int SelectButton (Device dev, bool mouseOver)
		{
			int button = -1;
			for (int i = 0; i < dev.num_buttons; i++) {
				var bs = dev.defaultBindings;
				bool state = bs.ButtonState (i) != 0;
				if (InputLine (state, dev.ButtonName (i), mouseOver)) {
					button = i;
				}
			}
			return button;
		}

		void DumpLine (string name, float value)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (name);
			GUILayout.FlexibleSpace ();
			GUILayout.Label (value.ToString ("G4"));
			GUILayout.EndHorizontal ();
		}

		void DumpLine (string name, int value)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (name);
			GUILayout.FlexibleSpace ();
			GUILayout.Label (value.ToString ());
			GUILayout.EndHorizontal ();
		}

		void DumpLine (string name, string value)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (name);
			GUILayout.FlexibleSpace ();
			GUILayout.Label (value);
			GUILayout.EndHorizontal ();
		}

		void DumpLine (string name, string str, float value)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (name);
			GUILayout.FlexibleSpace ();
			GUILayout.Label (str);
			GUILayout.FlexibleSpace ();
			GUILayout.Label (value.ToString ("G4"));
			GUILayout.EndHorizontal ();
		}

		void DumpLine (string name, bool value)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (name);
			GUILayout.FlexibleSpace ();
			GUILayout.Label (value.ToString ());
			GUILayout.EndHorizontal ();
		}

		void AxisPanel (Device dev, int axis)
		{
			GUILayout.BeginVertical ();
			GUILayout.Label (dev.AxisName (axis));
			DumpLine ("raw", dev.RawAxis (axis));
			var bs = dev.defaultBindings;
			DumpLine ("cooked", bs.AxisValue (axis, false));
			GUILayout.EndVertical ();
		}

		void ButtonPanel (Device dev, int button)
		{
			GUILayout.BeginVertical ();
			GUILayout.Label (dev.ButtonName (button));
			var bs = dev.defaultBindings;
			DumpLine ("state", bs.ButtonState (button));
			GUILayout.EndVertical ();
		}

		void Inputs (Device dev)
		{
			int index = -1;

			GUILayout.BeginHorizontal ();
			inputScroll.Begin ();
			if (dev != null) {
				switch (axisButtons) {
					case AB.Axis:
						index = SelectAxis (dev, inputScroll.mouseOver);
						break;
					case AB.Button:
						index = SelectButton (dev, inputScroll.mouseOver);
						break;
				}
			}
			newInput = index;
			inputScroll.End ();
			if (dev != null && currentInput >= 0) {
				switch (axisButtons) {
					case AB.Axis:
						AxisPanel (dev, currentInput);
						break;
					case AB.Button:
						ButtonPanel (dev, currentInput);
						break;
				}
			}
			GUILayout.EndHorizontal ();
		}

		void DeviceDetails (Device dev)
		{
			GUILayout.BeginVertical (width400);
			if (dev != null) {
				GUILayout.Label (dev.name, expandWidth);
				RenameDevice (dev);
				AxisButtons ();
				Inputs (dev);
			} else {
				GUILayout.Label ("No device selected", expandWidth);
			}
			GUILayout.EndVertical ();
		}

		void VersionInfo ()
		{
			string ver = AdvancedInputVersionReport.GetVersion ();
			GUILayout.Label (ver);
		}

		void WindowGUI (int windowID)
		{
			var e = Event.current;
			switch (e.type) {
				case EventType.MouseDown:
					mouseButtons |= 1 << e.button;
					break;
				case EventType.MouseUp:
					mouseButtons &= ~(1 << e.button);
					break;
				case EventType.Layout:
					if (newDevice != null && newDevice != currentDevice) {
						currentDevice = newDevice;
						ResetAxisStates (currentDevice);
						newDevice = null;
						currentInput = newInput = -1;
					}
					if (newInput != -1) {
						currentInput = newInput;
						newInput = -1;
					}
					break;
			}

			GUILayout.BeginVertical ();

			GUILayout.BeginHorizontal ();
			newDevice = SelectDevice ();
			DeviceDetails (currentDevice);
			GUILayout.EndHorizontal ();

			VersionInfo ();
			GUILayout.EndVertical ();

			GUI.DragWindow (windowdrag);
		}
	}
}

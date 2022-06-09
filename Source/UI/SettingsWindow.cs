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

using KodeUI;

namespace AdvancedInput {
	using InputLibWrapper;

	public class SettingsWindow : Window
	{
		List<InputLibWrapper.Device> devices
		{
			get {
				return InputLib.devices;
			}
		}

		public override void CreateUI ()
		{
			base.CreateUI ();
			this.Title (AdvancedInputVersionReport.GetVersion ())
				.Vertical ()
				.ControlChildSize (true, true)
				.ChildForceExpand (false, false)
				.PreferredSizeFitter (true, true)
				.Anchor (AnchorPresets.MiddleCenter)
				.Pivot (PivotPresets.TopLeft)
				.PreferredWidth (450)
				.SetSkin ("AI.Default")

				.Finish ();
		}

		public void SetVisible (bool visible)
		{
			if (!visible) {
			} else {
			}
			gameObject.SetActive (visible);
		}
	}

	[KSPAddon (KSPAddon.Startup.SpaceCentre, false)]
	public class AI_SettingsWindow : MonoBehaviour
	{
		struct WindowInfo {
			public bool visible;
			public Vector2 position;

			public void Load (ConfigNode node)
			{
				string val = node.GetValue ("position");
				if (val != null) {
					ParseExtensions.TryParseVector2 (val, out position);
				}
				val = node.GetValue ("visible");
				if (val != null) {
					bool.TryParse (val, out visible);
				}
			}

			public void Save (ConfigNode node)
			{
				node.AddValue ("position", position);
				node.AddValue ("visible", visible);
			}
		}

		static AI_SettingsWindow instance;

		static Canvas appCanvas;
		public static RectTransform appCanvasRect { get; private set; }

		void Awake ()
		{
			instance = this;
			AI_AppButton.Toggle += ToggleUI;

			appCanvas = DialogCanvasUtil.DialogCanvas;
			appCanvasRect = appCanvas.transform as RectTransform;
		}

		void Start ()
		{
		}

		void OnDestroy ()
		{
			AI_AppButton.Toggle -= ToggleUI;
			instance = null;
		}

		static WindowInfo windowInfo;
		static SettingsWindow window;
		public static void ToggleUI ()
		{
			if (!window) {
				window = UIKit.CreateUI<SettingsWindow> (appCanvasRect, "AISettingsWindow");
				window.transform.position = windowInfo.position;
			}
			windowInfo.visible = !windowInfo.visible;
			window.SetVisible (windowInfo.visible);
			if (windowInfo.visible) {
				window.rectTransform.SetAsLastSibling ();
			}
		}
/*
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
			bool found = false;
			for (int i = 0; i < devices.Count; i++) {
				if (currentDevice == devices[i]) {
					found = true;
				}
				if (DeviceLine (devices[i], mouseOver)) {
					dev = devices[i];
				}
			}
			if (!found) {
				currentDevice = null;
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
			GUILayout.Label (value.ToString ("G5"));
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

		int Deadzone (AxisRecipe recipe)
		{
			if (string.IsNullOrEmpty (deadzoneField.text)) {
				deadzoneField.text = recipe.deadzone.ToString ();
			}
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Deadzone");
			GUILayout.FlexibleSpace ();
			int val = recipe.deadzone;
			if (deadzoneField.HandleInput (numericWidth)) {
				float fval;
				float.TryParse (deadzoneField.text, out fval);
				val = (int) fval;
			}
			GUILayout.EndHorizontal ();
			return val;
		}

		int Maxzone (AxisRecipe recipe)
		{
			if (string.IsNullOrEmpty (maxzoneField.text)) {
				maxzoneField.text = recipe.maxzone.ToString ();
			}
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Maxzone");
			GUILayout.FlexibleSpace ();
			int val = recipe.maxzone;
			if (maxzoneField.HandleInput (numericWidth)) {
				float fval;
				float.TryParse (maxzoneField.text, out fval);
				val = (int) fval;
			}
			GUILayout.EndHorizontal ();
			return val;
		}

		bool Inverted (AxisRecipe recipe)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Inverted");
			GUILayout.FlexibleSpace ();
			bool val = GUILayout.Toggle (recipe.inverted, "");
			GUILayout.EndHorizontal ();
			return val;
		}

		bool Balanced (AxisRecipe recipe)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Balanced");
			GUILayout.FlexibleSpace ();
			bool val = GUILayout.Toggle (recipe.balanced, "");
			GUILayout.EndHorizontal ();
			return val;
		}

		void UpdateRecipe (BindingSet bs, int axis,
						   int dz, int mz, bool inv, bool bal)
		{
			var r = bs.axisRecipes[axis];
			if ((dz != r.deadzone || mz != r.maxzone
				 || inv != r.inverted || bal != r.balanced)
				&& r == BindingSet.defaultRecipe) {
				bs.axisRecipes[axis] = new AxisRecipe ();
				r = bs.axisRecipes[axis];
			}
			r.deadzone = dz;
			r.maxzone = mz;
			r.inverted = inv;
			r.balanced = bal;
		}

		void AxisPanel (Device dev, int axis)
		{
			GUILayout.BeginVertical ();
			GUILayout.Label (dev.AxisName (axis));
			DumpLine ("raw", dev.RawAxis (axis));
			var bs = dev.defaultBindings;
			DumpLine ("cooked", bs.AxisValue (axis, false));
			int dz = Deadzone (bs.axisRecipes[axis]);
			int mz = Maxzone (bs.axisRecipes[axis]);
			bool inv = Inverted (bs.axisRecipes[axis]);
			bool bal = Balanced (bs.axisRecipes[axis]);
			UpdateRecipe (bs, axis, dz, mz, inv, bal);
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
						deadzoneField.text = null;
						maxzoneField.text = null;
					}
					if (newInput != -1) {
						currentInput = newInput;
						newInput = -1;
						deadzoneField.text = null;
						maxzoneField.text = null;
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
*/
	}
}

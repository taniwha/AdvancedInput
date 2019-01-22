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
	public class AI_AxisMonitor : MonoBehaviour
	{
		List<Device> devices
		{
			get {
				return AI_FlightControl.instance.devices;
			}
		}
		int devindex;
		int axisIndex;
		int bsIndex;
		int numBindings;

		static AI_AxisMonitor instance;
		static bool hide_ui = false;
		static bool gui_enabled = false;
		static Rect windowpos;

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
			//AI_AppButton.Toggle += ToggleGUI;
			GameEvents.onHideUI.Add (onHideUI);
			GameEvents.onShowUI.Add (onShowUI);
		}

		void Start ()
		{
			UpdateGUIState ();
		}

		void OnDestroy ()
		{
			//AI_AppButton.Toggle -= ToggleGUI;
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

		Device SelectDevice ()
		{
			if (devices.Count < 1) {
				GUILayout.Label ("No devices found.");
				return null;
			}
			if (devindex >= devices.Count) {
				devindex = 0;
			}
			Device dev = devices[devindex];
			if (GUILayout.Button (dev.shortName)) {
				devindex++;
			}
			return dev;
		}

		BindingSet SelectBindingSet (Device dev)
		{
			if (bsIndex >= dev.activeBindingSets.Count) {
				bsIndex = -1;
			}
			BindingSet bs = dev.defaultBindings;
			if (bsIndex >= 0) {
				bs = dev.activeBindingSets[bsIndex];
			}
			if (GUILayout.Button (bs.name)) {
				bsIndex++;
			}
			return bs;
		}

		int SelectAxis (Device dev)
		{
			if (dev.num_axes < 1) {
				GUILayout.Label ("No axes.");
				return -1;
			}
			if (axisIndex >= dev.num_axes) {
				axisIndex = 0;
			}
			int axis = axisIndex;
			if (GUILayout.Button (dev.AxisName (axis))) {
				axisIndex++;
			}
			return axis;
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

		void WindowGUI (int windowID)
		{
			Device dev = SelectDevice ();
			if (dev != null) {
				int axis = SelectAxis (dev);
				if (axis >= 0) {
					DumpLine ("raw", dev.RawAxis (axis));
					BindingSet bs = SelectBindingSet (dev);
					DumpLine ("cooked", bs.AxisValue (axis, false));
					int bound = 0;
					for (int i = 0; i < bs.axisBindings.Count; i++) {
						if (bs.axisBindings[i].index == axis) {
							bound++;
							var binding = bs.axisBindings[i].binding;
							string inv = bs.axisBindings[i].invert ? "- " : "";
							DumpLine (binding.name,
									  inv + binding.GetParameters(),
									  bs.axisBindings[i].Value ());
						}
					}
					if (bound == 0) {
						DumpLine ("not bound", "");
					}
					if (bound < numBindings && numBindings > 1) {
						windowpos.height = 0;
					}
					numBindings = bound;
				}
			}
			string name = "Advanced Input";
			string ver = AdvancedInputVersionReport.GetVersion ();
			GUILayout.Label (name + " " + ver);
			GUI.DragWindow (new Rect (0, 0, 10000, 20));
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
				"Axis Monitor",
				GUILayout.Width (500));
		}
	}
}

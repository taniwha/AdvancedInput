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
using System.Reflection;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace AdvancedInput {
	using InputLibWrapper;
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class AdvancedInput : MonoBehaviour
	{
		public static AdvancedInput instance;

		void Awake ()
		{
			instance = this;
			GameObject.DontDestroyOnLoad(this);
			InputLibLoader.openlib ();
		}

		void OnDestroy ()
		{
			instance = this;
			InputLibLoader.closelib ();
		}
	}


	[KSPAddon (KSPAddon.Startup.SpaceCentre, false)]
	public class AI_TestWindow : MonoBehaviour
	{
		static Rect windowpos;
		private static bool gui_enabled = false;
		private static bool hide_ui;

		static AI_TestWindow instance;

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

		public void Awake ()
		{
			instance = this;
			GameEvents.onHideUI.Add (onHideUI);
			GameEvents.onShowUI.Add (onShowUI);
		}

		void OnDestroy ()
		{
			instance = null;
			GameEvents.onHideUI.Remove (onHideUI);
			GameEvents.onShowUI.Remove (onShowUI);
			InputLib.Close ();
		}

		InputLibWrapper.Device dev;
		DeviceNamesContainer devNames;
		int devidx;
		
		void Start ()
		{
			InputLib.Init ();
			UpdateGUIState ();
		}

		void DumpAxes ()
		{
			GUILayout.BeginVertical ();

			for (int i = 0; i < dev.num_axes; i++) {
				GUILayout.BeginHorizontal ();
				if (devNames != null) {
					GUILayout.Label (devNames.AxisName (i));
				} else {
					GUILayout.Label (i.ToString());
				}
				GUILayout.FlexibleSpace ();
				GUILayout.Label (dev.axes[i].value.ToString());
				GUILayout.EndHorizontal ();
			}

			GUILayout.EndVertical ();
		}

		void DumpButtons ()
		{
			GUILayout.BeginVertical ();

			for (int i = 0; i < dev.num_buttons; i++) {
				if (i > 0 && (i % 9) == 0) {
					GUILayout.EndVertical ();
					GUILayout.BeginVertical ();
				}
				GUILayout.BeginHorizontal ();
				if (devNames != null) {
					GUILayout.Label (devNames.ButtonName (i));
				} else {
					GUILayout.Label (i.ToString());
				}
				GUILayout.FlexibleSpace ();
				GUILayout.Label (dev.buttons[i].state.ToString());
				GUILayout.EndHorizontal ();
			}

			GUILayout.EndVertical ();
		}

		void WindowGUI (int windowID)
		{
			dev = InputLib.devices[devidx];
			AI_Database.DeviceNames.TryGetValue (dev.name, out devNames);
			InputLib.CheckInput ();
			if (GUILayout.Button (dev.name)) {
				if (++devidx >= InputLib.devices.Count) {
					devidx = 0;
				}
			}
			GUILayout.BeginHorizontal ();
			DumpAxes ();
			DumpButtons ();
			GUILayout.EndHorizontal ();
			GUI.DragWindow (new Rect (0, 0, 10000, 20));
		}

		void OnGUI ()
		{
			if (gui_enabled) { // don't create windows unless we're going to show them
				GUI.skin = HighLogic.Skin;
				if (windowpos.x == 0) {
					windowpos = new Rect (Screen.width / 2 - 250,
						Screen.height / 2 - 30, 0, 0);
				}
				string name = "Advanced Input";
				string ver = AdvancedInputVersionReport.GetVersion ();
				windowpos = GUILayout.Window (GetInstanceID (),
					windowpos, WindowGUI,
					name + " " + ver,
					GUILayout.Width (500));
				if (windowpos.Contains (new Vector2 (Input.mousePosition.x, Screen.height - Input.mousePosition.y))) {
					InputLockManager.SetControlLock ("AI_test_window_lock");
				} else {
					InputLockManager.RemoveControlLock ("AI_test_window_lock");
				}
			} else {
				InputLockManager.RemoveControlLock ("AI_test_window_lock");
			}
		}
	}
}

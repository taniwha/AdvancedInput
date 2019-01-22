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
		List<Device> devices
		{
			get {
				return AI_FlightControl.instance.devices;
			}
		}

		static GUILayoutOption width400 = GUILayout.Width (400);
		static GUILayoutOption expandWidth = GUILayout.ExpandWidth (true);
		//static GUILayoutOption noExpandWidth = GUILayout.ExpandWidth (false);

		const string devShortNameField = "devShortName.AdvancedInput";
		static TextField devShortName = new TextField (devShortNameField);
		static GUILayoutOption devShortNameWidth;
		static GUIStyle devNameStyle;

		static ScrollView devScroll = new ScrollView (150, 300);

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

			if (Event.current.type == EventType.Repaint) {
				var rect = GUILayoutUtility.GetLastRect ();
				if (mouseOver && rect.Contains (Event.current.mousePosition)) {
					devShortName.text = dev.shortName; //FIXME
					selected = true;
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

		void RenameDevice (Device dev)
		{
			if (devShortNameWidth == null) {
				var c = new GUIContent ("Short Name: ");
				var s = GUI.skin.label.CalcSize (c);
				devShortNameWidth = GUILayout.Width (s.x); 
			}
			GUILayout.BeginHorizontal (expandWidth);
			if (dev != null) {
				GUILayout.Label ("Short Name: ", devShortNameWidth);

				if (devShortName.HandleInput (expandWidth)) {
					if (dev.shortName != devShortName.text) {
						dev.shortName = devShortName.text;
					}
				}
			} else {
				GUILayout.Label ("", devShortNameWidth);
				GUILayout.Label ("", expandWidth);
			}

			GUILayout.EndHorizontal ();
		}

		void DeviceDetails (Device dev)
		{
			GUILayout.BeginVertical (width400);
			if (dev != null) {
				GUILayout.Label (dev.name, expandWidth);
			} else {
				GUILayout.Label ("No device selected", expandWidth);
			}
			RenameDevice (dev);
			GUILayout.EndVertical ();
		}

		void VersionInfo ()
		{
			string ver = AdvancedInputVersionReport.GetVersion ();
			GUILayout.Label (ver);
		}

		void WindowGUI (int windowID)
		{
			GUILayout.BeginVertical ();

			GUILayout.BeginHorizontal ();
			Device dev = SelectDevice ();
			DeviceDetails (dev);
			GUILayout.EndHorizontal ();

			VersionInfo ();
			GUILayout.EndVertical ();

			GUI.DragWindow (windowdrag);
		}
	}
}

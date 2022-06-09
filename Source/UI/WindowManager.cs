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

	[KSPAddon (KSPAddon.Startup.MainMenu, true)]
	public class AIWindowManager : MonoBehaviour
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

		static AIWindowManager instance;

		static Canvas appCanvas;
		public static RectTransform appCanvasRect { get; private set; }

		void Awake ()
		{
			instance = this;
			GameObject.DontDestroyOnLoad (this);
			AI_AppButton.Toggle += ToggleUI;
			toggleWindow = null;

			appCanvas = DialogCanvasUtil.DialogCanvas;
			appCanvasRect = appCanvas.transform as RectTransform;

			GameEvents.onGameSceneSwitchRequested.Add (onGameSceneSwitchRequested);
			GameEvents.onLevelWasLoadedGUIReady.Add (onLevelWasLoadedGUIReady);
		}

		void Start ()
		{
		}

		void OnDestroy ()
		{
			GameEvents.onGameSceneSwitchRequested.Remove (onGameSceneSwitchRequested);
			GameEvents.onLevelWasLoadedGUIReady.Remove (onLevelWasLoadedGUIReady);

			AI_AppButton.Toggle -= ToggleUI;
			instance = null;
		}

		void onGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> data)
		{
			if (settingsWindow) {
				settingsWindow.SetVisible (false);
				settingsWindowInfo.visible = false;
			}
		}

		delegate void ToggleDelegate();
		static ToggleDelegate toggleWindow;

		void onLevelWasLoadedGUIReady(GameScenes scene)
		{
			switch (scene) {
//				case GameScenes.FLIGHT:
//					break;
				case GameScenes.SPACECENTER:
					toggleWindow = ToggleSettingsWindow;
					break;
				default:
					toggleWindow = null;
					break;
			}
		}

		void ToggleSettingsWindow ()
		{
			if (!settingsWindow) {
				settingsWindow = UIKit.CreateUI<AISettingsWindow> (appCanvasRect, "AISettingsWindow");
				settingsWindow.transform.position = settingsWindowInfo.position;
			}
			settingsWindowInfo.visible = !settingsWindowInfo.visible;
			settingsWindow.SetVisible (settingsWindowInfo.visible);
			if (settingsWindowInfo.visible) {
				settingsWindow.rectTransform.SetAsLastSibling ();
			}
		}
		public static void HideSettingsWindow ()
		{
			if (settingsWindow) {
				settingsWindow.SetVisible (false);
			}
			settingsWindowInfo.visible = false;
		}

		static WindowInfo settingsWindowInfo;
		static AISettingsWindow settingsWindow;
		public static void ToggleUI ()
		{
			if (toggleWindow != null) {
				toggleWindow ();
			}
		}
	}
}

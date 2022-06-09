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
	public class AISettingsWindow : Window
	{
		TabController tabController;
		AIDeviceManagerView deviceManager;

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
				.PreferredWidth (800)
				.SetSkin ("AI.Default")

				.Add<TabController> (out tabController)
					.Horizontal ()
					.ControlChildSize (true, true)
					.ChildForceExpand (false, false)
					.FlexibleLayout (true, false)
					.Finish ()
				.Add<AIDeviceManagerView> (out deviceManager)
					.Finish ()

				.Add<UIButton> ()
					.Text ("Reload Settings")
					.OnClick (LoadSettings)
					.Finish ()

				.Finish ();

			var tabItems = new List<TabController.ITabItem> () {
				deviceManager,
			};
			tabController.Items (tabItems);

			titlebar
				.Add<UIButton> ()
					.OnClick (CloseWindow)
					.Anchor (AnchorPresets.TopRight)
					.Pivot (new Vector2 (1.25f, 1.25f))
					.SizeDelta (16, 16)
					.Finish();
				;
		}

		void CloseWindow ()
		{
			AIWindowManager.HideSettingsWindow ();
		}

		void LoadSettings ()
		{
			AI_Database.LoadConfigFiles ();
		}

		public void SetVisible (bool visible)
		{
			if (!visible) {
			} else {
			}
			gameObject.SetActive (visible);
		}
	}
}

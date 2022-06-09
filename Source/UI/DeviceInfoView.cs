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
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using KSP.Localization;
using TMPro;

using KodeUI;

namespace AdvancedInput {
	using InputLibWrapper;

	public class AIDeviceInfoView : LayoutPanel, IPointerEnterHandler, IPointerExitHandler
	{
		AIDeviceInfo device;
		new UIText name;

		public class AIDeviceInfoViewEvent : UnityEvent<AIDeviceInfo> { }
		AIDeviceInfoViewEvent onSelected;

		Toggle toggle;

		public override void CreateUI()
		{
			base.CreateUI ();

			onSelected = new AIDeviceInfoViewEvent ();

			toggle = gameObject.AddComponent<Toggle> ();
			toggle.targetGraphic = BackGround;
			toggle.onValueChanged.AddListener (onValueChanged);

			var statsMin = new Vector2 (0, 0);
			var statsMax = new Vector2 (124f/234f, 1);
			var costMin = new Vector2 (124f/234f, 0);
			var costMax = new Vector2 (1, 1);

			this.Horizontal ()
				.ControlChildSize (true, true)
				.ChildForceExpand (false, false)
				.Padding (3)
				.Add<Layout> ()
					.Vertical ()
					.ControlChildSize (true, true)
					.ChildForceExpand (false, false)
					.Add<UIText> (out name)
						.Alignment (TextAlignmentOptions.Left)
						.Size (18)
						//.PreferredSize (234, -1)
						.Finish ()
					.Finish ()
				;
			//
		}

		void onValueChanged (bool on)
		{
			if (on) {
				onSelected.Invoke (device);
			}
		}

		public AIDeviceInfoView Group (ToggleGroup group)
		{
			toggle.group = group;
			return this;
		}

		public AIDeviceInfoView OnSelected (UnityAction<AIDeviceInfo> action)
		{
			onSelected.AddListener (action);
			return this;
		}

		public AIDeviceInfoView Select ()
		{
			toggle.isOn = true;
			return this;
		}

		public AIDeviceInfoView Device (AIDeviceInfo device)
		{
			this.device = device;
			name.Text (device.name);
			return this;
		}
#region OnPointerEnter/Exit
		public void OnPointerEnter (PointerEventData eventData)
		{
		}

		public void OnPointerExit (PointerEventData eventData)
		{
		}
#endregion
	}
}

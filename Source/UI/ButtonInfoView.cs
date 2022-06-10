/*
This file is part of Advanced Input.

Advanced Input is free software: you can redistribute it and/or
modify it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Advanced Input is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Advanced Input.  If not, see
<http://www.gnu.org/licenses/>.
*/
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using KSP.Localization;
using TMPro;

using KodeUI;

namespace AdvancedInput {
	using InputLibWrapper;

	public class AIButtonInfoView : LayoutPanel, IPointerEnterHandler, IPointerExitHandler
	{
		AIButtonInfo button;
		new UIText name;

		public class AIButtonInfoViewEvent : UnityEvent<AIButtonInfo> { }
		AIButtonInfoViewEvent onSelected;

		Toggle toggle;
		MiniToggle activeIndicator;

		public override void CreateUI()
		{
			base.CreateUI ();

			onSelected = new AIButtonInfoViewEvent ();

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
				.FlexibleLayout (true, false)
				.Add<MiniToggle> (out activeIndicator)
					.PreferredSize (22, -1)
					.FlexibleLayout (false, true)
					.Finish ()
				.Add<UIText> (out name)
					.Alignment (TextAlignmentOptions.Left)
					.Size (18)
					.Finish ()
				;
			//
		}

		void onValueChanged (bool on)
		{
			if (on) {
				//onSelected.Invoke (button);
			}
		}

		public AIButtonInfoView Group (ToggleGroup group)
		{
			toggle.group = group;
			return this;
		}

		public AIButtonInfoView OnSelected (UnityAction<AIButtonInfo> action)
		{
			onSelected.AddListener (action);
			return this;
		}

		public AIButtonInfoView Select ()
		{
			toggle.isOn = true;
			return this;
		}

		public void SetState (bool pressed)
		{
			activeIndicator.SetIsOnWithoutNotify (pressed);
		}

		public AIButtonInfoView Button (AIButtonInfo button)
		{
			this.button = button;
			name.Text (button.name);
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

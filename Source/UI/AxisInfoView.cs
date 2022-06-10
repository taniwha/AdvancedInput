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

	public class AIAxisInfoView : LayoutPanel, IPointerEnterHandler, IPointerExitHandler
	{
		AIAxisInfo axis;
		new UIText name;

		public class AIAxisInfoViewEvent : UnityEvent<AIAxisInfo> { }
		AIAxisInfoViewEvent onSelected;

		UISlider axisFraction;
		UIText axisValue;

		int minValue;
		int maxValue;

		public override void CreateUI()
		{
			base.CreateUI ();

			onSelected = new AIAxisInfoViewEvent ();

			var axisMin = new Vector2 (0.2f, 0);
			var axisMax = new Vector2 (0.7f, 1);
			var textMargins = new Vector4 (5, 5, 10, 10);

			this.Vertical ()
				.ControlChildSize (true, true)
				.ChildForceExpand (false, false)
				.Padding (3)
				.FlexibleLayout (true, false)
				.Add<UIText> (out name)
					.Alignment (TextAlignmentOptions.Left)
					.Size (18)
					.Finish ()
				.Add<UIEmpty> ()
					.MinSize (-1, 22)
					.FlexibleLayout (true, false)
					.Anchor (axisMin, axisMax)
					.SizeDelta (0, 0)
					.Add<UISlider> (out axisFraction, "AxisFraction")
						.Direction (Slider.Direction.LeftToRight)
						.ShowHandle (false)
						.Anchor (AnchorPresets.StretchAll)
						.Finish ()
					.Add<UIText> (out axisValue, "AxisValue")
						.Size (15)
						.Margin (textMargins)
						.Alignment (TextAlignmentOptions.Center)
						.Anchor (AnchorPresets.StretchAll)
						.Finish ()
					.Finish ()
				;
			//
			axisFraction.interactable = false;
			axisFraction.slider.SetValueWithoutNotify (0.5f);
		}

		void onValueChanged (bool on)
		{
			if (on) {
				//onSelected.Invoke (axis);
			}
		}

		public AIAxisInfoView Group (ToggleGroup group)
		{
			return this;
		}

		public AIAxisInfoView OnSelected (UnityAction<AIAxisInfo> action)
		{
			onSelected.AddListener (action);
			return this;
		}

		public AIAxisInfoView Select ()
		{
			return this;
		}

		public void SetValue (int value)
		{
			float frac;
			if (minValue == maxValue) {
				// relative axis
				frac = value / 100f;
			} else {
				frac = (value - minValue) / (float) (maxValue - minValue);
			}
			axisFraction.slider.SetValueWithoutNotify (frac);
			axisValue.Text (value.ToString ());
		}

		public AIAxisInfoView Axis (AIAxisInfo axis)
		{
			this.axis = axis;
			name.Text (axis.name);
			minValue = axis.minValue;
			maxValue = axis.maxValue;
			SetValue (axis.value);
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

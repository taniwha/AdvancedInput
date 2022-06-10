/*
This file is part of Advancied Input.

Advancied Input is free software: you can redistribute it and/or
modify it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Advancied Input is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Advancied Input.  If not, see
<http://www.gnu.org/licenses/>.
*/

using KodeUI;

namespace AdvancedInput {

	public class HorizontalLayout : Layout
	{
		public override void CreateUI ()
		{
			this.Horizontal ()
				.ControlChildSize (true, true)
				.ChildForceExpand (false,false)
				;
		}
	}

	public class VerticalLayout : Layout
	{
		public override void CreateUI ()
		{
			this.Vertical ()
				.ControlChildSize (true, true)
				.ChildForceExpand (false,false)
				;
		}
	}
}

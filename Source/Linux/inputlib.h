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
#ifndef inputlib_inputlib_h
#define inputlib_inputlib_h

typedef struct {
	int         num, evnum;
	int         state;
} button_t;

typedef struct {
	int         num, evnum;
	int         value;
	// relative axes set these to 0
	int         min, max;
} axis_t;

typedef struct device_s {
	struct device_s *next;
	char       *path;
	char       *name;
	int         fd;
	int         max_button;
	int        *button_map;
	int         num_buttons;
	button_t   *buttons;
	int         max_abs_axis;
	int        *abs_axis_map;
	int         max_rel_axis;
	int        *rel_axis_map;
	int         num_abs_axes;
	int         num_rel_axes;
	// includes both abs and rel axes, with abs first
	int         num_axes;
	axis_t     *axes;
	int         event_count;
} device_t;

int inputlib_check_input (void);
void inputlib_close (void);
int inputlib_init (void (*dev_add) (device_t *), void (*dev_rem) (device_t *));

#endif//inputlib_inputlib_h

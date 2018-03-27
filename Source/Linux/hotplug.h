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
#ifndef inputlib_hotplug_h
#define inputlib_hotplug_h

int inputlib_hotplug_init(const char *path,
						  void (*created) (const char*),
						  void (*deleted) (const char *));

void inputlib_hotplug_close ();

int inputlib_hotplug_add_select (fd_set *fdset, int *maxfd);

int inputlib_hotplug_check_select (fd_set *fdset);

#endif//inputlib_hotplug_h

#include <linux/input.h>
#include <linux/joystick.h>
#include <stdio.h>
#include <sys/time.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <string.h>
#include <unistd.h>
#include <stdio.h>
#include <stdlib.h>
#include <dirent.h>
#include <string.h>

#include "inputlib.h"

device_t *devices;

static void
setup_buttons (device_t *dev)
{
	int         i, j, len;
	unsigned char buf[1024];
	button_t   *button;

	dev->max_button = -1;
	dev->num_buttons = 0;
	dev->button_map = 0;
	dev->buttons = 0;
	len = ioctl (dev->fd, EVIOCGBIT (EV_KEY, sizeof (buf)), buf);
	for (i = 0; i < len; i++) {
		//printf("%c%02x", !(i % 16) ? '\n': !(i % 8) ? '-' : ' ', buf[i]);
		for (j = 0; j < 8; j++) {
			if (buf[i] & (1 << j)) {
				dev->num_buttons++;
				dev->max_button = i * 8 + j;
			}
		}
	}
	//printf("\n");
	dev->button_map = malloc ((dev->max_button + 1) * sizeof (int));
	dev->buttons = malloc (dev->num_buttons * sizeof (button_t));
	for (i = 0, button = dev->buttons; i < len; i++) {
		for (j = 0; j < 8; j++) {
			int         button_ind = i * 8 + j;
			if (buf[i] & (1 << j)) {
				button->num = button - dev->buttons;
				button->evnum = button_ind;
				button->state = 0;
				dev->button_map[button_ind] = button->num;

				button++;
			} else {
				if (button_ind <= dev->max_button) {
					dev->button_map[button_ind] = -1;
				}
			}
		}
	}
	len = ioctl (dev->fd, EVIOCGKEY (sizeof (buf)), buf);
	for (i = 0; i < dev->num_buttons; i++) {
		int         key = dev->buttons[i].evnum;
		dev->buttons[i].state = !!(buf[key / 8] & (1 << (key % 8)));
	}
}

static void
setup_axes (device_t *dev)
{
	int         i, j, len;
	unsigned char buf[1024];
	axis_t     *axis;

	dev->max_axis = -1;
	dev->num_axes = 0;
	dev->axis_map = 0;
	dev->axes = 0;
	len = ioctl (dev->fd, EVIOCGBIT (EV_ABS, sizeof (buf)), buf);
	for (i = 0; i < len; i++) {
		for (j = 0; j < 8; j++) {
			if (buf[i] & (1 << j)) {
				dev->num_axes++;
				dev->max_axis = i * 8 + j;
			}
		}
	}
	dev->axis_map = malloc ((dev->max_axis + 1) * sizeof (int));
	dev->axes = malloc (dev->num_axes * sizeof (axis_t));
	for (i = 0, axis = dev->axes; i < len; i++) {
		for (j = 0; j < 8; j++) {
			int         axis_ind = i * 8 + j;
			if (buf[i] & (1 << j)) {
				struct input_absinfo absinfo;
				ioctl (dev->fd, EVIOCGABS(axis_ind), &absinfo);
				axis->num = axis - dev->axes;
				axis->evnum = axis_ind;
				axis->value = absinfo.value;
				axis->min = absinfo.minimum;
				axis->max = absinfo.maximum;
				dev->axis_map[axis_ind] = axis->num;

				axis++;
			} else {
				if (axis_ind <= dev->max_axis) {
					dev->axis_map[axis_ind] = -1;
				}
			}
		}
	}
}

int
check_device (const char *path)
{
	device_t   *dev;
	int         fd;
	char        buf[256];	// FIXME
	
	fd = open (path, O_RDWR);
	if (fd == -1)
		return -1;

	dev = malloc (sizeof (device_t));
	dev->next = devices;
	devices = dev;

	dev->path = strdup (path);
	dev->fd = fd;

	ioctl (fd, EVIOCGNAME (sizeof (buf)), buf);
	dev->name = strdup (buf);

	setup_buttons(dev);
	setup_axes(dev);

	dev->event_count = 0;

	//printf ("%s:\n", path);
	//printf ("\tname: %s\n", dev->name);
	//printf ("\tbuttons: %d\n", dev->num_buttons);
	//printf ("\taxes: %d\n", dev->num_axes);

	return fd;
}

static void
read_device_input (device_t *dev)
{
	struct input_event event;
	button_t *button;
	axis_t * axis;

	while (1) {
		if (read (dev->fd, &event, sizeof (event)) < 0) {
			perror(dev->name);
			return;
		}
		//printf ("%6d %6d %6x\n", event.type, event.code, event.value);
		switch (event.type) {
			case EV_SYN:
				dev->event_count++;
				return;
			case EV_KEY:
				button = &dev->buttons[dev->button_map[event.code]];
				button->state = event.value;
				break;
			case EV_ABS:
				axis = &dev->axes[dev->axis_map[event.code]];
				axis->value = event.value;
				break;
			case EV_MSC:
				break;
			case EV_REL:
			case EV_SW:
			case EV_LED:
			case EV_SND:
			case EV_REP:
			case EV_FF:
			case EV_PWR:
			case EV_FF_STATUS:
				printf ("%6d %6d %6x\n", event.type, event.code, event.value);
				break;
		}
	}
}

int
check_device_input (void)
{
	fd_set      fdset;
	struct timeval _timeout;
	struct timeval *timeout = &_timeout;
	int         res;
	int         maxfd = -1;
	device_t   *dev;

	_timeout.tv_sec = 0;
	_timeout.tv_usec = 0;

	FD_ZERO (&fdset);

	for (dev = devices; dev; dev = dev->next) {
		FD_SET (dev->fd, &fdset);
		if (dev->fd > maxfd) {
			maxfd = dev->fd;
		}
	}
	if (maxfd < 0) {
		return 0;
	}
	res = select (maxfd + 1, &fdset, NULL, NULL, timeout);
	if (res <= 0) {
		return 0;
	}
	for (dev = devices; dev; dev = dev->next) {
		if (FD_ISSET (dev->fd, &fdset)) {
			read_device_input (dev);
		}
	}
	return 1;
}

static int
check_input_device (const char *path, const char *name)
{
	int         plen = strlen (path);
	int         nlen = strlen (name);
	char       *devname = malloc (plen + nlen + 2);
	strcpy (devname, path);
	devname[plen] = '/';
	strcpy (devname + plen + 1, name);
	//puts (devname);
	return check_device (devname);
}

device_t *
scan_devices (void)
{
	struct dirent *dirent;
	DIR *dir;

	dir = opendir ("/dev/input");
	if (!dir) {
		return 0;
	}

	while ((dirent = readdir (dir))) {
		if (dirent->d_type != DT_CHR) {
			continue;
		}
		if (strncmp (dirent->d_name, "event", 5)) {
			continue;
		}
		if (check_input_device ("/dev/input", dirent->d_name) < 0) {
			continue;
		}
		printf("%s\n", dirent->d_name);
	}
	closedir (dir);
	return devices;
}

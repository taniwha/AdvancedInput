typedef struct {
	int         num, evnum;
	int         state;
} button_t;

typedef struct {
	int         num, evnum;
	int         value;
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
	int         max_axis;
	int        *axis_map;
	int         num_axes;
	axis_t     *axes;
	int         event_count;
} device_t;

extern device_t *devices;
int check_device (const char *path);
int check_device_input (void);
device_t *scan_devices (void);

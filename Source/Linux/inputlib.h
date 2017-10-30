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
	// includes both abs and rel axes, with abs first
	int         num_axes;
	axis_t     *axes;
	int         event_count;
} device_t;

int inputlib_check_input (void);
void inputlib_close (void);
int inputlib_init (void (*dev_add) (device_t *), void (*dev_rem) (device_t *));

#endif//inputlib_inputlib_h

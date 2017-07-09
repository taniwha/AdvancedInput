# Advanced Input
Advanced Input is (or will be) a KSP mod for better joystick handling. It
is currently in early development stages (just a debug window at this
stage) and works on only Linux (neither Mac OS nor Windows are supported:
PRs welcome).

The plan is to either incorporate Advanced Input into the existing (and
very nice) Advanced Fly By Wire mod, or absorb AFBW's features since its
author has disappeared, with the latter being more likely.

## WARNING
There is a bug in Unity's joystick driver which comes with KSP that causes
KSP to crash if high-numbered buttons are pressed (relevant to only devices
that have more than 20 buttons). This is despite Unity claiming to ignore
those buttons and also despite no joystick button bindings being set up in
KSP. Fortunately, there is a workaround: remove read permission from
/dev/lib/js\* such that Unity cannot open the device (this bug may well be
limited to Unity on Linux). Note that this may cause problems if you use
your joystick in other games. This works with Advanced Input because
Advanced Input uses /dev/lib/event\* instead.

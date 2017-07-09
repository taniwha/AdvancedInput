MODNAME		:= AdvancedInput
KSPDIR		:= ${HOME}/ksp/KSP_linux
MANAGED		:= ${KSPDIR}/KSP_Data/Managed
GAMEDATA	:= ${KSPDIR}/GameData
MSCGAMEDATA := ${GAMEDATA}/${MODNAME}
PLUGINDIR	:= ${MSCGAMEDATA}/Plugins

CFLAGS = -g -Wall -Werror -fPIC
CPPFLAGS = -MMD
#MCSFLAGS = -optimize -warnaserror -unsafe
MCSFLAGS = -optimize -warnaserror

lib_sources = inputlib.c

lib_objects = $(patsubst %.c, %.o, $(lib_sources))

lib = libinputlib.so

sources = ${lib_sources}
dependencies = $(patsubst %.c, %.d, sources)

%.o: %.c
	gcc -c $(CPPFLAGS) $(CFLAGS) -o $@ $<

%.d:

all: $(lib)

$(lib): $(lib_objects)
	gcc -shared -Wl,-soname,$@ -o $@ $<

install: all
	mkdir -p ${PLUGINDIR}
	cp $(lib) ${PLUGINDIR}

clean:
	rm -rf *.o *.d $(lib)

-include $(dependencies)

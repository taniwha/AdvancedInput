MODNAME		:= AdvancedInput
KSPDIR		:= ${HOME}/ksp/KSP_linux
GAMEDATA	:= ${KSPDIR}/GameData
MODGAMEDATA  := ${GAMEDATA}/${MODNAME}
PLUGINDIR	:= ${MODGAMEDATA}/Plugins
PLUGINDATA	:= ${PLUGINDIR}/PluginData

RESGEN2		:= resgen2
GMCS		:= gmcs
GMCSFLAGS	:= -optimize -warnaserror
GIT			:= git
TAR			:= tar
ZIP			:= zip

DATA := \
	saitek_x52.cfg	\
	$e

all: 

info:
	@echo "${MODNAME} Build Information"
	@echo "    resgen2:    ${RESGEN2}"
	@echo "    gmcs:       ${GMCS}"
	@echo "    gmcs flags: ${GMCSFLAGS}"
	@echo "    git:        ${GIT}"
	@echo "    tar:        ${TAR}"
	@echo "    zip:        ${ZIP}"
	@echo "    KSP Data:   ${KSPDIR}"

clean:
	rm -f EarlyBird_icon.png

install: all
	mkdir -p ${PLUGINDATA}/AdvancedInput
	cp ${DATA} ${PLUGINDATA}/AdvancedInput

.PHONY: all clean install

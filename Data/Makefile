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
	3Dconnexion_SpaceNavigator.cfg \
	deviceconfig.cfg \
	saitek_x52.cfg \
	thrustmaster_t16000m.cfg \
	thrustmaster_twcs_throttle.cfg \
	xbox_360_wireless.cfg \
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

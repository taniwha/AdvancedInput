MODNAME		:= AdvancedInput
KSPDIR		:= ${HOME}/ksp/KSP_linux
MANAGED		:= ${KSPDIR}/KSP_Data/Managed
GAMEDATA	:= ${KSPDIR}/GameData
MODGAMEDATA := ${GAMEDATA}/${MODNAME}
PLUGINDIR	:= ${MODGAMEDATA}/Plugins

DOC_FILES := \
	FreeEVA.png \
	License.txt \
	README.md

RESGEN2		:= resgen2
GMCS		:= gmcs
GMCSFLAGS	:= -optimize -warnaserror
GIT			:= git
TAR			:= tar
ZIP			:= zip

all: icon_button.png

info:
	@echo "Extraplanetary Launchpads Build Information"
	@echo "    resgen2:    ${RESGEN2}"
	@echo "    gmcs:       ${GMCS}"
	@echo "    gmcs flags: ${GMCSFLAGS}"
	@echo "    git:        ${GIT}"
	@echo "    tar:        ${TAR}"
	@echo "    zip:        ${ZIP}"
	@echo "    KSP Data:   ${KSPDIR}"

icon_button.png: icon_button.svg
	inkscape --export-width=24 --export-height=24 --export-background='#000000' --export-background-opacity=0 --export-png=$@ $^

HARMONY := 0Harmony.1.2.0.1.dll

clean:
	rm -f icon_button.png

install: all
	mkdir -p ${MODGAMEDATA}/Textures
	cp icon_button.png ${MODGAMEDATA}/Textures
	cp ${HARMONY} ${PLUGINDIR}

.PHONY: all clean install

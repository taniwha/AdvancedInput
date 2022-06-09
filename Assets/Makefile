MODNAME		:= AdvancedInput
KSPDIR		:= ${HOME}/ksp/KSP_linux
GAMEDATA	:= ${KSPDIR}/GameData
MODGAMEDATA := ${GAMEDATA}/${MODNAME}
PLUGINDIR	:= ${MODGAMEDATA}/Plugins

RESGEN2		:= resgen2
GMCS		:= gmcs
GMCSFLAGS	:= -optimize -warnaserror
GIT			:= git
TAR			:= tar
ZIP			:= zip

ICONS := \
	icon_button.png

UI_IMAGES := \
	ui_background.png   \
	ui_button.png       \
	ui_checkmark.png    \
	ui_dropdown.png     \
	ui_leftturn.png     \
	ui_rightturn.png    \
	ui_tabdesel.png     \
	ui_tabempty.png     \
	ui_tabsel.png       \
	ui_toggle_off.png   \
	ui_toggle_on.png

UI_CFG := \
	DefaultSkin.cfg

all: $(ICONS) $(UI_IMAGES)

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

ui_background.png: ui_background.svg
	inkscape --export-width=32 --export-height=32 --export-type=png -o $@ $^

ui_button.png: ui_button.svg
	inkscape --export-width=16 --export-height=16 --export-type=png -o $@ $^

ui_checkmark.png: ui_checkmark.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_dropdown.png: ui_dropdown.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_leftturn.png: ui_leftturn.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_rightturn.png: ui_rightturn.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_tabdesel.png: ui_tabdesel.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_tabempty.png: ui_tabempty.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_tabsel.png: ui_tabsel.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_toggle_off.png: ui_toggle_off.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

ui_toggle_on.png: ui_toggle_on.svg
	inkscape --export-width=24 --export-height=24 --export-type=png -o $@ $^

HARMONY := 0Harmony.1.2.0.1.dll

clean:
	rm -f $(ICONS) $(UI_IMAGES)

install: all
	mkdir -p ${MODGAMEDATA}/Textures
	cp $(ICONS) ${MODGAMEDATA}/Textures
	mkdir -p $(MODGAMEDATA)/UI
	cp $(UI_CFG) $(UI_IMAGES) $(MODGAMEDATA)/UI
	cp ${HARMONY} ${PLUGINDIR}

.PHONY: all clean install

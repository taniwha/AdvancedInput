MODNAME		:= AdvancedInput
KSPDIR		:= ${HOME}/ksp/KSP_linux
MANAGED		:= ${KSPDIR}/KSP_Data/Managed
GAMEDATA	:= ${KSPDIR}/GameData
MODGAMEDATA := ${GAMEDATA}/${MODNAME}
PLUGINDIR	:= ${MODGAMEDATA}/Plugins

TARGETS		:= bin/${MODNAME}.dll

MOD_FILES := \
    Assembly/AssemblyInfo.cs	\
	Assembly/VersionReport.cs	\
	Linux/InputLib.cs			\
	AdvancedInput.cs			\
	Data.cs						\
	DeviceNamesContainer.cs		\
	FlightControl.cs			\
	$e

DOC_FILES := \
	License.txt \
	README.md

RESGEN2		:= resgen2
GMCS		:= mcs
GMCSFLAGS	:= -optimize -warnaserror -unsafe
GIT			:= git
TAR			:= tar
ZIP			:= zip

SUBDIRS=Linux

all: version ${TARGETS}
	@for dir in ${SUBDIRS}; do \
		make -C $$dir $@ || exit 1; \
	done

.PHONY: version
version:
	@../tools/git-version.sh

info:
	@echo "${MODNAME} Build Information"
	@echo "    resgen2:    ${RESGEN2}"
	@echo "    gmcs:       ${GMCS}"
	@echo "    gmcs flags: ${GMCSFLAGS}"
	@echo "    git:        ${GIT}"
	@echo "    tar:        ${TAR}"
	@echo "    zip:        ${ZIP}"
	@echo "    KSP Data:   ${KSPDIR}"

bin/${MODNAME}.dll: ${MOD_FILES}
	@mkdir -p bin
	${GMCS} ${GMCSFLAGS} -t:library -lib:${MANAGED} \
		-r:Assembly-CSharp,Assembly-CSharp-firstpass \
		-r:UnityEngine,UnityEngine.UI \
		-out:$@ $^

clean:
	@for dir in ${SUBDIRS}; do \
		make -C $$dir $@ || exit 1; \
	done
	rm -f ${TARGETS} Assembly/AssemblyInfo.cs
	rmdir bin

install: all
	@for dir in ${SUBDIRS}; do \
		make -C $$dir $@ || exit 1; \
	done
	mkdir -p ${PLUGINDIR}
	cp ${TARGETS} ${PLUGINDIR}
#	cp ${DOC_FILES} ${MODGAMEDATA}

.PHONY: all clean install

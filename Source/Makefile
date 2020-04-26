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
	AxisBindings/LinearBrake.cs	\
	AxisBindings/MainThrottle.cs	\
	AxisBindings/Pitch.cs	\
	AxisBindings/PitchTrim.cs	\
	AxisBindings/Roll.cs	\
	AxisBindings/RollTrim.cs	\
	AxisBindings/ThrottleIncrement.cs	\
	AxisBindings/WheelSteer.cs	\
	AxisBindings/WheelSteerTrim.cs	\
	AxisBindings/WheelThrottle.cs	\
	AxisBindings/WheelThrottleTrim.cs	\
	AxisBindings/X.cs	\
	AxisBindings/Y.cs	\
	AxisBindings/Yaw.cs	\
	AxisBindings/YawTrim.cs	\
	AxisBindings/Z.cs	\
	ButtonBindings/Abort.cs	\
	ButtonBindings/ActionGroup.cs	\
	ButtonBindings/Brake.cs	\
	ButtonBindings/ButtonMode.cs	\
	ButtonBindings/CustomAction.cs	\
	ButtonBindings/Gear.cs	\
	ButtonBindings/Lights.cs	\
	ButtonBindings/MainThrottleLock.cs	\
	ButtonBindings/SAS.cs	\
	ButtonBindings/Stage.cs	\
	ButtonBindings/TranslateBack.cs	\
	ButtonBindings/TranslateDown.cs	\
	ButtonBindings/TranslateForward.cs	\
	ButtonBindings/TranslateLeft.cs	\
	ButtonBindings/TranslateRight.cs	\
	ButtonBindings/TranslateUp.cs	\
	ButtonBindings/WheelThrottleLock.cs	\
	Linux/InputLib.cs			\
	UI/ConfigWindow.cs			\
	UI/ScrollView.cs			\
	UI/Styles.cs				\
	UI/TextField.cs				\
	AdvancedInput.cs			\
	AxisBinding.cs				\
	AxisRecipe.cs				\
	BindingSet.cs				\
	ButtonBinding.cs			\
	Data.cs						\
	Device.cs					\
	DeviceNamesContainer.cs		\
	FlightControl.cs			\
	LinearBraking.cs			\
	SAS.cs						\
	Toolbar.cs					\
	Utils.cs					\
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
		-r:UnityEngine,UnityEngine.UI,UnityEngine.IMGUIModule,UnityEngine.CoreModule,UnityEngine.TextRenderingModule,UnityEngine.InputLegacyModule,UnityEngine.AnimationModule \
		-r:../Assets/0Harmony.1.2.0.1 \
		-out:$@ $^

clean:
	@for dir in ${SUBDIRS}; do \
		make -C $$dir $@ || exit 1; \
	done
	rm -f ${TARGETS} Assembly/AssemblyInfo.cs
	rm -rf bin

install: all
	@for dir in ${SUBDIRS}; do \
		make -C $$dir $@ || exit 1; \
	done
	mkdir -p ${PLUGINDIR}
	cp ${TARGETS} ${PLUGINDIR}

.PHONY: all clean install

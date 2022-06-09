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
	AxisBindings/EVAParachutePitch.cs	\
	AxisBindings/EVAParachuteRoll.cs	\
	AxisBindings/EVAX.cs	\
	AxisBindings/EVAPitch.cs	\
	AxisBindings/EVARoll.cs	\
	AxisBindings/EVAX.cs	\
	AxisBindings/EVAY.cs	\
	AxisBindings/EVAYaw.cs	\
	AxisBindings/EVAZ.cs	\
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
	ButtonBindings/EVAAngularBrake.cs	\
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
	UI/SettingsWindow.cs		\
	AdvancedInput.cs			\
	AxisBinding.cs				\
	AxisRecipe.cs				\
	BindingSet.cs				\
	ButtonBinding.cs			\
	Data.cs						\
	Device.cs					\
	DeviceNamesContainer.cs		\
	FlightControl.cs			\
	KerbalCtrlState.cs			\
	KerbalEVA.cs				\
	LinearBraking.cs			\
	SAS.cs						\
	Toolbar.cs					\
	Utils.cs					\
	$e

include KodeUI/KodeUI.inc

DOC_FILES := \
	License.txt \
	README.md

RESGEN2		:= resgen2
CSC		:= csc
CSCFLAGS	:= -highentropyva- -noconfig -nostdlib+ -t:library -optimize -warnaserror -debug -unsafe
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
	@echo "    csc:        ${CSC}"
	@echo "    csc flags:  ${CSCFLAGS}"
	@echo "    git:        ${GIT}"
	@echo "    tar:        ${TAR}"
	@echo "    zip:        ${ZIP}"
	@echo "    KSP Data:   ${KSPDIR}"

SYSTEM := \
	-lib:${MANAGED} \
	-r:${MANAGED}/mscorlib.dll \
	-r:${MANAGED}/System.dll \
	-r:${MANAGED}/System.Core.dll

KSP := \
	-r:Assembly-CSharp.dll \
	-r:Assembly-CSharp-firstpass.dll

UNITY := \
	-r:UnityEngine.dll \
	-r:UnityEngine.UI.dll \
	-r:UnityEngine.UIModule.dll \
	-r:UnityEngine.CoreModule.dll \
	-r:UnityEngine.TextRenderingModule.dll \
	-r:UnityEngine.InputLegacyModule.dll \
	-r:UnityEngine.AnimationModule.dll \
	-r:UnityEngine.PhysicsModule.dll \
	-r:UnityEngine.Physics2DModule.dll \
	$e

bin/${MODNAME}.dll: ${MOD_FILES} ${KodeUI}
	@mkdir -p bin
	${CSC} ${CSCFLAGS} ${SYSTEM} ${KSP} ${UNITY} \
		-r:../Assets/0Harmony.1.2.0.1.dll \
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

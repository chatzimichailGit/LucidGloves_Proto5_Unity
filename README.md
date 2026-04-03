# Haptic Glove Workbench

A single workspace that bundles the LucidVR LucidGloves firmware + STL resources, my Prototype 5 / Quest 2  , and the Unity experience that drives the latest haptic demo,based on the adaptations  I made for my bachelor's dissertation.

## Repository layout
- `lucidgloves-main/` – the preserved past snapshot with the upstream LucidVR firmware updates and documentation that I have been building on for an ESP32.
- `questtest1/` – Unity 6.0.0.29f1 project (XR Interaction Toolkit 3.1.1 + XR Hands 1.5.1) with custom scripts that parse the glove serial data, animate the hand models, and play haptic responses. `HapticQuest/` stores the last Windows build exported from that project.


## Requirements
- **Meta Quest headset (Quest 2 or later)** with hand tracking enabled. The Unity scenes rely on the headset’s front cameras to detect the hands, so the Quest’s built-in tracking is mandatory.
- A USB/serial-equipped controller board (Arduino Nano, ESP32, etc.) wired to the glove. The custom firmware lives under `lucidgloves/firmware/`.
- Hall effect sensors are part of the original LucidGloves build, but they are optional here because the Quest camera tracking provides the hand pose—install them only if you want redundant sensing.
- Prototype 5 3D-printed hardware (STL files under `lucidgloves/hardware/Prototype5_BETA/`).
- The servos need external powersupply.

## Getting started
1. **Firmware & hardware** – Open `lucidgloves/firmware/lucidgloves-firmware.ino` in Arduino IDE or PlatformIO, adjust the pin/configuration defines for your board, and flash. Print the Prototype 5 STL files under `lucidgloves/hardware/`.
2. **Unity / Quest experience** – Open `questtest1/questtest1.sln` with Unity 6.0.0.29f1, load the scenes, and ensure the XR settings reference the Oculus/OpenXR loaders that ship with the project. The scripts in `Assets/` handle haptics, parsing the glove serial stream, and interacting with physics objects.
3. Make sure to setup The correct COM PORT from ESP32Bridge
4. Test first without wearing the glove by pointing your hand towards objects with haptics enabled.

5.Download Meta Horizon Link and connect the headset.(if steamVR is enabled like the original LucidVR  it wont work)
## Attribution & licensing
- LucidVR / Lucas_VRTech created the original LucidGloves project. The `lucidgloves/` directory retains their MIT license (`lucidgloves/LICENSE`) and README. Consider that folder a working copy of their work with my Prototype 5 updates layered on top; the upstream snapshot is kept verbatim in `lucidgloves-main/`.
- This repository (the workspace root) follows MIT-friendly terms that mirror the LucidVR notices so collaborators understand the boundaries between my custom code and the upstream contribution.


## What I learned
- Implemented and adapted an open-source haptic glove design, gaining hands-on experience with Hall-effect sensors, servo actuation, 3D printing, wiring, and hardware integration.
- Built a custom Unity workflow that communicates directly with the microcontroller, helping me understand VR interaction design, object-based haptic logic, and real-time serial data handling.
- Explored alternative haptic approaches, including piezoelectric actuation research, and compared them against the final servo-based path used in the prototype.


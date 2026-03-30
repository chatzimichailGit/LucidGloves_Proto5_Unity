# Haptic Glove Workbench

A single workspace that bundles the LucidVR LucidGloves firmware + STL resources, my Prototype 5 / Quest 2 adaptations, and the Unity experience that drives the latest haptic demo.

## Repository layout
- `lucidgloves-main/` – the preserved past snapshot with the upstream LucidVR firmware updates and documentation that I have been building on.
- `lucidgloves/` – the Prototype 5 iteration I am currently developing; it contains the MIT-licensed firmware, my wiring/geometry tweaks, and the STL files for 3D printing the new parts.
- `questtest1/` – Unity 6.0.0.29f1 project (XR Interaction Toolkit 3.1.1 + XR Hands 1.5.1) with custom scripts that parse the glove serial data, animate the hand models, and play haptic responses. `HapticQuest/` stores the last Windows build exported from that project.
- helper sketches/experiments: `BLETest/`, `buzzertest/`, `esp32buzz/`, `multiplexertest/`, `opengloves-driver/`, `serialtest.py`, etc.

## Requirements
- **Meta Quest headset (Quest 2 or later)** with hand tracking enabled. The Unity scenes rely on the headset’s front cameras to detect the hands, so the Quest’s built-in tracking is mandatory.
- A USB/serial-equipped controller board (Arduino Nano, ESP32, etc.) wired to the glove. The custom firmware lives under `lucidgloves/firmware/`.
- Hall effect sensors are part of the original LucidGloves build, but they are optional here because the Quest camera tracking provides the hand pose—install them only if you want redundant sensing.
- Prototype 5 3D-printed hardware (STL files under `lucidgloves/hardware/Prototype5_BETA/`).

## Getting started
1. **Firmware & hardware** – Open `lucidgloves/firmware/lucidgloves-firmware.ino` in Arduino IDE or PlatformIO, adjust the pin/configuration defines for your board, and flash. Print the Prototype 5 STL files under `lucidgloves/hardware/`.
2. **Unity / Quest experience** – Open `questtest1/questtest1.sln` with Unity 6.0.0.29f1, load the scenes, and ensure the XR settings reference the Oculus/OpenXR loaders that ship with the project. The scripts in `Assets/` handle haptics, parsing the glove serial stream, and interacting with physics objects.
3. **Optional Windows build** – `questtest1/HapticQuest/` currently contains a Windows player build for demonstration. Delete it if you prefer to keep the repo slim and rebuild the player locally when needed.

## Attribution & licensing
- LucidVR / Lucas_VRTech created the original LucidGloves project. The `lucidgloves/` directory retains their MIT license (`lucidgloves/LICENSE`) and README. Consider that folder a working copy of their work with my Prototype 5 updates layered on top; the upstream snapshot is kept verbatim in `lucidgloves-main/`.
- This repository (the workspace root) follows MIT-friendly terms that mirror the LucidVR notices so collaborators understand the boundaries between my custom code and the upstream contribution.

## Publishing to GitHub
1. Create your target repository on GitHub (or use an existing one). Note the HTTPS URL, for example `https://github.com/<your-username>/haptic-glove.git`.
2. In this folder run:
   ```
   git remote set-url origin https://github.com/<your-username>/haptic-glove.git
   git push -u origin main
   ```
3. (Optional) If you want a fresh, clean history that only contains the current workspace, create a new folder elsewhere, copy `lucidgloves/` + `questtest1/` into it, `git init`, add a README (like this one), and push. That keeps this existing history for archival purposes.

Let me know if you want help creating the remote via the GitHub CLI or crafting a release note that highlights the Prototype 5 + Quest support.
## What I learned
- Implemented and adapted an open-source haptic glove design, gaining hands-on experience with Hall-effect sensors, servo actuation, 3D printing, wiring, and hardware integration.
- Built a custom Unity workflow that communicates directly with the microcontroller, helping me understand VR interaction design, object-based haptic logic, and real-time serial data handling.
- Explored alternative haptic approaches, including piezoelectric actuation research, and compared them against the final servo-based path used in the prototype.


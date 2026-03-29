# Lucid Gloves Proto 5 Unity Suite

This repository bundles the Proto 5 version of the LucidGloves firmware plus the Unity/Quest experience that drives the current haptic demo.

## Contents
- `lucidgloves-adapted/` – MIT-based LucidGloves firmware with my Prototype 5 hardware tweaks, wiring improvements, and configuration helpers. The original LucidVR license (`lucidgloves-adapted/LICENSE`) remains in place and should stay with any redistributed firmware.
- `questtest1/` – Unity 6.0.0.29f1 project built with XR Interaction Toolkit 3.1.1 and XR Hands 1.5.1. Custom scripts under `Assets/` read the glove serial stream, animate the finger poses, and trigger haptics. The `HapticQuest/` folder contains the latest Windows player build exported from that project (remove it if you want a lightweight repo and rebuild locally).

## Requirements
1. **Meta Quest (Quest 2 or later)** with hand tracking enabled. The scenes rely on the headset’s cameras to detect the user’s hands, so without a Quest headset and the built-in tracking you cannot run the demo.
2. **Microcontroller board** (Arduino Nano, ESP32, etc.) wired to your Proto 5 glove. Flash the firmware in `lucidgloves-adapted/` with the correct pin mappings for your chosen board.
3. **Prototype 5 3D-printed parts** – print the STL files that ship with the `lucidgloves` firmware folder if you need replacement hardware.
4. **Hall effect sensors** are optional; Quest camera tracking already detects finger motion, so install them only when you want redundant sensing or legacy compatibility.

## Getting started
1. Install the Arduino IDE or PlatformIO and open `lucidgloves-adapted/lucidgloves-firmware.ino`. Adjust the defines and serial settings for your board, then flash the board.
2. Launch Unity 6.0.0.29f1 and open `questtest1.sln` (the Unity project root is `questtest1/`). Load the scenes you need and make sure the XR settings reference the Oculus/OpenXR loaders included in the project. Plug the glove into your PC and let the scripts (`GloveParser`, `HapticGrabHelper`, `HapticGrabResponder`, etc.) receive the serial packets.
3. (Optional) Run the built Windows player in `questtest1/HapticQuest/` as a demo, or delete that folder and rebuild locally if you want a clean repository.

## Attribution
- This project builds on the original LucidGloves work by Lucas_VRTech / LucidVR. The firmware and STL core remain under their MIT notice (`lucidgloves-adapted/LICENSE`) and the upstream repo is preserved for reference. Thank you to LucidVR for releasing the materials that made this Proto 5 fork possible.
- The Unity assets are mine, but they rely on Unity’s XR Interaction Toolkit and XR Hands packages, so install those packages via the Package Manager if they are not already cached.

## Publishing tips
- Keep large binaries (like full Windows builds or `.mp4` recordings) outside this repo or manage them via Git LFS to avoid GitHub’s 100?MB limit.
- When pushing updates, verify `git status` is clean, and then run `git push -u origin main` (the branch now named `main`).
- Feel free to add a GitHub release note that highlights Prototype 5 support, Quest hand tracking requirements, and the LucidGloves attribution.

EOF

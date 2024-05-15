# CIDER: Collaborative Interior Design in Extended Reality

## About

CIDER (Collaborative Interior Design in Extended Reality) is an Augmented Reality (AR) application for Microsoft Hololens 2. It allows collaborative manipulation of 3D scenes between two or more users, in particular for interior design tasks.<br>
Build in C# with Unity and the [Microsoft Mixed Reality Toolkit 2 (MRTK2)](https://github.com/microsoft/MixedRealityToolkit-Unity).

This is an implementation of the system described in our paper of the same name: https://doi.org/10.1145/3605390.3605419

External view              |  Internal view
:-------------------------:|:-------------------------:
<img src="https://github.com/DebbyX3/CIDERHololensCollabManip/assets/26549164/feace467-6180-41a2-801e-af2dd50ce179" height="220"> | <img src="https://github.com/DebbyX3/CIDERHololensCollabManip/assets/26549164/5a37cb46-2d80-4421-930a-bda7664cbfcb" height="220">

### Video
Watch a video here: https://mega.nz/embed/nEdCFKKb#09WMKL2anHoGuXrepv4myk-sNmdohAE_T4D9urovbZk
<p align="center">
  <a href="https://mega.nz/embed/nEdCFKKb#09WMKL2anHoGuXrepv4myk-sNmdohAE_T4D9urovbZk"><img height="300" src="https://i.imgur.com/4QauwEUl.png" alt="Watch the video"></a><br>
  
</p>

<!---
[![Watch the video](https://i.imgur.com/4QauwEUl.png)](https://mega.nz/embed/nEdCFKKb#09WMKL2anHoGuXrepv4myk-sNmdohAE_T4D9urovbZk)
-->

## Goal

This work proposes (and evaluates) a system for shared collaborative interior design tasks, specifically the editing of augmented 3D scenes in co-located collaborative virtual environments (CVEs).

Goals: 
- Allow designers and non-designers to create spaces together
- Let users devise ideas independently, without surprises
- Share only valid ideas

It achieves them by letting users:
- Propose different variations of objects and scenes
- Compare the results
- Discuss them
- Create optimally merged results

## Examples of final scenes generated with CIDER

Living Room                |  Office
:-------------------------:|:-------------------------:
<img src="https://i.imgur.com/LxLhOHTl.jpg" height="220"> | <img src="https://i.imgur.com/prwqf6nl.jpg" height="220">
<img src="https://github.com/DebbyX3/CIDERHololensCollabManip/assets/26549164/912714f5-6f62-43ef-bf22-54a8ad3783d9" height="220"> | <img src="https://github.com/DebbyX3/CIDERHololensCollabManip/assets/26549164/fdb2d860-3200-4470-b93a-d01927037a66" height="220">

## Application overview

The application

## How to run

### Pre-requisites
 - Windows 10 or 11
 - Unity, at least version 2020.3.25f1
 - Microsoft Visual Studio, at least Community 2019 edition 16.11.8
 - MS Visual Studio set as the default external script editor in Unity 
 - Two Microsoft Hololens 2 devices

### How to
 - Clone the repository 
 - Import the root folder of the repository on Unity

### Prepare the environments - One-time setup

#### One-time first setup - Unity
 - Close all instances of Unity
 - From Unity Hub, go to Installs -> click the three dots on the installed Unity version -> Add Modules
 - Install "Universal Windows Platform Build Support"
 - Open Unity
 - Open the Scene located in Assets\Scenes\Main 4 WLT
 - In Unity, go to File -> Build Settings and under Platform select "Universal Windows Platform", then click the button "Switch platform" 
 - Select the following options in the drop-down menus:
   - Target device: Any Device
   - Architecture: x64
   - Build Type: D3D Project
   - Target SDK Version: Latest installed
   - Minimum platform version: 10.0.10240.0
   - Visual Studio Version: Latest installed
   - Build and run on: Local Machine
   - Build configuration: Master
   - Check "Copy References"
   - All other checkboxes should remain unchecked
 - Restart Unity and re-open the project

#### One-time first setup - Visual Studio
Skip these 3 steps if you've already installed the "Universal Windows Platform Development", "Desktop Development with C++" and "Game development with Unity" workloads on Visual Studio.

- Open Visual Studio Installer
- Locate your latest installed Visual Studio version and click "Edit"
- In the Workloads tab, check and install "Desktop Development with C++", "Universal Windows Platform Development" and "Game development with Unity"

Next, set MS Visual Studio to compile for the right architecture:
- Open Visual Studio Installer
- Locate your latest installed Visual Studio version and click "Edit"
- In the Single component tab, install "Support for UWP platform (Universal Windows Platform) C++ (v142)"
- In the Single components tab, search for "arm64"
- Check and install "MSVC v142 - VS 2019 C++ ARM64 build tools (Latest)" and "Support for UWP platform (Universal Windows Platform) C++ for Build Tools v142 (ARM64)"

Please note that the version of the components may be different if you have a different version of Visual Studio. As a rule of thumb, download the latest version.


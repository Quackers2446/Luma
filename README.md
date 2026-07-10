<<<<<<< HEAD
# Luma: Cozy AR Companion 🌸✨

**Luma** is a peaceful, magic-infused mobile AR companion app. Users can invite cute 2D illustrated characters (Sprout, Nimbus, and Mocha) into the real world using AR, move them around with natural gestures, change their expressions, and capture cozy photos or videos.

The emphasis is entirely on creating quiet, magical moments of connection in real space.

---

## 🛠️ Technology Stack

### Frontend (Unity Mobile Application)
* **Unity Version**: `2022.3.62f3 LTS` (Silicon/Mac native)
* **AR Framework**: AR Foundation 5.x
* **Device Drivers**: ARCore (Android) / ARKit (iOS)
* **API Handlers**: UnityWebRequest with dynamic Pixels Per Unit (PPU) scaling
* **Input Manager**: Unity legacy input system (optimized with unified `Both` backends)

### Backend (Node.js API)
* **Server**: Node.js + Express
* **Database**: PostgreSQL (persisting companion metadata, descriptions, and file pointers)
* **Assets**: PNG illustrations served dynamically with custom request header resolving (binds local network IP to support live phone testing)

---

## 🎯 Key MVP Features

### 🏡 Selection Screen (Home)
* **Refined Aesthetics**: Dark purple-indigo brand gradient background.
* **Frosted Glass UI**: Clean glassmorphism layout with translucent cards and glowing purple controls.
* **Invite Cards**: Beautiful slots with rounded thumbnails, bold character titles, and glowing peachy-coral `"INVITE →"` tags.
* **Auto-Discovery**: Pre-populates your Mac's current local network IP so the phone immediately loads characters over Wi-Fi without manual typing.

### 🍃 Spatial AR Placement
* **AR Plane Visualizer**: Soft green translucent grid overlays automatically fade in on floors and tables as soon as scanning completes, showing exactly where companions can stand.
* **Solid World Placement**: Enabled native `ARPoseDriver` tracking. Virtual coordinates match physical spaces; you can turn your phone away, walk around, and find your companions standing exactly where you left them.
* **Cylindrical Billboarding**: 2D illustrations always rotate upright to face the camera, maintaining a readable, cozy visual presence.
* **Natural Touch Gestures**:
  * **Drag**: Move companions smoothly along detected floors/tables.
  * **Pinch-to-Scale**: Resize companions (clamped between 3.5cm desk accessories and 1.05m giant toys).
  * **Two-Finger Rotate**: Spin companions around their vertical axes relative to the camera vector.

### 📸 Media Capture & Gallery Scanner
* **Photo Mode**: Hides the UI HUD completely, captures a screenshot, and saves it.
* **Video Mode**: Features a red recording overlay for 3 seconds, captures a mock recording, and saves it.
* **Android Media Folder Bypass**: Saves media under `/Android/media/` instead of `/Android/data/` so that the Android system MediaScanner has read permissions, allowing photos and videos to show up instantly inside your phone's native Photos/Gallery app.

---

## 🚀 Getting Started

### 1. Run the Express Backend
From the `backend` directory, launch the Node server:
```bash
cd backend
npm install
npm run init-db   # Seeds the PostgreSQL database
npm start        # Runs the backend on port 3000
```

### 2. Scaffold & Build the App
You can build the Unity client in two ways:

#### A. Direct Editor Build (Keep Unity Open)
1. Open the Unity project (`frontend` directory).
2. Click **Tools > Cozy AR > Setup, Build & Run** from the top menu bar.
3. Unity will regenerate the scenes with the new visual layout, compile the package, install it via USB, and open it on your phone!

#### B. Command Line CLI Build (Close Unity First)
If you close the Unity Editor, you can compile and deploy directly from your Mac terminal using our automation script:
```bash
./build_and_run.sh
```

---

## 📂 File Architecture (Highlights)
* **Scaffolding Builder**: [ProjectSetup.cs](file:///Users/quackers/dev/Luma/frontend/Assets/Editor/ProjectSetup.cs) (programmatic scene constructor, canvas scaler, EventSystem injector, Android compiler settings manager)
* **AR Manager**: [ARPlacementManager.cs](file:///Users/quackers/dev/Luma/frontend/Assets/Scripts/AR/ARPlacementManager.cs) (raycasting, tap-to-place, gesture drags, pinch-to-scale, two-finger rotations)
* **Gallery Helper**: [NativeGalleryHelper.cs](file:///Users/quackers/dev/Luma/frontend/Assets/Scripts/Utils/NativeGalleryHelper.cs) (Android Broadcast Intent triggers for gallery scanning)
* **Character Controller**: [CharacterController.cs](file:///Users/quackers/dev/Luma/frontend/Assets/Scripts/AR/CharacterController.cs) (expression swaps, dynamic box collider size adjustments)
* **Asset Scaler**: [BackendService.cs](file:///Users/quackers/dev/Luma/frontend/Assets/Scripts/Network/BackendService.cs) (JSON list parser, texture cacher, dynamic 35cm PPU sizing calculator)
* **Billboard Driver**: [CharacterBillboard.cs](file:///Users/quackers/dev/Luma/frontend/Assets/Scripts/AR/CharacterBillboard.cs) (camera-facing cylindrical transform constraints)
* **Automated Builder**: [build_and_run.sh](file:///Users/quackers/dev/Luma/build_and_run.sh) (command-line batch build and install wrapper)
=======
# Luma

## Overview

### Game Flow

Google Play Store / Apple Store App
 - Install
 - Make an Account
 - Character Creation
 - Create your own character

Gameplay Loop??

- Players have the ability to place their character through the camera onto a real life canvas (some sort of interaction)

>>>>>>> 3d1dc1faac6cb6bd68385bc8af615f12c7ef75d0

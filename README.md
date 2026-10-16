# JamKit: No-Code Unity Starter Kit

A no-code Unity starter kit for first-time game jammers: three player controllers (2D platformer, 2D top-down, 3D third-person), drag-and-drop World prefabs, win/lose screens and a WebGL export in one click.
Built for the **GameJamPlus Bangladesh 2026** pre-jam workshop.

**Golden rule:** Drag a prefab → Set fields in the Inspector → Press Play.

## Get it
- **Template project (recommended):** download `JamKit_Template_v1.0.zip` from Releases, unzip, Unity Hub → **Add** → pick the `JamKit` folder. Unity **6000.3.14f1** (Unity 6.3 LTS) with **Web Build Support**.
- **Or clone / Download ZIP** this repo: the repo root is the Unity project.
- **Mirror:** [Google Drive folder](https://drive.google.com/drive/folders/1g4-7vBvg0_PyQiaWrimGVb7xITLxi_0M) (same zip + cheat sheet PDF).
- **Existing project:** import `JamKit_v1.0.unitypackage` into a **URP** project (Universal 2D template), set Project Settings → Player → Active Input Handling = **Both**, then run **JamKit → Validate Scene** in a demo scene and click **Fix** on the renderer check (adds the 3D renderer at index 1).

## Inside
- `Assets/JamKit/Scenes/Demo/`: `Demo_2D_Platformer`, `Demo_2D_TopDown`, `Demo_3D_ThirdPerson`
- `Assets/JamKit/Docs/`: `JamKit_CheatSheet.pdf`, `README.md`, `JamKit_Reference.md` (every component, field and menu, generated from code)
- `JamKit` menu: New Level, Validate Scene, Prepare WebGL Build, 3D → Setup Mixamo Character, Help

## Licence
Code: MIT (see `LICENSE`). Placeholder art/audio: CC0. Mixamo files are never included. Credit in your jam game: *Made with JamKit by Shounak Sobahani (MIT)*.

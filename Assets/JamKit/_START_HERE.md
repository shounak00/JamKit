# Start here: JamKit v1.1

**Make a game without code.** The golden rule:

> **Drag a prefab → Set fields in the Inspector → Press Play.**

1. **Play a demo:** open `JamKit/Scenes/Demo/` and double-click `Demo_2D_Platformer`, `Demo_2D_TopDown` or `Demo_3D_ThirdPerson`. Press Play.
2. **Make your level:** menu **JamKit → New Level** (2D Platformer, 2D Top-Down or 3D Third-Person). Your level is saved in `Assets/_Game/Scenes`.
3. **Build it:** paint ground with the Tile Palette (Window → 2D → Tile Palette, palette `JamKit_Palette`), then drag prefabs from `JamKit/Prefabs/World` (Coin, Hazard, Goal…) into the `---- LEVEL ----` section.
4. **Your art:** select a prefab's `Visual` child and drop your sprite into the Sprite Renderer. Never edit the root.
5. **Check it:** **JamKit → Validate Scene**. Click **Fix** on anything red.
6. **Ship it (Friday night!):** **JamKit → Prepare WebGL Build**, then upload to itch.io.

Your files go in **`Assets/_Game`**. Don't edit `Assets/JamKit` (a kit update would overwrite it).

Full guide: **JamKit → Help (Cheat Sheet)** opens `Docs/JamKit_CheatSheet.pdf` (or `Docs/README.md`).

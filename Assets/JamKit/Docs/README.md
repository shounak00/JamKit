# JamKit v1.0: No-Code Unity Starter Kit

Short version of `JamKit_CheatSheet.pdf`. Exact component, field and menu names are in `JamKit_Reference.md`.

**Unity:** 6000.3.14f1 (Unity 6.3 LTS), URP. · **Repo:** https://github.com/shounak00/JamKit

## The golden rule
**Drag a prefab → Set fields in the Inspector → Press Play.** No code.

## Where things are
```
Assets/
├── _Game/      ← YOURS: Art, Audio, Prefabs, Scenes
└── JamKit/     ← the kit: use it, don't edit it
    ├── Prefabs/   Player · World · Systems · UI   (things you drag)
    ├── Scenes/    MainMenu · Level templates · Demo/
    ├── Configs/   PlayerConfig presets (the feel)
    ├── Tilemaps/  tiles + JamKit_Palette
    └── Docs/      this file, the cheat sheet, the reference
```
Every level has the same sections: `---- SYSTEMS ----`, `---- CAMERA ----`, `---- PLAYER ----`, `---- LEVEL ----`, `---- BACKGROUND ----`, `---- UI ----`.
Every World prefab is **Root / Visual / FX**: swap art on `Visual`, never edit the root. On the 3D player swap `Model`.

## Quick start
1. **JamKit → New Level → 2D Platformer** (or Top-Down / 3D Third-Person).
2. Paint ground: Window → 2D → Tile Palette → `JamKit_Palette`, paint on `Tilemap_Ground`.
3. Drag `JamKit/Prefabs/World/Coin_2D`, `Hazard_2D`, `Goal_2D` into the level.
4. Press Play. Touch the flag: you win. Touch the hazard 3 times: you lose. Press R to restart.
5. **JamKit → Validate Scene** → fix anything red.
6. **JamKit → Prepare WebGL Build** → Build → zip the folder's contents → upload to itch.io.

## Controls
| Action | Keyboard / mouse | Gamepad |
|---|---|---|
| Move | WASD / Arrows | Left stick |
| Jump | Space (W / Up in the 2D platformer) | A / ✕ |
| Sprint / Dash | Left Shift | X / □ or RT |
| Interact | E | Y / △ |
| Fire | J, Left Ctrl, Left mouse | B / ○ |
| Look (3D) | Mouse | Right stick |
| Pause / Restart | Esc or P / R | Start / Select |

## Recipes (no code)
- **Collect all coins to open the goal:** on `Goal_2D` set *Required Item Id* `coin` and *Required Amount* (or tick *Require All In Level*).
- **Key opens a door:** place `Key_2D`, `Door_2D` and a `TriggerZone_2D` next to the door. Zone: *Required Item Id* `key`, tick *Consume Item*. In **On Enter (+)** drag the Door in and pick `JamDoor → Open`.
- **Moving platform / elevator:** drag `MovingPlatform_2D`; move `Waypoints/Point1` in the Scene view. Tick *Start On Trigger* for an elevator.
- **Stompable enemy:** drag `PatrolEnemy_2D`. Jump on its head.
- **Survive 60 seconds:** add `JamTimer` to the GameManager: Count Down, 60, *On Zero* = Win.
- **Race the clock:** same, *On Zero* = Lose.
- **Checkpoints and lives:** drag `Checkpoint_2D`; on the GameManager tick *Use Lives*.
- **Tutorial sign:** drag `Sign_2D`, type your lines.
- **Health pickup:** drag `Heart_2D`.
- **Next level:** on the GameManager set *Next Level Name*; add both scenes to Build Settings (Validate Scene → Fix).
- **Enemy waves:** `Spawner_2D` + a Trigger Zone calling `JamSpawner → StartSpawning`.
- **Top-down shooting:** the top-down player already shoots (J / click) through `JamShooter` on its `FirePoint`.
- **Sounds:** any event → drag an object with `JamAudio` → `PlayOneShot` → drop your sound.

## Game feel
Drag a different `PlayerConfig` (Snappy, Floaty, Heavy…) into the player's **Config** field, or edit the asset while playing. Changes are kept.

## 3D and Mixamo
Start from **JamKit → New Level → 3D Third-Person** and use the `_3D` prefabs. To use a Mixamo character: download it from mixamo.com as **FBX for Unity, T-pose, With Skin** (animations: tick **In Place**, Without Skin), drop the files into `JamKit/_MixamoDropZone`, then **JamKit → 3D → Setup Mixamo Character**. Mixamo files are fine inside your game; never re-upload the raw files.

## Troubleshooting
| Problem | Fix |
|---|---|
| Player falls through the floor | Ground needs a collider (Tilemap Collider 2D + Composite Collider 2D; 3D: Box/Mesh Collider) |
| Pink sprites or materials | Use the template project (URP), or run Setup Mixamo Character again |
| Keyboard does nothing | Project Settings → Player → Active Input Handling = Both; click the Game view |
| Win screen never shows | No GameManager: Validate Scene → Fix |
| Next level doesn't load | Scene not in Build Settings: Validate Scene → Fix |
| 3D character T-poses / slides | Rig not Humanoid, or clip not In Place: run Setup Mixamo Character, re-download In Place |
| Mouse cursor disappeared (3D) | Press Esc |
| Blank page on itch.io | Run Prepare WebGL Build again (compression Disabled) and re-upload |

## Submission checklist
Build on the **first night of the jam** · test the itch link in an incognito window · cover image · controls on the page · credits (JamKit, CC0 art, Mixamo if used) · upload with time to spare.

Credit line for your jam page: *Made with JamKit by Shounak Sobahani (MIT) — github.com/shounak00/JamKit*.

# NAMES_CHANGED: where the kit differs from the plan

`JamKit_Reference.md` (generated from the code) wins over the plan. This list explains every difference so the slides and PDF can follow the code.

## Platform

| Plan | Kit | Why |
|---|---|---|
| Unity 6 LTS `6000.0.x` | **`6000.3.14f1` (Unity 6.3 LTS)** | Installed LTS with Web + Windows build support. Use this exact version in the install checklist (§7) and on slide A6. |
| Scene section headers tagged **EditorOnly** | Section headers are **plain GameObjects** (not EditorOnly) | EditorOnly objects are stripped from builds **with their children**, so every system under `---- SYSTEMS ----` would vanish in the WebGL build. |
| Active Input Handling: Both (troubleshooting) | Set to **Both** in the template project | As planned. Takes effect after the editor restarts. |

## Input (`JamInput`)

| Plan | Kit |
|---|---|
| `JamInput` component with optional rebind fields | `JamInput` is a **static helper** (nothing to add to a scene, no rebind fields). Keys are fixed: see the Reference §4. |
| — | Added **Fire**: J, Left Ctrl or Left mouse / gamepad East. Used by `JamShooter` with *Fire On Input*. |
| Pause: Esc / Start | Pause: **Esc or P** / Start (Esc also leaves fullscreen in browsers, P always works). |

## Systems

| Plan | Kit |
|---|---|
| `nextLevelName` blank = show Win screen | The Win screen **always** shows. `nextLevelName` set → its **Next** button loads that scene; blank → the Next button is hidden. |
| — | `JamGameManager` extra fields: `mainMenuName`, `useLives`, `lives`, `endScreenDelay`. Lives mode lives here (not on `JamCheckpoint`). |
| UI buttons wired to `JamGameManager` | Buttons call **`JamMenuActions`** on each screen root (`Resume`, `Restart`, `NextLevel`, `MainMenu`, `Quit`, `Play`, `LoadScene`), which forwards to the GameManager. Prefabs can't reference scene objects. |
| — | New: `JamHUD` (on `HUD`), `JamMenuActions`, `JamProjectile`, `JamAnimationEventRelay`, `JamMessageUI` (runtime hint/dialogue box). |

## Player

| Plan | Kit |
|---|---|
| Config presets "Floaty, Snappy, Heavy, TopDown, ThirdPerson" and "ThirdPerson_Default / _Platformer / _Explorer" | Asset names: `PlayerConfig_Snappy`, `PlayerConfig_Floaty`, `PlayerConfig_Heavy`, `PlayerConfig_TopDown`, `PlayerConfig_ThirdPerson_Default`, `PlayerConfig_ThirdPerson_Platformer`, `PlayerConfig_ThirdPerson_Explorer`. |
| "Copy values from Play mode" button on the config | **Not needed**: a `PlayerConfig` is an asset, so changes made in Play mode are already kept. The Inspector has a **Reset to defaults** button instead, and only shows the fields for the chosen **Kind**. |
| 3D walk speed field | Called `moveSpeed` in code, shown as **Walk Speed** in the 3D Inspector. |
| `Player_TopDown2D`: `JamShooter` (place unspecified) | `JamShooter` sits on the **`FirePoint`** child (root components stay as planned). |
| Player `FX` (dust) | `FX` also holds a `JamAudio` used by the jump sound. |
| 3D `Model` holds the Animator | As planned. `Model` also has `JamAnimationEventRelay`. Until a CC0 humanoid is added, `Model` contains `Placeholder_Body` (capsule) + `Placeholder_Visor`. |
| 2D grounding normal.y > 0.7 | **> 0.65** (slopes up to ~49° count as ground). |

## World

| Plan | Kit |
|---|---|
| World list: Hazard, Spikes, Coin, Key, Door, Goal, Checkpoint, MovingPlatform, PatrolEnemy, Spawner, TriggerZone (2D + 3D) | All present. **Extra prefabs:** `Heart_2D` / `Heart_3D` (health pickup), `Sign_2D` / `Sign_3D` (`JamDialogueTrigger`), `OneWayPlatform_2D`, `Chaser_2D`, `Turret_2D`, `Projectile_2D`, `EnemyProjectile_2D`. |
| `TriggerZone` Visual | `Visual` is empty (the zone is drawn as a purple box in the Scene view only). |
| `JamMover` waypoints | Children of the **`Waypoints`** child. The start position counts as the first point. |
| `JamGoal` | Extra: `requireAllInLevel`, `reachSound`, `lockedVisual`. `lockedMessage` supports `{have}`, `{need}`, `{item}`. |
| `JamTriggerZone` | Extra: `requireInteract` ("press E" mode), `interactPrompt`, `lockedMessage`. |
| `PatrolEnemy_3D` | Has a solid collider **and** a slightly bigger trigger collider, so it hurts a player standing still. |

## Animator

| Plan | Kit |
|---|---|
| Clips from CC0 pack | Until CC0 humanoid clips are added, states use **empty placeholder clips** `jk_Idle`, `jk_Walk`… (`Animation/3D/Placeholders`). This keeps the Animator Override Controller from Setup Mixamo Character working. |
| Death, Victory | No exit transitions (they hold until the level reloads or changes). |

## Added after the first build

| What | Detail |
|---|---|
| `JamPatrolEnemy.patrolRange` | New field (default 3): the enemy turns around after walking this far from where it started. Stops enemies walking back to the spawn point and trapping the player. 0 = turn only at walls and ledges. |
| Assemblies | Code is split into `JamKit.Runtime`, `JamKit.Editor` and `JamKit.Tests` (asmdefs). Component names are unchanged; nothing to do for users. |
| Tests | `JamKit/Tests/Editor/JamKitTests.cs`: 11 Edit Mode tests (inventory, game states, lives, goal requirement, Validate Scene on the 3 demos). Run them from Window → General → Test Runner. |
| Art | Real CC0 art (Kenney) replaces the coloured placeholders in the 2D demos. 3D stays blockout + placeholder character. See `ASSETS_TODO.md`. |
| Player Settings | Template project ships with Company "My Team", Product "My Jam Game", platform **WebGL**. `Prepare WebGL Build` asks for your own names. |
| Startup notice | With Active Input Handling = Both, Unity prints "This project uses Input Manager, which is marked for deprecation". Harmless: ignore it. |

## Editor menu

| Plan | Kit |
|---|---|
| `JamKit → Help` | **`JamKit → Help (Cheat Sheet)`**. Extra item: **`JamKit → Welcome Window`**. |
| `JamKit → Prepare WebGL Build` | Opens a small window: team / game name, **Apply WebGL settings**, **Build WebGL now…**. Compression = **Disabled**. |
| Mixamo prefab variant `Player_ThirdPerson3D_<Name>` (location unspecified) | Saved in **`_MixamoDropZone/Generated/`** with its override controller and textures, so it is git-ignored and never exported. |
| — | Maintainer-only items under **`JamKit → Internal`**: Rebuild Kit Content, Export Reference Doc, Export Package, Export Release Zip. |

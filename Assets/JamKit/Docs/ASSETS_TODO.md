# ASSETS_TODO: art and audio status

## Done (v1.0)

- **2D art:** Kenney *Pixel Platformer* (platformer) and *Tiny Dungeon* (top-down), CC0. Files in `JamKit/Art/2D/Override/`.
- **Sound effects:** Kenney *Digital Audio*, *Impact Sounds*, *Interface Sounds*, *Music Jingles*, CC0. Files in `JamKit/Audio/Override/`.
- Credits are listed in `Docs/CREDITS.txt`.

## Still placeholders (optional)

| What | Why it's still a placeholder | How to add it |
|---|---|---|
| **3D humanoid character + clips** | The good CC0 humanoid packs (e.g. Quaternius *Universal Base Characters* + *Universal Animation Library*) are only downloadable through an itch.io page. | Download them, put the character FBX/GLB in `JamKit/Art/3D/Characters/` (Rig → Humanoid) and the clips in `JamKit/Animation/3D/` (names containing Idle, Walk, Run, Jump, Fall, Land, Hurt, Death, Victory), then **JamKit → Internal → Rebuild Kit Content**. |
| **Music loops** | Kenney has jingles, not loops. | Put two CC0 loops in `JamKit/Audio/` (`mus_menu`, `mus_level`) and drag them onto the `MusicPlayer` prefab's **Clip**. |
| Parallax hills and clouds | Look fine as they are (JamKit's own, CC0). | — |

## How replacement art is picked up

**Rebuild Kit Content** uses any file in an Override folder with the same name as a placeholder:

| Folder | Names |
|---|---|
| `Art/2D/Override/*.png` | `spr_player`, `spr_enemy`, `spr_player_topdown`, `spr_enemy_topdown`, `spr_tile_ground`, `spr_tile_dirt`, `spr_tile_stone`, `spr_tile_wall`, `spr_tile_floor`, `spr_tile_bg`, `spr_grass`, `spr_platform`, `spr_lava`, `spr_turret`, `spr_diamond` (coin), `spr_key`, `spr_heart`, `spr_spike`, `spr_flag`, `spr_door` (1 wide × 2 tall), `spr_sign`, `spr_hill`, `spr_cloud`, `spr_circle` |
| `Audio/Override/*.ogg` | `sfx_jump`, `sfx_coin`, `sfx_hurt`, `sfx_stomp`, `sfx_door`, `sfx_shoot`, `sfx_checkpoint`, `sfx_win`, `sfx_lose` |

Any pixel size works: the importer sets Pixels Per Unit so one tile is still one unit. Real art keeps its own colours (placeholder tints are switched off).

**Never** put Mixamo files anywhere except `_MixamoDropZone` (git-ignored, never exported).

# JamKit v1.1 — Reference

> **Generated from the code** by `JamKit → Internal → Export Reference Doc` on 2026-10-23 20:04 with Unity 6000.3.14f1.
> This file is the source of truth for every component, field, event, menu, prefab and hierarchy name used in the slides and the cheat sheet. Do not edit by hand: regenerate it.

## Contents
1. Components (Systems, Player, World, UI)
2. Static helpers (`JamInput`, `JamInventory`)
3. `PlayerConfig` and presets
4. Input map
5. `JamKit` menu
6. Prefabs
7. Hierarchies (assets, scenes, prefab anatomy, UI)
8. Humanoid Animator (`JamHumanoid.controller`)
9. Tags, layers, renderers and other project facts

## 1. Components

Every component is found in **Add Component** by typing `Jam`. Each shows a one-line help box at the top of its Inspector (the *Help* line below).

### 1.1 Systems

| Component | What it does |
|---|---|
| `JamAudio` | Plays sound effects from any event. In an event list, drag this object in and pick JamAudio → PlayOneShot, then drop your sound in. |
| `JamCameraFollow2D` | Smoothly follows the player in 2D, looking a little ahead. Turn on 'Use Bounds' to stop the camera at the edges of your level (yellow box). |
| `JamGameManager` | The referee of the level. Knows if you are playing, paused, won or lost, shows the right screen and handles Restart / Next Level / Menu. One per level. |
| `JamMusicPlayer` | Background music that keeps playing between levels without restarting. Put one in every level; if the clip is the same it simply continues. |
| `JamSceneLoader` | Scene changes for buttons and events: LoadScene(name), LoadNext(), Reload(), Quit(). Drag this object into a Button's OnClick and pick a function. |
| `JamThirdPersonCamera` | Orbit camera for the 3D player: mouse / right stick to look, pulls in so it never goes through walls, locks the cursor while playing (Esc frees it). |

#### `JamAudio`

Plays sound effects from any event. In an event list, drag this object in and pick JamAudio → PlayOneShot, then drop your sound in.

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `volume` (Volume) | float | 0.8 | Volume of sounds played through this component (0–1). |
| `randomPitch` (Random Pitch) | float | 0.08 | Random pitch change per play, so repeated sounds don't feel robotic. 0 = always the same pitch. |

Functions you can call from any event (drag the object into an event slot, then pick): `PlayOneShot(AudioClip)`

#### `JamCameraFollow2D`

Smoothly follows the player in 2D, looking a little ahead. Turn on 'Use Bounds' to stop the camera at the edges of your level (yellow box).

Requires: `Camera` (added automatically).

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `target` (Target) | Transform | None | What to follow. Empty = the player (found automatically). |
| `smoothTime` (Smooth Time) | float | 0.15 | Seconds to catch up. Lower = tighter, higher = floatier. |
| `lookAhead` (Look Ahead) | float | 1.5 | How far (units) to look ahead in the direction the player is moving/facing. |
| `offset` (Offset) | Vector2 | (0, 1) | Offset from the target (e.g. Y = 1 shows more above the player). |
| `useBounds` (Use Bounds) | bool | off | Keep the camera inside the Bounds rectangle. |
| `bounds` (Bounds) | Rect | x -10 y -6 w 60 h 24 | Level area the camera may show (world units). Drawn as a yellow box in the Scene view. |

#### `JamGameManager`

The referee of the level. Knows if you are playing, paused, won or lost, shows the right screen and handles Restart / Next Level / Menu. One per level.

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `winScreen` (Win Screen) | GameObject | None | Shown when the player wins. Leave empty to auto-find an object called 'WinScreen'. |
| `loseScreen` (Lose Screen) | GameObject | None | Shown when the player loses. Leave empty to auto-find an object called 'LoseScreen'. |
| `pauseScreen` (Pause Screen) | GameObject | None | Shown while paused. Leave empty to auto-find an object called 'PauseMenu'. |
| `nextLevelName` (Next Level Name) | string | (empty) | Scene loaded by the Win screen's Next button. Leave blank and the Next button is hidden (the Win screen still shows). The scene must be in Build Settings. |
| `mainMenuName` (Main Menu Name) | string | "MainMenu" | Scene loaded by the Menu buttons. |
| `restartKey` (Restart Key) | KeyCode | R | Restarts the level at any time. |
| `pauseKey` (Pause Key) | KeyCode | Escape | Pauses / resumes. Esc, P and the gamepad Start button always work too. |
| `useLives` (Use Lives) | bool | off | ON: dying costs a life and respawns you at the last checkpoint. OFF: dying shows the Lose screen. |
| `lives` (Lives) | int | 3 | How many lives the player starts with (only used when 'Use Lives' is on). |
| `endScreenDelay` (End Screen Delay) | float | 0.75 | Seconds between reaching the goal (or dying) and the Win/Lose screen appearing. Lets the victory/death moment play. |

Events: `OnWin`, `OnLose`, `OnPause`, `OnResume`

Functions you can call from any event (drag the object into an event slot, then pick): `Win()`, `Lose()`, `Pause()`, `Resume()`, `TogglePause()`, `Restart()`, `NextLevel()`, `MainMenu()`, `Quit()`, `PlayerDied(JamPlayer)`

#### `JamMusicPlayer`

Background music that keeps playing between levels without restarting. Put one in every level; if the clip is the same it simply continues.

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `clip` (Clip) | AudioClip | None | The music loop to play. Leave empty for silence. |
| `volume` (Volume) | float | 0.5 | Music volume (0–1). Keep it lower than sound effects. |

Functions you can call from any event (drag the object into an event slot, then pick): `SetVolume(float)`, `Stop()`

#### `JamSceneLoader`

Scene changes for buttons and events: LoadScene(name), LoadNext(), Reload(), Quit(). Drag this object into a Button's OnClick and pick a function.

Functions you can call from any event (drag the object into an event slot, then pick): `LoadScene(string)`, `LoadNext()`, `Reload()`, `Quit()`

#### `JamThirdPersonCamera`

Orbit camera for the 3D player: mouse / right stick to look, pulls in so it never goes through walls, locks the cursor while playing (Esc frees it).

Requires: `Camera` (added automatically).

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `target` (Target) | Transform | None | What to orbit around. Empty = the player's 'CameraTarget' child (found automatically). |
| `distance` (Distance) | float | 6 | Normal distance from the target, in metres. |
| `minDistance` (Min Distance) | float | 0.8 | Closest the camera gets when pulled in by a wall. |
| `heightOffset` (Height Offset) | float | 0.4 | Extra height above the target. |
| `shoulderOffset` (Shoulder Offset) | float | 0 | Sideways offset for an over-the-shoulder view (0 = centred). |
| `sensitivity` (Sensitivity) | float | 1 | Look sensitivity. Multiplied by the player's PlayerConfig 'Look Sensitivity'. |
| `minPitch` (Min Pitch) | float | -30 | Lowest the camera can look (degrees, negative = from below). |
| `maxPitch` (Max Pitch) | float | 70 | Highest the camera can look (degrees). |
| `collisionRadius` (Collision Radius) | float | 0.25 | Radius used to detect walls between the target and the camera. |
| `followSmoothTime` (Follow Smooth Time) | float | 0.05 | Seconds to catch up with the target. 0 = locked on. |
| `lockCursor` (Lock Cursor) | bool | on | Hide and lock the mouse cursor while playing. It is released on pause, win and lose. |

### 1.2 Player

| Component | What it does |
|---|---|
| `JamHealth` | Hit points. Damage zones and enemies take these away; at zero the player loses (or loses a life) and enemies die. Works for players and enemies. |
| `JamHumanoidAnimator` | Feeds the 3D player's movement into the humanoid Animator (Speed, Grounded, VerticalVelocity, Jump, Hurt, Dead, Victory). Root motion stays off: the controller moves, the animation only shows. |
| `JamPlayerPlatformer2D` | Side-view run & jump player. All the feel (speed, jump height, gravity, double jump, wall jump) lives in the PlayerConfig asset. Swap the art on the 'Visual' child. |
| `JamPlayerThirdPerson3D` | Third-person 3D player (camera-relative: W walks away from the camera). Hold Shift to run. Swap the character on the 'Model' child; feel lives in the PlayerConfig asset. |
| `JamPlayerTopDown2D` | Top-down player: moves in 8 directions with an optional dash. Pairs with JamShooter (aims where you face) and JamChaser enemies. Feel lives in the PlayerConfig asset. |
| `JamSpriteSwapper` | Animation for artists, no Animator needed: drag your frames into the lists below (Idle, Run, Jump…). Put it on the 'Visual' child next to the SpriteRenderer. |

#### `JamHealth`

Hit points. Damage zones and enemies take these away; at zero the player loses (or loses a life) and enemies die. Works for players and enemies.

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `maxHealth` (Max Health) | int | 3 | Hit points at the start (hearts on the HUD). |
| `invincibilityTime` (Invincibility Time) | float | 1 | Seconds of invincibility after being hit (the character blinks). |
| `flashWhenHurt` (Flash When Hurt) | bool | on | Blink the character's sprites/meshes while invincible. |
| `destroyOnDeath` (Destroy On Death) | bool | off | Remove this object when it dies. Turn ON for enemies, leave OFF for the player. |
| `deathEffect` (Death Effect) | GameObject | None | Optional effect prefab spawned on death (particles, a puff…). |
| `hurtSound` (Hurt Sound) | AudioClip | None | Optional sound when hurt. |
| `deathSound` (Death Sound) | AudioClip | None | Optional sound on death. |

Events: `OnDamaged`, `OnHealed`, `OnDied`

Functions you can call from any event (drag the object into an event slot, then pick): `TakeDamage(int)`, `Heal(int)`, `Kill()`, `ResetHealth()`, `SetInvincible(float)`

#### `JamHumanoidAnimator`

Feeds the 3D player's movement into the humanoid Animator (Speed, Grounded, VerticalVelocity, Jump, Hurt, Dead, Victory). Root motion stays off: the controller moves, the animation only shows.

Requires: `JamPlayerThirdPerson3D` (added automatically).

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `animator` (Animator) | Animator | None | The Animator on the Model child. Found automatically. |
| `speedDamping` (Speed Damping) | float | 0.1 | Smoothing for the Speed parameter so the blend between idle, walk and run is soft. |
| `walkClipSpeed` (Walk Clip Speed) | float | 0 | How fast the walk clip moves its feet, in metres per second. Setup Mixamo Character fills this in so the feet don't slide. 0 = play the clip at its normal speed. |
| `runClipSpeed` (Run Clip Speed) | float | 0 | Same as Walk Clip Speed, for the run clip. |

#### `JamPlayerPlatformer2D`

Side-view run & jump player. All the feel (speed, jump height, gravity, double jump, wall jump) lives in the PlayerConfig asset. Swap the art on the 'Visual' child.

Requires: `Rigidbody2D`, `CapsuleCollider2D`, `JamHealth` (added automatically).

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `config` (Config) | PlayerConfig | None | All the feel numbers (speed, jump, gravity…). Swap presets here or edit the asset while playing. |
| `killY` (Kill Y) | float | -20 | Falling below this height costs 1 heart and respawns you at the last checkpoint. |
| `visual` (Visual) | Transform | None | The child that holds the art (SpriteRenderer / Animator). Found automatically if it is called 'Visual'. |
| `footstepInterval` (Footstep Interval) | float | 0.3 | Seconds between footstep events while running on the ground. |

Events: `OnJump`, `OnLand`, `OnDash`, `OnFootstep`, `OnHurt`, `OnDeath`

Functions you can call from any event (drag the object into an event slot, then pick): `Bounce(float)`

#### `JamPlayerThirdPerson3D`

Third-person 3D player (camera-relative: W walks away from the camera). Hold Shift to run. Swap the character on the 'Model' child; feel lives in the PlayerConfig asset.

Requires: `CharacterController`, `JamHealth` (added automatically).

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `config` (Config) | PlayerConfig | None | All the feel numbers (speed, jump, gravity…). Swap presets here or edit the asset while playing. |
| `killY` (Kill Y) | float | -20 | Falling below this height costs 1 heart and respawns you at the last checkpoint. |
| `model` (Model) | Transform | None | The child holding the character model and its Animator. Found automatically if it is called 'Model'. |
| `cameraTarget` (Camera Target) | Transform | None | Point the camera orbits around (shoulder height). Found automatically if it is called 'CameraTarget'. |
| `viewCamera` (View Camera) | Camera | None | Camera used for camera-relative movement. Empty = Main Camera. |
| `footstepDistance` (Footstep Distance) | float | 1.6 | Metres travelled between footstep events while walking (running uses a longer stride). Animation events named OnFootstep also work. |

Events: `OnJump`, `OnLand`, `OnDash`, `OnFootstep`, `OnHurt`, `OnDeath`

Functions you can call from any event (drag the object into an event slot, then pick): `AnimationFootstep()`, `Bounce(float)`

#### `JamPlayerTopDown2D`

Top-down player: moves in 8 directions with an optional dash. Pairs with JamShooter (aims where you face) and JamChaser enemies. Feel lives in the PlayerConfig asset.

Requires: `Rigidbody2D`, `CircleCollider2D`, `JamHealth` (added automatically).

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `config` (Config) | PlayerConfig | None | All the feel numbers (speed, jump, gravity…). Swap presets here or edit the asset while playing. |
| `killY` (Kill Y) | float | -20 | Falling below this height costs 1 heart and respawns you at the last checkpoint. |
| `visual` (Visual) | Transform | None | The child that holds the art (SpriteRenderer / Animator). Found automatically if it is called 'Visual'. |
| `footstepInterval` (Footstep Interval) | float | 0.35 | Seconds between footstep events while moving. |

Events: `OnJump`, `OnLand`, `OnDash`, `OnFootstep`, `OnHurt`, `OnDeath`

Functions you can call from any event (drag the object into an event slot, then pick): `Bounce(float)`

#### `JamSpriteSwapper`

Animation for artists, no Animator needed: drag your frames into the lists below (Idle, Run, Jump…). Put it on the 'Visual' child next to the SpriteRenderer.

Requires: `SpriteRenderer` (added automatically).

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `mode` (Mode) | SpriteSwapperMode | Auto | Auto picks Platformer or Top-Down from the player above it; anything else just loops 'Idle'. |
| `fps` (Fps) | float | 10 | Frames per second. |
| `idle` (Idle) | Sprite[] | None | Frames while standing still. In Loop mode these frames just loop. |
| `run` (Run) | Sprite[] | None | Frames while running. |
| `jump` (Jump) | Sprite[] | None | Frames while going up. |
| `fall` (Fall) | Sprite[] | None | Frames while falling. |
| `hurt` (Hurt) | Sprite[] | None | Frames while hurt / blinking (optional). |
| `walkDown` (Walk Down) | Sprite[] | None | Walking toward the camera / down the screen. |
| `walkUp` (Walk Up) | Sprite[] | None | Walking away / up the screen. |
| `walkSide` (Walk Side) | Sprite[] | None | Walking sideways (drawn facing right; flipped automatically for left). |

### 1.3 World

| Component | What it does |
|---|---|
| `JamChaser` | Moves toward the player when they come within range (top-down enemies, ghosts). Add a JamDamageZone so touching it hurts. |
| `JamCheckpoint` | Touch it and you respawn here after falling or losing a life. Turn on 'Use Lives' on the GameManager for lives mode. |
| `JamCollectible` | Something to pick up. Adds 'Amount' to the counter named 'Item Id' (coin, key, gem… any word). The HUD, Goal, Door and Trigger Zone can read that counter. |
| `JamDamageZone` | Hurts whatever touches it (the player by default): spikes, lava, saws, an enemy's body. Works with trigger and solid colliders, 2D and 3D. |
| `JamDialogueTrigger` | Signposts, tutorials and story: shows a text box with your lines when the player walks in (or presses E). Space / E advances. |
| `JamDoor` | A door or gate. Call Open / Close / Toggle from any event (e.g. a Trigger Zone that needs a key). When open its solid collider is switched off. |
| `JamGoal` | Touch it to win the level. Optionally locked until you have enough of an item (e.g. 10 coins); shows a hint if you arrive too early. |
| `JamMover` | Moves between waypoints: moving platforms, elevators, flying enemies. Waypoints are the children of the 'Waypoints' child; drag them in the Scene view. Carries the player. |
| `JamParallax` | Background layer that scrolls slower than the camera for depth. 0 = moves with the world, 1 = stuck to the camera (far away). Use 0.2–0.8 for layers in between. |
| `JamPatrolEnemy` | Walks back and forth, turning at walls and ledges. Jump on its head to defeat it ('Stompable'). Its body hurts through the JamDamageZone on the same object. |
| `JamProjectile` | A bullet / fireball fired by JamShooter. Flies straight, hurts what it hits, disappears on walls or after its lifetime. |
| `JamShooter` | Fires a projectile prefab, either every few seconds (turrets) or when the player presses Fire (J / Ctrl / left mouse). On the player it aims where the player faces. |
| `JamSpawner` | Spawns a prefab every few seconds, up to a maximum alive at once: enemy waves, falling rocks, collectibles. Spawn points are the children of 'SpawnPoints' (or this object). |
| `JamTimer` | A level timer shown on the HUD. Count up for speedruns, or count down: lose at zero (race the clock) or win at zero (survive 60 seconds). |
| `JamTriggerZone` | The universal glue. When the player walks in (or presses E inside), run the events below: open a door, start a platform, play a sound, spawn enemies. Can require an item, fire once, or wait. |

#### `JamChaser`

Moves toward the player when they come within range (top-down enemies, ghosts). Add a JamDamageZone so touching it hurts.

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `speed` (Speed) | float | 3 | Chase speed. |
| `detectRange` (Detect Range) | float | 6 | Starts chasing when the player is this close (orange circle). |
| `loseRange` (Lose Range) | float | 10 | Gives up when the player gets this far away. |
| `returnHome` (Return Home) | bool | on | Walk back to the start point after giving up. |

#### `JamCheckpoint`

Touch it and you respawn here after falling or losing a life. Turn on 'Use Lives' on the GameManager for lives mode.

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `activatedSprite` (Activated Sprite) | Sprite | None | Sprite shown once activated (2D). Optional. |
| `activatedColor` (Activated Color) | Color | #4CFF66FF | Tint applied to the Visual once activated (2D sprite or 3D mesh). |
| `respawnOffset` (Respawn Offset) | Vector3 | (0, 0.2, 0) | Where the player reappears, relative to this object. |
| `activateSound` (Activate Sound) | AudioClip | None | Optional sound when activated. |
| `healOnActivate` (Heal On Activate) | bool | off | Heal the player to full when activated. |

Events: `OnActivated`

#### `JamCollectible`

Something to pick up. Adds 'Amount' to the counter named 'Item Id' (coin, key, gem… any word). The HUD, Goal, Door and Trigger Zone can read that counter.

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `itemId` (Item Id) | string | "coin" | Name of the counter to add to. Use the same word in JamGoal / JamTriggerZone 'Required Item Id'. |
| `amount` (Amount) | int | 1 | How much to add. |
| `pickupSound` (Pickup Sound) | AudioClip | None | Optional sound when picked up. |
| `pickupEffect` (Pickup Effect) | GameObject | None | Optional effect prefab spawned when picked up (particles…). |
| `healAmount` (Heal Amount) | int | 0 | Hearts restored when picked up (health pickups). 0 = none. |
| `animate` (Animate) | bool | on | Gently bob (2D) or spin (3D) the 'Visual' child so it reads as a pickup. |

Events: `OnCollected`

#### `JamDamageZone`

Hurts whatever touches it (the player by default): spikes, lava, saws, an enemy's body. Works with trigger and solid colliders, 2D and 3D.

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `damage` (Damage) | int | 1 | Hearts removed per hit. |
| `instantKill` (Instant Kill) | bool | off | Kill instantly, whatever the health (bottomless pits, lava). |
| `affects` (Affects) | JamTargets | Player | Who gets hurt: the player, enemies (anything else with JamHealth), or both. |
| `knockbackForce` (Knockback Force) | float | 8 | How hard the victim is pushed away. 0 = no push. |
| `hitSound` (Hit Sound) | AudioClip | None | Optional sound when it hits. |

Events: `OnHit`

#### `JamDialogueTrigger`

Signposts, tutorials and story: shows a text box with your lines when the player walks in (or presses E). Space / E advances.

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `speakerName` (Speaker Name) | string | (empty) | Name shown above the text (leave empty for a sign). |
| `lines` (Lines) | string[] | 2 items | The lines, shown one at a time. |
| `pauseGame` (Pause Game) | bool | on | Freeze the game while the text is open. |
| `triggerOnce` (Trigger Once) | bool | on | Only show the first time. |
| `requireInteract` (Require Interact) | bool | off | Wait for the Interact key (E) instead of opening on enter. |

Events: `OnFinished`

Functions you can call from any event (drag the object into an event slot, then pick): `Show()`

#### `JamDoor`

A door or gate. Call Open / Close / Toggle from any event (e.g. a Trigger Zone that needs a key). When open its solid collider is switched off.

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `openSprite` (Open Sprite) | Sprite | None | Sprite when open (2D). If empty, the Visual is hidden when open. |
| `closedSprite` (Closed Sprite) | Sprite | None | Sprite when closed (2D). Empty = keep the current sprite. |
| `startOpen` (Start Open) | bool | off | Start the level with the door open. |
| `openSound` (Open Sound) | AudioClip | None | Sound when it opens. |
| `closeSound` (Close Sound) | AudioClip | None | Sound when it closes. |

Events: `OnOpened`, `OnClosed`

Functions you can call from any event (drag the object into an event slot, then pick): `Open()`, `Close()`, `Toggle()`

#### `JamGoal`

Touch it to win the level. Optionally locked until you have enough of an item (e.g. 10 coins); shows a hint if you arrive too early.

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `requiredItemId` (Required Item Id) | string | (empty) | Item needed to use the goal (e.g. 'coin' or 'key'). Leave empty for no requirement. |
| `requiredAmount` (Required Amount) | int | 0 | How many of that item are needed. |
| `requireAllInLevel` (Require All In Level) | bool | off | Need ALL of that item that exist in the level (counted at start). Overrides 'Required Amount'. |
| `lockedMessage` (Locked Message) | string | "Collect {need} {item} first! ({have}/{need})" | Hint shown when the player arrives without enough. {have} and {need} are replaced with numbers. |
| `reachSound` (Reach Sound) | AudioClip | None | Optional sound on reaching the goal. |
| `lockedVisual` (Locked Visual) | GameObject | None | Optional object shown while locked (e.g. a closed gate). Hidden when unlocked. |

Events: `OnReached`

#### `JamMover`

Moves between waypoints: moving platforms, elevators, flying enemies. Waypoints are the children of the 'Waypoints' child; drag them in the Scene view. Carries the player.

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `speed` (Speed) | float | 2 | Speed in units per second. |
| `mode` (Mode) | JamMoverMode | PingPong | PingPong: back and forth. Loop: last point back to the first. Once: stop at the end. |
| `waitAtPoint` (Wait At Point) | float | 0.5 | Seconds to pause at each waypoint. |
| `startOnTrigger` (Start On Trigger) | bool | off | Don't move until StartMoving() is called (from a Trigger Zone) or the player stands on it. |
| `waypointsRoot` (Waypoints Root) | Transform | None | Optional: the object holding the waypoints. Empty = a child called 'Waypoints'. |

Events: `OnArrived`

Functions you can call from any event (drag the object into an event slot, then pick): `StartMoving()`, `StopMoving()`, `ToggleMoving()`

#### `JamParallax`

Background layer that scrolls slower than the camera for depth. 0 = moves with the world, 1 = stuck to the camera (far away). Use 0.2–0.8 for layers in between.

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `parallaxFactor` (Parallax Factor) | Vector2 | (0.5, 0.3) | How much the layer follows the camera on X and Y. 0 = normal object, 1 = glued to the camera. |
| `infiniteHorizontal` (Infinite Horizontal) | bool | off | Repeat the sprite sideways forever (needs a SpriteRenderer on this object or its 'Visual'). |

#### `JamPatrolEnemy`

Walks back and forth, turning at walls and ledges. Jump on its head to defeat it ('Stompable'). Its body hurts through the JamDamageZone on the same object.

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `speed` (Speed) | float | 2 | Walking speed. |
| `turnAtLedges` (Turn At Ledges) | bool | on | Turn around before walking off a ledge. |
| `turnAtWalls` (Turn At Walls) | bool | on | Turn around when bumping into a wall. |
| `stompable` (Stompable) | bool | on | The player can defeat it by landing on top. |
| `bounceForce` (Bounce Force) | float | 14 | How high the player bounces after a stomp. |
| `startLeft` (Start Left) | bool | off | Start walking to the left instead of the right (3D: backward instead of forward). |
| `patrolRange` (Patrol Range) | float | 3 | Turn around after walking this far from the start point (units). 0 = only turn at walls and ledges. |
| `stompSound` (Stomp Sound) | AudioClip | None | Optional sound when stomped. |

Events: `OnStomped`

#### `JamProjectile`

A bullet / fireball fired by JamShooter. Flies straight, hurts what it hits, disappears on walls or after its lifetime.

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `damage` (Damage) | int | 1 | Hearts removed on hit. |
| `affects` (Affects) | JamTargets | Enemies | Who it can hurt. Player bullets: Enemies. Enemy bullets: Player. |
| `lifetime` (Lifetime) | float | 3 | Seconds before it disappears on its own. |
| `knockbackForce` (Knockback Force) | float | 4 | Push applied to what it hits. |
| `hitEffect` (Hit Effect) | GameObject | None | Optional effect spawned on impact. |

#### `JamShooter`

Fires a projectile prefab, either every few seconds (turrets) or when the player presses Fire (J / Ctrl / left mouse). On the player it aims where the player faces.

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `projectile` (Projectile) | GameObject | None | The projectile prefab to fire (should have JamProjectile on it). |
| `interval` (Interval) | float | 1 | Seconds between shots (automatic), or the minimum time between shots (on input). |
| `direction` (Direction) | Vector2 | (0, 0) | Fire direction. (0,0) = where the player faces, or this object's right (2D) / forward (3D). |
| `fireOnInput` (Fire On Input) | bool | off | Fire when the player presses the Fire button instead of automatically. |
| `aimAtPlayer` (Aim At Player) | bool | off | Turrets: aim at the player instead of using Direction. |
| `projectileSpeed` (Projectile Speed) | float | 10 | Projectile speed. |
| `firePoint` (Fire Point) | Transform | None | Where shots come from. Empty = a child called 'FirePoint', or this object. |
| `range` (Range) | float | 0 | Only shoot when the player is closer than this (0 = always). |
| `fireSound` (Fire Sound) | AudioClip | None | Optional sound per shot. |

Events: `OnFire`

Functions you can call from any event (drag the object into an event slot, then pick): `Fire()`

#### `JamSpawner`

Spawns a prefab every few seconds, up to a maximum alive at once: enemy waves, falling rocks, collectibles. Spawn points are the children of 'SpawnPoints' (or this object).

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `prefab` (Prefab) | GameObject | None | What to spawn. |
| `interval` (Interval) | float | 3 | Seconds between spawns. |
| `maxAlive` (Max Alive) | int | 5 | Never more than this many alive at once. |
| `totalToSpawn` (Total To Spawn) | int | 0 | Total to spawn before stopping. 0 = forever. |
| `spawnOnStart` (Spawn On Start) | bool | on | Start spawning when the level starts. Off = wait for StartSpawning() (e.g. from a Trigger Zone). |
| `randomRadius` (Random Radius) | float | 0 | Random offset around each spawn point (units). |

Events: `OnSpawn`

Functions you can call from any event (drag the object into an event slot, then pick): `StartSpawning()`, `StopSpawning()`, `SpawnOne()`

#### `JamTimer`

A level timer shown on the HUD. Count up for speedruns, or count down: lose at zero (race the clock) or win at zero (survive 60 seconds).

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `mode` (Mode) | JamTimerMode | CountDown | Count up from 0, or down from 'Seconds'. |
| `seconds` (Seconds) | float | 60 | Starting time for a countdown (seconds). |
| `onZero` (On Zero) | JamTimerEnd | Lose | What happens when a countdown reaches zero. |
| `startOnAwake` (Start On Awake) | bool | on | Start ticking when the level starts. Off = wait for StartTimer(). |

Events: `OnTimerEnd`

Functions you can call from any event (drag the object into an event slot, then pick): `StartTimer()`, `StopTimer()`, `ResetTimer()`, `AddTime(float)`

#### `JamTriggerZone`

The universal glue. When the player walks in (or presses E inside), run the events below: open a door, start a platform, play a sound, spawn enemies. Can require an item, fire once, or wait.

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `onlyPlayer` (Only Player) | bool | on | Only react to the player (ignore enemies, projectiles…). |
| `triggerOnce` (Trigger Once) | bool | off | Fire OnEnter only the first time. |
| `requireInteract` (Require Interact) | bool | off | Wait for the Interact key (E / gamepad North) while inside, instead of firing on enter. |
| `interactPrompt` (Interact Prompt) | string | "Press E" | Hint shown when the player is inside and must press Interact. |
| `requiredItemId` (Required Item Id) | string | (empty) | Item the player must have (e.g. 'key'). Leave empty for none. |
| `requiredAmount` (Required Amount) | int | 1 | How many of that item. |
| `consumeItem` (Consume Item) | bool | off | Use up the item when the zone fires (keys get used). |
| `lockedMessage` (Locked Message) | string | "You need a {item}." | Hint shown when the requirement isn't met. {need} and {item} are replaced. |
| `delay` (Delay) | float | 0 | Seconds to wait before OnEnter fires. |

Events: `OnEnter`, `OnExit`

Functions you can call from any event (drag the object into an event slot, then pick): `ForceFire()`

### 1.4 UI

| Component | What it does |
|---|---|
| `JamHUD` | Shows hearts (or a health bar), a counter for any item id (coins by default) and the level timer. Parts hide themselves when the level doesn't use them. |
| `JamMenuActions` | Button actions for the UI canvas: Resume, Restart, Next Level, Main Menu, Quit, Load Scene. Already wired on the kit's buttons. |

#### `JamHUD`

Shows hearts (or a health bar), a counter for any item id (coins by default) and the level timer. Parts hide themselves when the level doesn't use them.

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `hearts` (Hearts) | Transform | None | Parent of the heart icons. Its first child is used as the heart template. |
| `useBar` (Use Bar) | bool | off | Show a bar instead of hearts. |
| `healthBar` (Health Bar) | Image | None | Filled Image used as the health bar (when 'Use Bar' is on). |
| `heartFull` (Heart Full) | Color | #FF404CFF | Colour of a full heart. |
| `heartEmpty` (Heart Empty) | Color | #FFFFFF40 | Colour of an empty heart. |
| `coinCounter` (Coin Counter) | GameObject | None | Root of the counter (icon + text). Hidden if the level has none of this item. |
| `coinText` (Coin Text) | TMP_Text | None | The counter's text. |
| `itemId` (Item Id) | string | "coin" | Which counter to show (same word as the collectibles' Item Id). |
| `showTotal` (Show Total) | bool | on | Show 'have / total in level' instead of just 'have'. |
| `timer` (Timer) | GameObject | None | Root of the timer. Hidden if the level has no JamTimer. |
| `timerText` (Timer Text) | TMP_Text | None | The timer's text. |

#### `JamMenuActions`

Button actions for the UI canvas: Resume, Restart, Next Level, Main Menu, Quit, Load Scene. Already wired on the kit's buttons.

| Field (Inspector label) | Type | Default | Tooltip |
|---|---|---|---|
| `firstLevelName` (First Level Name) | string | (empty) | Scene loaded by the Play button on a main menu. Leave blank to load the next scene in Build Settings. |

Functions you can call from any event (drag the object into an event slot, then pick): `Play()`, `Resume()`, `Restart()`, `NextLevel()`, `MainMenu()`, `Quit()`, `LoadScene(string)`

### 1.5 Support components (added automatically, nothing to set)

| Component | What it does |
|---|---|
| `JamAnimationEventRelay` | Sits next to the 3D Animator (on `Model`) and forwards animation events named `OnFootstep` / `Footstep` / `FootL` / `FootR` to the player's `OnFootstep`. |
| `JamMessageUI` | Created at runtime. Shows hint messages (locked goal, locked door, "Press E") and dialogue boxes. |

## 2. Static helpers (no component to add)

| Helper | What it does |
|---|---|
| `JamInput` | The one place input is read. Works with the Input System and legacy Input. See §4. |
| `JamInventory` | Counters by item id (`coin`, `key`, any word). Filled by `JamCollectible`, read by `JamHUD`, `JamGoal`, `JamTriggerZone`. Resets when a level loads. Event: `OnChanged(itemId, count)`. |

## 3. `PlayerConfig` and presets

A `PlayerConfig` asset holds all the feel numbers of a player. Drag one into the player's **Config** field to swap presets. Changes made in Play mode are kept (it's an asset). Create your own: right-click in the Project window → **Create → JamKit → Player Config**. The Inspector only shows the settings for the chosen **Kind**.

| Field | Type | Tooltip |
|---|---|---|
| `kind` (Kind) | PlayerConfigKind | Which controller this config is for. The Inspector only shows the settings that matter for it. |
| `moveSpeed` (Move Speed) | float | Top speed in units per second (2D) or metres per second when walking (3D). |
| `acceleration` (Acceleration) | float | How fast you reach top speed. Higher = snappier. |
| `deceleration` (Deceleration) | float | How fast you stop when you let go. Higher = less sliding. |
| `airControl` (Air Control) | float | How much control you have in the air (0 = none, 1 = same as on the ground). |
| `jumpHeight` (Jump Height) | float | How high a full jump goes, in units/metres. Easier to reason about than a force. |
| `maxJumps` (Max Jumps) | int | Total jumps before landing. 1 = normal, 2 = double jump, 0 = no jumping. |
| `coyoteTime` (Coyote Time) | float | Grace time (seconds) to still jump just after walking off a ledge. |
| `jumpBuffer` (Jump Buffer) | float | If you press jump this many seconds before landing, the jump still happens. |
| `canDash` (Can Dash) | bool | Allow a quick dash on Left Shift / gamepad West button (2D controllers). |
| `dashSpeed` (Dash Speed) | float | Dash speed in units per second. |
| `dashDuration` (Dash Duration) | float | How long a dash lasts (seconds). |
| `dashCooldown` (Dash Cooldown) | float | Seconds before you can dash again. |
| `dashInvincible` (Dash Invincible) | bool | Can't be hurt while dashing. |
| `hurtControlLock` (Hurt Control Lock) | float | Seconds your controls are locked after being hit. |
| `gravityScale` (Gravity Scale) | float | Gravity multiplier while rising. Higher = faster, punchier jumps. |
| `fallGravityMultiplier` (Fall Gravity Multiplier) | float | Extra gravity while falling. Above 1 makes jumps feel less floaty. |
| `maxFallSpeed` (Max Fall Speed) | float | Maximum falling speed. |
| `jumpCutMultiplier` (Jump Cut Multiplier) | float | Letting go of jump early multiplies upward speed by this (short hop). 1 = no variable height. |
| `jumpWithUp` (Jump With Up) | bool | W and Up arrow also jump (on top of Space). |
| `wallSlide` (Wall Slide) | bool | Slide slowly down walls while pushing into them. |
| `wallSlideSpeed` (Wall Slide Speed) | float | Speed when sliding down a wall. |
| `wallJump` (Wall Jump) | bool | Jump off walls while sliding. |
| `wallJumpVelocity` (Wall Jump Velocity) | Vector2 | Wall-jump push: X = away from wall, Y = up. |
| `facing` (Facing) | TopDownFacing | How the character faces: 4 directions, 8 directions, or rotate the Visual toward movement. |
| `runSpeed` (Run Speed) | float | Speed while holding Sprint (Left Shift). 'Move Speed' is the walking speed. |
| `turnSmoothTime` (Turn Smooth Time) | float | Seconds to turn toward the movement direction. Lower = snappier turning. |
| `gravity` (Gravity) | float | Gravity in m/s² (negative). Earth is -9.81; games usually use more. |
| `terminalVelocity` (Terminal Velocity) | float | Fastest the character can fall. |
| `lookSensitivity` (Look Sensitivity) | float | Camera mouse / right-stick sensitivity. |
| `invertLookY` (Invert Look Y) | bool | Flip the camera's up/down look. |

### Presets (in `JamKit/Configs`)

| Preset | `kind` | `moveSpeed` | `runSpeed` | `acceleration` | `deceleration` | `airControl` | `jumpHeight` | `maxJumps` | `gravityScale` | `fallGravityMultiplier` | `canDash` | `wallJump` | `gravity` |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `PlayerConfig_Floaty` | Platformer2D | 7 | 6.5 | 45 | 45 | 0.85 | 3.6 | 1 | 2.2 | 1.2 | on | on | -25 |
| `PlayerConfig_Heavy` | Platformer2D | 6 | 6.5 | 30 | 50 | 0.4 | 4 | 1 | 5 | 2 | off | off | -25 |
| `PlayerConfig_Snappy` | Platformer2D | 8 | 6.5 | 70 | 80 | 0.65 | 3.2 | 1 | 3.5 | 1.6 | off | off | -25 |
| `PlayerConfig_TopDown` | TopDown2D | 6 | 6.5 | 60 | 70 | 1 | 3.2 | 0 | 3.5 | 1.6 | on | off | -25 |
| `PlayerConfig_ThirdPerson_Default` | ThirdPerson3D | 3 | 6.5 | 20 | 25 | 0.6 | 1.4 | 1 | 3.5 | 1.6 | off | off | -25 |
| `PlayerConfig_ThirdPerson_Explorer` | ThirdPerson3D | 2.2 | 5 | 20 | 25 | 0.4 | 1 | 1 | 3.5 | 1.6 | off | off | -25 |
| `PlayerConfig_ThirdPerson_Platformer` | ThirdPerson3D | 4 | 7.5 | 20 | 25 | 0.9 | 2.2 | 2 | 3.5 | 1.6 | off | off | -25 |

**The 5 values to tweak first**

| Controller | Values |
|---|---|
| Platformer 2D | `moveSpeed`, `jumpHeight`, `gravityScale`, `fallGravityMultiplier`, `maxJumps` |
| Top-Down 2D | `moveSpeed`, `acceleration`, `canDash`, `dashSpeed`, `facing` |
| Third Person 3D | `moveSpeed` (walk), `runSpeed`, `jumpHeight`, `turnSmoothTime`, `lookSensitivity` |

## 4. Input map (`JamInput`)

| Action | Keyboard / mouse | Gamepad | Used by |
|---|---|---|---|
| Move | WASD / Arrows | Left stick / D-pad | all |
| Jump | Space (W / Up also jump in the 2D platformer) | South (A / ✕) | Platformer2D, ThirdPerson3D |
| Sprint / Dash | Left Shift | West (X / □) or Right trigger | all (dash optional per config; 3D: hold to run) |
| Interact | E | North (Y / △) | `JamTriggerZone` / `JamDialogueTrigger` with *Require Interact* |
| Fire | J, Left Ctrl or Left mouse | East (B / ○) | `JamShooter` with *Fire On Input* |
| Look / Camera | Mouse | Right stick | ThirdPerson3D |
| Pause | Esc or P | Start | `JamGameManager` |
| Restart | R | Select | `JamGameManager` |
| Advance dialogue | Space, E, Fire or Enter | South / North / East | `JamDialogueTrigger` |

## 5. `JamKit` menu

| Menu item | What it does |
|---|---|
| `JamKit → New Level → 2D Platformer` | Copies the matching template scene into `Assets/_Game/Scenes`, names it, opens it, adds it to Build Settings and sets the camera renderer (0 = 2D, 1 = 3D). |
| `JamKit → New Level → 2D Top-Down` | Copies the matching template scene into `Assets/_Game/Scenes`, names it, opens it, adds it to Build Settings and sets the camera renderer (0 = 2D, 1 = 3D). |
| `JamKit → New Level → 3D Third-Person` | Copies the matching template scene into `Assets/_Game/Scenes`, names it, opens it, adds it to Build Settings and sets the camera renderer (0 = 2D, 1 = 3D). |
| `JamKit → 3D → Setup Mixamo Character` | Turns the FBX files in `JamKit/_MixamoDropZone` into a playable 3D player: humanoid rig, URP materials, loop settings, override controller, prefab variant, ✅/❌ report. |
| `JamKit → 3D → Fit Controller To Model` | Sizes the 3D player's CharacterController (and CameraTarget) to the meshes under `Model`. |
| `JamKit → Validate Scene` | ✅/❌ checklist: one Player, one GameManager, a Goal or win Timer, camera and renderer, HUD, EventSystem, scene in Build Settings, MainMenu first. Fix buttons where possible. |
| `JamKit → Prepare WebGL Build` | Switches to WebGL, compression Disabled, 960×540, asks for team and game name, can build straight away. |
| `JamKit → Help (Cheat Sheet)` | Opens `JamKit/Docs/JamKit_CheatSheet.pdf` (falls back to `Docs/README.md`). |
| `JamKit → Welcome Window` | Reopens the welcome window (shown automatically the first time the project opens). |
| `JamKit → Internal → Rebuild Kit Content` | Kit maintainers only: regenerates every JamKit sprite, sound, tile, prefab, config and scene from code. |
| `JamKit → Internal → Export Reference Doc` | Kit maintainers only: regenerates this file. |
| `JamKit → Internal → Export Package` | Kit maintainers only: exports `JamKit_v1.1.unitypackage` (no Mixamo files, no Internal tools). |
| `JamKit → Internal → Export Release Zip` | Kit maintainers only: zips the template project as `JamKit_Template_v1.1.zip` (no Library/Temp/Logs, no Mixamo files). |

## 6. Prefabs (`JamKit/Prefabs`)

World prefabs come in `_2D` and `_3D` variants with the same components; only the collider differs. Every World prefab follows **Root / Visual / FX**: never edit the root, swap the art on `Visual`, particles live in `FX`.

### Player

| Prefab | Root components | Children |
|---|---|---|
| `Player_Platformer2D` | `Rigidbody2D`, `CapsuleCollider2D`, `JamHealth`, `JamPlayerPlatformer2D` | `Visual`, `GroundCheckGizmo`, `FX` |
| `Player_ThirdPerson3D` | `CharacterController`, `JamHealth`, `JamPlayerThirdPerson3D`, `JamHumanoidAnimator` | `Model`, `CameraTarget`, `FX` |
| `Player_TopDown2D` | `Rigidbody2D`, `CircleCollider2D`, `JamHealth`, `JamPlayerTopDown2D` | `Visual`, `FirePoint`, `FX` |

### World

| Prefab | Root components | Children |
|---|---|---|
| `Chaser_2D` | `Rigidbody2D`, `CircleCollider2D`, `JamChaser`, `JamDamageZone`, `JamHealth` | `Visual`, `FX` |
| `Checkpoint_2D` | `BoxCollider2D`, `JamCheckpoint` | `Visual`, `FX` |
| `Checkpoint_3D` | `BoxCollider`, `JamCheckpoint` | `Visual`, `FX` |
| `Coin_2D` | `CircleCollider2D`, `JamCollectible` | `Visual`, `FX` |
| `Coin_3D` | `SphereCollider`, `JamCollectible` | `Visual`, `FX` |
| `Door_2D` | `BoxCollider2D`, `JamDoor` | `Visual`, `FX` |
| `Door_3D` | `BoxCollider`, `JamDoor` | `Visual`, `FX` |
| `EnemyProjectile_2D` | `Rigidbody2D`, `CircleCollider2D`, `JamProjectile` | `Visual`, `FX` |
| `Goal_2D` | `BoxCollider2D`, `JamGoal` | `Visual`, `FX` |
| `Goal_3D` | `BoxCollider`, `JamGoal` | `Visual`, `FX` |
| `Hazard_2D` | `BoxCollider2D`, `JamDamageZone` | `Visual`, `FX` |
| `Hazard_3D` | `BoxCollider`, `JamDamageZone` | `Visual`, `FX` |
| `Heart_2D` | `CircleCollider2D`, `JamCollectible` | `Visual`, `FX` |
| `Heart_3D` | `SphereCollider`, `JamCollectible` | `Visual`, `FX` |
| `Key_2D` | `CircleCollider2D`, `JamCollectible` | `Visual`, `FX` |
| `Key_3D` | `SphereCollider`, `JamCollectible` | `Visual`, `FX` |
| `MovingPlatform_2D` | `Rigidbody2D`, `BoxCollider2D`, `JamMover` | `Visual`, `Waypoints`, `FX` |
| `MovingPlatform_3D` | `Rigidbody`, `BoxCollider`, `JamMover` | `Visual`, `Waypoints`, `FX` |
| `OneWayPlatform_2D` | `BoxCollider2D`, `PlatformEffector2D` | `Visual`, `FX` |
| `PatrolEnemy_2D` | `Rigidbody2D`, `BoxCollider2D`, `JamPatrolEnemy`, `JamDamageZone`, `JamHealth` | `Visual`, `FX` |
| `PatrolEnemy_3D` | `Rigidbody`, `BoxCollider`, `BoxCollider`, `JamPatrolEnemy`, `JamDamageZone`, `JamHealth` | `Visual`, `FX` |
| `Projectile_2D` | `Rigidbody2D`, `CircleCollider2D`, `JamProjectile` | `Visual`, `FX` |
| `Sign_2D` | `BoxCollider2D`, `JamDialogueTrigger` | `Visual`, `FX` |
| `Sign_3D` | `BoxCollider`, `JamDialogueTrigger` | `Visual`, `FX` |
| `Spawner_2D` | `JamSpawner` | `Visual`, `SpawnPoints`, `FX` |
| `Spawner_3D` | `JamSpawner` | `Visual`, `SpawnPoints`, `FX` |
| `Spikes_2D` | `BoxCollider2D`, `JamDamageZone` | `Visual`, `FX` |
| `Spikes_3D` | `BoxCollider`, `JamDamageZone` | `Visual`, `FX` |
| `TriggerZone_2D` | `BoxCollider2D`, `JamTriggerZone` | `Visual`, `FX` |
| `TriggerZone_3D` | `BoxCollider`, `JamTriggerZone` | `Visual`, `FX` |
| `Turret_2D` | `BoxCollider2D`, `JamShooter` | `Visual`, `FX` |

### Systems

| Prefab | Root components | Children |
|---|---|---|
| `Camera2D` | `Camera`, `AudioListener`, `UniversalAdditionalCameraData`, `JamCameraFollow2D` |  |
| `GameManager` | `JamGameManager` |  |
| `MusicPlayer` | `AudioSource`, `JamMusicPlayer` |  |
| `ThirdPersonCamera` | `Camera`, `AudioListener`, `UniversalAdditionalCameraData`, `JamThirdPersonCamera` |  |

### UI

| Prefab | Root components | Children |
|---|---|---|
| `HUD` | `JamHUD` | `Hearts`, `HealthBar`, `CoinCounter`, `Timer` |
| `LoseScreen` | `JamMenuActions` | `Panel`, `Message`, `Retry`, `Menu` |
| `MainMenu` | `Canvas`, `CanvasScaler`, `GraphicRaycaster`, `JamMenuActions` | `Background`, `Title`, `Play`, `Quit`, `Credits` |
| `PauseMenu` | `JamMenuActions` | `Panel`, `Title`, `Resume`, `Restart`, `MainMenu` |
| `UI_Canvas` | `Canvas`, `CanvasScaler`, `GraphicRaycaster` | `HUD`, `PauseMenu`, `WinScreen`, `LoseScreen` |
| `WinScreen` | `JamMenuActions` | `Panel`, `Message`, `Next`, `Retry`, `Menu` |

### Pickup defaults (`JamCollectible` on each prefab)

| Prefab | `itemId` | `amount` | `healAmount` |
|---|---|---|---|
| `Coin_2D` | "coin" | 1 | 0 |
| `Coin_3D` | "coin" | 1 | 0 |
| `Heart_2D` | (empty) | 1 | 1 |
| `Heart_3D` | (empty) | 1 | 1 |
| `Key_2D` | "key" | 1 | 0 |
| `Key_3D` | "key" | 1 | 0 |

## 7. Hierarchies

### 7.1 Asset hierarchy (Project window)

```
Assets/
├── _Game/            (yours: your art, audio, prefab variants, levels)
│   ├── Art/
│   ├── Audio/
│   ├── Prefabs/
│   └── Scenes/
└── JamKit/            (the kit: use it, don't edit it)
    ├── _MixamoDropZone/  (empty: drop Mixamo FBX here; never published)
    │   └── Generated/
    │       └── Textures/
    ├── Animation/
    │   ├── 2D/
    │   └── 3D/
    │       └── Placeholders/
    ├── Art/
    │   ├── 2D/
    │   │   └── Override/
    │   ├── 3D/
    │   │   ├── Characters/
    │   │   └── Materials/
    │   └── UI/
    ├── Audio/
    │   └── Override/
    ├── Configs/          (PlayerConfig presets: the feel)
    │   └── Physics/
    ├── Docs/             (cheat sheet, this reference)
    │   └── Screenshots/
    ├── Prefabs/          (things you drag into a level)
    │   ├── Player/
    │   ├── Systems/
    │   ├── UI/
    │   └── World/
    ├── Scenes/           (menu, templates, demos)
    │   └── Demo/
    │       └── Checkpoints/
    ├── Scripts/
    │   ├── Editor/
    │   │   └── Internal/
    │   └── Runtime/
    │       ├── Core/
    │       ├── Player/
    │       ├── Systems/
    │       ├── UI/
    │       └── World/
    ├── Tests/
    │   └── Editor/
    └── Tilemaps/         (tiles + palette, ready to paint)
        └── Tiles/
```

`_Game/` is yours (green on the slides). `JamKit/` is the kit (grey): use it, don't edit it. `_MixamoDropZone/` is empty in the repo and git-ignored.

### 7.2 Scene hierarchy

**`Level_Template_Platformer2D`**

```
Level_Template_Platformer2D
├── ---- SYSTEMS ----
│   ├── GameManager  [prefab]
│   ├── MusicPlayer  [prefab]
│   └── EventSystem  (EventSystem)
├── ---- CAMERA ----
│   └── Main Camera  [prefab]
├── ---- PLAYER ----
│   ├── Player  [prefab]
│   └── SpawnPoint
├── ---- LEVEL ----
│   ├── Grid  (Grid)
│   │   ├── Tilemap_Ground  (Tilemap, TilemapRenderer, Rigidbody2D, TilemapCollider2D, CompositeCollider2D)
│   │   ├── Tilemap_Decor  (Tilemap, TilemapRenderer)
│   │   └── Tilemap_Background  (Tilemap, TilemapRenderer)
│   ├── Hazards
│   ├── Collectibles
│   ├── Enemies
│   ├── Platforms
│   ├── Triggers & Doors
│   └── Goal
│       └── Goal_2D  [prefab]
├── ---- BACKGROUND ----
└── ---- UI ----
    └── UI_Canvas  [prefab]
        ├── HUD  [prefab]
        ├── PauseMenu  [prefab]
        ├── WinScreen  [prefab]
        └── LoseScreen  [prefab]
```

**`Level_Template_TopDown2D`**

```
Level_Template_TopDown2D
├── ---- SYSTEMS ----
│   ├── GameManager  [prefab]
│   ├── MusicPlayer  [prefab]
│   └── EventSystem  (EventSystem)
├── ---- CAMERA ----
│   └── Main Camera  [prefab]
├── ---- PLAYER ----
│   ├── Player  [prefab]
│   └── SpawnPoint
├── ---- LEVEL ----
│   ├── Grid  (Grid)
│   │   ├── Tilemap_Ground  (Tilemap, TilemapRenderer, Rigidbody2D, TilemapCollider2D, CompositeCollider2D)
│   │   ├── Tilemap_Decor  (Tilemap, TilemapRenderer)
│   │   └── Tilemap_Background  (Tilemap, TilemapRenderer)
│   ├── Hazards
│   ├── Collectibles
│   ├── Enemies
│   ├── Platforms
│   ├── Triggers & Doors
│   └── Goal
│       └── Goal_2D  [prefab]
├── ---- BACKGROUND ----
└── ---- UI ----
    └── UI_Canvas  [prefab]
        ├── HUD  [prefab]
        ├── PauseMenu  [prefab]
        ├── WinScreen  [prefab]
        └── LoseScreen  [prefab]
```

**`Level_Template_ThirdPerson3D`**

```
Level_Template_ThirdPerson3D
├── ---- SYSTEMS ----
│   ├── GameManager  [prefab]
│   ├── MusicPlayer  [prefab]
│   └── EventSystem  (EventSystem)
├── ---- CAMERA ----
│   └── Main Camera  [prefab]
├── ---- PLAYER ----
│   ├── Player  [prefab]
│   └── SpawnPoint
├── ---- LEVEL ----
│   ├── Environment
│   │   ├── Floor
│   │   │   └── Ground  (MeshFilter, BoxCollider, MeshRenderer)
│   │   ├── Walls
│   │   │   ├── Wall_N  (MeshFilter, BoxCollider, MeshRenderer)
│   │   │   ├── Wall_S  (MeshFilter, BoxCollider, MeshRenderer)
│   │   │   ├── Wall_E  (MeshFilter, BoxCollider, MeshRenderer)
│   │   │   └── Wall_W  (MeshFilter, BoxCollider, MeshRenderer)
│   │   ├── Props
│   │   └── Lighting
│   │       ├── Directional Light  (Light)
│   │       └── Volume  (Volume)
│   ├── Hazards
│   ├── Collectibles
│   ├── Enemies
│   ├── Platforms
│   ├── Triggers & Doors
│   └── Goal
│       └── Goal_3D  [prefab]
├── ---- BACKGROUND ----
│   └── Skybox_Note (set the skybox in Window > Rendering > Lighting)
└── ---- UI ----
    └── UI_Canvas  [prefab]
        ├── HUD  [prefab]
        ├── PauseMenu  [prefab]
        ├── WinScreen  [prefab]
        └── LoseScreen  [prefab]
```

### 7.3 Prefab anatomy

```
Player_Platformer2D  (Rigidbody2D, CapsuleCollider2D, JamHealth, JamPlayerPlatformer2D)
├── Visual  (SpriteRenderer, JamSpriteSwapper)
├── GroundCheckGizmo
└── FX  (JamAudio)
    ├── JumpDust  (ParticleSystem, ParticleSystemRenderer)
    └── LandDust  (ParticleSystem, ParticleSystemRenderer)
```

```
Player_TopDown2D  (Rigidbody2D, CircleCollider2D, JamHealth, JamPlayerTopDown2D)
├── Visual  (SpriteRenderer, JamSpriteSwapper)
├── FirePoint  (JamShooter)
└── FX  (JamAudio)
    ├── Dust  (ParticleSystem, ParticleSystemRenderer)
    └── HurtFlash  (ParticleSystem, ParticleSystemRenderer)
```

```
Player_ThirdPerson3D  (CharacterController, JamHealth, JamPlayerThirdPerson3D, JamHumanoidAnimator)
├── Model  (Animator, JamAnimationEventRelay)
│   ├── Placeholder_Body  (MeshFilter, MeshRenderer)
│   └── Placeholder_Visor  (MeshFilter, MeshRenderer)
├── CameraTarget
└── FX  (JamAudio)
    ├── FootstepDust  (ParticleSystem, ParticleSystemRenderer)
    └── LandPuff  (ParticleSystem, ParticleSystemRenderer)
```

```
Coin_2D  (CircleCollider2D, JamCollectible)
├── Visual  (SpriteRenderer)
└── FX
    └── Sparkle  (ParticleSystem, ParticleSystemRenderer)
```

```
Coin_3D  (SphereCollider, JamCollectible)
├── Visual
│   └── Disc  (MeshFilter, MeshRenderer)
└── FX
    └── Sparkle  (ParticleSystem, ParticleSystemRenderer)
```

```
MovingPlatform_2D  (Rigidbody2D, BoxCollider2D, JamMover)
├── Visual  (SpriteRenderer)
├── Waypoints
│   └── Point1
└── FX
```

```
MovingPlatform_3D  (Rigidbody, BoxCollider, JamMover)
├── Visual
│   └── Platform  (MeshFilter, MeshRenderer)
├── Waypoints
│   └── Point1
└── FX
```

```
PatrolEnemy_2D  (Rigidbody2D, BoxCollider2D, JamPatrolEnemy, JamDamageZone, JamHealth)
├── Visual  (SpriteRenderer)
└── FX
    └── Poof  (ParticleSystem, ParticleSystemRenderer)
```

```
UI_Canvas  (Canvas, CanvasScaler, GraphicRaycaster)
├── HUD  [prefab]  (JamHUD)
│   ├── Hearts  (HorizontalLayoutGroup)
│   │   └── Heart  (CanvasRenderer, Image)
│   ├── HealthBar  (CanvasRenderer, Image)
│   ├── CoinCounter
│   │   ├── Icon  (CanvasRenderer, Image)
│   │   └── Text  (CanvasRenderer, TextMeshProUGUI)
│   └── Timer  (CanvasRenderer, TextMeshProUGUI)
├── PauseMenu  [prefab]  (JamMenuActions)
│   ├── Panel  (CanvasRenderer, Image)
│   ├── Title  (CanvasRenderer, TextMeshProUGUI)
│   ├── Resume  (CanvasRenderer, Image, Button)
│   │   └── Text  (CanvasRenderer, TextMeshProUGUI)
│   ├── Restart  (CanvasRenderer, Image, Button)
│   │   └── Text  (CanvasRenderer, TextMeshProUGUI)
│   └── MainMenu  (CanvasRenderer, Image, Button)
│       └── Text  (CanvasRenderer, TextMeshProUGUI)
├── WinScreen  [prefab]  (JamMenuActions)
│   ├── Panel  (CanvasRenderer, Image)
│   ├── Message  (CanvasRenderer, TextMeshProUGUI)
│   ├── Next  (CanvasRenderer, Image, Button)
│   │   └── Text  (CanvasRenderer, TextMeshProUGUI)
│   ├── Retry  (CanvasRenderer, Image, Button)
│   │   └── Text  (CanvasRenderer, TextMeshProUGUI)
│   └── Menu  (CanvasRenderer, Image, Button)
│       └── Text  (CanvasRenderer, TextMeshProUGUI)
└── LoseScreen  [prefab]  (JamMenuActions)
    ├── Panel  (CanvasRenderer, Image)
    ├── Message  (CanvasRenderer, TextMeshProUGUI)
    ├── Retry  (CanvasRenderer, Image, Button)
    │   └── Text  (CanvasRenderer, TextMeshProUGUI)
    └── Menu  (CanvasRenderer, Image, Button)
        └── Text  (CanvasRenderer, TextMeshProUGUI)
```

```
MainMenu  (Canvas, CanvasScaler, GraphicRaycaster, JamMenuActions)
├── Background  (CanvasRenderer, Image)
├── Title  (CanvasRenderer, TextMeshProUGUI)
├── Play  (CanvasRenderer, Image, Button)
│   └── Text  (CanvasRenderer, TextMeshProUGUI)
├── Quit  (CanvasRenderer, Image, Button)
│   └── Text  (CanvasRenderer, TextMeshProUGUI)
└── Credits  (CanvasRenderer, TextMeshProUGUI)
```

Rule: **never edit the root; only swap or restyle `Visual` (2D) or `Model` (3D).**

## 8. Humanoid Animator (`JamKit/Animation/3D/JamHumanoid.controller`)

Root motion is **off**: the controller moves the character, animation is visual only. Any humanoid (the kit's CC0 character, Mixamo Y Bot, your own) uses the same controller; `Setup Mixamo Character` makes an Animator Override Controller from it.

| Parameter | Type | Set by |
|---|---|---|
| `Speed` | Float | 0–1 walk, 1–2 run (`JamHumanoidAnimator`) |
| `Grounded` | Bool | on the ground |
| `VerticalVelocity` | Float | up/down speed |
| `Jump` | Trigger | player jumps |
| `Hurt` | Trigger | player takes damage |
| `Dead` | Bool | player dies |
| `Victory` | Trigger | `JamGameManager` win |

```
Base Layer
├── Locomotion  [Blend Tree 1D on Speed: jk_Idle 0 → jk_Walk 1 → jk_Run 2]  ← from Land after exit time 0.3; from Hurt after exit time 0.8
├── Jump  [jk_Jump]  ← from Locomotion when Jump
├── Fall  [jk_Fall]  ← from Locomotion when !Grounded && VerticalVelocity < -1; from Jump when VerticalVelocity < -1
├── Land  [jk_Land]  ← from Jump when Grounded && VerticalVelocity < 0.1; from Fall when Grounded
├── Hurt  [jk_Hurt]  ← Any State when Hurt && !Dead
├── Death  [jk_Death]  ← Any State when Dead
└── Victory  [jk_Victory]  ← Any State when Victory
```

Clips named `jk_*` are empty placeholders until CC0 humanoid clips are added to `JamKit/Animation/3D` (see `Docs/ASSETS_TODO.md`); `Rebuild Kit Content` picks them up by name (Idle, Walk, Run, Jump, Fall, Land, Hurt, Death, Victory).

## 9. Project facts

- **Unity version:** `6000.3.14f1` (Unity 6.3 LTS).
- **Render pipeline:** URP, one pipeline asset with two renderers: **index 0 = 2D Renderer** (2D scenes), **index 1 = Universal Renderer** (3D scene). Each camera picks its renderer.
- **Tags and layers:** none required. The player is found by component (`JamPlayer`); 2D ground detection uses contact normals.
- **Scene section headers** (`---- SYSTEMS ----` …) are plain empty GameObjects. They are **not** tagged EditorOnly, because EditorOnly objects are stripped from builds together with their children.
- **Active Input Handling:** Both (Input System + legacy). JamKit works with either.
- **Dependencies:** Unity built-in packages only (URP, 2D Tilemap, Input System, uGUI/TextMeshPro).
- **Build Settings order:** `MainMenu` → `Demo_2D_Platformer` → `Demo_2D_TopDown` → `Demo_3D_ThirdPerson`
- **Scenes shipped (`JamKit/Scenes`):** `Scenes/Demo/Checkpoints/Demo_01_Empty.unity`, `Scenes/Demo/Checkpoints/Demo_02_Player.unity`, `Scenes/Demo/Checkpoints/Demo_03_Hazards.unity`, `Scenes/Demo/Checkpoints/Demo_04_Glue.unity`, `Scenes/Demo/Demo_2D_Platformer.unity`, `Scenes/Demo/Demo_2D_TopDown.unity`, `Scenes/Demo/Demo_3D_ThirdPerson.unity`, `Scenes/Level_Template_Platformer2D.unity`, `Scenes/Level_Template_ThirdPerson3D.unity`, `Scenes/Level_Template_TopDown2D.unity`, `Scenes/MainMenu.unity`. The `Demo/Checkpoints` scenes are the live-build states to fall back to during the workshop.
- **Tile palette:** `JamKit_Palette` (`Tilemaps/JamKit_Palette.prefab`): open it from Window → 2D → Tile Palette.
- **Live-demo only (never shipped):** `_MixamoDropZone/Generated/Demo_3D_ThirdPerson_YBot` (the 3D demo with the Mixamo Y Bot, made by Setup Mixamo Character).
- **Sprite placeholders:** 32 px per unit, Point filter, no compression; characters pivot bottom-centre, everything else centre.


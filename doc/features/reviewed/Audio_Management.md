# Feature: Audio Management System (`IAudioService`)

## 📖 Overview

The Audio Management System is the centralised sound controller for IsoTactics. It is responsible for playing and managing all audio in the game: **background music (BGM)**, **ambient environment loops (AMB)**, and **sound effects (SFX)** including weapon foley, voice barks, and UI sounds.

The system is exposed as an `IAudioService` interface, registered as a singleton in the DI container via `GameBootstrapper`, and injected into any entity or service that needs to produce sound. Its core design principles are:

- **Smart pooling** — reuses `AudioStreamPlayer` / `AudioStreamPlayer2D` nodes rather than instantiating and freeing them per-sound.
- **Music crossfading** — transitions seamlessly between music tracks without jarring cuts.
- **Sound culling / ducking** — prevents audio overload when many entities trigger sounds simultaneously (e.g., 6 enemies running at once → max 2 footstep sounds played).
- **Bus-based volume control** — integrates with Godot's Audio Bus layout so `ISettingsService` can apply per-category volume preferences (Master, Music, SFX) without touching individual players.

> **Current Status:** ✅ **Partially Implemented (Prototype Complete)**. Core `IAudioService` contract is implemented in `AudioService.cs` and registered as a singleton in `GameBootstrapper.cs`. It handles the 6-bus layout (`Master`, `Music`, `SFX`, `Voice`, `Ambient`, `UI`), 2D positional SFX pooling, UI audio playback, background music playlist cycling with feature toggle bypass (`EnableMusic`), and options screen volume slider wiring. Advanced features (interactive/layered stems, dialogue bark ducking) remain scoped for later production phases.

---

## 🎯 Goals & Use Cases

- **Goal 1 — Atmospheric Immersion:** Play moody, context-sensitive music that shifts between exploration and combat. Ambient loops (rain, city traffic, warehouse hum) react to the current level's `WeatherPreset`.
- **Goal 2 — Responsive SFX:** Every action (gunshot, footstep, door kick, grenade bounce) produces the correct audio cue without any hard-coding inside entity scripts.
- **Goal 3 — Audio Sanity:** Prevent frame drops and audio clipping caused by dozens of entities triggering identical sounds on the same frame.
- **Goal 4 — Dialogue Barks:** Characters speak contextual voice lines (selection, move order, hit, hurt, trait trigger) without overlapping each other.
- **Goal 5 — Settings Integration:** Volume sliders in the Settings menu apply instantly by adjusting Godot Audio Bus levels — no audio restarts required.

### Use Cases

| Scenario | Expected Behaviour |
|---|---|
| Player enters a level | `IWeatherService` calls `IAudioService.PlayAmbient("AmbRainHeavy")` → ambient rain loop begins |
| First gunshot is fired | Game transitions from exploration BGM to combat BGM via a crossfade |
| All enemies eliminated | Combat BGM crossfades back to exploration BGM after a short delay |
| 8 enemies move simultaneously | Only 2 footstep sounds play per frame (culling active) |
| Character is selected | `BarkPlayer` on that character plays a selection voice line |
| Lightning strike (Weather) | `IWeatherService` calls `IAudioService.PlaySFX("SfxThunderRumble")` with randomised delay |
| Player adjusts Music volume | `ISettingsService` calls `IAudioService.SetBusVolume(AudioBus.Music, value)` |
| Scene is unloaded | All active SFX stop cleanly; BGM and ambient persist across scene transitions |

---

## 👥 Target Audience / User Roles

- **Game entities** (`IsoCharacter`, `IsoAnimal`) — play positional SFX and voice barks via `IAudioService`.
- **Game services** (`IWeatherService`, `IGameStateService`, `IActionQueueService`) — trigger music transitions and ambient crossfades.
- **Settings UI** (`ISettingsService`) — adjusts Godot Audio Bus volumes based on player preferences.
- **Designers** — author `AudioData` resources (`SfxPistolFire.wav`, `BgmCombatLoop.ogg`) without touching code.

---

## ⚙️ Functional Requirements

### Music (BGM)

- [ ] **REQ-BGM-01:** The service must support playing a looping BGM track on the dedicated `Music` audio bus.
- [ ] **REQ-BGM-02:** Music transitions must crossfade smoothly over a configurable duration (default: 1.5 s) rather than cutting abruptly.
- [ ] **REQ-BGM-03:** The service must expose a `PlayMusic(string trackKey, float fadeDuration)` method.
- [ ] **REQ-BGM-04:** The service must expose a `StopMusic(float fadeDuration)` method that fades out the current track.
- [ ] **REQ-BGM-05:** The system must maintain two `AudioStreamPlayer` nodes (A/B crossfade pattern) for seamless track switching.
- [ ] **REQ-BGM-06:** Music must persist across level loads (scene changes should not restart the current BGM unless explicitly requested).

### Ambient Loops (AMB)

- [ ] **REQ-AMB-01:** The service must support playing a looping ambient sound on the `Ambient` audio bus.
- [ ] **REQ-AMB-02:** Ambient crossfading must work identically to BGM crossfading (A/B players, configurable duration).
- [ ] **REQ-AMB-03:** The `IWeatherService` must be able to call `IAudioService.PlayAmbient(string ambientKey, float fadeDuration)` on level load.
- [ ] **REQ-AMB-04:** A `StopAmbient(float fadeDuration)` method must exist for interior zones with no ambient loop.

### Sound Effects (SFX)

- [ ] **REQ-SFX-01:** The service must expose a `PlaySFX(string sfxKey)` method for non-positional sounds (UI clicks, thunder).
- [ ] **REQ-SFX-02:** The service must expose a `PlaySFX2D(string sfxKey, Vector2 worldPosition)` method for positional sounds (gunshots, footsteps).
- [ ] **REQ-SFX-03:** The service must maintain a pool of `AudioStreamPlayer2D` nodes; sounds are checked out from the pool on play and returned when finished.
- [ ] **REQ-SFX-04:** The pool must have a configurable maximum size (default: 16 nodes).
- [ ] **REQ-SFX-05:** Culling must be implemented: if the same `sfxKey` is requested more than `MaxSimultaneousInstances` times within a single frame, excess requests are silently dropped. Default: `MaxSimultaneousInstances = 2`.
- [ ] **REQ-SFX-06:** Ducking must reduce the volume of lower-priority sounds when high-priority sounds (e.g., a close explosion) play.
- [ ] **REQ-SFX-07:** Pitch Modulation must be applied to repeating sounds (like sound effects) to create variation without requiring extra sound assets. The system must support configuring Pitch Modulation per audio category (e.g., Foley, Weapon, Voice), allowing each category to toggle modulation on/off and define its specific modulation range (e.g., +/- 10%).

### Voice Barks

- [ ] **REQ-BRK-01:** The service must expose a `PlayBark(string barkKey, Node2D sourceEntity)` method that plays from the entity's `BarkPlayer` node.
- [ ] **REQ-BRK-02:** Only one bark may play per character at a time (new bark requests on the same entity are queued or discarded if the current one has not finished).
- [ ] **REQ-BRK-03:** Bark subtitle text must be emitted alongside the audio so the UI can display it (see `DialogueBark.SubtitleText`).

### Volume & Settings

- [ ] **REQ-VOL-01:** The service must expose `SetBusVolume(AudioBus bus, float linearVolume)` to allow `ISettingsService` to adjust per-category volumes.
- [ ] **REQ-VOL-02:** Supported buses: `Master`, `Music`, `Ambient`, `SFX`, `Voice`.
- [ ] **REQ-VOL-03:** Volume values must be stored as linear floats (0.0–1.0) and converted to dB internally via `GD.LinearToDb`.
- [ ] **REQ-VOL-04:** The options menu must allow players to adjust the volume for each bus on a scale from 0 to 10 (which maps to the internal 0.0–1.0 linear floats).

### ⚙️ Adaptive Music — Tension-Driven Layering *(Optional / Stretch Goal)*

> [!NOTE]
> This feature is **optional**. The base system (hard crossfade between `BgmExploration` and `BgmCombat`) is sufficient for the prototype. Adaptive music should only be implemented once the core `IAudioService` is stable.

Adaptive music (also called **vertical re-mixing**) continuously shifts the music's intensity based on in-game tension level, rather than making a hard cut between two tracks. The result is a seamless musical gradient — barely noticeable on the edges, dramatic in full combat.

#### How Tension Works

A single float, `TensionLevel` (0.0–1.0), drives the music system:

| Tension Range | Music State | Typical Trigger |
|---|---|---|
| `0.0 – 0.3` | **Calm** | No enemies in awareness range, post-combat cool-down |
| `0.3 – 0.6` | **Tense** | 1–2 enemies alert or suspicious, player in danger zone |
| `0.6 – 0.85` | **Elevated** | Active combat with few enemies, player taking fire |
| `0.85 – 1.0` | **Intense** | Many enemies active, explosion nearby, character downed |

#### Vertical Re-Mixing (Layered Tracks)

Instead of two separate full tracks, each music "mood" is composed of **stacked layers** that are all played simultaneously but at different volumes:

```
BgmExploration_Base.ogg      ← Always audible (piano, bass)
BgmExploration_Tension.ogg   ← Fades in at TensionLevel > 0.3 (strings, low percussion)
BgmExploration_Intense.ogg   ← Fades in at TensionLevel > 0.6 (full drums, brass stabs)
```

All three files are the **same length and in perfect sync** (exported from the same DAW session). The `IAudioService` plays all three at once but controls each layer's volume based on `TensionLevel`:

```
Layer volume = Clamp((TensionLevel - layerThreshold) / layerRange, 0, 1)
```

#### Tension Calculation

`IGameStateService` (or a new lightweight `ITensionService`) continuously calculates `TensionLevel` from game state data:

| Factor | Contribution |
|---|---|
| Number of enemies in `Alert` state | `+0.15` per enemy, max `+0.6` |
| Number of enemies in `Suspicious` state | `+0.08` per enemy, max `+0.3` |
| Active gunfire in last 3 seconds | `+0.25` |
| A character is `Downed` | `+0.2` |
| Player character at < 30% HP | `+0.15` |
| No enemies in range for > 5 seconds | Gradual decay towards `0.0` |

The calculated value is smoothed with a lerp (`TensionDecaySpeed`, `TensionRiseSpeed`) to avoid jarring jumps, then pushed to `IAudioService`:

```csharp
// Called each frame by IGameStateService or ITensionService:
_audioService.SetTensionLevel(currentTension);
```

#### New `IAudioService` Methods (Adaptive Extension)

```csharp
/// <summary>
/// Sets the current tension level (0.0 = calm, 1.0 = maximum intensity).
/// The audio service uses this to blend music layer volumes continuously.
/// </summary>
void SetTensionLevel(float tensionLevel);

/// <summary>
/// Registers a layered music set for adaptive playback.
/// All layers must be identical in length and BPM.
/// </summary>
void PlayAdaptiveMusic(string baseKey, string tensionKey, string intenseKey,
                       float layerOneTreshold = 0.3f, float layerTwoThreshold = 0.6f);
```

#### Node Structure Extension (Adaptive Mode)

When `AdaptiveMusicEnabled` feature flag is on, the `SysAudioManager` spawns additional players per layer:

```
SysAudioManager (Node)
  ├── AdaptiveMusic (Node)
  │   ├── LayerBase_A    (AudioStreamPlayer)   ← Always playing — bus: Music
  │   ├── LayerBase_B    (AudioStreamPlayer)   ← A/B crossfade backup
  │   ├── LayerTension_A (AudioStreamPlayer)   ← bus: Music — volume driven by TensionLevel
  │   ├── LayerTension_B (AudioStreamPlayer)
  │   ├── LayerIntense_A (AudioStreamPlayer)   ← bus: Music — volume driven by TensionLevel
  │   └── LayerIntense_B (AudioStreamPlayer)
  └── ...
```

#### Feature Flag Gating

The adaptive system is guarded by `IFeatureService`:

```csharp
if (_featureService.IsEnabled("AdaptiveMusicEnabled"))
    this.PlayAdaptiveMusic("BgmExploration_Base", "BgmExploration_Tension", "BgmExploration_Intense");
else
    this.PlayMusic("BgmExploration");
```

This allows the feature to be toggled off in the prototype without touching any audio logic.

#### Adaptive Music Asset Requirements

Each music mood requires three synced `.ogg` exports from the same DAW session:

| Mood | Base Layer | Tension Layer | Intense Layer |
|---|---|---|---|
| Exploration | `BgmExploration_Base.ogg` | `BgmExploration_Tension.ogg` | `BgmExploration_Intense.ogg` |
| Combat | `BgmCombat_Base.ogg` | `BgmCombat_Tension.ogg` | `BgmCombat_Intense.ogg` |

> [!TIP]
> Composing adaptive music requires all layers to be rendered at identical tempo and duration from the DAW. Loops must be seamless (no click at the loop point). Consider composing in 8- or 16-bar phrases to make looping natural.

---



## 💻 Technical Details

### Architecture

The `IAudioService` is a **singleton Godot Node** (not a pure C# class) because it needs engine ticks to update crossfade tweens and manage the audio player pool. It is registered in `GameBootstrapper` and injected into consumers via the `ServiceLocator`.

```
Scripts/Core/Contracts/IAudioService.cs       ← Interface definition
Scripts/Core/Implementations/AudioService.cs  ← Node-based singleton implementation
Scenes/SysAudioManager.tscn                   ← Scene wrapper for the AudioService Node (AutoLoad)
```

### Godot Audio Bus Layout

```
Master
  ├── Music      (BGM tracks)
  ├── Ambient    (environmental loops)
  ├── SFX        (all positional and non-positional effects)
  │   └── Voice  (character barks — child of SFX for unified ducking)
```

### Node Structure (`SysAudioManager.tscn`)

```
SysAudioManager (Node)
  ├── MusicPlayerA (AudioStreamPlayer)    ← Crossfade A leg — bus: Music
  ├── MusicPlayerB (AudioStreamPlayer)    ← Crossfade B leg — bus: Music
  ├── AmbientPlayerA (AudioStreamPlayer)  ← Crossfade A leg — bus: Ambient
  ├── AmbientPlayerB (AudioStreamPlayer)  ← Crossfade B leg — bus: Ambient
  └── SFXPool (Node)                      ← Parent for pooled AudioStreamPlayer2D nodes
      ├── SfxPlayer_00 (AudioStreamPlayer2D)  ← bus: SFX
      ├── SfxPlayer_01 (AudioStreamPlayer2D)
      └── ... (up to MaxPoolSize = 16)
```

### `IAudioService` Contract

```csharp
namespace Scripts.Core.Contracts
{
    /// <summary>
    /// Centralised audio manager. Handles BGM crossfading, ambient loops,
    /// SFX pooling, sound culling, and voice bark playback.
    /// </summary>
    public interface IAudioService
    {
        // ── Music ──────────────────────────────────────────────────────────
        /// <summary>Crossfades to the specified BGM track.</summary>
        void PlayMusic(string trackKey, float fadeDuration = 1.5f);

        /// <summary>Fades out the currently playing BGM.</summary>
        void StopMusic(float fadeDuration = 1.5f);

        // ── Ambient ────────────────────────────────────────────────────────
        /// <summary>Crossfades to the specified ambient loop.</summary>
        void PlayAmbient(string ambientKey, float fadeDuration = 1.0f);

        /// <summary>Fades out the ambient loop (e.g. entering an interior).</summary>
        void StopAmbient(float fadeDuration = 1.0f);

        // ── Sound Effects ──────────────────────────────────────────────────
        /// <summary>Plays a non-positional sound effect (UI, thunder, etc.).</summary>
        void PlaySFX(string sfxKey);

        /// <summary>Plays a positional sound effect at the given world position.</summary>
        void PlaySFX2D(string sfxKey, Vector2 worldPosition);

        // ── Voice Barks ────────────────────────────────────────────────────
        /// <summary>
        /// Plays a character bark from the entity's BarkPlayer node.
        /// Emits the subtitle text via signal for the HUD to display.
        /// </summary>
        void PlayBark(string barkKey, Node2D sourceEntity);

        // ── Volume ─────────────────────────────────────────────────────────
        /// <summary>Sets the volume of a named audio bus (0.0–1.0 linear).</summary>
        void SetBusVolume(string busName, float linearVolume);
    }
}
```

### Key Data Objects

#### `AudioInstance` *(already defined in [`43 Game Objects.md`](file:///d:/Data_efilt/OneDrive/12%20Areas/Ideas/IsoTactics/doc/43 Game Objects.md))*

Pooled audio player managed by `IAudioService`.

| Property | Type | Description |
|---|---|---|
| `Category` | `enum` | `Weapon`, `Foley`, `Voice`, `Music`, `Ambient` |
| `Stream` | `AudioStream` | The audio clip to play |
| `CullingPriority` | `int` | Higher value = less likely to be culled when pool is full |

#### `DialogueBark` *(already defined in [`43 Game Objects.md`](file:///d:/Data_efilt/OneDrive/12%20Areas/Ideas/IsoTactics/doc/43 Game Objects.md))*

| Property | Type | Description |
|---|---|---|
| `Character` | `Character` | The character who speaks the line |
| `Trigger` | `enum` | `Selection`, `MoveOrder`, `Miss`, `Hit`, `Hurt`, `TraitTrigger` |
| `AudioClip` | `AudioStream` | The voice clip to play |
| `SubtitleText` | `string` | On-screen text for the bark |

### Crossfade Implementation (A/B Pattern)

Music and ambient crossfading uses two `AudioStreamPlayer` nodes per category:

```
Step 1: PlayerA is currently playing TrackX
Step 2: PlayMusic("TrackY") is called
Step 3: PlayerB.Stream = TrackY; PlayerB.Play(); PlayerB.VolumeDb = -80 (silent)
Step 4: Tween: PlayerA.VolumeDb → -80, PlayerB.VolumeDb → 0  (over fadeDuration)
Step 5: PlayerA.Stop(); swap active/inactive labels
```

### Sound Culling

Each `sfxKey` maps to a `CullBucket` that tracks how many instances played in the current frame. A bucket count check happens before any pool checkout:

```csharp
if (_cullBuckets[sfxKey] >= _maxSimultaneous[sfxKey])
    return; // Drop the request silently
_cullBuckets[sfxKey]++;
// ... play sound
```

`_cullBuckets` is reset to zero at the start of each `_Process` frame.

### DI Registration

In [`GameBootstrapper.cs`](file:///d:/Data_efilt/OneDrive/12%20Areas/Ideas/IsoTactics/src/Scripts/Core/GameBootstrapper.cs), uncomment:

```csharp
// MEDIA & POLISH
services.AddSingleton<IAudioService, AudioService>();
```

### Usage by Consumers

Entities resolve `IAudioService` from the DI container after the fix in `IsoEntity.cs`:

```csharp
// In IsoEntity.cs — planned injection (currently commented out):
protected IAudioService Audio { get; private set; }
// Audio = ServiceLocator.Provider.GetRequiredService<IAudioService>();
```

Services use it directly:

```csharp
// IWeatherService — on level load:
_audioService.PlayAmbient("AmbRainHeavy");

// IWeatherService — lightning strike:
_audioService.PlaySFX("SfxThunderRumble");

// IActionQueueService — during combat sequence:
_audioService.PlaySFX2D("SfxPistolFire", character.GlobalPosition);
```

---

## 🔊 Audio Asset Catalogue

All assets follow naming conventions defined in [`93 Resource Naming.md`](file:///d:/Data_efilt/OneDrive/12%20Areas/Ideas/IsoTactics/doc/93 Resource Naming.md).

---

### 🎵 Music Tracks (`Bgm` prefix, `.ogg`)

#### Standard Tracks (Base System)

| Key | File | Screen / Trigger | Mood & Instrumentation |
|---|---|---|---|
| `main_theme` | `BgmMainTheme.ogg` | Splash screen, Main Menu | Dark, cinematic. Slow piano motif over bass synth pads. Sets the crime-noir tone immediately. |
| `hq_idle` | `BgmHqIdle.ogg` | Headquarters / Safehouse | Low-key, slow jazz-influenced. Sparse bass, distant trumpet. Feels lived-in and weary. |
| `mission_briefing` | `BgmMissionBriefing.ogg` | Intel Board / Pre-Mission Prep | Tense, building. Strings with no resolution. Creates anticipation without excitement. |
| `exploration` | `BgmExploration.ogg` | Default in-level (no threat) | Moody, bass-heavy. Slow percussion, synth atmosphere. Dark urban feel. |
| `combat` | `BgmCombat.ogg` | Active combat (fallback, non-adaptive) | High-tempo, hard-hitting drums. Aggressive brass stabs. Feels chaotic and dangerous. |
| `stealth` | `BgmStealth.ogg` | All enemies unaware, player moving carefully | Minimal pulse. Low heartbeat-like rhythm, silence-heavy. Sparse high synth notes. |
| `mission_success` | `BgmMissionSuccess.ogg` | After-Action Report — success | Short triumphant sting (~8 s). Resolves the main theme motif. Bitter-sweet, not celebratory. |
| `mission_fail` | `BgmMissionFail.ogg` | Game Over / Mission Failed | Short bleak sting (~6 s). Single low note decay. No melody. |
| `loading` | `BgmLoading.ogg` | Loading screen | Ambient drone. Slow evolution. Should loop cleanly if load takes long. |

#### Adaptive Layers (Optional — `AdaptiveMusicEnabled` Feature Flag)

Each of the two core in-level moods (`exploration`, `combat`) ships as three synced layer files, all identical in duration and BPM. Exported from the same DAW session.

| Mood | Layer | File | Instruments | Activates at Tension |
|---|---|---|---|---|
| Exploration | Base | `BgmExploration_Base.ogg` | Piano, bass, slow synth pad | Always |
| Exploration | Tension | `BgmExploration_Tension.ogg` | Low strings, sparse hi-hat | `> 0.30` |
| Exploration | Intense | `BgmExploration_Intense.ogg` | Full drum kit, brass stabs | `> 0.60` |
| Combat | Base | `BgmCombat_Base.ogg` | Driving bass, rhythm guitar | Always (in combat state) |
| Combat | Tension | `BgmCombat_Tension.ogg` | Aggressive strings, snare rolls | `> 0.50` |
| Combat | Intense | `BgmCombat_Intense.ogg` | Full brass section, war drums | `> 0.80` |
| Stealth | Base | `BgmStealth_Base.ogg` | Heartbeat pulse, silence | Always (in stealth state) |
| Stealth | Tension | `BgmStealth_Tension.ogg` | Low drone, single piano note | `> 0.35` |
| Stealth | Intense | `BgmStealth_Intense.ogg` | Dissonant strings, rapid pulse | `> 0.65` |

> [!TIP]
> Total adaptive music asset count: **9 `.ogg` files**. Keep files at a consistent loop length (e.g. 64 bars at 90 BPM ≈ 2:50) so the loop point is always musically coherent.

---

### 🌧️ Ambient Loops (`Amb` prefix, `.ogg`)

All ambient loops must be seamlessly loopable (no audible click or gap at the loop boundary). Crossfaded by `IWeatherService` on level load.

| Key | File | Context / Weather Preset | Notes |
|---|---|---|---|
| `rain_heavy` | `AmbRainHeavy.ogg` | `HeavyRain`, `Thunderstorm`, `DownpourHeadlights` | Full downpour. Includes rain on concrete, metal drainpipe, and puddle splashes. |
| `rain_light` | `AmbRainLight.ogg` | `Drizzle`, `FogDrizzle` | Gentle patter. Softer, more distant. |
| `wind_strong` | `AmbWindStrong.ogg` | `OvercastWind`, `HeavyRainWind` | Howling gusts with metal creak. |
| `wind_light` | `AmbWindLight.ogg` | `ClearNight`, `PreDawnQuiet` | Subtle breeze. Nearly subliminal. |
| `city_day` | `AmbCityDay.ogg` | Outdoor levels, daytime | Distant traffic, car horns, pigeons, construction hum. |
| `city_night` | `AmbCityNight.ogg` | `ClearNight`, `CityLightsNight` | Quieter traffic, distant music from a bar, occasional siren. |
| `city_night_rain` | `AmbCityNightRain.ogg` | Levels 07, 09, 12 | City night atmosphere blended with rain. Cars passing through puddles. |
| `warehouse_hum` | `AmbWarehouseHum.ogg` | Interior warehouse levels | HVAC hum, distant forklift, metal settling. |
| `office_interior` | `AmbOfficeInterior.ogg` | Office / corporate interior levels | Ventilation, distant phone, fluorescent light buzz. |
| `bar_ambience` | `AmbBarAmbience.ogg` | Bar / restaurant interior levels | Muffled crowd murmur, glasses clinking, distant music. |
| `smoky_interior` | `AmbSmokyInterior.ogg` | `SmokyInterior` preset (Level 08) | Dense echo, distant machinery, eerie silence-heavy. |
| `strobe_blackout` | `AmbStrobeBlackout.ogg` | `StrobeBlackout` preset (Level 14) | Electrical hum, broken generator rhythm. Very sparse. |
| `golden_hour` | `AmbGoldenHour.ogg` | `GoldenHour` preset (Level 15) | Distant birds, warm breeze, slight traffic. |
| `pre_dawn` | `AmbPreDawn.ogg` | `PreDawnQuiet` preset (Level 10) | Near silence. Distant dog bark, faint wind. |
| `room_tone` | `AmbRoomTone.ogg` | Sealed interiors / no-weather zones | Neutral room tone (~-60 dB). Prevents dead silence. |

---

### 💥 Sound Effects (`Sfx` prefix, `.wav`)

#### Weapons — Fire

| Key | File | Culling Max | Trigger |
|---|---|---|---|
| `pistol_fire` | `SfxPistolFire.wav` | 4 | 9mm Handgun / .45 Suppressed (standard) |
| `pistol_fire_suppressed` | `SfxPistolFireSuppressed.wav` | 4 | Suppressed pistol — quieter crack, less reverb tail |
| `revolver_fire` | `SfxRevolverFire.wav` | 3 | Heavy Revolver — louder, deeper boom |
| `shotgun_fire` | `SfxShotgunFire.wav` | 3 | Pump-Action Shotgun |
| `shotgun_sawnoff_fire` | `SfxShotgunSawnOffFire.wav` | 3 | Sawed-Off — shorter blast, wider spray feel |
| `smg_fire` | `SfxSmgFire.wav` | 4 | Submachine Gun — rapid stutter |
| `rifle_fire` | `SfxRifleFire.wav` | 4 | Assault Rifle — single shot |
| `rifle_burst` | `SfxRifleBurst.wav` | 3 | Assault Rifle — burst mode (3-round) |
| `sniper_fire` | `SfxSniperFire.wav` | 2 | Bolt-Action Sniper — extreme crack + echo tail |
| `bullet_whiz` | `SfxBulletWhiz.wav` | 3 | Near miss — plays at receiving entity's position |

#### Weapons — Reload & Handling

| Key | File | Culling Max | Notes |
|---|---|---|---|
| `pistol_reload` | `SfxPistolReload.wav` | 2 | Mag drop + insert + slide rack |
| `revolver_reload` | `SfxRevolverReload.wav` | 2 | Cylinder swing open + shells drop + close |
| `shotgun_pump` | `SfxShotgunPump.wav` | 2 | Shell eject pump |
| `shotgun_reload` | `SfxShotgunReload.wav` | 2 | Shell-by-shell tube reload |
| `smg_reload` | `SfxSmgReload.wav` | 2 | Mag drop + click in |
| `rifle_reload` | `SfxRifleReload.wav` | 2 | Mag drop + insert + charging handle |
| `sniper_bolt` | `SfxSniperBolt.wav` | 1 | Bolt cycle after each shot |
| `sniper_reload` | `SfxSniperReload.wav` | 1 | Magazine swap |
| `weapon_empty_click` | `SfxWeaponEmptyClick.wav` | 2 | Dry-fire on empty magazine |

#### Weapons — Throwables & Explosives

| Key | File | Culling Max | Notes |
|---|---|---|---|
| `grenade_pin` | `SfxGrenadePin.wav` | 1 | Pin pull before throw |
| `grenade_bounce` | `SfxGrenadeBounce.wav` | 2 | Metallic bounce off concrete |
| `explosion_grenade` | `SfxExplosionGrenade.wav` | 2 | Frag grenade blast — sharp crack + debris |
| `explosion_large` | `SfxExplosionLarge.wav` | 1 | Breaching charge / Molotov secondary — deeper boom |
| `flashbang_throw` | `SfxFlashbangThrow.wav` | 1 | Light rattle in air |
| `flashbang_detonate` | `SfxFlashbangDetonate.wav` | 1 | Blinding pop + high-frequency ring |
| `molotov_throw` | `SfxMolotovThrow.wav` | 1 | Woosh |
| `molotov_smash` | `SfxMolotovSmash.wav` | 1 | Glass shatter + ignition whoosh |
| `fire_burn` | `SfxFireBurn.wav` | 2 | Molotov fire loop (looping SFX, played on area) |
| `smoke_deploy` | `SfxSmokeDeploy.wav` | 1 | Hiss as canister vents |

#### Foley — Movement

| Key | File | Culling Max | Surface |
|---|---|---|---|
| `footstep_concrete` | `SfxFootstepConcrete.wav` | 2 | Outdoor pavement, warehouse floor |
| `footstep_metal` | `SfxFootstepMetal.wav` | 2 | Fire escapes, scaffolding, grating |
| `footstep_wood` | `SfxFootstepWood.wav` | 2 | Crates, floorboards |
| `footstep_gravel` | `SfxFootstepGravel.wav` | 2 | Back alleys, rooftop pebble |
| `footstep_puddle` | `SfxFootstepPuddle.wav` | 2 | Wet outdoor surfaces |
| `footstep_carpet` | `SfxFootstepCarpet.wav` | 2 | Office interiors |
| `sprint_effort` | `SfxSprintEffort.wav` | 1 | Brief exhale when sprint action starts |
| `vault_rustle` | `SfxVaultRustle.wav` | 1 | Cloth/gear rustle when vaulting |
| `crouch_slide` | `SfxCrouchSlide.wav` | 1 | Snapping into crouch / cover |
| `land_heavy` | `SfxLandHeavy.wav` | 1 | Landing from height (climb-down) |

#### Foley — Environment Interactions

| Key | File | Culling Max | Trigger |
|---|---|---|---|
| `door_open` | `SfxDoorOpen.wav` | 2 | Standard door opening |
| `door_close` | `SfxDoorClose.wav` | 2 | Standard door closing |
| `door_kick` | `SfxDoorKick.wav` | 1 | Violent breach — wood splinter + bang |
| `door_locked` | `SfxDoorLocked.wav` | 1 | Rattle on locked door |
| `lock_pick` | `SfxLockPick.wav` | 1 | Lock-pick manipulation loop |
| `hack_beep` | `SfxHackBeep.wav` | 1 | Terminal interaction progress |
| `hack_success` | `SfxHackSuccess.wav` | 1 | Electronic unlock confirmation |
| `loot_pickup` | `SfxLootPickup.wav` | 2 | Item retrieved from container |
| `loot_container_open` | `SfxLootContainerOpen.wav` | 1 | Crate lid / safe door |
| `glass_break` | `SfxGlassBreak.wav` | 2 | Window / bottle smash |
| `cover_destroy_wood` | `SfxCoverDestroyWood.wav` | 1 | Wooden crate splinter from bullet/blast |
| `cover_destroy_concrete` | `SfxCoverDestroyConc.wav` | 1 | Debris crumble |
| `ricochet` | `SfxRicochet.wav` | 3 | Bullet glancing off metal/concrete |
| `bullet_impact_concrete` | `SfxImpactConcrete.wav` | 3 | Bullet hits wall/floor |
| `bullet_impact_metal` | `SfxImpactMetal.wav` | 3 | Bullet hits car/container |
| `bullet_impact_wood` | `SfxImpactWood.wav` | 3 | Bullet hits crate/door |
| `revive_use` | `SfxReviveUse.wav` | 1 | Trauma kit deploy sound |

#### Ambient One-Shots

| Key | File | Culling Max | Notes |
|---|---|---|---|
| `thunder_rumble` | `SfxThunderRumble.wav` | 1 | Triggered by `IWeatherService` with randomised delay after lightning flash |
| `thunder_crack` | `SfxThunderCrack.wav` | 1 | Close lightning strike — sharp crack variant |
| `siren_pass` | `SfxSirenPass.wav` | 1 | Police/ambulance doppler pass — atmosphere |
| `dog_bark_distant` | `SfxDogBarkDistant.wav` | 1 | Distant dog reacting to noise |
| `cat_hiss` | `SfxCatHiss.wav` | 1 | Stray cat startled |
| `rat_scurry` | `SfxRatScurry.wav` | 2 | Rat disturbed in alley |

#### UI Sounds

| Key | File | Culling Max | Notes |
|---|---|---|---|
| `ui_click` | `SfxUiClick.wav` | 4 | Button press |
| `ui_hover` | `SfxUiHover.wav` | 4 | Mouse enter on interactive element |
| `ui_confirm` | `SfxUiConfirm.wav` | 1 | Action confirmed |
| `ui_cancel` | `SfxUiCancel.wav` | 1 | Action cancelled / back |
| `ui_error` | `SfxUiError.wav` | 1 | Action unavailable |
| `ui_notification` | `SfxUiNotification.wav` | 2 | `INotificationService` passive toast |
| `game_saved` | `SfxGameSaved.wav` | 1 | Autosave completion feedback |
| `turn_end` | `SfxTurnEnd.wav` | 1 | End of player turn (tactical mode) |
| `overwatch_trigger` | `SfxOverwatchTrigger.wav` | 2 | Enemy enters overwatch cone |

---

### 🎙️ Voice Barks — Full Character Breakdown

Each character has clips across **8 trigger types**, with **2–3 variants per trigger** to avoid repetition. Total: ~22 clips per character, ~88 clips across all 4 characters.

File convention: `SfxVoice[Character][Trigger][Variant].wav`

#### Trigger Types

| Trigger | `enum` Value | When it fires |
|---|---|---|
| `Selection` | `Selection` | Player clicks/selects the character |
| `MoveOrder` | `MoveOrder` | Character given a move command |
| `Shoot` | `Shoot` | Character fires their weapon |
| `Miss` | `Miss` | Shot misses the target |
| `Hit` | `Hit` | Shot connects with an enemy |
| `Hurt` | `Hurt` | Character receives damage |
| `Reload` | `Reload` | Character reloads their weapon |
| `TraitTrigger` | `TraitTrigger` | Character's unique trait activates |

---

#### Milena Vlková — The Demolitionist 🔴
*Czech, enthusiastic, gleefully obsessed with destruction. Laughs during explosions.*

| Trigger | Variant | Example Subtitle |
|---|---|---|
| `Selection` | 01 | *"Yes? What do you need blown up?"* |
| `Selection` | 02 | *"I was just thinking about shaped charges."* |
| `Selection` | 03 | *"Talk to me. I'm already bored."* |
| `MoveOrder` | 01 | *"Moving. Try not to block my sight lines."* |
| `MoveOrder` | 02 | *"On it. I'll take the scenic route."* |
| `Shoot` | 01 | *"There we go—"* *(pulls trigger)* |
| `Shoot` | 02 | *"Don't move—perfect."* |
| `Miss` | 01 | *"Hah! I missed. That almost never happens."* |
| `Miss` | 02 | *"Wind. Definitely the wind."* |
| `Hit` | 01 | *"That's the one."* |
| `Hit` | 02 | *"Precision. Even with bullets."* |
| `Hurt` | 01 | *"Ow! That was my shooting arm!"* |
| `Hurt` | 02 | *"Fine. Fine. I've had worse. Much worse."* |
| `Reload` | 01 | *"Reloading — don't panic."* |
| `Reload` | 02 | *"Hold on. Mag swap."* |
| `TraitTrigger` | 01 | *"Partial cover? *(laughs)* Adorable."* *(Calculated Yield activates — blast ignores cover)* |
| `TraitTrigger` | 02 | *"Predictable yield. Clean radius. Beautiful."* |

---

#### Lin Chen — The Ghost 🔵
*Hong Kong, whispered and minimal. Communicates in clipped single-syllable tactical callouts. Unsettling.*

| Trigger | Variant | Example Subtitle |
|---|---|---|
| `Selection` | 01 | *"..."* *(just a quiet exhale)* |
| `Selection` | 02 | *"Ready."* |
| `Selection` | 03 | *"Speak."* |
| `MoveOrder` | 01 | *"Moving."* |
| `MoveOrder` | 02 | *"Silent."* |
| `Shoot` | 01 | *(no line — just the shot)* |
| `Shoot` | 02 | *"Now."* |
| `Miss` | 01 | *"Adjusted."* |
| `Miss` | 02 | *"..."* *(a single quiet exhale of irritation)* |
| `Hit` | 01 | *"Down."* |
| `Hit` | 02 | *"Confirmed."* |
| `Hurt` | 01 | *"..."* *(sharp inhale)* |
| `Hurt` | 02 | *"Noted."* |
| `Reload` | 01 | *"Reloading."* |
| `Reload` | 02 | *(magazine click — no words)* |
| `TraitTrigger` | 01 | *(dead silence — trait is her making no noise at all)* |
| `TraitTrigger` | 02 | *"You won't hear me."* |

---

#### Deacon — The Operator 🟡
*London English, impeccably polite. Calculates odds aloud before doing anything dangerous. Apologises to enemies.*

| Trigger | Variant | Example Subtitle |
|---|---|---|
| `Selection` | 01 | *"I calculate a seventy-three percent chance this ends badly. Shall we proceed?"* |
| `Selection` | 02 | *"You have my attention. Briefly."* |
| `Selection` | 03 | *"Ah. Yes. What would you like me to destroy?"* |
| `MoveOrder` | 01 | *"Relocating. Odds of survival improve by eleven percent over there."* |
| `MoveOrder` | 02 | *"Right. Moving."* |
| `Shoot` | 01 | *"My apologies."* *(fires)* |
| `Shoot` | 02 | *"This will be brief."* |
| `Miss` | 01 | *"Statistically, a miss was overdue. I've been running hot."* |
| `Miss` | 02 | *"Hm. The odds disagreed with me."* |
| `Hit` | 01 | *"As expected."* |
| `Hit` | 02 | *"The mathematics were sound."* |
| `Hurt` | 01 | *"That— that was uncalled for."* |
| `Hurt` | 02 | *"I've recalculated. This is now a problem."* |
| `Reload` | 01 | *"One moment. Recalibrating."* |
| `Reload` | 02 | *"Reloading. Don't do anything inadvisable while I'm busy."* |
| `TraitTrigger` | 01 | *"Double or nothing."* *(Double or Nothing — high crit fires)* |
| `TraitTrigger` | 02 | *"I'll admit the odds on that were... unfavourable."* *(catastrophic failure variant)* |

---

#### Arthur Baxter — The Enforcer 🟠
*Manchester English, gruff, cynical. Complains about his knees, outdated kit, and young criminals with no manners.*

| Trigger | Variant | Example Subtitle |
|---|---|---|
| `Selection` | 01 | *"What? I was comfortable."* |
| `Selection` | 02 | *"Alright, alright. My knees, though."* |
| `Selection` | 03 | *"Yeah. I'm here. What d'you want?"* |
| `MoveOrder` | 01 | *"Moving. If these knees hold out."* |
| `MoveOrder` | 02 | *"Right. Going. Stop nagging."* |
| `Shoot` | 01 | *"Have that."* |
| `Shoot` | 02 | *"Back in your box."* |
| `Miss` | 01 | *"Bloody hell. Used to be better than this."* |
| `Miss` | 02 | *"The sights are off. That's the problem. The sights."* |
| `Hit` | 01 | *"Get in."* |
| `Hit` | 02 | *"That'll learn ya."* |
| `Hurt` | 01 | *"OI. Watch it, you—"* |
| `Hurt` | 02 | *"Right. That's done it. I'm angry now."* |
| `Reload` | 01 | *"Reloading. Don't die while I'm doing this."* |
| `Reload` | 02 | *"Give us a second. Old hands."* |
| `TraitTrigger` | 01 | *"Lovely. Took out the whole wall."* *(Blunt Instrument — cover destroyed, bonus applies)* |
| `TraitTrigger` | 02 | *"...I may have overdone that slightly."* *(backfire variant — structural damage alerts reinforcements)* |

---



## 🔗 Integration Points

| System | How it uses `IAudioService` |
|---|---|
| [`IWeatherService`](file:///d:/Data_efilt/OneDrive/12%20Areas/Ideas/IsoTactics/doc/42 Game Services.md) | `PlayAmbient()` on level load; `PlaySFX("SfxThunderRumble")` for lightning |
| [`IGameStateService`](file:///d:/Data_efilt/OneDrive/12%20Areas/Ideas/IsoTactics/doc/42 Game Services.md) | `PlayMusic("combat")` on combat start; `PlayMusic("exploration")` on return to idle |
| [`IActionQueueService`](file:///d:/Data_efilt/OneDrive/12%20Areas/Ideas/IsoTactics/doc/42 Game Services.md) | `PlaySFX2D()` for weapon fire during sequenced combat animation |
| [`ISettingsService`](file:///d:/Data_efilt/OneDrive/12%20Areas/Ideas/IsoTactics/doc/42 Game Services.md) | `SetBusVolume()` when player adjusts sliders in Settings screen |
| [`IsoEntity`](file:///d:/Data_efilt/OneDrive/12%20Areas/Ideas/IsoTactics/src/Scripts/Core/IsoEntity.cs) | Planned DI injection (`protected IAudioService Audio`) — currently commented out |
| [`IsoCharacter`](file:///d:/Data_efilt/OneDrive/12%20Areas/Ideas/IsoTactics/src/Scripts/Core/IsoCharacter.cs) | `PlayBark()` on selection, movement, hits |
| `WeatherPreset.AmbientSoundLoop` | Provides the ambient track reference consumed by `IWeatherService` → `IAudioService` |

---

## 🧪 Testing Plan

- [ ] **Unit:** Mock `IAudioService` via DI swap in automated tests — no real audio plays, methods are verifiable.
- [ ] **Integration:** Register `AudioService` in a test scene; call `PlayMusic`, `PlayAmbient`, `PlaySFX2D` — verify correct player nodes are activated.
- [ ] **Culling:** Spawn 10 entities firing simultaneously; verify only `MaxSimultaneous` gunshot sounds play per frame in the Godot audio debugger.
- [ ] **Crossfade:** Trigger `PlayMusic("combat")` followed immediately by `PlayMusic("exploration")` — verify smooth A/B fade, no audio pop.
- [ ] **Volume:** Adjust bus volumes via `SetBusVolume`; verify Godot's Audio Bus panel reflects the change in real time.
- [ ] **Scene reload:** Load `Lvl01Warehouse`, change music to combat, reload scene, verify BGM continues uninterrupted.

---

## ❓ Open Questions & Design Decisions

### Q1 — Audio Playlists: Global or Per-Level?

**Question:** Should music playlists (the set of BGM tracks available for a given context) be defined globally for the whole game, or configured individually per level?

| Option | Description | Pros | Cons |
|---|---|---|---|
| **A — Global playlist** | One shared set of music tracks used across all levels. `IAudioService` manages a single registry of track keys (`exploration`, `combat`, `stealth`) that maps to fixed files. | Simple to implement. No per-level data required. Consistent audio identity across the game. | Every level sounds the same. No room for a level with a unique musical mood (e.g. Level 15 `GoldenHour` feeling different from a rainy warehouse). |
| **B — Per-level playlist** | Each `Level` resource defines its own `AudioPlaylist` resource specifying which BGM files to use for each state. `IAudioService` loads this playlist on level load. | Levels can have unique soundscapes. Supports mood variation (jazz for a bar level, industrial for a warehouse). Natural fit with `WeatherPreset` per-level data. | More assets to produce. Slightly more complex service API. Requires a `LevelAudioConfig` resource per level. |
| **C — Hybrid** | A global default playlist is used, but individual levels can optionally override specific tracks. If no override is defined, the default fires. | Best of both: simple for most levels, flexible when needed. | Slightly more complex to implement (fallback chain in `IAudioService`). |

> [!IMPORTANT]
> **Recommendation:** Option C (Hybrid). The global default covers all prototype levels with the existing `BgmExploration` / `BgmCombat` / `BgmStealth` tracks. Individual levels (e.g. the bar interior, the golden-hour rooftop) can override with a curated track without requiring a full per-level audio config for every scene.
>
> This also aligns well with the existing `WeatherPreset` pattern in `Level` resources — a `LevelAudioOverride` optional resource alongside `WeatherPreset` would be the natural extension point.

**Decision needed from:** Game director / audio lead.  
**Impacts:** `Level` data model (`43 Game Objects.md`), `IAudioService.PlayMusic()` signature, asset production scope.

---

## 🔗 Related Resources


- [`42 Game Services.md`](file:///d:/Data_efilt/OneDrive/12%20Areas/Ideas/IsoTactics/doc/42 Game Services.md) — `IAudioService` original stub and responsibilities
- [`43 Game Objects.md`](file:///d:/Data_efilt/OneDrive/12%20Areas/Ideas/IsoTactics/doc/43 Game Objects.md) — `AudioInstance`, `DialogueBark`, `GameSettings` data models
- [`53 Asset List.md`](file:///d:/Data_efilt/OneDrive/12%20Areas/Ideas/IsoTactics/doc/53 Asset List.md) — Audio asset category overview
- [`93 Resource Naming.md`](file:///d:/Data_efilt/OneDrive/12%20Areas/Ideas/IsoTactics/doc/93 Resource Naming.md) — `Bgm`, `Amb`, `Sfx` file naming conventions
- [`32 Weather Effects.md`](file:///d:/Data_efilt/OneDrive/12%20Areas/Ideas/IsoTactics/doc/32 Weather Effects.md) — Weather × audio crossfade integration
- [`71 Game Scene Setup.md`](file:///d:/Data_efilt/OneDrive/12%20Areas/Ideas/IsoTactics/doc/71 Game Scene Setup.md) — `SysAudioManager.tscn` scene naming; `SFXPlayer` / `BarkPlayer` node layout on characters
- [`GameBootstrapper.cs`](file:///d:/Data_efilt/OneDrive/12%20Areas/Ideas/IsoTactics/src/Scripts/Core/GameBootstrapper.cs) — DI registration point

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Cardtrip** is a Unity 6 (6000.0.36f1) WebGL-based AR travel card game developed by FUTUREVISION (Kim Bummoo). The game is set in Yongin City (용인시) and uses AR world tracking to place 3D objects in the real world via a mobile browser.

## Engine & Platform

- **Unity 6**: Version `6000.0.36f1`
- **Build Target**: WebGL only (camera control uses `Application.ExternalCall` to call JavaScript)
- **Rendering**: URP (Universal Render Pipeline) 17.0.3, with separate `Mobile_RPAsset` and `PC_RPAsset` settings
- **AR**: Imagine WebAR (wTracker 6) — world tracking. The AR camera's position and name are hardcoded; do not rename or move it.
- **Custom WebGL Template**: `Assets/WebGLTemplates/wTracker 6/` — includes `arcamera.js`, `wtracker.js`, `opencv.js`

## Key Packages

- `com.unity.addressables` 2.2.2 — asset loading
- `com.unity.inputsystem` 1.12.0
- `com.unity.render-pipelines.universal` 17.0.3
- `com.unity.nuget.newtonsoft-json` 3.2.2 — JSON serialization
- `com.unity.timeline` 1.8.7

## Architecture

### MVVM Pattern (namespace `FUTUREVISION`)

All game scripts extend from a custom base class hierarchy defined in `Assets/00_FUTUREVISION/02. System/BaseComponent/`:

```
MonoBehaviour
└── Base                    # isInitialize flag, virtual Initialize()
    ├── BaseModel            # data/state storage
    ├── BaseView             # UI show/hide (CanvasGroup)
    ├── BaseViewModel        # owns SubViewList, initializes them
    ├── BaseItem             # interactive world objects
    └── BaseSingleton<T>     # generic singleton with DontDestroyOnLoad
        └── BaseManager<T>   # owns ModelList + ViewModelList, initializes both
```

`Base.OnEnable()` auto-re-calls `Initialize()` if the object was already initialized, so initialization logic must be idempotent.

### Singleton Managers

| Manager | Singleton | Owns |
|---|---|---|
| `GlobalManager` | `GlobalManager.Instance` | `DataModel`, `SoundModel`, `Gemini_Chatbot` |
| `WebARManager` | `WebARManager.Instance` | `ARTrackerModel`, `ARViewModel`, `ContentViewModel` |

### Content Flow

`ContentViewModel` drives the main game state machine via `ContentState`:

```
Intro → Recommendation → Location → CardTrip → Stamp → Reward
```

**CardTrip** has its own sub-stages (`ECardTripStage`):
```
Stage1Guide → Stage1Play (find treasure)
→ Stage2Guide → Stage2Play (collect orbs)
→ Stage3Guide → Stage3Play (drag card onto AR target)
→ StageClear
```

### AR States

`ARTrackerModel` manages two AR modes:
- `ScreenState` — content tracked to screen
- `WorldState` — content anchored in world via `WorldTracker.PlaceOrigin()`

Camera modes: `Front` / `Back` (switching calls JS via `Application.ExternalCall("SetWebCamSetting", ...)`)

## Directory Structure

```
Assets/
├── 00_FUTUREVISION/         # Reusable FUTUREVISION framework
│   ├── 02. System/
│   │   ├── BaseComponent/   # Base, BaseModel, BaseView, BaseViewModel, BaseItem, BaseSingleton, BaseManager
│   │   ├── Component/       # ImageSwapper, WebGLDownloader, LoadWithReference (Addressable loader)
│   │   ├── Content/         # Reusable content views (Intro, Bingo, Quiz, etc.)
│   │   ├── UIItem/          # TextUIItem, ImageUIItem, ButtonUIItem, TextBoxUIItem
│   │   └── Temp/            # Experimental: GeminiLive, WebCamera, GPS, FaceTracking
│   └── 03. Art/
├── 02. System/              # Project-specific implementation
│   ├── GlobalManager/       # GlobalManager.cs, DataModel.cs, SoundModel.cs
│   ├── WebAR/               # WebARManager, ARTrackerModel, ARViewModel, ARObjectView, ARUIView
│   └── Content/             # ContentViewModel, IntroView, RecommendationView,
│                            # CardTripView, StampView, RewardView, LocationView
├── 01. Scenes/              # MainScene.unity (only scene)
├── Settings/                # URP render pipeline assets (Mobile/PC)
└── WebGLTemplates/          # wTracker 6 WebGL template
```

## Key Systems

### Data & Persistence
- `DataModel` parses URL query parameters on startup (`Application.absoluteURL`)
- Mission progress saved via `PlayerPrefs` (keys: `Mission1Clear`–`Mission4Clear`)
- `contentIndex` URL param selects which of 4 missions is active
- Generic JSON save/load helpers: `DataModel.SaveJsonData<T>` / `LoadJsonData<T>`

### Gemini AI Integration
- `Gemini_Chatbot` (in `GlobalManager`) calls `gemini-2.5-flash` REST API
- Used for travel recommendation: user answers 4 survey questions, answers are sent to Gemini, response is a single digit (1/2/3) selecting a course
- API key is set in the Inspector on the `Gemini_Chatbot` component

### SoundModel
- `PlayButtonClickSound()`, `PlayMissionSound(bool)`, `PlayPopupSound()`, `PlayWrongSound()`
- SFX created as temporary child `AudioSource` objects, destroyed after playback; BGM uses a persistent `AudioSource`

### Addressables
- `DataModel.ObjectReferences` — AR content prefabs loaded at startup via `LoadAssetAsync<GameObject>()`
- `LoadWithReference` component — configurable load/release timing for individual scene objects

## Script Template

New scripts should use `Assets/ScriptTemplates/10-FutureVision__Monobehaviour Script-NewBehaviourScript.cs.txt` as the template, which sets up the `FUTUREVISION` namespace and `Initialize()` method.

## Building

This is a standard Unity project — build via Unity Editor:
- **Build target**: WebGL
- **WebGL Template**: Select `wTracker 6` in Player Settings
- URP settings: use `Mobile_RPAsset` for mobile WebGL builds

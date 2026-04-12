# SettingsSingleton — Auto-Loading Settings Framework

## Overview

SettingsSingleton<T> and its base SettingsSingleton provide a framework for ScriptableObject-based
settings that automatically load and initialize before the Unity splash screen. Each settings type
is a singleton accessible via a static Instance property.

```
┌─────────────────────────────────────────────────────────────────────┐
│  SettingsSingleton : ScriptableObject, ISettings (abstract)        │
│  [Serializable]                                                     │
│                                                                     │
│  Properties:                                                        │
│    virtual bool IncludeInBuild => true                              │
│                                                                     │
│  Lifecycle:                                                         │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  [RuntimeInitializeOnLoadMethod(BeforeSplashScreen)]          │  │
│  │  LoadAll()                                                    │  │
│  │    │                                                          │  │
│  │    ├─ UNITY_EDITOR:                                          │  │
│  │    │   AssetDatabase.FindAssets("t:SettingsSingleton a:all") │  │
│  │    │   → load each asset → Initialize()                      │  │
│  │    │                                                          │  │
│  │    └─ RUNTIME:                                                │  │
│  │        Resources.FindObjectsOfTypeAll<SettingsSingleton>()    │  │
│  │        → Initialize() on each                                │  │
│  └───────────────────────────────────────────────────────────────┘  │
│                                                                     │
│  abstract void Initialize()  ← called on each found singleton      │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│  SettingsSingleton<T> : SettingsSingleton (abstract)               │
│    where T : SettingsSingleton                                      │
│                                                                     │
│  Static:                                                            │
│    static T I { get; private set; }                                 │
│      └─ GetSingleton(ref settings) → lazy CreateInstance<T>()       │
│                                                                     │
│  sealed override void Initialize():                                 │
│    Assert.AreEqual(this.GetType(), typeof(T))                       │
│    I = this as T                                                    │
│    OnInitialize()  ← virtual hook                                   │
│                                                                     │
│  virtual void OnInitialize() { }  ← empty default                  │
└─────────────────────────────────────────────────────────────────────┘
```

## Load Flow

```
BeforeSplashScreen
       │
       ▼
  LoadAll()
       │
       ├─ EDITOR: AssetDatabase.FindAssets → load each .asset
       │
       └─ RUNTIME: Resources.FindObjectsOfTypeAll
       │
       ▼
  For each found SettingsSingleton:
       │
       ▼
  setting.Initialize()
       │
       ▼ (in generic SettingsSingleton<T>)
  Assert type matches generic parameter
       │
       ▼
  I = this as T  ← sets static Instance
       │
       ▼
  OnInitialize()  ← user hook
```

## Key Design Decisions

- **BeforeSplashScreen**: Loads before the game even starts, ensuring settings are available
  in any system's OnCreate.
- **Editor vs Runtime**: Editor uses AssetDatabase to find assets by type; runtime relies on
  Resources.FindObjectsOfTypeAll (assets must be included in build).
- **IncludeInBuild**: Virtual property allows settings to opt out of build inclusion.
- **Lazy instance**: GetSingleton creates a default instance via CreateInstance if the field
  is null (falsy), ensuring I always returns a valid reference.
- **Sealed Initialize**: The generic class seals Initialize to prevent user error; use
  OnInitialize instead.

## Verified Data

```
SettingsSingleton (base)
  Kind: abstract class, base=ScriptableObject
  Static Methods: T GetSingleton<T>, Void LoadAll, Void InitializeInEditor
  LoadAll uses [RuntimeInitializeOnLoadMethod(BeforeSplashScreen)]

Verified: 2 checks, 0 failures
```


> Tested example: [Example/SettingsExample.cs](../Example/SettingsExample.cs)

## Source

- [BovineLabs.Core/Settings/SettingsSingleton.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Settings/SettingsSingleton.cs)

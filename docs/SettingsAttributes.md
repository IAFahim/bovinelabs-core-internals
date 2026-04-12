# Settings Attributes — Editor Organization

## Overview

Three attributes control how settings ScriptableObjects are organized in the Unity Editor:
SettingsGroupAttribute groups them in menus, SettingSubDirectoryAttribute specifies asset
subdirectory locations, and SettingsWorldAttribute targets settings to specific ECS worlds.

## SettingsGroupAttribute

```
[SettingsGroup(string group)]
  Target: class, Inherited = false
  Property: string Group { get; }

  Usage:
  [SettingsGroup("Gameplay")]
  public class GameplaySettings : SettingsSingleton<GameplaySettings> { }

  → Groups the settings under "Gameplay" in the settings window
```

## SettingSubDirectoryAttribute

```
[SettingSubDirectory(string directory)]
  Target: class
  Property: string Directory { get; }

  Usage:
  [SettingSubDirectory("Settings/Game")]
  public class GameConfig : SettingsSingleton<GameConfig> { }

  → Creates/finds the settings asset in Assets/Settings/Game/
```

## SettingsWorldAttribute

```
[SettingsWorld(params string[] worlds)]
  Target: class, Inherited = false
  Property: string[] Worlds { get; }

  Usage:
  [SettingsWorld("Client", "Server")]
  public class NetworkSettings : SettingsSingleton<NetworkSettings> { }

  → Only loads in worlds matching "Client" or "Server" (case-insensitive)
  → Keys match EditorSettings world names
```

## Key Design Decisions

- **Inherited=false on group/world**: SettingsGroupAttribute and SettingsWorldAttribute don't
  inherit to prevent subclasses from accidentally inheriting their parent's grouping/world.
- **Case-insensitive world matching**: Worlds are compared case-insensitively for robustness.
- **params for worlds**: SettingsWorldAttribute accepts multiple world names, allowing a single
  settings class to target several ECS worlds.

## Verified Data

```
SettingsGroupAttribute
  Kind: class, base=Attribute
  Properties: String Group

SettingsWorldAttribute
  Kind: class, base=Attribute
  Properties: String[] Worlds

SettingSubDirectoryAttribute
  Kind: class, base=Attribute
  Properties: String Directory

Verified: 3 checks, 0 failures
```

## Source

- [BovineLabs.Core/Settings/SettingsGroupAttribute.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Settings/SettingsGroupAttribute.cs)
- [BovineLabs.Core/Settings/SettingSubDirectoryAttribute.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Settings/SettingSubDirectoryAttribute.cs)
- [BovineLabs.Core/Settings/SettingsWorldAttribute.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Settings/SettingsWorldAttribute.cs)

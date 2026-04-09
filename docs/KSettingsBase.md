# KSettingsBase

## Inner Workings Diagram

```
 KSettingsBase
 ======================================================================
 Defined as: KSettingsBase
 Namespace:  BovineLabs.Core.Keys

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ KSettingsBase                                                      │
 ├────────────────────────────────────────────────────────────────────┤
 │ T                        I                                         │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ NameToKey(FixedString32Bytes name)                                 │
 │   → TV                                                             │
 │ TryNameToKey(FixedString32Bytes name, out TV key)                  │
 │   → bool                                                           │
 │ KeyToName(TV key)                                                  │
 │   → FixedString32By                                                │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core/Keys/KSettingsBaseTV.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Keys/KSettingsBaseTV.cs)

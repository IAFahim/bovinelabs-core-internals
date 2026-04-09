# FeatureToggle

## Inner Workings Diagram

```
 FeatureToggle
 ======================================================================
 Defined as: FeatureToggle
 Namespace:  BovineLabs.Core.Editor.Welcome

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ FeatureToggle                                                      │
 ├────────────────────────────────────────────────────────────────────┤
 │ string                   USSFeatureClassName                       │
 │ string                   FeatureToggleUssClassName                 │
 │ string                   FeatureToggleDescriptionUssClassName      │
 │ string                   Define                                    │
 │ bool                     FeatureEnabled                            │
 │ string                   Description                               │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ SetFeatureEnabledWithoutNotify(bool newValue)                      │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
Welcome: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [BovineLabs.Core.Editor/Welcome/FeatureToggle.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Editor/Welcome/FeatureToggle.cs)

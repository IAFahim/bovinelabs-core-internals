# WelcomeWindow

## Inner Workings Diagram

```
 WelcomeWindow
 ======================================================================
 Defined as: WelcomeWindow
 Namespace:  BovineLabs.Core.Editor.Welcome

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ WelcomeWindow                                                      │
 ├────────────────────────────────────────────────────────────────────┤
 │ string                   PackageName                               │
 │ string                   GitUrl                                    │
 │ PackageElement           Element                                   │
 │ IReadOnlyList<string>    Dependencies                              │
 │ bool                     Installed                                 │
 │ AddAndRemoveRequest      InstallRequest                            │
 │ bool                     HadError                                  │
 │ string                   Define                                    │
 │ bool                     Supported                                 │
 │ FeatureToggle            FeatureToggle                             │
 │ Toggle                   Toggle                                    │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core.Editor/Welcome/WelcomeWindow.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Editor/Welcome/WelcomeWindow.cs)

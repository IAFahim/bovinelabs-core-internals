# SearchElement — Popup Search Field

## Overview

SearchElement is a UIElements field (BaseField<int>) that shows a button with a dropdown arrow.
When clicked, it opens a SearchWindow popup where users can search and select from a list of items.
Used throughout BovineLabs editor tools for type/component/variable selection.

```
┌─────────────────────────────────────────────────────────────────────┐
│  SearchElement : BaseField<int>                                     │
│                                                                     │
│  Constructor:                                                       │
│    SearchElement(List<SearchView.Item> items, string defaultText,   │
│                  string displayName = "")                           │
│                                                                     │
│  Visual Structure:                                                  │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │  [Label]  [  Button Text                    ▼ ]             │    │
│  │           └──── componentButton ────┘ └── arrow ──┘        │    │
│  └─────────────────────────────────────────────────────────────┘    │
│                                                                     │
│  Events:                                                            │
│    event Action<SearchView.Item> OnSelection                        │
│                                                                     │
│  Properties:                                                        │
│    Func<SearchView.Item, string> SetText { get; set; }              │
│      → default: item => item.Name                                   │
│    float Height { get; set; } = 315                                 │
│    string Text { get; set; }  ← button text                        │
│                                                                     │
│  Methods:                                                           │
│    void SetValue(int index)                                         │
│      → Selects item at index, fires OnSelection, updates text       │
└─────────────────────────────────────────────────────────────────────┘
```

## Popup Flow

```
Button Click
       │
       ▼
  SearchWindow.Create()
       │
       ▼
  searchWindow.Title = displayName
  searchWindow.Items = items
  searchWindow.OnSelection += callback
       │
       ▼
  Position popup below the field element
       │
       ▼
  searchWindow.ShowPopup()
       │
       ▼
  User selects item:
    OnSelection?.Invoke(item)
    componentButton.text = SetText(item)
```

## Key Design Decisions

- **BaseField<int>**: Inherits from BaseField<int> for UIElements integration, though the int
  value itself isn't heavily used (selection is callback-driven).
- **Popup positioning**: Calculates world bounds from the element's position in the editor window.
- **SetText func**: Customizable display text function — defaults to item.Name but can map to
  any string representation.
- **CSS class mimicking**: Adds popup-field CSS classes to visually match Unity's standard popup
  fields.

## Verified Data

```
SearchElement
  Kind: class, base=BaseField<int>
  Properties: Func<SearchView.Item, String> SetText, Single Height, String Text
  Methods: Void SetValue
  Fields: List<SearchView.Item> items (private), Button componentButton (private),
    Action<SearchView.Item> OnSelection (private)

Verified: 1 checks, 0 failures
```


> Tested example: [Example/EditorUIExample.cs](../Example/EditorUIExample.cs)

## Source

- [BovineLabs.Core.Editor/UI/SearchElement.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Editor/UI/SearchElement.cs)

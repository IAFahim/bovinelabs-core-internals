# ElementEditor and ElementProperty — Custom Inspector Bases

## Overview

ElementEditor and ElementProperty are base classes for building custom Unity Inspector GUIs using
UIElements. They provide a structured lifecycle (PreElementCreation → CreateElement loop →
PostElementCreation) with sensible defaults that fall back to standard PropertyField rendering.

## ElementEditor

```
┌─────────────────────────────────────────────────────────────────────┐
│  ElementEditor : Editor (abstract)                                  │
│                                                                     │
│  Properties:                                                        │
│    VisualElement Parent { get; }                                    │
│    virtual bool IncludeScript => true                               │
│    bool MultiEditing => targets.Length > 1                          │
│                                                                     │
│  Sealed Override:                                                   │
│    CreateInspectorGUI() → VisualElement                             │
│    ┌───────────────────────────────────────────────────────────┐    │
│    │  1. Create parent VisualElement                           │    │
│    │  2. If IncludeScript: add disabled Script PropertyField   │    │
│    │  3. createElements = PreElementCreation(parent)           │    │
│    │  4. If createElements:                                    │    │
│    │       foreach property in IterateAllChildren:              │    │
│    │         element = CreateElement(property)                  │    │
│    │         if element != null: parent.Add(element)            │    │
│    │  5. PostElementCreation(parent, createElements)           │    │
│    │  6. return parent                                         │    │
│    └───────────────────────────────────────────────────────────┘    │
│                                                                     │
│  Virtual Overrides:                                                 │
│    CreateElement(SerializedProperty) → PropertyField by default     │
│    PreElementCreation(VisualElement) → true by default              │
│    PostElementCreation(VisualElement, bool) → empty                 │
│                                                                     │
│  Helpers:                                                           │
│    static CreatePropertyField(property, serializedObject)           │
│    static CreateFoldout(text, value = false)                        │
│      → foldout with list-view styling (no left margin)              │
└─────────────────────────────────────────────────────────────────────┘
```

## ElementProperty

```
┌─────────────────────────────────────────────────────────────────────┐
│  ElementProperty : PropertyDrawer (abstract)                        │
│                                                                     │
│  Properties:                                                        │
│    VisualElement Parent                                             │
│    SerializedObject SerializedObject                                │
│    SerializedProperty RootProperty                                  │
│    virtual ParentTypes ParentType => ParentTypes.Foldout            │
│                                                                     │
│  Sealed Override:                                                   │
│    CreatePropertyGUI(SerializedProperty) → VisualElement             │
│    ┌───────────────────────────────────────────────────────────┐    │
│    │  Parent container based on ParentType:                    │    │
│    │    Foldout → Foldout { text = displayName }               │    │
│    │    Label  → VisualElement + Label header                  │    │
│    │    None   → VisualElement                                 │    │
│    │                                                           │    │
│    │  If Generic property: iterate children                    │    │
│    │  Else: CreateElement on the root property                 │    │
│    └───────────────────────────────────────────────────────────┘    │
│                                                                     │
│  Cache<T>() → T                                                     │
│    → Shared state dictionary keyed by RootProperty                  │
│                                                                     │
│  Virtual: CreateElement, PreElementCreation, PostElementCreation    │
│  Virtual: GetDisplayName(property) → property.displayName           │
│                                                                     │
│  ParentTypes enum: Foldout, Label, None                             │
└─────────────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **Sealed lifecycle methods**: CreateInspectorGUI and CreatePropertyGUI are sealed so subclasses
  can't break the lifecycle; they override CreateElement instead.
- **Null element filtering**: CreateElement can return null to skip rendering a property entirely.
- **PropertyField fallback**: Default CreateElement returns a PropertyField, so without any
  overrides the inspector looks identical to the default.
- **Cache<T> for state sharing**: ElementProperty provides a simple cache mechanism for sharing
  state across redraws without fields.

## Verified Data

```
ElementEditor
  Kind: abstract class, base=Editor
  Fields: VisualElement parent (private)
  Methods: VisualElement CreateInspectorGUI (sealed)

ElementProperty
  Kind: abstract class, base=PropertyDrawer
  Fields: SerializedObject serializedObject (private), VisualElement parent (private),
    ParentTypes <ParentType>k__BackingField, SerializedProperty <RootProperty>k__BackingField
  Methods: VisualElement CreatePropertyGUI (sealed)

Verified: 2 checks, 0 failures
```


> Tested example: [Example/EditorUIExample.cs](../Example/EditorUIExample.cs)

## Source

- [BovineLabs.Core.Editor/Inspectors/ElementEditor.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Editor/Inspectors/ElementEditor.cs)
- [BovineLabs.Core.Editor/Inspectors/ElementProperty.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Editor/Inspectors/ElementProperty.cs)

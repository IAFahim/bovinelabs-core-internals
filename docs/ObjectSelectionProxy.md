# ObjectSelectionProxy — Inspector Object Wrapper

## Overview

ObjectSelectionProxy is a ScriptableObject hack that wraps an arbitrary C# object so it can be
selected and displayed in the Unity Inspector. It registers with the Undo system so proxy
creation is undoable.

```
┌─────────────────────────────────────────────────────────────────────┐
│  ObjectSelectionProxy : ScriptableObject, ISerializationCallbackReceiver │
│                                                                     │
│  Fields:                                                            │
│    private object obj                                               │
│                                                                     │
│  Properties:                                                        │
│    object Obj { get; set; }                                         │
│                                                                     │
│  Static Factory:                                                    │
│    CreateInstance(object obj) → ObjectSelectionProxy                 │
│      1. ScriptableObject.CreateInstance<ObjectSelectionProxy>()      │
│      2. hideFlags = DontSaveInBuild | DontSaveInEditor | NotEditable│
│      3. proxy.Obj = obj                                             │
│      4. Undo.RegisterCreatedObjectUndo(proxy, "Create proxy...")    │
│      5. Undo.CollapseUndoOperations(undoGroup)                      │
│                                                                     │
│  ISerializationCallbackReceiver:                                    │
│    OnBeforeSerialize() { }  ← empty                                │
│    OnAfterDeserialize() { } ← empty                                │
└─────────────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **ScriptableObject wrapper**: Unity's Inspector can only display objects that inherit from
  UnityEngine.Object. ObjectSelectionProxy bridges this gap for plain C# objects/structs.
- **HideFlags**: Set to DontSaveInBuild | DontSaveInEditor | NotEditable so the proxy is
  temporary and never persists or appears in the project browser.
- **Undo support**: Registered with the Undo system so creating a proxy can be undone,
  preventing orphaned ScriptableObjects.
- **No serialization**: The obj field is not serialized (OnBeforeSerialize is empty). The proxy
  is a transient runtime-only wrapper.

## Verified Data

```
ObjectSelectionProxy
  Kind: class, base=ScriptableObject
  Properties: Object Obj
  Methods: Void OnBeforeSerialize, Void OnAfterDeserialize
  Fields: Object obj (private)
  Implements ISerializationCallbackReceiver

Verified: 1 checks, 0 failures
```


> Tested example: [Example/EditorUIExample.cs](../Example/EditorUIExample.cs)

## Source

- [BovineLabs.Core.Editor/UI/ObjectSelectionProxy.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Editor/UI/ObjectSelectionProxy.cs)

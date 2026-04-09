# ObjectId

## Inner Workings Diagram

```
 ObjectId
 ======================================================================
 Namespace:  BovineLabs.Core.ObjectManagement

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ ObjectId                 Null                                      │
 │ int                      MaxModsIds                                │
 │ ushort                   Mod                                       │
 │ int                      ID                                        │
 │ bool                     operator                                  │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ CompareTo(ObjectId other)                                          │
 │   → int                                                            │
 │ ToString()                                                         │
 │   → string                                                         │
 │ ToFixedString()                                                    │
 │   → FixedString32By                                                │
 │ Equals(object obj)                                                 │
 │   → bool                                                           │
 │ Equals(ObjectId other)                                             │
 │   → bool                                                           │
 │ GetHashCode()                                                      │
 │   → int                                                            │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core.Extensions/ObjectManagement/ObjectId.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/ObjectManagement/ObjectId.cs)

# GroupId

## Inner Workings Diagram

```
 GroupId
 ======================================================================
 Namespace:  BovineLabs.Core.ObjectManagement

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ GroupId                  Null                                      │
 │ short                    ID                                        │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Equals(GroupId other)                                              │
 │   → bool                                                           │
 │ GetHashCode()                                                      │
 │   → int                                                            │
 │ CompareTo(GroupId other)                                           │
 │   → int                                                            │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core.Extensions/ObjectManagement/GroupId.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/ObjectManagement/GroupId.cs)

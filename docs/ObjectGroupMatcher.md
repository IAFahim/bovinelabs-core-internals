# ObjectGroupMatcher

## Inner Workings Diagram

```
 ObjectGroupMatcher
 ======================================================================
 Defined as: ObjectGroupMatcher
 Namespace:  BovineLabs.Core.ObjectManagement

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ ObjectGroupMatcher                                                 │
 ├────────────────────────────────────────────────────────────────────┤
 │ GroupId                  GroupId                                   │
 │ ObjectId                 ObjectId                                  │
 │ class                    ObjectGroupMatcherExtensions              │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Equals(ObjectGroupKey other)                                       │
 │   → bool                                                           │
 │ Equals((GroupId GroupId, ObjectId ObjectId)                        │
 │   → bool                                                           │
 │ GetHashCode()                                                      │
 │   → int                                                            │
 │ Matches(this DynamicBuffer<ObjectGroupMatcher> buffer, Obj)        │
 │   → bool                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core.Extensions/ObjectManagement/ObjectGroupMatcher.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/ObjectManagement/ObjectGroupMatcher.cs)

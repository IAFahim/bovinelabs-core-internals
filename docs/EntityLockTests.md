# EntityLockTests

## Inner Workings Diagram

```
 EntityLockTests
 ======================================================================
 Defined as: EntityLockTests
 Namespace:  BovineLabs.Core.Tests.Utility

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ EntityLockTests                                                    │
 ├────────────────────────────────────────────────────────────────────┤
 │ EntityLock               EntityLock                                │
 │ ComponentLookup<Count>   Counts                                    │
 │ Entity                   Value                                     │
 │ int3                     Value                                     │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Test()                                                             │
 │   → void                                                           │
 │ OnUpdate(ref SystemState state)                                    │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
Utility: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [BovineLabs.Core.Tests/Utility/EntityLockTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Utility/EntityLockTests.cs)

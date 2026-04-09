# MathExPerformanceTests

## Inner Workings Diagram

```
 MathExPerformanceTests
 ======================================================================
 Defined as: MathExPerformanceTests
 Namespace:  BovineLabs.Core.PerformanceTests.Utility

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ MathExPerformanceTests                                             │
 ├────────────────────────────────────────────────────────────────────┤
 │ NativeArray<float>       Input                                     │
 │ NativeReference<float>   Result                                    │
 │ NativeArray<float>       Input                                     │
 │ NativeReference<float>   Result                                    │
 │ NativeArray<float>       Input                                     │
 │ NativeReference<float>   Result                                    │
 │ NativeArray<float>       Input                                     │
 │ NativeReference<float>   Result                                    │
 │ NativeArray<int>         Input                                     │
 │ NativeArray<int>         Result                                    │
 │ int                      Value                                     │
 │ NativeArray<int>         Input                                     │
 │ NativeArray<int>         Result                                    │
 │ int                      Value                                     │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ MaxTest(int length)                                                │
 │   → void                                                           │
 │ MaxTestComparison(int length)                                      │
 │   → void                                                           │
 │ SumTest(int length)                                                │
 │   → void                                                           │
 │ SumTestComparison(int length)                                      │
 │   → void                                                           │
 │ AddTest(int length)                                                │
 │   → void                                                           │
 │ AddTestComparison(int length)                                      │
 │   → void                                                           │
 │ Execute()                                                          │
 │   → void                                                           │
 │ Execute()                                                          │
 │   → void                                                           │
 │ Execute()                                                          │
 │   → void                                                           │
 │ Execute()                                                          │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
Utility: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [BovineLabs.Core.Tests/Utility/MathExPerformanceTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Utility/MathExPerformanceTests.cs)

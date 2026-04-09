# NativeStreamExtensions.WriteLarge

## Inner Workings Diagram

```
 NativeStreamExtensions.WriteLarge
 ======================================================================
 Namespace:  BovineLabs.Core.Extensions

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ class                    NativeStreamExtensions                    │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ WriteLarge(this ref NativeStream.Writer writer, byte* data, i)     │
 │   → void                                                           │
 │ ReadLarge(this ref NativeStream.Reader reader, byte* buffer,)      │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
NativeStreamExtensions: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [BovineLabs.Core/Extensions/NativeStreamExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/NativeStreamExtensions.cs)

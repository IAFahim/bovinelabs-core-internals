# NativeStreamExtensions.ReadLarge

## Inner Workings Diagram

```
 NativeStreamExtensions.ReadLarge
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

## Source

- [BovineLabs.Core/Extensions/NativeStreamExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/NativeStreamExtensions.cs)

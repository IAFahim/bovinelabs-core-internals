# StringExtensions.ToDotNotation

## Inner Workings Diagram

```
 StringExtensions.ToDotNotation
 ======================================================================
 Namespace:  BovineLabs.Core.Extensions


 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ ToSentence(this string input)                                      │
 │   → string                                                         │
 │ ToDotNotation(this string input)                                   │
 │   → string                                                         │
 │ ToLowerNoSpaces(this string input)                                 │
 │   → string                                                         │
 │ FirstCharToUpper(this string input)                                │
 │   → string                                                         │
 │ FirstCharToLower(this string input)                                │
 │   → string                                                         │
 │ TrimStart(this string source, string value)                        │
 │   → string                                                         │
 │ TrimEnd(this string source, string value)                          │
 │   → string                                                         │
 │ Max(this string source, int length, string replacement)            │
 │   → string                                                         │
 │ ToFixedString32NoError(this string source)                         │
 │   → FixedString32By                                                │
 │ ToFixedString64NoError(this string source)                         │
 │   → FixedString64By                                                │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core/Extensions/StringExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/StringExtensions.cs)

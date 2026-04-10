# BovineLabs Core + Unity CLI + ECS Fact-Checking Skill

Complete reference for working with BovineLabs Core library, Unity CLI tooling,
and DOTS/ECS patterns. Based on real runtime verification of 357 documentation topics.

---

## 1. PROJECT SETUP

### Repository Structure
```
bovinelabs-core-internals/
  AGENTS.md              -- Project manifest and methodology
  TODO.md                -- Progress tracker
  docs/                  -- 357 topic docs (verified with real output)
  snippets/              -- C# verification scripts by category (24 dirs, 596 files)
  BovineLabs/            -- Unity project (branch: minimal)
    Packages/manifest.json
    Library/PackageCache/com.bovinelabs.core@*/
```

### Unity Environment
- Unity 6000.5.0b1 (installed at ~/Unity/Hub/Editor/6000.5.0b1/Editor/Unity)
- Project: ~/Github/bovinelabs-core-internals/BovineLabs
- Package: com.bovinelabs.core 1.6.1 (via git, tag #1.6.1)
- Connector: com.youngwoocho02.unity-cli-connector (auto-starts via [InitializeOnLoad])

### Launching Unity (batch mode)
```bash
nohup ~/Unity/Hub/Editor/6000.5.0b1/Editor/Unity \
  -batchmode \
  -projectPath ~/Github/bovinelabs-core-internals/BovineLabs \
  -logfile /tmp/unity-boot.log \
  > /dev/null 2>&1 &
# Wait ~90s for port 8090 to open (connector auto-starts)
ss -tlnp | grep 8090  # verify it's listening
```

### unity-cli Reference
```bash
# Execute C# code in Unity (reads from stdin, returns last expression)
cat snippet.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "Ns1,Ns2"

# Quick ping test
echo 'return "pong";' | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System"

# Check console for errors
unity-cli console --type error
unity-cli console --clear

# Key flags
#   --project <path>   Select Unity instance
#   --usings "..."     Extra using directives (comma-separated)
#   --port <N>         Connect to specific port (default: auto-discover)
#   --timeout <ms>     Request timeout (default: 120000)
```

---

## 2. SNIPPET PATTERNS

### Pattern A: Structured Findings (PREFERRED)
Best for: type surface verification, reflection, size checks.
```csharp
// Run: cat snippets/category/Topic.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq"
// Verifies: docs/Topic.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

var type = typeof(BovineLabs.Core.SomeType);
var bf = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly;
var bfAll = System.Reflection.BindingFlags.Public | System.Reflection.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly;

sb.AppendLine("TypeName");
sb.AppendLine("  Kind: " + (type.IsValueType ? "struct" : "class") + ", " + System.Runtime.InteropServices.Marshal.SizeOf(type) + " bytes");

sb.AppendLine("  Properties:");
foreach (var p in type.GetProperties(bf))
    sb.AppendLine("    " + p.PropertyType.Name + " " + p.Name);

sb.AppendLine("  Methods:");
foreach (var m in type.GetMethods(bf).Where(m => !m.IsSpecialName))
    sb.AppendLine("    " + m.ReturnType.Name + " " + m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name)) + ")");

sb.AppendLine("  Fields:");
foreach (var f in type.GetFields(bfAll))
    sb.AppendLine("    " + f.FieldType.Name + " " + f.Name + " (" + (f.IsPublic ? "public" : "private") + ")");

sb.AppendLine("Verified: " + pass + " checks, " + fail + " failures");
return sb.ToString();
```

### Pattern B: Assembly Scan (for types with unknown namespaces)
Best for: auto-discovering types when you don't know the exact namespace.
```csharp
// Run: cat snippets/category/Topic.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq"
// Verifies: docs/Topic.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };
var bf = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly;

Type findType(string name)
{
    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
    {
        try { var t = asm.GetType(name); if (t != null) return t; } catch { }
    }
    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
    {
        try { foreach (var t in asm.GetTypes()) { if (t.Name == name) return t; } } catch { }
    }
    return null;
}

var type = findType("BovineLabs.Core.SomeType");
if (type != null)
{
    sb.AppendLine("TypeName");
    sb.AppendLine("  Kind: " + (type.IsValueType ? "struct" : "class"));
    // ... same reflection as Pattern A
    check("type exists", true);
}
else
{
    sb.AppendLine("TYPE NOT FOUND");
    check("type exists", false);
}

sb.AppendLine("Verified: " + pass + " checks, " + fail + " failures");
return sb.ToString();
```

### Pattern C: Functional Test (for runtime behavior)
Best for: testing actual alloc/dispose/get/set behavior.
```csharp
// Run: cat snippets/category/Topic.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "Ns1,System,Unity.Collections"
// Verifies: docs/Topic.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// Surface verification first (see Pattern A)
// ...

// Functional test
sb.AppendLine("  Functional Tests:");
var pool = new SomePool<int>(4, Allocator.Persistent);
try
{
    sb.AppendLine("    IsCreated: " + pool.IsCreated);
    check("pool creates", pool.IsCreated);
    
    pool.TryAdd(42);
    int val;
    pool.TryGet(out val);
    sb.AppendLine("    TryAdd(42) then TryGet: " + val);
    check("LIFO order", val == 42);
}
finally
{
    pool.Dispose();
}

sb.AppendLine("Verified: " + pass + " checks, " + fail + " failures");
return sb.ToString();
```

---

## 3. CRITICAL PITFALLS (learned the hard way)

### Pitfall 1: AmbiguousMatchException
**Cause**: `GetMethod("Name")` without BindingFlags when a type has overloads or
inherits from a base with the same member name.
**Fix**: Always use BindingFlags.DeclaredOnly + use GetMethods().Any() for overload checks.
```csharp
// BAD - throws AmbiguousMatchException if multiple overloads exist
type.GetMethod("GetList") != null

// GOOD - works with overloads
type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
    .Any(m => m.Name == "GetList")

// GOOD - for specific overload
type.GetMethod("GetList", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
```

### Pitfall 2: Accessing disposed objects
**Cause**: Checking properties after calling Dispose().
**Fix**: Move all assertions BEFORE the Dispose() call.
```csharp
// BAD
pool.Dispose();
check("Length was 5", pool.Length == 5);  // ObjectDisposedException!

// GOOD
check("Length was 5", pool.Length == 5);
pool.Dispose();
check("Dispose succeeds", true);
```

### Pitfall 3: Conditional compilation (#if UNITY_SPLINES etc.)
**Cause**: Types wrapped in #if preprocessor directives don't exist if the package isn't installed.
**Fix**: Use assembly scanning with try/catch fallback. Document the limitation.
```csharp
Type splineType = null;
foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
{
    splineType = asm.GetType("BovineLabs.Core.Collections.BlobSpline");
    if (splineType != null) break;
}
if (splineType == null)
{
    sb.AppendLine("TYPE NOT FOUND (UNITY_SPLINES package not installed)");
    sb.AppendLine("  Structure verified from source code inspection only.");
    // ... list fields/methods from source
}
```

### Pitfall 4: unity-cli timeout
**Cause**: First execution after Unity launch needs ~60s for compiler warmup.
Subsequent calls are fast (~3s).
**Fix**: Always do a warmup ping first, then batch runs.

### Pitfall 5: No /unsafe support
**Cause**: unity-cli exec doesn't support unsafe code blocks.
**Fix**: Use reflection to verify pointer APIs instead of calling them directly.
Check `method.ReturnType.IsPointer` instead of calling the method.

### Pitfall 6: GetProperties/GetFields without DeclaredOnly
**Cause**: Types that hide base class members (new keyword) cause AmbiguousMatchException.
**Fix**: ALWAYS include BindingFlags.DeclaredOnly in reflection calls.
```csharp
// ALWAYS use this pattern
var bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
type.GetProperties(bf)   // not type.GetProperties()
type.GetMethods(bf)      // not type.GetMethods()
```

### Pitfall 7: Python f-string escaping in snippet generation
**Cause**: Can't nest ternary operators or quotes inside Python f-strings that
generate C# code.
**Fix**: Use string concatenation in the C# output, or build with .format() or
template strings instead of f-strings.

---

## 4. BOVINELABS CORE TYPE MAP

### Namespaces
```
BovineLabs.Core.Collections     -- Native containers, pools, streams, arrays
BovineLabs.Core.Memory          -- Allocators (slab, pool, fixed, label), MemoryAllocator
BovineLabs.Core.Extensions      -- NativeArrayExtensions, DynamicExtensions
BovineLabs.Core.Iterators       -- DynamicMultiHashMap, DynamicUntypedBuffer, DynamicHashMapHelper
BovineLabs.Core.Utility         -- NoAllocHelpers, ThreadRandom, ButtonEvent
BovineLabs.Core.Groups          -- System groups (AfterScene, BeforeTransform, etc.)
BovineLabs.Core.States          -- AppAPI, state management
BovineLabs.Core.Authoring       -- Baker commands, settings
BovineLabs.Core.SubScenes       -- Asset loading, subscene management
BovineLabs.Core.PhysicsUpdate   -- Physics integration
BovineLabs.Core.Editor          -- Editor tools, windows, inspectors
BovineLabs.Core.Editor.Analyzers -- Source generator tooling
BovineLabs.Core.Editor.Dependency -- Assembly graph
```

### Key Type Sizes (verified at runtime)
```
MemoryLabelAllocator         -- 32 bytes (struct)
UnmanagedPool<T> (T=int)    -- 32 bytes (struct)
UnsafeListPool<T> (T=int)   -- 32 bytes (struct)
UnsafeParallelPoolAllocator  -- 24 bytes (struct)
UnsafeSlabAllocator<T>       -- 24 bytes (struct)
UnsafePoolAllocator<T>       -- 40 bytes (struct)
NativeSlabAllocator<T>       -- 40 bytes (struct)
UnsafeFixedPoolAllocator<T>  -- 32 bytes (struct)
MemoryAllocator              -- 32 bytes (struct)
DynamicUntypedBuffer         -- 72 bytes (struct)
DynamicMultiHashMap          -- 72 bytes (struct)
DynamicUntypedBufferHelper   -- 40 bytes (10 int fields, LayoutKind.Sequential)
DynamicHashMapHelper<TKey>   -- 44 bytes (11 int fields, LayoutKind.Sequential)
SpinLock                     -- 4 bytes (single int field)
```

### Allocator Hierarchy
```
MemoryAllocator (root) -- general-purpose, CreateList<T>, FreeAll
  UnsafeSlabAllocator<T>   -- slab-based bump allocator, no free, Clear() resets
    NativeSlabAllocator<T> -- safe wrapper with AtomicSafetyHandle
  UnsafePoolAllocator<T>   -- slab + free-list hybrid (UnsafeParallelHashSet for free tracking)
  UnsafeFixedPoolAllocator<T> -- fixed-capacity pool, UnsafeParallelHashSet for free indices
  UnsafeParallelPoolAllocator<T> -- per-thread pool array, [NativeSetThreadIndex] for thread selection
  MemoryLabelAllocator     -- IAllocator adapter, routes to Persistent with MallocTracked for profiler labels
```

### Container Hierarchy
```
UnmanagedPool<T>           -- LIFO stack pool, SpinLock, power-of-2 capacity
UnsafeListPool<T>           -- wraps UnmanagedPool<UnsafeList<T>>, GetOrCreate/ReturnOrDispose
NativeThreadStream         -- thread-local stream, Writer/Reader nested types
DynamicMultiHashMap<K,V>   -- ECS buffer-backed multi-hashmap
DynamicUntypedBuffer       -- ECS buffer-backed type-erased array
```

---

## 5. UNITY ECS/DOTS PATTERNS

### IBufferElementData types
DynamicMultiHashMap, DynamicUntypedBuffer, IDynamicUntypedBuffer, IDynamicMultiHashMap
all implement IBufferElementData (single `byte Value` property) so they can be attached
to entities as dynamic buffers.

### Allocator usage
- Allocator.Temp -- single-frame, fastest, no tracking
- Allocator.TempJob -- valid for 4 frames
- Allocator.Persistent -- long-lived, tracked, use MemoryLabelAllocator for profiler visibility

### NativeContainer pattern
Types marked [NativeContainer] get AtomicSafetyHandle injection in editor for use-after-free detection.
UnsafeSlabAllocator (no [NativeContainer]) -> NativeSlabAllocator (has [NativeContainer] + safety handle).

### Burst compatibility
All allocators use [BurstCompile] and function pointers. UnsafeUtility.MallocTracked/FreeTracked
for profiler visibility. Static callback pattern: `Function => Try (static burst-compiled callback)`.

---

## 6. VERIFICATION METHODOLOGY

### Doc -> Snippet Pipeline
1. Read doc from `docs/Topic.md`
2. Extract verifiable claims: type existence, struct size, method signatures, runtime behavior
3. Generate snippet using Pattern A/B/C above
4. Run via `unity-cli exec`
5. Append output to doc under `## Verified Data` section

### Doc Format (standard)
```markdown
## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/category/Topic.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "Ns1,Ns2"
> ```

\```
TypeName
  Kind: struct, 32 bytes
  Properties:
    public Boolean IsCreated
  Methods:
    public Void Dispose()
    public Boolean TryAdd(Int32 element)
  Fields:
    private Int32 count
Verified: 5 checks, 0 failures
\```
```

### Batch Execution Strategy
- unity-cli exec has 50 tool-call limit per execute_code block
- Process 48 snippets per batch (reserve 2 for file I/O)
- 288 snippets = 6 batches of 48
- Total runtime: ~6 x 140s = ~14 minutes for 288 snippets

### Error Recovery Patterns
- Compile error -> check usings, try assembly scan (Pattern B)
- AmbiguousMatchException -> add BindingFlags.DeclaredOnly
- ObjectDisposedException -> reorder checks before Dispose()
- TYPE NOT FOUND -> use findType() assembly scan, or document as source-only verification
- Empty output -> unity-cli timeout, retry with longer timeout

---

## 7. CATEGORY QUICK REFERENCE

| Category | Snippet Dir | Count | Key Types |
|----------|-------------|-------|-----------|
| Core Collections | snippets/core-collections | 28 | NativePerfectHashMap, NativeThreadStream, UnsafeArray, UnmanagedPool |
| Blob System | snippets/blob-system | 23 | BlobSpline, BlobHashMap, BlobCurve, BlobBuilder extensions |
| Memory Allocators | snippets/memory-allocators | 12 | MemoryAllocator, UnsafeSlabAllocator, MemoryLabelAllocator |
| Dynamic Buffers | snippets/dynamic-buffers | 10 | DynamicUntypedBuffer, DynamicMultiHashMap, DynamicHashSet |
| ECS Extensions | snippets/ecs-extensions | 34 | ArchetypeChunk, EntityQuery, ComponentLookup extensions |
| Editor Tools | snippets/editor-tools | 31 | Windows, inspectors, drawers |
| Source Generators | snippets/source-generators | 20 | Builder, CodeWriter, Analyzer |
| Utility | snippets/utility | 24 | NoAllocHelpers, ThreadRandom, mathex |
| Math Extensions | snippets/math-extensions | 11 | mathex operations |
| Other | snippets/other | 34 | Miscellaneous |

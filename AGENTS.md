# AGENTS.md — BovineLabs Core Fact-Checking Project

## What We're Doing

Systematically verifying every claim in the 357 documentation files under `docs/` against the **actual runtime behavior** of `com.bovinelabs.core` 1.6.1 using `unity-cli exec`.

**STATUS: 357/357 docs verified (100%). All snippets run successfully with structured output.**

## Method

1. Read each doc topic from `docs/`
2. Extract verifiable claims (struct sizes, field offsets, method signatures, runtime behavior, type hierarchies)
3. Write a `.cs` snippet that proves/disproves each claim when piped through `unity-cli exec`
4. Each snippet is self-contained with `check()` assertions and structured StringBuilder output
5. Snippets are organized in `snippets/<category>/` folders (24 categories, 596 files)
6. Each doc gets a `## Verified Data` section with the real runtime output

## Project Structure

```
bovinelabs-core-internals/
  AGENTS.md          -- This file (project manifest)
  SKILLS.md          -- Complete skill reference (see below)
  TODO.md            -- Progress tracker
  docs/              -- 357 topic docs (ALL verified)
  snippets/          -- C# verification scripts (24 categories, 596 files)
    core-collections/      -- Native containers, pools, streams
    blob-system/           -- Blob data structures
    memory-allocators/     -- Allocators (slab, pool, fixed, label)
    dynamic-buffers/       -- ECS buffer-backed containers
    ecs-extensions/        -- EntityQuery, ComponentLookup, ArchetypeChunk
    editor-tools/          -- Windows, inspectors, drawers
    source-generators/     -- Builder, CodeWriter, Analyzer
    utility/               -- NoAllocHelpers, ThreadRandom, mathex
    math-extensions/       -- mathex operations
    spatial-physics/       -- AABB, collision, physics
    extension-methods/     -- NativeArray extensions
    authoring-baking/      -- Baker commands, settings
    subscene-system/       -- Asset loading, subscenes
    state-model/           -- State management, AppAPI
    config-vars/           -- ConfigVar system
    singleton-system/      -- Singleton patterns
    pause-time/            -- Pause and time control
    jobs-threading/        -- Jobs, parallel, threading
    relevancy-netcode/     -- Relevancy, netcode
    life-cycle/            -- Lifecycle management
    tests-diagnostics/     -- Test utilities
    other/                 -- Miscellaneous
  BovineLabs/        -- Unity test project (branch: minimal)
```

## Unity CLI Setup

- **Project**: `~/Github/bovinelabs-core-internals/BovineLabs` (branch: `minimal`)
- **Packages**: Core 1.6.1 + InputSystem + unity-cli-connector
- **Unity**: 6000.5.0b1, installed at `~/Unity/Hub/Editor/6000.5.0b1/Editor/Unity`
- **Run in batchmode**: `-batchmode -projectPath` (NO -executeMethod; connector auto-starts via [InitializeOnLoad])
- **First exec after launch is slow (~60s)** — compiler warmup, subsequent calls ~3s
- **Run command**: `cat snippet.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "Ns1,Ns2"`

## Launching Unity

```bash
# Start Unity in background
nohup ~/Unity/Hub/Editor/6000.5.0b1/Editor/Unity \
  -batchmode \
  -projectPath ~/Github/bovinelabs-core-internals/BovineLabs \
  -logfile /tmp/unity-boot.log &

# Wait for connector (port 8090)
for i in $(seq 1 30); do
  ss -tlnp | grep -q 8090 && echo "READY" && break
  sleep 3
done

# Test connection
echo 'return "pong";' | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs
```

## Snippet Patterns

See `SKILLS.md` for the full reference with all 3 patterns:
- **Pattern A**: Structured Findings (direct typeof, preferred for known types)
- **Pattern B**: Assembly Scan (findType helper, for unknown namespaces)
- **Pattern C**: Functional Test (runtime behavior verification)

### Quick Template (Pattern A — most common)

```csharp
// Run: cat snippets/category/Topic.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq"
// Verifies: docs/Topic.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

var type = typeof(BovineLabs.Core.SomeType);
var bf = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly;
var bfAll = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly;

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

## Critical Pitfalls

1. **AmbiguousMatchException**: Always use `BindingFlags.DeclaredOnly` + `GetMethods().Any(m => m.Name == "...")` instead of bare `GetMethod("Name")` for types with overloads
2. **ObjectDisposedException**: Put all assertions BEFORE `Dispose()`
3. **No /unsafe**: unity-cli doesn't support unsafe blocks — use reflection instead
4. **Compile errors**: If typeof doesn't resolve, switch to Pattern B (assembly scan)
5. **BindingFlags**: ALWAYS include `.DeclaredOnly` — types that hide base members fail without it
6. **Timeout**: First exec ~60s, plan warmup ping before batch runs

## Categories (24 total, 357 topics)

| # | Category | Count | Status |
|---|----------|-------|--------|
| 1 | Core Collections | 28 | DONE |
| 2 | Blob System | 23 | DONE |
| 3 | Memory & Allocators | 12 | DONE |
| 4 | Dynamic Buffers | 10 | DONE |
| 5 | ECS Extensions | 34 | DONE |
| 6 | Jobs & Threading | 6 | DONE |
| 7 | State & Model | 13 | DONE |
| 8 | Spatial & Physics | 20 | DONE |
| 9 | Utility | 24 | DONE |
| 10 | Extension Methods | 15 | DONE |
| 11 | ConfigVars | 13 | DONE |
| 12 | Authoring & Baking | 11 | DONE |
| 13 | Editor Tools | 31 | DONE |
| 14 | Source Generators | 20 | DONE |
| 15 | SubScene System | 19 | DONE |
| 16 | Pause & Time | 7 | DONE |
| 17 | Relevancy & Netcode | 6 | DONE |
| 18 | Singleton System | 8 | DONE |
| 19 | Object Management | 13 | DONE |
| 20 | Physics States | 3 | DONE |
| 21 | Life Cycle | 11 | DONE |
| 22 | Tests & Diagnostics | 5 | DONE |
| 23 | Math Extensions | 11 | DONE |
| 24 | Other | 34 | DONE |

## Git Conventions

- All commits include structured output in doc `## Verified Data` sections
- Snippets have `// Run:` header comment with exact command to reproduce
- Snippets have `// Verifies:` header comment linking to the doc
- No old-format PASS/FAIL assertions (replaced with structured findings)

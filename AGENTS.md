# AGENTS.md — BovineLabs Core Fact-Checking Project

## What We're Doing

Systematically verifying every claim in the 357 documentation files under `docs/` against the **actual runtime behavior** of `com.bovinelabs.core` 1.6.1 using `unity-cli exec`.

## Method

1. Read each doc topic from `docs/`
2. Extract verifiable claims (struct sizes, field offsets, method signatures, runtime behavior, type hierarchies)
3. Write a `.cs` snippet that proves/disproves each claim when piped through `unity-cli exec`
4. Each snippet is self-contained with PASS/FAIL assertions
5. Snippets are organized in `snippets/<category>/` folders
6. Each doc gets a `> Verified Data` section appended with markdown links to the snippet

## Project Structure

```
bovinelabs-core-internals/
  AGENTS.md          — This file (project overview)
  TODO.md            — Progress tracker
  docs/              — 357 topic docs (being annotated with verified data)
  snippets/          — C# test scripts organized by category
    core-collections/
    blob-system/
    memory-allocators/
    ...
```

## Unity CLI Setup

- **Project**: `~/Github/bovinelabs-core-internals/BovineLabs` (branch: `minimal`)
- **Packages**: Core 1.6.1 + InputSystem + unity-cli-connector
- **Unity**: 6000.5.0b1, running in batchmode
- **First exec after launch is slow (~60s)** — compiler warmup
- **Run command**: `cat snippet.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System.Linq"`

## Snippet Template

```csharp
// Run: cat snippets/category/Topic.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "Ns1,Ns2"
// Verifies: docs/Topic.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// Tests here...

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
```

## Categories (24 total, 357 topics)

1. Core Collections (28)
2. Blob System (23)
3. Memory & Allocators (12)
4. Dynamic Buffers (10)
5. ECS Extensions (34)
6. Jobs & Threading (6)
7. State & Model (13)
8. Spatial & Physics (20)
9. Utility (24)
10. Extension Methods (15)
11. ConfigVars (13)
12. Authoring & Baking (11)
13. Editor Tools (31)
14. Source Generators (20)
15. SubScene System (19)
16. Pause & Time (7)
17. Relevancy & Netcode (6)
18. Singleton System (8)
19. Object Management (13)
20. Physics States (3)
21. Life Cycle (11)
22. Tests & Diagnostics (5)
23. Math Extensions (11)
24. Other (34)

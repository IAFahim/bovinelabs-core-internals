#!/usr/bin/env python3
"""Retry fact-check on SKIP branches - processes in batches, resumable."""
import subprocess, time, sys, os

REPO = "/home/l/Github/bovinelabs-core-internals"
MODEL = "gemini-2.5-flash"
TIMEOUT = 120
RESULTS_FILE = os.path.join(REPO, "factcheck_results_v2.txt")
DELAY = 3  # seconds between API calls

def run_sh(cmd, timeout=120):
    r = subprocess.run(cmd, shell=True, capture_output=True, text=True, timeout=timeout, cwd=REPO)
    return r.stdout.strip(), r.stderr.strip(), r.returncode

def gemini_check(content):
    prompt = f"""You are a Unity ECS (Entities 1.x DOTS) expert. Fact-check this ASCII diagram.
List ONLY factual errors (max 3 bullets). If correct, respond: NO ERRORS

{content[:3000]}"""
    try:
        result = subprocess.run(
            ["gemini", "--model", MODEL, "-p", prompt],
            capture_output=True, text=True, timeout=TIMEOUT, cwd=REPO,
            stdin=subprocess.DEVNULL
        )
        output = result.stdout.strip()
        # Filter out CLI noise
        lines = []
        skip_patterns = ['Loaded cached', 'logout', '__HERMES', 'tcsetattr', 
                         'Gaxios', 'job control', 'cannot set terminal',
                         'Inappropriate ioctl']
        for l in output.split('\n'):
            if not any(p in l for p in skip_patterns):
                lines.append(l)
        return '\n'.join(lines).strip()
    except subprocess.TimeoutExpired:
        return "ERROR: TIMEOUT"
    except Exception as e:
        return f"ERROR: {e}"

def load_progress():
    """Load previously processed branches from v2 results."""
    done = set()
    if os.path.exists(RESULTS_FILE):
        with open(RESULTS_FILE) as f:
            for line in f:
                line = line.strip()
                if line.startswith("CLEAN: "):
                    done.add(line[7:])
                elif line.startswith("SKIP: "):
                    done.add(line[6:])
                elif line.startswith("ISSUES: "):
                    done.add(line[8:])
    return done

ALL_SKIPPED = [
    "AabbExtensions", "AlwaysUpdatePhysicsWorld", "ArchetypeChunk_DidChange",
    "ArchetypeChunk_GetDynamicBufferAccessor", "BitArrayUtilities", "BlobBuilderExtensions",
    "BlobCurve", "BlobHashMapData", "BlobPerfectHashMap", "BurstTrampoline",
    "BurstUtil", "CalculateEventMapBucketsJob", "CodecService",
    "ComponentLookup_GetOptionalComponentDataRW", "ComponentLookup_SetChangeFilter",
    "ConvexHullBuilder", "CurveRemapUtility", "DebugUtil.SplitInt", "Deserializer",
    "DistanceHitSortAscending", "DynamicHashSet", "DynamicMultiHashMap",
    "DynamicUntypedBuffer", "DynamicVariableMap", "EnableMaskCreator", "EntityLock",
    "EntityQueryBuilder_WithAllRW", "EntityQuery_GetFirstEntity",
    "EntityQuery_GetSingletonBufferNoSync", "EntityQuery_QueryHasSharedFilter",
    "EntityQuery_ReplaceSharedComponentFilter", "FixedArray", "GlobalRandom",
    "HSV", "HalfSizeTriangleMatrix", "IJobChunkWorkerBeginEnd", "IJobHashMapDefer",
    "IJobParallelForDeferBatch", "IJobParallelForDeferExtensions", "ISpatialPosition",
    "IState", "IntersectionTests", "LibraryLoader", "LocalSpatialMap",
    "MathematicsExtensions_Encapsulate", "MeshSimplifier", "MiniString", "NativeCounter",
    "NativeKeyedMap", "NativeLinearCongruentialGenerator", "NativePerfectHashMap",
    "NativeThreadStream", "NativeUntypedHashMap", "NativeWorkQueue", "NoAllocHelpers",
    "PhysicsExtensions_Raycast", "Pin", "PolygonUtility", "PooledNativeList",
    "PositionBuilder", "ReflectionUtility", "ShortHalfUnion", "SpatialKeyedMap",
    "SpatialMap", "SpinLock", "StateInstanceUtil", "SystemState_GetSingletonEntity",
    "TimerFixed", "TimerTriggerResetJob", "UnmanagedPool", "UnsafeFixedPoolAllocator",
    "UnsafePoolAllocator", "UnsafeSlabAllocator", "mathex_GenerateGaussianNoise",
    "mathex_add", "mathex_minMax", "mathex_mod"
]

done = load_progress()
remaining = [b for b in ALL_SKIPPED if b not in done]

print(f"Already done: {len(done)}, Remaining: {len(remaining)}")
print(f"Rate limit: {DELAY}s delay between calls, {TIMEOUT}s timeout per call")
print()

for i, branch in enumerate(remaining):
    print(f"[{len(done)+i+1}/{len(ALL_SKIPPED)}] {branch:<50}", end=" ", flush=True)
    
    out, _, rc = run_sh(f"git show topic/{branch}:README.md 2>/dev/null")
    if rc != 0 or not out.strip():
        print("NO README")
        with open(RESULTS_FILE, 'a') as f:
            f.write(f"SKIP: {branch}\n")
        time.sleep(DELAY)
        continue
    
    result = gemini_check(out)
    
    if "NO ERRORS" in result.upper():
        print("CLEAN")
        with open(RESULTS_FILE, 'a') as f:
            f.write(f"CLEAN: {branch}\n")
    elif "ERROR" in result or "TIMEOUT" in result:
        status = result[:60]
        print(f"SKIP ({status})")
        with open(RESULTS_FILE, 'a') as f:
            f.write(f"SKIP: {branch}\n")
    else:
        print("ISSUES")
        with open(RESULTS_FILE, 'a') as f:
            f.write(f"ISSUES: {branch}\n{result}\n\n")
    
    # Rate limiting between calls
    if i < len(remaining) - 1:
        time.sleep(DELAY)

# Final summary from v2 results
print(f"\n{'='*60}")
print("READING FINAL RESULTS...")
done_final = load_progress()
print(f"Total processed in v2: {len(done_final)}/{len(ALL_SKIPPED)}")

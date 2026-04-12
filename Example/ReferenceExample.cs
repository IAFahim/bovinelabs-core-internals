// Example: Reference<T> — Unmanaged blob alternative for MemoryAllocator
// When to use: Need BlobAssetReference-like semantics with custom MemoryAllocator
// Alternative: BlobAssetReference<T> (Unity built-in, requires BlobBuilder pipeline)
//
// Run: cat Example/ReferenceExample.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,BovineLabs.Core.Collections,BovineLabs.Core.Memory,Unity.Collections"

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else { fail++; sb.AppendLine("  FAIL: " + n); } };

var allocator = new MemoryAllocator(Allocator.Persistent);
try
{
    // === Example 1: Create from int value (simplest case) ===
    sb.AppendLine("=== Example 1: Create from primitive ===");
    var intRef = Reference<int>.Create(42, allocator);
    check("int IsCreated", intRef.IsCreated);
    check("int Value == 42", intRef.Value == 42);
    sb.AppendLine("  Created int ref: " + intRef.Value);

    // === Example 2: Create from float ===
    sb.AppendLine();
    sb.AppendLine("=== Example 2: Create from float ===");
    var floatRef = Reference<float>.Create(3.14f, allocator);
    check("float IsCreated", floatRef.IsCreated);
    check("float Value", System.Math.Abs(floatRef.Value - 3.14f) < 0.001f);
    sb.AppendLine("  Created float ref: " + floatRef.Value);

    // === Example 3: Null reference ===
    sb.AppendLine();
    sb.AppendLine("=== Example 3: Null reference ===");
    var nullRef = Reference<int>.Null;
    check("Null.IsCreated == false", !nullRef.IsCreated);
    check("Null == Null", nullRef == Reference<int>.Null);
    check("ref != Null", intRef != Reference<int>.Null);
    sb.AppendLine("  Null check: IsCreated=" + nullRef.IsCreated + ", ref!=Null=" + (intRef != Reference<int>.Null));

    // === Example 4: Create from byte array ===
    sb.AppendLine();
    sb.AppendLine("=== Example 4: Create from byte[] ===");
    var data = new byte[] { 1, 2, 3, 4, 5 };
    var bytesRef = Reference<byte>.Create(data, allocator);
    check("bytes IsCreated", bytesRef.IsCreated);
    check("byte[0] == 1", bytesRef.Value == 1);
    sb.AppendLine("  Created byte[] ref, first byte=" + bytesRef.Value);

    // === Example 5: Equality semantics (same type) ===
    sb.AppendLine();
    sb.AppendLine("=== Example 5: Equality ===");
    var sameRef = intRef;
    check("Same ref equals", intRef.Equals(sameRef));
    check("== operator", intRef == sameRef);
    check("GetHashCode consistent", intRef.GetHashCode() == sameRef.GetHashCode());
    var otherRef = Reference<int>.Create(99, allocator);
    check("Different refs not equal", intRef != otherRef);
    sb.AppendLine("  Same==Same: " + (intRef == sameRef) + ", Different: " + (intRef != otherRef));

    // === Example 6: ReferenceData access ===
    sb.AppendLine();
    sb.AppendLine("=== Example 6: ReferenceData ===");
    var rd = intRef.ReferenceData;
    // Example 6 happens before Example 7 mutation, so Value is still 42 at this point
    // Note: Value is `ref T`, so writes go directly to the allocated memory
    var rdRef = new Reference<int>(rd);
    check("Roundtrip via ReferenceData", rdRef.IsCreated && rdRef.Value == 42);
    sb.AppendLine("  ReferenceData roundtrip: IsCreated=" + rdRef.IsCreated + " Value=" + rdRef.Value);

    // === Example 7: Mutate through Value ===
    sb.AppendLine();
    sb.AppendLine("=== Example 7: Mutate through ref ===");
    intRef.Value = 100;
    check("Mutated to 100", intRef.Value == 100);
    sb.AppendLine("  After mutation: " + intRef.Value);

    // === Use Case Decision Matrix ===
    sb.AppendLine();
    sb.AppendLine("=== Decision Matrix ===");
    sb.AppendLine("  USE Reference<T> when:");
    sb.AppendLine("    - Need blob-like data in unmanaged memory via MemoryAllocator");
    sb.AppendLine("    - Want use-after-free detection (ValidationPtr)");
    sb.AppendLine("    - Working in Burst jobs without BlobAssetReference access");
    sb.AppendLine("  USE BlobAssetReference<T> when:");
    sb.AppendLine("    - Need BlobBuilder for complex nested/ref data");
    sb.AppendLine("    - Want Unity serialization + asset integration");
    sb.AppendLine("  USE NativeArray<T> when:");
    sb.AppendLine("    - Just need a simple array, not reference semantics");
}
finally
{
    allocator.FreeAll();
    allocator.Dispose();
}

sb.AppendLine();
sb.AppendLine("Verified: " + pass + " checks, " + fail + " failures");
return sb.ToString();

# ComponentLookup.GetOptionalComponentDataRW

## Inner Workings Diagram

```
 ComponentLookup<T>.GetOptionalComponentDataRW(entity)
 ══════════════════════════════════════════════════════════════════
 Returns a raw pointer T* that may be NULL if the entity lacks the component

 PURPOSE:
 ═════════
 Standard ComponentLookup throws if the component doesn't exist.
 This returns a nullable pointer — the caller checks for null.


 ┌───────────────────────────────────────────────────────────────────┐
 │  T* GetOptionalComponentDataRW<T>(                                │
 │      ref this ComponentLookup<T> lookup, Entity entity)           │
 │                                                                   │
 │  Returns: T* — pointer to component data, or null                 │
 └───────────────────────┬───────────────────────────────────────────┘
                        │
                        ▼
 ┌────────────────────────────────────────────────────────────────────┐
 │  Step 1: Reinterpret to internal layout                            │
 │  ┌─────────────────────────────────────────────────────────────┐  │
 │  │  ref var internal = ref UnsafeUtility.As<                  │  │
 │  │      ComponentLookup<T>, ComponentLookupInternal>(ref lk)  │  │
 │  │                                                             │  │
 │  │  ComponentLookupInternal:                                   │  │
 │  │  ┌──────────────────────────────────────────────────────┐  │  │
 │  │  │  EntityDataAccess* m_Access                          │  │  │
 │  │  │  LookupCache        m_Cache                          │  │  │
 │  │  │  TypeIndex          m_TypeIndex                      │  │  │
 │  │  │  uint               m_GlobalSystemVersion            │  │  │
 │  │  │  byte               m_IsZeroSized                    │  │  │
 │  │  │  byte               m_IsReadOnly                     │  │  │
 │  │  └──────────────────────────────────────────────────────┘  │  │
 │  └─────────────────────────────────────────────────────────────┘  │
 │                        │                                          │
 │                        ▼                                          │
 │  Step 2: Validate                                                  │
 │  ┌─────────────────────────────────────────────────────────────┐  │
 │  │  CheckWriteAndThrow(m_Safety)                               │  │
 │  │  AssertExistsNotZeroSize()  — zero-size types have no ptr   │  │
 │  │  AssertEntitiesExist(&entity, 1)                            │  │
 │  └─────────────────────────────────────────────────────────────┘  │
 │                        │                                          │
 │                        ▼                                          │
 │  Step 3: Get OPTIONAL component pointer (key difference)           │
 │  ┌─────────────────────────────────────────────────────────────┐  │
 │  │  return (T*)ecs->GetOptionalComponentDataWithTypeRW(        │  │
 │  │      entity,                                                │  │
 │  │      m_TypeIndex,                                           │  │
 │  │      m_GlobalSystemVersion,                                 │  │
 │  │      ref m_Cache);                                          │  │
 │  │                                                             │  │
 │  │  "Optional" variant returns NULL when component absent      │  │
 │  │  Standard variant would throw/abort                         │  │
 │  └─────────────────────────────────────────────────────────────┘  │
 └────────────────────────────────────────────────────────────────────┘


 POINTER RETURN SCENARIOS
 ┌──────────────────────────────────────────────────────────────────┐
 │                                                                  │
 │  Entity has component T:                                         │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  Archetype: [Entity, Position, Velocity]                   │  │
 │  │  Entity(5) belongs to this archetype                       │  │
 │  │                                                            │  │
 │  │  GetOptionalComponentDataRW<Position>(e5)                  │  │
 │  │  → returns &Position[5] ─→ valid T* (non-null)            │  │
 │  └────────────────────────────────────────────────────────────┘  │
 │                                                                  │
 │  Entity does NOT have component T:                               │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  Archetype: [Entity, Position]  (no Velocity)              │  │
 │  │  Entity(12) belongs to this archetype                      │  │
 │  │                                                            │  │
 │  │  GetOptionalComponentDataRW<Velocity>(e12)                 │  │
 │  │  → returns (T*)null                                        │  │
 │  └────────────────────────────────────────────────────────────┘  │
 └──────────────────────────────────────────────────────────────────┘


 USAGE PATTERN
 ┌──────────────────────────────────────────────────────────────────┐
 │  var ptr = lookup.GetOptionalComponentDataRW<Velocity>(entity);  │
 │  if (ptr != null)                                                │
 │  {                                                               │
 │      ptr->Speed = 10f;  // direct write through pointer          │
 │  }                                                               │
 │  // No try-catch, no HasComponent+GetComponent double lookup     │
 └──────────────────────────────────────────────────────────────────┘

 ADVANTAGES over standard patterns:
 ════════════════════════════════════
 • Single ECS lookup (HasComponent + GetComponent = 2 lookups)
 • No exception overhead for missing components
 • Direct pointer for high-performance write access
 • Version bumped via GlobalSystemVersion (proper RW semantics)
